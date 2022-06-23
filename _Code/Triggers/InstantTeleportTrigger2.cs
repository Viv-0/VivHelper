using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework.Graphics;

namespace VivHelper.Triggers {


    public static class TeleportV2Hooks {
        public static Vector2? HackedFocusPoint;
        public static Vector2 HackfixFocusPoint => HackedFocusPoint ?? SpotlightWipe.FocusPoint;

        public static void Load() {
            On.Celeste.SpotlightWipe.ctor += SpotlightWipe_ctor;
            IL.Celeste.SpotlightWipe.Render += SpotlightWipe_Render;
        }

        public static void Unload() {
            On.Celeste.SpotlightWipe.ctor -= SpotlightWipe_ctor;
            IL.Celeste.SpotlightWipe.Render -= SpotlightWipe_Render;
        }

        private static void SpotlightWipe_Render(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<SpotlightWipe>("FocusPoint"))) {
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Vector2, Scene, Vector2>>(ModifyFocusPoint);
            }
        }
        private static Vector2 ModifyFocusPoint(Vector2 orig, Scene scene) {
            if (!(scene is Level level))
                return orig;
            if (!VivHelperModule.Session.TeleportState)
                return orig;
            if (scene.Tracker.TryGetEntity<Player>(out Player player)) {
                return player.Position - level.Camera.Position - new Vector2(0, 8);
            }
            return HackfixFocusPoint;
        }

        private static void SpotlightWipe_ctor(On.Celeste.SpotlightWipe.orig_ctor orig, SpotlightWipe self, Scene scene, bool wipeIn, Action onComplete) {
            orig(self, scene, wipeIn, onComplete);
            if (scene is Level level && VivHelperModule.Session.TeleportState) {
                self.Linear = true;
                self.Duration = 0.5f;
            }
        }
    }

    /// <summary>
    /// the target for a teleport. Extends Trigger for the usability of the two-way teleporters
    /// </summary>
    [Tracked]
    [CustomEntity("VivHelper/TeleportTarget = Load")]
    public class TeleportTarget : Trigger {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if (entityData.Bool("AddTriggerOffset", false)) {
                List<float> w, h;
                w = new List<float>();
                h = new List<float>();
                MapData mapData = level?.Session?.MapData;
                if (mapData != null) {
                    foreach (LevelData _LevelData in mapData.Levels) {
                        foreach (EntityData _EntityData in _LevelData.Entities) {
                            if (_EntityData.Name == "VivHelper/ITPT1Way" && _EntityData.Attr("TargetID", "") == entityData.Attr("TargetID", null)) {
                                w.Add(_EntityData.Width);
                                h.Add(_EntityData.Height);
                            }
                        }
                    }
                }
            }
            return new TeleportTarget(entityData, offset, new List<float>() { 8 }, new List<float>() { 8 });
        }

        public static string[] ValidTeleportTargetNames = new string[] { "VivHelper/TeleportTarget", "VivHelper/ITPT2Way" };

        public enum SpeedModifier {
            NoChange = 0, Add = 1, Multiply = 2, Set = 3
        }

        public class SpeedDefiner {
            SpeedModifier mod;
            //if wrt s (position/distance), dv = (ds * sqrt(2), ds * sqrt(2))
            Vector2 dv;
            bool setByLength;

            public SpeedDefiner(SpeedModifier i, float j) {
                mod = i;
                //multiplying is a scalar operation so we need this check, addition will add the wrong amount 
                dv = new Vector2(i == SpeedModifier.Add ? j * 0.7071067f : j);
                if (mod == SpeedModifier.Set)
                    setByLength = true;
            }

            public SpeedDefiner(SpeedModifier i, float j0, float j1) {
                mod = i;
                dv = new Vector2(j0, j1);
            }

            public Vector2 ModifySpeed(Vector2 input) {
                switch (mod) {
                    case SpeedModifier.Multiply:
                        return new Vector2(input.X * dv.X, input.Y * dv.Y);
                    case SpeedModifier.Set:
                        return setByLength ? input.SafeNormalize(Vector2.Zero) * dv.X : dv;
                    case SpeedModifier.Add:
                        return input + dv;
                    default:
                        return input;
                }
            }
        }

        protected readonly bool rotateToAngle, rotateBefore; //default is rotate degrees from angle
        protected readonly float? rotVal = null;
        protected readonly SpeedDefiner speedChange;

        public bool modifyVelocity;
        public string targetID;
        public string roomName;
        public bool active;
        public bool addTriggerOffset;
        public int setState;
        public List<float> DebugWidths, DebugHeights;

        public Vector2? forceSpawnpoint;


        public string NewRoom => (Scene as Level)?.Session?.Level;

        public TeleportTarget(EntityData data, Vector2 offset, List<float> VisualWidth, List<float> VisualHeight) : base(data, offset) {
            DebugWidths = VisualWidth;
            DebugHeights = VisualHeight;
            Tag = Tags.FrozenUpdate;
            roomName = data.Level.Name;
            addTriggerOffset = data.Bool("AddTriggerOffset", false);
            try { forceSpawnpoint = data.Nodes?[0] + offset ?? null; } catch { forceSpawnpoint = null; }
            if (data.Has("RotationValue")) {
                rotateBefore = data.Bool("RotateBeforeSpeedChange", false);
                rotateToAngle = data.Bool("RotateToAngle", false);
                rotVal = data.Float("RotationValue", 0f);
            }
            if (data.Has("SpeedModifier")) {
                SpeedModifier sm = data.Enum<SpeedModifier>("SpeedModifier");
                float r = sm == SpeedModifier.Multiply ? 1f : 0f;
                if (data.Has("SpeedChangeStrength")) {
                    speedChange = new SpeedDefiner(sm, data.Float("SpeedChangeStrength", r));
                } else {
                    speedChange = new SpeedDefiner(sm, data.Float("SpeedChangeX", r), data.Float("SpeedChangeY", r));
                }
            } else
                speedChange = new SpeedDefiner(SpeedModifier.NoChange, 0f);


            targetID = data.NoEmptyString("TargetID", "");
            string temp = data.Attr("SetState", "-1");
            if (string.IsNullOrWhiteSpace(temp))
                setState = -1;
            else {
                VivHelper.DefineSetState(temp, out setState);
            }
        }

        public Vector2 ModifyVelocity(Vector2 input) {
            Vector2 output = input;
            if (rotateBefore && rotVal.HasValue) {
                output = rotateToAngle ? output.RotateTowards(rotVal.Value, 6.3f) : output.Rotate(rotVal.Value);
            }
            output = speedChange.ModifySpeed(output);
            if (!rotateBefore && rotVal.HasValue) {
                output = rotateToAngle ? output.RotateTowards(rotVal.Value, 6.3f) : output.Rotate(rotVal.Value);
            }
            return output;
        }


    }

    [CustomEntity(
        "VivHelper/ITPT1Way"
        )]
    public class InstantTeleportTrigger1Way : Trigger {
        private struct GlitchType {
            public float GlitchStrength;
            public Ease.Easer GlitchEaser;
            public float GlitchLength;
        }

        public enum TransitionType {
            None,
            ColorFlash,
            Lightning,
            Glitch,
            Wipe
        }

        /* -2 = Opposite side only: (n*4)%15
         * -1 = Not entry side: 15-n
         * 0 = None
         * 1 = Right
         * 2 = Up
         * 4 = Left
         * 8 = Down 
         * 15 = Any
         */
        public int ExitDirection;
        private int EntrySide = 0;
        public string[] flagsNeeded, flagsSet;
        public string roomName;
        public Vector2 startPos;
        public bool resetDashes;
        public TransitionType transition;
        public List<object> transitionInfo; //This is just random info necessary for the transition
        /* none = null
         * ColorFlash : Color colorFlash (includes flashAlpha)
         * Lightning : bool flash, bool shake, float[] delaySet, int count, float[] X offset range from CameraTarget
         * GlitchEffect : two Tuples containing: float GlitchValue, Ease.Easer GlitchEaser, float GlitchLength
         * Wipe : bool onLeave, onBegin
         */
        public float delay, customDelay;
        public bool addRespawnDelay; //Wipe only
        public bool ignoreIfDummy;
        public string targetID;
        public string specificRoom, specificRoomErrorCheckName;
        public static bool active = false;
        public bool onlyOnce;
        public bool endCutscene;
        public bool preActive;
        public string setState = null;
        public bool bringHoldableThrough;
        private bool delayAwakeAction;
        public bool freezeGameOnTeleport;
        public TriggerPersistence persistence;
        public EntityID ID;
        //Handler is the one that actually runs the coroutines for everything because it has Pause and Frozen immunity which is necessary, since sometimes we force the game to pause at the teleport point.
        public Entity Handler => HelperEntities.AllUpdateHelperEntity;
        public Coroutine lockRoom;

        public InstantTeleportTrigger1Way(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
            ID = id;
            roomName = data.Level.Name;
            startPos = data.Position + offset;
            targetID = data.NoEmptyString("TargetID", null);
            if (targetID == null)
                throw new InvalidPropertyException($"Teleport Trigger in room {roomName} at position {data.Position} has an empty targetID property.");
            ExitDirection = data.Bool("onExit", false) ? Calc.Clamp(data.Int("ExitDirection", 15), -2, 15) : 0;
            string _f = data.Attr("RequiredFlags", "");
            if (!string.IsNullOrWhiteSpace(_f))
                flagsNeeded = _f.Split(',');
            _f = data.Attr("FlagsOnTeleport", "");
            if (!string.IsNullOrWhiteSpace(_f))
                flagsSet = _f.Split(',');
            resetDashes = data.Bool("ResetDashes", true);
            specificRoomErrorCheckName = specificRoom = data.NoEmptyString("RoomName", null);
            if (specificRoom == null)
                specificRoomErrorCheckName = "No Value Set!";
            transition = data.Enum<TransitionType>("TransitionType", TransitionType.None);
            delay = 0f;
            ignoreIfDummy = !data.Bool("IgnoreNoSpawnpoints", true);
            persistence = data.Enum<TriggerPersistence>("Persistence", TriggerPersistence.Default);
            bringHoldableThrough = data.Bool("BringHoldableThrough");
            switch (transition) {
                case TransitionType.ColorFlash:
                    transitionInfo = new List<object>() { VivHelper.ColorFix(data.Attr("FlashColor")) * Calc.Clamp(data.Float("FlashAlpha"), 0f, 1f) };
                    break;
                case TransitionType.Lightning:
                    int c = data.Int("LightningCount", 2);
                    var s = data.Attr("LightningOffsetRange", "-130,130").Split(',');
                    float[] q;
                    try {
                        if (s.Length == 1)
                            q = new float[] { int.Parse(s[0].Trim()), int.Parse(s[0].Trim()) };
                        else if (s.Length == 2)
                            q = new float[] { int.Parse(s[0].Trim()), int.Parse(s[1].Trim()) };
                        else
                            q = new float[] { -130, 130 };
                    } catch (Exception d) {
                        throw new InvalidPropertyException($"Lightning Offset Range property in Teleport Trigger in Room {roomName}, Position {startPos}, invalid: needs to be an integer or two integers separated by commas.", d);
                    }
                    float[] j = new float[2] { data.Float("LightningDelay"), 0.25f };
                    if (float.TryParse(data.Attr("LightningMaxDelay", ""), out j[1]))
                        j[1] -= j[0];
                    transitionInfo = new List<object>() { data.Bool("Flash", true), data.Bool("Shake", true), j, c, q };
                    break;
                case TransitionType.Glitch:

                    GlitchType _start = new GlitchType {
                        GlitchStrength = data.Float("GlitchStrength", 0.5f),
                        GlitchEaser = VivHelper.TryGetEaser(data.Attr("StartingGlitchEase", "Linear"), out Ease.Easer e) ? e : Ease.Linear,
                        GlitchLength = data.Float("StartingGlitchDuration", 0.3f)
                    };
                    GlitchType _end = new GlitchType {
                        GlitchStrength = data.Float("GlitchStrength", 0.5f),
                        GlitchEaser = VivHelper.TryGetEaser(data.Attr("EndingGlitchEase", "Linear"), out Ease.Easer f) ? f : Ease.Linear,
                        GlitchLength = data.Float("EndingGlitchDuration", 0.3f)
                    };
                    delay = _start.GlitchLength;
                    transitionInfo = new List<object> { _start, _end };
                    break;
                case TransitionType.Wipe:
                    transitionInfo = new List<object> { data.Bool("wipeOnLeave", true), data.Bool("wipeOnEnter", true) };
                    break;
            }
            if (data.Has("freezeOnTeleport"))
                freezeGameOnTeleport = data.Bool("freezeOnTeleport", true);
            if (data.Float("CustomDelay", -1) >= 0)
                customDelay = Math.Max(0f, data.Float("CustomDelay") - delay);
            endCutscene = data.Bool("EndCutsceneOnWarp", true);
            Depth = 5000;

        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (targetID.Trim() != "-") {
                delayAwakeAction = true; 
            }

        }

        public void DelayedAwakeAction(Scene scene) {
            if (scene is Level level) {
                delayAwakeAction = false;
                Session session = level?.Session;
                if (transition == TransitionType.Wipe) {
                    if (level.Wipe != null) {
                        customDelay = Math.Max(0f, customDelay - (level.Wipe.Duration));
                    }
                }
                if (session != null) {
                    List<LevelData> levels = session.MapData.Levels;
                    if (levels.First(f => f.Name == specificRoom).Dummy)
                        Collidable = false;
                    if (specificRoom == null || !VivHelper.IsValidRoomName(specificRoom, levels)) {
                        foreach (LevelData l in levels) {
                            foreach (EntityData e in l.Entities) {
                                if (TeleportTarget.ValidTeleportTargetNames.Contains(e.Name) && e.Attr("TargetID", null) == targetID) {
                                    specificRoom = l.Name;
                                    setState = e.NoEmptyString("SetState");
                                    if (levels.First(f => f.Name == specificRoom).Dummy)
                                        Collidable = false;
                                    return;
                                }
                            }
                        }
                        throw new Exception($"There is no TeleportTarget associated with the TargetID \"{targetID}\" in any rooms in your map. Please make sure your TeleportTarget has that ID.");
                    }
                }
            } else {
                return;
            }
        }

        public override void Update() {
            if (delayAwakeAction)
                DelayedAwakeAction(Scene);
            base.Update();
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (ExitDirection != 0) {
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
                sides[0] *= (this.Left > prevTopRight.X && this.Left <= player.TopRight.X) ? 1 : 0;
                sides[1] *= (this.Right < prevBottomLeft.X && this.Right >= player.BottomLeft.X) ? 1 : 0;
                sides[2] *= (this.Top > prevBottomLeft.Y && this.Top <= player.BottomLeft.Y) ? 1 : 0;
                sides[3] *= (this.Bottom < prevTopRight.Y && this.Bottom >= player.TopRight.Y) ? 1 : 0;
                int setValue = sides.Max();
                //Invert the values for 1 and 2 so that the format is Right = 1, Up = 2, Left = 3, Down = 4
                //Then set values to 2^setValue. Optimized with jump statements because the mathematical operations are less efficient than the jump set (the table isn't that slow but to change setValue to 2 or 1 alongside the table) is slower than using the switch statement.
                switch (setValue) {
                    case 0:
                        EntrySide = 1;
                        break;
                    case 1:
                        EntrySide = 4;
                        break;
                    case 2:
                        EntrySide = 2;
                        break;
                    case 3:
                        EntrySide = 8;
                        break;
                }
            } else if (flagsNeeded == null || VivHelperModule.OldGetFlags(Scene as Level, flagsNeeded, "and")) {
                if(Handler == null)
                    HelperEntities.AllUpdateHelperEntity = new Entity() { Tag = Tags.PauseUpdate | Tags.FrozenUpdate | Tags.TransitionUpdate | Tags.Global | Tags.Persistent };
                if (Handler.Scene != Scene)
                    Scene.Add(HelperEntities.AllUpdateHelperEntity);
                Handler.Add(new Coroutine(TeleportMaster(player)));
            }
        }

        public override void OnLeave(Player player) {
            if (ExitDirection != 0) {
                base.OnLeave(player);
                if (ExitDirection % 15 == 0 && (flagsNeeded == null || VivHelperModule.OldGetFlags(Scene as Level, flagsNeeded, "and"))) { Handler?.Add(new Coroutine(TeleportMaster(player))); return; }
                int ExitSide = 0;
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
                int setValue = sides.Max();
                switch (setValue) {
                    case 0:
                        ExitSide = 1;
                        break;
                    case 1:
                        ExitSide = 4;
                        break;
                    case 2:
                        ExitSide = 2;
                        break;
                    case 3:
                        ExitSide = 8;
                        break;
                }
                if (TriggerCheck(ExitSide) && (flagsNeeded == null || VivHelperModule.OldGetFlags(Scene as Level, flagsNeeded, "and"))) {
                    if (Handler == null)
                        HelperEntities.AllUpdateHelperEntity = new Entity() { Tag = Tags.PauseUpdate | Tags.FrozenUpdate | Tags.TransitionUpdate | Tags.Global | Tags.Persistent };
                    if (Handler.Scene != Scene)
                        Scene.Add(HelperEntities.AllUpdateHelperEntity);
                    Handler.Add(new Coroutine(TeleportMaster(player)));
                }
            }
        }
        private bool TriggerCheck(int ExitSide) { 
            switch (ExitDirection) {
                case -2:
                    /* true if:
                     * ExitSide = 1 and EntrySide = 4 (4*4 = 16) % 15 = 1
                     * ExitSide = 2 and EntrySide = 8 (8*4 = 32) % 15 = 2
                     * ExitSide = 4 and EntrySide = 1 (1*4 = 4) % 15 = 4
                     * ExitSide = 8 and EntrySide = 2 (2*4 = 8) % 15 = 8
                     * Fun fact, for a set of numbers defined this way this will work for any square number n^2,
                     * as the form Opposite "Side" = for a value q, its opposite is (q*n) % (n^2 - 1),
                     * this also holds for numbers in between because the evaluation is distributive, i.e.
                     * (3*C) % (C^2 - 1) == (2*C) % (C^2 - 1) + (1*C) % (C^2 - 1).
                    */
                    return ExitSide == (EntrySide * 4) % 15;
                //NEQ is fastest here
                case -1:
                    return ExitSide != EntrySide;
                //The default case, this just determines if ExitDirection has a valid bit set to the same bit as the ExitSide,
                //which is the same as checking if the binary composition of ExitDirection contains the bit flagged by ExitSide,
                //since ExitSide must be a 4 binary number with one on state. You can confirm this should work if you want.
                default:
                    return (ExitDirection & ExitSide) > 0;
            }
        }

        public IEnumerator TeleportMaster(Player player) {
            if (active) {
                yield break;
            }
            active = true;
            if (customDelay > 0)
                yield return customDelay;
            if (player == null) {
                active = false;
                yield break;
            }
            Level level = player.Scene as Level;
            if (level == null || level.Bounds == null)
                yield break;
            preActive = true;
            Handler?.Add(new Coroutine(LockRoom(player, level.Bounds)));
            VivHelperModule.Session.TeleportState = true;
            switch (transition) {
                case TransitionType.ColorFlash:
                    level.Flash((Color) transitionInfo[0]);
                    Teleport(player, level);
                    break;
                case TransitionType.Glitch:

                    GlitchType g = (GlitchType) transitionInfo[0];
                    Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, g.GlitchEaser, g.GlitchLength, false);
                    tween1.OnUpdate = delegate (Tween t) {

                        Glitch.Value = g.GlitchStrength * (t.Eased);
                    };
                    tween1.OnComplete = delegate (Tween t) {
                        if (freezeGameOnTeleport)
                            level.Frozen = false;
                        Teleport(player, level);
                    };
                    Handler?.Add(tween1);
                    if (freezeGameOnTeleport)
                        level.Frozen = true;
                    tween1.Start();
                    break;
                case TransitionType.Wipe:

                    if ((bool) transitionInfo[0]) {
                        AreaData.Get(level.Session).DoScreenWipe(level, false, delegate {
                            if (freezeGameOnTeleport)
                                level.Frozen = false;
                            Teleport(player, level);
                        });
                        if (freezeGameOnTeleport)
                            level.Frozen = true;
                    } else {
                        Teleport(player, level);
                    }
                    break;

                default:
                    Teleport(player, level);
                    break;
            }

            level.Paused = false;
            level.PauseLock = true;
        }




        private void Teleport(Player player, Level level) {
            preActive = false;
            if (player?.Dead ?? true) //Cancels on death
            {
                level.PauseLock = false;
                VivHelperModule.Session.TeleportState = false;
                active = false;
                return;
            }
            level.OnEndOfFrame += delegate //Required to prevent Entity removal or locking the EntityList
            {
                Vector2 triggerOffset = player.TopLeft - this.TopLeft;
                LevelData levelData = level.Session.MapData.Get(specificRoom);
                if (setState == null) {
                    try {
                        setState = levelData.Triggers.First(e => e.Name == "VivHelper/TeleportTarget" && e.NoEmptyString("TargetID", "") == targetID).NoEmptyString("SetState", "-1");
                    } catch {
                        setState = "-1";
                    }
                }
                EntityData teleportTargetData = levelData.Triggers.FirstOrDefault(e => e.Name == "VivHelper/TeleportTarget" && e.Attr("TargetID") == targetID);

                if (player?.Dead ?? true)
                    return; //Cancels on death
                Facings facing = player.Facing;
                int newDashes = player.Dashes;
                Vector2 pDashDir = Vector2.Zero;
                if (VivHelperModule.MatchDashState(player.StateMachine.State)) { pDashDir = player.DashDir; }

                //It's crazy how little you need if you base your camera code on CameraTargets
                Vector2 cameraDifferential = level.Camera.Position - player.CameraTarget;

                if (endCutscene)
                    level.EndCutscene();
                Vector2 oldPlayerPosition = player.TopLeft;
                Vector2 oldPlayerSpeed = player.Speed;
                Leader leader = player.Leader;
                foreach (Follower follower in leader.Followers) {
                    if (follower.Entity != null) {
                        follower.Entity.Position -= oldPlayerPosition;
                        follower.Entity.AddTag(Tags.Global);
                        if (!VivHelper.CompareEntityIDs(follower.ParentEntityID, EntityID.None))
                            level.Session.DoNotLoad.Add(follower.ParentEntityID);
                    }
                }
                for (int i = 0; i < leader.PastPoints.Count; i++) {
                    leader.PastPoints[i] -= oldPlayerPosition;
                }
                int prevState = -1;
                if (setState != "-1") {
                    prevState = player.StateMachine.State;
                    if (prevState == Player.StSummitLaunch)
                        player.StopSummitLaunch();
                    player.StateMachine.State = Player.StDummy;
                }
                bool prevForceCam = player.ForceCameraUpdate;
                int prevLockCam = VivHelperModule.Session.lockCamera;
                player.ForceCameraUpdate = true;
                VivHelperModule.Session.lockCamera = 1;
                player.CleanUpTriggers();
                if (resetDashes) {
                    player.RefillDash();

                }
                //Removes the current level we are in without breaking everything
                level.Remove(player);
                level.Entities.Remove(level.Entities.FindAll(VivHelperModule.UnloadTypesWhenTeleporting)); //There's some weird stuff that goes on in awake for these classes.
                //Clears the burst info
                level.Displacement.Clear();
                //Clears the TrailManager if the room doesn't match the previous room
                if (roomName != specificRoom) {
                    level.ParticlesBG.Clear();
                    level.Particles.Clear();
                    level.ParticlesFG.Clear();
                    TrailManager.Clear();
                }
                level.UnloadLevel();

                level.Session.Level = specificRoom;
                //Sets the future values of the player.
                bool c = false;
                if (teleportTargetData != default(EntityData)) {
                    Vector2? v = teleportTargetData.FirstNodeNullable(levelData.Position);
                    if (v.HasValue) {
                        level.Session.RespawnPoint = v.Value;
                    } else {
                        level.Session.RespawnPoint = level.Session.GetSpawnPoint(levelData.Position + teleportTargetData.Position);
                    }
                    c = true;
                }
                level.Session.FirstLevel = false;
                level.Add(player);
                level.LoadLevel(Player.IntroTypes.Transition);



                //Retrieves the TeleportTarget
                TeleportTarget target = level.Tracker.GetFirstEntity<TeleportTarget>(t => t.targetID == targetID);
                if (target == null) {
                    throw new Exception("TeleportTarget not found! This error usually means that no TeleportTarget exists in the room set in your Instant Teleport Trigger.");
                }
                if (!c) {
                    if (target.forceSpawnpoint.HasValue) {
                        level.Session.RespawnPoint = target.forceSpawnpoint;
                    } else {
                        level.Session.RespawnPoint = level.Session.GetSpawnPoint(target.Position);
                    }
                }
                //Sets the player's position
                if (target.addTriggerOffset) {
                    player.TopLeft = target.TopLeft + triggerOffset;
                } else {
                    player.Center = target.Center;
                    player.Position = player.Position.Floor(); //Necessary because sometimes weird bugs occur. Apologies to TASers.
                }
                if (player.CollideCheck<Solid>()) {
                    player.Position -= Vector2.UnitY;
                    if (player.CollideCheck<Solid>()) {
                        player.Position += Vector2.UnitY;
                    }
                }
                player.Hair.MoveHairBy(player.TopLeft - oldPlayerPosition);
                if (!resetDashes) {
                    player.Dashes = newDashes;
                }
                //Copies over the follower data.
                foreach (Follower follower in leader.Followers) {
                    if (follower.Entity != null) {
                        follower.Entity.Position += player.TopLeft;
                        follower.Entity.RemoveTag(Tags.Global);
                        //Prevents loading in of follower entities if you have collected them.
                        if (!VivHelper.CompareEntityIDs(follower.ParentEntityID, EntityID.None) && !level.Session.Keys.Contains(follower.ParentEntityID))
                            level.Session.DoNotLoad.Remove(follower.ParentEntityID);
                    }
                }
                for (int i = 0; i < leader.PastPoints.Count; i++) {
                    leader.PastPoints[i] += player.Position;
                }
                leader.TransferFollowers();

                player.Facing = facing;
                //Modifies the player's speed, based on the information put into the TeleportTarget
                player.Speed = target.ModifyVelocity(player.Speed);
                //Camera magic, this is such a dumb strategy but it works
                foreach (Trigger trigger in level.Tracker.GetEntities<Trigger>().Where(t => Collide.Check(player, t) && t.GetType().Name.IndexOf("camera", StringComparison.OrdinalIgnoreCase) > -1)) {
                    VivHelper.PlayerTriggerCheck(player, trigger);
                }
                level.Camera.Position = player.CameraTarget - cameraDifferential;
                var CameraPos = level.Camera.Position;
                //Player state fixes
                if (VivHelper.DefineSetState(setState, out int state)) {
                    switch (prevState) {
                        case Player.StSummitLaunch:
                            player.SummitLaunch(player.Position.X);
                            break;

                        default:
                            player.StateMachine.State = state;
                            if (VivHelperModule.MatchDashState(state)) { player.DashDir = pDashDir.RotateTowards(player.Speed.Angle(), 6.3f).EightWayNormal(); }
                            break;
                    }
                } else if (prevState != -1) {
                    player.StateMachine.State = prevState;
                }
                switch (transition) {
                    case TransitionType.Glitch:

                        GlitchType g = (GlitchType) transitionInfo[1];
                        Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, g.GlitchEaser, g.GlitchLength, false);
                        tween2.OnUpdate = delegate (Tween t) {
                            Glitch.Value = g.GlitchStrength * (1 - t.Eased);
                        };
                        tween2.OnComplete = delegate (Tween t) { if (freezeGameOnTeleport) level.Frozen = false; };
                        HelperEntities.AllUpdateHelperEntity.Add(tween2);
                        if (freezeGameOnTeleport)
                            level.Frozen = true;
                        tween2.Start();
                        break;
                    case TransitionType.Lightning:
                        float f2 = player.CameraTarget.X + 90f;
                        bool[] doStuff = new bool[2] { (bool) transitionInfo[0], (bool) transitionInfo[1] };
                        float[] delayRange = (float[]) transitionInfo[2];
                        int count = (int) transitionInfo[3];
                        float[] offsetRange = (float[]) transitionInfo[4];
                        float delayTally = delayRange[0];
                        if (doStuff[0])
                            level.Flash(Color.White);
                        if (doStuff[1])
                            level.Shake();
                        for (int i = 0; i < count; i++) {
                            level.Add(new LightningStrike(new Vector2(Calc.Random.Range(f2 + offsetRange[0], f2 + offsetRange[1]), player.CameraTarget.Y - 32f), (10 + 30 * i) % 63, 255, delayTally));
                            delayTally += (float) (Calc.Random.NextDouble() * delayRange[1]);
                        }
                        break;
                    case TransitionType.Wipe:
                        TeleportV2Hooks.HackedFocusPoint = player.Position - CameraPos - new Vector2(0, 8);
                        AreaData.Get(level.Session).DoScreenWipe(level, true, delegate {
                            TeleportV2Hooks.HackedFocusPoint = null;
                            if (freezeGameOnTeleport)
                                level.Frozen = false;
                        });
                        if (freezeGameOnTeleport)
                            level.Frozen = true;

                        break;
                }
                player.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate { player.ForceCameraUpdate = prevForceCam; VivHelperModule.Session.lockCamera = prevLockCam; }, 0.02f, true));
                level.PauseLock = false;
                if(flagsSet != null)
                    foreach (string s in flagsSet)
                        if (!string.IsNullOrWhiteSpace(s))
                            level.Session.SetFlag(s);
                VivHelperModule.Session.TeleportState = false;
                active = false;
                RunPersistence(level.Session);
            };
        }

        private void RunPersistence(Session session) {
            switch (persistence) {
                //OnlyOnce
                case TriggerPersistence.OncePerMapPlay:
                    session.DoNotLoad.Add(ID);
                    break;
            }
        }



        private IEnumerator ErrorRoutine(Level level, string roomName) {
            yield return null;
            Audio.SetMusic(null);
            LevelEnterExt.ErrorMessage = $"Failed to enter {roomName} because there was no valid TeleportTarget in that room.";
            LevelEnter.Go(new Session(level.Session?.Area ?? new AreaKey(1).SetSID("")), fromSaveData: false);
        }

        //Code borrowed from ContortHelper
        private IEnumerator LockRoom(Player player, Rectangle bounds) {
            if (player?.Dead ?? true) {
                yield break;
            }
            while (preActive) {
                if (player.Left < (float) bounds.Left + 1) {
                    player.Left = bounds.Left;
                    player.OnBoundsH();
                } else if (player.Right > (float) bounds.Right - 1) {
                    player.Right = bounds.Right;
                    player.OnBoundsH();
                } else if (player.Top < (float) bounds.Top + 1) {
                    player.Top = bounds.Top;
                    player.OnBoundsV();
                } else if (player.Bottom > (float) bounds.Bottom - 1) {
                    if (SaveData.Instance.Assists.Invincible) {
                        player.Play("event:/game/general/assist_screenbottom");
                        player.Bounce(bounds.Bottom);
                    } else if (player != null && !player.Dead && player.Bottom < (float) bounds.Bottom) {
                        player.Die(Vector2.Zero);
                    }
                }
                yield return null;
            }
        }

        private void LockRoom(Rectangle bounds, Player player) {
            if (player?.Dead ?? true)
                return;
            if (player.Left < (float) bounds.Left + 1) {
                player.Left = bounds.Left;
                player.OnBoundsH();
            } else if (player.Right > (float) bounds.Right - 1) {
                player.Right = bounds.Right;
                player.OnBoundsH();
            } else if (player.Top < (float) bounds.Top + 1) {
                player.Top = bounds.Top;
                player.OnBoundsV();
            } else if (player.Bottom > (float) bounds.Bottom - 1) {
                if (SaveData.Instance.Assists.Invincible) {
                    player.Play("event:/game/general/assist_screenbottom");
                    player.Bounce(bounds.Bottom);
                } else if (player != null && !player.Dead && player.Bottom < (float) bounds.Bottom) {
                    player.Die(Vector2.Zero);
                }
            }
        }
    }
}
