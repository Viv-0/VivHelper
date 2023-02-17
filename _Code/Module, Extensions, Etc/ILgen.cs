using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    internal static class ILgen {
        internal static Func<Texture, Texture, int> _texRawCompareTo = IL_texRawCompareTo();
        private static Func<Texture, Texture, int> IL_texRawCompareTo() {
            string methodName = "VivHelper._texRawCompareTo";
            DynamicMethodDefinition method = new DynamicMethodDefinition(methodName, typeof(int), new Type[] { typeof(Texture), typeof(Texture) });
            var gen = method.GetILProcessor();

            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, typeof(Texture).GetMethod("CompareTo", BindingFlags.NonPublic | BindingFlags.Instance));

            return (Func<Texture, Texture, int>) method.Generate().CreateDelegate(typeof(Func<Texture, Texture, int>));
        }
    }
}
