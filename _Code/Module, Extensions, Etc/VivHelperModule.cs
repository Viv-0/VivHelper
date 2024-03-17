using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Editor;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using MonoMod.Utils;
using VivHelper.Colliders;
using VivHelper.Effects;
using VivHelper.Entities;
using VivHelper.PartOfMe;
using VivHelper.Entities.Boosters;
using VivHelper.Module__Extensions__Etc;
using VivHelper.Triggers;
using MonoMod.ModInterop;
using VivHelper.Module__Extensions__Etc.Helpers;
using VivHelper.Entities.SpikeStuff;
using System.Runtime.InteropServices.ComTypes;
using VivHelper.Module__Extensions__Etc.CustomWipeSupport;
using Celeste.Mod.Meta;

namespace VivHelper {
    public class VivHelperModule : EverestModule {
        public static VivHelperModule Instance;
        public static SpriteBank spriteBank;
        public static string SeekerFolderPath;
        public static MethodInfo playerWallJump;
        public static MethodInfo RendererList_Update;
        public static MethodInfo Level_get_ShouldCreateCassetteManager = typeof(Level).GetProperty("ShouldCreateCassetteManager", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        private static FieldInfo crushBlockCrushDir;
        public static int type;

        public static int BooState, PinkState, OrangeState, WindBoostState, CustomDashState, CustomBoostState;

        public static bool maxHelpingHandLoaded { get; private set; }
        public static EverestModule mhhModule;
        public static bool extVariantsLoaded { get; private set; }

        public static bool gravityHelperLoaded { get; private set; }

        public static bool CelesteTASLoaded { get; private set; }
        private static EverestModule CelesteTASModuleInstance;
        private static MethodInfo CelesteTAS_EntityDebugColor;
        private static MethodInfo CelesteTAS_TriggerDebugColor;
        private static Hook hook_Scene_OccasionalCelesteTASDataCheck;

        internal static bool createdCassetteManager;
        public static Color? EntityDebugColor { get; private set; } = null;
        public static Color? TriggerDebugColor { get; private set; } = null;

        internal static string CommandDebugString;

        public static Type[] UnloadTypesWhenTeleporting = null; //Any entity which resets the loaded count of entities of that type, see FlingBird.

        public static string[] UnspawnedEntityNames = new string[]
        { "VivHelper/CollectibleGroup", "VivHelper/MapRespriter", "VivHelper/HideRoomInMap",
          "VivHelper/CustomDashStateDefiner", "VivHelper/PreviousBerriesToFlag", "VivHelper/DisableArbitrarySpawnInDebug",
          "VivHelper/DisableNeutralOnHoldable", "VivHelper/GoldenBerryToFlag"
        };

        public VivHelperModule() {
            Instance = this;
            VivHelper.EaseHelper = new Dictionary<string, Ease.Easer>()
            {
                {"Linear", Ease.Linear },
                {"SineIn", Ease.SineIn },
                {"SineOut", Ease.SineOut },
                {"SineInOut", Ease.SineInOut },
                {"QuadIn", Ease.QuadIn },
                {"QuadOut", Ease.QuadOut },
                {"QuadInOut", Ease.QuadInOut },
                {"CubeIn", Ease.CubeIn },
                {"CubeOut", Ease.CubeOut },
                {"CubeInOut", Ease.CubeInOut },
                {"QuintIn", Ease.QuintIn },
                {"QuintOut", Ease.QuintOut },
                {"QuintInOut", Ease.QuintInOut },
                {"BackIn", Ease.BackIn },
                {"BackOut", Ease.BackOut },
                {"BackInOut", Ease.BackInOut },
                {"ExpoIn", Ease.ExpoIn },
                {"ExpoOut", Ease.ExpoOut },
                {"ExpoInOut", Ease.ExpoInOut },
                {"BigBackIn", Ease.BigBackIn },
                {"BigBackOut", Ease.BigBackOut },
                {"BigBackInOut", Ease.BigBackInOut },
                {"ElasticIn", Ease.ElasticIn },
                {"ElasticOut", Ease.ElasticOut },
                {"ElasticInOut", Ease.ElasticInOut },
                {"BounceIn", Ease.BounceIn },
                {"BounceOut", Ease.BounceOut },
                {"BounceInOut", Ease.BounceInOut }
            }; //Somehow this is more efficient lmao
            VivHelper.colorHelper = new Dictionary<string, Color>()
            {
                { "transparent", Color.Transparent},
                { "aliceblue", Color.AliceBlue},
                { "antiquewhite", Color.AntiqueWhite},
                { "aqua", Color.Aqua},
                { "aquamarine", Color.Aquamarine},
                { "azure", Color.Azure},
                { "beige", Color.Beige},
                { "bisque", Color.Bisque},
                { "black", Color.Black},
                { "blanchedalmond", Color.BlanchedAlmond},
                { "blue", Color.Blue},
                { "blueviolet", Color.BlueViolet},
                { "brown", Color.Brown},
                { "burlywood", Color.BurlyWood},
                { "cadetblue", Color.CadetBlue},
                { "chartreuse", Color.Chartreuse},
                { "chocolate", Color.Chocolate},
                { "coral", Color.Coral},
                { "cornflowerblue", Color.CornflowerBlue},
                { "cornsilk", Color.Cornsilk},
                { "crimson", Color.Crimson},
                { "cyan", Color.Cyan},
                { "darkblue", Color.DarkBlue},
                { "darkcyan", Color.DarkCyan},
                { "darkgoldenrod", Color.DarkGoldenrod},
                { "darkgray", Color.DarkGray},
                { "darkgreen", Color.DarkGreen},
                { "darkkhaki", Color.DarkKhaki},
                { "darkmagenta", Color.DarkMagenta},
                { "darkolivegreen", Color.DarkOliveGreen},
                { "darkorange", Color.DarkOrange},
                { "darkorchid", Color.DarkOrchid},
                { "darkred", Color.DarkRed},
                { "darksalmon", Color.DarkSalmon},
                { "darkseagreen", Color.DarkSeaGreen},
                { "darkslateblue", Color.DarkSlateBlue},
                { "darkslategray", Color.DarkSlateGray},
                { "darkturquoise", Color.DarkTurquoise},
                { "darkviolet", Color.DarkViolet},
                { "deeppink", Color.DeepPink},
                { "deepskyblue", Color.DeepSkyBlue},
                { "dimgray", Color.DimGray},
                { "dodgerblue", Color.DodgerBlue},
                { "firebrick", Color.Firebrick},
                { "floralwhite", Color.FloralWhite},
                { "forestgreen", Color.ForestGreen},
                { "fuchsia", Color.Fuchsia},
                { "gainsboro", Color.Gainsboro},
                { "ghostwhite", Color.GhostWhite},
                { "gold", Color.Gold},
                { "goldenrod", Color.Goldenrod},
                { "gray", Color.Gray},
                { "green", Color.Green},
                { "greenyellow", Color.GreenYellow},
                { "honeydew", Color.Honeydew},
                { "hotpink", Color.HotPink},
                { "indianred", Color.IndianRed},
                { "indigo", Color.Indigo},
                { "ivory", Color.Ivory},
                { "khaki", Color.Khaki},
                { "lavender", Color.Lavender},
                { "lavenderblush", Color.LavenderBlush},
                { "lawngreen", Color.LawnGreen},
                { "lemonchiffon", Color.LemonChiffon},
                { "lightblue", Color.LightBlue},
                { "lightcoral", Color.LightCoral},
                { "lightcyan", Color.LightCyan},
                { "lightgoldenrodyellow", Color.LightGoldenrodYellow},
                { "lightgray", Color.LightGray},
                { "lightgreen", Color.LightGreen},
                { "lightpink", Color.LightPink},
                { "lightsalmon", Color.LightSalmon},
                { "lightseagreen", Color.LightSeaGreen},
                { "lightskyblue", Color.LightSkyBlue},
                { "lightslategray", Color.LightSlateGray},
                { "lightsteelblue", Color.LightSteelBlue},
                { "lightyellow", Color.LightYellow},
                { "lime", Color.Lime},
                { "limegreen", Color.LimeGreen},
                { "linen", Color.Linen},
                { "magenta", Color.Magenta},
                { "maroon", Color.Maroon},
                { "mediumaquamarine", Color.MediumAquamarine},
                { "mediumblue", Color.MediumBlue},
                { "mediumorchid", Color.MediumOrchid},
                { "mediumpurple", Color.MediumPurple},
                { "mediumseagreen", Color.MediumSeaGreen},
                { "mediumslateblue", Color.MediumSlateBlue},
                { "mediumspringgreen", Color.MediumSpringGreen},
                { "mediumturquoise", Color.MediumTurquoise},
                { "mediumvioletred", Color.MediumVioletRed},
                { "midnightblue", Color.MidnightBlue},
                { "mintcream", Color.MintCream},
                { "mistyrose", Color.MistyRose},
                { "moccasin", Color.Moccasin},
                { "navajowhite", Color.NavajoWhite},
                { "navy", Color.Navy},
                { "oldlace", Color.OldLace},
                { "olive", Color.Olive},
                { "olivedrab", Color.OliveDrab},
                { "orange", Color.Orange},
                { "orangered", Color.OrangeRed},
                { "orchid", Color.Orchid},
                { "palegoldenrod", Color.PaleGoldenrod},
                { "palegreen", Color.PaleGreen},
                { "paleturquoise", Color.PaleTurquoise},
                { "palevioletred", Color.PaleVioletRed},
                { "papayawhip", Color.PapayaWhip},
                { "peachpuff", Color.PeachPuff},
                { "peru", Color.Peru},
                { "pink", Color.Pink},
                { "plum", Color.Plum},
                { "powderblue", Color.PowderBlue},
                { "purple", Color.Purple},
                { "red", Color.Red},
                { "rosybrown", Color.RosyBrown},
                { "royalblue", Color.RoyalBlue},
                { "saddlebrown", Color.SaddleBrown},
                { "salmon", Color.Salmon},
                { "sandybrown", Color.SandyBrown},
                { "seagreen", Color.SeaGreen},
                { "seashell", Color.SeaShell},
                { "sienna", Color.Sienna},
                { "silver", Color.Silver},
                { "skyblue", Color.SkyBlue},
                { "slateblue", Color.SlateBlue},
                { "slategray", Color.SlateGray},
                { "snow", Color.Snow},
                { "springgreen", Color.SpringGreen},
                { "steelblue", Color.SteelBlue},
                { "tan", Color.Tan},
                { "teal", Color.Teal},
                { "thistle", Color.Thistle},
                { "tomato", Color.Tomato},
                { "turquoise", Color.Turquoise},
                { "violet", Color.Violet},
                { "wheat", Color.Wheat},
                { "white", Color.White},
                { "whitesmoke", Color.WhiteSmoke},
                { "yellow", Color.Yellow},
                { "yellowgreen", Color.YellowGreen}
            }; //Somehow this is more efficient lmao     
        }

        public override Type SettingsType => typeof(VivHelperModuleSettings);
        public static VivHelperModuleSettings Settings => (VivHelperModuleSettings) Instance._Settings;

        public override Type SaveDataType => typeof(VivHelperModuleSaveData);
        public static VivHelperModuleSaveData SaveData => (VivHelperModuleSaveData) Instance._SaveData;

        public override Type SessionType => typeof(VivHelperModuleSession);
        public static VivHelperModuleSession Session => (VivHelperModuleSession) Instance._Session;

        public static VivHelperModuleSession.HoldableBarrierCh defaultHBController = new VivHelperModuleSession.HoldableBarrierCh() {
            particleColorHex = "5a6ee1",
            baseColorHex = "5a6ee1",
            solidOnRelease = true,
            particleDir = Vector2.UnitY
        };

        public static VivHelperModuleSession.CrystalBombDetonatorCh defaultCBDController = new VivHelperModuleSession.CrystalBombDetonatorCh() {
            particleColorHex = "Yellow",
            baseColorHex = "800080",
            CanBeNeutralized = true,
            DetonationDelay = 0,
            particleDir = Vector2.UnitY
        };

        public override void Initialize() {
            base.Initialize();
            string mainDir = Everest.PathEverest;
            SeekerFolderPath = Path.Combine(mainDir, "Mods", "Cache", "VivHelper_YAMLData");
            playerWallJump = typeof(Player).GetMethod("WallJumpCheck", BindingFlags.Instance | BindingFlags.NonPublic);
            VariantKevin.P_Activate_Maddy = new ParticleType(CrushBlock.P_Activate) { Color = Calc.HexToColor("AC3232") };
            VariantKevin.P_Activate_Baddy = new ParticleType(CrushBlock.P_Activate) { Color = Calc.HexToColor("9B3FB5") };
            VivHelper.CreateFastDelegates();
            CustomDashStateCh.LoadPresets();
            FloatyFluorescentLight.collidingTypes = new List<Type>(3) { typeof(Lightning) };
            if (VivHelper.TryGetType("Celeste.Mod.JackalCollabHelper.Entities.DarkMatter", out Type t, false)) { FloatyFluorescentLight.collidingTypes.Add(t); }
            if (VivHelper.TryGetType("Celeste.Mod.StrawberryJam2021.Entities.DarkMatter", out Type t2, false)) { FloatyFluorescentLight.collidingTypes.Add(t2); }
            //Would add ChronoHelper but lazy.

            //Cheaty resolution to a huge lag in productivity - This resolves forcing all maps to reload over one berry that is used in like 3 maps that will not be getting updated.
            //Future berries will actually go into this section because of this lag, which is coming soon!
            StrawberryRegistry.Register(typeof(CustomStrawberry), true, false);
        }

        private static ILHook hook_CrushBlock_AttackSequence;
        private static ILHook hook_Player_origUpdate;
        private static ILHook hook_Player_origWallJump;
        private static Hook hook_MapMeta_addWipes;

        public override void Load() {
            Logger.SetLogLevel("VivHelper", LogLevel.Info);
            VivHelper.StoredTypesByName = new Dictionary<string, Type>();
            On.Celeste.GameLoader.Begin += LateInitialize;
            IL.Celeste.Leader.Update += Leader_Update;
            On.Celeste.TouchSwitch.ctor_Vector2 += AddCustomSeekerCollision;
            On.Celeste.Player.ctor += Player_ctor;
            On.Celeste.Player.StartDash += orangeRemove;
            IL.Celeste.Player.SummitLaunchUpdate += Player_SummitLaunchUpdate;
            IL.Celeste.Player.ReflectionFallUpdate += Player_ReflectionFallUpdate;
            On.Celeste.DashBlock.OnDashed += AddRedBubbleModdedStates;
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool += BreakHook;
            IL.Celeste.Player.Render += Player_Render;
            IL.Celeste.Player.WallJumpCheck += Player_WallJumpCheck; //Adds Corner Boost accountability, removes Custom Spike Wallbounces, and mods DashAttacking to include CustomBoosters
            EntityChangingInterfaces.Load();
            hook_Player_origUpdate = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance), Player_origUpdate);
            BooMushroom.Load();
            SpeedPowerup.Load();
            RefillCancel.Load();
            HoldableBarrier.Load();
            On.Celeste.Strawberry.OnCollect += CustomBerryCheck;
            //SeekerState.Load();
            Everest.Events.Level.OnLoadBackdrop += Level_OnLoadBackdrop;
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
            Everest.Events.Level.OnLoadEntity += Level_OnLoadEntity;
            Everest.Events.LevelLoader.OnLoadingThread += LoadingThreadMod;
            SolidModifierComponent.Load();
            EntityMuterComponent.Load();
            ExplodeLaunchModifier.Load();
            SpawnPointHooks.Load();
            SeekerKillBarrier.Load();
            BadelineBoostCustom.Load();
            type = typeof(Key).GetHashCode();
            //Custom Falling Block Kevin Trigger
            crushBlockCrushDir = typeof(CrushBlock).GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo m = typeof(CrushBlock).GetMethod("AttackSequence", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
            hook_CrushBlock_AttackSequence = new ILHook(m, (il) => AttackSequence_CrushCustomFallingBlock(m.DeclaringType.GetField("<>4__this"), il));
            hook_Player_origWallJump = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic), DisableNeutralsOnHoldable);
            hook_MapMeta_addWipes = new Hook(typeof(MapMeta).GetMethod("ApplyTo"), typeof(VivHelperModule).GetMethod(nameof(parseCustomWipes), BindingFlags.NonPublic | BindingFlags.Static));

