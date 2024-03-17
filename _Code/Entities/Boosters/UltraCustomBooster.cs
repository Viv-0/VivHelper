using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using Microsoft.Xna.Framework;
using MonoMod;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;
using VivHelper.Entities.Boosters;
using VivHelper;
using Celeste.Mod;
using VivHelper.Module__Extensions__Etc;

namespace VivHelper.Entities.Boosters {
    public static class UltraCustomDash {


        //Current expected values is 6: AnonHelper CloudRefill, AnonHelper JellyRefill, CherryHelper ShadowDash, ChronoHelper ShatterDash, Crystalline TimeCrystalDash, Crystalline StarPower Dash
        public static Dictionary<string, CustomDashActions> customDashSpecialHandlers = new Dictionary<string, CustomDashActions>(6);

        internal static bool FragileCheck = false;
        internal static Vector2 beforeDashSpeed; //Just incase we need it, this is inherently faster than retrieving from DynData.

        // This duplicates the behavior of DashBegin but with DynData. This *shouldn't* be problematic given that the player DynamicData has most likely already been stored via Boost.
        public static void CDashBegin(Player player) {
            DynamicData dyn = DynamicData.For(player);
            CustomDashStateCh customDashState = VivHelperModule.Session.customDashState;
            if (VivHelperModule.Session.CurrentBooster != null && VivHelperModule.Session.CurrentBooster is UltraCustomBooster booster)
                customDashState = booster.customDashState;
            else if (customDashState == null) {
                Logger.Log("VivHelper", "CustomDash failed!");
                player.StateMachine.State = 0;
                return;
            }
            bool onGround = dyn.Get<bool>("onGround");
            bool demoDashed = dyn.Get<bool>("demoDashed");
            dyn.Set("calledDashEvents", true);
            dyn.Set("dashStartedOnGround", onGround);
            dyn.Set("launched", false);
            dyn.Set("canCurveDash", customDashState.SuperDashSteerSpeed > 0f);
            if (customDashState.FreezeFrames > 0) {
                Celeste.Celeste.Freeze(customDashState.FreezeFrames);
            }
            dyn.Set("dashCooldownTimer", customDashState.dashCooldownTime);
            dyn.Set("dashRefillCooldownTimer", customDashState.dashRefillCooldownTime);
            dyn.Set("StartedDashing", true);
            dyn.Set("wallSlideTimer", 1.2f);
            dyn.Set("dashTrailTimer", 0.1f);
            dyn.Set("dashTrailCounter", 1);
            if (!SaveData.Instance.Assists.DashAssist) {
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            float dashAttackTimer = 0.15f; //Default for 0 dash duration.
            float gliderBoostTimer = 0.4f;
            if (customDashState.DashDuration < 0) {
                dashAttackTimer = float.MaxValue; //a nice fix for HeldDash
                gliderBoostTimer = float.MaxValue; //These are just set so that it's not 0 on begin. Technically, if this is the case it's set to some value every frame it's occurring.
            } else {
                dashAttackTimer += customDashState.DashDuration;
                gliderBoostTimer += customDashState.DashDuration;
            }
            dyn.Set("dashAttackTimer", dashAttackTimer);
            dyn.Set("gliderBoostTimer", gliderBoostTimer);
            beforeDashSpeed = player.Speed;
            player.Speed = Vector2.Zero;
            player.DashDir = Vector2.Zero;
            if (!onGround && player.Ducking && player.CanUnDuck) {
                player.Ducking = false;
            } else if (!player.Ducking && (demoDashed || Input.MoveY.Value == 1)) {
                player.Ducking = true;
            }
            //Player::DashAssistInit
            if (SaveData.Instance.Assists.DashAssist && !demoDashed) {
                Input.LastAim = Vector2.UnitX * (float) player.Facing;
                Engine.DashAssistFreeze = true;
                Engine.DashAssistFreezePress = false;
                PlayerDashAssist playerDashAssist = player.Scene.Tracker.GetEntity<PlayerDashAssist>();
                if (playerDashAssist == null) {
                    player.Scene.Add(playerDashAssist = new PlayerDashAssist());
                }
                playerDashAssist.Direction = Input.GetAimVector(player.Facing).Angle();
                playerDashAssist.Scale = 0f;
                playerDashAssist.Offset = (VivHelperModule.Session.CurrentBooster == null) ? Vector2.Zero : new Vector2(0f, -4f);
            }
        }
        public static int CDashUpdate(Player player) {
            DynamicData dyn = DynamicData.For(player);
            CustomDashStateCh customDashState = VivHelperModule.Session.customDashState;
            if (VivHelperModule.Session.CurrentBooster != null && VivHelperModule.Session.CurrentBooster is UltraCustomBooster booster) {
                customDashState = booster.customDashState;
            }
            if(customDashState.CanDashExit && player.CanDash) {
                return player.StartDash();
            }
            dyn.Set("StartedDashing", false);
            float dashTrailTimer = dyn.Get<float>("dashTrailTimer");
            float dashCounter = dyn.Get<int>("dashTrailCounter");
            if (dashTrailTimer > 0f && dashCounter > 0) {
                dashTrailTimer -= Engine.DeltaTime;
                if (dashTrailTimer <= 0f) {
                    Vector2 scale = new Vector2(Math.Abs(player.Sprite.Scale.X) * (float) player.Facing, player.Sprite.Scale.Y);
                    TrailManager.Add(player, scale, player.GetCurrentTrailColor());
                    dashTrailTimer = 0.1f;
                }
            }
            dyn.Set("dashTrailTimer", dashTrailTimer);
            if (customDashState.SuperDashSteerSpeed > 0f) {
                if (Input.Aim.Value != Vector2.Zero && player.Speed != Vector2.Zero) {
                    Vector2 aimVector = Input.GetAimVector();
                    aimVector = CustomBooster.CorrectDashPrecision(aimVector);
                    float num = Vector2.Dot(aimVector, player.Speed.SafeNormalize());
                    if (num >= -0.1f && num < 0.99f) {
                        float steerSpeed = customDashState.SuperDashSteerSpeed * Calc.DegToRad / Engine.DeltaTime; //From deg/f to rad/s
                        player.Speed = player.Speed.RotateTowards(aimVector.Angle(), steerSpeed * Engine.DeltaTime);
                        player.DashDir = player.Speed.SafeNormalize();
                    }
                }
                //We don't want this superdash code to occur that occurs in normal dashes here.
            }
            player.DashDir = CustomBooster.CorrectDashPrecision(player.DashDir);

            if (Math.Abs(player.Speed.X) < 0.005)
                player.Speed.X = 0;
            if (Math.Abs(player.Speed.Y) < 0.005)
                player.Speed.Y = 0;
            if (player.Holding == null && player.DashDir != Vector2.Zero && Input.GrabCheck && !dyn.Get<bool>("IsTired") && player.CanUnDuck) {
                foreach (Holdable component in player.Scene.Tracker.GetComponents<Holdable>()) {
                    if (component.Check(player) && (bool) VivHelper.player_Pickup.Invoke(player, new object[] { component })) {
                        return Player.StPickup;
                    }
                }
            }
            if (Math.Abs(player.DashDir.Y) < 0.1f) {
                foreach (JumpThru entity in player.Scene.Tracker.GetEntities<JumpThru>()) {
                    if (player.CollideCheck(entity) && player.Bottom - entity.Top <= 6f && !(bool) VivHelper.player_DashCorrectCheck.Invoke(player, new object[] { Vector2.UnitY * (entity.Top - player.Bottom)})) {
                        player.MoveVExact((int) (entity.Top - player.Bottom));
                    }
                }
                if (player.CanUnDuck && Input.Jump.Pressed && dyn.Get<float>("jumpGraceTimer") > 0f) {
                    VivHelper.player_SuperJump.Invoke(player, Array.Empty<object>());
                    return 0;
                }
            }
            //Optimized Dash Update behaviors
            if (Input.Jump.Pressed && player.CanUnDuck) {
                if ((bool) VivHelper.player_WallJumpCheck.Invoke(player, VivHelper.oneOne)) {
                    if (Math.Abs(player.DashDir.X) <= 0.2f && player.DashDir.Y <= -0.75f) {
                        VivHelper.player_SuperWallJump.Invoke(player, VivHelper.negOne);
                    } else if (player.Facing == Facings.Right && Input.GrabCheck && player.Stamina > 0f && player.Holding == null && !ClimbBlocker.Check(player.Scene, player, player.Position + Vector2.UnitX * 3f)) {
                        VivHelper.player_ClimbJump.Invoke(player, Array.Empty<object>());
                    } else {
                        VivHelper.player_WallJump.Invoke(player, VivHelper.negOne);
                    }
                    return 0;
                }
                if ((bool) VivHelper.player_WallJumpCheck.Invoke(player, VivHelper.negOne)) {
                    if (Math.Abs(player.DashDir.X) <= 0.2f && player.DashDir.Y <= -0.75f) {
                        VivHelper.player_SuperWallJump.Invoke(player, VivHelper.oneOne);
                    } else if (player.Facing == Facings.Left && Input.GrabCheck && player.Stamina > 0f && player.Holding == null && !ClimbBlocker.Check(player.Scene, player, player.Position - Vector2.UnitX * 3f)) {
                        VivHelper.player_ClimbJump.Invoke(player, Array.Empty<object>());
                    } else {
                        VivHelper.player_WallJump.Invoke(player, VivHelper.oneOne);
                    }
                    return 0;
                }
            }
            return VivHelperModule.CustomDashState;
        }

        public static IEnumerator CDashRoutine(Player player) {
            DynamicData dyn = DynamicData.For(player);
            var customDashState = VivHelperModule.Session.customDashState;
            if (VivHelperModule.Session.CurrentBooster is UltraCustomBooster booster) {
                customDashState = booster.customDashState;
                yield return null;
                booster.PlayerBoosted(player, player.DashDir);
            } else {
                yield return null;
            }
            if (SaveData.Instance.Assists.DashAssist) {
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            Level level = player.Scene as Level;
            level.Displacement.AddBurst(player.Center, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
            Vector2 value = dyn.Get<Vector2>("lastAim");
            value = CustomBooster.CorrectDashPrecision(value);
            switch (customDashState.ForceDashDirection) {
                case DashAngleState.ForceDashDirection:
                    value = Calc.AngleToVector(customDashState.AngleOffset, value.Length());
                    break;
                case DashAngleState.OffsetByAngle:
                    value = value.Rotate(customDashState.AngleOffset);
                    break;
                case DashAngleState.ForcePreviousSpeedAngle:
                    value = value.RotateTowards(beforeDashSpeed.Angle(), 6.3f);
                    break;
            }
            player.DashDir = value;
            Vector2 speed = value * customDashState.DashSpeed;
            switch (customDashState.SpeedMagnitudeCheck) {
                case SpeedMagnitudeCheck.Default:
                    break;
                case SpeedMagnitudeCheck.MomentumRetain:
                    if (Math.Sign(beforeDashSpeed.X) == Math.Sign(speed.X) && Math.Abs(beforeDashSpeed.X) > Math.Abs(speed.X)) {
                        speed.X = beforeDashSpeed.X;
                    }
                    break;
            }
            player.Speed = speed;

            if (player.CollideCheck<Water>()) {
                player.Speed *= 0.75f;
            }
            dyn.Set("gliderBoostDir", value);
            level.DirectionalShake(player.DashDir, 0.2f);
            if (player.DashDir.X != 0f) {
                player.Facing = (Facings) Math.Sign(player.DashDir.X);
            }
            //Skip the CallDashEvents code because this is a booster-based dash.
            if (player.StateMachine.PreviousState == Player.StStarFly) {
                level.Particles.Emit(FlyFeather.P_Boost, 12, player.Center, Vector2.One * 4f, (-value).Angle());
            }
            if (!customDashState.HeldDash && dyn.Get<bool>("onGround") && player.DashDir.X != 0f && player.DashDir.Y > 0f && player.Speed.Y > 0f && (!player.Inventory.DreamDash || !player.CollideCheck<DreamBlock>(player.Position + Vector2.UnitY))) {
                player.DashDir.X = Math.Sign(player.DashDir.X);
                player.DashDir.Y = 0f;
                player.Speed.Y = 0f;
                player.Speed.X *= 1.2f;
                player.Ducking = true;
            }
            SlashFx.Burst(player.Center, player.DashDir.Angle());
            Vector2 scale = new Vector2(Math.Abs(player.Sprite.Scale.X) * (float) player.Facing, player.Sprite.Scale.Y);
            TrailManager.Add(player, scale, player.GetCurrentTrailColor());
            if (player.DashDir.X != 0f && Input.GrabCheck) {
                SwapBlock swapBlock = player.CollideFirst<SwapBlock>(player.Position + Vector2.UnitX * Math.Sign(player.DashDir.X));
                if (swapBlock != null && swapBlock.Direction.X == (float) Math.Sign(player.DashDir.X)) {
                    player.StateMachine.State = 1;
                    player.Speed = Vector2.Zero;
                    yield break;
                }
            }
            Vector2 swapCancel = Vector2.One;
            foreach (SwapBlock entity in player.Scene.Tracker.GetEntities<SwapBlock>()) {
                if (player.CollideCheck(entity, player.Position + Vector2.UnitY) && entity != null && entity.Swapping) {
                    if (player.DashDir.X != 0f && entity.Direction.X == (float) Math.Sign(player.DashDir.X)) {
                        player.Speed.X = (swapCancel.X = 0f);
                    }
                    if (player.DashDir.Y != 0f && entity.Direction.Y == (float) Math.Sign(player.DashDir.Y)) {
                        player.Speed.Y = (swapCancel.Y = 0f);
                    }
                }
            }

            float t = customDashState.DashDuration;
            var dashDurationOrig = t; //DashDuration
            if (t < 0f)
                t = float.MaxValue;
            else
                t += Engine.DeltaTime;
            while (t > 0f &&
                  (customDashState.HeldDash ? Input.Dash.Check || Input.CrouchDash.Check :
                  (!customDashState.CanDashExit || dyn.Get<float>("dashCooldownTimer") > 0f || player.Dashes < 1 || (!Input.DashPressed && !Input.CrouchDashPressed)))) //Handles everything nicely
            {
                if (customDashState.IsRedDashEsque) {
                    dyn.Set("gliderBoostTimer", 0.30f);
                    dyn.Set("dashAttackTimer", 0.15f);
                }
                yield return null;
                t -= Engine.DeltaTime;
            }
            scale = new Vector2(Math.Abs(player.Sprite.Scale.X) * (float) player.Facing, player.Sprite.Scale.Y);
            TrailManager.Add(player, scale, player.GetCurrentTrailColor());
            dyn.Set("dashTrailCounter", 0);
            player.AutoJump = true;
            player.AutoJumpTimer = 0f;
            if (CustomDashStateCh.DashEndControl(customDashState.DashEndType, player.DashDir)) {
                player.Speed = player.DashDir * (customDashState.DashSpeed * customDashState.DashEndMultiplier);
                player.Speed.X *= swapCancel.X;
                player.Speed.Y *= swapCancel.Y;
            }
            if (player.Speed.Y < 0f) {
                player.Speed.Y *= 0.75f;
            }
            player.StateMachine.State = 0;
        }

        public static void CDashEnd(Player player) {
            VivHelperModule.Session.CurrentBooster?.ExitCustomDash(); //Fixes a rather rare order issue.
            VivHelper.player_demoDashed.SetValue(player, false);
        }
    }

    /// <summary>
    /// This is the class that acts as StBoost, *not* the custom dash itself. CustomDash is handled with another statehandler.
    /// </summary>
    public static class UltraCustomBoost //Tied to VivHelperModule.CustomBoostState
    {
        public static UltraCustomBooster StoredBooster; // This is only used to ensure passage to CustomBoost::Begin if exiting from one booster to a second.
        
        public static void CustomBoost(this Player self, UltraCustomBooster booster) {
            DynamicData dyn = DynamicData.For(self);
            StoredBooster = booster;
            self.StateMachine.State = VivHelperModule.CustomBoostState;
            self.Speed = Vector2.Zero;
            dyn.Set("boostTarget", booster.Center);
            if (booster.customDashState.DisableRetentionTech) {
                dyn.Set("wallSpeedRetained", Vector2.Zero);
                dyn.Set("wallSpeedRetentionTimer", 0f);
            }
        }

        public static void Begin(Player player) {
            StoredBooster.storedSpeed = player.Speed;
            VivHelperModule.Session.CurrentBooster = StoredBooster;
            var customDashState = StoredBooster?.customDashState ?? VivHelperModule.Session.customDashState;
            if (player.Holding != null && customDashState.DropHoldable) {
                player.Drop();
            }
            customDashState.HandleParameters(CustomDashActionTypes.OnCustomBoostBegin, player);
            if (customDashState.RefillTiming == RefillTiming.BoostBegin) {
                customDashState.HandleRefill(player);
            }
            StoredBooster = null; // fixes a savestate misalignment bug
        }

        public static int Update(Player player) {
            var booster = VivHelperModule.Session.CurrentBooster as UltraCustomBooster;
            var customDashState = booster?.customDashState ?? VivHelperModule.Session.customDashState;
            if (customDashState != null) {
                GravityHelperAPI.BeginOverride?.Invoke();
                Vector2 vector = Input.Aim.Value * 3f;
                Vector2 vector2 = Calc.Approach(player.ExactPosition, (Vector2)VivHelper.player_boostTarget.GetValue(player) - player.Collider.Center + vector, 80f * Engine.DeltaTime);
                player.MoveToX(vector2.X);
                player.MoveToY(vector2.Y);
                GravityHelperAPI.EndOverride?.Invoke();
                if (customDashState.FastBubbleState != FastBubbleState.NoFastBubble && (Input.Dash.Pressed || Input.CrouchDash.Pressed)) {
                    VivHelper.player_demoDashed.SetValue(player, Input.CrouchDashPressed);
                    if (!customDashState.HeldDash) {
                        Input.Dash.ConsumePress();
                        Input.CrouchDash.ConsumeBuffer();
                    }
                    return VivHelperModule.CustomDashState;
                }
                return VivHelperModule.CustomBoostState;
            }
            return VivHelperModule.CustomBoostState;
        }

        public static IEnumerator Coroutine(Player player) {
            UltraCustomBooster booster = null;
            while (VivHelperModule.Session.CurrentBooster == null || VivHelperModule.Session.CurrentBooster is not UltraCustomBooster)
                yield return null;
            booster = VivHelperModule.Session.CurrentBooster as UltraCustomBooster;
            var timer = booster.customDashState.FastBubbleTimer;
            if (timer > 0) {
                //For this case, we know that if no fastbubble, kill the player. Extra color fluff is implemented from FrostHelper yellow booster.
                if (booster.customDashState.FastBubbleState == FastBubbleState.FastBubbleOrKill) {
                    if (timer < 0.05f * Engine.TimeRate) {
                        yield return null;
                        booster.SetColor(booster.customDashState.FlashColor);
                    } else {
                        yield return timer / 6;
                        booster.SetColor(booster.customDashState.FlashColor);
                        yield return timer / 3;
                        booster.SetColor(Color.White);
                        yield return timer / 6;
                        booster.SetColor(booster.customDashState.FlashColor);
                        yield return timer / 3;
                        player.Die(booster.customDashState.CustomDashDir(player.DashDir, player.Speed)); //State Swap handled by the Update
                        yield break;
                    }
                } else {
                    yield return timer;
                    player.StateMachine.State = VivHelperModule.CustomDashState;
                }
            }
        }

        public static void End(Player player) {
            Vector2 boostTarget = (Vector2)VivHelper.player_boostTarget.GetValue(player) - player.Collider.Center;
            if (VivHelperModule.Session.CurrentBooster != null && VivHelperModule.Session.CurrentBooster is UltraCustomBooster booster) {
                if (booster.color != Color.White)
                    booster.SetColor(Color.White);
                booster.customDashState.HandleParameters(CustomDashActionTypes.OnCustomBoostEnd, player);
                if (booster.customDashState.RefillTiming == RefillTiming.BoostEnd) {
                    booster.customDashState.HandleRefill(player);
                }
                //DashSpeed == null equivalent to Speed Retention Dash by its definition, however the speed is stored in Begin. This has some ramifications, but basically, we ignore if the speed *becomes* zero
                if (booster.customDashState.DisableRetentionTech) {
                    player.Speed = Vector2.Zero;
                }
                booster.cannotUseTimer = 0.2f / (booster.customDashState.DashSpeed / 240f);
                if (!booster.customDashState.LeaveWiggle) {
                    Vector2 v = (boostTarget - player.Collider.Center).Floor();
                    player.MoveToX(v.X);
                    player.MoveToY(v.Y);
                }
                if (booster.customDashState.HeldDash && (player.ExactPosition - boostTarget).LengthSquared() <= 18f) { //magic number, sqrt(18) was roughly max distance from the center we could get for this check, tested at 10000x,10000y (fpu differentials are possible)
                    if (player.StateMachine.State != VivHelperModule.CustomDashState) {
                        UltraCustomDash.CDashEnd(player);
                    }
                    return;
                }
            }
            Vector2 vector = boostTarget.Floor();
            player.MoveToX(vector.X);
            player.MoveToY(vector.Y);
            if (player.StateMachine.State != VivHelperModule.CustomDashState) {
                UltraCustomDash.CDashEnd(player);
            }
        }
    }

    [CustomEntity("VivHelper/UltimateCustomBooster")]
    [Tracked]
    public class UltraCustomBooster : CustomBooster {
        public static Vector2 playerOffset = new Vector2(0, -2);

        public ParticleType P_Appear;
        public ParticleType P_Burst;

        public CustomDashStateCh customDashState;
        public readonly Vector2 startPosition;

        public Color color => useComposite ? compositeSprite.Color : sprite.Color;

        public bool useComposite;
        public Sprite sprite;
        public CompositeSpritesheet compositeSprite;
        public Entity outline;
        private Wiggler wiggler;

        public float respawnTimer;
        public float cannotUseTimer;
        public SoundSource loopingSfx;
        private DashListener dashListener;

        public string audioEnter, audioExit, audioDash, audioMove, audioRespawn;
        public float RespawnTime;

        public void SetColor(Color color) {
            if (useComposite)
                compositeSprite.Color = color;
            else
                sprite.Color = color;
        }


        public UltraCustomBooster(EntityData _data, Vector2 offset, EntityID id) : base(_data.Position + offset) {
            ID = id;
            startPosition = Position;
            EntityData data = default(EntityData);
            if (_data.Values.TryGetValue("UsePreset", out object v) && v is string s && !string.IsNullOrWhiteSpace(s) && CustomDashStateCh.presetCustomBoosterStates.TryGetValue(s, out data))
                data = Extensions.ConjoinEntityData(_data, data);
            else
                data = _data;



            customDashState = new CustomDashStateCh {
                AngleOffset = -Calc.DegToRad * VivHelper.mod((data.Float("DashSpeed", 240) < 0 ? -180 : 0) + data.Float("AngleOffset", 0), 360), //This is done in this way to handle Angle modulus 360.
                CanDashExit = data.Bool("CanDashExit", true),
                DashDuration = data.Float("DashDuration", 0.15f),
                DashRefill = data.Int("DashRefillAmount", -1),
                DashRefillModifier = data.Bool("DashRefillType", false),
                DashSpeed = Math.Abs(data.Float("DashSpeed", 240)),
                DropHoldable = data.Bool("DropHoldableOnEntry", true),
                ExtraParameters = data.Attr("ExtraParameters"),
                ForceDashDirection = data.Enum<DashAngleState>("DashAngleState", DashAngleState.OffsetByAngle),
                FastBubbleState = data.Enum<FastBubbleState>("FastBubbleState", FastBubbleState.Normal),
                FastBubbleTimer = data.Float("FastBubbleTimer", 0.25f),
                HeldDash = data.Bool("HeldDash"),
                RefillTiming = data.Enum<RefillTiming>("RefillTiming", RefillTiming.BoostBegin),
                StamRefill = data.Float("StaminaRefillAmount", 110f),
                StamRefillModifier = data.Bool("StaminaRefillType", false),
                SpeedMagnitudeCheck = data.Bool("SpeedRetention", false) ? SpeedMagnitudeCheck.MomentumRetain : SpeedMagnitudeCheck.Default,
                SuperDashSteerSpeed = data.Float("SuperDashSteerSpeed", 0f),

                dashCooldownTime = data.Float("DashCooldownTime", 0.2f),
                dashRefillCooldownTime = data.Float("DashRefillCooldownTime", 0.1f),
                DashEndType = data.Int("DashEndFormat", -2),
                DashEndMultiplier = data.Float("DashEndMultiplier", 0.6666f),
                ImpactsObjectsAsDash = true
            };
            string t = data.Attr("DashIntoSolidEffect", "Normal");
            if (!Enum.TryParse<DashSolidContact>(t, out customDashState.DashSolidEffect)) {
                customDashState.DashSolidEffect = customDashState.DashDuration < 0 ? DashSolidContact.Ignore : DashSolidContact.Default;
            }
            useComposite = !data.Bool("UseSpritesFromXML", false);
            if (useComposite) {
                MTexture m = GFX.Game[data.NoEmptyStringReplace("SpriteReference", "VivHelper/boosters/hiCustomBooster")];
                compositeSprite = new CompositeSpritesheet(true, m, 32, 32);
                compositeSprite.Justify = new Vector2(0.5f, 0.5f);
                compositeSprite.AddLoop("loop", 0.1f, 0, 1, 2, 3, 4);
                compositeSprite.AddLoop("inside", 0.1f, 5, 6, 7, 8);
                compositeSprite.AddLoop("spin", 0.06f, 18, 19, 20, 21, 22, 23, 24, 25);
                compositeSprite.Add("pop", 0.08f, 9, 10, 11, 12, 13, 14, 15, 16, 17);
                string _cs = data.NoEmptyString("ColorSet");
                if (_cs != null)
                    compositeSprite.DefineColorSet(VivHelper.OldColorsFromString(_cs, ','));
                else {
                    compositeSprite.DefineColorSet(new List<Color>
                    {
                        data.Color("OutlineColor", Color.Black),
                        data.Color("ShineColor", Calc.HexToColor("8cf7cf")),
                        data.Color("BubbleColor", Calc.HexToColor("4acfc6")),
                        data.Color("LightBase", Calc.HexToColor("1c7856")),
                        data.Color("DarkBase", Calc.HexToColor("0e4a36")),
                        data.Color("LightCore", Calc.HexToColor("172b21")),
                        data.Color("DarkCore", Calc.HexToColor("0e1c15")),
                        data.Color("LightPoof", Color.White),
                        data.Color("DarkPoof", Calc.HexToColor("291c33"))
                    });
                }
                compositeSprite.CenterOrigin();
                Add(compositeSprite);
                compositeSprite.Play("loop");
            } else {
                Add(sprite = GFX.SpriteBank.Create(data.NoEmptyStringReplace("SpriteReference", "booster")));
            }
            P_Appear = new ParticleType(Booster.P_Appear) { Color = data.Color("AppearColor", useComposite ? compositeSprite.colorSet[3] : Calc.HexToColor("4acfc6")) };
            var c = data.Color("BurstColor", useComposite ? compositeSprite.colorSet[3] : Calc.HexToColor("2c956e"));
            P_Burst = new ParticleType(Booster.P_Burst) { Color = Color.Lerp(c, Color.Black, 0.15f) * 0.9f, Color2 = Color.Lerp(c, Color.White, 0.15f) * 0.9f, ColorMode = ParticleType.ColorModes.Choose };

            audioEnter = data.Attr("audioOnEnter", SFX.game_05_redbooster_enter);
            audioExit = data.Attr("audioOnExit", SFX.game_05_redbooster_end);
            audioDash = data.Attr("audioOnBoost", SFX.game_05_redbooster_dash);
            audioMove = data.Attr("audioWhileDashing", SFX.game_05_redbooster_move_loop);
            audioRespawn = data.Attr("audioOnRespawn", SFX.game_05_redbooster_reappear);

            base.Depth = -8500;
            base.Collider = new Circle(10f, 0f, 2f);
            Add(new PlayerCollider(OnPlayer));
            if (data.Bool("AddLight", true))
                Add(new VertexLight(Color.White, 1f, 16, 32));
            if (data.Bool("AddBloom", true))
                Add(new BloomPoint(0.1f, 16f));
            Add(dashRoutine = new Coroutine(removeOnComplete: false));
            Add(dashListener = new DashListener());
            Add(new MirrorReflection());
            Add(loopingSfx = new SoundSource());
            dashListener.OnDash = OnPlayerDashed;
            RespawnTime = Math.Max(0, data.Float("RespawnTime", 1f));
            Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                if(useComposite) compositeSprite.Scale = Vector2.One * (1f + f * 0.25f); else sprite.Scale = Vector2.One * (1f + f * 0.25f); 
            }));
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Image image = new Image(GFX.Game["objects/booster/outline"]);
            image.CenterOrigin();
            image.Color = Color.White * 0.75f;
            outline = new Entity(Position);
            outline.Depth = 8999;
            outline.Visible = false;
            outline.Add(image);
            scene.Add(outline);
        }
        private void AppearParticles() {
            if (P_Appear != null) {
                ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
                for (int i = 0; i < 360; i += 30) {
                    particlesBG.Emit(P_Appear, 1, base.Center, Vector2.One * 2f, i * Calc.DegToRad);
                }
            }
        }
        public void PlayerBoosted(Player player, Vector2 direction) {
            Audio.Play(audioDash, Position);
            if (customDashState.DashDuration < 0f || customDashState.DashDuration > 0.3f) {
                loopingSfx.Play(audioMove);
                loopingSfx.DisposeOnTransition = false;
            }
            BoostingPlayer = true;
            base.Tag = (int) Tags.Persistent | (int) Tags.TransitionUpdate;
            if (useComposite) {
                compositeSprite.Play("spin");
                compositeSprite.FlipX = player.Facing == Facings.Left;
            } else {
                sprite.Play("spin");
                sprite.FlipX = player.Facing == Facings.Left;
            }
            outline.Visible = true;
            dashRoutine.Replace(BoostRoutine(player));
        }

