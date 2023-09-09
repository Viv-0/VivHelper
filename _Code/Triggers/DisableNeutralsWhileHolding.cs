using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/DisableNeutralsOnHoldableTrigger")]
    internal class DisableNeutralsWhileHoldingTrigger : Trigger {

        public bool state;
        public bool RevertOnLeave;
        public bool oldState;

        public DisableNeutralsWhileHoldingTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            state = data.Bool("state");
            RevertOnLeave = data.Bool("RevertOnLeave");
        }
        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (RevertOnLeave) {
                oldState = VivHelperModule.Session.DisableNeutralsOnHoldable;
            }
            VivHelperModule.Session.DisableNeutralsOnHoldable = state;
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);
            if (RevertOnLeave) {
                VivHelperModule.Session.DisableNeutralsOnHoldable = oldState;
            }
        }
    }
}
