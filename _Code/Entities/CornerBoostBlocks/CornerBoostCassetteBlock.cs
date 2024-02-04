using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using MonoMod;
using Microsoft.Xna.Framework;
using Celeste.Mod;

namespace VivHelper.Entities {
    [Tracked]
    public class CornerBoostCassetteBlock : CassetteBlock {

        public CornerBoostCassetteBlock(EntityData e, Vector2 v, EntityID id) : base(e, v, id) {
            Add(new SolidModifierComponent(e.Bool("PerfectCornerBoost", false) ? 2 : 1, false, false));
        }
    }
}
