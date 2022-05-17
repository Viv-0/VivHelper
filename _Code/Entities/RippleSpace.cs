using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;


namespace VivHelper.Entities {
    [CustomEntity("VivHelper/RippleSpace")]
    public class RippleSpace : Entity {
        public Color color;
        public RippleSpace(EntityData data, Vector2 offset) : base(data.Position + offset) {
            base.Collider = new Hitbox(data.Width, data.Height);
            color = Calc.HexToColor("8080" + Calc.Clamp(data.Int("RippleRate", 128), 0, 256).ToString("X"));
            Add(new DisplacementRenderHook(Ripple));
        }

        public void Ripple() {
            Draw.Rect(base.X, base.Y, base.Width, base.Height, color);
        }
    }
}
