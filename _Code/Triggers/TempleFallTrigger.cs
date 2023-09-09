using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/TempleFallTrigger")]
    public class TempleFallTrigger : Trigger {
        private int OverrideOffsetX = 160;

        public TempleFallTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            OverrideOffsetX = data.Int("OffsetX", 160);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            VivHelperModule.Session.OverrideTempleFallX = OverrideOffsetX;
            player.StateMachine.State = 20;
        }
    }
}
