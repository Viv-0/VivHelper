using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Triggers {
    [TrackedAs(typeof(Trigger))]
    [CustomEntity("VivHelper/ReenableableCameraTargetTrigger")]
    class MultiflagCameraTargetTrigger : CameraTargetTrigger {
        public string[] flags;
        private Level level;
        public MultiflagCameraTargetTrigger(EntityData data, Vector2 offset, string[] flagArray = null) : base(data, offset) {
            if (flagArray != null) { flags = flagArray; } else if (data.Attr("ComplexFlagData", "") == "") { flags = new string[1]; flags[0] = data.Attr("SingleFlag", ""); } else { flags = data.Attr("ComplexFlagData", "").Split(','); }
        }

        public override void Awake(Scene scene) { base.Awake(scene); level = SceneAs<Level>(); }

        public override void OnStay(Player player) {
            if (VivHelperModule.OldGetFlags(level, flags, "and")) {
                base.OnStay(player);
            }
        }
    }
}
