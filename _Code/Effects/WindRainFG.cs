using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.VivHelper;
using Celeste.Mod.Entities;

namespace VivHelper.Effects {
    public class WindRainFG : Backdrop {
        private struct Particle {
            public Vector2 Position;

            public float Speed;

            public float Rotation;

            public Vector2 Scale;

            public Color Color;

            public void Init() {
                Position = new Vector2(-32f + Calc.Random.NextFloat(384f), -32f + Calc.Random.NextFloat(244f));
                Rotation = Consts.PIover2 + Calc.Random.Range(-0.05f, 0.05f);
                Speed = Calc.Random.Range(200f, 600f);
                Scale = new Vector2(4f + (Speed - 200f) / 400f * 12f, 1f);
            }
        }

        public float Alpha = 1f;

        private float visibleFade = 1f;

        private float linearFade = 1f;

        private Particle[] particles = new Particle[240];

        public Color[] Colors;

        private Level level;

        public float windStrength;

        public Vector2 scroll;
#pragma warning disable CS0612
        public WindRainFG(Vector2 scroll, string colors, float windStrength) {
            this.Scroll = scroll;
            this.windStrength = windStrength;
            if (colors == "") this.Colors = new Color[] { Calc.HexToColor("161933") };
            else if (colors[0] == '§') {
                string[] c = colors.Substring(1).Split(',');
                this.Colors = new Color[c.Length];
                for (int i = 0; i < c.Length; i++) {
                    c[i].Trim();
                    c[i].TrimStart('#');
                    Colors[i] = VivHelper.OldColorFunction(c[i]);
                }
            } else {
                
            }
            for (int i = 0; i < particles.Length; i++) {
                particles[i].Init();
            }
            level = null;
        }

#pragma warning restore CS0612
        public override void Update(Scene scene) {
            base.Update(scene);
            bool flag = (scene as Level).Raining = IsVisible(scene as Level);
            visibleFade = Calc.Approach(visibleFade, flag ? 1 : 0, Engine.DeltaTime * (flag ? 10f : 0.25f));
            if (FadeX != null) {
                linearFade = FadeX.Value((scene as Level).Camera.X + 160f);
            }
            for (int i = 0; i < particles.Length; i++) {

                particles[i].Position += (Calc.AngleToVector(particles[i].Rotation, particles[i].Speed) + (scene as Level).Wind * windStrength) * Engine.DeltaTime;

            }
        }

        public override void Render(Scene scene) {
            if (!(Alpha <= 0f) && !(visibleFade <= 0f) && !(linearFade <= 0f)) {
                float colFade = 0.5f * Alpha * linearFade * visibleFade;
                Camera camera = (scene as Level).Camera;
                for (int i = 0; i < particles.Length; i++) {
                    float t = (float) Math.Pow((particles[i].Speed - 400) / 400, 1.1);
                    var u = Calc.Angle(Calc.AngleToVector(particles[i].Rotation, (t + 1) * 400) + (scene as Level).Wind * windStrength);
                    Vector2 position = new Vector2(mod(particles[i].Position.X - camera.X * Scroll.X, 320f), mod(particles[i].Position.Y - camera.Y * Scroll.Y, 180f));
                    Draw.Pixel.DrawCentered(position,
                                            Colors[(int) (i * mod(i * (i + 2) * Math.Abs(i - 7.5f), Math.Abs((2 * i - 1) * i - 3)) * (i + 4.5f)) % Colors.Length],
                                            particles[i].Scale,
                                            u);
                    var v = particles[i].Scale.Rotate(u);
                    if (position.Y + v.Y > 180) {
                        if (position.X + v.X > 320) {
                            Draw.Pixel.DrawCentered(new Vector2(position.X - 320, position.Y - 180),
                                                    Colors[(int) (i * mod(i * (i + 2) * Math.Abs(i - 7.5f), Math.Abs((2 * i - 1) * i - 3)) * (i + 4.5f)) % Colors.Length],
                                                    particles[i].Scale,
                                                    u);
                        } else {
                            Draw.Pixel.DrawCentered(new Vector2(position.X, position.Y - 180),
                                                    Colors[(int) (i * mod(i * (i + 2) * Math.Abs(i - 7.5f), Math.Abs((2 * i - 1) * i - 3)) * (i + 4.5f)) % Colors.Length],
                                                    particles[i].Scale,
                                                    u);
                        }
                    }
                    else if(position.X + v.X > 320) {
                        Draw.Pixel.DrawCentered(new Vector2(position.X - 320, position.Y),
                                                Colors[(int) (i * mod(i * (i + 2) * Math.Abs(i - 7.5f), Math.Abs((2 * i - 1) * i - 3)) * (i + 4.5f)) % Colors.Length],
                                                particles[i].Scale,
                                                u);
                    }

                }
            }
        }

        private float mod(float x, float m) => VivHelper.mod(x, m);
    }
}
