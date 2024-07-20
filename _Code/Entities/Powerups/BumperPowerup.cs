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
        private static int previousDashes;

        public BumperRefill(EntityData data, Vector2 offset) : base(data, offset) {
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
            level.ParticlesFG.Emit(ShatterParticle(), 5, Position, Vector2.One * 4f, num - Consts.PIover2);
            level.ParticlesFG.Emit(ShatterParticle(), 5, Position, Vector2.One * 4f, num + Consts.PIover2);
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
            player.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => {
                Vector2 dir = (Vector2) VivHelper.player_lastAim.GetValue(player);
                player.DashDir = dir;
                Audio.Play(SFX.game_06_pinballbumper_hit);
                Vector2 oldSpeed = player.Speed;
                ExplodeLaunchModifier.EightWayLaunch(player, player.Center - dir, ExplodeLaunchModifier.RestrictBoost.NoBoost, true);
                if (oldSpeed.LengthSquared() > 78400) { // 280^2
                    player.Speed = Vector2.Normalize(player.Speed) * (140 + oldSpeed.Length() / 2);
                } else if (dir.X != 0 && dir.Y < -0.7f && dir.Y > -0.71f) {
                    player.Speed.Y = -225; // makes the Y value 225 which just feels better to play
                    player.Speed.X = Math.Sign(player.Speed.X) * 225;
                }
                player.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Launch, 10, player.Center, Vector2.One * 6f, Consts.PI + dir.Angle());
            }, 0.01f, true));
            player.Add(new Coroutine(Routine(player)));
            Celeste.Celeste.Freeze(0.05f);
            
        }

        public static IEnumerator Routine(Player player) {
            yield return null;
            while(player.StateMachine.State == Player.StLaunch) {
                if(player.CollideCheck<Solid>(player.Position + Vector2.UnitY)) {
                    player.StateMachine.State = Player.StNormal;
                    yield break;
                } else if(player.DashDir.X != 0 && player.CollideCheck<Solid>(player.Position + Vector2.UnitX * player.DashDir.X) && player.Speed.LengthSquared() > 44100 && player.Speed.Y > 0) {
                    player.Speed.Y /= 2f;
                    player.StateMachine.State = Player.StNormal;
                    yield break;
                }
                yield return null;
            }
        }
    }
}
