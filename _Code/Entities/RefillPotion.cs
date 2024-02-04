using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using static Celeste.TrackSpinner;
using System.Collections;
using VivHelper.Module__Extensions__Etc;
using Celeste.Mod;

namespace VivHelper.Entities {

    [CustomEntity("VivHelper/RefillPotion")]
    public class RefillPotion : Actor {


        [Pooled]
        private class Debris : Actor {

            private Image image;

            private float lifeTimer;

            private float alpha;

            private Vector2 speed;

            private Collision collideH;

            private Collision collideV;

            private int rotateSign;

            private float fadeLerp;

            private bool playSound = true;

            private bool dreaming;

            private SineWave dreamSine;

            public Debris()
                : base(Vector2.Zero) {
                base.Collider = new Hitbox(4f, 4f, -2f, -2f);
                base.Tag = Tags.Persistent;
                base.Depth = -20000;
                collideH = OnCollideH;
                collideV = OnCollideV;
                image = new Image(null);
                Add(image);
                Add(dreamSine = new SineWave(0.6f, 0f));
                dreamSine.Randomize();
            }

            public override void Added(Scene scene) {
                base.Added(scene);
                dreaming = SceneAs<Level>().Session.Dreaming;
            }

            public Debris Init(Vector2 pos, Vector2 from) {
                Position = pos;
                lifeTimer = Calc.Random.Range(0.6f, 2.6f);
                alpha = 1f;
                speed = Vector2.Zero;
                fadeLerp = 0f;
                rotateSign = Calc.Random.Choose(1, -1);
                image.Texture = Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("particles/VivHelper/debrisGlass"));
                image.CenterOrigin();
                image.Color = Color.White * alpha;
                image.Rotation = Calc.Random.NextAngle();
                image.Scale.X = Calc.Random.Range(0.5f, 1f);
                image.Scale.Y = Calc.Random.Range(0.5f, 1f);
                image.FlipX = Calc.Random.Chance(0.5f);
                image.FlipY = Calc.Random.Chance(0.5f);
                float length = Calc.Random.Range(30, 40);
                speed = (Position - from).SafeNormalize(length) * 1.25f;
                speed = speed.Rotate(Calc.Random.Range(Consts.PI / -12f, Consts.PI / 12f));
                return this;
            }

            private void OnCollideH(CollisionData data) {
                speed.X *= -0.8f;
            }

            private void OnCollideV(CollisionData data) {
                speed.Y *= -0.6f;
                if (speed.Y < 0f && speed.Y > -50f) {
                    speed.Y = 0f;
                }
            }


            public override void Update() {
                base.Update();
                image.Rotation += Math.Abs(speed.X) * (float) rotateSign * Engine.DeltaTime;
                if (fadeLerp < 1f) {
                    fadeLerp = Calc.Approach(fadeLerp, 1f, 2f * Engine.DeltaTime);
                }
                MoveH(speed.X * Engine.DeltaTime, collideH);
                MoveV(speed.Y * Engine.DeltaTime, collideV);
                if (dreaming) {
                    speed.X = Calc.Approach(speed.X, 0f, 50f * Engine.DeltaTime);
                    speed.Y = Calc.Approach(speed.Y, 6f * dreamSine.Value, 100f * Engine.DeltaTime);
                } else {
                    bool flag = OnGround();
                    speed.X = Calc.Approach(speed.X, 0f, (flag ? 50f : 20f) * Engine.DeltaTime);
                    if (!flag) {
                        speed.Y = Calc.Approach(speed.Y, 80f, 400f * Engine.DeltaTime);
                    }
                }
                if (lifeTimer > 0f) {
                    lifeTimer -= Engine.DeltaTime;
                } else if (alpha > 0f) {
                    alpha -= 2f * Engine.DeltaTime;
                    if (alpha <= 0f) {
                        RemoveSelf();
                    }
                }
                image.Color = Color.Lerp(Color.White, Color.Gray, fadeLerp) * alpha;
            }
        }



        public static ParticleType P_Gas = new ParticleType(ParticleTypes.VentDust) {
            Color = Calc.HexToColor("a5fff7")
        };
        public static ParticleType P_Gas_Two = new ParticleType(ParticleTypes.VentDust) {
            Color = Calc.HexToColor("FFA5AA")
        };

