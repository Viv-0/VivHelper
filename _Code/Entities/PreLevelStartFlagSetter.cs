using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/EarlyFlagSetter")]
    public class PreLevelStartFlagSetter : Entity {
        private string flag;

        private bool state;
        public PreLevelStartFlagSetter(EntityData data, Vector2 offset) : base() {
            flag = data.Attr("flag");
            state = data.Bool("state");
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            (scene as Level)?.Session.SetFlag(flag, state);
        }

        public override void Update() { RemoveSelf(); }
    }
}
