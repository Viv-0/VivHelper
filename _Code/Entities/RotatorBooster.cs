using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities.Boosters;

namespace VivHelper.Entities {
    public class RotatorBooster : CustomBooster {

        public RotatorBooster(EntityData data, Vector2 offset) : base(data.Position + offset) {

        }
    }
}
