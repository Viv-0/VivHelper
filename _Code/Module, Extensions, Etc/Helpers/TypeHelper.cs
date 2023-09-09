using Celeste.Mod.Helpers;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public static partial class VivHelper {
        public static Dictionary<string, Type> StoredTypesByName; 

        public static Type GetType(string typeName, bool throwOnNotFound, bool store = true) {
            if (StoredTypesByName?.TryGetValue(typeName, out Type value) ?? false)
                return value;
            Type type = FakeAssembly.GetFakeEntryAssembly().GetType(typeName, throwOnNotFound); //bruh I been stupids
            if (type == null) { return null; } //if throwOnNotFound is true, then it will get here, otherwise it throws
            //At this point the type was found so we just add it to the StoredTypesByName (since this is significantly faster)
            if (store)
                StoredTypesByName?.Add(typeName, type);
            return type;
        }
        public static bool TryGetType(string typeName, out Type type, bool store = true) {
            type = GetType(typeName, false, store);
            return type != null;
        }

        /// <summary>
        /// Appends Types to a Type list for setup for Meta-entities. This is coded as a comma separated string list, with exact type matching as regular text, and assignable type matching as *(assignableType)
        /// Exact type matching means that the Types need to be equal, in other words, you cannot say that the type Solid matches to type Platform, even though Solid can be evaluated as type Platform.
        /// </summary>
        /// <param name="TypeSet">The string you need to input, generally EntityData "Types" parameter</param>
        /// <param name="exactList">The list of types that need to match exactly, not assignable</param>
        /// <param name="assignableList">The list of types that need to match or extend from the given type</param>
        public static void AppendTypesToList(string TypeSet, ref List<Type> exactList, ref List<Type> assignableList, Type minimumAssignableSubset = null) {
            if (minimumAssignableSubset == null)
                minimumAssignableSubset = typeof(Entity);
            if (exactList == null)
                exactList = new List<Type>();
            if (assignableList == null)
                assignableList = new List<Type>();
            if (string.IsNullOrWhiteSpace(TypeSet)) {
                assignableList.Add(minimumAssignableSubset);
            } else {
                string[] strings = TypeSet.Split(',');
                foreach (string _s in strings) {
                    string s = _s.Trim();
                    if (s.StartsWith("*")) {
                        Type t = VivHelper.GetType(s.Substring(1), false);
                        if (t != null && (minimumAssignableSubset?.IsAssignableFrom(t) ?? true)) {
                            assignableList.Add(t);
                        }
                    } else {
                        Type t = VivHelper.GetType(s, false);
                        if (t != null && (minimumAssignableSubset?.IsAssignableFrom(t) ?? true)) {
                            exactList.Add(t);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Appends Types to a Type array for setup for Meta-entities. This is coded as a comma separated string list, with exact type matching as regular text, and assignable type matching as *(assignableType)
        /// Exact type matching means that the Types need to be equal, in other words, you cannot say that the type Solid matches to type Platform, even though Solid can be evaluated as type Platform.
        /// </summary>
        /// <param name="TypeSet">The string you need to input, generally EntityData "Types" parameter</param>
        /// <param name="exactList">The array of types that need to match exactly, not assignable</param>
        /// <param name="assignableList">The array of types that need to match or extend from the given type</param>
        public static void AppendTypesToArray(string TypeSet, ref Type[] exactList, ref Type[] assignableList) {
            List<Type>[] lists = new List<Type>[] { exactList?.ToList<Type>() ?? null, assignableList?.ToList<Type>() ?? null };
            AppendTypesToList(TypeSet, ref lists[0], ref lists[1]);
            exactList = lists[0].ToArray();
            assignableList = lists[1].ToArray();
            return;
        }

        public static bool MatchTypeFromTypeSet(Type t, IEnumerable<Type> exactList, IEnumerable<Type> assignableList) {
            if (t == null)
                return false;
            foreach (Type u in exactList) {
                if (t == u)
                    return true;
            }
            foreach (Type v in assignableList) {
                if (v?.IsAssignableFrom(t) ?? false)
                    return true;
            }
            return false; // I have had so many fucking issues with this code.  
        }
    }
}
