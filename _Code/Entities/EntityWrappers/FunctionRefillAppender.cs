using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using System.Collections;

namespace VivHelper.Entities {
    /// <summary>
    /// OLDER VERSION
    /// </summary>

    [CustomEntity("VivHelper/FunctionRefillAppender")]
    public class FunctionRefillAppender : Entity {
        public List<Type> Types, assignableTypes;
        public string audioEventName;
        public bool all;

        public DashCollisionResults? checkOnDashedForSolids;

        public Func<int, int> replaceDashes;
        public Func<float, float> replaceStamina;

        public FunctionRefillAppender(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            all = data.Bool("all");
            string q = data.Attr("Types", "");
            audioEventName = data.NoEmptyString("audioEventName");
            checkOnDashedForSolids = data.Attr("includeSolidOnDash") == "None" ? null : data.Enum<DashCollisionResults>("includeSolidOnDash");
            assignableTypes = new List<Type>();
            Types = new List<Type>();
            VivHelper.AppendTypesToList(q, ref Types, ref assignableTypes);
            SetRefillActions(data.Attr("DashesLogic"), data.Attr("StaminaLogic"));
            Depth = int.MinValue;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Collidable = true;
            foreach (PlayerCollider tie in CollideAllByComponent<PlayerCollider>()) {
                Entity entity = tie.Entity;
                if (VivHelper.MatchTypeFromTypeSet(entity.GetType(), Types, assignableTypes)) {
                    Action<Player> oldOnCollide = tie.OnCollide;
                    entity.Remove(entity.Get<PlayerCollider>());
                    entity.Add(new PlayerCollider(delegate (Player p) {
                        int dashes = p.Dashes;
                        float stamina = p.Stamina;
                        oldOnCollide(p);
                        p.Dashes = replaceDashes(dashes);
                        p.Stamina = replaceStamina(stamina);

                    }));
                    if (!all)
                        break;
                }
            }
            if(checkOnDashedForSolids != null) {
                foreach (Platform solid in CollideAll<Platform>()) {
                    if (solid.OnDashCollide != null && VivHelper.MatchTypeFromTypeSet(solid.GetType(), Types, assignableTypes)) {
                        DashCollision oldDashCollision = solid.OnDashCollide;
                        solid.OnDashCollide = (p, v) => {
                            int dashes = p.Dashes;
                            float stamina = p.Stamina;
                            var r = oldDashCollision(p, v);
                            if (r == checkOnDashedForSolids) {
                                p.Dashes = replaceDashes(dashes);
                                p.Stamina = replaceStamina(stamina);
                                if(p.Dashes > dashes) {
                                    Audio.Play(audioEventName ?? (p.Dashes > 1 ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch"), Position);
                                }
                            }
                            return r;
                        };
                        if (!all)
                            break;
                    }
                }
            }
            RemoveSelf();
        }


        public void SetRefillActions(string dashes, string stamina) {
            if (string.IsNullOrWhiteSpace(dashes) || dashes == "D")
                replaceDashes = (int i) => i;
            else {
                int b = 0;
                if (dashes[0] == '+' || dashes[0] == '-') {
                    b = dashes[0] == '+' ? 1 : -1;
                    dashes = dashes.Substring(1);
                }
                if (!IntParser(dashes, out int outDash)) {
                    replaceDashes = (int i) => i;
                } else {
                    switch (b) {
                        case -1:
                            replaceDashes = (int i) => Math.Max(0, i - outDash);
                            break;
                        case 1:
                            replaceDashes = (int i) => Math.Max(0, i + outDash);
                            break;
                        default:
                            replaceDashes = (int i) => Math.Max(0, outDash);
                            break;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(stamina) || stamina == "D")
                replaceStamina = (float i) => 110f;
            else {
                int c = 0;
                if (stamina[0] == '+' || stamina[0] == '-') {
                    c = stamina[0] == '+' ? 1 : -1;
                    stamina = stamina.Substring(1);
                }
                if (!FloatParser(stamina, out float outStam)) {
                    replaceStamina = (float i) => 110f;
                } else {
                    switch (c) {
                        case -1:
                            replaceStamina = (float i) => Math.Max(0, i - outStam);
                            break;
                        case 1:
                            replaceStamina = (float i) => Math.Max(0, i + outStam);
                            break;
                        default:
                            replaceStamina = (float i) => Math.Max(0, outStam);
                            break;
                    }
                }
            }
        }

        private bool IntParser(string number, out int k) {
            if (string.IsNullOrEmpty(number)) {
                throw new Exception("Integer was empty.");
            }
            //+ refers to addition
            if (number.Contains("+")) {
                string[] q = number.Split('+');
                int p = 0;
                for (int s = 0; s < q.Length; s++) { if (!IntParser(q[s], out int o)) { k = 0; return false; } p += o; }
                k = p;
                return true;
            }
            // * refers to multiplication
            if (number.Contains("*")) {
                string[] q = number.Split('*');
                int p = 1;
                for (int s = 0; s < q.Length; s++) { if (!IntParser(q[s], out int o)) { k = 0; return false; } p *= o; }
                k = p;
                return true;
            }
            // / refers to division
            if (number.Contains("/")) {
                string[] q = number.Split('/');
                int p = 1;
                for (int s = 0; s < q.Length; s++) { if (!IntParser(q[s], out int o)) { k = 0; return false; } if (o == 0) { k = 0; return false; } p /= o; }
                k = p;
                return true;
            }
            //if (number.Trim() == "U") { k = UseNumber; return true; }
            //if (number.Trim() == "u") { k = count; return true; }
            if (number.Trim() == "D") { k = SceneAs<Level>()?.Session?.Inventory.Dashes ?? 1; return true; }
            return int.TryParse(number.Trim(), out k);
        }

        private bool FloatParser(string number, out float k) {
            if (string.IsNullOrEmpty(number)) {
                throw new Exception("Float was empty.");
            }
            //+ refers to addition
            if (number.Contains("+")) {
                string[] q = number.Split('+');
                float p = 0;
                for (int s = 0; s < q.Length; s++) { if (!FloatParser(q[s], out float o)) { k = 0; return false; } p += o; }
                k = p;
                return true;
            }
            // * refers to multiplication
            if (number.Contains("*")) {
                string[] q = number.Split('*');
                float p = 1;
                for (int s = 0; s < q.Length; s++) { if (!FloatParser(q[s], out float o)) { k = 0; return false; } p *= o; }
                k = p;
                return true;
            }
            // / refers to division
            if (number.Contains("/")) {
                string[] q = number.Split('/');
                float p = 1;
                for (int s = 0; s < q.Length; s++) { if (!FloatParser(q[s], out float o)) { k = 0; return false; } if (o == 0) { k = 0; return false; } p /= o; }
                k = p;
                return true;
            }
            if (number.Trim() == "D") { k = 110f; return true; }
            return float.TryParse(number.Trim(), out k);
        }
    }
}