        private IEnumerator BoostRoutine(Player player) {
            while ((player.StateMachine.State == VivHelperModule.CustomDashState) && BoostingPlayer) {
                if (useComposite) {
                    compositeSprite.RenderPosition = player.Center + playerOffset;
                } else {
                    sprite.RenderPosition = player.Center + playerOffset;
                }
                loopingSfx.Position = player.Center + playerOffset - Position;
                loopingSfx.UpdateSfxPosition(); //Cheeky hack

                if (Scene.OnInterval(0.02f)) {
                    (Scene as Level).ParticlesBG.Emit(P_Burst, 2, player.Center + playerOffset, new Vector2(3f, 3f), player.DashDir.Angle());
                }
                yield return null;
            }
            if(BoostingPlayer)
                PlayerReleased();
            if (player.StateMachine.State == VivHelperModule.CustomBoostState) {
                if (useComposite)
                    compositeSprite.Visible = false;
                else
                    sprite.Visible = false;
            }
            while (SceneAs<Level>().Transitioning) {
                yield return null;
            }
            Tag = 0;
        }

        public void OnPlayerDashed(Vector2 direction) {
            if (BoostingPlayer) {
                BoostingPlayer = false;
            }
        }

        public override void PlayerReleased() {
            Audio.Play(audioExit, useComposite ? compositeSprite.RenderPosition : sprite.RenderPosition);
            if (useComposite)
                compositeSprite.Play("pop");
            else
                sprite.Play("pop");
            cannotUseTimer = 0f;
            respawnTimer = RespawnTime;
            BoostingPlayer = false;
            loopingSfx.Stop();
        }

