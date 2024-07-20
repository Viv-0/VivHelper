using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Triggers {
    public class AmbienceParamTrigger : Trigger {

        public string Parameter;
        public float Value;
        public AmbienceParamTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            Parameter = data.Attr("parameter");
            Value = data.Float("value");
        }

        public override void OnEnter(Player player) {
            Audio.CurrentAmbienceEventInstance.setParameterValue(Parameter, Value);
        }
    }
}
