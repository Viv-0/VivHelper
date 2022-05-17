using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;

namespace VivHelper.Entities.Watchtowers {
    public class Hud : Entity {
        public bool TrackMode;

        public float TrackPercent;

        public bool OnlyY;

        public float Easer;

        private float timerUp;

        private float timerDown;

        private float timerLeft;

        private float timerRight;

        private float multUp;

        private float multDown;

        private float multLeft;

        private float multRight;

        private float left;

        private float right;

        private float up;

        private float down;

        private Vector2 aim;

        private MTexture halfDot = GFX.Gui["dot"].GetSubtexture(0, 0, 64, 32);

        public List<MTexture> hints;

        public Color paddingColor;

        public Hud() {
            AddTag(Tags.HUD);

            paddingColor = Color.White;
        }

        public override void Update() {
            Level level = SceneAs<Level>();
            Vector2 position = level.Camera.Position;
            Rectangle bounds = level.Bounds;
            int num = 320;
            int num2 = 180;
            bool flag = base.Scene.CollideCheck<LookoutBlocker>(new Rectangle((int) (position.X - 8f), (int) position.Y, num, num2));
            bool flag2 = base.Scene.CollideCheck<LookoutBlocker>(new Rectangle((int) (position.X + 8f), (int) position.Y, num, num2));
            bool flag3 = (TrackMode && TrackPercent >= 1f) || base.Scene.CollideCheck<LookoutBlocker>(new Rectangle((int) position.X, (int) (position.Y - 8f), num, num2));
            bool flag4 = (TrackMode && TrackPercent <= 0f) || base.Scene.CollideCheck<LookoutBlocker>(new Rectangle((int) position.X, (int) (position.Y + 8f), num, num2));
            left = Calc.Approach(left, (!flag && position.X > (float) (bounds.Left + 2)) ? 1 : 0, Engine.DeltaTime * 8f);
            right = Calc.Approach(right, (!flag2 && position.X + (float) num < (float) (bounds.Right - 2)) ? 1 : 0, Engine.DeltaTime * 8f);
            up = Calc.Approach(up, (!flag3 && position.Y > (float) (bounds.Top + 2)) ? 1 : 0, Engine.DeltaTime * 8f);
            down = Calc.Approach(down, (!flag4 && position.Y + (float) num2 < (float) (bounds.Bottom - 2)) ? 1 : 0, Engine.DeltaTime * 8f);
            aim = Input.Aim.Value;
            if (aim.X < 0f) {
                multLeft = Calc.Approach(multLeft, 0f, Engine.DeltaTime * 2f);
                timerLeft += Engine.DeltaTime * 12f;
            } else {
                multLeft = Calc.Approach(multLeft, 1f, Engine.DeltaTime * 2f);
                timerLeft += Engine.DeltaTime * 6f;
            }
            if (aim.X > 0f) {
                multRight = Calc.Approach(multRight, 0f, Engine.DeltaTime * 2f);
                timerRight += Engine.DeltaTime * 12f;
            } else {
                multRight = Calc.Approach(multRight, 1f, Engine.DeltaTime * 2f);
                timerRight += Engine.DeltaTime * 6f;
            }
            if (aim.Y < 0f) {
                multUp = Calc.Approach(multUp, 0f, Engine.DeltaTime * 2f);
                timerUp += Engine.DeltaTime * 12f;
            } else {
                multUp = Calc.Approach(multUp, 1f, Engine.DeltaTime * 2f);
                timerUp += Engine.DeltaTime * 6f;
            }
            if (aim.Y > 0f) {
                multDown = Calc.Approach(multDown, 0f, Engine.DeltaTime * 2f);
                timerDown += Engine.DeltaTime * 12f;
            } else {
                multDown = Calc.Approach(multDown, 1f, Engine.DeltaTime * 2f);
                timerDown += Engine.DeltaTime * 6f;
            }
            base.Update();
        }

