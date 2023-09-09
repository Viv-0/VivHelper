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
using Celeste.Mod;

namespace VivHelper.Entities {
    public class Firework : Component {
        private static Vector2 gravity = new Vector2(0, 80f);
        ParticleType p_Pop;
        Color color;
        public Firework(ParticleType p_Pop, Color color) : base(true,true) {
            this.p_Pop = new ParticleType(p_Pop) {
                LifeMin = 1f, LifeMax = 1.8f,
                SpeedMin = 300f, SpeedMax = 300f,
            };
            this.color = color;
        }

        public void Pop() {
            Player p = Entity as Player;
            ExplodeLaunchModifier.EightWayLaunch(p, (Vector2)VivHelper.player_lastAim.GetValue(p), ExplodeLaunchModifier.RestrictBoost.NoBoost);
            for (int i = 0; i < 13; i++) {
                float angle = (float) Math.PI * 2f / (float) i;
                p_Pop.Acceleration = Calc.AngleToVector(angle, -360) + gravity;
                SceneAs<Level>().ParticlesFG.Emit(p_Pop, 1, p.Center + Vector2.One.RotateTowards(angle,7f)*12f, Vector2.Zero, color, angle);
            }
            RemoveSelf();
        }
    }

    public class FireworkRefill : RefillBase {
        internal static ParticleType drip = new ParticleType() {
            Color = Color.White,
            Color2 = Color.Gray,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Linear,
            Size = 1f,
            SizeRange = 0f,
            SpeedMin = 20f,
            SpeedMax = 40f,
            Direction = (float) Math.PI / 2f,
            DirectionRange = 0.05f,
            Acceleration = Vector2.UnitY * 20f,
            LifeMin = 0.9f,
            LifeMax = 1.3f
        };
        public static void EffectBefore(Player player) {
            if(player.Scene.OnInterval(0.04f))
                player.SceneAs<Level>().ParticlesBG.Emit(drip, player.Position);
        }

        public static void EffectAt(Player player) {
        }

        public static void EffectCancel(string str, Player player) {

        }

        public static IEnumerator RoutineAfter(Player player) {
            Vector2 dir = (Vector2) VivHelper.player_lastAim.GetValue(player);
            ExplodeLaunchModifier.EightWayLaunch(player, player.Position - dir, ExplodeLaunchModifier.RestrictBoost.NoBoost);
            Firework f = player.Get<Firework>();
            if(f == null) {
                player.SceneAs<Level>().ParticlesFG.Emit(Bumper.P_Launch, 10, player.Center, Vector2.One * 6f, -dir.Angle());
                yield break;
            }
            f.Pop();
        }

        public const string FireworkPowerup = "vh_firework";

        private static ParticleType P_Shatter;
        private static ParticleType P_Glow;
        private static ParticleType P_Regen;

        private Image i1, i2, i3; // i1 Visibility handles all 3 render checks
        private Color color;
        public FireworkRefill(EntityData data, Vector2 offset)
            : base(data, offset) {
            if (P_Shatter == null) {
                P_Shatter = new ParticleType(Refill.P_Shatter) {
                    Color2 = Color.White,
                    ColorMode = ParticleType.ColorModes.Blink
                };
                P_Glow = new ParticleType(Refill.P_Glow) {
                    Color2 = Color.White,
                    ColorMode = ParticleType.ColorModes.Blink
                };
                P_Regen = new ParticleType(Refill.P_Regen) {
                    Color2 = Color.White,
                    ColorMode = ParticleType.ColorModes.Blink
                };
            }
            p_shatter = P_Shatter;
            p_glow = P_Glow;
            p_regen = P_Regen;
            outline = new Image(GFX.Game["VivHelper/genericCircleRefill/outline"]);
            outline.CenterOrigin();
            outline.Visible = false;
            Remove(sprite);
            sprite = null;
            i1 = new Image(GFX.Game["VivHelper/fireworkRefill/outline"]);
            i2 = new Image(GFX.Game["VivHelper/fireworkRefill/main"]);
            i3 = new Image(GFX.Game["VivHelper/fireworkRefill/overlay"]);
            Add(flash = new Sprite(GFX.Game, "VivHelper/genericCircleRefill/flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Remove(wiggler);
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                i1.Scale = i2.Scale = i3.Scale = flash.Scale = Vector2.One * (1f + v * 0.2f);
            }));
            color = data.Color("fireworkColor", Color.Red);

        }

        protected override void UpdateY() {
            bloom.Y = flash.Y = i1.Y = i2.Y = i3.Y = sine.Value * 2f;
        }

        public override void Render() {
            if (outline.Visible)
                outline.Render();
            if (i1.Visible) {
                i1.DrawOutline();
                i1.Render();
                i2.Render();
                i3.Render();
            }
            if (flash.Visible)
                flash.Render();
        }

        protected override void OnPlayer(Player player) {
            if (DashPowerupManager.GivePowerup(FireworkPowerup, player)) {
                if (player.Dashes < player.Inventory.Dashes)
                    player.Dashes = player.Inventory.Dashes;
                Audio.Play("event:/VivHelper/fireworkWhistle" , Position);
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
            i1.Visible = flash.Visible = false;
            if (!oneUse) {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, color, num - (float) Math.PI / 2f);
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, color, num + (float) Math.PI / 2f);
            SlashFx.Burst(Position, num);
            if (oneUse) {
                RemoveSelf();
            }
        }
    }
}
