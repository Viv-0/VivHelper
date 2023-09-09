using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Collections;
using System.Runtime.CompilerServices;

namespace VivHelper.Entities.Powerups {
    public class FeatherRefill : Entity {
        public static ParticleType P_Shatter;

        public static ParticleType P_Regen;

        public static ParticleType P_Glow;

        private Sprite sprite;

        private Sprite flash;

        private Image outline;

        private Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Level level;

        private SineWave sine;

        private bool oneUse;

        private ParticleType p_shatter;

        private ParticleType p_regen;

        private ParticleType p_glow;

        private float respawnTimer;

        public FeatherRefill(Vector2 position, bool oneUse)
            : base(position) {
            if(P_Shatter == null) {
                P_Shatter = new ParticleType(Refill.P_Shatter) {
                    Source = GFX.Game["particles/feather"],
                    Color = Player.FlyPowerHairColor,
                    Color2 = Calc.HexToColor("fff20f"),
                    ColorMode = ParticleType.ColorModes.Blink,
                };
                P_Glow = new ParticleType(Refill.P_Glow) {
                    Source = GFX.Game["particles/feather"],
                    Color = Player.FlyPowerHairColor,
                    Color2 = Calc.HexToColor("fff20f"),
                    ColorMode = ParticleType.ColorModes.Blink,
                };
                P_Regen = FlyFeather.P_Respawn;
            }
            base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            this.oneUse = oneUse;
            string text = "objects/refill/";
                p_shatter = P_Shatter;
                p_regen = P_Regen;
                p_glow = P_Glow;
            Add(outline = new Image(GFX.Game[text + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, text + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, text + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            base.Depth = -100;
        }

        public FeatherRefill(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("oneUse")) {
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
            if (base.Scene.OnInterval(2f) && sprite.Visible) {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }

        private void Respawn() {
            if (!Collidable) {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                base.Depth = -100;
                wiggler.Start();
                Audio.Play("event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY() {
            Sprite obj = flash;
            Sprite obj2 = sprite;
            float num2 = (bloom.Y = sine.Value * 2f);
            float num5 = (obj.Y = (obj2.Y = num2));
        }

        public override void Render() {
            if (sprite.Visible) {
                sprite.DrawOutline();
            }
            base.Render();
        }

        private void OnPlayer(Player player) {
            if (DashPowerupManager.GivePowerup("vh_feather", player)) {
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));
                respawnTimer = 2.5f;
            }
        }

        private IEnumerator RefillRoutine(Player player) {
            global::Celeste.Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = (flash.Visible = false);
            if (!oneUse) {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float) Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float) Math.PI / 2f);
            SlashFx.Burst(Position, num);
            if (oneUse) {
                RemoveSelf();
            }
        }
    }
}
