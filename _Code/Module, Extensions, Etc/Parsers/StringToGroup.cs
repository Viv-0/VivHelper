using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public static partial class VivHelper {
        public static T[] ParseArrayFromString<T>(string @string, char groupSeparator, Func<string, T> tParser) {
            string[] s = @string.Split(groupSeparator);
            T[] ts = new T[s.Length];
            for (int i = 0; i < s.Length; i++) {
                ts[i] = tParser(s[i]);
            }
            return ts;
        }

        public static Dictionary<string, T> ParseDictFromString<T>(string @string, char groupSeparator, char keyValSeparator, Func<string, T> tParser) {
            string[] _s = @string.Split(groupSeparator);
            Dictionary<string, T> dict = new Dictionary<string, T>();
            foreach (string s in _s) {
                string[] r = s.Split(keyValSeparator);
                if (r.Length != 2)
                    throw new Exception("Invalid Key-Value Pair in string!");
                T t = tParser(r[1]);
                dict[r[0]] = t;
            }
            return dict;
        }
    }
}
