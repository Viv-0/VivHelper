using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.VivHelper;
using System.Reflection;

namespace VivHelper.Entities {
    [TrackedAs(typeof(Glider))]
    [CustomEntity("VivHelper/WrappableGlider")]
    public class WrappableGlider : Glider {
        private Level level2;
        public bool removeOnBottom;
        public RoomWrapController wC;
        public WrappableGlider(EntityData data, Vector2 offset) : base(data, offset) {
            removeOnBottom = data.Bool("removeOnBottom", true);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level2 = SceneAs<Level>();
            wC = null;
        }

        public override void Update() {
            base.Update();
            if (wC == null) {
                wC = VivHelperModule.FindWC(base.Scene);
            } else {
                if (VivHelperModule.OldGetFlags(level2, wC.flag, "and")) {
                    if (base.Left < (float) level2.Bounds.Left + 1f + wC.playerOffsets[3] && wC.scrollL) { base.Right = (float) level2.Bounds.Right - 1f + wC.playerOffsets[1]; }
                    if (base.Right > (float) level2.Bounds.Right - 1f + wC.playerOffsets[1] && wC.scrollR) { base.Left = (float) level2.Bounds.Left + 1f + wC.playerOffsets[3]; }
                    if (base.Top < (float) level2.Bounds.Top + 2f + wC.playerOffsets[0] && wC.scrollT && wC.scrollB && !removeOnBottom) { base.Bottom = (float) level2.Bounds.Bottom - 2f + wC.playerOffsets[2]; }
                    if (base.Bottom > (float) level2.Bounds.Bottom - 2f + wC.playerOffsets[2] && wC.scrollB && !removeOnBottom) { base.Top = (float) level2.Bounds.Top + 2f + wC.playerOffsets[0]; }
                }
            }
            InLevelTeleporter ilt = CollideFirst<InLevelTeleporter>();
            if (ilt != null && ilt.enabled) {
                switch (ilt.Direction) {
                    case InLevelTeleporter.Directions.Right:
                        if (Left > ilt.Left && CenterY <= ilt.Bottom && CenterY >= ilt.Top) {
                            Speed = ilt.pair.TeleportEntityWithSpeed(this, Top - ilt.Top, Speed, ilt.Direction);

                        }
                        break;
                    case InLevelTeleporter.Directions.Left:
                        if (Right < ilt.Right && CenterY <= ilt.Bottom && CenterY >= ilt.Top) {
                            Speed = ilt.pair.TeleportEntityWithSpeed(this, ilt.Bottom - Bottom, Speed, ilt.Direction);
                        }
                        break;
                    case InLevelTeleporter.Directions.Up:
                        if (Bottom < ilt.Bottom && CenterX <= ilt.Right && CenterX >= ilt.Left) {
                            Speed = ilt.pair.TeleportEntityWithSpeed(this, ilt.Right - Right, Speed, ilt.Direction);
                        }
                        break;
                    case InLevelTeleporter.Directions.Down:
                        if (Top > ilt.Top && CenterX <= ilt.Right && CenterX >= ilt.Left) {
                            Speed = ilt.pair.TeleportEntityWithSpeed(this, Left - ilt.Left, Speed, ilt.Direction);
                        }
                        break;
                }
            }
        }
    }
}
