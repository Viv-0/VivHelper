using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.TrackSpinner;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;
using MonoMod.Utils;
using Celeste.Mod.Entities;
using System.Reflection;
using static MonoMod.InlineRT.MonoModRule;
using YamlDotNet.Core.Tokens;

namespace VivHelper.Entities {

    public class WarpDashIndicator : Component {

        public WarpDashIndicator() : base(true, true) {
        }

        public override void Render() {
            Vector2 aim = (Vector2) VivHelper.player_lastAim.GetValue(Entity as Player);
            Color c = (Entity as Player).Hair.GetHairColor(0);
            Vector2 rawPosition = Entity.Position - new Vector2(16,32) + aim * 36;
            SpawnPoint._texture.Draw(rawPosition, Vector2.Zero, c);
            for (int i = 1; i < 6; i++) { 
                SpawnPoint._texture.Draw(rawPosition + aim * i, Vector2.Zero, c * (1- (i * 0.125f)));
                SpawnPoint._texture.Draw(rawPosition - aim * i, Vector2.Zero, c * (1 - (i * 0.125f)));
            }
        }
    }

    [CustomEntity("VivHelper/WarpDashRefill")]
    public class WarpDashRefill : RefillBase {
        public const int WiggleRoom = 7;
        public const string WarpDashPowerup = "vh_warpdash";
        public static int WarpDashState;
        public static ParticleType particle;
        public static void WarpDashBegin(Player player) {
            foreach (DashListener component in player.Scene.Tracker.GetComponents<DashListener>()) {
                if (component.OnDash != null) {
                    component.OnDash(player.DashDir);
                }
            }
            if (player.Get<WarpDashIndicator>() is { } wdi) {
                wdi.Visible = false;
            }
            Audio.Play("event:/VivHelper/warp");
        }

        public static int WarpDashUpdate(Player player) {
            Celeste.Celeste.Freeze(0.05f);
            player.DashDir = (Vector2) VivHelper.player_lastAim.GetValue(player);
            if (player.DashDir == Vector2.Zero) {
                player.DashDir = Vector2.UnitX * (int) player.Facing;
            }
            ExplodeLaunchModifier.DisableFreeze = true;
            for (float t = 0f; t < 36; t++) {
                player.Position.X += player.DashDir.X;
                Solid h = player.CollideFirst<Solid>();
                if (h != null && h.OnDashCollide != null) {
                    h.OnDashCollide(player, Vector2.Normalize(player.DashDir.XComp()));
                }
                player.Position.Y += player.DashDir.Y;
                h = player.CollideFirst<Solid>();
                if (h != null && h.OnDashCollide != null) {
                    h.OnDashCollide(player, Vector2.Normalize(player.DashDir.YComp()));
                }
            }
            Vector2 oldPos = player.Position;
            ExplodeLaunchModifier.DetectFreeze = false;
            ExplodeLaunchModifier.DisableFreeze = false;
            Vector2 beforeDashSpeed = player.Speed;
            Vector2 value = VivHelper.CorrectDashPrecision(player.DashDir);
            Vector2 speed = value * 240f;
            if (Math.Abs(beforeDashSpeed.X) > Math.Abs(speed.X)) {
                speed.X = Math.Abs(beforeDashSpeed.X) * Calc.Sign(player.DashDir).X;
            }
            if (Math.Abs(beforeDashSpeed.Y) > Math.Abs(speed.Y)) {
                speed.Y = Math.Abs(beforeDashSpeed.Y) * Calc.Sign(player.DashDir).Y;
            }
            player.Speed = speed;
            if (!TrySquishWiggle(player)) {
                player.Position = oldPos;
                player.Die(player.DashDir);
            }
            if((float)VivHelper.player_jumpGraceTimer.GetValue(player) > 0.02f)
                VivHelper.player_jumpGraceTimer.SetValue(player, 0.02f);
            return 0;
        }

        private static bool TrySquishWiggle(Player player) {
            for (int i = 0; i <= WiggleRoom; i++) {
                for (int j = 0; j <= WiggleRoom; j++) {
                    if (i == 0 && j == 0) {
                        continue;
                    }
                    for (int num = 1; num >= -1; num -= 2) {
                        for (int num2 = 1; num2 >= -1; num2 -= 2) {
                            Vector2 vector = new Vector2(i * num, j * num2);
                            if (!player.CollideCheck<Solid>(player.Position + vector)) {
                                player.Position += vector;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static void WarpDashEnd(Player player) {
            for (int i = -5; i < 6; i++) {
                (player.Scene as Level).ParticlesFG.Emit(particle, player.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), player.DashDir.Angle() + 0.05f * i);
            }
        }
        public static void EffectBefore(Player player) {
            WarpDashIndicator warp = player.Get<WarpDashIndicator>();
            if (warp == null)
                player.Add(warp = new WarpDashIndicator());
            warp.Visible = true;
            if (player.Scene.OnInterval(0.2f)) {
                (player.Scene as Level).ParticlesBG.Emit(particle, player.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), Calc.Random.NextAngle());
            }
        }

        public static void EffectCancel(string replaceWith, Player player) {
            player.Get<WarpDashIndicator>().Visible = false;
        }
        public WarpDashRefill(EntityData data, Vector2 offset) : base(data, offset) {
            if (particle == null)
                particle = new ParticleType(Player.P_DashA) {
                    Color = Calc.HexToColor("0866de"),
                    Color2 = Calc.HexToColor("ff8000"),
                    DirectionRange = 0.05f
                };
            p_shatter = particle;
            p_glow = particle;
            p_regen = particle;
            outline = new Image(GFX.Game["VivHelper/TSStelerefill/outline"]);
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, "VivHelper/TSStelerefill/"));
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, "objects/refill/flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate {
                flash.Visible = false;
            };
            flash.CenterOrigin();
        }

        protected override void OnPlayer(Player player) {
            if (DashPowerupManager.GivePowerup(WarpDashPowerup, player)) {
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
            (Scene as Level).Shake();
            sprite.Visible = flash.Visible = false;
            if (oneUse)
                outline.Visible = false;
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            (Scene as Level).ParticlesFG.Emit(particle, 5, Position, Vector2.One * 4f, num - (float) Math.PI / 2f);
            (Scene as Level).ParticlesFG.Emit(particle, 5, Position, Vector2.One * 4f, num + (float) Math.PI / 2f);
            SlashFx.Burst(Position, num);
            if (oneUse) {
                RemoveSelf();
            }
        }
    }
}
