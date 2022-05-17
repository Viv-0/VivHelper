using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.VivHelper;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/PlayerIndicator")]
    public class PlayerIndicatorTrigger : Trigger {
        public bool state;
        public bool revert;
        public bool prevState;
        public PlayerIndicatorTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            this.state = data.Bool("state");
            this.revert = data.Bool("revertOnLeave");
        }

        public override void OnEnter(Player player) {
            prevState = VivHelperModule.Session.ShowIndicator;
            VivHelperModule.Session.ShowIndicator = state;
        }

        public override void OnLeave(Player player) {
            if (revert)
                VivHelperModule.Session.ShowIndicator = prevState;
        }
    }
}