        public static bool CanAttemptPickup(Player player) {
            if (player?.Dead ?? true)
                return false;
            if (player.Holding != null)
                return false;
            if (!Input.GrabCheck || (bool) VivHelper.player_IsTired.Invoke(player, Array.Empty<object>()))
                return false;
            if (player.StateMachine.State == Player.StNormal || player.StateMachine.State == Player.StLaunch) {
                return !player.Ducking;
            } else if (player.DashAttacking) {
                return player.DashDir != Vector2.Zero && player.CanUnDuck;
            }
            return false;
        }


        public Holdable hold;
        public Vector2 Speed;
        public bool two;
        public Sprite sprite;
        public bool shatterOnGround;
        private bool floating;
        public bool createEntity;
        public bool notPickedUp;
        public bool useWhenUnable;

        private const int shatterDashTime = 4;

        private Vector2 prevLiftSpeed;
        private bool shattering;
        private float noGravityTimer;
        private Collision onCollideH;
        private Collision onCollideV;
        private ParticleType gas;
        private int shatterDashTimer = shatterDashTime;

        private Hitbox solidCollider;
        private Circle dashCollider; // Operates as the basis for the alternate "PlayerCollider" framework
        private Circle dashCollider2;
        

        private float hardVerticalHitSoundCooldown;

