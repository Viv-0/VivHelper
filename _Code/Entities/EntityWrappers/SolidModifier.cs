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
    [CustomEntity("VivHelper/SolidModifier")]
    public class SolidModifier : Entity {
        //Types = exact type, assignableTypes = IsAssignableFrom
        public List<Type> Types, assignableTypes;
        public bool bufferClimbJumpTrigger;
        public bool touchTrigger;
        public bool bottomTouch;
        public int cornerboost;

        public bool all;

        public SolidModifier(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collidable = false;
            Collider = new Hitbox(data.Width, data.Height);
            Depth = 1000000;
            string q = data.Attr("Types", "");
            Types = new List<Type>();
            assignableTypes = new List<Type>();
            if (string.IsNullOrWhiteSpace(q) || q == "*Solid") {
                assignableTypes.Add(typeof(Solid));
            } else {
                string[] strings = q.Split(',');
                foreach (string s in strings) {
                    if (s.StartsWith("*")) {
                        Type t = VivHelper.GetType(s.Substring(1), false);
                        if (t != null) {
                            assignableTypes.Add(t);
                        }
                    } else {
                        Type t = VivHelper.GetType(s, false);
                        if (t != null) {
                            Types.Add(t);
                        }
                    }
                }
            }
            cornerboost = data.Int("CornerBoostBlock", 0);
            bufferClimbJumpTrigger = data.Bool("TriggerOnBufferInput");
            switch (data.Int("TriggerOnTouch")) {
                case 0:
                    touchTrigger = false;
                    bottomTouch = false;
                    break;
                case 1:
                    touchTrigger = true;
                    bottomTouch = false;
                    break;
                case 2:
                    touchTrigger = true;
                    bottomTouch = true;
                    break;
                default:
                    touchTrigger = false;
                    bottomTouch = false;
                    break;
            }
            all = data.Bool("EntitySelect");

        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            foreach (Solid solid in CollideAll<Solid>()) {
                Type t = solid.GetType();
                if (t == null) { throw new Exception("Please report this to Viv on Discord @Viv#1113 asap, thank you"); } //Added this get because of a very bizarre crash that occurred once ever.
                if ((Types.Count > 0 && Types.Contains(t)) || (assignableTypes.Count > 0 && assignableTypes.Any((u) => t.IsAssignableFrom(u)))) //Added the count check because of a weird bug.
                {
                    solid.AddOrAddToSolidModifierComponent(new SolidModifierComponent(cornerboost, bufferClimbJumpTrigger, touchTrigger, bottomTouch));
                    if (!all)
                        break;
                }
            }
        }
    }
}