        public override void Render() {
            Level level = base.Scene as Level;
            float num = Ease.CubeInOut(Easer);
            Color color = paddingColor * num;
            int num2 = (int) (80f * num);
            int num3 = (int) (80f * num * 0.5625f);
            int num4 = 8;
            if (level.FrozenOrPaused || level.RetryPlayerCorpse != null) {
                color *= 0.25f;
            }
            Draw.Rect(num2, num3, 1920 - num2 * 2 - num4, num4, color);
            Draw.Rect(num2, num3 + num4, num4 + 2, 1080 - num3 * 2 - num4, color);
            Draw.Rect(1920 - num2 - num4 - 2, num3, num4 + 2, 1080 - num3 * 2 - num4, color);
            Draw.Rect(num2 + num4, 1080 - num3 - num4, 1920 - num2 * 2 - num4, num4, color);
            if (level.FrozenOrPaused || level.RetryPlayerCorpse != null) {
                return;
            }
            MTexture mTexture = GFX.Gui["towerarrow"];
            float y = (float) num3 * up - (float) (Math.Sin(timerUp) * 18.0 * (double) MathHelper.Lerp(0.5f, 1f, multUp)) - (1f - multUp) * 12f;
            mTexture.DrawCentered(new Vector2(960f, y), color * up, 1f, (float) Math.PI / 2f);
            float y2 = 1080f - (float) num3 * down + (float) (Math.Sin(timerDown) * 18.0 * (double) MathHelper.Lerp(0.5f, 1f, multDown)) + (1f - multDown) * 12f;
            mTexture.DrawCentered(new Vector2(960f, y2), color * down, 1f, 4.712389f);
            if (!TrackMode && !OnlyY) {
                float num5 = left;
                float num6 = multLeft;
                float num7 = timerLeft;
                float num8 = right;
                float num9 = multRight;
                float num10 = timerRight;
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode) {
                    num5 = right;
                    num6 = multRight;
                    num7 = timerRight;
                    num8 = left;
                    num9 = multLeft;
                    num10 = timerLeft;
                }
                float x = (float) num2 * num5 - (float) (Math.Sin(num7) * 18.0 * (double) MathHelper.Lerp(0.5f, 1f, num6)) - (1f - num6) * 12f;
                mTexture.DrawCentered(new Vector2(x, 540f), color * num5);
                float x2 = 1920f - (float) num2 * num8 + (float) (Math.Sin(num10) * 18.0 * (double) MathHelper.Lerp(0.5f, 1f, num9)) + (1f - num9) * 12f;
                mTexture.DrawCentered(new Vector2(x2, 540f), color * num8, 1f, (float) Math.PI);
            } else if (TrackMode) {
                int num11 = 1080 - num3 * 2 - 128 - 64;
                int num12 = 1920 - num2 - 64;
                float num13 = (float) (1080 - num11) / 2f + 32f;
                Draw.Rect(num12 - 7, num13 + 7f, 14f, num11 - 14, Color.Black * num);
                halfDot.DrawJustified(new Vector2(num12, num13 + 7f), new Vector2(0.5f, 1f), Color.Black * num);
                halfDot.DrawJustified(new Vector2(num12, num13 + (float) num11 - 7f), new Vector2(0.5f, 1f), Color.Black * num, new Vector2(1f, -1f));
                GFX.Gui["lookout/cursor"].DrawCentered(new Vector2(num12, num13 + (1f - TrackPercent) * (float) num11), Color.White * num, 1f);
                GFX.Gui["lookout/summit"].DrawCentered(new Vector2(num12, num13 - 64f), Color.White * num, 0.65f);
            }
            if (hints != null) {
                if (hints.Count < 3) {
                    for (int i = 0; i < hints.Count; i++) {
                        hints[i].DrawCentered(new Vector2(num2 + num4 + 21f + 37f * i, 1080f - num3 - num4 - 21f), Color.White * (float) (color.A / 255f), 2f);
                    }
                } else {   // Structure:
                           // 1 2 3 4
                           // 5 6 7
                           // 5px offsets from bottom left corner
                    int i = 0;
                    int k = (hints.Count + 1) / 2;
                    for (; i < k; i++) {
                        hints[i].DrawCentered(new Vector2(num2 + num4 + 21f + 37f * i, 1080f - num3 - num4 - 21f - 37f), Color.White * (float) (color.A / 255f), 2f);
                    }
                    for (; i < hints.Count; i++) {
                        hints[i].DrawCentered(new Vector2(num2 + num4 + 21f + 37f * (i - k), 1080f - num3 - num4 - 21f), Color.White * (float) (color.A / 255f), 2f);
                    }
                }
            }

        }
    }
}
