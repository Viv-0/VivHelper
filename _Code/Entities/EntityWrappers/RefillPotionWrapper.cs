using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace VivHelper.Entities {
    public class RefillPotionWrapper : Entity {

        DynamicData dynRefill;
        bool isRefillSubclass;

        string typeName;
        string spriteVarname;
    }
}
