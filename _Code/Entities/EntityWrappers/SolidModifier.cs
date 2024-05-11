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
    [CustomEntity("VivHelper/SolidModifier","VivHelper/SolidModifier2")]
    public class SolidModifier : Entity {
        //Types = exact type, assignableTypes = IsAssignableFrom
        public List<Type> Types, assignableTypes;
        public bool bufferClimbJumpTrigger;
        public bool touchTrigger;
        public bool bottomTouch;
        public int cornerboost;
        public bool retainMomentumThroughCornerBoost;

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
            if(data.Name == "VivHelper/SolidModifier2") {
                cornerboost = -data.Int("cornerBoostBlock", 0);
                retainMomentumThroughCornerBoost = cornerboost == 2 || data.Bool("RetainWallSpeed", true);

            } else {
                cornerboost = data.Int("CornerBoostBlock", 0);
            }
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
                if (t == null) { throw new Exception("Please report this to Viv on Discord @vividescence asap, thank you"); } //Added this get because of a very bizarre crash that occurred once ever.
                if (VivHelper.MatchTypeFromTypeSet(t, Types, assignableTypes)) //Added the count check because of a weird bug.
                {
                    if (solid is SolidTiles st &&
                        VivHelper.GridRectIntersection(st.Grid, new Rectangle((int) Top, (int) Left, (int) Width, (int) Height), out Grid overlap, out Rectangle scope)) {
                        // Get the size of the overlapping rectangles and produce a Solid object of that size at that point
                        Solid output = new Solid(new Vector2(scope.X, scope.Y), scope.Width, scope.Height, true); 
                        output.Collider = overlap;
                        output.AddOrAddToSolidModifierComponent(new SolidModifierComponent(cornerboost, bufferClimbJumpTrigger, touchTrigger, bottomTouch));
                        scene.Add(output);
                    } else {
                        solid.AddOrAddToSolidModifierComponent(new SolidModifierComponent(cornerboost, bufferClimbJumpTrigger, touchTrigger, bottomTouch));
                    }
                    if (!all)
                        break;
                }
            }
        }
    }
    public class SoftSolidTiles : SolidTiles {
        // This is *solely* used as a fix for CornerBoostBlocks on Solid Tiles
        public SoftSolidTiles(Vector2 position, Grid grid) : base(position, new VirtualMap<char>(new char[grid.CellsX, grid.CellsY], '0')) {
            Grid = grid;
            base.Collider = grid;
            Visible = false; // Pray noone actually bothers to force Visibility on this
            Tag -= Tags.Global;
        }
    }
}
