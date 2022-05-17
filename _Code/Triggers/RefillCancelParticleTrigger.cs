using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/RCSParticleCountTrigger")]
    public class RefillCancelParticleTrigger : Trigger {
        public VivHelperModuleSettings.ColorRefillType set;
        private VivHelperModuleSettings.ColorRefillType prevVal;
        public bool revertOnLeave;

        public RefillCancelParticleTrigger(EntityData e, Vector2 v) : base(e, v) {
            set = e.Enum<VivHelperModuleSettings.ColorRefillType>("SetValue", VivHelperModuleSettings.ColorRefillType.Normal);
            revertOnLeave = e.Bool("RevertOnLeave");
        }

        public override void OnEnter(Player player) {
            if (revertOnLeave) {
                prevVal = VivHelperModule.Settings.DecreaseParticles;
            }
            base.OnEnter(player);
            VivHelperModule.Settings.DecreaseParticles = set;
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);
            if (revertOnLeave) {
                VivHelperModule.Settings.DecreaseParticles = prevVal;
            }
        }
    }
}
