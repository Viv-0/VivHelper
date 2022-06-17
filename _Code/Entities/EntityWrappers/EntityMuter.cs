using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Mono.Cecil;
using FMOD.Studio;

namespace VivHelper.Entities {
    public class EntityMuterComponent : Component {
        #region Hooks
        internal static int mute;
        private static FieldInfo SoundSource_instance = typeof(SoundSource).GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance);

        // I genuinely think that this is the dumbest thing in my helper. This takes the cake.
        public static void Load() {
            On.Celeste.SoundSource.Update += SoundSource_Update;
            On.Celeste.Audio.Play_string += AudioOverride1;
            On.Celeste.Audio.Play_string_Vector2 += AudioOverride2;
            /*hooks = new IDetour[114];
            MethodInfo tempMethodInfo;
            hooks[0] = new ILHook(typeof(BadelineBoost).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[1] = new ILHook(typeof(BirdNPC).GetMethod("<.ctor>b__20_0", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(BirdNPC).GetMethod("Caw").GetStateMachineTarget();
            hooks[2] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            tempMethodInfo = typeof(BirdNPC).GetMethod("ClimbingTutorial", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
            hooks[3] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            tempMethodInfo = typeof(BirdNPC).GetMethod("Startle", BindingFlags.Public | BindingFlags.Instance).GetStateMachineTarget();
            hooks[4] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[5] = new ILHook(typeof(BirdNPC).GetMethod("FlapSfxCheck", BindingFlags.Public|BindingFlags.Static), ArbitraryAudioPlayBlock);
            hooks[6] = new ILHook(typeof(BladeTrackSpinner).GetMethod("OnTrackStart", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[7] = new ILHook(typeof(Bonfire).GetMethod("SetMode", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[8] = new ILHook(typeof(Booster).GetMethod("Appear", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[9] = new ILHook(typeof(Booster).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[10] = new ILHook(typeof(Booster).GetMethod("PlayerBoosted", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[11] = new ILHook(typeof(Booster).GetMethod("PlayerReleased", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[12] = new ILHook(typeof(Booster).GetMethod("Respawn", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[13] = new ILHook(typeof(BounceBlock).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[14] = new ILHook(typeof(BounceBlock).GetMethod("Break", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[15] = new ILHook(typeof(BridgeTile).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[16] = new ILHook(typeof(Bumper).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[17] = new ILHook(typeof(Bumper).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[18] = new ILHook(typeof(Cassette).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(Cassette).GetMethod("CollectRoutine", (BindingFlags)52).GetStateMachineTarget();
            hooks[19] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[20] = new ILHook(typeof(Celeste.Cloud).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[21] = new ILHook(typeof(ClutterDoor).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(ClutterDoor).GetMethod("UnlockRoutine", (BindingFlags)52).GetStateMachineTarget();
            hooks[22] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[23] = new ILHook(typeof(ClutterDoor).GetMethod("OnDashed", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[24] = new ILHook(typeof(CoreModeToggle).GetMethod("SetSprite", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(CrumblePlatform).GetMethod("Sequence", (BindingFlags)52).GetStateMachineTarget();
            hooks[25] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            tempMethodInfo = typeof(CrumblePlatform).GetMethod("TileIn", (BindingFlags)52).GetStateMachineTarget();
            hooks[26] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[27] = new ILHook(typeof(CrumbleWallOnRumble).GetMethod("Break", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[28] = new ILHook(typeof(CrushBlock).GetMethod("Attack", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(CrushBlock).GetMethod("AttackSequence", (BindingFlags)52).GetStateMachineTarget();
            hooks[29] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[30] = new ILHook(typeof(CrystalStaticSpinner).GetMethod("Destroy", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[31] = new ILHook(typeof(DashBlock).GetMethod("Break", new Type[] { typeof(Vector2), typeof(Vector2), typeof(bool), typeof(bool) }), ArbitraryAudioPlayBlock);
            hooks[32] = new ILHook(typeof(DashSwitch).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[33] = new ILHook(typeof(DashSwitch).GetMethod("OnDashed", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[34] = new ILHook(typeof(Door).GetMethod("Open", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[35] = new ILHook(typeof(Door).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[36] = new ILHook(typeof(ExitBlock).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[37] = new ILHook(typeof(FakeHeart).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[38] = new ILHook(typeof(FakeWall).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[39] = new ILHook(typeof(FallingBlock).GetMethod("ShakeSfx", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[40] = new ILHook(typeof(FallingBlock).GetMethod("ImpactSfx", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[41] = new ILHook(typeof(FinalBoss).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[42] = new ILHook(typeof(FinalBoss).GetMethod("<CreateBossSprite>b__34_0", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(FinalBoss).GetMethod("Beam", (BindingFlags)52).GetStateMachineTarget();
            hooks[43] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            tempMethodInfo = typeof(FinalBossMovingBlock).GetMethod("MoveSequence", (BindingFlags)52).GetStateMachineTarget();
            hooks[44] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[45] = new ILHook(typeof(FireBall).GetMethod("OnBounce", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[46] = new ILHook(typeof(FlingBird).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[47] = new ILHook(typeof(FlyFeather).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[48] = new ILHook(typeof(FlyFeather).GetMethod("Respawn", (BindingFlags)52), ArbitraryAudioPlayBlock);
            //We skip Glider::Update because Glider::Update's Audio.Play scenario is dependent on SoundSource playing which should always be false.
            hooks[49] = new ILHook(typeof(Glider).GetMethod("OnCollideH", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[50] = new ILHook(typeof(Glider).GetMethod("OnCollideV", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[51] = new ILHook(typeof(Glider).GetMethod("OnRelease", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[52] = new ILHook(typeof(HeartGem).GetMethod("<Awake>b__30_1", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[53] = new ILHook(typeof(HeartGem).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(HeartGemDoor).GetMethod("Routine", (BindingFlags)52).GetStateMachineTarget();
            hooks[54] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            tempMethodInfo = typeof(IntroCrusher).GetMethod("Sequence", (BindingFlags)52).GetStateMachineTarget();
            hooks[55] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[56] = new ILHook(typeof(Key).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(Key).GetMethod("NodeRoutine", (BindingFlags)52).GetStateMachineTarget();
            hooks[57] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            tempMethodInfo = typeof(Lookout).GetMethod("LookRoutine", (BindingFlags)52).GetStateMachineTarget();
            hooks[58] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[59] = new ILHook(typeof(Celeste.Mod.Entities.CustomMemorial).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[60] = new ILHook(typeof(MoveBlock).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(MoveBlock).GetMethod("Controller", (BindingFlags)52).GetStateMachineTarget();
            hooks[61] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[62] = new ILHook(typeof(MrOshiroDoor).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[63] = new ILHook(typeof(MrOshiroDoor).GetMethod("OnDashed", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[64] = new ILHook(typeof(PlayerPlayback).GetMethod("Restart", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[65] = new ILHook(typeof(PlayerPlayback).GetMethod("SetFrame", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[66] = new ILHook(typeof(PlayerPlayback).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(PlayerSeeker).GetMethod("IntroSequence", (BindingFlags)52).GetStateMachineTarget();
            hooks[67] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[68] = new ILHook(typeof(PlayerSeeker).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[69] = new ILHook(typeof(PlayerSeeker).GetMethod("OnCollide", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[70] = new ILHook(typeof(PlayerSeeker).GetMethod("Dash", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[71] = new ILHook(typeof(Puffer).GetMethod("GotoIdle", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[72] = new ILHook(typeof(Puffer).GetMethod("GotoHit", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[73] = new ILHook(typeof(Puffer).GetMethod("Explode", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[74] = new ILHook(typeof(Puffer).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[75] = new ILHook(typeof(Puffer).GetMethod("Alert", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[76] = new ILHook(typeof(Refill).GetMethod("Respawn", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[77] = new ILHook(typeof(Refill).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(RidgeGate).GetMethod("EnterSequence", (BindingFlags)52).GetStateMachineTarget();
            hooks[78] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[79] = new ILHook(typeof(Seeker).GetMethod("<.ctor>b__58_2", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[80] = new ILHook(typeof(Seeker).GetMethod("SlammedIntoWall", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[81] = new ILHook(typeof(Seeker).GetMethod("AttackBegin", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[82] = new ILHook(typeof(Seeker).GetMethod("SkiddingBegin", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[83] = new ILHook(typeof(Seeker).GetMethod("RegenerateBegin", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[84] = new ILHook(typeof(SeekerStatue).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[85] = new ILHook(typeof(SinkingPlatform).GetMethod("Update", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[86] = new ILHook(typeof(Snowball).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[87] = new ILHook(typeof(Snowball).GetMethod("OnPlayerBounce", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[88] = new ILHook(typeof(Spring).GetMethod("BounceAnimate", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[89] = new ILHook(typeof(StarTrackSpinner).GetMethod("OnTrackStart", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[90] = new ILHook(typeof(Strawberry).GetMethod("OnAnimate", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[91] = new ILHook(typeof(Strawberry).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(Strawberry).GetMethod("FlyAwayRoutine", (BindingFlags)52).GetStateMachineTarget();
            hooks[92] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            tempMethodInfo = typeof(Strawberry).GetMethod("CollectRoutine", (BindingFlags)52).GetStateMachineTarget();
            hooks[93] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            tempMethodInfo = typeof(StrawberrySeed).GetMethod("ReturnRoutine", (BindingFlags)52).GetStateMachineTarget();
            hooks[94] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[95] = new ILHook(typeof(StrawberrySeed).GetMethod("<Awake>b__25_0", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[96] = new ILHook(typeof(SummitCheckpoint).GetMethod("Update"), ArbitraryAudioPlayBlock);
            hooks[97] = new ILHook(typeof(SummitGem).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[98] = new ILHook(typeof(SwapBlock).GetMethod("OnDash", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[99] = new ILHook(typeof(SwapBlock).GetMethod("Update"), ArbitraryAudioPlayBlock);
            tempMethodInfo = typeof(SwitchGate).GetMethod("Sequence", (BindingFlags)52).GetStateMachineTarget();
            hooks[100] = new ILHook(tempMethodInfo, (i) => ArbitraryAudioPlayBlockForStateMachine(i, tempMethodInfo));
            hooks[101] = new ILHook(typeof(TempleCrackedBlock).GetMethod("Break"), ArbitraryAudioPlayBlock);
            hooks[102] = new ILHook(typeof(TempleGate).GetMethod("Open"), ArbitraryAudioPlayBlock);
            hooks[103] = new ILHook(typeof(TempleGate).GetMethod("Close"), ArbitraryAudioPlayBlock);
            hooks[104] = new ILHook(typeof(TemplePortalTorch).GetMethod("Light"), ArbitraryAudioPlayBlock);
            hooks[105] = new ILHook(typeof(TheoCrystal).GetMethod("Update"), ArbitraryAudioPlayBlock);
            hooks[106] = new ILHook(typeof(TheoCrystal).GetMethod("HitSeeker"), ArbitraryAudioPlayBlock);
            hooks[107] = new ILHook(typeof(TheoCrystal).GetMethod("Die"), ArbitraryAudioPlayBlock);
            hooks[108] = new ILHook(typeof(TheoCrystal).GetMethod("OnCollideH", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[109] = new ILHook(typeof(TheoCrystal).GetMethod("OnCollideV", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[110] = new ILHook(typeof(TheoCrystalPedestal).GetMethod("<.ctor>b__2_0", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[111] = new ILHook(typeof(Torch).GetMethod("OnPlayer", (BindingFlags)52), ArbitraryAudioPlayBlock);
            hooks[112] = new ILHook(typeof(Water).GetMethod("Update"), ArbitraryAudioPlayBlock);
            hooks[113] = new ILHook(typeof(WhiteBlock).GetMethod("Activate", (BindingFlags)52), ArbitraryAudioPlayBlock);*/
        }