            //Add PreviousRoom value to VivHelperSession
            On.Celeste.Level.TransitionRoutine += Level_TransitionRoutine;
            //Debris hooks for Debris limiter
            On.Celeste.Debris.Added += Debris_Added;
            On.Celeste.Level.Update += Level_Update;
            IL.Celeste.Player.TempleFallUpdate += Player_TempleFallUpdate;
            On.Celeste.Player.TempleFallCoroutine += Player_TempleFallCoroutine;

            MoonHooks.Load();
            DashPowerupManager.Load();
            CustomSeeker.Load();

            BoostFunctions.Load();
            TeleportV2Hooks.Load();
            Collectible.Load();

            CustomCollectible.Load();
            WrappableCrushBlockReskinnable.Load();
            AudioFixSwapBlock.Load();
            CassetteTileEntity.Load();
            PolygonCollider.Load();
            LightRegion.Load();

            IL.Monocle.Engine.Update += Engine_Update;
            IL.Monocle.Commands.UpdateClosed += Commands_UpdateClosed;

            //BronzeBerry.Load();

            //ModInterop
            typeof(VivHelperAPI).ModInterop();
            Debugging.Load();
            //Tester.Load();
        }
        private static void newLeaderUpdate(On.Celeste.Leader.orig_Update orig, Leader self) {
            Vector2 vector = self.Entity.Position + self.Position;
            if (self.PastPoints.Count == 0 || (vector - self.PastPoints[0]).Length() >= getFPDistance()) {
                self.PastPoints.Insert(0, vector);
                if (self.PastPoints.Count > 350) {
                    self.PastPoints.RemoveAt(self.PastPoints.Count - 1);
                }
            }
            int num = (int) getFPDistance();
            foreach (Follower follower in self.Followers) {
                if (num >= self.PastPoints.Count) {
                    break;
                }
                Vector2 value = self.PastPoints[num];
                if (follower.DelayTimer <= 0f && follower.MoveTowardsLeader) {
                    follower.Entity.Position = follower.Entity.Position + (value - follower.Entity.Position) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime / (Settings.MakeClose ? num == 0 ? 0.05f : .25f : 1f)));
                }
                num += getFFDistance();
            }
        }

        private void Leader_Update(ILContext il) {

        }

        private IEnumerator Player_TempleFallCoroutine(On.Celeste.Player.orig_TempleFallCoroutine orig, Player self) {
            yield return new SwapImmediately(orig(self));
            VivHelperModule.Session.OverrideTempleFallX = null;
        }
        private void Player_TempleFallUpdate(ILContext il) {
            ILCursor cursor = new(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(160))) {
                cursor.EmitDelegate<Func<int, int>>(f => VivHelperModule.Session.OverrideTempleFallX ?? f);
            }
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);
            spriteBank = new SpriteBank(GFX.Game, "Graphics/VivHelper/Sprites.xml");

            maxHelpingHandLoaded = Everest.Loader.TryGetDependency(new EverestModuleMetadata { Name = "MaxHelpingHand", VersionString = "1.16.5" }, out mhhModule);
            extVariantsLoaded = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "ExtendedVariantMode", VersionString = "0.21.0" });
            /*gravityHelperLoaded = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "GravityHelper", VersionString = "1.1.10" });
            if (gravityHelperLoaded) {
                typeof(GravityHelperAPI).ModInterop();
            }*/
            //Collectible coins require this
            Collectible.P_Flash = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark) { Color = Color.White };
            //Big method, instantiates the Base Collectibles
            Collectible.LoadBaseCollectibles();
            DashPowerupManager.LoadDefaultPowerups();
            SpawnPoint._texture = GFX.Game["VivHelper/player_outline"];

            //Loads in mod-related variables
            CelesteTASLoaded = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "CelesteTAS", VersionString = "3.4.18" });
            if (CelesteTASLoaded && VivHelper.TryGetModule(new EverestModuleMetadata { Name = "CelesteTAS", VersionString = "3.4.18" }, out CelesteTASModuleInstance)) {
                CelesteTAS_EntityDebugColor = CelesteTASModuleInstance.SettingsType.GetProperty("EntityHitboxColor", BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(false);
                CelesteTAS_TriggerDebugColor = CelesteTASModuleInstance.SettingsType.GetProperty("TriggerHitboxColor", BindingFlags.Instance|BindingFlags.Public)?.GetGetMethod(false);
                hook_Scene_OccasionalCelesteTASDataCheck = new Hook(typeof(Scene).GetMethod("BeforeUpdate", BindingFlags.Public | BindingFlags.Instance), typeof(VivHelperModule).GetMethod("Scene_BeforeUpdate", BindingFlags.NonPublic | BindingFlags.Static));
            }
        }

        public override void Unload() {
            On.Celeste.GameLoader.Begin -= LateInitialize;
            IL.Celeste.Leader.Update -= Leader_Update;
            On.Celeste.TouchSwitch.ctor_Vector2 -= AddCustomSeekerCollision;
            On.Celeste.Player.ctor -= Player_ctor;
            On.Celeste.Player.StartDash -= orangeRemove;
            IL.Celeste.Player.SummitLaunchUpdate -= Player_SummitLaunchUpdate;
            IL.Celeste.Player.ReflectionFallUpdate -= Player_ReflectionFallUpdate;
            On.Celeste.DashBlock.OnDashed -= AddRedBubbleModdedStates;
            IL.Celeste.Player.WallJumpCheck -= Player_WallJumpCheck;
            IL.Celeste.Player.Render -= Player_Render;
            On.Celeste.Strawberry.OnCollect -= CustomBerryCheck;
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool -= BreakHook;
            EntityChangingInterfaces.Unload();
            hook_Player_origUpdate?.Dispose();
            BooMushroom.Unload();
            SpeedPowerup.Unload();
            RefillCancel.Unload();
            HoldableBarrier.Unload();
            //SeekerState.Unload();
            Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
            Everest.Events.Level.OnLoadEntity -= Level_OnLoadEntity;
            Everest.Events.LevelLoader.OnLoadingThread -= LoadingThreadMod;
            SolidModifierComponent.Unload();
            EntityMuterComponent.Unload();
            ExplodeLaunchModifier.Unload();
            SeekerKillBarrier.Unload();
            hook_CrushBlock_AttackSequence?.Dispose();
            BadelineBoostCustom.Unload();

            hook_Scene_OccasionalCelesteTASDataCheck?.Dispose();
            SpawnPointHooks.Unload();

            On.Celeste.Debris.Added -= Debris_Added;
            On.Celeste.Level.Update -= Level_Update;

            MoonHooks.Unload();
            DashPowerupManager.Unload();
            CustomSeeker.Unload();

            BoostFunctions.Unload();
            TeleportV2Hooks.Unload();
            Collectible.Unload();
            hook_MapMeta_addWipes?.Dispose();
            WrappableCrushBlockReskinnable.Unload();
            AudioFixSwapBlock.Unload();
            CassetteTileEntity.Unload();
            PolygonCollider.Unload();
            LightRegion.Unload();

            IL.Monocle.Engine.Update -= Engine_Update;
            IL.Monocle.Commands.UpdateClosed -= Commands_UpdateClosed;

            //BronzeBerry.Unload();


            Debugging.Unload();
        }

        public static void LateInitialize(On.Celeste.GameLoader.orig_Begin orig, GameLoader self) {
            orig(self);
            // Temporary.
            VivHelper.player_WallJumpCheck_getNum = (player, dir) => {
                int num = 3;
                bool flag = player.DashAttacking && player.DashDir.X == 0f && player.DashDir.Y == -1f;
                if (flag) {
                    Spikes.Directions directions = ((dir <= 0) ? Spikes.Directions.Right : Spikes.Directions.Left);
                    foreach (Spikes entity in player.Scene.Tracker.GetEntities<Spikes>()) {
                        if (entity.Direction == directions && player.CollideCheck(entity, player.Position + Vector2.UnitX * dir * 5f)) {
                            flag = false;
                            break;
                        }
                    }
                }
                if (flag) {
                    num = 5;
                }
                return num;
            };
//            CILAbuse.LoadIL();
        }

        private static void Scene_BeforeUpdate(On.Monocle.Scene.orig_BeforeUpdate orig, Scene self) {
            orig(self);
            if (self.OnRawInterval(self.Paused ? 3f : 40f)) {
                try {
                    EntityDebugColor = (Color) CelesteTAS_EntityDebugColor.Invoke(CelesteTASModuleInstance._Settings, Array.Empty<object>());
                } catch {
                    EntityDebugColor = null;
                }
                try {
                    TriggerDebugColor = (Color) CelesteTAS_TriggerDebugColor.Invoke(CelesteTASModuleInstance._Settings, Array.Empty<object>());
                } catch {
                    TriggerDebugColor = null;
                }
            }/*
            if (self.Tracker.TryGetEntities<Atom>(out List<Entity> entities)) {
                entities.ForEach(e => { if (e is Atom a) a.ChangedDepths(); });
            }*/
        }

        private static void parseCustomWipes(Action<MapMeta, AreaData> orig, Celeste.Mod.Meta.MapMeta self, AreaData area) {
            orig(self, area);
            if (self.Wipe == "VivHelper/FastSpotlight") {
                area.Wipe = (scene, wipeIn, onComplete) => { new FastSpotlight(scene, wipeIn, onComplete); };
            }
        }

        private void Level_Update(On.Celeste.Level.orig_Update orig, Level self) {
            self.Session?.SetFlag("VH_Photosensitive", Celeste.Settings.Instance.DisableFlashes);
            orig(self);
            self.Session?.SetFlag("VivHelper/IsPlayerAlive", self.Tracker.TryGetEntity<Player>(out var p) && !p.Dead);
            //Blockout Hook moved from Blockout hooks, legacy
            if (!self.FrozenOrPaused) {
                // progressively fade in or out.
                Blockout.alphaFade = Calc.Approach(Blockout.alphaFade, Session.Blackout ? 0f : 1f, Engine.DeltaTime * 2f);
            }
        }

        private void Debris_Added(On.Celeste.Debris.orig_Added orig, Celeste.Debris self, Scene scene) {
            orig(self, scene);
            if (Session.DebrisLimiter == 1) { self.RemoveSelf(); return; } else if (Session.DebrisLimiter > 0) {
                if (!Calc.Chance(Calc.Random, Session.DebrisLimiter)) { self.RemoveSelf(); }
            }
        }

        private void Player_ReflectionFallUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdcI4(18))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Player>>(f => {
                    Level level = f.Scene as Level;
                    CustomSpinner customSpinner = level.CollideFirst<CustomSpinner>(new Rectangle((int) (f.X - 4f), (int) (f.Y - 40f), 8, 12));
                    if (customSpinner != null) {
                        customSpinner.Destroy();
                        level.Shake();
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                        Celeste.Celeste.Freeze(0.01f);
                    }
                });
            }
        }

        private static void Player_SummitLaunchUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdcI4(10))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Player>>(f => {
                    Level level = f.Scene as Level;
                    CustomSpinner customSpinner = level.CollideFirst<CustomSpinner>(new Rectangle((int) (f.X - 4f), (int) (f.Y - 40f), 8, 12));
                    if (customSpinner != null) {
                        customSpinner.Destroy();
                        level.Shake();
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                        Celeste.Celeste.Freeze(0.01f);
                    }
                });
            }
        }


        private static string[] MapSIDsWithOlderCFB_Behavior = new string[] { "Kaerra/DELTASCENEONE/DeltaSceneOne", "ValentinesContest2021/1-Submissions/KAERRA", "WinterCollab2021/1-Maps/DanTKO", "WinterCollab2021/1-Maps/DanTKO_Viv" };
        private bool Level_OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if (UnspawnedEntityNames.Contains(entityData.Name))
                return true;
            if (entityData.Name == "VivHelper/CustomFallingBlock") {
                string SID = AreaData.Get(level.Session).SID;
                if (MapSIDsWithOlderCFB_Behavior.Contains(SID)) {
                    level.Add(new CustomFallingBlock_140(entityData, offset));
                } else {
                    level.Add(new CustomFallingBlock(entityData, offset));
                }
                return true;
            } else if (entityData.Name == "VivHelper/CassetteTileEntity" ||
                       entityData.Name == "VivHelper/CornerBoostCassetteBlock" ||
                       entityData.Name == "VivHelper/CassetteBooster") {
                level.HasCassetteBlocks = true;
                if (level.CassetteBlockTempo == 1f) {
                    level.CassetteBlockTempo = entityData.Float("tempo", 1f);
                }
                int newCap = entityData.Has("log2idx") ? VivHelper.bitlog2((uint) entityData.Int("log2idx", 1)) + 1 :
                    entityData.Int("index", 0) + 1;
                level.CassetteBlockBeats = Math.Max(level.CassetteBlockBeats, newCap);



                if (!createdCassetteManager) {
                    createdCassetteManager = true;
                    if (level.Tracker.GetEntity<CassetteBlockManager>() == null && (bool) Level_get_ShouldCreateCassetteManager.Invoke(level, null)) {
                        if (!level.Entities.ToAdd.Any(e => e is CassetteBlockManager)) {
                            level.Entities.ForceAdd(new CassetteBlockManager());
                        }
                    }
                }

                switch (entityData.Name) {
                    case "VivHelper/CassetteTileEntity":
                        level.Add(new CassetteTileEntity(entityData, offset));
                        break;
                    case "VivHelper/CornerBoostCassetteBlock":
                        level.Add(new CornerBoostCassetteBlock(entityData, offset, new EntityID(levelData.Name, entityData.ID)));
                        break;
                    case "VivHelper/CassetteBooster":
                        level.Add(new CassetteBooster(entityData, offset));
                        break;
                }
                return true;
            }
            else if(entityData.Name.StartsWith("VivHelper/AnimatedSpikes") && Enum.TryParse<Spikes.Directions>(entityData.Name.Substring(23), out var dir)) {
                switch (entityData.Int("version", 0)) {
                    case 2: level.Add(new BetterAnimatedSpikes(entityData, offset, (DirectionPlus) (1 << (int) dir))); return true;
                    default: level.Add(new AnimatedSpikes(entityData, offset, dir)); return true;
                }
            }
            return false;
        }

        private static void AttackSequence_CrushCustomFallingBlock(FieldInfo f, ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(5))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f);
                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldfld, crushBlockCrushDir);
                cursor.EmitDelegate<Action<CrushBlock, Vector2>>((c, d) => CrushBlockBreakCustomFallingBlocks(c, d));
            }
        }

        private static void CrushBlockBreakCustomFallingBlocks(CrushBlock c, Vector2 cDir) {
            CustomFallingBlock d = c.CollideFirst<CustomFallingBlock>(c.Position + cDir);
            if (d != null && d.IsTriggeredByCrushBlock) {
                d.Triggered = true;
            }
        }

        #region BoostHooks
        //Represents the "edge" that the player is closest to exiting.
        internal static Vector2[] directionSet = new Vector2[] { Vector2.UnitX, Vector2.UnitY, Vector2.UnitX * -1f, Vector2.UnitY * -1f };

        public static int orangeRemove(On.Celeste.Player.orig_StartDash orig, Player self) {
            return (self.StateMachine.State == OrangeState || (self.StateMachine.State == WindBoostState && WindBoost.timer > 0f) || (self.StateMachine.State == PinkState && WindBoost.timer > 0f)) ? 5 : (int) orig.Invoke(self);
        }
        #endregion

        public static void BreakHook(On.Celeste.DashBlock.orig_Break_Vector2_Vector2_bool_bool orig, DashBlock self, Vector2 a, Vector2 b, bool c, bool d) {
            if (self is CustomDashBlock) { (self as CustomDashBlock).Break2(a, b, c, d); } else { orig(self, a, b, c, d); }
        }
        private static void LoadingThreadMod(Level Level) {
            if (UnloadTypesWhenTeleporting == null)
                UnloadTypesWhenTeleporting = new Type[] { typeof(FlingBird), VivHelper.GetType("Celeste.Mod.JackalHelper.Entities.BraveBird", false), VivHelper.GetType("Celeste.Mod.JackalHelper.Entities.AltBraveBird", false) };
            //Add all of the HelperEntities
            if (!(Level.Entities.ToAdd.Contains(HelperEntities.AllUpdateHelperEntity) || Level.Entities.Contains(HelperEntities.AllUpdateHelperEntity))) {
                HelperEntities.AllUpdateHelperEntity = new HelperEntity() { Tag = Tags.FrozenUpdate | Tags.Persistent | Tags.PauseUpdate | Tags.Global | Tags.PauseUpdate };
                Level.Add(HelperEntities.AllUpdateHelperEntity);
            }
            //Holdable Barrier Renderer addition
            List<LevelData> Levels = Level.Session?.MapData?.Levels ?? null;
            if (Levels == null)
                return;
            VivHelperModule.Session.FFDistance = VivHelperModule.Settings.FFDistance;
            VivHelperModule.Session.FPDistance = VivHelperModule.Settings.FPDistance;
            VivHelperModule.Session.MakeClose = VivHelperModule.Settings.MakeClose;
            SceneryAdder.LoadingThreadAddendum(Levels, Level);
            foreach (LevelData level in Levels) {
                if (level.Entities == null)
                    continue;
                bool collCont = false;
                bool hbR = false;
                bool cbdR = false;
                bool cpmh = false;
                bool gbfC = false;
                bool sG = false;
                if (level.BgDecals?.Any(b => b.Texture.StartsWith("VivHelper/coins/")) ?? false || (level.FgDecals?.Any(b2 => b2.Texture.StartsWith("VivHelper/coins/")) ?? false)) {
                    List<EntityData> datas = new List<EntityData>();
                    foreach (LevelData l in Levels) {
                        if (l.Entities != null) {
                            datas.AddRange(l.Entities.Where(e => e.Name == "VivHelper/CollectibleGroup"));
                        }

                    }
                    Level.Add(new CollectibleController(datas));
                }
                foreach (EntityData entity in level.Entities) {
                    if (entity.Name == "VivHelper/HoldableBarrier" && !hbR) {
                        Level.Add(new HoldableBarrierRenderer());
                        hbR = true;
                    } else if (entity.Name == "VivHelper/CrystalBombDetonator" && !cbdR) {
                        Level.Add(new CrystalBombDetonatorRenderer());
                        cbdR = true;
                        /*} else if (entity.Name == "VivHelper/CustomPauseMenuHeader" && !cpmh) {
                            List<EntityData> datas = new List<EntityData>();
                            foreach (LevelData l in Levels) {
                                if (l.Entities != null) {
                                    datas.AddRange(l.Entities.Where(e => e.Name == "VivHelper/CustomPauseMenuHeader"));
                                }
                            }
                            cpmh = true;
                            self.Level.Add(new );*/
                    } else if (!collCont && (entity.Name == "VivHelper/CollectibleGroup" || entity.Name == "VivHelper/Collectible")) {
                        List<EntityData> datas = new List<EntityData>();
                        foreach (LevelData l in Levels) {
                            if (l.Entities != null) {
                                datas.AddRange(l.Entities.Where(e => e.Name == "VivHelper/CollectibleGroup"));
                            }
                        }
                        collCont = true;
                        Level.Add(new CollectibleController(datas));
                    } else if (entity.Name == "VivHelper/GoldenBerryToFlag" && !gbfC) {
                        Level.Add(new GoldenBerryFlagController());
                        gbfC = true;
                    } else if (entity.Name == "VivHelper/PreviousBerriesToFlag") {
                        Session session = Level.Session;
                        AreaKey area = session.Area;
                        AreaModeStats areaModeStats = Celeste.SaveData.Instance.Areas_Safe[area.ID].Modes[(int) area.Mode];
                        ModeProperties modeProperties = AreaData.Get(area).Mode[(int) area.Mode];
                        int totalStrawberries = modeProperties.TotalStrawberries;
                        if (totalStrawberries <= 0) {
                            continue;
                        }
                        int num5 = ((modeProperties.Checkpoints == null) ? 1 : (modeProperties.Checkpoints.Length + 1));
                        for (int i = 0; i < num5; i++) {
                            int num6 = ((i == 0) ? modeProperties.StartStrawberries : modeProperties.Checkpoints[i - 1].Strawberries);
                            for (int j = 0; j < num6; j++) {
                                EntityData entityData = modeProperties.StrawberriesByCheckpoint[i, j];
                                if (entityData == null) {
                                    continue;
                                }
                                foreach (EntityID strawberry2 in areaModeStats.Strawberries) {
                                    if (entityData.ID == strawberry2.ID && entityData.Level.Name == strawberry2.Level) {
                                        session.SetFlag($"VivHelper_PreviousCollectedBerries_{i}:{j}");
                                    }
                                }

                            }
                        }
                    } else if (mutingObjects.Contains(entity.Name)) {
                        VivHelperModule.Session.MapChangesToAudioSet(entity);
                    } else if (entity.Name == "VivHelper/DisableNeutralOnHoldable") {
                        VivHelperModule.Session.DisableNeutralsOnHoldable = true;
                    } else if(entity.Name == "VivHelper/DashPowerupManager") {
                        Session.dashPowerupManager = new DashPowerupManager(entity.Enum("format", PowerupFormat.Override), entity.Bool("convolve"), entity.NoEmptyString("defaultOverride"));
                    } else if(DashPowerupManager.entityDataLinks.TryGetValue(entity.Name, out var value)) {
                        if(Session.dashPowerupManager == null) {
                            Session.dashPowerupManager = new DashPowerupManager(PowerupFormat.Override, false);
                        }
                        Session.dashPowerupManager.validPowerups.AddRange(value);
                    }
                }
            }
            SpawnPointHooks.AddLevelInfoCache(Level.Session);
        }
        private static string[] mutingObjects = new string[] { "VivHelper/SoundMuter", "VivHelper/SoundReplacer" };
        private static string[] rainbowObjects = new string[] {"VivHelper/CustomSpinnerV2", "VivHelper/CustomSpinner", "VivHelper/AnimatedSpinner",
        "VivHelper/RainbowSpikesUp", "VivHelper/RainbowSpikesDown", "VivHelper/RainbowSpikesLeft", "VivHelper/RainbowSpikesRight",
        "VivHelper/RainbowTriggerSpikesUp", "VivHelper/RainbowTriggerSpikesDown", "VivHelper/RainbowTriggerSpikesLeft", "VivHelper/RainbowTriggerSpikesRight"};
        private static MethodInfo player_get_CameraTarget = typeof(Player).GetProperty("CameraTarget", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
        private static void Player_origUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            // Adds Custom Spikes to the list of checked spikes for the no refill check in Player.Update

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<Player>("AutoJump")) && cursor.TryGotoNext(MoveType.After, i2 => i2.MatchLdcR4(110f)) && cursor.TryGotoNext(MoveType.Before, i3 => i3.MatchStfld<Player>("Stamina"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>((a, b) => b.CollideAnyWhere<RestrictingFloor>(c => c.PreventStaminaRefill, out _) ? b.Stamina : a);
            }
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall("Monocle.Entity", "System.Boolean CollideCheck<Celeste.Spikes>(Microsoft.Xna.Framework.Vector2)"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<bool, Player, bool>>((b, player) => b || player.CollideAnyWhere<CustomSpike>(c => c.NoRefillDash, player.Position) || player.CollideAnyWhere<RestrictingFloor>(c => c.PreventDashRefill, out _));
            }
            // Adds a forced Session check for locking the Camera
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt(player_get_CameraTarget))) //Goes to the first instance of get_CameraTarget
            {
                //Goes before the last instance of load integer 18, in StateMachine.State == 18
                //Bneuns modifier: if the parameters we have set in the delegate, return the value that is being compared to in the bneuns instruction
                if (cursor.TryGotoPrev(MoveType.Before, instr => instr.OpCode == OpCodes.Bne_Un_S)) {
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.EmitDelegate<Func<int, Player, int>>((i, p) => {
                        if (Session?.lockCamera != 0) {
                            Session.lockCamera = Session.lockCamera - 1;
                            if (Session.lockCamera == 0 && p.ForceCameraUpdate)
                                p.ForceCameraUpdate = false;
                            return p.StateMachine.State;
                        }
                        return i;
                    });
                }
            }
        }
        private void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            createdCassetteManager = false;
            orig(self, playerIntro, isFromLoader);
            Session.Blackout = false;
        }

        private Backdrop Level_OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above) {
            if (child.Name.Equals("VivHelper/WindRainFG", StringComparison.OrdinalIgnoreCase)) {
                if (child.HasAttr("colors") && !string.IsNullOrWhiteSpace(child.Attr("colors")))
                    return new WindRainFG(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), "§" + child.Attr("colors"), child.AttrFloat("windStrength"));
                else
                    return new WindRainFG(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), child.Attr("Colors"), child.AttrFloat("windStrength"));
            } else if (child.Name.Equals("VivHelper/CustomRain", StringComparison.OrdinalIgnoreCase)) {
                if (child.HasAttr("colors") && !string.IsNullOrWhiteSpace(child.Attr("colors")))
                    return new CustomRain(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), child.AttrFloat("angle", 270f), child.AttrFloat("angleDiff", 3f), child.AttrFloat("speedMult", 1f), child.AttrInt("Amount", 240), "§" + child.Attr("colors", "161933"), child.AttrFloat("alpha"));
                else
                    return new CustomRain(new Vector2(child.AttrFloat("Scrollx"), child.AttrFloat("Scrolly")), child.AttrFloat("angle", 270f), child.AttrFloat("angleDiff", 3f), child.AttrFloat("speedMult", 1f), child.AttrInt("Amount", 240), child.Attr("Colors", "161933"), child.AttrFloat("alpha"));
            } 
            return null;
        }

        private DashCollisionResults AddRedBubbleModdedStates(On.Celeste.DashBlock.orig_OnDashed orig, DashBlock self, Player p, Vector2 d) {
            if (p.StateMachine.State == OrangeState || p.StateMachine.State == WindBoostState || p.StateMachine.State == PinkState) {
                self.Break(p.Center, d, true);
                return DashCollisionResults.Ignore; //We do this because it saves us hooks later, namely, hooking OnCollideH and OnCollideV.
            }
            return orig(self, p, d);
        }

        private void Player_Render(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchStfld<GraphicsComponent>("Color"))) {
                cursor.EmitDelegate<Func<Color, Color>>((Color c) => FlashCombine(c));
                cursor.Index++;
            }
        }

        private static Color FlashCombine(Color c) {
            if (!VivHelper.TryGetPlayer(out Player player))
                return c;
            if (c == Color.Red) {
                return Settings.SetFlashColorToHair ? player.Hair.GetHairColor(player.Dashes) : Color.Red;
            } else if (c == Color.White) {
                return !(Settings.DisableStaminaFlash && new DynData<Player>(player).Get<bool>("IsTired")) ? Color.White : Settings.SetFlashColorToHair ? player.Hair.GetHairColor(player.Dashes) : c;
            } else
                return c; //How in the schweet fuck did this happen wHAT
        }

        private void CustomBerryCheck(On.Celeste.Strawberry.orig_OnCollect orig, Strawberry self) {
            if (self is CustomStrawberry) { (self as CustomStrawberry).OnCollect2(); } else { orig(self); }
        }

        private static void Player_WallJumpCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            ILLabel returnFalse = null, label1 = null;
            int flagIndex = 0;
            cursor.GotoNext(MoveType.Before, i => i.MatchCallvirt<Player>("ClimbBoundsCheck"), i => i.MatchBrfalse(out returnFalse));
            cursor.Index = 0;
            if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(out flagIndex), i => i.MatchBrfalse(out label1)) && cursor.TryGotoPrev(MoveType.After, i => i.MatchStloc(flagIndex))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Call, typeof(RestrictingEntityHooks).GetMethod("AddWallCheck"));
                cursor.Emit(OpCodes.Brtrue, returnFalse);
                //Custom Spike block wallbounces
                cursor.GotoLabel(label1);
                cursor.GotoNext(MoveType.Before, i => i.MatchBrfalse(out _));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Call, typeof(CustomSpike).GetMethod("AddWallCheck"));
            }
        }

        private void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig.Invoke(self, position, spriteMode);
            BooState = self.StateMachine.AddState(BooMushroom.BooUpdate, null, BooMushroom.BooBegin, BooMushroom.BooEnd);
            PinkState = self.StateMachine.AddState(PinkBoost.PinkUpdate, PinkBoost.PinkCoroutine, PinkBoost.PinkBegin, PinkBoost.PinkEnd);
            OrangeState = self.StateMachine.AddState(OrangeBoost.Update, OrangeBoost.Coroutine, OrangeBoost.Begin, OrangeBoost.End);
            WindBoostState = self.StateMachine.AddState(WindBoost.Update, WindBoost.Coroutine, WindBoost.Begin, WindBoost.End);
            CustomBoostState = self.StateMachine.AddState(UltraCustomBoost.Update, UltraCustomBoost.Coroutine, UltraCustomBoost.Begin, UltraCustomBoost.End);
            CustomDashState = self.StateMachine.AddState(UltraCustomDash.CDashUpdate, UltraCustomDash.CDashRoutine, UltraCustomDash.CDashBegin, UltraCustomDash.CDashEnd);
            WarpDashRefill.WarpDashState = self.StateMachine.AddState(WarpDashRefill.WarpDashUpdate, null, WarpDashRefill.WarpDashBegin, WarpDashRefill.WarpDashEnd);
            if(VivHelperModule.Session.dashPowerupManager is DashPowerupManager m) {
                self.Add(new DashPowerupController(true, true));
            }
        }


        private void AddCustomSeekerCollision(On.Celeste.TouchSwitch.orig_ctor_Vector2 orig, TouchSwitch self, Vector2 position) {
            orig.Invoke(self, position);
            CustomSeekerCollider csc = new CustomSeekerCollider(null, new Hitbox(24f, 24f, -12f, -12f));
            csc.OnCollide = delegate {
                if (self.SceneAs<Level>().InsideCamera(self.Position, 10f)) {
                    self.TurnOn();
                }
            };
            self.Add(csc);
        }

        private static float getFPDistance() {
            return (float) (Session.FPDistance < 1 ? 0.1f : Session.FPDistance / 10f);
        }
        private static int getFFDistance() {
            return Session.FFDistance < 1 ? 1 : Session.FFDistance;
        }

        public static RoomWrapController FindWC(Scene scene) {
            List<Entity> temp = scene.Tracker.GetEntities<RoomWrapController>();
            foreach (Entity r in temp) {
                if (r is RoomWrapController v) {
                    if (v.allEntities) { return v; }
                }
            }
            return null;
        }

        public static bool OldGetFlags(Level l, string[] flags, string and_or) {
            if (l == null)
                return false;
            bool b = and_or == "and";
            if (flags == null || flags.Length == 0 || (flags.Length == 1 && flags[0] == ""))
                return true;
            foreach (string flag in flags) {
                if (and_or == "or") { b |= flag[0] != '!' ? l.Session.GetFlag(flag) : !l.Session.GetFlag(flag.TrimStart('!')); } else { b &= flag[0] != '!' ? l.Session.GetFlag(flag) : !l.Session.GetFlag(flag.TrimStart('!')); }
            }
            return b;
        }

        public static IEnumerator TimeSlowDown(float duration = .25f, float speedInit = 0.5f) {
            for (float i = 0; i < duration; i += .05f) {
                Engine.TimeRate = speedInit + (1 - speedInit) * i;
                yield return .05f;
            }
            Engine.TimeRate = 1f;
        }

        private IEnumerator Level_TransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            if (self.Session?.Level != null) {
                Session.PreviousLevel = self.Session.Level;
            }
            return orig(self, next, direction);
        }

        private static void DisableNeutralsOnHoldable(ILContext il) {

            ILCursor cursor = new ILCursor(il);
            if(cursor.TryGotoNext(MoveType.After, i => i.MatchLdarg(0), j => j.MatchLdfld<Player>("moveX"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<int, Player, int, int>>((b, p, d) => {
                    if (b != 0)
                        return b;
                    if(VivHelperModule.Session.DisableNeutralsOnHoldable && p.Holding != null) {
                        return d;
                    }
                    return b;
                });
            }
        }


        public static bool MatchDashState(int state) {
            return new int[] { 2, 5, VivHelperModule.OrangeState, VivHelperModule.WindBoostState, VivHelperModule.PinkState, VivHelperModule.CustomDashState }.Contains(state);
        }

        // Used to maintain compatibility for Rainbow stuff with Maddie's Helping Hand RainbowSpinnerColorController
        public static CrystalStaticSpinner crystalSpinner;

        public static float MagicStaminaFix() { return 110f; } //Right now this isn't useful, but it will be in the future, if any mod caps stamina over 110.

       

        private void Commands_UpdateClosed(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<ButtonBinding>("get_Pressed"))) {
                cursor.EmitDelegate<Func<bool, bool>>((b) => {
                    if (b)
                        return true;
                    if (CommandDebugString != null) {
                        Engine.Commands.Log(CommandDebugString + (Celeste.Mod.Core.CoreModule.Settings.DebugModeInEverest ? "" : "\nExit this menu by typing \"q\" and then hitting enter."));
                        CommandDebugString = null;
                        return true;
                    }
                    return false;
                });
            }
        }

        private void Engine_Update(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Monocle.Commands>("Enabled"))) {
                cursor.EmitDelegate<Func<bool, bool>>((b) => b || CommandDebugString != null);
            }
        }

        public static void SendErrorMessageThroughDebugConsole(string message) {
            if (message == null)
                return;

            Engine.Commands.Open = true;
            Engine.Commands.Log(message);
        }



    }
}