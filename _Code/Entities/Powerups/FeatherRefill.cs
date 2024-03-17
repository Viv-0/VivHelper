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
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/FeatherRefill")]
    public class FeatherRefill : RefillBase {
        public const string FeatherPowerup = "vh_feather";

        protected override ParticleType ShatterParticle() => FlyFeather.P_Collect;
        protected override ParticleType RegenParticle() => FlyFeather.P_Respawn;

        public FeatherRefill(EntityData data, Vector2 offset) : base(data, offset) {
            outline = new Image(GFX.Game["VivHelper/genericCircleRefill/outline"]);
            outline.CenterOrigin();
            outline.Visible = false;
            sprite = new Sprite(GFX.Game, "VivHelper/TSSfeatherRefill/");
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, "VivHelper/TSSfeatherRefill/"));
            flash.Add("flash", "flash", 0.05f);
            flash.OnFinish = delegate {
                flash.Visible = false;
            };
            flash.CenterOrigin();
        }

        protected override void OnPlayer(Player player) {
            if(DashPowerupManager.GivePowerup(FeatherPowerup, player)) {
                if (player.Dashes < player.Inventory.Dashes)
                    player.Dashes = player.Inventory.Dashes;
                Audio.Play("event:/VivHelper/tele_diamond_touch", Position);
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
            sprite.Visible = flash.Visible = false;
            if (!oneUse) {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(ShatterParticle(), 5, Position, Vector2.One * 4f, num - Consts.PIover2);
            level.ParticlesFG.Emit(ShatterParticle(), 5, Position, Vector2.One * 4f, num + Consts.PIover2);
            SlashFx.Burst(Position, num);
            if (oneUse) {
                RemoveSelf();
            }
        }

        public static void EffectBefore(Player player) {
            if (player.Scene.OnInterval(0.05f)) {
                float angle = player.Speed.LengthSquared() < 0.01f ? (player.Facing == Facings.Left ? 0 : MathF.PI) : -player.Speed.Angle();
                player.SceneAs<Level>().ParticlesFG.Emit(FlyFeather.P_Boost, 1, player.Center, Vector2.One * 4f, angle);
            }
        }
    }
}