        public static void Unload() {
            On.Celeste.SoundSource.Update -= SoundSource_Update;
            On.Celeste.Audio.Play_string += AudioOverride1;
            On.Celeste.Audio.Play_string_Vector2 += AudioOverride2;
            //This is an extremely comedic line of code.
            //foreach(IDetour hook in hooks) hook.Dispose();
        }

        private static EventInstance AudioOverride1(On.Celeste.Audio.orig_Play_string orig, string path) {
            if (mute != 0)
                return null;
            return orig(path);
        }
        private static EventInstance AudioOverride2(On.Celeste.Audio.orig_Play_string_Vector2 orig, string path, Vector2 position) {
            if (mute != 0)
                return null;
            return orig(path, position);
        }

        private static void SoundSource_Update(On.Celeste.SoundSource.orig_Update orig, SoundSource self) {
            if (mute != 0) {
                if (self.Playing) {
                    switch (mute) {
                        case 1:
                            self.Pause();
                            break;
                        case -1:
                            (SoundSource_instance.GetValue(self) as EventInstance).setVolume(0f);
                            break;
                    }
                }
            } else if (!self.Playing) {
                switch (mute) {
                    case 1:
                        self.Resume();
                        break;
                    case -1:
                        (SoundSource_instance.GetValue(self) as EventInstance).setVolume(1f);
                        break;
                }
            }
        }
        #endregion


