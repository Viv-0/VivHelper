using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Module__Extensions__Etc.Helpers {
    // Why is this here? Basically, System.Diagnostics.TraceInternal.WriteLine doesn't actually write to the Console output and if some 3rd party application correctly uses TraceInternal, we still need a debugging output.
    // Namely, this occurs with KeraLua, as of right now, but can expand to other 3rd parties, and ... while I think this might be beneficial to hook in the Celeste.dll patch, I am lazy~.
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
