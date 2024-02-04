using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using VivHelper;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/RefillSpaceParticleMod")]
    public class Thingy : Solid {
        public static ParticleType P_Smash;

        public static ParticleType P_Sparks;

        private Sprite sprite;

        private Vector2 start;

        private float sink;

        private int health = 2;

        private float shakeCounter;

        private Vector2 bounceDir;

        private Wiggler bounce;

        private Shaker shaker;

        private bool makeSparks;

        private bool smashParticles;

        private Coroutine pulseRoutine;

        private SoundSource firstHitSfx;

        private bool spikesLeft;

        private bool spikesRight;

        private bool spikesUp;

        private bool spikesDown;

        private bool[] set;

        public Thingy(Vector2 position)
            : base(position, 32f, 32f, safe: true) {
            base.Depth = -7000;
            SurfaceSoundIndex = 9;
            start = Position;
            sprite = VivHelperModule.spriteBank.Create("particlebox");
            sprite.CenterOrigin();
            Sprite obj = sprite;
            obj.OnLastFrame = (Action<string>) Delegate.Combine(obj.OnLastFrame, (Action<string>) delegate (string anim) {
                if (anim == "break") {
                    Visible = false;
                } else if (anim == "open") {
                    makeSparks = true;
                }
            });
            sprite.Position = new Vector2(base.Width, base.Height) / 2f;
            Add(sprite);
            bounce = Wiggler.Create(1f, 0.5f);
            bounce.StartZero = false;
            Add(bounce);
            Add(shaker = new Shaker(on: false));
            OnDashCollide = Dashed;
        }

        public Thingy(EntityData e, Vector2 levelOffset)
            : this(e.Position + levelOffset) {
            set = new bool[4];
            set[0] = e.Bool("Normal", true);
            set[1] = e.Bool("Decreased", true);
            set[2] = e.Bool("Minimal", true);
            set[3] = e.Bool("Minimal", true);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            spikesUp = CollideCheck<Spikes>(Position - Vector2.UnitY);
            spikesDown = CollideCheck<Spikes>(Position + Vector2.UnitY);
            spikesLeft = CollideCheck<Spikes>(Position - Vector2.UnitX);
            spikesRight = CollideCheck<Spikes>(Position + Vector2.UnitX);
        }

        public DashCollisionResults Dashed(Player player, Vector2 dir) {
            if (!SaveData.Instance.Assists.Invincible) {
                if (dir == Vector2.UnitX && spikesLeft) {
                    return DashCollisionResults.NormalCollision;
                }
                if (dir == -Vector2.UnitX && spikesRight) {
                    return DashCollisionResults.NormalCollision;
                }
                if (dir == Vector2.UnitY && spikesUp) {
                    return DashCollisionResults.NormalCollision;
                }
                if (dir == -Vector2.UnitY && spikesDown) {
                    return DashCollisionResults.NormalCollision;
                }
            }
            (base.Scene as Level).DirectionalShake(dir);
            sprite.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);
            Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);
            Celeste.Celeste.Freeze(0.2f);
            player.RefillDash();
            VivHelperModule.Settings.DecreaseParticles = (VivHelperModuleSettings.ColorRefillType) (((int) VivHelperModule.Settings.DecreaseParticles + 1) % 4);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            SmashParticles(dir.Perpendicular());
            SmashParticles(-dir.Perpendicular());
            return DashCollisionResults.Rebound;
        }

        private void SmashParticles(Vector2 dir) {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            int num;
            if (dir == Vector2.UnitX) {
                direction = 0f;
                position = base.CenterRight - Vector2.UnitX * 12f;
                positionRange = Vector2.UnitY * (base.Height - 6f) * 0.5f;
                num = (int) (base.Height / 8f) * 4;
            } else if (dir == -Vector2.UnitX) {
                direction = Consts.PI;
                position = base.CenterLeft + Vector2.UnitX * 12f;
                positionRange = Vector2.UnitY * (base.Height - 6f) * 0.5f;
                num = (int) (base.Height / 8f) * 4;
            } else if (dir == Vector2.UnitY) {
                direction = Consts.PIover2;
                position = base.BottomCenter - Vector2.UnitY * 12f;
                positionRange = Vector2.UnitX * (base.Width - 6f) * 0.5f;
                num = (int) (base.Width / 8f) * 4;
            } else {
                direction = -Consts.PIover2;
                position = base.TopCenter + Vector2.UnitY * 12f;
                positionRange = Vector2.UnitX * (base.Width - 6f) * 0.5f;
                num = (int) (base.Width / 8f) * 4;
            }
            num += 2;
            SceneAs<Level>().Particles.Emit(LightningBreakerBox.P_Smash, num, position, positionRange, direction);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

        }

        public override void Update() {
            base.Update();
            if (Collidable) {
                bool flag = HasPlayerRider();
                sink = Calc.Approach(sink, flag ? 1 : 0, 2f * Engine.DeltaTime);
                Vector2 vector = start;
                vector.Y += sink * 6f;
                vector += bounce.Value * bounceDir * 12f;
                MoveToX(vector.X);
                MoveToY(vector.Y);
                if (smashParticles) {
                    smashParticles = false;
                    SmashParticles(bounceDir.Perpendicular());
                    SmashParticles(-bounceDir.Perpendicular());
                }
            }
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, Engine.DeltaTime * 4f);
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, Engine.DeltaTime * 4f);
            LiftSpeed = Vector2.Zero;
        }

        public override void Render() {
            Vector2 position = sprite.Position;
            sprite.Position += shaker.Value;
            base.Render();
            sprite.Position = position;
        }

        private void Pulse() {
            pulseRoutine = new Coroutine(Lightning.PulseRoutine(SceneAs<Level>()));
            Add(pulseRoutine);
        }

    }
}
