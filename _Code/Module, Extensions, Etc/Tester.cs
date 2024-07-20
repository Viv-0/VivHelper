using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using VivHelper.Entities;

namespace VivHelper {
    internal static class Tester {

        private static FieldInfo trailman_buffer = typeof(TrailManager).GetField("buffer", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Load() {
            //IL.Celeste.TrailManager.Snapshot.Render += Snapshot_Render;
            //IL.Celeste.HudRenderer.RenderContent += HudRenderer_RenderContent;
        }
        public static void Unload() {
            //IL.Celeste.TrailManager.Snapshot.Render -= Snapshot_Render;
        }

        private static void Snapshot_Render(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if(cursor.TryGotoNext(MoveType.Before, i => i.MatchCall(typeof(Vector2).GetProperty("One").GetGetMethod()))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(TrailManager.Snapshot).GetField("Sprite", BindingFlags.Public | BindingFlags.Instance));
                cursor.EmitDelegate<Func<Vector2, Image, Vector2>>((v, i) => i.Origin);
            }
        }
        private static void HudRenderer_RenderContent(ILContext il) {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.Before, i => i.MatchCall<HiresRenderer>("EndRender"))) {
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Action<Scene>>(scene => {
                    if (scene is not Level)
                        return;
                    if(scene.Tracker.TryGetEntity<TrailManager>(out var man)) {
                        VirtualRenderTarget buffer = (VirtualRenderTarget)trailman_buffer.GetValue(man);
                        if (buffer != null) {
                            Draw.SpriteBatch.Draw(buffer, Vector2.Zero, Color.White);
                        }
                    }
                });
            }
        }
    }
}
