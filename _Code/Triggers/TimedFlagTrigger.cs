using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using VivHelper;
using Celeste.Mod.VivHelper;


namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/TimedFlagTrigger")]
    public class TimedFlagTrigger : Trigger {
        private enum Modes {
            OnPlayerEnter,
            OnPlayerLeave,
            OnLevelStart
        }

        private string flag;

        private bool state;

        private Modes mode;

        private bool onlyOnce;

        private int deathCount;

        private float delay;

        private bool triggered;

        public TimedFlagTrigger(EntityData data, Vector2 offset)
            : base(data, offset) {
            flag = data.Attr("flag");
            state = data.Bool("state");
            mode = data.Enum("mode", Modes.OnPlayerEnter);
            onlyOnce = data.Bool("only_once");
            deathCount = data.Int("death_count", -1);
            delay = data.Float("Delay", 0f);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (mode == Modes.OnLevelStart) {
                Add(new Coroutine(Trigger()));
            }
        }

        public override void OnEnter(Player player) {
            if (mode == Modes.OnPlayerEnter) {
                Add(new Coroutine(Trigger()));
            }
        }

        public override void OnLeave(Player player) {
            if (mode == Modes.OnPlayerLeave) {
                Add(new Coroutine(Trigger()));
            }
        }

        private IEnumerator Trigger() {
            yield return delay;
            if (!triggered && (deathCount < 0 || (base.Scene as Level).Session.DeathsInCurrentLevel == deathCount)) {
                (base.Scene as Level).Session.SetFlag(flag, state);
                if (onlyOnce) {
                    triggered = true;
                }
            }
        }
    }
}
