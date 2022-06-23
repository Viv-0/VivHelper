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

namespace VivHelper {
    public class VivHelperModule : EverestModule {
        public static VivHelperModule Instance;
        public static SpriteBank spriteBank;
        public static string SeekerFolderPath;
        public static MethodInfo playerWallJump;
        public static MethodInfo RendererList_Update;
        public static MethodInfo Level_get_ShouldCreateCassetteManager = typeof(Level).GetProperty("ShouldCreateCassetteManager", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        private static FieldInfo crushBlockCrushDir;
        public static Dictionary<string, Type> StoredTypesByName;
        public static int type;

        public static int BooState, PinkState, OrangeState, WindBoostState, CustomDashState, CustomBoostState;

        public static bool maxHelpingHandLoaded { get; private set; }
        public static bool extVariantsLoaded { get; private set; }

        public static bool CelesteTASLoaded { get; private set; }
        private static EverestModule CelesteTASModuleInstance;
        private static MethodInfo CelesteTAS_EntityDebugColor;
        private static Hook hook_Scene_OccasionalCelesteTASDataCheck;

        internal static bool createdCassetteManager;
        public static Color? EntityDebugColor { get; private set; } = null;

        internal static string CommandDebugString;

        public static Type[] UnloadTypesWhenTeleporting = null; //Any entity which resets the loaded count of entities of that type, see FlingBird.

        public static string[] UnspawnedEntityNames = new string[]
        { "VivHelper/CollectibleGroup", "VivHelper/MapRespriter", "VivHelper/HideRoomInMap", "VivHelper/CustomDashStateDefiner", "VivHelper/PreviousBerriesToFlag", "VivHelper/DisableArbitrarySpawnInDebug" };

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
            VivHelper.ColorHelper = new Dictionary<string, Color>()
            {
                { "Transparent", Color.Transparent},
                { "AliceBlue", Color.AliceBlue},
                { "AntiqueWhite", Color.AntiqueWhite},
                { "Aqua", Color.Aqua},
                { "Aquamarine", Color.Aquamarine},
                { "Azure", Color.Azure},
                { "Beige", Color.Beige},
                { "Bisque", Color.Bisque},
                { "Black", Color.Black},
                { "BlanchedAlmond", Color.BlanchedAlmond},
                { "Blue", Color.Blue},
                { "BlueViolet", Color.BlueViolet},
                { "Brown", Color.Brown},
                { "BurlyWood", Color.BurlyWood},
                { "CadetBlue", Color.CadetBlue},
                { "Chartreuse", Color.Chartreuse},
                { "Chocolate", Color.Chocolate},
                { "Coral", Color.Coral},
                { "CornflowerBlue", Color.CornflowerBlue},
                { "Cornsilk", Color.Cornsilk},
                { "Crimson", Color.Crimson},
                { "Cyan", Color.Cyan},
                { "DarkBlue", Color.DarkBlue},
                { "DarkCyan", Color.DarkCyan},
                { "DarkGoldenrod", Color.DarkGoldenrod},
                { "DarkGray", Color.DarkGray},
                { "DarkGreen", Color.DarkGreen},
                { "DarkKhaki", Color.DarkKhaki},
                { "DarkMagenta", Color.DarkMagenta},
                { "DarkOliveGreen", Color.DarkOliveGreen},
                { "DarkOrange", Color.DarkOrange},
                { "DarkOrchid", Color.DarkOrchid},
                { "DarkRed", Color.DarkRed},
                { "DarkSalmon", Color.DarkSalmon},
                { "DarkSeaGreen", Color.DarkSeaGreen},
                { "DarkSlateBlue", Color.DarkSlateBlue},
                { "DarkSlateGray", Color.DarkSlateGray},
                { "DarkTurquoise", Color.DarkTurquoise},
                { "DarkViolet", Color.DarkViolet},
                { "DeepPink", Color.DeepPink},
                { "DeepSkyBlue", Color.DeepSkyBlue},
                { "DimGray", Color.DimGray},
                { "DodgerBlue", Color.DodgerBlue},
                { "Firebrick", Color.Firebrick},
                { "FloralWhite", Color.FloralWhite},
                { "ForestGreen", Color.ForestGreen},
                { "Fuchsia", Color.Fuchsia},
                { "Gainsboro", Color.Gainsboro},
                { "GhostWhite", Color.GhostWhite},
                { "Gold", Color.Gold},
                { "Goldenrod", Color.Goldenrod},
                { "Gray", Color.Gray},
                { "Green", Color.Green},
                { "GreenYellow", Color.GreenYellow},
                { "Honeydew", Color.Honeydew},
                { "HotPink", Color.HotPink},
                { "IndianRed", Color.IndianRed},
                { "Indigo", Color.Indigo},
                { "Ivory", Color.Ivory},
                { "Khaki", Color.Khaki},
                { "Lavender", Color.Lavender},
                { "LavenderBlush", Color.LavenderBlush},
                { "LawnGreen", Color.LawnGreen},
                { "LemonChiffon", Color.LemonChiffon},
                { "LightBlue", Color.LightBlue},
                { "LightCoral", Color.LightCoral},
                { "LightCyan", Color.LightCyan},
                { "LightGoldenrodYellow", Color.LightGoldenrodYellow},
                { "LightGray", Color.LightGray},
                { "LightGreen", Color.LightGreen},
                { "LightPink", Color.LightPink},
                { "LightSalmon", Color.LightSalmon},
                { "LightSeaGreen", Color.LightSeaGreen},
                { "LightSkyBlue", Color.LightSkyBlue},
                { "LightSlateGray", Color.LightSlateGray},
                { "LightSteelBlue", Color.LightSteelBlue},
                { "LightYellow", Color.LightYellow},
                { "Lime", Color.Lime},
                { "LimeGreen", Color.LimeGreen},
                { "Linen", Color.Linen},
                { "Magenta", Color.Magenta},
                { "Maroon", Color.Maroon},
                { "MediumAquamarine", Color.MediumAquamarine},
                { "MediumBlue", Color.MediumBlue},
                { "MediumOrchid", Color.MediumOrchid},
                { "MediumPurple", Color.MediumPurple},
                { "MediumSeaGreen", Color.MediumSeaGreen},
                { "MediumSlateBlue", Color.MediumSlateBlue},
                { "MediumSpringGreen", Color.MediumSpringGreen},
                { "MediumTurquoise", Color.MediumTurquoise},
                { "MediumVioletRed", Color.MediumVioletRed},
                { "MidnightBlue", Color.MidnightBlue},
                { "MintCream", Color.MintCream},
                { "MistyRose", Color.MistyRose},
                { "Moccasin", Color.Moccasin},
                { "NavajoWhite", Color.NavajoWhite},
                { "Navy", Color.Navy},
                { "OldLace", Color.OldLace},
                { "Olive", Color.Olive},
                { "OliveDrab", Color.OliveDrab},
                { "Orange", Color.Orange},
                { "OrangeRed", Color.OrangeRed},
                { "Orchid", Color.Orchid},
                { "PaleGoldenrod", Color.PaleGoldenrod},
                { "PaleGreen", Color.PaleGreen},
                { "PaleTurquoise", Color.PaleTurquoise},
                { "PaleVioletRed", Color.PaleVioletRed},
                { "PapayaWhip", Color.PapayaWhip},
                { "PeachPuff", Color.PeachPuff},
                { "Peru", Color.Peru},
                { "Pink", Color.Pink},
                { "Plum", Color.Plum},
                { "PowderBlue", Color.PowderBlue},
                { "Purple", Color.Purple},
                { "Red", Color.Red},
                { "RosyBrown", Color.RosyBrown},
                { "RoyalBlue", Color.RoyalBlue},
                { "SaddleBrown", Color.SaddleBrown},
                { "Salmon", Color.Salmon},
                { "SandyBrown", Color.SandyBrown},
                { "SeaGreen", Color.SeaGreen},
                { "SeaShell", Color.SeaShell},
                { "Sienna", Color.Sienna},
                { "Silver", Color.Silver},
                { "SkyBlue", Color.SkyBlue},
                { "SlateBlue", Color.SlateBlue},
                { "SlateGray", Color.SlateGray},
                { "Snow", Color.Snow},
                { "SpringGreen", Color.SpringGreen},
                { "SteelBlue", Color.SteelBlue},
                { "Tan", Color.Tan},
                { "Teal", Color.Teal},
                { "Thistle", Color.Thistle},
                { "Tomato", Color.Tomato},
                { "Turquoise", Color.Turquoise},
                { "Violet", Color.Violet},
                { "Wheat", Color.Wheat},
                { "White", Color.White},
                { "WhiteSmoke", Color.WhiteSmoke},
                { "Yellow", Color.Yellow},
                { "YellowGreen", Color.YellowGreen}
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
            StoredTypesByName = new Dictionary<string, Type>();
            VariantKevin.P_Activate_Maddy = new ParticleType(CrushBlock.P_Activate) { Color = Calc.HexToColor("AC3232") };
            VariantKevin.P_Activate_Baddy = new ParticleType(CrushBlock.P_Activate) { Color = Calc.HexToColor("9B3FB5") };
            CreateFastDelegates();
            CustomDashStateCh.LoadPresets();
            FloatyFluorescentLight.collidingTypes = new List<Type>(3) { typeof(Lightning) };
            if (VivHelper.TryGetType("Celeste.Mod.JackalCollabHelper.Entities.DarkMatter", out Type t, false)) { FloatyFluorescentLight.collidingTypes.Add(t); }
            if (VivHelper.TryGetType("Celeste.Mod.StrawberryJam2021.Entities.DarkMatter", out Type t2, false)) { FloatyFluorescentLight.collidingTypes.Add(t2); }
            //Would add ChronoHelper but lazy.

            //Cheaty resolution to a huge lag in productivity - This resolves forcing all maps to reload over one berry that is used in like 3 maps that will not be getting updated.
            //Future berries will actually go into this section because of this lag, which is coming soon!
            StrawberryRegistry.Register(typeof(CustomStrawberry), true, false);
        }

        private static IDetour hook_CrushBlock_AttackSequence;
        private static IDetour hook_Player_origUpdate;

        public override void Load() {
            On.Celeste.Leader.Update += newLeaderUpdate;
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
            Blockout.Load();
            HoldableBarrier.Load();
            On.Celeste.Strawberry.OnCollect += CustomBerryCheck;
            //SeekerState.Load();
            Everest.Events.Level.OnLoadBackdrop += Level_OnLoadBackdrop;
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
            Everest.Events.Level.OnLoadEntity += Level_OnLoadEntity;
            Everest.Events.Level.OnEnter += Level_OnEnter;
            IL.Celeste.LevelLoader.LoadingThread += Level_LoadingThread;
            CornerBoostCassetteBlock.Load();
            SolidModifierComponent.Load();
            EntityMuterComponent.Load();
            ExplodeLaunchModifier.Load();
            SpawnPointHooks.Load();
            SeekerKillBarrier.Load();
            type = typeof(Key).GetHashCode();
            //Custom Falling Block Kevin Trigger
            crushBlockCrushDir = typeof(CrushBlock).GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo m = typeof(CrushBlock).GetMethod("AttackSequence", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
            hook_CrushBlock_AttackSequence = new ILHook(m, (il) => AttackSequence_CrushCustomFallingBlock(m.DeclaringType.GetField("<>4__this"), il));

            On.Celeste.Mod.Meta.MapMeta.ApplyTo += parseCustomWipes;

            //Add PreviousRoom value to VivHelperSession
            On.Celeste.Level.TransitionRoutine += Level_TransitionRoutine;
            //Debris hooks for Debris limiter
            On.Celeste.Debris.Added += Debris_Added;
            On.Celeste.Level.Update += Level_Update;

            MoonHooks.Load();
            BoostFunctions.Load();
            TeleportV2Hooks.Load();
            Collectible.Load();
            WrappableCrushBlockReskinnable.Load();
            AudioFixSwapBlock.Load();
            CassetteTileEntity.Load();
            PolygonCollider.Load();

            IL.Monocle.Engine.Update += Engine_Update;
            IL.Monocle.Commands.UpdateClosed += Commands_UpdateClosed;

            

            //ModInterop
            typeof(VivHelperAPI).ModInterop();
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);
            spriteBank = new SpriteBank(GFX.Game, "Graphics/VivHelper/Sprites.xml");

            maxHelpingHandLoaded = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "MaxHelpingHand", VersionString = "1.9.3" });
            extVariantsLoaded = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "ExtendedVariantMode", VersionString = "0.21.0" });

            //Collectible coins require this
            Collectible.P_Flash = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark) { Color = Color.White };
            //Big method, instantiates the Base Collectibles
            Collectible.baseCollectibles = new Dictionary<string, Collectible>()
            {
                {"goldcoin", new Collectible() {
                    PlayerCollect = true,
                    HoldableCollect = Collectible.HoldableCollectTypes.None,
                    SeekerCollect = false,
                    SpriteReference = "goldcoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Gold, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Gold, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"whitecoin", new Collectible() {
                    PlayerCollect = true,
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    SeekerCollect = true,
                    SpriteReference = "whitecoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.WhiteSmoke, Color2 = Color.WhiteSmoke * 0.4f }
                } },
                {"redcoin", new Collectible() {
                    PlayerCollect = true,
                    HoldableCollect = Collectible.HoldableCollectTypes.None,
                    SeekerCollect = false,
                    SpriteReference = "redcoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Red, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Red, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"bluecoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.JellyOnly,
                    PlayerCollect = false, SeekerCollect = false,
                    SpriteReference = "bluecoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Blue, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Blue, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"greencoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.TheoOnly,
                    PlayerCollect = false, SeekerCollect = false,
                    SpriteReference = "greencoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Green, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Green, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"cyancoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    PlayerCollect = false, SeekerCollect = false,
                    SpriteReference = "cyancoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Cyan, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Cyan, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"orangecoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    PlayerCollect = false, SeekerCollect = true,
                    SpriteReference = "orangecoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Orange, Color.WhiteSmoke, 0.2f), Color2 = Color.Lerp(Color.Orange, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                { "purplecoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    PlayerCollect = true, SeekerCollect = true,
                    SpriteReference = "purplecoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Purple, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Purple, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },

                {"AllCollect/goldcoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    PlayerCollect = true, SeekerCollect = true,
                    SpriteReference = "goldcoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Gold, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Gold, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"AllCollect/redcoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    PlayerCollect = true, SeekerCollect = true,
                    SpriteReference = "redcoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Red, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Red, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"AllCollect/bluecoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    PlayerCollect = true, SeekerCollect = true,
                    SpriteReference = "bluecoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Blue, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Blue, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"AllCollect/greencoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    PlayerCollect = true, SeekerCollect = true,
                    SpriteReference = "greencoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Green, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Green, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"AllCollect/cyancoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    PlayerCollect = true, SeekerCollect = true,
                    SpriteReference = "cyancoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Cyan, Color.WhiteSmoke, 0.3f), Color2 = Color.Lerp(Color.Cyan, Color.WhiteSmoke, 0.3f) * 0.5f }
                } },
                {"AllCollect/orangecoin", new Collectible() {
                    HoldableCollect = Collectible.HoldableCollectTypes.AllHoldables,
                    PlayerCollect = true, SeekerCollect = true,
                    SpriteReference = "orangecoin",
                    particleType = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark){Color = Color.Lerp(Color.Orange, Color.WhiteSmoke, 0.2f), Color2 = Color.Lerp(Color.Orange, Color.WhiteSmoke, 0.3f) * 0.5f }
                } }
            };
            SpawnPoint._texture = GFX.Game["VivHelper/player_outline"];



            //Loads in mod-related variables
            CelesteTASLoaded = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "CelesteTAS", VersionString = "3.4.18" });
            if (CelesteTASLoaded && VivHelper.TryGetModule(new EverestModuleMetadata { Name = "CelesteTAS", VersionString = "3.4.18" }, out CelesteTASModuleInstance)) {
                CelesteTAS_EntityDebugColor = CelesteTASModuleInstance.SettingsType.GetProperty("EntityHitboxColor", BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(false);
                hook_Scene_OccasionalCelesteTASDataCheck = new Hook(typeof(Scene).GetMethod("BeforeUpdate", BindingFlags.Public | BindingFlags.Instance), typeof(VivHelperModule).GetMethod("Scene_BeforeUpdate", BindingFlags.NonPublic | BindingFlags.Static));
            }
        }

        public override void Unload() {
            On.Celeste.Leader.Update -= newLeaderUpdate;
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
            Blockout.Unload();
            HoldableBarrier.Unload();
            //SeekerState.Unload();
            Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
            Everest.Events.Level.OnLoadEntity -= Level_OnLoadEntity;
            Everest.Events.Level.OnEnter -= Level_OnEnter;
            IL.Celeste.LevelLoader.LoadingThread -= Level_LoadingThread; //Modified call to resolve rare Scene EntityList lock scenario
            CornerBoostCassetteBlock.Unload();
            SolidModifierComponent.Unload();
            EntityMuterComponent.Unload();
            ExplodeLaunchModifier.Unload();
            SeekerKillBarrier.Unload();
            hook_CrushBlock_AttackSequence?.Dispose();

            hook_Scene_OccasionalCelesteTASDataCheck?.Dispose();
            SpawnPointHooks.Unload();

            On.Celeste.Debris.Added -= Debris_Added;
            On.Celeste.Level.Update -= Level_Update;

            MoonHooks.Unload();

            BoostFunctions.Unload();
            TeleportV2Hooks.Unload();
            Collectible.Unload();
            On.Celeste.Mod.Meta.MapMeta.ApplyTo -= parseCustomWipes;
            WrappableCrushBlockReskinnable.Unload();
            AudioFixSwapBlock.Unload();
            CassetteTileEntity.Unload();
            PolygonCollider.Unload();

            IL.Monocle.Engine.Update -= Engine_Update;
            IL.Monocle.Commands.UpdateClosed -= Commands_UpdateClosed;
        }

        private static void Scene_BeforeUpdate(On.Monocle.Scene.orig_BeforeUpdate orig, Scene self) {
            orig(self);

            if (self.OnRawInterval(self.Paused ? 3f : 40f)) {
                try {
                    EntityDebugColor = (Color) CelesteTAS_EntityDebugColor.Invoke(CelesteTASModuleInstance._Settings, new object[] { });
                } catch {
                    EntityDebugColor = null;
                }
            }
        }

        private void parseCustomWipes(On.Celeste.Mod.Meta.MapMeta.orig_ApplyTo orig, Celeste.Mod.Meta.MapMeta self, AreaData area) {
            orig(self, area);
            if (self.Wipe == "VivHelper/FastSpotlight") {
                area.Wipe = (scene, wipeIn, onComplete) => { new FastSpotlight(scene, wipeIn, onComplete); };
            }
        }

        private static void Level_OnEnter(Session session, bool fromSaveData) {
            if (session.MapData == null)
                return;
            foreach(LevelData level in session.MapData.Levels) {
                foreach(EntityData entity in level.Entities) {
                    if(entity.Name == "VivHelper/PreviousBerriesToFlag") { 
                            AreaKey area = session.Area;
                            var areaModeStats = Celeste.SaveData.Instance.Areas_Safe[area.ID].Modes[(int) area.Mode];
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
                    }
                    
                }
            }
            
        }

        private void Level_Update(On.Celeste.Level.orig_Update orig, Level self) {

            orig(self);
            //Blockout Hook moved from Blockout hooks, legacy
            if (!self.FrozenOrPaused) {
                // progressively fade in or out.
                Blockout.alphaFade = Calc.Approach(Blockout.alphaFade, Session.Blackout ? 0f : 1f, Engine.DeltaTime * 2f);
            }
        }

        private void Debris_Added(On.Celeste.Debris.orig_Added orig, Debris self, Scene scene) {
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
                string SID = AreaData.Get(level.Session).GetSID();
                if (MapSIDsWithOlderCFB_Behavior.Contains(SID)) {
                    level.Add(new CustomFallingBlock_140(entityData, offset));
                } else {
                    level.Add(new CustomFallingBlock(entityData, offset));
                }
                return true;
            }
            if (entityData.Name == "VivHelper/CassetteTileEntity") {
                level.HasCassetteBlocks = true;
                if (level.CassetteBlockTempo == 1f) {
                    level.CassetteBlockTempo = entityData.Float("tempo", 1f);
                }
                level.CassetteBlockBeats = Math.Max(entityData.Int("index", 0) + 1, level.CassetteBlockBeats);

                if (!createdCassetteManager) {
                    createdCassetteManager = true;
                    if (level.Tracker.GetEntity<CassetteBlockManager>() == null && (bool) Level_get_ShouldCreateCassetteManager.Invoke(level, null)) {
                        if (!level.Entities.ToAdd.Any(e => e is CassetteBlockManager)) {
                            level.Entities.ForceAdd(new CassetteBlockManager());
                        }
                    }
                }
            }
            return false;
        }

        private static void AttackSequence_CrushCustomFallingBlock(FieldInfo f, ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(instr => instr.MatchStloc(5))) {
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

        private static void Level_LoadingThread(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(instr => instr.MatchRet());
            if (cursor.TryGotoPrev(instr => instr.MatchLdarg(0))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<LevelLoader>>(LoadingThreadMod);
            }
        }

        private static void LoadingThreadMod(LevelLoader self) {
            if (UnloadTypesWhenTeleporting == null)
                UnloadTypesWhenTeleporting = new Type[] { typeof(FlingBird), VivHelper.GetType("Celeste.Mod.JackalHelper.Entities.BraveBird", false), VivHelper.GetType("Celeste.Mod.JackalHelper.Entities.AltBraveBird", false) };
            //Add all of the HelperEntities
            if (!self.Level.Contains(HelperEntities.FrozenUpdateHelperEntity))
                self.Level.Add(HelperEntities.FrozenUpdateHelperEntity);
            if (!self.Level.Contains(HelperEntities.PauseUpdateHelperEntity))
                self.Level.Add(HelperEntities.PauseUpdateHelperEntity);
            if (!self.Level.Contains(HelperEntities.TransitionUpdateHelperEntity))
                self.Level.Add(HelperEntities.TransitionUpdateHelperEntity);
            if (!self.Level.Contains(HelperEntities.AllUpdateHelperEntity))
                self.Level.Add(HelperEntities.AllUpdateHelperEntity);
            //Holdable Barrier Renderer addition
            List<LevelData> Levels = self.Level.Session?.MapData?.Levels ?? null;
            if (Levels == null)
                return;
            SceneryAdder.LoadingThreadAddendum(Levels, self.Level);
            foreach (LevelData level in Levels) {
                if (level.Entities == null)
                    continue;
                bool b = false;
                if(level.BgDecals?.Any(b => b.Texture.StartsWith("VivHelper/coins/")) ?? false || (level.FgDecals?.Any(b2 => b2.Texture.StartsWith("VivHelper/coins/")) ?? false)) {
                    List<EntityData> datas = new List<EntityData>();
                    foreach (LevelData l in Levels) {
                        if (l.Entities != null) {
                            datas.AddRange(l.Entities.Where(e => e.Name == "VivHelper/CollectibleGroup"));
                        }

                    }
                    self.Level.Add(new CollectibleController(datas));
                }
                foreach(EntityData entity in level.Entities) {
                    if (entity.Name == "VivHelper/HoldableBarrier" && self.Entities.FindFirst<HoldableBarrierRenderer>() == null)
                        self.Level.Add(new HoldableBarrierRenderer());
                    else if (entity.Name == "VivHelper/CrystalBombDetonator" && self.Entities.FindFirst<CrystalBombDetonatorRenderer>() == null)
                        self.Level.Add(new CrystalBombDetonatorRenderer());
                    else if (!b && self.Entities.FindFirst<CollectibleController>() == null && (entity.Name == "VivHelper/CollectibleGroup" || entity.Name == "VivHelper/Collectible")) {
                        List<EntityData> datas = new List<EntityData>();
                        foreach (LevelData l in Levels) {
                            if (l.Entities != null) {
                                datas.AddRange(l.Entities.Where(e => e.Name == "VivHelper/CollectibleGroup"));
                            }

                        }
                        self.Level.Add(new CollectibleController(datas));
                    } else if (entity.Name == "VivHelper/GoldenBerryToFlag" && self.Entities.FindFirst<GoldenBerryFlagController>() == null)
                        self.Level.Add(new GoldenBerryFlagController());
                }
            }
            SpawnPointHooks.AddLevelInfoCache(self.Level.Session);
        }

        private static string[] rainbowObjects = new string[] {"VivHelper/CustomSpinner", "VivHelper/AnimatedSpinner",
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
            if (child.Name.Equals("VivHelper/WindRainFG", StringComparison.OrdinalIgnoreCase))
                return new WindRainFG(new Vector2(child.AttrFloat("scrollx"), child.AttrFloat("scrolly")), child.Attr("colors"), child.AttrFloat("speed"));
            else if (child.Name.Equals("VivHelper/CustomRain", StringComparison.OrdinalIgnoreCase))
                return new CustomRain(new Vector2(child.AttrFloat("scrollx"), child.AttrFloat("scrolly")), child.AttrFloat("angle", 270f), child.AttrFloat("angleDiff", 3f), child.AttrFloat("speedMult", 1f), child.AttrInt("Amount", 240), child.Attr("colors", "161933"), child.AttrFloat("alpha"));
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
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_Red") || instr.MatchCall<Color>("get_White"))) {
                cursor.EmitDelegate<Func<Color, Color>>((Color c) => FlashCombine(c));
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
            //Look if we really need to add force-found indices here then I'll fix this part but do we *really* need to? It's not like someone's going to be foolish enough to add their variable in not at the end of the variable list, and if they do then the community will yell at them, right? Especially on WallJumpCheck.

            //CornerBoostComponent
            VariableDefinition varDef = new VariableDefinition(il.Import(typeof(Vector2)));
            il.Body.Variables.Add(varDef);
            ILLabel returnFalse = null, label1 = null;
            int flagIndex = 0;
            cursor.GotoNext(MoveType.Before, i => i.MatchCallvirt<Player>("ClimbBoundsCheck"), i => i.MatchBrfalse(out returnFalse));
            cursor.Index = 0;
            if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(out flagIndex), i => i.MatchBrfalse(out label1)) && cursor.TryGotoPrev(MoveType.After, i => i.MatchStloc(flagIndex))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Player, int, bool>>((p, d) => RestrictingEntityHooks.AddWallCheck(p, d));
                cursor.Emit(OpCodes.Brtrue, returnFalse);
                //Custom Spike block wallbounces
                cursor.GotoLabel(label1);
                cursor.GotoNext(MoveType.Before, i => i.MatchBrfalse(out _));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<bool, Player, int, bool>>((b, p, d) => CustomSpike.AddWallCheck(p, b, d));
                //CornerBoostBlock info
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCall("Monocle.Entity", "System.Boolean CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)"))) {
                    cursor.Emit(OpCodes.Stloc, varDef);
                    cursor.Emit(OpCodes.Ldloc, varDef);
                    cursor.Index++;
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Ldloc, varDef);
                    cursor.EmitDelegate<Func<bool, Player, Vector2, bool>>((b, player, v) => {
                        if (b)
                            return true;
                        else
                            return ModifiedSolidCollideCheck(player, v);
                    }); //replaces it with my modified collide check which works with CornerBoost Components.
                }
            }

        }

        private static bool ModifiedSolidCollideCheck(Player player, Vector2 at) {

            bool a = false;
            Vector2 position = player.Position;
            player.Position = at;
            foreach (Solid solid in player.Scene.Tracker.Entities[typeof(Solid)]) {
                if (solid.Get<SolidModifierComponent>() != null)
                    a |= SolidModifierComponent.WJ_CollideCheck(solid, player, solid.Get<SolidModifierComponent>().CornerBoostBlock);
                else
                    a |= player.CollideCheck(solid);
                if (a)
                    break;
            }
            player.Position = position;
            return a;
        }

        private void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig.Invoke(self, position, spriteMode);
            BooState = self.StateMachine.AddState(BooMushroom.BooUpdate, null, BooMushroom.BooBegin, BooMushroom.BooEnd);
            PinkState = self.StateMachine.AddState(PinkBoost.PinkUpdate, PinkBoost.PinkCoroutine, PinkBoost.PinkBegin, PinkBoost.PinkEnd);
            OrangeState = self.StateMachine.AddState(OrangeBoost.Update, OrangeBoost.Coroutine, OrangeBoost.Begin, OrangeBoost.End);
            WindBoostState = self.StateMachine.AddState(WindBoost.Update, WindBoost.Coroutine, WindBoost.Begin, WindBoost.End);
            CustomBoostState = self.StateMachine.AddState(UltraCustomBoost.Update, UltraCustomBoost.Coroutine, UltraCustomBoost.Begin, UltraCustomBoost.End);
            CustomDashState = self.StateMachine.AddState(UltraCustomDash.Update, UltraCustomDash.Coroutine, UltraCustomDash.Begin, UltraCustomDash.End);
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

        public static float getFPDistance() {
            return (float) (Settings.FPDistance < 1 ? 0.1f : Settings.FPDistance / 10f);
        }
        public static int getFFDistance() {
            return Settings.FFDistance < 1 ? 1 : Settings.FFDistance;
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


        public static bool MatchDashState(int state) {
            return new int[] { 2, 5, VivHelperModule.OrangeState, VivHelperModule.WindBoostState, VivHelperModule.PinkState, VivHelperModule.CustomDashState }.Contains(state);
        }

        // Used to maintain compatibility for Rainbow stuff with Max's Helping Hand RainbowSpinnerColorController
        public static CrystalStaticSpinner crystalSpinner;

        public static float MagicStaminaFix() { return 110f; } //Right now this isn't useful, but it will be in the future, if any mod caps stamina over 110.

        public static void CreateFastDelegates() {
            //This is always called *after* playerWallJump is defined.
            VivHelper.player_WallJumpCheck = playerWallJump.GetFastDelegate();
            VivHelper.player_WallJump = typeof(Player).GetMethod("WallJump", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
            VivHelper.player_ClimbJump = typeof(Player).GetMethod("ClimbJump", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
            VivHelper.player_DashCorrectCheck = typeof(Player).GetMethod("DashCorrectCheck", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
            VivHelper.player_DreamDashCheck = typeof(Player).GetMethod("DreamDashCheck", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
            VivHelper.player_SuperJump = typeof(Player).GetMethod("SuperJump", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
            VivHelper.player_SuperWallJump = typeof(Player).GetMethod("SuperWallJump", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
            VivHelper.player_Pickup = typeof(Player).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
            VivHelper.player_DustParticleFromSurfaceIndex = typeof(Player).GetMethod("DustParticleFromSurfaceIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
        }

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
            if (Engine.Commands.Open)
                Engine.Commands.Log(message);
            else
                CommandDebugString = (CommandDebugString == null ? message : CommandDebugString + "\n" + message);
        }
    }
}