using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;
using Celeste.Mod;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/BadelineBoostNoRefill = BaddyBoostNoRefill", "VivHelper/BadelineBoostCustomRefill = BaddyBoostCustom", "VivHelper/BadelineBoostCustom = BaddyBoostCustom")]
    [Tracked]
    public class BadelineBoostCustom : BadelineBoost {
        #region Hooks
        private static Func<BadelineBoost, Player> baddyboost_holding = typeof(BadelineBoost).GetField("holding", BindingFlags.NonPublic | BindingFlags.Instance).CreateFastGetter<BadelineBoost, Player>();
        private static IDetour hook_BadelineBoost_AddCustomRefill;
        private static FieldInfo player_dashCooldownTimer = typeof(Player).GetField("dashCooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Load() {
            using (new DetourContext { After = { "*" } }) {
                MethodInfo n = typeof(BadelineBoost).GetMethod("BoostRoutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
                hook_BadelineBoost_AddCustomRefill = new ILHook(n, (il) => BoostRoutineAddCustomRefillTraits(n.DeclaringType.GetField("<>4__this"), il)); //Fixes "visual" bug with BadelineBoost, and replaces the Action within the Alarm to be a new function that resolves itself.

                On.Celeste.BadelineBoost.BoostRoutine += BadelineBoost_BoostRoutine;
                IL.Celeste.Player.BadelineBoostLaunch += BaddyBoostLaunchMod;
            }
        }

        public static void Unload() {
            hook_BadelineBoost_AddCustomRefill?.Dispose();
            hook_BadelineBoost_AddCustomRefill = null;
            On.Celeste.BadelineBoost.BoostRoutine -= BadelineBoost_BoostRoutine;
            IL.Celeste.Player.BadelineBoostLaunch -= BaddyBoostLaunchMod;
        }

        public const string BadelineBoostMultCacheName = "VH_BBLM";

        private static void BaddyBoostLaunchMod(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-330f))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>((f, p) => {
                    DynamicData d = DynamicData.For(p);
                    float q = f;
                    if (d.Get(BadelineBoostMultCacheName) != null) {
                        q *= (float) d.Get(BadelineBoostMultCacheName);
                        d.Set(BadelineBoostMultCacheName, null);
                    }
                    return q;
                });
            }

        }

        private static IEnumerator BadelineBoost_BoostRoutine(On.Celeste.BadelineBoost.orig_BoostRoutine orig, BadelineBoost self, Player player) {
            if (self is BadelineBoostCustom custom) {
                int oldDash = player.Dashes;
                float oldStam = player.Stamina;
                yield return new SwapImmediately(orig(self, player));
                BaddyBoostCustomRefill(custom, player, oldDash, oldStam);
                if (VivHelperModule.extVariantsLoaded) {
                    player.Add(new Coroutine(BaddyBoostCustomRefillIEnum(custom, player, oldDash, oldStam)));
                }

            } else {
                yield return new SwapImmediately(orig(self, player));
            }

        }
        internal static IEnumerator BaddyBoostCustomRefillIEnum(BadelineBoostCustom b, Player p, int dash, float stam) {
            BaddyBoostCustomRefill(b, p, dash, stam);
            yield return null; //Wacky ExtVar override shit
            BaddyBoostCustomRefill(b, p, dash, stam);
            yield return null; //Weird bug with SwapImmediately, with the wacky extvar override shit.
            BaddyBoostCustomRefill(b, p, dash, stam);
        }
        internal static void BaddyBoostCustomRefill(BadelineBoostCustom b, Player p, int dash, float stam) {
            p.Dashes = b.replaceDashes(dash, p.Inventory.Dashes);
            p.Stamina = b.replaceStamina(stam, 110f);
        }



        private static void BoostRoutineAddCustomRefillTraits(FieldInfo f, ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<Player>("DummyGravity"))) {
                var cursor2 = cursor.Clone();
                ILLabel target = cursor.Clone().GotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("RefillStamina")).MarkLabel();
                if (cursor2.TryGotoNext(MoveType.After, instr => instr.MatchStloc(8))) {
                    var cursor3 = cursor2.Clone();

                    if (cursor3.TryGotoNext(MoveType.Before, instr => instr.MatchLdloc(1))) {
                        var cursor4 = cursor.Clone();
                        cursor4.Index = 0;
                        //Reset cursor4.
                        if (cursor4.TryGotoNext(i => i.MatchLdcR4(0.15f), i2 => i2.MatchLdcI4(1), i3 => i3.MatchCall<Alarm>("Create"))) {
                            cursor.Emit(OpCodes.Ldarg_0);
                            cursor.Emit(OpCodes.Ldfld, f);
                            cursor.EmitDelegate<Func<BadelineBoost, bool>>((b) => {
                                if (b is BadelineBoostCustom) {
                                    Player d = baddyboost_holding(b);
                                    d.Dashes = (b as BadelineBoostCustom).replaceDashes(d.Dashes, d.Inventory.Dashes); //replaceDashes is a Func<int, int> which takes in the current Dashes and outputs a new value for Dashes
                                    d.Stamina = (b as BadelineBoostCustom).replaceStamina(d.Stamina, 110f); //replaceStamina is a Func<float, float> which takes in the current Stamina and outputs a new value for Stamina
                                    return true;
                                } else
                                    return false;
                            });
                            cursor.Emit(OpCodes.Brtrue, target);

                            cursor2.Emit(OpCodes.Ldarg_0);
                            cursor2.Emit(OpCodes.Ldfld, f);
                            cursor2.EmitDelegate<Action<BadelineBoost>>((b) => { if (b is BadelineBoostCustom && (b as BadelineBoostCustom).FloorPositionOnLaunch) baddyboost_holding.Invoke(b).Position.Floor(); });

                            cursor3.Emit(OpCodes.Ldarg_0);
                            cursor3.Emit(OpCodes.Ldfld, f);
                            cursor3.EmitDelegate<Func<Player, BadelineBoost, Player>>((p, b) => {
                                if (b is BadelineBoostCustom b2) {
                                    if (b2.LaunchStrengthMultiplier == null) {
                                        int ind = DynamicData.For(b).Get<int>("nodeIndex");
                                        if (ind > b2.LaunchStrengthMultiplierSet.Length) {
                                            Console.WriteLine("Uhoh.");
                                            //Do nothing here
                                        } else {
                                            DynamicData.For(p).Add(BadelineBoostMultCacheName, b2.LaunchStrengthMultiplierSet[ind]);
                                        }
                                    } else {
                                        DynamicData.For(p).Add(BadelineBoostMultCacheName, b2.LaunchStrengthMultiplier);
                                    }
                                }
                                return p;
                            });

                            cursor4.Emit(OpCodes.Ldarg_0);
                            cursor4.Emit(OpCodes.Ldfld, f);
                            cursor4.EmitDelegate<Func<Action, BadelineBoost, Action>>(ModifiedBoostAlarm);
                        }
                    }
                }
            }
        }


        internal static Action ModifiedBoostAlarm(Action orig, BadelineBoost bb) {
            if (bb is not BadelineBoostCustom c)
                return orig;
            if (c.DashLogic == "D")
                return orig;
            if (VivHelper.GetPlayer() is not Player player)
                return orig;
            return new Action(delegate {
                    int d = player?.Dashes ?? 0;
                    orig();
                    if(player!=null) player.Dashes = d; // Crash prevention
                });
        }
        #endregion

        public static Entity BaddyBoostNoRefill(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new BadelineBoostCustom(entityData, offset, true);
        public static Entity BaddyBoostCustom(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new BadelineBoostCustom(entityData, offset, false);
        /// <summary>
        /// arg1: player.Dashes
        /// arg2: player.Inventory.Dashes
        /// </summary>
        public Func<int, int, int> replaceDashes;
        /// <summary>
        /// arg1: player.Stamina
        /// arg2: maxStamina
        /// </summary>
        public Func<float, float, float> replaceStamina;
        public bool FloorPositionOnLaunch, ignoreCameraLock;
        public float? LaunchStrengthMultiplier;
        public float[] LaunchStrengthMultiplierSet;
        public string DashLogic, StamLogic;

        public BadelineBoostCustom(EntityData data, Vector2 offset, bool noRefillOverride) : base(data, offset) {
            replaceDashes = (i, j) => i;
            replaceStamina = (i, j) => i;
            if (noRefillOverride || (data.Has("NoDashRefill") && !data.Has("DashesLogic"))) {
                DashLogic = data.Bool("NoDashRefill") || noRefillOverride ? "+0" : "D";
            } else {
                DashLogic = data.Attr("DashesLogic", "");
            }
            if (data.Has("NoStamRefill") && !data.Has("StaminaLogic")) {
                DashLogic = data.Bool("NoStamRefill") ? "+0" : "D";
            } else {
                StamLogic = data.Attr("StaminaLogic", "");
            }
            FloorPositionOnLaunch = data.Bool("FloorPositionOnLaunch", false);
            LaunchStrengthMultiplier = 0; //This value is restricted by the code.
            string lsm = data.Attr("LaunchStrengthMultiplier", "1");
            if (lsm.Contains(',')) {
                string[] q_ = lsm.Split(',');
                int r = data.Nodes.Length; //Yes, this needs to not be NodesWithPosition, because we should account for the fact that final node never boosts.
                LaunchStrengthMultiplierSet = new float[r];
                for (int i = 0; i < Math.Min(r, q_.Length); i++) {
                    LaunchStrengthMultiplierSet[i] = float.Parse(q_[i]);
                }
                for (int i = Math.Min(r, q_.Length); i < r; i++) {
                    LaunchStrengthMultiplierSet[i] = 1;
                }
                LaunchStrengthMultiplier = null;

            } else
                LaunchStrengthMultiplier = Math.Max(0.1f, float.Parse(lsm)); //Minimum cap at launch speed of 30 (330 * 1/33)
            ignoreCameraLock = data.Bool("IgnoreCameraLock", false);

        }

        public override void Added(Scene scene) {
            base.Added(scene);
            SetRefillActions(DashLogic, StamLogic, scene);
        }

        public void SetRefillActions(string dashes, string stamina, Scene scene) {
            if (string.IsNullOrWhiteSpace(dashes) || dashes == "D")
                replaceDashes = (i, j) => j > 1 ? 1 : i < j ? j : i; //Simplified func for refilling dashes in BadelineBoost::BoostRoutine
            else {
                int b = 0;
                if (dashes[0] == '+' || dashes[0] == '-') {
                    b = dashes[0] == '+' ? 1 : -1;
                    dashes = dashes.Substring(1);
                }
                if (!VivHelper.IntParserV1(dashes, this.IntParserEntityValues_Default(), out int outDash)) {
                    replaceDashes = (i, j) => j > 1 ? 1 : i < j ? j : i;
                } else {
                    switch (b) {
                        case -1:
                            replaceDashes = (i, j) => Math.Max(0, i - outDash);
                            break;
                        case 1:
                            replaceDashes = (i, j) => Math.Max(0, i + outDash);
                            break;
                        default:
                            replaceDashes = (i, j) => Math.Max(0, outDash);
                            break;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(stamina) || stamina == "D")
                replaceStamina = (i, j) => j;
            else {
                int c = 0;
                if (stamina[0] == '+' || stamina[0] == '-') {
                    c = stamina[0] == '+' ? 1 : -1;
                    stamina = stamina.Substring(1);
                }
                if (!VivHelper.FloatParserV1(stamina, this.FloatParserEntityValues_Default(), out float outStam)) {
                    replaceStamina = (i, j) => j;
                } else {
                    switch (c) {
                        case -1:
                            replaceStamina = (i, j) => Math.Max(0, i - outStam);
                            break;
                        case 1:
                            replaceStamina = (i, j) => Math.Max(0, i + outStam);
                            break;
                        default:
                            replaceStamina = (i, j) => Math.Max(0, outStam);
                            break;
                    }
                }
            }
        }
    }
}
