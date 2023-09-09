using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Celeste;
using Microsoft.Xna.Framework;
using System.Collections;
using MonoMod.Utils;
using Celeste.Mod.Entities;

namespace VivHelper.Entities
{
    [CustomEntity("VivHelper/RedDashRefill")]
    public class RedDashRefill : RefillBase
    {
        public const string RedDashPowerup = "vh_reddash";

        private static ParticleType P_Shatter;
        private static ParticleType P_Glow;
        private static ParticleType P_Regen;

        public RedDashRefill(EntityData data, Vector2 offset)
            : base(data, offset) { 
            if(P_Shatter == null) {
                P_Shatter = new ParticleType(Refill.P_Shatter) {
                    Color = Calc.HexToColor("ffc494"),
                    Color2 = Calc.HexToColor("5e1009")
                };
                P_Glow = new ParticleType(Refill.P_Glow) {
                    Color = Calc.HexToColor("ff594a"),
                    Color2 = Calc.HexToColor("9c1105")
                };
                P_Regen = new ParticleType(Refill.P_Regen) {
                    Color = Calc.HexToColor("ff594a"),
                    Color2 = Calc.HexToColor("9c1105")
                };
            }
            p_shatter = P_Shatter;
            p_glow = P_Glow;
            p_regen = P_Regen;
            outline = new Image(GFX.Game["VivHelper/redDashRefill/redOutline"]);
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, "VivHelper/redDashRefill/redIdle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, "objects/refill/flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
        }

        public override void Render() {
            if (sprite.Visible) {
                sprite.DrawOutline();
            }
            base.Render();
        }

        protected override void OnPlayer(Player player) {
            if (DashPowerupManager.GivePowerup(RedDashPowerup, player)) {
                if (player.Dashes < player.Inventory.Dashes)
                    player.Dashes = player.Inventory.Dashes;
                Audio.Play("event:/VivHelper/red_diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));
                respawnTimer = 2.5f;
            }
        }

        public static void EffectBefore(Player player) {
            Vector2 dir = Vector2.Normalize(player.Speed);
            if (player.Scene.OnInterval(0.02f)) {
                (player.Scene as Level).ParticlesBG.Emit(Booster.P_BurstRed, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), new Color(49, 15, 21, 85), (-dir).Angle());
            }
        }
        public static void EffectAt(Player player) {
            if (player.Dashes == 0)
                player.Dashes = 1;
        }
        public static void EffectDuring(Player player) {
            if (player.Scene.OnInterval(0.02f)) {
                (player.Scene as Level).ParticlesBG.Emit(Booster.P_BurstRed, 2, player.Center - player.DashDir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), new Color(144, 48, 62), (-player.DashDir).Angle());
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
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, Color.Red, num - (float) Math.PI / 2f);
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, Color.Red, num + (float) Math.PI / 2f);
            SlashFx.Burst(Position, num);
            if (oneUse) {
                RemoveSelf();
            }
        }
    }
}
