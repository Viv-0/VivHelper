using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    public class ShatterCustomSpinnerOnTouchComponent : Component {
        public ShatterCustomSpinnerOnTouchComponent() : base(true, false) {

        }

        public override void Update() {
            base.Update();
            DestroySpinners(true);
        }

        public void DestroySpinners(bool fullDestroy) {
            if (Entity?.CollideCheck<CustomSpinner>() ?? false) {
                foreach (CustomSpinner cs in Entity.CollideAll<CustomSpinner>()) {
                    if (!cs.AttachToSolid || cs.Get<StaticMover>().Platform != Entity)
                        if (fullDestroy)
                            cs.Destroy();
                        else
                            cs.RemoveSelf();
                }
            }
        }
    }
}
