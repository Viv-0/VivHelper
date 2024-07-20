using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod;
using MonoMod.Utils;
using MonoMod;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace VivHelper.Entities {
    public static class ExplodeLaunchModifier {

        public static bool DisableFreeze = false;
        public static bool DetectFreeze = false;

        public static void Load() {
            // pre-Core: using(new DetourContext { After = {"*"} }) {
            using (new DetourConfigContext(new DetourConfig("VivHelper", before: new[] { "*" })).Use())
                On.Celeste.Celeste.Freeze += _DisableFreeze;
            On.Monocle.Entity.DebugRender += AddBumperWrapperCheck;
        }

        private static void AddBumperWrapperCheck(On.Monocle.Entity.orig_DebugRender orig, Entity self, Camera camera) {
            if (self.Get<BumperWrapperDebugRenderModifier>()?.RenderNewDebug(camera) ?? false)
                return;
            orig(self, camera);
        }

        public static void Unload() {
            On.Celeste.Celeste.Freeze -= _DisableFreeze;
            On.Monocle.Entity.DebugRender -= AddBumperWrapperCheck;
        }

        private static void _DisableFreeze(On.Celeste.Celeste.orig_Freeze orig, float time) {
            if (DisableFreeze) { DetectFreeze = true; return; }
            orig(time);
        }

        public enum BumperModifierTypes { IgnoreAll = 0, Ignore, Cardinal, Diagonal, EightWay, Alt4way }
        public static BumperModifierTypes bumperWrapperType = 0;

        public enum RestrictBoost { NoBoost = -2, OldBehavior = -1, Default = 0, BetterBoost = 1, AlwaysBoost = 2 }

        public static Vector2 ExplodeLaunchMaster(Player self, Vector2 from, bool snapUp, bool sidesOnly, BumperWrapper bw = null) {
            if (sidesOnly)
                return self.ExplodeLaunch(from, snapUp, sidesOnly);
            switch (bumperWrapperType) {

                case BumperModifierTypes.Cardinal:
                    return CardinalLaunch(self, from, bw?.bumperBoost ?? RestrictBoost.Default);
                case BumperModifierTypes.Diagonal:
                    return DiagonalLaunch(self, from);
                case BumperModifierTypes.EightWay:
                    return EightWayLaunch(self, from, bw?.bumperBoost ?? RestrictBoost.Default);
                case BumperModifierTypes.Alt4way:
                    return Alt4WayLaunch(self, from, bw);
                default:
                    if (bw == null || bw.bumperBoost == RestrictBoost.Default)
                        return self.ExplodeLaunch(from, snapUp, sidesOnly);
                    else
                        return ExplodeLaunch(self, from, snapUp, bw.bumperBoost);
            }
        }

        private static Vector2 ExplodeLaunch(Player self, Vector2 from, bool snapUp, RestrictBoost restrictBoost) {
            DynData<Player> dyn = new DynData<Player>(self);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            if (restrictBoost == RestrictBoost.BetterBoost)
                self.Scene.Add(new BetterBoostTest(self, dyn));
            else
                Celeste.Celeste.Freeze(0.1f);
            dyn.Set<float?>("launchApproachX", null);
            Vector2 vector = (self.Center - from).SafeNormalize(-Vector2.UnitY);
            float num = Vector2.Dot(vector, Vector2.UnitY);
            if (snapUp && num <= -0.7f) {
                vector.X = 0f;
                vector.Y = -1f;
            } else if (num <= 0.65f && num >= -0.55f) {
                vector.Y = 0f;
                vector.X = Math.Sign(vector.X);
            }
            self.Speed = 280f * vector;
            if (self.Speed.Y <= 50f) {
                self.Speed.Y = Math.Min(-150f, self.Speed.Y);
                self.AutoJump = true;
            }
            if (self.Speed.X != 0f && restrictBoost != RestrictBoost.NoBoost && restrictBoost != RestrictBoost.BetterBoost) {

                if (Input.MoveX.Value == Math.Sign(self.Speed.X) || restrictBoost == RestrictBoost.AlwaysBoost) {
                    dyn.Set<float>("explodeLaunchBoostTimer", 0f);
                    self.Speed.X *= 1.2f;
                } else if (restrictBoost != RestrictBoost.OldBehavior) {
                    dyn.Set<float>("explodeLaunchBoostTimer", 0.01f);
                    dyn.Set<float>("explodeLaunchBoostSpeed", self.Speed.X * 1.2f);
                }
            }
            SlashFx.Burst(self.Center, self.Speed.Angle());
            if (!self.Inventory.NoRefills) {
                self.RefillDash();
            }
            self.RefillStamina();
            dyn.Set<float>("dashCooldownTimer", 0.2f);
            self.StateMachine.State = 7;
            return vector;
        }

        private static Vector2 CardinalLaunch(Player self, Vector2 from, RestrictBoost restrictBoost, bool restrictDashes = false) {
            DynData<Player> dyn = new DynData<Player>(self);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            if (restrictBoost == RestrictBoost.BetterBoost)
                self.Scene.Add(new BetterBoostTest(self, dyn));
            else
                Celeste.Celeste.Freeze(0.1f);
            dyn.Set<float?>("launchApproachX", null);
            self.Speed = (self.Center - from).SafeNormalize(-Vector2.UnitY).FourWayNormal() * 280f;
            if (self.Speed.Y == 0f) {
                self.Speed.Y = Math.Min(-150f, self.Speed.Y);
                self.AutoJump = true;
            }
            if (self.Speed.X != 0f && restrictBoost != RestrictBoost.NoBoost && restrictBoost != RestrictBoost.BetterBoost) {
                if (Input.MoveX.Value == Math.Sign(self.Speed.X)) {
                    dyn.Set<float>("explodeLaunchBoostTimer", 0f);
                    self.Speed.X *= 1.2f;
                } else if (restrictBoost == RestrictBoost.OldBehavior) {
                    dyn.Set<float>("explodeLaunchBoostTimer", 0.01f);
                    dyn.Set<float>("explodeLaunchBoostSpeed", self.Speed.X * 1.2f);
                }
            }
            SlashFx.Burst(self.Center, self.Speed.Angle());
            if (!self.Inventory.NoRefills && !restrictDashes) {
                self.RefillDash();
            }
            self.RefillStamina();
            dyn.Set<float>("dashCooldownTimer", 0.2f);
            self.StateMachine.State = 7;
            return self.Speed;
        }

        private static Vector2 DiagonalLaunch(Player self, Vector2 from, bool restrictDashes = false) {
            DynData<Player> dyn = new DynData<Player>(self);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Celeste.Freeze(0.1f);
            dyn.Set<float?>("launchApproachX", null);
            Vector2 vector = Diagonal4Way((self.Center - from).SafeNormalize(Vector2.Normalize(-Vector2.UnitY))) * 280f;
            if (self.Speed.Y <= -50f) {
                self.AutoJump = true;
            }
            SlashFx.Burst(self.Center, self.Speed.Angle());
            if (!self.Inventory.NoRefills && !restrictDashes) {
                self.RefillDash();
            }
            self.RefillStamina();
            dyn.Set<float>("dashCooldownTimer", 0.2f);
            self.StateMachine.State = 7;
            return self.Speed;
        }

        private static Vector2 Diagonal4Way(Vector2 v) {
            float f = v.Angle();
            if (f > 0f && f <= Consts.PIover2)
                return Calc.AngleToVector(Consts.PIover4, 1f);
            if (f > Consts.PIover2 && f <= Consts.PI)
                return Calc.AngleToVector(Consts.PIover4 * 3, 1f);
            if (f > (float) -Consts.PI && f <= (float) -Consts.PIover2)
                return Calc.AngleToVector(Consts.PIover4 * -3, 1f);
            return Calc.AngleToVector(-Consts.PIover4, 1f);
        }

        public static Vector2 EightWayLaunch(Player self, Vector2 from, RestrictBoost restrictBoost, bool restrictDashes = false) {
            DynData<Player> dyn = new DynData<Player>(self);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            if (restrictBoost == RestrictBoost.BetterBoost)
                self.Scene.Add(new BetterBoostTest(self, dyn));
            else
                Celeste.Celeste.Freeze(0.1f);
            dyn.Set<float?>("launchApproachX", null);
            self.Speed = (self.Center - from).SafeNormalize(-Vector2.UnitY).EightWayNormal() * 280f;
            if (self.Speed.Y == 0f) {
                self.Speed.Y = Math.Min(-150f, self.Speed.Y);
                self.AutoJump = true;
            }
            if (self.Speed.X != 0f && restrictBoost != RestrictBoost.NoBoost && restrictBoost != RestrictBoost.BetterBoost) {
                if (Input.MoveX.Value == Math.Sign(self.Speed.X)) {
                    dyn.Set<float>("explodeLaunchBoostTimer", 0f);
                    self.Speed.X *= 1.2f;
                } else if (restrictBoost == RestrictBoost.OldBehavior) {
                    dyn.Set<float>("explodeLaunchBoostTimer", 0.01f);
                    dyn.Set<float>("explodeLaunchBoostSpeed", self.Speed.X * 1.2f);
                }
            }
            SlashFx.Burst(self.Center, self.Speed.Angle());
            if (!self.Inventory.NoRefills && !restrictDashes) {
                self.RefillDash();
            }
            self.RefillStamina();
            dyn.Set<float>("dashCooldownTimer", 0.2f);
            self.StateMachine.State = 7;
            return self.Speed;
        }

        private static Vector2 Alt4WayNormal(Vector2 q, BumperWrapper bw) {
            float r = q.Angle();
            Vector2 v;
            if (r >= 1.0472f && r <= 2.0944f) //down angle: if 60 < r < 120 degrees
                v = Calc.AngleToVector(1.5708f, 1f); // 90 deg
            else if (-0.6545f <= r && r < 1.0472f) { //right angle: if -37.5 <= r < 60 degrees
                v = Calc.AngleToVector(-0.436332f, 1f); // 335 deg
                v.Y /= bw.ExplodeMultiplier;
            } else if ((r > 2.0944f && r <= 3.141593f) || (r > -3.1416f && r < -2.4871f)) { //left angle: if 120 < r < 217.5
                v = Calc.AngleToVector(-2.70526f, 1f); // 205 deg
                v.Y /= bw.ExplodeMultiplier;
            } else
                v = Calc.AngleToVector(-1.5708f, 1f);

            return v;
        }

        private static Vector2 Alt4WayLaunch(Player self, Vector2 from, BumperWrapper bw, bool restrictDashes = false) {
            DynData<Player> dyn = new DynData<Player>(self);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Celeste.Freeze(0.1f);
            dyn.Set<float?>("launchApproachX", null);
            self.Speed = Alt4WayNormal((self.Center - from).SafeNormalize(-Vector2.UnitY), bw) * 280f;
            if (self.Speed.Y <= 0f) {
                self.Speed.Y = Math.Min(-150f, self.Speed.Y);
                self.AutoJump = true;
            }
            SlashFx.Burst(self.Center, self.Speed.Angle());
            if (!self.Inventory.NoRefills && !restrictDashes) {
                self.RefillDash();
            }
            self.RefillStamina();
            dyn.Set<float>("dashCooldownTimer", 0.2f);
            self.StateMachine.State = 7;
            return self.Speed;
        }

        private class BetterBoostTest : Entity {
            private Player player;
            private DynData<Player> dyn;
            private bool Better;
            public BetterBoostTest(Player p, DynData<Player> d) : base() {
                Tag = Tags.FrozenUpdate;
                player = p;
                dyn = d;

            }
            public override void Added(Scene scene) {
                base.Added(scene);
                Celeste.Celeste.Freeze(0.05f);
            }

            public override void Update() {
                base.Update();
                bool b = false;

                if (Engine.FreezeTimer > 0f) {
                    if (Input.MoveX.Value == Math.Sign(player.Speed.X) && !b) {
                        dyn.Set<float>("explodeLaunchBoostTimer", 0f);
                        player.Speed.X *= 1.2f;
                        b = true;
                    }
                } else {
                    Celeste.Celeste.Freeze(0.05f);
                    RemoveSelf();
                }


            }



        }
    }
}
