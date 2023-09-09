using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System.Reflection;

namespace VivHelper.Entities {
    public static class CollectibleHooks { }
    /// <summary>
    /// Collectibles are just coins that get saved in Session, but don't account for anything outside of the map they're in. (I already made Tokens for this)
    /// </summary>
    [Tracked]
    [CustomEntity("VivHelper/Collectible")]
    public class Collectible : Entity {

        internal static readonly Dictionary<string, string> legacyAudioManager = new Dictionary<string, string>(5) {
            { "event:/VivHelper/CoinCollect", "event:/VivHelper/coin|type=1" },
            { "event:/VivHelper/CorrectCollect", "event:/VivHelper/coin|type=4" },
            { "event:/VivHelper/DragonCoinCollect","event:/VivHelper/coin|type=2" },
            { "event:/VivHelper/IncorrectCollect","event:/VivHelper/coin|type=5" },
            { "event:/VivHelper/MoveCollect","event:/VivHelper/coin|type=3" }
        };

        internal static int idIntegerForDecalEntities = 0;
        private static IDetour hook_Level_orig_LoadLevel;
        public static void Load() {
            hook_Level_orig_LoadLevel = new ILHook(typeof(Level).GetMethod("orig_LoadLevel", BindingFlags.Public | BindingFlags.Instance), ParseCollectibleDecals);
        }
        public static void Unload() {
            hook_Level_orig_LoadLevel?.Dispose();
        }
        private static void ParseCollectibleDecals(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            ILLabel target = null; //required because out target is not always responsive.
            int lIndex = -1; //LevelData Index
            int dIndex = -1; //DecalData Index
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Session>("get_LevelData"), i2 => i2.MatchStloc(out lIndex))) {
                cursor.EmitDelegate<Action>(() => { idIntegerForDecalEntities = 0; }); //Static variable we use for creating the EntityID for any given room, since this resets on each room no EntityID should overlap (unless someone calls a room `X` and then another room $`X`_decal, which, should be fine
                if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<LevelData>("FgDecals")) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(out int _), instr => instr.MatchBr(out target))) {
                    //brtrue <target> is now our free "continue" operator
                    if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(out dIndex))) //it is safe to assume that since we have retrieved the stloc.s X, that that will remain consistent, since it is within the context of the function running it.
                    {
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldloc, dIndex); //dIndex is absolutely set
                        cursor.Emit(OpCodes.Ldloc, lIndex); //lIndex is absolutely set by first if 
                        cursor.EmitDelegate<Func<Level, DecalData, LevelData, bool>>(ImplementCustomDecalToEntitySet);
                        cursor.Emit(OpCodes.Brtrue, target);
                    }
                }
                if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<LevelData>("BgDecals")) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(out int _), instr => instr.MatchBr(out target))) {
                    //brtrue <target> is now our free "continue" operator
                    if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(out dIndex))) //it is safe to assume that since we have retrieved the stloc.s X, that that will remain consistent, since it is within the context of the function running it.
                    {
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldloc, dIndex); //dIndex is absolutely set
                        cursor.Emit(OpCodes.Ldloc, lIndex); //lIndex is absolutely set by first if 
                        cursor.EmitDelegate<Func<Level, DecalData, LevelData, bool>>(ImplementCustomDecalToEntitySet);
                        cursor.Emit(OpCodes.Brtrue, target);
                    }
                }
            }
        }

        private static bool ImplementCustomDecalToEntitySet(Level level, DecalData data, LevelData levelData) {
            if (data.Texture.StartsWith("VivHelper/Coins/")) {
                var s = data.Texture.Substring(16, data.Texture.Length - 20);
                if (!baseCollectibles.ContainsKey(s)) {
                    s = "AllCollect/goldcoin";
                }
                idIntegerForDecalEntities++;
                level.Add(Collectible.SimpleCollectible(Collectible.baseCollectibles[s], data.Position + new Vector2(levelData.Bounds.Left, levelData.Bounds.Top), data.Scale, new EntityID(levelData.Name + "_$decal", idIntegerForDecalEntities)));
                return true;
            }
            return false;
        }

        public enum HoldableCollectTypes { None = 0, TheoOnly = 1, JellyOnly = 2, TheoAndJelly = 3, AllHoldables = 4 }
        public enum PersistenceTypes { None = 0, OnCollect, OnGroup }

        public static ParticleType P_Flash;
        //Initialized in Module Constructor
        public static Dictionary<string, Collectible> baseCollectibles;


        //Random public variables for easier access, I guess I don't *need* these
        public bool PlayerCollect, SeekerCollect;
        public HoldableCollectTypes HoldableCollect;
        //These two variables are for very very customizable coins
        public Type[] CollectOnExplicitType;
        public Type[] CollectOnExtensibleType;

        public string group; //The "group" that the coins are in. Requires magic CollectibleController to work properly

        public EntityID ID;

        private CollectibleController controller;

        public PersistenceTypes persistence;

        public string SpriteReference;
        public Sprite sprite;

        public string audioEvent;
        public Color particleColor;
        public ParticleType particleType;
        public readonly bool isKeyCoin;

        public bool enabled, collected;
        private Level level;
        public Vector2 Scale;
        public bool MoveUpOnCollect, ProduceSparkles;

        internal Collectible() { }

        /// <summary>
        /// Ideally, only use baseCollectibles items for this
        /// </summary>
        /// <param name="copy">use baseCollectibles[stringhere] please</param>
        /// <returns>a usable collectible</returns>
        public static Collectible SimpleCollectible(Collectible copy, Vector2 position, Vector2 scale, EntityID id) {
            Collectible ret = new Collectible {
                group = "",
                PlayerCollect = copy.PlayerCollect,
                HoldableCollect = copy.HoldableCollect,
                SeekerCollect = copy.SeekerCollect,
                persistence = PersistenceTypes.None,
                CollectOnExplicitType = null,
                CollectOnExtensibleType = null,
                audioEvent = copy.audioEvent,
                particleType = copy.particleType, //Passed by reference because smort efficiency moment :>
                Position = position,
                Scale = scale,
                MoveUpOnCollect = copy.MoveUpOnCollect,
                ProduceSparkles = copy.ProduceSparkles
            };
            if (ret.audioEvent == null)
                ret.audioEvent = "event:/VivHelper/CoinCollect"; //if empty, leave as empty, but if it is a null string, use our default. Empty string connotates no audio
            ret.ID = id;
            if (copy.SpriteReference == null && copy.sprite != null) {
                //Extremely slow, this should ideally not be used
                ret.sprite = (Sprite) VivHelper.CloneSprite(copy.sprite);
            } else {
                try {
                    ret.sprite = VivHelperModule.spriteBank.Create(copy.SpriteReference);
                } catch {
                    ret.sprite = GFX.SpriteBank.Create(copy.SpriteReference);
                }
            }
            ret.sprite.OnLoop = delegate (string _s) {
                if (_s == "empty") {
                    ret.RemoveSelf();
                }
            };
            ret.sprite.Scale = scale;
            ret.DefineHitbox();
            return ret;
        }

        public Collectible(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {

            particleColor = VivHelper.ColorFix(data.Attr("particleColor", "Goldenrod"));
            audioEvent = data.Attr("CollectAudio", "event:/VivHelper/coin:1");
            group = data.Attr("group", "");

            //Collection formats
            PlayerCollect = data.Bool("CollectOnPlayer", true);
            SeekerCollect = data.Bool("CollectOnSeeker", false);
            int _i = 0;
            _i += data.Bool("CollectOnHoldable") ? 4 : 0;
            if (_i == 0) { _i += data.Bool("CollectOnTheo") ? 1 : 0; _i += data.Bool("CollectOnJelly") ? 2 : 0; }
            HoldableCollect = (HoldableCollectTypes) _i;
            string s = data.Attr("CustomTypeSet", "");
            if (!string.IsNullOrWhiteSpace(s)) {
                VivHelper.AppendTypesToArray(s, ref CollectOnExplicitType, ref CollectOnExtensibleType);
            }
            isKeyCoin = data.Bool("isKeyCoin", false);
            SpriteReference = data.Attr("SpriteTag", "goldcoin");
            if (data.Bool("AlwaysUseDefaultSpriteBank", false)) {
                sprite = GFX.SpriteBank.Create(SpriteReference);
            } else {
                try {
                    sprite = VivHelperModule.spriteBank.Create(SpriteReference);
                } catch {
                    sprite = GFX.SpriteBank.Create(SpriteReference);
                }
            }
            sprite.OnLoop = delegate (string _s) {
                if (_s == "empty") {
                    RemoveSelf();
                }
            };
            sprite.Scale = Scale;
            sprite.RenderPosition = sprite.RenderPosition.Floor();
            DefineHitbox();
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            controller = scene.Tracker.GetEntity<CollectibleController>();
            if (controller == null) {
                controller = new CollectibleController(new List<EntityData>());
            }
            controller.Track(this);
            Add(sprite);
            level = scene as Level;
            if (PlayerCollect)
                Add(new PlayerCollider((p) => Collect()));
            if (SeekerCollect)
                Add(new SeekerCollider((s) => Collect()));
        }

        public override void Update() {
            base.Update();
            if (Collidable) {
                switch (HoldableCollect) {
                    case HoldableCollectTypes.AllHoldables:
                        CollideAllByComponent<Holdable>().ForEach(e => Collect());
                        break;
                    case HoldableCollectTypes.TheoAndJelly:
                        CollideAllByComponent<Holdable>().ForEach(e => { if (e.Entity is Glider || e.Entity is CustomGlider || e.Entity is TheoCrystal) Collect(); });
                        break;
                    case HoldableCollectTypes.TheoOnly:
                        CollideAllByComponent<Holdable>().ForEach(e => { if (e.Entity is TheoCrystal) Collect(); });
                        break;
                    case HoldableCollectTypes.JellyOnly:
                        CollideAllByComponent<Holdable>().ForEach(e => { if (e.Entity is Glider || e.Entity is CustomGlider) Collect(); });
                        break;
                }
                if ((CollectOnExplicitType != null && CollectOnExplicitType.Length > 0) || (CollectOnExtensibleType != null && CollectOnExtensibleType.Length > 0)) {
                    foreach (var e in Scene.Entities.getListOfEntities()) {
                        if (CollideCheck(e) && VivHelper.MatchTypeFromTypeSet(e.GetType(), CollectOnExplicitType, CollectOnExtensibleType)) {
                            Collect();
                        }
                    }
                }
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            controller.Untrack(this);
        }

        public void Enable(bool emitParticles) {
            if (enabled)
                return;
            Collidable = true;
            sprite.Play("idle");
            enabled = true;
            if (emitParticles && (particleColor == null || particleColor == Color.Transparent)) {
                level.ParticlesBG.Emit(P_Flash, 5, sprite.Center, Vector2.One * 2, particleColor * 0.25f);
            }
        }

        public void Collect() {
            if (collected)
                return;
            Collidable = false;
            sprite.Play("collect");
            if (particleColor == null || particleColor == Color.Transparent) {
                level.ParticlesBG.Emit(P_Flash, 10, sprite.Center, Vector2.One * 2, particleColor);
            }
            if (!string.IsNullOrWhiteSpace(audioEvent))
                Audio.Play(audioEvent, Position);
            controller.AddCollectedCoin(this);
        }

        public void DefineHitbox() {
            MTexture m = sprite?.Animations?["idle"]?.Frames?[0];
            if (m == null) {
                Collider = new Hitbox(8, 8, -4, -4);
            }
            Collider = new Hitbox(m.Width * Scale.X, m.Height * Scale.Y, m.Width * Scale.X / -2, m.Height * Scale.Y / -2);
        }


        public static void LoadBaseCollectibles() {
            baseCollectibles = new Dictionary<string, Collectible>()
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
        }
    }
}
