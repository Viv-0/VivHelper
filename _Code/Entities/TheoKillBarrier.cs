using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Utils;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/TheoKillBarrier")]
    public class TheoKillBarrier : SeekerBarrier {

        private DynData<SeekerBarrier> dyn;
        private static Color baseColor = Calc.HexToColor("40c0f0");

        public TheoKillBarrier(EntityData data, Vector2 offset) : base(data, offset) {
            dyn = new DynData<SeekerBarrier>(this);
            Active = true;
            
        }

        public override void Update() {
            base.Update();
            Collidable = true;
            if(CollideAllByComponent<Holdable>() is { } q) {
                foreach(Holdable h in q) {
                    if(h.Entity != null && h.Entity is TheoCrystal tc) {
                        tc.Die();
                    }
                }
            }
            Collidable = false;
        }

        public override void Render() {
            VivHelper.Entity_Render(this);
            foreach (Vector2 particle in dyn.Get<List<Vector2>>("particles")) {
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, baseColor * 0.5f);
            }
            if (Flashing) {
                Draw.Rect(base.Collider, Color.Lerp(Color.White, baseColor, Flash) * 0.5f);
            }
        }
    }
}
