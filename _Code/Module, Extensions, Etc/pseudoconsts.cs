using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VivHelper.Module__Extensions__Etc {
    //Reduce memory cost by predefining values.
    internal static class pseudoconsts {
        internal static Vector2 DL = new Vector2(-1, 1);
        internal static Vector2 UR = new Vector2(1, -1);
    }
}
