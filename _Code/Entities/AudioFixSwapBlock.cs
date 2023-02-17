using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using FMOD.Studio;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/AudioFixSwapBlock")]
    [TrackedAs(typeof(SwapBlock))]
    public class AudioFixSwapBlock : SwapBlock {
        public static void Load() {
            IL.Celeste.SwapBlock.Update += SwapBlock_Update;
        }
        public static void Unload() {
            IL.Celeste.SwapBlock.Update -= SwapBlock_Update;
        }

        private static void SwapBlock_Update(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            //So this IL in Celeste's CIL code is truly galaxy brain. It stores the Previous value of Position as a stack variable,
            //since it doesn't touch it until much later, effectively preserving its value without the need for a local variable.
            //This is not at all obvious though, so be wary when hooking into here.
            if (cursor.TryGotoNext(MoveType.After, i => i.MatchLdarg(0), i2 => i2.MatchLdfld<Entity>("Position"), i3 => i3.MatchCall<Vector2>("op_Inequality"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Call, typeof(AudioFixSwapBlock).GetMethod("ModifiedCheckHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
            }
        }

        internal static bool ModifiedCheckHandler(bool @in, SwapBlock swap) {
            if (!(swap is AudioFixSwapBlock self))
                return @in;
            var lerp = self.dyn.Get<float>("lerp");
            var target = self.dyn.Get<int>("target");
            Audio.Position(self.dyn.Get<EventInstance>("moveSfx"), self.Center);
            Audio.Position(self.dyn.Get<EventInstance>("returnSfx"), self.Center);
            if (lerp == target) {
                if (target == 0) {
                    Audio.SetParameter(self.dyn.Get<EventInstance>("returnSfx"), "end", 1f);
                    Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", self.Center);
                } else {
                    Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", self.Center);
                }
            }
            return false;
        }

        public DynData<SwapBlock> dyn;

        public AudioFixSwapBlock(EntityData data, Vector2 offset) : base(data, offset) {
            dyn = new DynData<SwapBlock>(this);
        }
    }
}
