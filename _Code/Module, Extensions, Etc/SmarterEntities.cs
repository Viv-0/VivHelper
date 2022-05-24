using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace VivHelper {
    public static class HelperEntities {
        public static Entity PauseUpdateHelperEntity { get; private set; } = new Entity() { Tag = Tags.PauseUpdate | Tags.Global | Tags.Persistent };
        public static Entity FrozenUpdateHelperEntity { get; private set; } = new Entity() { Tag = Tags.FrozenUpdate | Tags.Global | Tags.Persistent };
        public static Entity TransitionUpdateHelperEntity { get; private set; } = new Entity() { Tag = Tags.TransitionUpdate | Tags.Global | Tags.Persistent };
        public static Entity AllUpdateHelperEntity { get; set; } = new Entity() { Tag = Tags.PauseUpdate | Tags.FrozenUpdate | Tags.TransitionUpdate | Tags.Global | Tags.Persistent };

    }

    public class DebugEntity : Entity {

        public DebugEntity(Vector2 position) : base(position) {
            Collider = new Hitbox(8, 8);
        }


        public override void DebugRender(Camera camera) {
            if (Collider != null) {
                Draw.HollowRect(Collider, Color.LightCyan);
            }
        }
    }

    public class RenderEntity : Entity {


        public RenderEntity() {
            Tag = Tags.Global | Tags.HUD | Tags.PauseUpdate;
            Depth = int.MaxValue;
        }

        public override void Render() {
            if (Scene is Level level) {
                Camera c = level.Camera;
                Draw.Rect(c.X - 1f, c.Y - 1f, 2 + 320 * c.Zoom, 2 + 320 * c.Zoom, Color.Black);
            }
        }
    }
}
