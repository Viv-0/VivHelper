using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace VivHelper.Entities {
    public class RemovableStaticMover : StaticMover {
        public static void Load() {
            IL.Monocle.Entity.RemoveSelf += RemoveRemovableStaticMovers;
        }
        public static void Unload() {
            IL.Monocle.Entity.RemoveSelf -= RemoveRemovableStaticMovers;
        }
        private static void RemoveRemovableStaticMovers(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Entity>>(e => { foreach (RemovableStaticMover r in e.Components) r.Destroy(); });
        }

        public RemovableStaticMover() : base() { }

        public static void MakeStaticMoverRemovable(Entity e) {
            Component[] comps = e.Components.ToArray();
            foreach (StaticMover sm in comps) {
                e.Remove(sm);
                var rsm = new RemovableStaticMover() {
                    JumpThruChecker = sm.JumpThruChecker,
                    SolidChecker = sm.SolidChecker,
                    OnAttach = sm.OnAttach,
                    OnEnable = sm.OnEnable,
                    OnDestroy = sm.OnDestroy,
                    OnDisable = sm.OnDisable,
                    OnMove = sm.OnMove,
                    OnShake = sm.OnShake,
                    Platform = sm.Platform
                };
                e.Add(rsm);
                rsm.EntityAdded(e.Scene);


            }
        }
    }

    [CustomEntity("VivHelper/MakeStaticMoversRemovable = Load1", "VivHelper/AddRemovableStaticMover = Load2")]
    public class RemovableStaticMoverMeta : Entity {
        public static Entity Load1(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new RemovableStaticMoverMeta(entityData, offset, true);
        public static Entity Load2(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new RemovableStaticMoverMeta(entityData, offset, false);

        public List<Type> Types, assignableTypes;
        public bool all;
        public bool replace;

        public RemovableStaticMoverMeta(EntityData e, Vector2 o, bool replace) : base(e.Position + o) {
            Collider = new Hitbox(e.Width, e.Height);
            this.replace = replace;
            string q = e.Attr("Types", "");
            assignableTypes = new List<Type>();
            Types = new List<Type>();
            VivHelper.AppendTypesToList(q, ref Types, ref assignableTypes);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Collidable = true;
            if (replace) {
                foreach (Entity e in scene.Entities.Where<Entity>((f) => f.Get<StaticMover>() != null && Collide.Check(this, f))) {
                    if (VivHelper.MatchTypeFromTypeSet(e.GetType(), Types, assignableTypes)) {
                        RemovableStaticMover.MakeStaticMoverRemovable(e);
                        if (!all)
                            break;
                    }
                }
            } else {
                foreach (Entity e in scene.Entities.Where<Entity>((f) => Collide.Check(this, f))) {
                    if (VivHelper.MatchTypeFromTypeSet(e.GetType(), Types, assignableTypes)) {
                        e.Add(new RemovableStaticMover());
                        if (!all)
                            break;
                    }
                }
            }

        }

        public override void Update() {
            RemoveSelf();
        }

    }


}
