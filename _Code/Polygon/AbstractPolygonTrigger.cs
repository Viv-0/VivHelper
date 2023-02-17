using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using VivHelper.Colliders;

namespace VivHelper.Polygon {
    public abstract class AbstractPolygonTrigger : Trigger {
        internal static float Percentage(float val, float min, float max) => (val - min) / (max - min);
        /// <summary>
        /// Gets the where the player is relative to the bounding box if the player is inside. *CAN INCLUDE NEGATIVE VALUES*
        /// </summary>
        /// <param name="playerCenter"></param>
        /// <returns></returns>
        protected Vector2? GetPercentageOfBoundingBox(Vector2 playerCenter) => PlayerIsInside ? new Vector2(Percentage(playerCenter.X, Collider.AbsoluteLeft, Collider.AbsoluteRight), Percentage(playerCenter.Y, Collider.AbsoluteTop, Collider.AbsoluteBottom)) : null;
        protected Vector2? GetPercentageOfBoundingBox_Safe(Vector2 playerCenter) {
            Vector2? v = GetPercentageOfBoundingBox(playerCenter);
            if (v == null)
                return null;
            return Vector2.Clamp(v.Value, Vector2.Zero, Vector2.One);
        }

        protected Vector2 TriggerPoint; //The value set where the trigger is triggered
        protected bool onlyOnce;

        /// <summary>
        /// Creates an abstract Polygonal collider, PolygonalTriggers should extend this class.
        /// </summary>
        public AbstractPolygonTrigger(EntityData data, Vector2 offset) : base(data, offset) {

            onlyOnce = data.Bool("oneUse", false);
            Collider = new PolygonCollider(data.NodesOffset(offset), this, true);
        }

        public override void DebugRender(Camera camera) {

        }
    }
}
