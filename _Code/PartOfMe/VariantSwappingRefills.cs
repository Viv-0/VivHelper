using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.PartOfMe {
    [CustomEntity("VivHelper/VariantRefill")]
    public class VariantRefill : Entity {

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

        private bool refillDash;
        private bool canInteract = false;
        private bool redpurpswap;
        private bool backup;
        protected static bool partOfMefunc;

        private string varType;

        public VariantRefill(Vector2 position, bool oneUse, bool refillDash = true, string varType = "swap", bool x = false)
            : base(position) {
            base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            backup = x;
            partOfMefunc = false;
            this.oneUse = oneUse;
            this.refillDash = refillDash;
            string str = "VivHelper/VariantRefill/";
            this.varType = varType;
            str += varType;
            p_shatter = Refill.P_Shatter;
            p_regen = Refill.P_Regen;
            p_glow = Refill.P_Glow;
            Add(outline = new Image(GFX.Game[str + "Outline"]));
            outline.CenterOrigin();
            outline.Visible = false;

            Add(sprite = new Sprite(GFX.Game, str + "Idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, str + "Flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v) {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f));
            sine.Randomize();
            UpdateY();
            base.Depth = -100;
        }

        public VariantRefill(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("oneUse"), data.Bool("RefillDashOnUse", true), data.Attr("VariantSwapType", "swap"), data.Bool("PartOfMeFunctions", false)) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
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
            partOfMefunc = backup && SaveData.Instance.Assists.PlayAsBadeline;
            if (varType == "swap") { if (redpurpswap == SaveData.Instance.Assists.PlayAsBadeline) { redpurpswap = !SaveData.Instance.Assists.PlayAsBadeline; } }
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
            float num2 = bloom.Y = sine.Value * 2f;
            float num5 = obj.Y = (obj2.Y = num2);
        }

        public override void Render() {
            if (sprite.Visible) {
                sprite.DrawOutline();
            }
            base.Render();
        }

        private void OnPlayer(Player player) {
            if (Collidable && (SaveData.Instance.Assists.PlayAsBadeline != redpurpswap || (player.UseRefill(false) && refillDash))) {
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));
                respawnTimer = 2.5f;
            }
        }

        private IEnumerator RefillRoutine(Player player) {
            Celeste.Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = (flash.Visible = false);
            if (refillDash && player != null) { player.RefillDash(); player.RefillStamina(); }
            if (!oneUse) {
                outline.Visible = true;
            }
            base.Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - Consts.PIover2);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + Consts.PIover2);
            SlashFx.Burst(Position, num);
            if (varType == "red") { SaveData.Instance.Assists.PlayAsBadeline = false; } else if (varType == "purp") { SaveData.Instance.Assists.PlayAsBadeline = true; } else { SaveData.Instance.Assists.PlayAsBadeline = redpurpswap; }
            PlayerSpriteMode mode = SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode;
            if (player.Active) {
                player.ResetSpriteNextFrame(mode);
            } else {
                player.ResetSprite(mode);
            }


            if (oneUse) {
                RemoveSelf();
            }
        }

    }
}
