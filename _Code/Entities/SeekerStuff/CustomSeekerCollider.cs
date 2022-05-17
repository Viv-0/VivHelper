using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {
    [Tracked(false)]
    public class CustomSeekerCollider : Component {
        public Action<CustomSeeker> OnCollide;

        public Collider Collider;

        public CustomSeekerCollider(Action<CustomSeeker> onCollide, Collider collider = null)
            : base(active: false, visible: false) {
            this.OnCollide = onCollide;
            Collider = null;
        }

        public void Check(CustomSeeker seeker) {
            if (OnCollide != null) {
                Collider collider = base.Entity.Collider;
                if (Collider != null) {
                    base.Entity.Collider = Collider;
                }
                if (seeker.CollideCheck(base.Entity)) {
                    OnCollide(seeker);
                }
                base.Entity.Collider = collider;
            }
        }
    }
}
