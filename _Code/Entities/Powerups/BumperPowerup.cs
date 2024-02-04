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
            sprite = new Sprite(GFX.Game, "VivHelper/TSSbumperrefill/");
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, "VivHelper/TSSbumperrefill/"));
            flash.Add("flash", "flash", 0.05f);
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
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - Consts.PIover2);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + Consts.PIover2);
            SlashFx.Burst(Position, num);
            if (oneUse) {
                RemoveSelf();
            }
        }

        public static void EffectBefore(Player player) {
            if(player.Scene.OnInterval(0.05f))
                player.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Ambience, 1, player.Center, Vector2.One * 4f);
        }

        public static void EffectAt(Player player) {
            Vector2 dir = (Vector2) VivHelper.player_lastAim.GetValue(player);
            Audio.Play(SFX.game_06_pinballbumper_hit);
            ExplodeLaunchModifier.EightWayLaunch(player, player.Center - dir, ExplodeLaunchModifier.RestrictBoost.NoBoost);
            if (dir.X != 0 && dir.Y < -0.7f && dir.Y > -0.71f) {
                player.Speed.Y = -225; // makes the Y value 225 which just feels better to play
                player.Speed.X = Math.Sign(player.Speed.X) * 225;
            }
            player.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Launch, 10, player.Center, Vector2.One * 6f, Consts.PI + dir.Angle());
        }
    }
}