        public void Respawn() {
            Position = startPosition;
            Audio.Play(audioRespawn, Position);
            if (useComposite) {
                compositeSprite.Position = Vector2.Zero;
                compositeSprite.Play("loop", true);
                compositeSprite.Visible = true;
            } else {
                sprite.Position = Vector2.Zero;
                sprite.Play("loop", true);
                sprite.Visible = true;
            }
            outline.Visible = false;
            AppearParticles();
        }

        private void OnPlayer(Player player) {
            if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer && player.StateMachine.State != VivHelperModule.CustomBoostState) {
                cannotUseTimer = float.MaxValue;
                player.CustomBoost(this);
                Audio.Play(audioEnter, Position);
                if (useComposite) {
                    compositeSprite.Play("inside");
                    compositeSprite.FlipX = player.Facing == Facings.Left;
                } else {
                    sprite.Play("inside");
                    sprite.FlipX = player.Facing == Facings.Left;
                }
                wiggler.Start();
            }
        }

        public override void Update() {
            base.Update();
            if (cannotUseTimer > 1000f && !BoostingPlayer) {
                cannotUseTimer = 0.45f;
            } else if (cannotUseTimer > 0f) {
                cannotUseTimer -= Engine.DeltaTime;
            }
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f) {
                    Respawn();
                }
            }
            if (!dashRoutine.Active && respawnTimer <= 0f) {
                Vector2 target = Vector2.Zero;
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null && CollideCheck(entity)) {
                    target = entity.Center + playerOffset - Position;
                }
            }
            if (useComposite) {
                if (!dashRoutine.Active && respawnTimer <= 0f) {
                    Vector2 target = Vector2.Zero;
                    Player entity = base.Scene.Tracker.GetEntity<Player>();
                    if (entity != null && CollideCheck(entity)) {
                        target = entity.Center + playerOffset - Position;
                    }
                    compositeSprite.Position = Calc.Approach(compositeSprite.Position, target, 80f * Engine.DeltaTime);
                }
                if (compositeSprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>()) {
                    compositeSprite.Play("loop");
                }
            } else {
                if (!dashRoutine.Active && respawnTimer <= 0f) {
                    Vector2 target = Vector2.Zero;
                    Player entity = base.Scene.Tracker.GetEntity<Player>();
                    if (entity != null && CollideCheck(entity)) {
                        target = entity.Center + playerOffset - Position;
                    }
                    sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
                }
                if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>()) {
                    sprite.Play("loop");
                }
            }
        }

        public override void Render() {
            if (useComposite) {
                Vector2 position = compositeSprite.Position;
                compositeSprite.Position = position.Floor();
                base.Render();
                this.customDashState.HandleParameterRender(compositeSprite.Position);
                compositeSprite.Position = position;
            } else {
                Vector2 position = sprite.Position;
                sprite.Position = position.Floor();
                if (sprite.CurrentAnimationID != "pop" && sprite.Visible) {
                    sprite.DrawOutline();
                }
                base.Render();
                this.customDashState.HandleParameterRender(sprite.Position);
                sprite.Position = position;
            }
        }

    }
}
