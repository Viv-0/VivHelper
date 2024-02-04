using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CornerBoostZipMover")]
    [Tracked]
    public class CornerBoostZipMover : CornerBoostSolid {
        public enum Themes {
            Normal,
            Moon
        }

        private class ZipMoverPathRenderer : Entity {
            public CornerBoostZipMover ZipMover;

            private MTexture cog;

            private Vector2 from;

            private Vector2 to;

            private Vector2 sparkAdd;

            private float sparkDirFromA;

            private float sparkDirFromB;

            private float sparkDirToA;

            private float sparkDirToB;

            public ZipMoverPathRenderer(CornerBoostZipMover zipMover) {
                base.Depth = 5000;
                ZipMover = zipMover;
                from = ZipMover.start + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
                to = ZipMover.target + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
                sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
                float num = (from - to).Angle();
                sparkDirFromA = num + Consts.PIover8;
                sparkDirFromB = num - Consts.PIover8;
                sparkDirToA = num + Consts.PIover8 * 7;
                sparkDirToB = num + Consts.PIover8 * 9;
                if (zipMover.theme == Themes.Moon) {
                    cog = GFX.Game["objects/zipmover/moon/cog"];
                } else {
                    cog = GFX.Game["objects/zipmover/cog"];
                }
            }

            public void CreateSparks() {
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
            }

            public override void Render() {
                DrawCogs(Vector2.UnitY, Color.Black);
                DrawCogs(Vector2.Zero);
                if (ZipMover.drawBlackBorder) {
                    Draw.Rect(new Rectangle((int) (ZipMover.X + ZipMover.Shake.X - 1f), (int) (ZipMover.Y + ZipMover.Shake.Y - 1f), (int) ZipMover.Width + 2, (int) ZipMover.Height + 2), Color.Black);
                }
            }

            private void DrawCogs(Vector2 offset, Color? colorOverride = null) {
                Vector2 vector = (to - from).SafeNormalize();
                Vector2 value = vector.Perpendicular() * 3f;
                Vector2 value2 = -vector.Perpendicular() * 4f;
                float rotation = ZipMover.percent * Consts.TAU;
                Draw.Line(from + value + offset, to + value + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
                Draw.Line(from + value2 + offset, to + value2 + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
                for (float num = 4f - ZipMover.percent * (float) Math.PI * 8f % 4f; num < (to - from).Length(); num += 4f) {
                    Vector2 value3 = from + value + vector.Perpendicular() + vector * num;
                    Vector2 value4 = to + value2 - vector * num;
                    Draw.Line(value3 + offset, value3 + vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
                    Draw.Line(value4 + offset, value4 - vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
                }
                cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
                cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
            }
        }

        public static ParticleType P_Scrape = ZipMover.P_Scrape;

        public static ParticleType P_Sparks = ZipMover.P_Sparks;

        private Themes theme;

        private MTexture[,] edges = new MTexture[3, 3];

        private Sprite streetlight;

        private BloomPoint bloom;

        private ZipMoverPathRenderer pathRenderer;

        private List<MTexture> innerCogs;

        private MTexture temp = new MTexture();

        private bool drawBlackBorder;

        private Vector2 start;

        private Vector2 target;

        private float percent;

        private static readonly Color ropeColor = Calc.HexToColor("663931");

        private static readonly Color ropeLightColor = Calc.HexToColor("9b6157");

        private SoundSource sfx = new SoundSource();

        public CornerBoostZipMover(Vector2 position, int width, int height, Vector2 target, Themes theme, bool perfectCB)
            : base(position, width, height, safe: false, perfectCB) {
            base.Depth = -9999;
            start = Position;
            this.target = target;
            this.theme = theme;
            Add(new Coroutine(Sequence()));
            Add(new LightOcclude());
            string path;
            string id;
            string key;
            if (theme == Themes.Moon) {
                path = "objects/zipmover/moon/light";
                id = "objects/zipmover/moon/block";
                key = "objects/zipmover/moon/innercog";
                drawBlackBorder = false;
            } else {
                path = "objects/zipmover/light";
                id = "objects/zipmover/block";
                key = "objects/zipmover/innercog";
                drawBlackBorder = true;
            }
            innerCogs = GFX.Game.GetAtlasSubtextures(key);
            Add(streetlight = new Sprite(GFX.Game, path));
            streetlight.Add("frames", "", 1f);
            streetlight.Play("frames");
            streetlight.Active = false;
            streetlight.SetAnimationFrame(1);
            streetlight.Position = new Vector2(base.Width / 2f - streetlight.Width / 2f, 0f);
            Add(bloom = new BloomPoint(1f, 6f));
            bloom.Position = new Vector2(base.Width / 2f, 4f);
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    edges[i, j] = GFX.Game[id].GetSubtexture(i * 8, j * 8, 8, 8);
                }
            }
            SurfaceSoundIndex = 7;
            sfx.Position = new Vector2(base.Width, base.Height) / 2f;
            Add(sfx);
        }

        public CornerBoostZipMover(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal), data.Bool("PerfectCornerBoost", false)) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            scene.Add(pathRenderer = new ZipMoverPathRenderer(this));
        }

        public override void Removed(Scene scene) {
            scene.Remove(pathRenderer);
            pathRenderer = null;
            base.Removed(scene);
        }

        public override void Update() {
            base.Update();
            bloom.Y = streetlight.CurrentAnimationFrame * 3;
        }

        public override void Render() {
            Vector2 position = Position;
            Position += base.Shake;
            Draw.Rect(base.X + 1f, base.Y + 1f, base.Width - 2f, base.Height - 2f, Color.Black);
            int num = 1;
            float num2 = 0f;
            int count = innerCogs.Count;
            for (int i = 4; (float) i <= base.Height - 4f; i += 8) {
                int num3 = num;
                for (int j = 4; (float) j <= base.Width - 4f; j += 8) {
                    int index = (int) (VivHelper.mod((num2 + (float) num * percent * Consts.TAU * 2f) / Consts.PIover2, 1f) * (float) count);
                    MTexture mTexture = innerCogs[index];
                    Rectangle rectangle = new Rectangle(0, 0, mTexture.Width, mTexture.Height);
                    Vector2 zero = Vector2.Zero;
                    if (j <= 4) {
                        zero.X = 2f;
                        rectangle.X = 2;
                        rectangle.Width -= 2;
                    } else if ((float) j >= base.Width - 4f) {
                        zero.X = -2f;
                        rectangle.Width -= 2;
                    }
                    if (i <= 4) {
                        zero.Y = 2f;
                        rectangle.Y = 2;
                        rectangle.Height -= 2;
                    } else if ((float) i >= base.Height - 4f) {
                        zero.Y = -2f;
                        rectangle.Height -= 2;
                    }
                    mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
                    mTexture.DrawCentered(Position + new Vector2(j, i) + zero, Color.White * ((num < 0) ? 0.5f : 1f));
                    num = -num;
                    num2 += Consts.PIover3;
                }
                if (num3 == num) {
                    num = -num;
                }
            }
            for (int k = 0; (float) k < base.Width / 8f; k++) {
                for (int l = 0; (float) l < base.Height / 8f; l++) {
                    int num4 = ((k != 0) ? (((float) k != base.Width / 8f - 1f) ? 1 : 2) : 0);
                    int num5 = ((l != 0) ? (((float) l != base.Height / 8f - 1f) ? 1 : 2) : 0);
                    if (num4 != 1 || num5 != 1) {
                        edges[num4, num5].Draw(new Vector2(base.X + (float) (k * 8), base.Y + (float) (l * 8)));
                    }
                }
            }
            base.Render();
            Position = position;
        }

        private void ScrapeParticlesCheck(Vector2 to) {
            if (!base.Scene.OnInterval(0.03f)) {
                return;
            }
            bool flag = to.Y != base.ExactPosition.Y;
            bool flag2 = to.X != base.ExactPosition.X;
            if (flag && !flag2) {
                int num = Math.Sign(to.Y - base.ExactPosition.Y);
                Vector2 value = ((num != 1) ? base.TopLeft : base.BottomLeft);
                int num2 = 4;
                if (num == 1) {
                    num2 = Math.Min((int) base.Height - 12, 20);
                }
                int num3 = (int) base.Height;
                if (num == -1) {
                    num3 = Math.Max(16, (int) base.Height - 16);
                }
                if (base.Scene.CollideCheck<Solid>(value + new Vector2(-2f, num * -2))) {
                    for (int i = num2; i < num3; i += 8) {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2(0f, (float) i + (float) num * 2f), Consts.PIover4 * ((num == 1) ? -1 : 1));
                    }
                }
                if (base.Scene.CollideCheck<Solid>(value + new Vector2(base.Width + 2f, num * -2))) {
                    for (int j = num2; j < num3; j += 8) {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopRight + new Vector2(-1f, (float) j + (float) num * 2f), Consts.PIover4 * ((num == 1) ? -3 : 3));
                    }
                }
            } else {
                if (!flag2 || flag) {
                    return;
                }
                int num4 = Math.Sign(to.X - base.ExactPosition.X);
                Vector2 value2 = ((num4 != 1) ? base.TopLeft : base.TopRight);
                int num5 = 4;
                if (num4 == 1) {
                    num5 = Math.Min((int) base.Width - 12, 20);
                }
                int num6 = (int) base.Width;
                if (num4 == -1) {
                    num6 = Math.Max(16, (int) base.Width - 16);
                }
                if (base.Scene.CollideCheck<Solid>(value2 + new Vector2(num4 * -2, -2f))) {
                    for (int k = num5; k < num6; k += 8) {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2((float) k + (float) num4 * 2f, -1f), Consts.PIover4 * ((num4 == 1) ? 3 : 1));
                    }
                }
                if (base.Scene.CollideCheck<Solid>(value2 + new Vector2(num4 * -2, base.Height + 2f))) {
                    for (int l = num5; l < num6; l += 8) {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.BottomLeft + new Vector2((float) l + (float) num4 * 2f, 0f), Consts.PIover4 * ((num4 == 1) ? -3 : -1));
                    }
                }
            }
        }

        private IEnumerator Sequence() {
            Vector2 start = Position;
            while (true) {
                if (!HasPlayerRider()) {
                    yield return null;
                    continue;
                }
                sfx.Play((theme == Themes.Normal) ? "event:/game/01_forsaken_city/zip_mover" : "event:/new_content/game/10_farewell/zip_mover");
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                StartShaking(0.1f);
                yield return 0.1f;
                streetlight.SetAnimationFrame(3);
                StopPlayerRunIntoAnimation = false;
                float at2 = 0f;
                while (at2 < 1f) {
                    yield return null;
                    at2 = Calc.Approach(at2, 1f, 2f * Engine.DeltaTime);
                    percent = Ease.SineIn(at2);
                    Vector2 vector = Vector2.Lerp(start, target, percent);
                    ScrapeParticlesCheck(vector);
                    if (Scene.OnInterval(0.1f)) {
                        pathRenderer.CreateSparks();
                    }
                    MoveTo(vector);
                }
                StartShaking(0.2f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().Shake();
                StopPlayerRunIntoAnimation = true;
                yield return 0.5f;
                StopPlayerRunIntoAnimation = false;
                streetlight.SetAnimationFrame(2);
                at2 = 0f;
                while (at2 < 1f) {
                    yield return null;
                    at2 = Calc.Approach(at2, 1f, 0.5f * Engine.DeltaTime);
                    percent = 1f - Ease.SineIn(at2);
                    Vector2 position = Vector2.Lerp(target, start, Ease.SineIn(at2));
                    MoveTo(position);
                }
                StopPlayerRunIntoAnimation = true;
                StartShaking(0.2f);
                streetlight.SetAnimationFrame(1);
                yield return 0.5f;
            }
        }
    }

}
