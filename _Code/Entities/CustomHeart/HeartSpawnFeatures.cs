using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;

namespace VivHelper.Entities {
    public class LevelUpOrb : Entity {
        public Image Sprite;

        public BloomPoint Bloom;

        private float ease;

        public Vector2 Target;

        public Coroutine Routine;

        public float Ease {
            get {
                return ease;
            }
            set {
                ease = value;
                Sprite.Scale = Vector2.One * ease;
                Bloom.Alpha = ease;
            }
        }

        public LevelUpOrb(Vector2 position, Color color)
            : base(position) {
            Add(Sprite = new Image(GFX.Game["characters/badeline/orb"]));
            Add(Bloom = new BloomPoint(0f, 32f));
            Add(Routine = new Coroutine(FloatRoutine()));
            Sprite.CenterOrigin();
            base.Depth = -10001;
        }

        public IEnumerator FloatRoutine() {
            Vector2 speed = Vector2.Zero;
            Ease = 0.2f;
            while (true) {
                Vector2 target = Target + Calc.AngleToVector(Calc.Random.NextFloat(Consts.TAU), 16f + Calc.Random.NextFloat(40f));
                float reset = 0f;
                while (reset < 1f && (target - Position).Length() > 8f) {
                    Vector2 value = (target - Position).SafeNormalize();
                    speed += value * 420f * Engine.DeltaTime;
                    if (speed.Length() > 90f) {
                        speed = speed.SafeNormalize(90f);
                    }
                    Position += speed * Engine.DeltaTime;
                    reset += Engine.DeltaTime;
                    Ease = Calc.Approach(Ease, 1f, Engine.DeltaTime * 4f);
                    yield return null;
                }
            }
        }

        public IEnumerator CircleRoutine(float offset) {
            Vector2 from = Position;
            float ease = 0f;
            while (true) {
                float angleRadians = Scene.TimeActive * 2f + offset;
                Vector2 value = Target + Calc.AngleToVector(angleRadians, 24f);
                ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 2f);
                Position = from + (value - from) * Monocle.Ease.CubeInOut(ease);
                yield return null;
            }
        }

        public IEnumerator AbsorbRoutine() {
            Vector2 from = Position;
            Vector2 to = Target;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime) {
                float num = Monocle.Ease.BigBackIn(p);
                Position = from + (to - from) * num;
                Ease = 0.2f + (1f - num) * 0.8f;
                yield return null;
            }
        }
    }
}
