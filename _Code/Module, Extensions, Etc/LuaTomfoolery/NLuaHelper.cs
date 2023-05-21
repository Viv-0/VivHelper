using Celeste.Mod;
using Mono.Cecil.Cil;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Module__Extensions__Etc.LuaTomfoolery {
    public static class NLuaHelper {

        private static Dictionary<string, Delegate> storedStringExpressions = new();

        public static string GetFileContent(string path) {
            Stream stream = Everest.Content.Get(path)?.Stream;

            if (stream != null) {
                using (StreamReader reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }

            return null;
        }

        private static bool SafeMoveNext(this LuaCoroutine enumerator) {
            try {
                return enumerator.MoveNext();
            } catch (Exception e) {
                Logger.Log(LogLevel.Error, "VivHelper", $"Failed to resume coroutine");
                Logger.LogDetailed(e);

                return false;
            }
        }

        public static IEnumerator LuaCoroutineToIEnumerator(LuaCoroutine routine) {
            while (routine != null && routine.SafeMoveNext()) {
                if (routine.Current is double || routine.Current is long) {
                    yield return Convert.ToSingle(routine.Current);
                } else {
                    yield return routine.Current;
                }
            }

            yield return null;
        }

        public static LuaTable DictionaryToLuaTable(IDictionary<object, object> dict) {
            Lua lua = Everest.LuaLoader.Context;
            LuaTable table = lua.DoString("return {}").FirstOrDefault() as LuaTable;

            foreach (KeyValuePair<object, object> pair in dict) {
                table[pair.Key] = pair.Value;
            }

            return table;
        }

        public static LuaTable ListToLuaTable(IList list) {
            Lua lua = Everest.LuaLoader.Context;
            LuaTable table = lua.DoString("return {}").FirstOrDefault() as LuaTable;

            int ptr = 1;

            foreach (var value in list) {
                table[ptr++] = value;
            }

            return table;
        }

        // Attempt to eval the string if possible
        // Returns eval result if possible, otherwise the input string
        public static object LoadArgumentsString(string arguments) {
            Lua lua = Everest.LuaLoader.Context;

            try {
                object[] results = lua.DoString("return " + arguments);

                if (results.Length == 1) {
                    object result = results.FirstOrDefault();

                    return result ?? arguments;
                } else {
                    return ListToLuaTable(results);
                }
            } catch {
                return arguments;
            }
        }

        /// <summary>
        /// Takes simple lua statements and converts them into LuaFunctions, then converts them to C# delegates.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="argsByName"></param>
        /// <returns></returns>
        public static void CreateSimpleExpressionFromLua(string expr, Dictionary<string, Type> argsByName) {
            
        }
    }
}
