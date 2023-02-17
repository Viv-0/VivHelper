using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Microsoft.Xna.Framework;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/InstantCatchupTrigger")]
    public class InstantLockingCameraTrigger : Trigger {
        public TriggerPersistence Persistence;
        public bool State;
        public EntityID id;
        public int prevValue;
        public bool resetOnLeave;

        public InstantLockingCameraTrigger(EntityData data, Vector2 offset, EntityID eid) : base(data, offset) {
            id = eid;
            Persistence = data.Enum<TriggerPersistence>("persistence");
            resetOnLeave = data.Bool("resetOnLeave", true);
            State = data.Bool("state");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (resetOnLeave)
                prevValue = VivHelperModule.Session.lockCamera;
            VivHelperModule.Session.lockCamera = State ? -1 : 0;
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);
            if (resetOnLeave)
                VivHelperModule.Session.lockCamera = prevValue;
            switch (Persistence) {
                case TriggerPersistence.OncePerRetry:
                    RemoveSelf();
                    break;
                case TriggerPersistence.OncePerMapPlay:
                    (Scene as Level).Session.DoNotLoad.Add(id);
                    RemoveSelf();
                    break;
            }

        }
    }
}
