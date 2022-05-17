using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/InfDashTrigger")]
    public class InfiniteDashTrigger : Trigger {
        private Vector2[] particles = null;

        public InfiniteDashTrigger(EntityData data, Vector2 offset)
           : base(data, offset) {

        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            player.Dashes = 2; //I know that it says infinite but with OnStay you can't dash twice in one frame so we're fine
        }
        public override void OnStay(Player player) {
            base.OnStay(player);
            player.Dashes = 2;
        }
        public override void OnLeave(Player player) {
            base.OnLeave(player);
            player.Dashes = player.Inventory.Dashes;
        }
    }
}
