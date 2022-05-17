using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using MonoMod.Cil;
using MonoMod;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using FMOD.Studio;

namespace VivHelper {
    public class FastSpotlight : ScreenWipe {
        private static VertexPositionColor[] vertexBuffer = new VertexPositionColor[768];
        private Vector2? FocusPoint = null;

        public FastSpotlight(Scene scene, bool wipeIn, Action onComplete = null)
        : base(scene, wipeIn, onComplete) {
            Player p = scene.Tracker.GetEntity<Player>();
            if (p != null) {
                FocusPoint = p.Position - new Vector2(0, 8);
                return;
            }
            foreach (Entity e in scene.Entities.getListOfEntities()) {
                if (e is PlayerDeadBody) { FocusPoint = e.Center; return; }
            }

        }

        public override void Render(Scene scene) {
            if (scene is Level level) {
                float num = (WipeIn ? Percent : (1f - Percent));
                Vector2 focusPoint = FocusPoint.HasValue ? FocusPoint.Value - level.Camera.Position : level.Camera.Position;
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode) {
                    focusPoint.X = 320f - focusPoint.X;
                }
                focusPoint.X *= 6f;
                focusPoint.Y *= 6f;
                DrawSpotlight(radius: Ease.QuadOut(num) * 1920f, position: focusPoint, color: ScreenWipe.WipeColor);
            }
        }

        public static void DrawSpotlight(Vector2 position, float radius, Color color) {
            Vector2 vector = new Vector2(1f, 0f);
            for (int i = 0; i < vertexBuffer.Length; i += 12) {
                Vector2 vector2 = Calc.AngleToVector(((float) i + 12f) / (float) vertexBuffer.Length * ((float) Math.PI * 2f), 1f);
                vertexBuffer[i].Position = new Vector3(position + vector * 5000f, 0f);
                vertexBuffer[i].Color = color;
                vertexBuffer[i + 1].Position = new Vector3(position + vector * radius, 0f);
                vertexBuffer[i + 1].Color = color;
                vertexBuffer[i + 2].Position = new Vector3(position + vector2 * radius, 0f);
                vertexBuffer[i + 2].Color = color;
                vertexBuffer[i + 3].Position = new Vector3(position + vector * 5000f, 0f);
                vertexBuffer[i + 3].Color = color;
                vertexBuffer[i + 4].Position = new Vector3(position + vector2 * 5000f, 0f);
                vertexBuffer[i + 4].Color = color;
                vertexBuffer[i + 5].Position = new Vector3(position + vector2 * radius, 0f);
                vertexBuffer[i + 5].Color = color;
                vertexBuffer[i + 6].Position = new Vector3(position + vector * radius, 0f);
                vertexBuffer[i + 6].Color = color;
                vertexBuffer[i + 7].Position = new Vector3(position + vector * (radius - 2f), 0f);
                vertexBuffer[i + 7].Color = Color.Transparent;
                vertexBuffer[i + 8].Position = new Vector3(position + vector2 * (radius - 2f), 0f);
                vertexBuffer[i + 8].Color = Color.Transparent;
                vertexBuffer[i + 9].Position = new Vector3(position + vector * radius, 0f);
                vertexBuffer[i + 9].Color = color;
                vertexBuffer[i + 10].Position = new Vector3(position + vector2 * radius, 0f);
                vertexBuffer[i + 10].Color = color;
                vertexBuffer[i + 11].Position = new Vector3(position + vector2 * (radius - 2f), 0f);
                vertexBuffer[i + 11].Color = Color.Transparent;
                vector = vector2;
            }
            ScreenWipe.DrawPrimitives(vertexBuffer);
        }
    }
}
