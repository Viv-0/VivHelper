using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [Pooled]
    public class CustomCrystalDebris : Actor {
        public static ParticleType P_Dust = CrystalDebris.P_Dust;

        private Image image;

        private float percent;

        private float duration;

        private Vector2 speed;

        private Collision collideH;

        private Collision collideV;

        private Color color;

        private bool bossShatter;

        public CustomCrystalDebris()
            : base(Vector2.Zero) {
            base.Depth = -9990;
            base.Collider = new Hitbox(2f, 2f, -1f, -1f);
            collideH = OnCollideH;
            collideV = OnCollideV;
            image = new Image(GFX.Game["particles/shard"]);
            image.CenterOrigin();
            Add(image);
        }

        private void Init(Vector2 position, Color color, bool boss, Image i, float scale) {
            Position = position;
            if (image.Entity != null)
                Remove(image);
            this.image = i;
            image.CenterOrigin();
            Add(image);
            image.Color = (this.color = color);
            image.Scale = Vector2.One * scale;
            percent = 0f;
            duration = (boss ? Calc.Random.Range(0.25f, 1f) : Calc.Random.Range(1f, 2f));
            speed = Calc.AngleToVector(Calc.Random.NextAngle(), boss ? Calc.Random.Range(200, 240) : Calc.Random.Range(60, 160));
            bossShatter = boss;
        }

        public override void Update() {
            speed.X = Calc.Clamp(speed.X, -100000f, 100000f);
            speed.Y = Calc.Clamp(speed.Y, -100000f, 100000f);
            orig_Update();
        }

        public override void Render() {
            Color color = image.Color;
            image.Color = Color.Black;
            image.Position = new Vector2(-1f, 0f);
            image.Render();
            image.Position = new Vector2(0f, -1f);
            image.Render();
            image.Position = new Vector2(1f, 0f);
            image.Render();
            image.Position = new Vector2(0f, 1f);
            image.Render();
            image.Position = Vector2.Zero;
            image.Color = color;
            base.Render();
        }

        private void OnCollideH(CollisionData hit) {
            speed.X *= -0.8f;
        }

        private void OnCollideV(CollisionData hit) {
            if (bossShatter) {
                RemoveSelf();
                return;
            }
            if (Math.Sign(speed.X) != 0) {
                speed.X += Math.Sign(speed.X) * 5;
            } else {
                speed.X += Calc.Random.Choose(-1, 1) * 5;
            }
            speed.Y *= -1.2f;
        }

        public static void Burst(Vector2 position, Color color, bool boss, int count = 1, string imagePath = "particles/shard", float scale = 1f) {
            for (int i = 0; i < count; i++) {
                CustomCrystalDebris crystalDebris = Engine.Pooler.Create<CustomCrystalDebris>();
                Vector2 position2 = position + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4));
                crystalDebris.Init(position2, color, boss, new Image(GFX.Game[imagePath]), scale);
                Engine.Scene.Add(crystalDebris);
            }
        }

        public void orig_Update() {
            base.Update();
            if (percent > 1f) {
                RemoveSelf();
                return;
            }
            percent += Engine.DeltaTime / duration;
            if (!bossShatter) {
                speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 20f);
                speed.Y += 200f * Engine.DeltaTime;
            } else {
                float val = speed.Length();
                val = Calc.Approach(val, 0f, 300f * Engine.DeltaTime);
                speed = speed.SafeNormalize() * val;
            }
            if (speed.Length() > 0f) {
                image.Rotation = speed.Angle();
            }
            image.Scale = Vector2.One * Calc.ClampedMap(percent, 0.8f, 1f, 1f, 0f);
            image.Scale.X *= Calc.ClampedMap(speed.Length(), 0f, 400f, 1f, 2f);
            image.Scale.Y *= Calc.ClampedMap(speed.Length(), 0f, 400f, 1f, 0.2f);
            MoveH(speed.X * Engine.DeltaTime, collideH);
            MoveV(speed.Y * Engine.DeltaTime, collideV);
            if (base.Scene.OnInterval(0.05f)) {
                (base.Scene as Level).ParticlesFG.Emit(P_Dust, Position);
            }
        }
    }
}
