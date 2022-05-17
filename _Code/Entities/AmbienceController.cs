using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/AmbienceParamController")]
    public class AmbienceController : Entity {
        public bool random;
        public float min, max;
        public float[][] sequence;
    }
}
