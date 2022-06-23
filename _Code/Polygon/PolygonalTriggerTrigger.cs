using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using VivHelper.Colliders;

namespace VivHelper.Polygon {
    [CustomEntity("VivHelper/PolygonTriggerTrigger")]
    public class PolygonalTriggerTrigger : Trigger {
        public string flagToggle;
        public bool onlyOnce;
        public List<Trigger> Associators;
        public List<Type> Types, assignableTypes;

        private Vector2 triggerPoint;

        internal Vector2 percentageBoundingBox; //Handles PositionModes automatically.

        public PolygonalTriggerTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            triggerPoint = Position;
            onlyOnce = data.Bool("oneUse", false);
            var q = data.NodesOffset(offset);
            Position = PolygonCollider.GetCentroidOfNonComplexPolygon(q);
            Collider = new PolygonCollider(q, this);
            string r = data.Attr("Types", "");
            assignableTypes = new List<Type>();
            Types = new List<Type>();
            VivHelper.AppendTypesToList(r, ref Types, ref assignableTypes, typeof(Trigger));
            Collidable = false;
            Associators = new List<Trigger>(Types.Count + assignableTypes.Count);
            Visible = true;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            foreach (Entity e in scene.Entities.Where<Entity>((f) => Collide.CheckPoint(f, triggerPoint))) {
                Type t = e.GetType();
                if (Types.Contains(t) || assignableTypes.Any((u) => t.IsAssignableFrom(u)) && e.Collider.GetType() != typeof(PolygonCollider)) {
                    Associators.Add(e as Trigger);
                    e.Collidable = false;
                    break;
                }
            }
            Collidable = true;
        }

        internal static float Percentage(float val, float min, float max) => (val - min) / (max - min);

        private Vector2 GetPercentageOfBoundingBox(Vector2 playerCenter) =>
            new Vector2(Percentage(playerCenter.X, Collider.AbsoluteLeft, Collider.AbsoluteRight), Percentage(playerCenter.Y, Collider.AbsoluteTop, Collider.AbsoluteBottom));


        public override void OnEnter(Player player) {
            base.OnEnter(player);
            foreach (Trigger associator in Associators) {
                if (associator == null || associator.Scene == null || flagToggle != null && !(Scene as Level).Session.GetFlag(flagToggle))
                    continue; //Inverse check is faster

                Vector2 oldPosition = player.Position;
                Vector2 relPos = GetPercentageOfBoundingBox(player.Center);
                player.Position = associator.TopLeft + new Vector2(associator.Width * relPos.X, associator.Height * relPos.Y);
                associator.Triggered = true;
                associator.OnEnter(player);
                player.Position = oldPosition;
            }
        }

        public override void OnStay(Player player) {
            base.OnStay(player);
            foreach (Trigger associator in Associators) {
                if (associator == null || associator.Scene == null || flagToggle != null && !(Scene as Level).Session.GetFlag(flagToggle))
                    continue; //Inverse check is faster

                Vector2 oldPosition = player.Position;
                Vector2 relPos = GetPercentageOfBoundingBox(player.Center);
                player.Position = associator.TopLeft + new Vector2(associator.Width * relPos.X, associator.Height * relPos.Y);
                associator.OnStay(player);
                player.Position = oldPosition;
            }

        }

        public override void OnLeave(Player player) {
            base.OnStay(player);
            foreach (Trigger associator in Associators) {
                if (associator == null || associator.Scene == null || flagToggle != null && !(Scene as Level).Session.GetFlag(flagToggle))
                    continue; //Inverse check is faster

                Vector2 oldPosition = player.Position;
                Vector2 relPos = GetPercentageOfBoundingBox(player.Center);
                player.Position = associator.TopLeft + new Vector2(associator.Width * relPos.X, associator.Height * relPos.Y);
                associator.OnLeave(player);
                associator.Triggered = false;
                player.Position = oldPosition;
            }
        }

        public override void Render() {
            Collider.Render((Scene as Level).Camera, Color.Red);
        }
    }
}
