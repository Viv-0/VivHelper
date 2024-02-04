using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Module__Extensions__Etc.Helpers {
    internal static class Debugging {

        private static ILHook abomination;

        public static void Load() {
            abomination = new ILHook(typeof(System.Diagnostics.Debug).GetMethod("WriteLine", new Type[2] { typeof(string), typeof(object[]) }), Abomination);
        }

        public static void Unload() {
            abomination?.Dispose();
        }

        private static void Abomination(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if(cursor.TryGotoNext(i=>i.MatchCall("System.Diagnostics.TraceInternal", "WriteLine"))) {
                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[1] { typeof(string) }));
            }
        }
    }
}
