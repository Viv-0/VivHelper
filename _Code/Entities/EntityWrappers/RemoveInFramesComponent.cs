using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod;

namespace VivHelper.Entities {
    public class RemoveInFramesComponent : Component {
        int frames;
        int count;

        public RemoveInFramesComponent(int frames) : base(true, false) {
            count = Math.Max(1, frames);
        }

        public override void Update() {
            if (count < 1 && Entity != null) {
                Entity.RemoveSelf();
            }
        }


    }
}
