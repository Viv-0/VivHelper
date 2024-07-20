using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Module__Extensions__Etc;
using static VivHelper.VivHelper;

namespace VivHelper.Triggers {

    /*[TrackedAs(typeof(Trigger))]
    [CustomEntity(
        "VivHelper/BasicInstantTeleportTrigger = Basic",
        "VivHelper/MainInstantTeleportTrigger = Main",
        "VivHelper/CustomInstantTeleportTrigger = Custom")]*/
    public class OldInstTeleportTriggerRework : Trigger {
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
        public float[] vel, cameraOffset;
        public float rot, vel2;
        public TransitionType transition;
        public bool velMod, rotMod;
        public bool dreaming;
        public bool addTriggerOffset;
        private bool triggered = false;
        private float timeSlowDown;
        private Color flashColor;
        private float delay;
        private int legacyCamera;
        private bool legacyBerry, legacySpawnpoint;
        private bool resetDashes;
        private bool forceNormalState;
        private bool onExit, DifferentSide; private int direction = -1;
        private bool wackyFollowerOverride;
        private bool TransitionListeners;

        private bool doFlash;
        private float flash;
        public static Entity Basic(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new InstantTeleportTrigger(entityData, offset);
        public static Entity Main(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new InstantTeleportTrigger(entityData, offset);
        public static Entity Custom(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new InstantTeleportTrigger(entityData, offset);
        public OldInstTeleportTriggerRework(EntityData data, Vector2 offset) : base(data, offset) {
            newRoom = data.Attr("WarpRoom", currentRoom).Trim();
            newPos = new Vector2(data.Float("newPosX", -1f), data.Float("newPosY", -1f));
            cameraOffset = new float[] { data.Float("CameraOffsetX", 0f), data.Float("CameraOffsetY", 0f) };
            velMod = data.Bool("VelocityModifier", false);
            vel = new float[] { data.Float("ExitVelocityX", velMod ? 1 : 0), data.Float("ExitVelocityY", velMod ? 1 : 0) };
            vel2 = data.Float("ExitVelocityS", -1);
            rotMod = data.Bool("RotationType", false);
            rot = 0 - Calc.DegToRad * data.Float("RotationActor", 0);
            dreaming = data.Bool("Dreaming", true);
            transition = data.Enum("TransitionType", TransitionType.None);
            flashColor = VivHelper.GetColorWithFix(data, "Color", "color", GetColorParams.None, GetColorParams.None, Color.White).Value; // VivHelper.OldColorFunction(data.Attr("Color", "White"));
            flags = data.Attr("ZFlagsData", "").Split(',');
            addTriggerOffset = data.Bool("AddTriggerOffset", true);
            timeSlowDown = data.Float("TimeSlowDown", 0f);
            delay = data.Float("TimeBeforeTeleport", 0f);
            legacyCamera = data.Int("CameraType", -1);
            legacySpawnpoint = data.Bool("LegacySpawnpoint", true);
            resetDashes = data.Bool("ResetDashes", true);
            forceNormalState = data.Bool("ForceNormalState");
            if (legacyCamera == -1)
                legacyCamera = 1;
            onExit = data.Bool("OnExit");
            DifferentSide = onExit ? data.Bool("DifferentSide") : false;
            wackyFollowerOverride = data.Bool("followerForcePosition");
            TransitionListeners = data.Bool("ActAsTransition", false);
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
                vel = new float[] { velMod ? 1 : 0, velMod ? 1 : 0 };
                vel2 = -1;
                dreaming = true;
                cameraOffset = new float[] { 0, 0 };
                transition = TransitionType.None;
                flashColor = Color.White;
                rotMod = false;
                rot = 0f;
            }
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
        }

        private void Transition(TransitionType transition, Player player) {
            if (transition == TransitionType.Lightning) {
                level.Flash(Color.White);
                level.Shake();
                level.Add(new LightningStrike(new Vector2(player.X + 60f, player.Y - 180), 10, 200f));
                level.Add(new LightningStrike(new Vector2(player.X + 220f, player.Y - 180), 40, 200f, 0.25f));
                Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
            }

        }
        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (onExit) {
                ///Code by Oppenheimer

                // Calculate entrance direction
                // [2 horizontal directions], [2 vertical directions]
                int[] sides = new int[4];
                // Set priority
                // 2^sideindex
                sides[0] = player.Speed.X < 0 ? 1 : -1; //Right
                sides[1] = -sides[0]; //Left
                sides[2] = player.Speed.Y > 0 ? 1 : -1; //Top
                sides[3] = -sides[2]; //Bottom

                Vector2 prevTopRight = player.TopRight - player.Speed; //TopLeft, TopRight, BottomLeft, BottomRight
                Vector2 prevBottomLeft = player.BottomLeft - player.Speed;
                // Check if the segment prevLoc -> curLoc crosses any of the boundaries
                sides[0] *= (Left > prevTopRight.X && Left <= player.TopRight.X) ? 1 : 0;
                sides[1] *= (Right < prevBottomLeft.X && Right >= player.BottomLeft.X) ? 1 : 0;
                sides[2] *= (Top > prevBottomLeft.Y && Top <= player.BottomLeft.Y) ? 1 : 0;
                sides[3] *= (Bottom < prevTopRight.Y && Bottom >= player.TopRight.Y) ? 1 : 0;
                int setValue = 0;
                int temp = 0;
                for (int i = 0; i < sides.Length; i++)
                    if (sides[i] > temp) { setValue = i; temp = sides[i]; }
                //Invert the values for 1 and 2 so that the format is Right = 1, Up = 2, Left = 3, Down = 4
                //Then set values to 2^setValue. Optimized with jump statements because the mathematical operations are less efficient than the jump set (the table isn't that slow but to change setValue to 2 or 1 alongside the table) is slower than using the switch statement.
                switch (setValue) {
                    case 0:
                        direction = 1;
                        break;
                    case 1:
                        direction = 4;
                        break;
                    case 2:
                        direction = 2;
                        break;
                    case 3:
                        direction = 8;
                        break;
                }
            } else if (!VivHelperModule.Session.TeleportState)
                TeleportMaster(player);
        }

        public override void OnLeave(Player player) {
            if (onExit) {
                bool trigger = true;
                if (DifferentSide) {
                    // Calculate exit direction
                    // [2 horizontal directions], [2 vertical directions]
                    int[] sides = new int[4];
                    // Set priority
                    sides[0] = player.Speed.X > 0 ? -1 : 1; //Left
                    sides[1] = -sides[0]; //Right
                    sides[2] = player.Speed.Y > 0 ? -1 : 1; //Top
                    sides[3] = -sides[2]; //Bottom

                    Vector2 prevTopRight = player.TopRight - player.Speed; //TopLeft, TopRight, BottomLeft, BottomRight
                    Vector2 prevBottomLeft = player.BottomLeft - player.Speed;
                    // Check if the segment prevLoc -> curLoc crosses any of the boundaries
                    sides[0] *= (this.Left < prevTopRight.X && this.Left >= player.TopRight.X) ? 1 : 0;
                    sides[1] *= (this.Right > prevBottomLeft.X && this.Right <= player.BottomLeft.X) ? 1 : 0;
                    sides[2] *= (this.Top < prevBottomLeft.Y && this.Top >= player.BottomLeft.Y) ? 1 : 0;
                    sides[3] *= (this.Bottom > prevTopRight.Y && this.Bottom <= player.TopRight.Y) ? 1 : 0;
                    int maxValue = sides.Max();
                    int exitDirection = Array.IndexOf(sides, maxValue);
                    trigger = exitDirection != this.direction;
                }
                if (trigger) TeleportMaster(player);
                direction = -1;
            }
        }

        private void TeleportMaster(Player player) {
            if (delay > 0)
                Alarm.Create(Alarm.AlarmMode.Oneshot, () => {
                    if (!triggered && VivHelperModule.OldGetFlags(level, flags, "and") && player != null && !player.Dead)
                        Teleport(player);
                }, delay, true);
            else if (!triggered && VivHelperModule.OldGetFlags(level, flags, "and") && player != null && !player.Dead)
                Teleport(player);
        }

        private void Teleport(Player player) {
            float temp4 = transition == TransitionType.GlitchEffect ? 0.5f : 0f;
            Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.3f, start: true);
            tween1.OnUpdate = delegate (Tween t) {
                Glitch.Value = temp4 * (t.Eased);
            };
            player.Add(tween1);

            LevelData levelData = level.Session.MapData.Get(newRoom);
            //Sets up the new Player position to v. This is done before the player is added to account for mod interoperability
            Vector2 newPos = player.Position - level.LevelOffset;
            if (this.newPos.X >= 0 && this.newPos.X <= level.Bounds.X + level.Bounds.Width - level.LevelOffset.X &&
                this.newPos.Y >= 0 && this.newPos.Y <= level.Bounds.Y + level.Bounds.Height - level.LevelOffset.Y) {
                newPos = levelData.Position + this.newPos;
                if (addTriggerOffset) { newPos += player.Position.X < this.Position.X ? player.Position - this.Position + new Vector2(8, 0) : player.Position - this.Position; }

            } else if (newPos.X >= 0 && newPos.X <= level.Bounds.X + level.Bounds.Width - level.LevelOffset.X &&
                  newPos.Y >= 0 && newPos.Y <= level.Bounds.Y + level.Bounds.Height - level.LevelOffset.Y) {
                newPos = levelData.Position + newPos;
            } else {
                newPos = levelData.Position + new Vector2(1, 1);
            }

            TeleporterFunctions.Teleport(player, level, newRoom, newPos, null, false, true, PreTeleportFunction, PostTeleportFunction);
        }

