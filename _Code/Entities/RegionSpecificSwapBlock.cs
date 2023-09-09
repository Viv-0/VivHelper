using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/RegionalSwapBlock")]
    public class RegionalSwapBlock : SwapBlock {
        public RegionalSwapBlock(EntityData data, Vector2 offset) : base(data, offset) {
            DashListener dl = Get<DashListener>();
            Action<Vector2> oldOnDash = dl.OnDash;
            dl.OnDash = delegate (Vector2 value) {
                if(VivHelper.TryGetAlivePlayer(out Player player) && player.CollideCheck<SwapBlockRegion>()) {
                    oldOnDash(value);
                }
            };
        }
    }
    [CustomEntity("VivHelper/SwapBlockRegion")]
    public class SwapBlockRegion : Entity {
        public SwapBlockRegion(EntityData data, Vector2 offset) :base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
        }
    }
}
