using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace VivHelper.Entities {
    [CustomEntity("VivHelper/DepthSetter")]
    public class DepthSetter : Entity {
        public int newDepth;
        public bool onUpdate, earlyAwake;
        public List<Type> Types, assignableTypes;

        public DepthSetter(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            Depth = newDepth = data.Int("depth", 0);
            string q = data.Attr("Types", "");
            assignableTypes = new List<Type>();
            Types = new List<Type>();
            if (!string.IsNullOrEmpty(q)) {
                VivHelper.AppendTypesToList(q, ref Types, ref assignableTypes);
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Collidable = true;
            foreach (Entity e in scene.Entities.Where<Entity>((f) => Collide.Check(this, f))) {
                var prev = e.Collidable;
                e.Collidable = true;
                if (Collide.Check(this, e) && VivHelper.MatchTypeFromTypeSet(e.GetType(), Types, assignableTypes)) {
                    e.Depth = newDepth;
                }
                e.Collidable = prev;
            }
            RemoveSelf();
        }
    }
}
