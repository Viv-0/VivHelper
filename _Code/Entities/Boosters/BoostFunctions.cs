using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Collections;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using static Celeste.TrackSpinner;

namespace VivHelper.Entities.Boosters {
    public static class BoostFunctions {

        public static void CustomBoost(this Player player, CustomBooster booster) {
            player.Position = booster.Center;
            player.Speed = Vector2.Zero;
            DynamicData.For(player).Set("boostTarget", booster.Center);
            booster.PlayerBoosted(player);
        }
        public static void Load() {
            On.Celeste.Player.Die += Player_Die;
            On.Celeste.Player.OnCollideH += Player_OnCollideH;
            On.Celeste.Player.OnCollideV += Player_OnCollideV;
            IL.Celeste.Player.BeforeDownTransition += il => TranslateRedDash(il, true);
            IL.Celeste.Player.BeforeUpTransition += il => TranslateRedDash(il, true);
            IL.Celeste.Player.OnBoundsH += il => TranslateRedDash(il, true);
            IL.Celeste.Player.OnBoundsV += il => TranslateRedDash(il, true);
            IL.Celeste.Player.OnCollideH += il => TranslateRedDash(il, false);
            IL.Celeste.Player.OnCollideV += il => TranslateRedDash(il, false);
            IL.Celeste.DashBlock.OnDashed += TranslateRedDash2;
            On.Celeste.Player.Bounce += Player_Bounce;
            On.Celeste.Player.SuperBounce += Player_SuperBounce;

        }

        public static void Unload() {
            On.Celeste.Player.Die -= Player_Die;
            IL.Celeste.Player.BeforeDownTransition -= il => TranslateRedDash(il, true);
            IL.Celeste.Player.BeforeUpTransition -= il => TranslateRedDash(il, true);
            IL.Celeste.Player.OnBoundsH -= il => TranslateRedDash(il, true);
            IL.Celeste.Player.OnBoundsV -= il => TranslateRedDash(il, true);
            On.Celeste.Player.OnCollideH -= Player_OnCollideH;
            On.Celeste.Player.OnCollideV -= Player_OnCollideV;
            IL.Celeste.Player.OnCollideH -= il => TranslateRedDash(il, false);
            IL.Celeste.Player.OnCollideV -= il => TranslateRedDash(il, false);
            On.Celeste.Player.Bounce -= Player_Bounce;
            On.Celeste.Player.SuperBounce -= Player_SuperBounce;
        }

