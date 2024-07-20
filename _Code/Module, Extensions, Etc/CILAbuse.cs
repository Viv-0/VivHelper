using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using MonoMod.RuntimeDetour;
using Celeste.Mod;
using System.Reflection;
using YamlDotNet.Core;
using MonoMod.Utils.Cil;

namespace VivHelper.Module__Extensions__Etc {
    internal static class CILAbuse {


        // This instruction is specifically for retrieving sets of IL code with hooks from some method, and then storing it in another method.
        public static void LoadIL() {
        }

        /// <summary>
        /// Given an ILContext, outputs a Delegate containing the code from the given ILContext between the InstructionOffsets `start` and `end`, inclusive of any hooks that were patched in.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="codeInstructions"></param>
        /// <param name="delegate"></param>
        public static void GetDelegateFromILBlock<TDelegate>(MethodBase method, Predicate<ILCursor> start, Predicate<ILCursor> end, List<Instruction> codeInstructions, ref TDelegate @delegate) where TDelegate : Delegate {
            if (method == null) {
                throw new ArgumentNullException(nameof(method));
            } else if (codeInstructions == null) {
                throw new Exception($"`{nameof(codeInstructions)}` is null!\n`{nameof(codeInstructions)}` must be a list of instructions with 1 null instruction, which serves as the `code block` insertion point.", new ArgumentNullException(nameof(codeInstructions)));
            } else if (codeInstructions.IndexOf(null) != codeInstructions.LastIndexOf(null) && codeInstructions.IndexOf(null) != -1) {
                throw new Exception($"`{nameof(codeInstructions)}` must be a list of instructions with 1 null instruction, which serves as the `code block` insertion point. You either have 0 or more than 1.");
            }
            Mono.Cecil.Cil.MethodBody body = null; // Get MethodBody from MethodBase *with* Hooks??????? May break in Reorg?
            ILContext ctx = new ILContext(body.Method); // ????
            ILCursor cursor = new ILCursor(ctx);
            cursor.Index = 0;
            if (!end(cursor)) {
                throw new Exception("The Predicate `end` failed to complete.");
            }
            int endIndex = cursor.Index;
            cursor.Index = 0;
            if (!start(cursor)) {
                throw new Exception("The Predicate `start` failed to complete.");
            }
            DynamicMethodDefinition dmd = new DynamicMethodDefinition($"VH_{method}_segment_{cursor.Index}_{endIndex}",
                @delegate.Method.ReturnType,
                @delegate.Method.GetParameters().Select(p=>p.ParameterType).ToArray());
            ILProcessor ilpro = dmd.GetILProcessor(); // how the fuck do I add branches with a Processor this is stupid
            foreach (Instruction i0 in codeInstructions) {
                if(i0 != null) {
                    ilpro.Append(i0);
                } else {
                    while (cursor.Index < endIndex) {
                        ilpro.Append(cursor.Next); // this breaks on branches, how the fuck do i add branches with a Processor
                        cursor.Index++;
                    }
                }
            }
            @delegate = (TDelegate)dmd.Generate().CreateDelegate(typeof(TDelegate));
        }
        private static void NOTAHOOK(ILContext il) {
            DynamicMethodDefinition dmd = new DynamicMethodDefinition("VivHelper._playerwjc_yieldnum", typeof(int), new Type[] { typeof(Player), typeof(int) });
            var gen = dmd.GetILProcessor();
            foreach (VariableDefinition v in il.Body.Variables) {
                gen.Body.Variables.Add(new VariableDefinition(v.VariableType)); //deepcopy
            }
            ILCursor cursor = new(il);
            List<Instruction> instrs = new List<Instruction>();
            cursor.Index = 0;
            while (!((cursor.Previous?.MatchLdarg(0) ?? false) && (cursor.Previous?.Previous?.MatchStloc(0) ?? false) && (cursor.Previous?.Previous?.Previous?.MatchLdcI4(5) ?? false))) {
                gen.Append(cursor.Next);
                cursor.Index++;
            }
            gen.Emit(OpCodes.Pop);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            VivHelper.player_WallJumpCheck_getNum = (Func<Player, int, int>) dmd.Generate().CreateDelegate<Func<Player, int, int>>();
        }

        // NOT WORKING
    }
}