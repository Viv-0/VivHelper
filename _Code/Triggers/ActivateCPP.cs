using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using VivHelper.Entities;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/ActivateCPP","VivHelper/ActivateCPPTrigger")]
    public class ActivateCPP : Trigger {
        private enum Modes {
            OnPlayerEnter,
            OnPlayerLeave,
            OnLevelStart,
            WhilePlayerInside
        }
        public string[] IDs;
        public bool state;
        private bool onlyOnce;
        private Modes mode;
        private bool triggered;

        public ActivateCPP(EntityData e, Vector2 offset) : base(e, offset) {
            IDs = e.Attr("CPPID", "").Split(',');
            state = e.Bool("state", true);
            onlyOnce = e.Bool("onlyOnce");
            mode = e.Enum<Modes>("mode", Modes.OnPlayerEnter);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (mode == Modes.OnLevelStart) {
                Trigger();
            }
        }

        public override void OnEnter(Player player) {
            if (mode == Modes.OnPlayerEnter || mode == Modes.WhilePlayerInside) {
                Trigger();
            }
        }

        public override void OnLeave(Player player) {
            if (mode == Modes.OnPlayerLeave) {
                Trigger();
            }
            if (mode == Modes.WhilePlayerInside) {
                Detrigger();
            }
        }

        private void Trigger() {
            if (!triggered) {
                foreach (string s in IDs) {
                    foreach (CustomPlayerPlayback cpp in SceneAs<Level>().Tracker.GetEntities<CustomPlayerPlayback>()) {
                        if (cpp.customID == s) { cpp.active = state; if (!state) cpp.Restart(); }
                    }
                }
                if (onlyOnce) {
                    triggered = true;
                }
            }
        }

        private void Detrigger() {
            foreach (string s in IDs) {
                foreach (CustomPlayerPlayback cpp in SceneAs<Level>().Tracker.GetEntities<CustomPlayerPlayback>()) {
                    if (cpp.customID == s) { cpp.active = !state; if (state) cpp.Restart(); }
                }
            }
        }

    }
}
