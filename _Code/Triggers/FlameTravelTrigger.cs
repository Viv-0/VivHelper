using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/FlameTravelTrigger")]
    public class FlameTravelTrigger : Trigger {
        public string[] travelingFlameIDs;
        public List<int> nodeTriggerables = new List<int>();
        protected bool removeOnExit;
        protected List<TravelingFlame> trackedEntities;
        public FlameTravelTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            travelingFlameIDs = data.Attr("TravelingFlameID").Split(',');
            string[] t = data.Attr("Nodes", "-1").Split(',');
            foreach (string s in t) { nodeTriggerables.Add(int.Parse(s.Trim())); }
            removeOnExit = data.Bool("removeOnExit", false);
            trackedEntities = new List<TravelingFlame>();
        }

        public override void Added(Scene scene) {
            base.Added(scene);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Add(new Coroutine(TrackTheseDamnTravelingFlames(scene)));
        }

        public override void OnEnter(Player player) {
            foreach (TravelingFlame tf in trackedEntities) {
                if (!tf.isActive && (nodeTriggerables.Contains<int>(tf.currentNode) || nodeTriggerables.Contains<int>(-1))) {

                    tf.MoveToNextNode();
                }
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);
            if (removeOnExit) { RemoveSelf(); }
        }

        protected IEnumerator TrackTheseDamnTravelingFlames(Scene scene) {
            Level level = null;
            while (level == null) {
                level = base.Scene as Level;
                yield return Engine.RawDeltaTime;
            }
            foreach (TravelingFlame t in level.Tracker.GetEntities<TravelingFlame>()) {
                if (!t.onCycle) { if (travelingFlameIDs.Contains<string>(t.identifier)) { trackedEntities.Add(t); } }
            }
        }
    }

    [CustomEntity("VivHelper/FlameLightSwitch")]
    public class FlameLightSwitch : FlameTravelTrigger {
        public bool onoff;
        public FlameLightSwitch(EntityData data, Vector2 offset) : base(data, offset) { onoff = data.Bool("TurnOn", false); }
        public override void OnEnter(Player player) {
            foreach (TravelingFlame tf in trackedEntities) {
                if (!tf.isActive && (nodeTriggerables.Contains<int>(tf.currentNode) || nodeTriggerables.Contains<int>(-1))) {

                    tf.Lights(onoff);
                }
            }
        }
    }
}