        public RefillPotion(EntityData data, Vector2 offset) : base(data.Position + offset) {
            two = data.Bool("twoDash", false);
            gas = two ? P_Gas_Two : P_Gas;
            shatterOnGround = data.Bool("shatterOnGround", false);
            floating = data.Bool("floating", false);
            useWhenUnable = data.Bool("useAlways", false);
            solidCollider = new Hitbox(8f, 10f, -4f, -10f);
            dashCollider = new Circle(7f, 0f, -8f);
            dashCollider2 = new Circle(11f, 0f, -8f);
            Collider = solidCollider;
            Add(hold = new Holdable(0.1f));
            hold.PickupCollider = new Hitbox(18f, 22f, -9f, -16f);
            hold.SlowFall = false;
            hold.SlowRun = data.Bool("heavy", false);
            hold.OnPickup = OnPickup;
            hold.OnRelease = OnRelease;
            hold.SpeedGetter = () => Speed;

            onCollideH = OnCollideH;
            onCollideV = OnCollideV;

            LiftSpeedGraceTime = 0.1f;
            sprite = new Sprite(GFX.Game, "VivHelper/Potions/");
            sprite.Add("idle", (two ? "PotRefillTwo" : "PotRefill"), 0.1f);
            sprite.Play("idle");
            sprite.JustifyOrigin(0.5f, 1f);
            Add(sprite);
            notPickedUp = true;
            Depth = -5;

        }
        public override void Update() {

            base.Update();
            if (base.Scene.OnInterval(0.2f)) {
                SceneAs<Level>().Particles.Emit(gas, 1, base.Center + Vector2.UnitY * -9f, new Vector2(2f, 4f), -Consts.PIover2 - 0.4f + Calc.Random.NextFloat(0.8f) );
            }
            if (shattering)
                return;
            DashCollisionCode();
            if (hold.IsHeld) {
                prevLiftSpeed = Vector2.Zero;
            } else {
                if (OnGround()) {
                    float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = base.LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero) {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f) {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f) {
                            noGravityTimer = 0.15f;
                        }
                    } else {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f) {
                            Speed.Y = 0f;
                        }
                    }
                } else if (hold.ShouldHaveGravity) {
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f) {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f) {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f) {
                        noGravityTimer -= Engine.DeltaTime;
                    } else {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            }
        }

        public void DashCollisionCode() { // shattering is already checked for
            Collider = dashCollider2;
            if(this.CollideCheck(out Player player)) {
                if(shatterDashTimer == shatterDashTime) {
                    Collider = dashCollider;
                    if(Collide.Check(this, player) && !hold.IsHeld && player.DashAttacking && !CanAttemptPickup(player)) {
                        shatterDashTimer--;
                    }
                } else if (!hold.IsHeld && player.DashAttacking && !CanAttemptPickup(player)) {
                    shatterDashTimer--;
                } else {
                    shatterDashTimer = shatterDashTime;
                }
            } else {
                shatterDashTimer = shatterDashTime;
            }
            Collider = solidCollider;
            if (shatterDashTimer < 1) {
                if ((player.UseRefill(two) || useWhenUnable)) {
                    shattering = true;
                    Collidable = false;
                    Shatter(player);
                } else {
                    shatterDashTimer = 1;
                }
            }
        }

        private Vector2 PlatformAdd(int num) {
            return new Vector2(-12 + num, -5 + (int) Math.Round(Math.Sin(base.Scene.TimeActive + (float) num * 0.2f) * 1.7999999523162842));
        }

        private Color PlatformColor(int num) {
            if (num <= 1 || num >= 22) {
                return Color.White * 0.4f;
            }
            return Color.White * 0.8f;
        }
        private void OnPickup() {
            if (floating) {
                for (int i = 0; i < 24; i++) {
                    SceneAs<Level>().Particles.Emit(Glider.P_Platform, Position + PlatformAdd(i), PlatformColor(i));
                }
            }
            AddTag(Tags.Persistent);
            AllowPushing = false;

        }

        private void OnRelease(Vector2 force) {
            RemoveTag(Tags.Persistent);
            AllowPushing = true;
            if (force.X != 0f && force.Y == 0f) {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero) {
                noGravityTimer = 0.1f;
            }
        }

        public void Shatter(Player player, bool createEntity = false) {
            VivHelperAPI.AudioPlayWithMuteControl(this, two ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);

            if (hold.IsHeld) {
                Vector2 speed2 = hold.Holder.Speed;
                hold.Holder.Drop();
                Speed = speed2 * 0.333f;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            Add(new Coroutine(RefillRoutine(player)));
        }
        private IEnumerator RefillRoutine(Player player) {
            if(player is not null)
                Celeste.Celeste.Freeze(0.05f);
            yield return null;
            if (player is not null)
                SceneAs<Level>().Shake();
            sprite.Visible = false;
            float num = player?.DashDir.Angle() ?? -Consts.PIover2;
            for (int i = 0; i < 4; i++) {
                Vector2 v = Vector2.UnitX.Rotate(Consts.TAU / (i + 1));
                Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + Vector2.UnitY * -4 + v * 2, Position + Vector2.UnitY * -4));
            }
            SlashFx.Burst(Position, num);
            RemoveSelf();
        }

        private void OnCollideH(CollisionData data) {
            if (data.Hit is DashSwitch) {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            if (Math.Abs(Speed.X) > 100f) {
                ImpactParticles(data.Direction);
            }
            Speed.X *= -0.4f;
        }
        private void OnCollideV(CollisionData data) {
            if (data.Hit is DashSwitch) {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0f) {
                if (hardVerticalHitSoundCooldown <= 0f) {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                } else {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
                }
            }
            if (Speed.Y > 160f) {
                ImpactParticles(data.Direction);
            }
            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch)) {
                Speed.Y *= -0.6f;
            } else {
                Speed.Y = 0f;
            }
        }

        public override void DebugRender(Camera camera) {
            base.DebugRender(camera);
            Collider = dashCollider;
            Collider.Render(camera, Color.HotPink * 0.8f);
            Collider = dashCollider2;
            Collider.Render(camera, Color.HotPink * 0.6f);
            Collider = solidCollider;
        }

        private void ImpactParticles(Vector2 dir) {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f) {
                direction = Consts.PI;
                position = new Vector2(base.Right, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if (dir.X < 0f) {
                direction = 0f;
                position = new Vector2(base.Left, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if (dir.Y > 0f) {
                direction = -Consts.PIover2;
                position = new Vector2(base.X, base.Bottom);
                positionRange = Vector2.UnitX * 6f;
            } else {
                direction = Consts.PIover2;
                position = new Vector2(base.X, base.Top);
                positionRange = Vector2.UnitX * 6f;
            }
            SceneAs<Level>().Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        public override bool IsRiding(Solid solid) {
            if (Speed.Y == 0f) {
                return base.IsRiding(solid);
            }
            return false;
        }

        protected override void OnSquish(CollisionData data) {
            if (!TrySquishWiggle(data, 3, 3) && !SaveData.Instance.Assists.Invincible) {
                Shatter(null, false);
            }
        }
    }
}
