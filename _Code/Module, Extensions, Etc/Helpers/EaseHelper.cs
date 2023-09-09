using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper {
    public static partial class VivHelper {
        public static Dictionary<string, Ease.Easer> EaseHelper;

        /// <summary>
        /// tries to get an easer from the Easer Dictionary
        /// </summary>
        public static bool TryGetEaser(string easeName, out Ease.Easer easer) {
            easer = null;
            if (!EaseHelper.ContainsKey(easeName)) {
                return false;
            }
            easer = EaseHelper[easeName];
            return true;
        }
    }
}
