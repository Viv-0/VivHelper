using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Celeste.Mod.VivHelper;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CustomLightbeam")]
    public class CustomLightBeam : Entity {
        public static ParticleType P_Glow;

        private MTexture texture;

        private Color color = new Color(0.8f, 1f, 1f);

        private float alpha;

        public int LightWidth;

        public int LightLength;

        public float Rotation;

        public string Flag, DisableFlag;

        private float MaxAlphaDistMod = 1f;

        private float timer = Calc.Random.NextFloat(1000f);

        private bool FadeWhenNear, NoParticles;
        private string DisableParticlesOnFlag;

        public CustomLightBeam(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            base.Tag = Tags.TransitionUpdate;
            base.Depth = data.Int("Depth", -9998);
            LightWidth = data.Width;
            LightLength = data.Height;
            DisableFlag = data.Attr("DisableFlag");
            Flag = data.Attr("ChangeFlag");
            Rotation = data.Float("rotation") * ((float) Math.PI / 180f);
            color = VivHelper.ColorFix(data.Attr("Color", "ccffff")) * Calc.Clamp(data.Float("Alpha", 1f), 0f, 1f);
            texture = GFX.Game[data.Attr("Texture", "util/lightbeam")];
            FadeWhenNear = data.Bool("FadeWhenNear", true);
            NoParticles = data.Bool("NoParticles", false);
            DisableParticlesOnFlag = data.Attr("DisableParticlesFlag", "");
        }

        public override void Update() {
            timer += Engine.DeltaTime;
            Level level = base.Scene as Level;
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null && (string.IsNullOrEmpty(Flag) || level.Session.GetFlag(Flag))) {
                Vector2 value = Calc.AngleToVector(Rotation + (float) Math.PI / 2f, 1f);
                Vector2 value2 = Calc.ClosestPointOnLine(Position, Position + value * 10000f, entity.Center);
                float target = Math.Min(1f, Math.Max(0f, (value2 - Position).Length() - 8f) / (float) LightLength);
                if ((value2 - entity.Center).Length() > (float) LightWidth / 2f || !FadeWhenNear) {
                    target = 1f;
                }
                if (level.Transitioning) {
                    target = 0f;
                }
                alpha = Calc.Approach(alpha, target, Engine.DeltaTime * 4f);
            }
            if (alpha >= 0.5f && level.OnInterval(0.8f) && !NoParticles && (DisableParticlesOnFlag == "" || !(DisableParticlesOnFlag == "*" || level.Session.GetFlag(DisableParticlesOnFlag)))) {
                Vector2 vector = Calc.AngleToVector(Rotation + (float) Math.PI / 2f, 1f);
                Vector2 position = Position - vector * 4f;
                float scaleFactor = Calc.Random.Next(LightWidth - 4) + 2 - LightWidth / 2;
                position += scaleFactor * vector.Perpendicular();
                level.Particles.Emit(LightBeam.P_Glow, 1, position, Vector2.Zero, Color.Lerp(Color.White, color, 0.5f), Rotation + (float) Math.PI / 2f);
            }
            base.Update();
        }

        public override void Render() {
            if (alpha > 0f && (string.IsNullOrEmpty(DisableFlag) || !SceneAs<Level>().Session.GetFlag(DisableFlag))) {
                DrawTexture(0f, LightWidth, (float) (LightLength - 4) + (float) Math.Sin(timer * 2f) * 4f, 0.4f);
                for (int i = 0; i < LightWidth; i += 4) {
                    float num = timer + (float) i * 0.6f;
                    float num2 = 4f + (float) Math.Sin(num * 0.5f + 1.2f) * 4f;
                    float offset = (float) Math.Sin((double) ((num + (float) (i * 32)) * 0.1f) + Math.Sin(num * 0.05f + (float) i * 0.1f) * 0.25) * ((float) LightWidth / 2f - num2 / 2f);
                    float length = (float) LightLength + (float) Math.Sin(num * 0.25f) * 8f;
                    float a = 0.6f + (float) Math.Sin(num + 0.8f) * 0.3f;
                    DrawTexture(offset, num2, length, a);
                }
            }
        }

        private void DrawTexture(float offset, float width, float length, float a) {
            float rotation = Rotation + (float) Math.PI / 2f;
            if (width >= 1f) {
                texture.Draw(Position + Calc.AngleToVector(Rotation, 1f) * offset, new Vector2(0f, 0.5f), color * a * alpha, new Vector2(1f / (float) texture.Width * length, width), rotation);
            }
        }
    }
}
