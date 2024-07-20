using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {
    public class BouncySpikes : CustomSpike {

        public BouncySpikes(EntityData data, Vector2 offset, DirectionPlus dir) : base(data.Position + offset, dir, GetSize(data.Height, data.Width, dir), false, false, false, true) {

        }

        protected override void OnCollide(Player player) {

        }
    }
}
