using Celeste;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {
    public class CustomPauseMenuHeader {/*
        private static IDetour pauseHook = null;

        public static void Load() {
            pauseHook = new ILHook(typeof(Level).GetMethod("orig_Pause"), hookPaused);
        }

        public static void Unload() {
            pauseHook?.Dispose();
        }

        private static void hookPaused(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if(cursor.TryGotoNext(MoveType.After, i=>i.MatchLdstr("menu_pause_title"), j=>j.MatchLdnull(), k=>k.MatchCall(typeof(Dialog).GetMethod("Clean", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Call, typeof(CustomPauseMenuHeader).GetMethod("ReplaceTitle"));
            }
        }

        public static string ReplaceTitle(string prev, Level level) {

        }*/
    }
}
