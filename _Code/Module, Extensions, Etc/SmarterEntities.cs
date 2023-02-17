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

        public static HelperEntity AllUpdateHelperEntity;

        public static Entity GetHelperEntity(Scene scene) {
            if (scene.Tracker.TryGetEntity<HelperEntity>(out var e))
                return e;
            return null;
        }
    }

    /// <summary>
    /// Simple Wrapper class used solely for determining Helper objects. Helper objects are objects that have tags referential to their use case.
    /// </summary>
    [Tracked]
    public class HelperEntity : Entity { }

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
