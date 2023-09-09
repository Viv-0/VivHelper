using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/BumperRefill")]
    public class BumperRefill : RefillBase {
        public const string BumperPowerup = "vh_bumper";

        public BumperRefill(EntityData data, Vector2 offset) : base(data, offset) {
            p_shatter = Bumper.P_Launch;
            p_glow = Bumper.P_Ambience;
            p_regen = Bumper.P_Ambience;
            outline = new Image(GFX.Game["VivHelper/genericCircleRefill/outline"]);
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, "VivHelper/TSSbumperrefill/"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, "VivHelper/TSSbumperrefill/"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate {
                flash.Visible = false;
            };
            flash.CenterOrigin();
        }

        protected override void OnPlayer(Player player) {
            if (DashPowerupManager.GivePowerup(BumperPowerup, player)) {
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
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float) Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float) Math.PI / 2f);
            SlashFx.Burst(Position, num);
            if (oneUse) {
                RemoveSelf();
            }
        }

        public static void EffectBefore(Player player) {
            player.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Ambience, 2, player.Center, Vector2.One * 4f);
        }

        public static void EffectAt(Player player) {
            Vector2 dir = (Vector2) VivHelper.player_lastAim.GetValue(player);
            ExplodeLaunchModifier.EightWayLaunch(player, player.Position - dir, ExplodeLaunchModifier.RestrictBoost.NoBoost);
            player.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Launch, 10, player.Center, Vector2.One * 6f, -dir.Angle());
        }
    }
}
