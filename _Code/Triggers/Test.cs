using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using VivHelper;
using VivHelper.Module__Extensions__Etc;
using VivHelper.Triggers;

[CustomEntity(new string[] { "VivHelper/ITPT1Way = Load" })]
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

    public int ExitDirection;

    private int EntrySide = 0;

    public string[] flagsNeeded;

    public string[] flagsSet;

    public string roomName;

    public Vector2 startPos;

    public bool resetDashes;

    public TransitionType transition;

    public List<object> transitionInfo;

    public float delay;

    public float customDelay;

    public bool addRespawnDelay;

    public bool ignoreIfDummy;

    public string targetID;

    public string specificRoom;

    public string specificRoomErrorCheckName;

    public static bool active;

    public bool onlyOnce;

    public bool endCutscene;

    public bool preActive;

    public string setState = null;

    public bool bringHoldableThrough;

    private bool delayAwakeAction;

    public bool freezeGameOnTeleport;

    public TriggerPersistence persistence;

    public EntityID ID;

    public bool transitionListeners;

    public Coroutine lockRoom;

    public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
        if (entityData.NoEmptyString("RoomName") == null && entityData.Has("TargetID")) {
            foreach (LevelData item in level?.Session?.MapData.Levels) {
                foreach (EntityData entity in item.Entities) {
                    if (entity.Name == "VivHelper/TeleportTarget" && entity.Attr("TargetID", null) == entityData.Attr("TargetID", null)) {
                        entityData.Values["RoomName"] = item.Name;
                        break;
                    }
                }
            }
        }
        if (entityData.Has("RoomName")) {
            return new InstantTeleportTrigger1Way(entityData, offset, new EntityID(levelData.Name, entityData.ID));
        }
        throw new Exception(string.Format("No valid Teleport Target ID matching Instant Teleport Trigger @\nroom {0}, position {1}, ID: {2}", levelData.Name, entityData.Position, entityData.Attr("TargetID")));
    }

    public InstantTeleportTrigger1Way(EntityData data, Vector2 offset, EntityID id)
        : base(data, offset) {
        ID = id;
        roomName = data.Level.Name;
        startPos = data.Position + offset;
        targetID = data.NoEmptyString("TargetID");
        if (targetID == null) {
            throw new InvalidPropertyException($"Teleport Trigger in room {roomName} at position {data.Position} has an empty targetID property.");
        }
        ExitDirection = ((!data.Has("onExit")) ? Calc.Clamp(data.Int("ExitDirection"), -2, 15) : (data.Bool("onExit") ? Calc.Clamp(data.Int("ExitDirection", 15), -2, 15) : 0));
        string text = data.Attr("RequiredFlags");
        if (!string.IsNullOrWhiteSpace(text)) {
            flagsNeeded = text.Split(',');
        }
        text = data.Attr("FlagsOnTeleport");
        if (!string.IsNullOrWhiteSpace(text)) {
            flagsSet = text.Split(',');
        }
        resetDashes = data.Bool("ResetDashes", defaultValue: true);
        specificRoomErrorCheckName = (specificRoom = data.NoEmptyString("RoomName"));
        if (specificRoom == null) {
            specificRoomErrorCheckName = "No Value Set!";
        }
        transition = data.Enum("TransitionType", TransitionType.None);
        delay = 0f;
        ignoreIfDummy = !data.Bool("IgnoreNoSpawnpoints", defaultValue: true);
        persistence = data.Enum("Persistence", TriggerPersistence.Default);
        bringHoldableThrough = data.Bool("BringHoldableThrough");
        switch (transition) {
            case TransitionType.ColorFlash:
                transitionInfo = new List<object> { global::VivHelper.VivHelper.ColorFix(data.Attr("FlashColor")) * Calc.Clamp(data.Float("FlashAlpha", 1f), 0f, 1f) };
                break;
            case TransitionType.Lightning: {
                    int num = data.Int("LightningCount", 2);
                    string[] array = data.Attr("LightningOffsetRange", "-130,130").Split(',');
                    float[] item;
                    try {
                        item = ((array.Length == 1) ? new float[2]
                        {
                    int.Parse(array[0].Trim()),
                    int.Parse(array[0].Trim())
                        } : ((array.Length != 2) ? new float[2] { -130f, 130f } : new float[2]
                        {
                    int.Parse(array[0].Trim()),
                    int.Parse(array[1].Trim())
                        }));
                    } catch (Exception inner) {
                        throw new InvalidPropertyException($"Lightning Offset Range property in Teleport Trigger in Room {roomName}, Position {startPos}, invalid: needs to be an integer or two integers separated by commas.", inner);
                    }
                    float[] array2 = new float[2]
                    {
                data.Float("LightningDelay"),
                0.25f
                    };
                    if (float.TryParse(data.Attr("LightningMaxDelay"), out array2[1])) {
                        array2[1] -= array2[0];
                    }
                    transitionInfo = new List<object>
                    {
                data.Bool("Flash", defaultValue: true),
                data.Bool("Shake", defaultValue: true),
                array2,
                num,
                item
            };
                    break;
                }
            case TransitionType.Glitch: {
                    Ease.Easer easer;
                    GlitchType glitchType = new GlitchType {
                        GlitchStrength = Calc.Clamp(data.Float("GlitchStrength", 0.5f), 0f, 1f),
                        GlitchEaser = (global::VivHelper.VivHelper.TryGetEaser(data.Attr("StartingGlitchEase", "Linear"), out easer) ? easer : Ease.Linear),
                        GlitchLength = data.Float("StartingGlitchDuration", 0.3f)
                    };
                    Ease.Easer easer2;
                    GlitchType glitchType2 = new GlitchType {
                        GlitchStrength = Calc.Clamp(data.Float("GlitchStrength", 0.5f), 0f, 1f),
                        GlitchEaser = (global::VivHelper.VivHelper.TryGetEaser(data.Attr("EndingGlitchEase", "Linear"), out easer2) ? easer2 : Ease.Linear),
                        GlitchLength = data.Float("EndingGlitchDuration", 0.3f)
                    };
                    delay = glitchType.GlitchLength;
                    transitionInfo = new List<object> { glitchType, glitchType2 };
                    break;
                }
            case TransitionType.Wipe:
                transitionInfo = new List<object>
                {
                data.Bool("wipeOnLeave", defaultValue: true),
                data.Bool("wipeOnEnter", defaultValue: true)
            };
                break;
        }
        if (data.Has("freezeOnTeleport")) {
            freezeGameOnTeleport = data.Bool("freezeOnTeleport", defaultValue: true);
        }
        if (data.Float("CustomDelay", -1f) >= 0f) {
            customDelay = Math.Max(0f, data.Float("CustomDelay") - delay);
        }
        endCutscene = data.Bool("EndCutsceneOnWarp", defaultValue: true);
        base.Depth = 5000;
        transitionListeners = data.Bool("ActAsTransition", defaultValue: true);
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (targetID.Trim() != "-") {
            delayAwakeAction = true;
        } else {
            DelayedAwakeAction(scene);
        }
        if ((scene as Level).Session.DoNotLoad.Contains(ID)) {
            RemoveSelf();
        }
    }

    public void DelayedAwakeAction(Scene scene) {
        if (!(scene is Level level)) {
            return;
        }
        delayAwakeAction = false;
        Session session = level?.Session;
        if (transition == TransitionType.Wipe && level.Wipe != null) {
            customDelay = Math.Max(0f, customDelay - level.Wipe.Duration);
        }
        if (session == null || specificRoom == null) {
            return;
        }
        List<LevelData> levels = session.MapData.Levels;
        LevelData levelData = levels.FirstOrDefault((LevelData f) => f.Name == specificRoom);
        if (levelData == null) {
            return;
        }
        if (levelData.Dummy && ignoreIfDummy) {
            Collidable = false;
        }
        if (specificRoom != null && global::VivHelper.VivHelper.IsValidRoomName(specificRoom, levels)) {
            return;
        }
        foreach (LevelData item in levels) {
            foreach (EntityData entity in item.Entities) {
                if (TeleportTarget.ValidTeleportTargetNames.Contains(entity.Name) && entity.Attr("TargetID", null) == targetID) {
                    specificRoom = item.Name;
                    setState = entity.NoEmptyString("SetState");
                    return;
                }
            }
        }
        throw new Exception("There is no TeleportTarget associated with the TargetID \"" + targetID + "\" in any rooms in your map. Please make sure your TeleportTarget has that ID.");
    }

    public override void Update() {
        if (delayAwakeAction) {
            DelayedAwakeAction(base.Scene);
        }
        base.Update();
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        string[] array = flagsNeeded;
        Logger.Log("VivHelper", (array != null && array.Length != 0) ? string.Join(",", flagsNeeded) : "empty or null");
        if (ExitDirection != 0) {
            int[] array2 = new int[4];
            array2[0] = ((player.Speed.X < 0f) ? 1 : (-1));
            array2[1] = ((player.Speed.Y > 0f) ? 1 : (-1));
            array2[2] = -array2[0];
            array2[3] = -array2[2];
            Vector2 vector = player.TopRight - player.Speed;
            Vector2 vector2 = player.BottomLeft - player.Speed;
            array2[0] *= ((base.Left > vector.X && base.Left <= player.TopRight.X) ? 1 : 0);
            array2[1] *= ((base.Top > vector2.Y && base.Top <= player.BottomLeft.Y) ? 1 : 0);
            array2[2] *= ((base.Right < vector2.X && base.Right >= player.BottomLeft.X) ? 1 : 0);
            array2[3] *= ((base.Bottom < vector.Y && base.Bottom >= player.TopRight.Y) ? 1 : 0);
            int num = 0;
            for (int i = 0; i < array2.Length; i++) {
                if (array2[i] > num) {
                    EntrySide = 2 << i;
                    num = array2[i];
                }
            }
        } else if (!VivHelperModule.Session.TeleportState && (flagsNeeded == null || VivHelperModule.OldGetFlags(player.Scene as Level, flagsNeeded, "and"))) {
            HelperEntities.GetHelperEntity(player.Scene).Add(new Coroutine(TeleportMaster(player)));
        }
    }

    public override void OnLeave(Player player) {
        if (player == null || player.Dead || ExitDirection == 0) {
            return;
        }
        base.OnLeave(player);
        if (ExitDirection % 15 == 0 && (flagsNeeded == null || VivHelperModule.OldGetFlags(player.Scene as Level, flagsNeeded, "and"))) {
            HelperEntities.GetHelperEntity(player.Scene).Add(new Coroutine(TeleportMaster(player)));
            return;
        }
        int exitSide = 0;
        int[] array = new int[4];
        array[0] = ((!(player.Speed.X > 0f)) ? 1 : (-1));
        array[1] = ((!(player.Speed.Y < 0f)) ? 1 : (-1));
        array[2] = -array[0];
        array[3] = -array[2];
        Vector2 vector = player.TopRight - player.Speed;
        Vector2 vector2 = player.BottomLeft - player.Speed;
        array[0] *= ((base.Left < vector.X && base.Left >= player.TopRight.X) ? 1 : 0);
        array[1] *= ((base.Top < vector2.Y && base.Top >= player.BottomLeft.Y) ? 1 : 0);
        array[2] *= ((base.Right > vector2.X && base.Right <= player.BottomLeft.X) ? 1 : 0);
        array[3] *= ((base.Bottom > vector.Y && base.Bottom <= player.TopRight.Y) ? 1 : 0);
        int num = 0;
        for (int i = 0; i < array.Length; i++) {
            if (array[i] > num) {
                exitSide = 2 << i;
                num = array[i];
            }
        }
        if (TriggerCheck(exitSide) && (flagsNeeded == null || VivHelperModule.OldGetFlags(player.Scene as Level, flagsNeeded, "and"))) {
            HelperEntities.GetHelperEntity(player.Scene).Add(new Coroutine(TeleportMaster(player)));
        }
    }

    private bool TriggerCheck(int ExitSide) {
        return ExitDirection switch {
            -2 => ExitSide == EntrySide * 4 % 15,
            -1 => ExitSide != EntrySide,
            _ => (ExitDirection & ExitSide) > 0,
        };
    }

    private void ClearTeleportFunction(Level level) {
        Level level2 = level ?? (base.Scene as Level) ?? (HelperEntities.AllUpdateHelperEntity.Scene as Level) ?? (Engine.Scene as Level);
        active = false;
        level2.PauseLock = false;
        VivHelperModule.Session.TeleportState = false;
        VivHelperModule.Session.TeleportAction = null;
    }

    public IEnumerator TeleportMaster(Player player) {
        if (active) {
            yield break;
        }
        active = true;
        Level level = player.Scene as Level;
        if (customDelay > 0f) {
            yield return customDelay;
        }
        if (player?.Dead ?? true) {
            ClearTeleportFunction(level);
            yield break;
        }
        int num;
        if (level != null) {
            _ = level.Bounds;
            num = ((VivHelperModule.Session.TeleportAction != null) ? 1 : 0);
        } else {
            num = 1;
        }
        if (num != 0) {
            yield break;
        }
        preActive = true;
        HelperEntities.GetHelperEntity(level).Add(new Coroutine(LockRoom(player, level.Bounds)));
        VivHelperModule.Session.TeleportState = true;
        switch (transition) {
            case TransitionType.ColorFlash:
                level.Flash((Color) transitionInfo[0]);
                PreTeleport(player, level);
                break;
            case TransitionType.Glitch: {
                    GlitchType g = (GlitchType) transitionInfo[0];
                    Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, g.GlitchEaser, g.GlitchLength);
                    tween1.OnUpdate = delegate (Tween t)
                    {
                        Glitch.Value = g.GlitchStrength * t.Eased;
                    };
                    tween1.OnComplete = delegate
                    {
                        if (freezeGameOnTeleport) {
                            level.Frozen = false;
                        }
                        PreTeleport(player, level);
                    };
                    HelperEntities.GetHelperEntity(level).Add(tween1);
                    if (freezeGameOnTeleport) {
                        level.Frozen = true;
                    }
                    tween1.Start();
                    break;
                }
            case TransitionType.Wipe:
                if ((bool) transitionInfo[0]) {
                    AreaData.Get(level.Session).DoScreenWipe(level, wipeIn: false, delegate
                    {
                        if (freezeGameOnTeleport) {
                            level.Frozen = false;
                        }
                        PreTeleport(player, level);
                    });
                    if (freezeGameOnTeleport) {
                        level.Frozen = true;
                    }
                } else {
                    PreTeleport(player, level);
                }
                break;
            default:
                PreTeleport(player, level);
                break;
        }
        level.Paused = false;
        level.PauseLock = true;
    }

    private void PreTeleport(Player player, Level level) {
        preActive = false;
        if (player == null || player.Dead) {
            ClearTeleportFunction(level);
            return;
        }
        VivHelperModule.Session.TeleportAction = Teleport;
        level.PauseLock = true;
    }

    public List<object> PreTeleportFunction(Player player, Level level) {
        if (global::VivHelper.VivHelper.DefineSetState(setState, out var output) && player.StateMachine.State != output) {
            int state = player.StateMachine.State;
            int num = state;
            if (num == 10) {
                player.Speed.Y = -140f;
                player.AutoJump = true;
                DynamicData.For(player).Set("varJumpSpeed", -140f);
            }
        }
        return new List<object> { player.StateMachine.State };
    }

    public void Teleport(Player player, Level level) {
        Player player2 = player;
        if (player2 == null || player2.Dead) {
            Logger.Log(LogLevel.Info, "VivHelperModule", "Attempted to Teleport but player is either dead or null.");
            ClearTeleportFunction(level);
            return;
        }
        Vector2 vector = player.TopLeft - base.TopLeft;
        LevelData levelData = level.Session?.MapData?.Get(specificRoom);
        if (setState == null) {
            try {
                setState = levelData.Triggers.First((EntityData e) => e.Name == "VivHelper/TeleportTarget" && e.NoEmptyString("TargetID", "") == targetID).NoEmptyString("SetState", "-1");
            } catch {
                setState = "-1";
            }
        }
        EntityData entityData = levelData.Triggers.FirstOrDefault((EntityData e) => e.Name == "VivHelper/TeleportTarget" && e.Attr("TargetID") == targetID);
        Facings facing = player.Facing;
        int dashes = player.Dashes;
        Vector2 zero = Vector2.Zero;
        if (VivHelperModule.MatchDashState(player.StateMachine.State)) {
            zero = player.DashDir;
        }
        Vector2 vector2 = level.Camera.Position - player.CameraTarget;
        if (endCutscene) {
            level.EndCutscene();
        }
        Vector2 topLeft = player.TopLeft;
        Leader leader = player.Leader;
        foreach (Follower follower in leader.Followers) {
            if (follower.Entity != null) {
                follower.Entity.Position -= topLeft;
                follower.Entity.AddTag(Tags.Global);
                if (!global::VivHelper.VivHelper.CompareEntityIDs(follower.ParentEntityID, EntityID.None)) {
                    level.Session.DoNotLoad.Add(follower.ParentEntityID);
                }
            }
        }
        for (int i = 0; i < leader.PastPoints.Count; i++) {
            leader.PastPoints[i] -= topLeft;
        }
        if (bringHoldableThrough && player.Holding != null) {
            player.Holding.Entity.AddTag(Tags.Global);
        }
        int output;
        bool flag = global::VivHelper.VivHelper.DefineSetState(setState, out output) && player.StateMachine.State != output;
        if (flag) {
            int state = player.StateMachine.State;
            int num = state;
            if (num == 10) {
                player.Speed.Y = -140f;
                player.AutoJump = true;
                DynamicData.For(player).Set("varJumpSpeed", -140f);
            }
        }
        player.CleanUpTriggers();
        if (resetDashes) {
            player.RefillDash();
        }
        level.Remove(player);
        level.Entities.Remove(level.Entities.FindAll(VivHelperModule.UnloadTypesWhenTeleporting));
        level.Displacement.Clear();
        if (roomName != specificRoom) {
            level.ParticlesBG.Clear();
            level.Particles.Clear();
            level.ParticlesFG.Clear();
            TrailManager.Clear();
        }
        level.UnloadLevel();
        level.Session.Level = specificRoom;
        bool flag2 = false;
        if (entityData != null) {
            Vector2? vector3 = entityData.FirstNodeNullable(levelData.Position);
            if (vector3.HasValue) {
                level.Session.RespawnPoint = vector3.Value;
            } else {
                try {
                    level.Session.RespawnPoint = level.Session.GetSpawnPoint(levelData.Position + entityData.Position);
                } catch (ArgumentOutOfRangeException) {
                    level.Session.RespawnPoint = levelData.Position + entityData.Position + new Vector2(4f, 8f);
                }
            }
            flag2 = true;
        }
        level.Session.FirstLevel = false;
        level.Add(player);
        level.LoadLevel(Player.IntroTypes.Transition);
        TeleportTarget firstEntity = level.Tracker.GetFirstEntity((TeleportTarget t) => t.targetID == targetID);
        if (firstEntity == null) {
            throw new Exception("TeleportTarget not found! This error usually means that no TeleportTarget exists in the room set in your Instant Teleport Trigger.");
        }
        if (!flag2) {
            if (firstEntity.forceSpawnpoint.HasValue) {
                level.Session.RespawnPoint = firstEntity.forceSpawnpoint;
            } else {
                level.Session.RespawnPoint = level.Session.GetSpawnPoint(firstEntity.Position);
            }
        }
        if (firstEntity.addTriggerOffset) {
            player.TopLeft = firstEntity.TopLeft + vector;
        } else {
            player.Center = firstEntity.Center;
            player.Position = player.Position.Floor();
        }
        if (player.CollideCheck<Solid>()) {
            player.Position -= Vector2.UnitY;
            if (player.CollideCheck<Solid>()) {
                player.Position += Vector2.UnitY;
            }
        }
        player.Hair.MoveHairBy(player.TopLeft - topLeft);
        if (!resetDashes) {
            player.Dashes = dashes;
        }
        if (bringHoldableThrough) {
            player.UpdateCarry();
            player.Holding?.Entity?.RemoveTag(Tags.Global);
        }
        foreach (Follower follower2 in leader.Followers) {
            if (follower2.Entity != null) {
                follower2.Entity.Position += player.TopLeft;
                follower2.Entity.RemoveTag(Tags.Global);
                if (!global::VivHelper.VivHelper.CompareEntityIDs(follower2.ParentEntityID, EntityID.None) && !level.Session.Keys.Contains(follower2.ParentEntityID)) {
                    level.Session.DoNotLoad.Remove(follower2.ParentEntityID);
                }
            }
        }
        for (int j = 0; j < leader.PastPoints.Count; j++) {
            leader.PastPoints[j] += player.Position;
        }
        leader.TransferFollowers();
        player.Facing = facing;
        player.Speed = firstEntity.ModifyVelocity(player.Speed);
        player.DashDir = firstEntity.ModifyVelocity(player.DashDir);
        foreach (Trigger item in from t in level.Tracker.GetEntities<Trigger>()
                                 where Collide.Check(player, t) && t.GetType().Name.IndexOf("camera", StringComparison.OrdinalIgnoreCase) > -1
                                 select t) {
            global::VivHelper.VivHelper.PlayerTriggerCheck(player, item);
        }
        level.Camera.Position = player.CameraTarget + vector2;
        Vector2 position = level.Camera.Position;
        if (flag && output == 10) {
            player.SummitLaunch(player.X);
        }
        switch (transition) {
            case TransitionType.Glitch: {
                    GlitchType g = (GlitchType) transitionInfo[1];
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, g.GlitchEaser, g.GlitchLength);
                    tween.OnUpdate = delegate (Tween t)
                    {
                        Glitch.Value = g.GlitchStrength * (1f - t.Eased);
                    };
                    tween.OnComplete = delegate
                    {
                        if (freezeGameOnTeleport) {
                            level.Frozen = false;
                        }
                    };
                    HelperEntities.GetHelperEntity(level).Add(tween);
                    if (freezeGameOnTeleport) {
                        level.Frozen = true;
                    }
                    tween.Start();
                    break;
                }
            case TransitionType.Lightning: {
                    float num2 = player.CameraTarget.X + 90f;
                    bool[] array = new bool[2]
                    {
                (bool)transitionInfo[0],
                (bool)transitionInfo[1]
                    };
                    float[] array2 = (float[]) transitionInfo[2];
                    int num3 = (int) transitionInfo[3];
                    float[] array3 = (float[]) transitionInfo[4];
                    float num4 = array2[0];
                    if (array[0]) {
                        level.Flash(Color.White);
                    }
                    if (array[1]) {
                        level.Shake();
                    }
                    for (int k = 0; k < num3; k++) {
                        level.Add(new LightningStrike(new Vector2(Calc.Random.Range(num2 + array3[0], num2 + array3[1]), player.CameraTarget.Y - 32f), (10 + 30 * k) % 63, 255f, num4));
                        num4 += (float) (Calc.Random.NextDouble() * (double) array2[1]);
                    }
                    break;
                }
            case TransitionType.Wipe:
                TeleportV2Hooks.HackedFocusPoint = player.Position - position - new Vector2(0f, 8f);
                AreaData.Get(level.Session).DoScreenWipe(level, wipeIn: true, delegate
                {
                    TeleportV2Hooks.HackedFocusPoint = null;
                    if (freezeGameOnTeleport) {
                        level.Frozen = false;
                    }
                });
                if (freezeGameOnTeleport) {
                    level.Frozen = true;
                }
                break;
        }
        HelperEntities.GetHelperEntity(level).Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate {
            base.Tag -= (int) Tags.Global;
        }, 0.02f, start: true));
        level.PauseLock = false;
        if (flagsSet != null) {
            string[] array4 = flagsSet;
            foreach (string text in array4) {
                if (!string.IsNullOrWhiteSpace(text)) {
                    level.Session.SetFlag(text);
                }
            }
        }
        ClearTeleportFunction(level);
        RunPersistence(level.Session);
    }

    private void RunPersistence(Session session) {
        switch (persistence) {
            case TriggerPersistence.OncePerMapPlay:
                session.DoNotLoad.Add(ID);
                break;
            case TriggerPersistence.OncePerRetry:
                RemoveSelf();
                break;
        }
    }

    private IEnumerator ErrorRoutine(Level level, string roomName) {
        yield return null;
        Audio.SetMusic(null);
        LevelEnterExt.ErrorMessage = "Failed to enter " + roomName + " because there was no valid TeleportTarget in that room.";
        LevelEnter.Go(new Session(level.Session?.Area ?? new AreaKey(1).SetSID("")), fromSaveData: false);
    }

    private IEnumerator LockRoom(Player player, Rectangle bounds) {
        if (player?.Dead ?? true) {
            yield break;
        }
        while (preActive) {
            if (player.Left < (float) bounds.Left + 1f) {
                player.Left = bounds.Left;
                player.OnBoundsH();
            } else if (player.Right > (float) bounds.Right - 1f) {
                player.Right = bounds.Right;
                player.OnBoundsH();
            } else if (player.Top < (float) bounds.Top + 1f) {
                player.Top = bounds.Top;
                player.OnBoundsV();
            } else if (player.Bottom > (float) bounds.Bottom - 1f) {
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
        if (player == null || player.Dead) {
            ClearTeleportFunction(player?.Scene as Level);
        } else if (player.Left < (float) bounds.Left + 1f) {
            player.Left = bounds.Left;
            player.OnBoundsH();
        } else if (player.Right > (float) bounds.Right - 1f) {
            player.Right = bounds.Right;
            player.OnBoundsH();
        } else if (player.Top < (float) bounds.Top + 1f) {
            player.Top = bounds.Top;
            player.OnBoundsV();
        } else if (player.Bottom > (float) bounds.Bottom - 1f) {
            if (SaveData.Instance.Assists.Invincible) {
                player.Play("event:/game/general/assist_screenbottom");
                player.Bounce(bounds.Bottom);
            } else if (player != null && !player.Dead && player.Bottom < (float) bounds.Bottom) {
                player.Die(Vector2.Zero);
            }
        }
    }
}
