using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.VivHelper;
using MonoMod.Utils;
using System.Reflection;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/InLevelTeleporter")]
    public class InLevelTeleporter : Entity {

        public enum Directions { Up = 3, Down = 1, Left = 2, Right = 0 }

        public InLevelTeleporter pair;
        public Directions Direction { get; private set; }
        public string[] Flags;
        public float speedOut;
        public int length;
        public bool enabled;
        public bool cameraWarp;
        public int numOfUses;
        public int useCount;
        public string audio;
        private bool allActors; //Experimental. Why did I agree to do this?
        private bool legacy;
        private bool center, OHMod;
        private List<Sprite> sprites = null;


        public InLevelTeleporter(EntityData data, Vector2 offset) : this(data.Enum<Directions>("dir1", Directions.Up), data.Attr("flags1"), data.Float("sO1"), data.Int("l", 16), data.Bool("eO1"), data.Bool("allActors"), data.Bool("cW"), data.Int("NumberOfUses1", int.MaxValue), data.Attr("Audio", ""), data.Position + offset, data.Bool("legacy", true), data.Bool("center"), data.Bool("OutbackHelperMod"), data.Attr("Path"), data.Attr("ParticleColor", "Transparent")) {
            Vector2 pos2 = data.Nodes[0] + offset;
            pair = new InLevelTeleporter(data.Enum<Directions>("dir2", Directions.Up), data.Attr("flags2"), data.Float("sO2"), data.Int("l", 8), data.Bool("eO2"), data.Bool("allActors"), data.Bool("cW"), data.Int("NumberOfUses2", int.MaxValue), data.Attr("Audio", ""), pos2, data.Bool("legacy", true), data.Bool("center"), data.Bool("OutbackHelperMod"), data.Attr("Path"), data.Attr("ParticleColor", "Transparent"));


        }
        //You shouldn't ever use this in your code if you choose to inherit this class. This constructor is to make sure that the base of the portal is made, and then the other constructor builds both portals.
        public InLevelTeleporter(Directions dir, string flags, float sOut, int length, bool eO, bool aA, bool cW, int use, string audio, Vector2 position, bool legacyTP, bool center, bool OOHMod, string spritePath, string particleColor) : base(position) {
            Direction = dir;
            Flags = flags.Split(',');
            speedOut = sOut;
            this.length = length;
            this.audio = audio;
            cameraWarp = cW;
            numOfUses = use;
            if (numOfUses == -1) { numOfUses = int.MaxValue; } else if (numOfUses < 1) { numOfUses = 1; }
            switch (Direction) {
                case Directions.Left:
                    base.Collider = new Hitbox(16f, length, -16f);
                    break;
                case Directions.Down:
                    base.Collider = new Hitbox(length, 16f);
                    break;
                case Directions.Right:
                    base.Collider = new Hitbox(16f, length);
                    break;
                default:
                    base.Collider = new Hitbox(length, 16f, y: -16f);
                    break;
            }
            if (!eO)
                Add(new PlayerCollider(OnPlayer, base.Collider));

            enabled = false;
            useCount = 0;
            allActors = aA;
            legacy = legacyTP;
            this.center = center;
            this.OHMod = OOHMod;

            if (!string.IsNullOrWhiteSpace(spritePath)) {
                GFX.Game.PushFallback(null);
                sprites = new List<Sprite>();
                MTexture q = GFX.Game[spritePath];
                if (q != null) {
                    for (int a = 0; a < 3; a++) {
                        MTexture r = q.GetSubtexture(0, 8 * a, 8, 8);
                        Sprite s = new Sprite(GFX.Game, "");
                        s.AddLoop("main", 0.06f, r);
                        sprites.Add(s);

                    }
                } else {
                    List<MTexture> set = GFX.Game.GetAtlasSubtextures(spritePath);
                    if (set.Count != 0) {
                        List<List<MTexture>> r = new List<List<MTexture>>() { new List<MTexture>(), new List<MTexture>(), new List<MTexture>() };

                        foreach (MTexture t in set) {
                            r[0].Add(t.GetSubtexture(0, 0, 8, 8));
                            r[1].Add(t.GetSubtexture(0, 8, 8, 8));
                            r[2].Add(t.GetSubtexture(0, 16, 8, 8));
                        }
                        for (int b = 0; b < 3; b++) {
                            Sprite s = new Sprite(GFX.Game, "");
                            s.AddLoop("main", 0.06f, r[b].ToArray());
                            sprites.Add(s);
                        }
                    }
                }

            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (pair != null) { pair.pair = this; this.Scene.Add(pair); }
        }

        public override void Update() {
            base.Update();
            Collidable = enabled = VivHelperModule.OldGetFlags(base.Scene as Level, Flags, "and");

            if (allActors) {
                foreach (Actor actor in CollideAll<Actor>().Where((Entity a) => !(a is Player))) {

                    switch (Direction) {
                        case Directions.Right:
                            if (actor.Left > Left && actor.CenterY <= Bottom && actor.CenterY >= Top) {

                                pair.TeleportEntity(actor, actor.Top - Top, Direction);

                            }
                            break;
                        case Directions.Left:
                            if (actor.Right < Right && actor.CenterY <= Bottom && actor.CenterY >= Top) {
                                pair.TeleportEntity(actor, Bottom - actor.Bottom, Direction);
                            }
                            break;
                        case Directions.Up:
                            if (actor.Bottom < Bottom && actor.CenterX <= Right && actor.CenterX >= Left) {
                                pair.TeleportEntity(actor, Right - actor.Right, Direction);
                            }
                            break;
                        case Directions.Down:
                            if (actor.Top > Top && actor.CenterX <= Right && actor.CenterX >= Left) {

                                pair.TeleportEntity(actor, actor.Left - Left, Direction);
                            }
                            break;
                    }
                }
            }
        }

        public override void Render() {
            base.Render();
            if (sprites != null) {

            }
        }

        protected virtual void OnPlayer(Player player) {
            if (useCount < numOfUses) {
                useCount++;
                switch (Direction) {
                    case Directions.Right:
                        if (player.Left > Left && player.CenterY <= Bottom && player.CenterY >= Top) {
                            pair.TeleportPlayer(player, player.Top - Top, player.Speed, Direction);

                        }
                        break;
                    case Directions.Left:
                        if (player.Right < Right && player.CenterY <= Bottom && player.CenterY >= Top) {
                            pair.TeleportPlayer(player, Bottom - player.Bottom, player.Speed, Direction);
                        }
                        break;
                    case Directions.Up:
                        if (player.Bottom < Bottom && player.CenterX <= Right && player.CenterX >= Left) {
                            pair.TeleportPlayer(player, Right - player.Right, player.Speed, Direction);
                        }
                        break;
                    case Directions.Down:
                        if (player.Top > Top && player.CenterX <= Right && player.CenterX >= Left) {
                            pair.TeleportPlayer(player, player.Left - Left, player.Speed, Direction);
                        }
                        break;
                }

            }
        }
        protected void TeleportPlayer(Player player, float distance, Vector2 speed, Directions d) {

            Level l = SceneAs<Level>();
            if (l == null) { return; }
            if (legacy) {
                switch (Direction) {
                    case Directions.Right:
                        if (center)
                            player.CenterRight = new Vector2(Left, CenterY);
                        else
                            player.BottomRight = new Vector2(Left, Bottom - distance);
                        break;
                    case Directions.Left:
                        if (center)
                            player.CenterLeft = new Vector2(Right, CenterY);
                        else
                            player.TopLeft = new Vector2(Right, Top + distance);
                        break;
                    case Directions.Up:
                        if (center)
                            player.BottomCenter = new Vector2(CenterX, Bottom);
                        else
                            player.BottomLeft = new Vector2(Left + distance, Bottom);
                        break;
                    case Directions.Down:
                        if (center)
                            player.TopCenter = new Vector2(CenterX, Top);
                        else
                            player.TopRight = new Vector2(Right - distance, Top);
                        break;
                }
                player.Position = player.Position.Ceiling();
                if (WiggleOutFail(player as Entity)) { player.Die(Vector2.Zero); return; }
                player.Speed = SpeedMod(player, d);
                if (cameraWarp) {
                    CameraShit(player.Scene as Level, player);
                }
            } else {

                l.OnEndOfFrame += delegate {
                    switch (Direction) {
                        case Directions.Right:
                            player.BottomRight = new Vector2(Left, Bottom - distance);
                            break;
                        case Directions.Left:
                            player.TopLeft = new Vector2(Right, Top + distance);
                            break;
                        case Directions.Up:
                            player.BottomLeft = new Vector2(Left + distance, Bottom);
                            break;
                        case Directions.Down:
                            player.TopRight = new Vector2(Right - distance, Top);
                            break;
                    }
                    if (WiggleOutFail(player as Entity)) { player.Die(Vector2.Zero); return; }
                    player.Speed = SpeedMod(player, d);
                    if (cameraWarp) {
                        CameraShit(player.Scene as Level, player);
                    }
                };
            }
        }

        private Vector2 SpeedMod(Player player, Directions d) //d == direction, Direction == portal.direction (if reading in OutbackHelper)
        {
            Vector2 speed = player.Speed;
            if (OHMod) {
                if ((int) d == 3)
                    speed.Y = Math.Max(speed.Y, 150f);
                //This code is an improvement on the Matrix transform.
                float anglediff = (Direction - d) * (float) Math.PI / 2f;
                speed = speed.Rotate(anglediff);
                speed *= -1f;
                if (player.StateMachine.State == Player.StDash) {
                    player.StateMachine.State = Player.StDummy;
                }
                //Optimized code
                if (player.StateMachine.State != 5) {
                    if ((int) d == 1) {
                        speed *= 1.5f;
                        if ((int) Direction % 2 == 0) {
                            speed.Y -= 150f;
                        }
                    } else if ((int) d == 3) {
                        speed *= 1.5f;
                    }
                }
                if ((int) Direction == 1) {
                    speed.Y = Math.Min(player.Speed.Y, -150f);
                }
            } else {
                float anglediff = (Direction - d) * (float) Math.PI / 2f;
                speed = speed.Rotate(anglediff);
                speed *= -1f;

            }
            return speed + new Vector2(speedOut * (float) Math.Cos(Calc.Angle(speed)), speedOut * (float) Math.Sin(Calc.Angle(speed)));
        }

        private void CameraShit(Level level, Player player) {
            Vector2 camPos = player.Position;
            Vector2 vector2 = new Vector2(player.X - 160f, player.Y - 90f);
            camPos.X = MathHelper.Clamp(vector2.X, (float) level.Bounds.Left, (float) (level.Bounds.Right - 320));
            camPos.Y = MathHelper.Clamp(vector2.Y, (float) level.Bounds.Top, (float) (level.Bounds.Bottom - 180));
            level.Camera.Position = camPos;
        }

        private bool WiggleOutFail(Entity entity) {
            if (entity.CollideCheck<Solid>()) {
                for (int i = 1; i <= 5; i++) {
                    for (int j = -1; j <= 1; j += 2) {
                        for (int k = 1; k <= 5; k++) {
                            for (int l = -1; l <= 1; l += 2) {
                                Vector2 vector = new Vector2(i * j, k * l);
                                if (!entity.CollideCheck<Solid>(Position + vector)) {
                                    entity.Position += vector;
                                    return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public void TeleportEntity(Entity entity, float distance, Directions d) {

            switch (Direction) {
                case Directions.Right:
                    entity.BottomRight = new Vector2(Left, Bottom - distance);
                    break;
                case Directions.Left:
                    entity.TopLeft = new Vector2(Right, Top + distance);
                    break;
                case Directions.Up:
                    entity.BottomLeft = new Vector2(Left + distance, Bottom);
                    break;
                case Directions.Down:
                    entity.TopRight = new Vector2(Right - distance, Top);
                    break;
            }
            if (WiggleOutFail(entity)) { entity.RemoveSelf(); return; }

        }

        public Vector2 TeleportEntityWithSpeed(Entity entity, float distance, Vector2 speed, Directions d) {
            float anglediff = (Direction - d) * (float) Math.PI / 2f;
            speed = speed.Rotate(anglediff);
            speed *= -1f;
            switch (Direction) {
                case Directions.Right:
                    entity.BottomRight = new Vector2(Left, Bottom - distance);
                    break;
                case Directions.Left:
                    entity.TopLeft = new Vector2(Right, Top + distance);
                    break;
                case Directions.Up:
                    entity.BottomLeft = new Vector2(Left + distance, Bottom);
                    break;
                case Directions.Down:
                    entity.TopRight = new Vector2(Right - distance, Top);
                    break;
            }
            if (WiggleOutFail(entity)) { entity.RemoveSelf(); return Vector2.Zero; }
            return speed + new Vector2(speedOut * (float) Math.Cos(Calc.Angle(speed)), speedOut * (float) Math.Sin(Calc.Angle(speed)));
        }

    }
}
