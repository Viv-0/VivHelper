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
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace VivHelper.Entities {

    public class WarpDashIndicator : Component {

        public static ParticleType particle;
        public static MTexture[] textures;

        private Player player;
        private TrailManager.Snapshot snapshot;

        public WarpDashIndicator() : base(true, true) {
            if (particle == null)
                particle = new ParticleType(Player.P_DashA) {
                    Color = Calc.HexToColor("0866de"),
                    Color2 = Calc.HexToColor("ff8000"),
                    DirectionRange = 0.05f
                };
        }

        public override void Update() {
            base.Update();
            if(snapshot != null) {
                snapshot.RemoveSelf();
            }
            if (player == null)
                player = Entity as Player;
            if (Visible) {
                snapshot = TrailManager.Add(Entity.Position, player.Sprite, player.Hair, new Vector2((float)player.Facing, 1f) * player.Sprite.Scale, player.GetCurrentTrailColor(), Entity.Depth + 1, 1f);
                snapshot.Visible = false;
            }
        }

        public override void Render() {
            base.Render();
            if (player == null)
                return;
            Vector2 aim = ((Vector2) VivHelper.player_lastAim.GetValue(player)).EightWayNormal();
            Vector2 oldPos = snapshot.Position;
            snapshot.Position = Entity.Position + aim * 36;
            snapshot.Render();
            snapshot.Position = oldPos;
        }

        /*public void HudRender(Level level) {
            Player p = Entity as Player;
            Color c = p.Hair.GetHairColor(0);
            MTexture tex;
            float angle = aim.Angle();
            switch (angle) {
                case 0:
                case -Consts.PI:
                case Consts.PI:
                    tex = textures[0];
                    break;
                case Consts.PIover4:
                case 3*Consts.PIover4:
                    tex = textures[3];
                    break;
                case Consts.PIover2:
                case -Consts.PIover2:
                    tex = textures[1];
                    break;
                default: tex = textures[2]; break;
            }
            Vector2 newPos = level.WorldToScreen();
            tex.DrawCentered(newPos, c, level.Zoom, 0, p.Facing == Facings.Right ? Microsoft.Xna.Framework.Graphics.SpriteEffects.None : Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally);
            tex.DrawCentered(newPos, c, level.Zoom, 0, p.Facing == Facings.Right ? Microsoft.Xna.Framework.Graphics.SpriteEffects.None : Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally);
        }*/
    }

    [CustomEntity("VivHelper/WarpDashRefill")]
    public class WarpDashRefill : RefillBase {
        public const int WiggleRoom = 7;
        public const string WarpDashPowerup = "vh_warpdash";
        public static int WarpDashState;

        protected override ParticleType ShatterParticle() => WarpDashIndicator.particle;

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
            Celeste.Celeste.Freeze(0.05f);
        }

        public static int WarpDashUpdate(Player player) {
            player.DashDir = ((Vector2) VivHelper.player_lastAim.GetValue(player)).EightWayNormal();
            if (player.DashDir == Vector2.Zero) {
                player.DashDir = Vector2.UnitX * (int) player.Facing;
            }
            ExplodeLaunchModifier.DisableFreeze = true;
            Vector2 oldPos = player.Position;
            Solid h = null;
            for (float t = 0f; t < 36; t++) {
                player.Position.X += player.DashDir.X;
                h = player.CollideFirst<Solid>();
                if (h != null && h.OnDashCollide != null) {
                    h.OnDashCollide(player, Vector2.Normalize(player.DashDir.XComp()));
                }
                player.Position.Y += player.DashDir.Y;
                h = player.CollideFirst<Solid>();
                if (h != null && h.OnDashCollide != null) {
                    h.OnDashCollide(player, Vector2.Normalize(player.DashDir.YComp()));
                }
            }
            player.Position = Calc.Round(player.Position);
            h = player.CollideFirst<Solid>();
            if (h != null && h.OnDashCollide != null) {
                h.OnDashCollide(player, Vector2.Normalize(player.DashDir.XComp()));
            }
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
                (player.Scene as Level).ParticlesFG.Emit(WarpDashIndicator.particle, player.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), player.DashDir.Angle() + 0.05f * i);
            }
        }
        public static void EffectBefore(Player player) {
            WarpDashIndicator warp = player.Get<WarpDashIndicator>();
            if (warp == null)
                player.Add(warp = new WarpDashIndicator());
            warp.Visible = true;
            if (player.Scene.OnInterval(0.1f)) {
                (player.Scene as Level).ParticlesBG.Emit(WarpDashIndicator.particle, player.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), Calc.Random.NextAngle());
            }
        }

        public static void EffectCancel(string replaceWith, Player player) {
            player.Get<WarpDashIndicator>().Visible = false;
        }
        public WarpDashRefill(EntityData data, Vector2 offset) : base(data, offset) {
            sprite = new Sprite(GFX.Game, "VivHelper/TSStelerefill/");
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.Play("idle");
            outline = new Image(GFX.Game["VivHelper/TSStelerefill/outline"]);
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
            if (sprite != null) { sprite.Visible = false; }
            if (flash != null) { flash.Visible = true; }
            if (!oneUse && outline != null) outline.Visible = true;
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            (Scene as Level).ParticlesFG.Emit(ShatterParticle(), 5, Position, Vector2.One * 4f, num - Consts.PIover2);
            (Scene as Level).ParticlesFG.Emit(ShatterParticle(), 5, Position, Vector2.One * 4f, num + Consts.PIover2);
            SlashFx.Burst(Position, num);
            if (oneUse) {
                RemoveSelf();
            }
        }
    }
}
