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
    [CustomEntity("VivHelper/DashTempleGate")]
    public class DashGate : Entity {
        public int length;
        public bool horizontal;
        public int DashLimit;
        public bool invert;
        public MTexture texture;
        public int dashCount;
        public bool enabled;
        private int textureHeight;
        private Color? laserColor;

        public DashGate(EntityData data, Vector2 offset) : base(data.Position + offset) {

            DashLimit = data.Int("DashesToClose");
            invert = data.Bool("Invert");
            texture = GFX.Game[data.NoEmptyString("TexturePath") ?? "VivHelper/entities/dashgate"];
            textureHeight = texture.Height;
            Depth = -10001;
            dashCount = 0;
            laserColor = VivHelper.ColorFixWithNull(data.Attr("BeamColor", null));
            enabled = Collidable = invert;
            Add(new DashListener(OnDash));
            if (data.Has("H")) {
                horizontal = true;
                length = data.Width;
            } else {
                length = data.Height;
            }
        }

        public void OnDash(Vector2 v) {
            if (++dashCount > DashLimit) {
                enabled = Collidable = !invert;
            }
        }

        public override void Render() {
            if (horizontal) {

            }
        }
    }
}
