using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.VivHelper;
using MonoMod.Utils;
using System.Collections;
using VivHelper;

namespace VivHelper.Entities {
    public class SpeedPowerup {
        private static bool Store = false, Launch = false;

        public static void Load() {
            On.Celeste.Player.DashBegin += speedPowerBegin;
            On.Celeste.Player.DashEnd += speedPowerEnd;
            On.Celeste.Player.Die += resetSpeedPow;
        }

        public static void Unload() {
            On.Celeste.Player.DashBegin -= speedPowerBegin;
            On.Celeste.Player.DashEnd -= speedPowerEnd;
            On.Celeste.Player.Die -= resetSpeedPow;
        }

        private static PlayerDeadBody resetSpeedPow(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true) {
            VivHelperModule.Session.ResetSpeedPowerup();
            PlayerDeadBody pdb = orig.Invoke(self, direction, evenIfInvincible, registerDeathInStats);
            return pdb;

        }

        private static void speedPowerBegin(On.Celeste.Player.orig_DashBegin orig, Player self) {
            if (!VivHelperModule.Session.HasSpeedPower) { if (VivHelperModule.Session.AlwaysBreakDashBlockDash == 1) { VivHelperModule.Session.AlwaysBreakDashBlockDash = 2; } orig.Invoke(self); } else {
                if (VivHelperModule.Session.CanAddSpeed) {
                    VivHelperModule.Session.StoredSpeed += self.Speed;
                    self.Speed = Vector2.Zero;
                    Store = true;
                    self.StateMachine.State = 0;
                } else {
                    Vector2 value = new DynData<Player>(self).Get<Vector2>("lastAim");
                    value = 240 * Vector2.Normalize(CorrectDashPrecision(value));
                    self.Speed = value + new Vector2((VivHelperModule.Session.Facing == self.Facing ? 1 : -1) * VivHelperModule.Session.StoredSpeed.X, VivHelperModule.Session.StoredSpeed.Y);
                    VivHelperModule.Session.StoredSpeed = Vector2.Zero;
                    Launch = true;
                    self.StateMachine.State = 0;

                }
            }
        }

        private static Vector2 CorrectDashPrecision(Vector2 dir) {
            if (dir.X != 0f && Math.Abs(dir.X) < 0.001f) {
                dir.X = 0f;
                dir.Y = Math.Sign(dir.Y);
            } else if (dir.Y != 0f && Math.Abs(dir.Y) < 0.001f) {
                dir.Y = 0f;
                dir.X = Math.Sign(dir.X);
            }
            return dir;
        }

        private static void speedPowerEnd(On.Celeste.Player.orig_DashEnd orig, Player self) {
            if (!VivHelperModule.Session.HasSpeedPower) { VivHelperModule.Session.AlwaysBreakDashBlockDash = 0; orig.Invoke(self); } else {
                if (Store) {
                    VivHelperModule.Session.CanAddSpeed = false;
                    Store = false;
                } else if (Launch) {
                    VivHelperModule.Session.ResetSpeedPowerup();
                    Launch = false;
                }
            }

        }
    }

    [CustomEntity("VivHelper/EnergyCrystal")]
    public class EnergyCrystal : Entity {
        protected Image sprite;

        protected Image outline;

        protected Wiggler wiggler;

        protected BloomPoint bloom;

        protected VertexLight light;

        protected Level level;

        protected SineWave sine;

        protected bool oneUse;

        protected ParticleType p_shatter;

        protected ParticleType p_regen;

        protected ParticleType p_glow;

        protected float respawnTimer;

        protected SoundSource sfx;

        protected float scale;

        public EnergyCrystal(Vector2 position, bool oneUse, float scale)
            : base(position) {
            this.scale = scale;
            base.Collider = new Hitbox(12f * scale, 12f * scale, -6f * scale, -6f * scale);
            Add(new PlayerCollider(OnPlayer));
            this.oneUse = oneUse;
            p_shatter = Refill.P_Shatter;
            p_regen = Refill.P_Regen;
            p_glow = Refill.P_Glow;
            Add(outline = new Image(GFX.Game["VivHelper/entities/gemOutline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Image(GFX.Game["VivHelper/entities/gem"]));
            sprite.Visible = true;

            sprite.Scale = Vector2.One * scale;
            sprite.CenterOrigin();
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f * scale));
            Add(light = new VertexLight(Color.White, 1f, (int) (16 * scale), (int) (48 * scale)));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            base.Depth = -100;
            Add(sfx = new SoundSource());
            Add(new Coroutine(Distort()));
        }

        public EnergyCrystal(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("oneUse"), data.Float("Scale", 1f)) {
        }

        public IEnumerator Distort() {
            Level level = null;
            while (level == null) {
                level = base.Scene as Level;
            }
            while (true) {
                if (respawnTimer <= 0f) {
                    level.Displacement.AddBurst(base.Center, 5f, 0f, 30f * scale, 0.8f);
                    yield return 2.5f;
                }
                yield return null;
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update() {
            base.Update();
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f) {
                    Respawn();
                }
            } else if (base.Scene.OnInterval(0.1f)) {
                level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
            }
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
        }

        private void Respawn() {
            if (!Collidable) {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                base.Depth = -100;
                sfx.Play("event:/game/04_cliffside/arrowblock_reappear");
                level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY() {
            Image obj2 = sprite;
            float num2 = bloom.Y = sine.Value * 2f;
            float num5 = (obj2.Y = num2);
        }

        public override void Render() {
            if (sprite.Visible) {
                sprite.DrawOutline();
            }
            base.Render();
        }

        protected virtual void OnPlayer(Player player) {
            VivHelperModule.Session.HasSpeedPower = true;
            VivHelperModule.Session.CanAddSpeed = false;
            VivHelperModule.Session.StoredSpeed = player.Speed;
            VivHelperModule.Session.Facing = player.Facing;
            player.Speed = Vector2.Zero;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = 2.5f;
            for (int i = 0; i < 4; i++)
                Scene.Add(new AbsorbOrb(Position + new Vector2((float) Math.Cos(i * Math.PI / 2), (float) Math.Sin(i * Math.PI / 2))));
        }

        protected virtual IEnumerator RefillRoutine(Player player) {
            sfx.Play("event:/char/badeline/appear");
            Celeste.Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = false;
            if (!oneUse) {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - Consts.PIover2);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + Consts.PIover2);
            SlashFx.Burst(Position, num);
            if (oneUse) {
                RemoveSelf();
            }
        }

    }
}
