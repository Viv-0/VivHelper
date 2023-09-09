using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {

    [Tracked]
    public abstract class LightRegion : Entity {
        public static void Load() { }// => IL.Celeste.LightingRenderer.BeforeRender += InfluenceLightTarget;
        public static void Unload() { }// => IL.Celeste.LightingRenderer.BeforeRender -= InfluenceLightTarget;

        private static void InfluenceLightTarget(ILContext il) {
            ILCursor cursor = new(il);
            if(cursor.TryGotoNext(MoveType.After, i => i.MatchCallvirt<GraphicsDevice>("Clear"))) {
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Ldc_I4_1);
                cursor.Emit(OpCodes.Call, typeof(LightRegion).GetMethod("DrawLightsToTarget"));
            }
            if(cursor.TryGotoNext(MoveType.Before, i => i.MatchRet())) {
                // At this point, the RenderTarget is still set to Light, so we can add additional draw
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Ldc_I4_0);
                cursor.Emit(OpCodes.Call, typeof(LightRegion).GetMethod("DrawLightsToTarget"));
            }

        }

        private static void DrawLightsToTarget(Scene scene, bool blurred) {
            if (!scene.Tracker.TryGetEntities(typeof(LightRegion), out var lights) || !(scene is Level level))
                return;
            List<VertexPositionColor> vs = new();
            GameplayRenderer.Begin();
            Rectangle bounds = Rectangle.Empty;
            foreach(LightRegion light in lights) {
                if (blurred == light.Blur && Collide.CheckRect(light, level.Camera.Viewport.Bounds)) {
                    bounds = Rectangle.Union(bounds, light.Collider.Bounds); // This assumes that the Rendering data is inside the bounds of the Collider, which is just a rule I'm arbitrarily setting.
                    light.RenderLight(level, ref vs);
                }
            }
            if (vs.Count > 0) {
                ScreenWipe.DrawPrimitives(vs.ToArray());
            }
            foreach(LightOcclude occlude in scene.Tracker.GetComponents<LightOcclude>()) {
                if (bounds.Intersects(occlude.RenderBounds)) {
                    Draw.Rect(occlude.RenderBounds, Color.Black * occlude.Alpha);
                }
            }
            GameplayRenderer.End();

        }

        public bool Blur;
        public abstract void RenderLight(Level level, ref List<VertexPositionColor> vertices);

        public LightRegion(Vector2 position) : base(position) {
        }
    }
}
