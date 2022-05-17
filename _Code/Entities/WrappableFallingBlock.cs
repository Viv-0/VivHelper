using Monocle;
using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Collections;
using Celeste.Mod.VivHelper;

namespace VivHelper.Entities {
    [TrackedAs(typeof(FallingBlock))]
    [CustomEntity("VivHelper/WrappableFallingBlock")]
    public class WrappableFallingBlock : FallingBlock {
        public RoomWrapController wC;
        private int count;
        private List<StaticMover> staticMovers2;
        private Vector2 PrevPosition;

        public WrappableFallingBlock(EntityData data, Vector2 offset) : base(data, offset) {
            count = 1 + data.Int("maxRevolutions", -1);
            if (count == 0)
                count = int.MaxValue;

        }

        public override void Added(Scene scene) {
            base.Added(scene);
            wC = null;
            staticMovers2 = base.staticMovers;

        }

        public override void Update() {
            base.Update();
            if (wC == null) {
                wC = VivHelperModule.FindWC(base.Scene);
            } else {
                Level l = Engine.Scene as Level;
                if (VivHelperModule.OldGetFlags(l, wC.flag, "and")) {
                    if (!(count < 0)) {
                        if (base.Top > l.Bounds.Bottom - 8f + wC.playerOffsets[2] && wC.scrollB) {
                            MoveToY(l.Bounds.Top + 8f + wC.playerOffsets[0] - Height);
                            count -= 1;
                        }
                    }
                }
            }
            InLevelTeleporter ilt = CollideFirst<InLevelTeleporter>();
            if (ilt != null && ilt.enabled) {
                if (ilt.Direction == InLevelTeleporter.Directions.Down) {
                    if (Top > ilt.Top && CenterX <= ilt.Right && CenterX >= ilt.Left) {

                        ilt.pair.TeleportEntity(this, Left - ilt.Left, ilt.Direction);

                    }

                }
            }

        }
    }
}
