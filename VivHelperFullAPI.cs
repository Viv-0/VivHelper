using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.ModInterop;

namespace ShrimpHelper {
    [ModImportName("VivHelperAPI")]
    public static class VivHelperAPI {
        public static Action<MethodInfo, bool> SimpleEntityMuteMethod;
    }
}