        public string flag;
        public int val;
        public EntityMuterComponent(string flag = null, bool contiguousAudio = false) : base(true, false) {
            this.flag = flag;
            val = contiguousAudio ? -1 : 1;
        }

        public override void Added(Entity entity) {
            base.Added(entity);
            entity.PreUpdate += Entity_PreUpdate;
            entity.PostUpdate += Entity_PostUpdate;
        }

        private void Entity_PreUpdate(Entity obj) {
            if (flag == null)
                mute = val;
            //mute will always be 0 at Entity_PreUpdate until modified by this function, so we don't need to conditionally set the value.
            else if (obj.SceneAs<Level>()?.Session?.GetFlag(flag) ?? false)
                mute = val;
        }
        private void Entity_PostUpdate(Entity obj) {
            mute = 0;
        }
    }

    [CustomEntity("VivHelper/EntityMuter")]
    [Tracked]
    public class EntityMuter : Entity {
        public List<Type> Types, assignableTypes;
        public bool all;
        public string flag;

        public EntityMuter(EntityData e, Vector2 v) : base(e.Position + v) {
            Collider = new Hitbox(e.Width, e.Height);
            all = e.Bool("all");
            string q = e.Attr("Types", "");
            assignableTypes = new List<Type>();
            Types = new List<Type>();
            VivHelper.AppendTypesToList(q, ref Types, ref assignableTypes);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Collidable = true;
            foreach (Entity e in scene.Entities.Where<Entity>((f) => Collide.Check(this, f))) {
                Type t = e.GetType();
                if (Types.Contains(t) || assignableTypes.Any((u) => u.IsAssignableFrom(t))) {
                    e.Add(new EntityMuterComponent());
                    if (!all)
                        break;
                }

            }
        }
    }
}
