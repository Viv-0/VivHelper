using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.VivHelper;
using MonoMod.Utils;

namespace VivHelper.Entities.BooCrystal {
    [TrackedAs(typeof(GhostBarrier))]
    public class GhostBarrier : SeekerBarrier {
        public Color color;
        private DynData<SeekerBarrier> dyn;
        public GhostBarrier(EntityData data, Vector2 offset) : base(data, offset) {
            color = VivHelper.OldColorFunction(data.Attr("Color", "Lavender"));
            dyn = new DynData<SeekerBarrier>(this);
        }



        public override void Render() {
            Color c = color * 0.5f;
            foreach (Vector2 particle in dyn.Get<List<Vector2>>("particles")) {
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
            }
            if (Flashing) {
                Draw.Rect(base.Collider, color * Flash * 0.5f);
            }
        }


    }
}
