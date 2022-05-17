using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;
using FMOD.Studio;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CornerBoostSwapBlock")]
    [Tracked(false)]
    public class CornerBoostSwapBlock : SwapBlock {
        public CornerBoostSwapBlock(EntityData e, Vector2 o) : base(e, o) { Add(new SolidModifierComponent(e.Bool("PerfectCornerBoost", false) ? 2 : 1, false, false)); }
    }

}