        private static void TranslateRedDash(ILContext il, bool b) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(5) && instr.Previous.MatchCallvirt<StateMachine>("get_State"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, Player, int>>(
                    (f, p) => f == VivHelperModule.WindBoostState || f == VivHelperModule.PinkState || f == VivHelperModule.OrangeState || f == VivHelperModule.CustomDashState ? p.StateMachine.State : f);
            }
        }
        private static void TranslateRedDash2(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(5) && instr.Previous.MatchCallvirt<StateMachine>("get_State"))) {
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Ldfld, typeof(Player).GetField("StateMachine"));
                cursor.Emit(OpCodes.Callvirt, typeof(StateMachine).GetMethod("get_State"));
                cursor.Emit(OpCodes.Call, typeof(BoostFunctions).GetMethod("TransRedDash2"));
            }
        }

        public static int TransRedDash(int orig, int pState) {
            return pState == VivHelperModule.PinkState || pState == VivHelperModule.WindBoostState || pState == VivHelperModule.OrangeState ||
                (pState == VivHelperModule.CustomDashState && VivHelperModule.Session.CurrentBooster is UltraCustomBooster booster && booster.customDashState.DashDuration < 0) ? pState : orig;
        }
        public static int TransRedDash2(int orig, int pState) {
            return pState == VivHelperModule.PinkState || pState == VivHelperModule.WindBoostState || pState == VivHelperModule.OrangeState ? pState : orig;
        }
        private static void Player_Bounce(On.Celeste.Player.orig_Bounce orig, Player self, float fromY) {
            int pState = self.StateMachine.State;
            if ((pState == VivHelperModule.PinkState || pState == VivHelperModule.WindBoostState || pState == VivHelperModule.OrangeState || pState == VivHelperModule.CustomDashState) && VivHelperModule.Session.CurrentBooster is { } booster) {
                booster.PlayerReleased();
            }
            orig(self, fromY);
        }
        private static void Player_SuperBounce(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY) {
            int pState = self.StateMachine.State;
            if ((pState == VivHelperModule.PinkState || pState == VivHelperModule.WindBoostState || pState == VivHelperModule.OrangeState || pState == VivHelperModule.CustomDashState) && VivHelperModule.Session.CurrentBooster is { } booster) {
                booster.PlayerReleased();
            }
            orig(self, fromY);
        }

        public static void Player_OnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            if (self.StateMachine.State != VivHelperModule.CustomDashState) { orig.Invoke(self, data); return; }
            CustomDashStateCh customDashState = VivHelperModule.Session.customDashState;
            if (VivHelperModule.Session.CurrentBooster != null && VivHelperModule.Session.CurrentBooster is UltraCustomBooster booster) {
                customDashState = booster.customDashState;
            }
            if (customDashState == null) {
                self.StateMachine.State = 0;
                return;
            }
            DynamicData dyn = DynamicData.For(self);
            if (customDashState.PrioritizeCornerCorrection) {
                Vector2 speed = Vector2.Normalize(self.Speed);
                if (self.Speed.Y == 0f && self.Speed.X != 0f) {
                    for (int i = 1; i <= 4; i++) {
                        for (int num = 1; num >= -1; num -= 2) {
                            Vector2 vector = new Vector2(Math.Sign(self.Speed.X), i * num);
                            Vector2 vector2 = self.Position + vector;
                            if (!self.CollideCheck<Solid>(vector2) && self.CollideCheck<Solid>(vector2 - Vector2.UnitY * num) && !(bool) VivHelper.player_DashCorrectCheck.Invoke(self, new object[] { vector })) {
                                self.MoveVExact(i * num);
                                self.MoveHExact(Math.Sign(self.Speed.X));
                                return;
                            }
                        }
                    }
                }
                if (customDashState.CanEnterDreamBlock && (bool) VivHelper.player_DreamDashCheck.Invoke(self, new object[] { Vector2.UnitX * Math.Sign(self.Speed.X) })) {
                    self.StateMachine.State = 9;
                    dyn.Set("dashAttackTimer", 0f);
                    dyn.Set("gliderBoostTimer", 0f);
                    return;
                }
            }
            switch (customDashState.DashSolidEffect) {

                case DashSolidContact.Kill:
                    if (Math.Abs(Vector2.Dot(Vector2.Normalize(self.Speed), Vector2.UnitX)) >= customDashState.DashKillDotLimit) { self.Die(Vector2.Normalize(-self.Speed)); }
                    if (customDashState.ImpactsObjectsAsDash && data.Direction.X == Math.Sign(self.Speed.X))
                        data.Hit?.OnDashCollide?.Invoke(self, data.Direction);
                    return;
                case DashSolidContact.Bounce:
                    int g = 0;
                    while (self.CollideCheck(data.Hit, self.Position - (data.Direction * ++g))) { }
                    self.Speed.X = -self.Speed.X;
                    self.DashDir.X = -self.DashDir.X;
                    if (customDashState.ImpactsObjectsAsDash && data.Direction.X == Math.Sign(-self.Speed.X))
                        data.Hit?.OnDashCollide?.Invoke(self, data.Direction);
                    return;
                case DashSolidContact.Ignore:
                    if (customDashState.ImpactsObjectsAsDash && data.Hit != null && data.Hit.OnDashCollide != null) {
                        data.Hit.OnDashCollide.Invoke(self, data.Direction);
                        if (customDashState.DashDuration < 0)
                            return;
                    }
                    break;
                case DashSolidContact.Default:
                    if (customDashState.ImpactsObjectsAsDash && data.Hit != null && data.Hit.OnDashCollide != null && Math.Sign(data.Direction.X) == Math.Sign(self.DashDir.X)) {
                        switch (data.Hit.OnDashCollide(self, data.Direction)) {
                            case DashCollisionResults.Rebound:
                                self.Rebound();
                                return;
                            case DashCollisionResults.Bounce:
                                self.ReflectBounce(new Vector2(-Math.Sign(self.Speed.X), 0f));
                                return;
                            case DashCollisionResults.Ignore:
                                return;
                        }
                    }
                    break;
            }
            if (!customDashState.PrioritizeCornerCorrection) {
                if (self.Speed.Y == 0f && self.Speed.X != 0f) {
                    for (int num = 1; num >= -1; num -= 2) {
                        for (int i = 1; i <= 4; i++) {
                            Vector2 vector = new Vector2(Math.Sign(self.Speed.X), i * num);
                            Vector2 vector2 = self.Position + vector;
                            if (!self.CollideCheck<Solid>(vector2) && self.CollideCheck<Solid>(vector2 - Vector2.UnitY * num) && !(bool) VivHelper.player_DashCorrectCheck.Invoke(self, new object[] { vector })) {
                                self.MoveVExact(i * num);
                                self.MoveHExact(Math.Sign(self.Speed.X));
                                return;
                            }
                        }
                    }
                }
                if (customDashState.CanEnterDreamBlock && (bool) VivHelper.player_DreamDashCheck.Invoke(self, new object[] { Vector2.UnitX * Math.Sign(self.Speed.X) })) {
                    self.StateMachine.State = 9;
                    dyn.Set("dashAttackTimer", 0f);
                    dyn.Set("gliderBoostTimer", 0f);
                    return;
                }
            }
            if (dyn.Get<float>("wallSpeedRetentionTimer") <= 0f) {
                dyn.Set("wallSpeedRetained", self.Speed.X);
                dyn.Set("wallSpeedRetentionTimer", 0.06f);
            }
            self.Speed.X = 0f;
            dyn.Set("dashAttackTimer", 0f);
            dyn.Set("gliderBoostTimer", 0f);
            if (customDashState.DashDuration < 0f) {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                self.SceneAs<Level>().Displacement.AddBurst(self.Center, 0.5f, 8f, 48f, 0.4f, Ease.QuadOut, Ease.QuadOut);
                self.StateMachine.State = 6;
            }
        }

        public static void Player_OnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            if (self.StateMachine.State != VivHelperModule.CustomDashState) { orig(self, data); return; }
            CustomDashStateCh customDashState = VivHelperModule.Session.customDashState;
            if (VivHelperModule.Session.CurrentBooster != null && VivHelperModule.Session.CurrentBooster is UltraCustomBooster booster) {
                customDashState = booster.customDashState;
            }
            if (customDashState == null) {
                self.StateMachine.State = 0;
                return;
            }
            DynamicData dyn = DynamicData.For(self);
            if (customDashState.PrioritizeCornerCorrection && CornerCorrectionV(self, dyn, customDashState.CanEnterDreamBlock))
                return;
            switch (customDashState.DashSolidEffect) {

                case DashSolidContact.Kill:
                    if (Math.Abs(Vector2.Dot(Vector2.Normalize(self.Speed), Vector2.UnitY)) >= 0.3f) { self.Die(Vector2.Normalize(-self.Speed)); }
                    if (customDashState.ImpactsObjectsAsDash)
                        data.Hit.OnDashCollide?.Invoke(self, data.Direction);
                    return;
                case DashSolidContact.Bounce:
                    int g = 0;
                    while (self.CollideCheck(data.Hit, self.Position - (data.Direction * ++g))) { }
                    self.Speed.Y = -self.Speed.Y;
                    self.DashDir.Y = -self.DashDir.Y;
                    if (customDashState.ImpactsObjectsAsDash && data.Direction.Y == Math.Sign(-self.Speed.Y))
                        data.Hit.OnDashCollide?.Invoke(self, data.Direction);
                    return;
                case DashSolidContact.Ignore:
                    if (customDashState.ImpactsObjectsAsDash && data.Hit != null && data.Hit.OnDashCollide != null) {
                        data.Hit.OnDashCollide.Invoke(self, data.Direction);
                        if (customDashState.DashDuration < 0)
                            return;
                    }
                    break;
                case DashSolidContact.Default:
                    if (customDashState.ImpactsObjectsAsDash && data.Hit != null && data.Hit.OnDashCollide != null && Math.Sign(data.Direction.Y) == Math.Sign(self.DashDir.Y)) { 
                        switch (data.Hit.OnDashCollide(self, data.Direction)) {
                            case DashCollisionResults.Rebound:
                                self.Rebound();
                                return;
                            case DashCollisionResults.Bounce:
                                self.ReflectBounce(new Vector2(0f, -Math.Sign(self.Speed.Y)));
                                return;
                            case DashCollisionResults.Ignore:
                                return;
                        }
                    }
                    break;
            }
            if (!customDashState.PrioritizeCornerCorrection && CornerCorrectionV(self, dyn, customDashState.CanEnterDreamBlock))
                return;
            if (customDashState.ImpactsObjectsAsDash) {
                data.Hit?.OnCollide?.Invoke(data.Direction);
            }
            self.Speed.Y = 0f;
            dyn.Set("dashAttackTimer", 0f);
            dyn.Set("gliderBoostTimer", 0f);
            if (customDashState.DashDuration < 0f) {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                self.SceneAs<Level>().Displacement.AddBurst(self.Center, 0.5f, 8f, 48f, 0.4f, Ease.QuadOut, Ease.QuadOut);
                self.StateMachine.State = 6;
            }
        }

        internal static bool CornerCorrectionV(Player self, DynamicData dyn, bool canEnterDreamBlock) {
            if (self.Speed.Y > 0f) {
                if (!dyn.Get<bool>("dashStartedOnGround")) {
                    if (self.Speed.X <= 0.01f) {
                        for (int num = -1; num >= -4; num--) {
                            if (!self.OnGround(self.Position + new Vector2(num, 0f))) {
                                self.MoveHExact(num);
                                self.MoveVExact(1);
                                return true;
                            }
                        }
                    }
                    if (self.Speed.X >= -0.01f) {
                        for (int i = 1; i <= 4; i++) {
                            if (!self.OnGround(self.Position + new Vector2(i, 0f))) {
                                self.MoveHExact(i);
                                self.MoveVExact(1);
                                return true;
                            }
                        }
                    }
                }
                if (canEnterDreamBlock && (bool) VivHelper.player_DreamDashCheck.Invoke(self, new object[] { Vector2.UnitY * Math.Sign(self.Speed.Y) })) {
                    self.StateMachine.State = 9;
                    dyn.Set("dashAttackTimer", 0f);
                    dyn.Set("gliderBoostTimer", 0f);
                    return true;
                }
                if (self.DashDir.X != 0f && self.DashDir.Y > 0f && self.Speed.Y > 0f) {
                    self.DashDir.X = Math.Sign(self.DashDir.X);
                    self.DashDir.Y = 0f;
                    self.Speed.Y = 0f;
                    self.Speed.X *= 1.2f;
                    self.Ducking = true;
                }
                if (self.StateMachine.State != 1) {
                    float amount = Math.Min(self.Speed.Y / 240f, 1f); //240f is "Max FastFall speed" not dash speed.
                    self.Sprite.Scale.X = MathHelper.Lerp(1f, 1.6f, amount);
                    self.Sprite.Scale.Y = MathHelper.Lerp(1f, 0.4f, amount);
                    if (dyn.Get<float>("highestAirY") < self.Y - 50f && self.Speed.Y >= 160f && Math.Abs(self.Speed.X) >= 90f) {
                        self.Sprite.Play("runStumble");
                    }
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                    Celeste.Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(self.CollideAll<Celeste.Platform>(self.Position + new Vector2(0f, 1f), dyn.Get<List<Entity>>("temp")));
                    int num2 = -1;
                    if (platformByPriority != null) {
                        num2 = platformByPriority.GetLandSoundIndex(self);
                        if (num2 >= 0 && !self.MuffleLanding) {
                            self.Play((dyn.Get<float>("playFootstepOnLand") > 0f) ? SFX.char_mad_footstep : SFX.char_mad_land, "surface_index", num2);
                        }
                        if (platformByPriority is DreamBlock) {
                            (platformByPriority as DreamBlock).FootstepRipple(self.Position);
                        }
                        self.MuffleLanding = false;
                    }
                    if (self.Speed.Y >= 80f) {
                        Dust.Burst(self.Position, new Vector2(0f, -1f).Angle(), 8, (ParticleType) VivHelper.player_DustParticleFromSurfaceIndex.Invoke(self, new object[] { num2 }));
                    }
                    dyn.Set("playFootstepOnLand", 0f);
                }
            } else {
                if (self.Speed.Y < 0f) {
                    int num3 = 4;
                    if (self.DashAttacking && Math.Abs(self.Speed.X) < 0.01f) {
                        num3 = 5;
                    }
                    if (self.Speed.X <= 0.01f) {
                        for (int j = 1; j <= num3; j++) {
                            if (!self.CollideCheck<Solid>(self.Position + new Vector2(-j, -1f))) {
                                self.Position += new Vector2(-j, -1f);
                                return true;
                            }
                        }
                    }
                    if (self.Speed.X >= -0.01f) {
                        for (int k = 1; k <= num3; k++) {
                            if (!self.CollideCheck<Solid>(self.Position + new Vector2(k, -1f))) {
                                self.Position += new Vector2(k, -1f);
                                return true;
                            }
                        }
                    }
                    if (dyn.Get<float>("varJumpTimer") < 0.15f) {
                        dyn.Set("varJumpTimer", 0f);
                    }
                }
                if (canEnterDreamBlock && (bool) VivHelper.player_DreamDashCheck.Invoke(self, new object[] { Vector2.UnitY * Math.Sign(self.Speed.Y) })) {
                    self.StateMachine.State = 9;
                    dyn.Set("dashAttackTimer", 0f);
                    dyn.Set("gliderBoostTimer", 0f);
                    return true;
                }
            }
            return false;
        }

        private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
            VivHelperModule.Session.CurrentBooster?.PlayerDied(self);
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public static MethodInfo rdU = typeof(Player).GetMethod("RedDashUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    [Tracked(true)]
    public abstract class CustomBooster : Entity {
        protected Coroutine dashRoutine;
        public EntityID ID;
        public const float timerStart = 0.2f;

        public static Vector2 CorrectDashPrecision(Vector2 dir) {
            if (dir.X != 0f && Math.Abs(dir.X) < 0.01f) {
                dir.X = 0f;
                dir.Y = Math.Sign(dir.Y);
            } else if (dir.Y != 0f && Math.Abs(dir.Y) < 0.01f) {
                dir.Y = 0f;
                dir.X = Math.Sign(dir.X);
            }
            return dir;
        }
        public bool BoostingPlayer { get; protected set; }

        public Vector2 storedSpeed;
        public CustomBooster(Vector2 position) : base(position) { }

        public virtual void PlayerBoosted(Player player) {
            BoostingPlayer = true;
            base.AddTag((int) Tags.Persistent | (int) Tags.TransitionUpdate);
        }

        public virtual void PlayerReleased() {
            BoostingPlayer = false;
            base.RemoveTag((int) Tags.Persistent | (int) Tags.TransitionUpdate);
        }

        public virtual void PlayerDied(Player player) {
            if (BoostingPlayer) {
                PlayerReleased();
                dashRoutine.Active = false;
                Tag = 0;
            }
        }

        public void ExitCustomDash() {
            if (VivHelperModule.Session.CurrentBooster.ID.Equals(this.ID)) {
                VivHelperModule.Session.CurrentBooster = null;
            }
        }
    }
}
