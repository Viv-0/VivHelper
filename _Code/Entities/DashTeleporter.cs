using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.VivHelper;
using System.Collections;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/TeleporterDash")]
    public class DashTeleporter : Solid {
        public enum TransitionType {
            None,
            Lightning,
            GlitchEffect,
            ColorFlash
        }
        public Level level;
        public string[] flags;
        public string currentRoom, newRoom;
        public Vector2 currentPos, newPos;
        public TransitionType transition;
        private bool triggered = false;
        private Color flashColor;
        private float cooldown;
        private bool doFlash;
        private float flash;
        private float cooldownTimer = 0;
        public char tiletype;
        public bool blendIn, alwaysBounceUp;
        public TileGrid tiles;
        private Bezier2[] spiral;

        private float spiralScale;

        public DashTeleporter(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: true) {
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            tiletype = data.Char("tiletype", '3');
            newRoom = data.Attr("WarpRoom", currentRoom);
            newPos = new Vector2(data.Float("newPosX", -1f), data.Float("newPosY", -1f));
            transition = data.Enum("TransitionType", TransitionType.None);
            flashColor = VivHelper.OldColorFunction(data.Attr("Color", "White"));
            flags = data.Attr("ZFlagsData", "").Split(',');
            cooldown = data.Float("CooldownLength", 0f);
            blendIn = data.Bool("blendin", false);
            alwaysBounceUp = data.Bool("AlwaysBounceUp", false);
            OnDashCollide = OnDash;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
            currentRoom = level.Session.Level;
            List<LevelData> Levels = level.Session.MapData.Levels;
            bool b = false;
            foreach (LevelData l in Levels) { b |= l.Name == newRoom; }
            if (!b) {
                newRoom = currentRoom;
                newPos = new Vector2(-1, -1);
                transition = TransitionType.None;
                flashColor = Color.White;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            TileGrid tileGrid;
            if (!blendIn) {
                tileGrid = GFX.FGAutotiler.GenerateBox(tiletype, (int) base.Width / 8, (int) base.Height / 8).TileGrid;
                Add(new LightOcclude());
            } else {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (base.X / 8f) - tileBounds.Left;
                int y = (int) (base.Y / 8f) - tileBounds.Top;
                int tilesX = (int) base.Width / 8;
                int tilesY = (int) base.Height / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tiletype, x, y, tilesX, tilesY, solidsData).TileGrid;
                Add(new EffectCutout());
                base.Depth = -10501;
            }
            Add(tileGrid);
            tiles = tileGrid;
            Add(new TileInterceptor(tileGrid, highPriority: true));
        }

        public override void Update() {
            base.Update();
            if (doFlash) {
                flash = Calc.Approach(flash, 1f, Engine.DeltaTime * 10f);
                if (flash >= 1f) {
                    doFlash = false;
                }
            } else if (flash > 0f) {
                flash = Calc.Approach(flash, 0f, Engine.DeltaTime * 3f);
            }
            if (cooldownTimer > 0f) {
                cooldownTimer -= Engine.DeltaTime;
            } else if (triggered) { cooldownTimer = 0f; triggered = false; }
        }



        public DashCollisionResults OnDash(Player player, Vector2 direction) {
            if (cooldownTimer > 0f) {
                return DashCollisionResults.NormalCollision;
            } else {
                TeleportSequence(player);
                return DashCollisionResults.Rebound;
            }
        }

        public void TeleportSequence(Player player) {
            if (player.Dead || player == null) { return; }
            if (!triggered && VivHelperModule.OldGetFlags(level, flags, "and")) {
                triggered = true;
                cooldownTimer = cooldown;
                float temp4 = transition == TransitionType.GlitchEffect ? 0.5f : 0f;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, temp4 == 0f ? 0.01f : 0.1f, start: true);
                if (temp4 != 0) {
                    tween.OnUpdate = delegate (Tween t) {
                        Glitch.Value = temp4 * t.Eased;
                    };
                }
                tween.OnComplete = delegate {

                    level.OnEndOfFrame += delegate {

                        //Instantiates all the variables of the previous player object into something we can use later.
                        Facings facing = player.Facing;
                        Vector2 levelOffset = level.LevelOffset;
                        Vector2[] vals = new Vector2[3];
                        vals[0] = player.Position - levelOffset;
                        vals[1] = level.Camera.Position - levelOffset;
                        vals[2] = player.Position - this.Position;
                        Dictionary<Entity, Vector2> followerPoints = new Dictionary<Entity, Vector2>();
                        Leader leader = player.Leader;
                        foreach (Follower follower in leader.Followers) {
                            if (follower.Entity != null) {
                                followerPoints[follower.Entity] = (follower.Entity.Position - player.Position);
                                follower.Entity.AddTag(Tags.Global);
                                level.Session.DoNotLoad.Add(follower.ParentEntityID);
                            }
                        }
                        int state = player.StateMachine.State;
                        //Removes the current level we are in without breaking everything
                        level.Remove(player);
                        level.UnloadLevel();
                        //Sets the future values of the player. This code is modified from the Teleport Trigger to accommodate more things.
                        level.Session.Level = newRoom;
                        level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                        level.Session.FirstLevel = false;
                        level.LoadLevel(Player.IntroTypes.Transition);
                        level.Add(player);
                        if (newPos.X >= 0 && newPos.X <= level.Bounds.X + level.Bounds.Width - level.LevelOffset.X &&
                            newPos.Y >= 0 && newPos.Y <= level.Bounds.Y + level.Bounds.Height - level.LevelOffset.Y) {
                            player.Position = level.LevelOffset + newPos;
                            CameraStuff(level, player);
                        } else if (vals[0].X >= 0 && vals[0].X <= level.Bounds.X + level.Bounds.Width - level.LevelOffset.X &&
                              vals[0].Y >= 0 && vals[0].Y <= level.Bounds.Y + level.Bounds.Height - level.LevelOffset.Y) {
                            player.Position = level.LevelOffset + vals[0];
                            CameraStuff(level, player);
                        } else { player.Position = level.LevelOffset + new Vector2(1, 1); CameraStuff(level, player); }

                        if (state == 10) { player.SummitLaunch(player.Position.X); }
                        foreach (Follower follower in leader.Followers) {
                            if (follower.Entity != null) {
                                follower.Entity.Position = player.Position + followerPoints[follower.Entity];
                                follower.Entity.RemoveTag(Tags.Global);
                            }
                        }
                        leader.TransferFollowers();

                        player.Facing = facing;
                        player.Hair.MoveHairBy(level.LevelOffset - levelOffset);

                        /*if (level.Wipe != null)
                        {
                            level.Wipe.Cancel();
                        }*/
                        Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.3f, start: true);
                        tween2.OnUpdate = delegate (Tween t) {
                            Glitch.Value = temp4 * (1f - t.Eased);
                        };
                        player.Add(tween2);
                        if (transition == TransitionType.ColorFlash) { level.Flash(flashColor, false); }

                    };
                };
                player.Add(tween);
            }
            return;
        }

        private void CameraStuff(Level level, Player player) {
            Vector2 camPos = player.Position;
            Vector2 vector2 = new Vector2(player.X - 160f, player.Y - 90f);
            camPos.X = Calc.Clamp(vector2.X, (float) level.Bounds.Left, (float) (level.Bounds.Right - 320));
            camPos.Y = Calc.Clamp(vector2.Y, (float) level.Bounds.Top, (float) (level.Bounds.Bottom - 180));
            level.Camera.Position = camPos;
        }
    }
}