        private List<object> PreTeleportFunction(Player player, Level level) {
            if(!resetDashes) { player.RefillDash(); }
            return null;
        }

        private void PostTeleportFunction(Player player, Level level, List<object> stored) {
            ModifySpeed(player);

            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.3f, start: true);
            tween2.OnUpdate = delegate (Tween t) {
                Glitch.Value = (transition == TransitionType.GlitchEffect ? 0.5f : 0f) * (1f - t.Eased);
            };
            player.Add(tween2);
            if (transition == TransitionType.ColorFlash) { level.Flash(flashColor, false); } else if (transition == TransitionType.Lightning) {
                level.Flash(Color.White);
                level.Shake();
                level.Add(new LightningStrike(new Vector2(player.X + 60f, player.Y - 180), 10, 200f));
                level.Add(new LightningStrike(new Vector2(player.X + 220f, player.Y - 180), 40, 200f, 0.25f));
                Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
            }
            if (timeSlowDown != 0f) {
                HelperEntities.AllUpdateHelperEntity.Add(new Coroutine(VivHelperModule.TimeSlowDown(timeSlowDown, .1f)));
            }
        }

        private void ModifySpeed(Player player) {
            if (vel2 == -1f && !(vel[0] == 0f && vel[1] == 0f)) {
                player.Speed = velMod ? new Vector2(player.Speed.X * vel[0], player.Speed.Y * vel[1]) : player.Speed + new Vector2(vel[0], vel[1]);
            }
            if (vel2 != -1f && vel[0] == 0f && vel[1] == 0f) {
                player.Speed = velMod ? player.Speed * vel2 : player.Speed + new Vector2(vel2 * (float) Math.Cos(Calc.Angle(player.Speed)), vel2 * (float) Math.Sin(Calc.Angle(player.Speed)));
            }
            player.Speed = rotMod ? Calc.RotateTowards(player.Speed, rot, 6.3f) : Calc.Rotate(player.Speed, rot);

        }
    }

    // ################################################### NEW TELEPORTER ##################################################################

    //  No more Teleport Target objects! YIPPEE
    //  TeleportTarget specification: 
    /// Position to spawn: 
    ///   if AddTriggerOffset, use relative position on entry trigger -> take distance between position of trigger and position of player, then set the position for the warp to New Position - difference
    ///   else
    ///     Take position of center of trigger and place the player at the center of the trigger, floored. (This is because the Player has an odd height.)
    ///     If the player is now 1 pixel above the ground, place player on ground -- done in postprocessor
    /// Variables: 
    ///   X,Y -> Position
    ///   Node[0] -> Spawn Position
    ///   TargetID -> determines what Teleporter goes to what.
    ///   AddTriggerOffset -> if true, uses relative positioning on entry trigger
    /// Version 0:
    ///   SpeedModifier
}
