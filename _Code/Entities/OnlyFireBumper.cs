using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using MonoMod.Utils;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/EvilBumper")]
    public class OnlyFireBumper : Bumper {
        private bool wobble;
        DynData<Bumper> dyn;

        public OnlyFireBumper(EntityData data, Vector2 offset) : base(data, offset) {
            wobble = data.Bool("wobble", true);
            dyn = new DynData<Bumper>(this);
            if (!wobble)
                Position = dyn.Get<Vector2>("anchor");
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Remove(Get<CoreModeListener>());
            dyn.Set<bool>("fireMode", true);
            dyn.Get<Sprite>("sprite").Visible = false;
            dyn.Get<Sprite>("spriteEvil").Visible = true;
        }

        public override void Update() {
            base.Update();
            if (!wobble)
                Position = dyn.Get<Vector2>("anchor");
        }
    }
}
