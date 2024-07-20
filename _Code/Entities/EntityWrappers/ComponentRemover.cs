using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/ComponentRemover")]
    public class ComponentRemover : Entity {


        public List<Type> Types, assignableTypes;
        public string ComponentName;

        public ComponentRemover(EntityData data, Vector2 offset) {
            string q = data.Attr("Types", "");
            assignableTypes = new List<Type>();
            Types = new List<Type>();
            if (!string.IsNullOrEmpty(q)) {
                VivHelper.AppendTypesToList(q, ref Types, ref assignableTypes);
            }
            ComponentName = data.Attr("componentName", "VertexLight");
            Depth = int.MaxValue;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Collidable = true;
            foreach (Entity e in scene.Entities.Where<Entity>((f) => {
                var prev = f.Collidable;
                f.Collidable = true;
                var ret = Collide.Check(this, f);
                f.Collidable = prev;
                return ret;
            })) {
                if (VivHelper.MatchTypeFromTypeSet(e.GetType(), Types, assignableTypes)) {
                    foreach (Component c in e.Components) {
                        if(c.GetType().Name == ComponentName || c.GetType().FullName == ComponentName) {
                            e.Remove(c);
                        }
                    }
                }
            }
            RemoveSelf();
        }
    }
}
