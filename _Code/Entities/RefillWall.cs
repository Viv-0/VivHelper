using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;
using Celeste.Mod.VivHelper;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/RefillWall")]
    public class RefillWall : Entity {
        public static ParticleType P_Shatter = Refill.P_Shatter;

        public static ParticleType P_Regen = Refill.P_Regen;

        public static ParticleType P_Glow = Refill.P_Glow;

        public static ParticleType P_ShatterTwo = Refill.P_ShatterTwo;

        public static ParticleType P_RegenTwo = Refill.P_RegenTwo;

        public static ParticleType P_GlowTwo = Refill.P_GlowTwo;

        private BloomPoint bloom;

        private VertexLight light;

        private Level level;

        private bool twoDashes;

        private bool oneUse;

        private Image sprite;
        private Sprite altSprite; private SineWave sine;
        private bool alt;

        private ParticleType p_shatter;

        private ParticleType p_regen;

        private ParticleType p_glow;

        private float respawnTimer;

        private Vector2 shake;
        private Shaker shaker;
        private float alpha;
        private float RespawnTime;

        //Added Edges from Seeker Barrier and modified them as needed
        private class Edge {
            public RefillWall Parent;

            public bool Visible;

            public Vector2 A;

            public Vector2 B;

            public Vector2 Min;

            public Vector2 Max;

            public Vector2 Normal;

            public Vector2 Perpendicular;

            public float[] Wave;

            public float Length;

            public Edge(RefillWall parent, Vector2 a, Vector2 b) {
                Parent = parent;
                Visible = true;
                A = a;
                B = b;
                Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
                Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
                Normal = (b - a).SafeNormalize();
                Perpendicular = -Normal.Perpendicular();
                Length = (a - b).Length();
            }

            public void UpdateWave(float time) {
                if (Wave == null || (float) Wave.Length <= Length) {
                    Wave = new float[(int) Length + 2];
                }
                for (int i = 0; (float) i <= Length; i++) {
                    Wave[i] = GetWaveAt(time, i, Length);
                }
            }

            private float GetWaveAt(float offset, float along, float length) {
                if (along <= 1f || along >= length - 1f) {
                    return 0f;
                }
                float num = offset + along * 0.25f;
                float num2 = (float) (Math.Sin(num) * 2.0 + Math.Sin(num * 0.25f));
                return (1f + num2 * Ease.SineInOut(Calc.YoYo(along / length)));
            }

            public bool InView(ref Rectangle view) {
                if ((float) view.Left < Parent.X + Max.X && (float) view.Right > Parent.X + Min.X && (float) view.Top < Parent.Y + Max.Y) {
                    return (float) view.Bottom > Parent.Y + Min.Y;
                }
                return false;
            }
        }


        private enum ColorStates { WhiteFlash, Normal, Outline }
        private ColorStates colorStates;

        public RefillWall(Vector2 position, bool twoDashes, bool oneUse)
            : base(position) {

            Add(new PlayerCollider(OnPlayer));
            this.twoDashes = twoDashes;
            this.oneUse = oneUse;
            if (twoDashes) {
                p_shatter = P_ShatterTwo;
                p_regen = P_RegenTwo;
                p_glow = P_GlowTwo;
            } else {
                p_shatter = P_Shatter;
                p_regen = P_Regen;
                p_glow = P_Glow;
            }


            Add(new MirrorReflection());
            colorStates = ColorStates.Normal;
            base.Depth = 100;
        }

        public RefillWall(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("twoDashes"), data.Bool("oneUse")) {
            base.Collider = new Hitbox(data.Width, data.Height);
            alpha = data.Float("Alpha", 0f);
            alt = data.Bool("UseFullSprite", false);
            RespawnTime = data.Float("RespawnTime", 2.5f);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
            if (alt) {
                string q = twoDashes ? "objects/refillTwo/" : "objects/refill/";
                altSprite = new Sprite(GFX.Game, q);
                altSprite.AddLoop("idle", "idle", 0.1f);
                altSprite.AddLoop("outline", 0.05f, GFX.Game[q + "outline"]);
                altSprite.Add("flash", "flash", 0.05f, "outline");
                altSprite.CenterOrigin();
                base.Add(this.sine = new SineWave(0.6f, 0f));
                altSprite.Position = Position + new Vector2(Width / 2 - 10, Height / 2 - 10);
            } else {
                string t = twoDashes ? "objects/refillTwo/idle00" : "objects/refill/idle00";
                sprite = new Image(GFX.Game[t]);
                sprite.Position = this.Position + new Vector2(Width / 2 - sprite.Texture.Width / 2, Height / 2 - sprite.Texture.Height / 2);
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
        }

        public override void Update() {
            base.Update();
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f) {
                    Respawn();
                }
            } else {
                if (Scene.OnRawInterval(1 / 60))
                    level.ParticlesFG.Emit(p_glow, 1, Center, new Vector2(Width / 2, Height / 2));
            }


        }

        private void Respawn() {
            if (!Collidable) {
                Collidable = true;
                colorStates = ColorStates.Normal;
                base.Depth = 100;
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(p_regen, 16, Center, Vector2.One * 2f);
            }
        }

        public override void Render() {
            Camera camera = SceneAs<Level>().Camera;
            if (base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom) {
                return;
            }
            Color color = ColorDetermine(true);
            Color color2 = ColorDetermine(false);
            //			Draw.Rect(Position - Vector2.One * 4 * sin.Value, Width + 8 * sin.Value, Height + 8 * sin.Value, color);
            if (respawnTimer > 0) {
                int i;
                for (i = 0; i < Width; i += 8) {
                    Draw.Line(TopLeft + Vector2.UnitX * (i + 2), TopLeft + Vector2.UnitX * (i + 6), color2);
                    Draw.Line(BottomLeft + Vector2.UnitX * (i + 2), BottomLeft + Vector2.UnitX * (i + 6), color2);
                }
                for (i = 0; i < Height; i += 8) {
                    Draw.Line(TopLeft + Vector2.UnitY * (i + 2), TopLeft + Vector2.UnitY * (i + 6), color2);
                    Draw.Line(TopRight + Vector2.UnitY * (i + 2), TopRight + Vector2.UnitY * (i + 6), color2);
                }
                if (!alt) {
                    sprite.Color = Color.White * 0.25f;
                }
            } else if (oneUse) {
                Draw.HollowRect(X - 1, Y - 1, Width + 2, Height + 2, color2 * 0.6f);
                int i;
                for (i = 0; i < Width; i += 8) {
                    Draw.Line(TopLeft - Vector2.UnitY + Vector2.UnitX * (i + 2), TopLeft - Vector2.UnitY + Vector2.UnitX * (i + 6), color2);
                    Draw.Line(BottomLeft + Vector2.UnitX * (i + 2), BottomLeft + Vector2.UnitX * (i + 6), color2);
                }
                for (i = 0; i < Height; i += 8) {
                    Draw.Line(TopLeft + Vector2.UnitY * (i + 2), TopLeft + Vector2.UnitY * (i + 6), color2);
                    Draw.Line(TopRight + Vector2.UnitX + Vector2.UnitY * (i + 2), TopRight + Vector2.UnitX + Vector2.UnitY * (i + 6), color2);
                }
                Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, color);
                if (alt) {
                    altSprite.Render();
                } else {
                    sprite.Render();
                }
            } else {
                Draw.HollowRect(X - 1, Y - 1, Width + 2, Height + 2, color2);
                Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, color);
                if (!alt) {
                    sprite.Color = Color.White;
                }
            }
            if (alt) {
                altSprite.Render();
            } else {
                sprite.Render();
            }
        }

        private Color ColorDetermine(bool inside) {
            Color color;
            switch (colorStates) {
                case ColorStates.Normal:
                    if (inside) { color = twoDashes ? new Color(189, 64, 148, 255) * alpha : new Color(32, 128, 32, 255) * alpha; } else { color = twoDashes ? new Color(226, 104, 209, 255) * alpha : new Color(147, 189, 64, 255) * alpha; }
                    break;
                case ColorStates.WhiteFlash:
                    color = Color.White * 0f;
                    break;
                case ColorStates.Outline:
                    if (inside) { color = Color.Transparent; } else { color = new Color(128, 128, 128, 255) * (alpha * 0.25f); }
                    break;
                default:
                    throw new Exception("InvalidColorState");
            }
            return color;
        }

        private void OnPlayer(Player player) {
            if (player.UseRefill(twoDashes)) {
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));

                colorStates = ColorStates.Outline;
                respawnTimer = RespawnTime;
            }
        }

        private IEnumerator RefillRoutine(Player player) {
            Celeste.Celeste.Freeze(0.05f);
            level.Shake();
            Depth = 8999;
            yield return 0.05f;
            if (oneUse) {
                RemoveSelf();
            }
        }

        private float LineAmplitude(float seed, float index) {
            return (float) (Math.Sin((double) (seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
        }
    }
}
