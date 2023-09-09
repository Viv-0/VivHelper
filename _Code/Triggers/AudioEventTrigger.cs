using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Triggers {
    public class AudioEventTrigger : Trigger {
        public string audioEvent;

        public AudioEventTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            audioEvent = data.NoEmptyString("event", "event:/none");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

        }
    }
}
