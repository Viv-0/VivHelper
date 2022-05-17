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

        public InstantLockingCameraTrigger(EntityData data, Vector2 offset, EntityID eid) : base(data, offset) {
            id = eid;
            Persistence = data.Enum<TriggerPersistence>("persistence");
            State = data.Bool("state");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            prevValue = VivHelperModule.Session.lockCamera;
            VivHelperModule.Session.lockCamera = State ? -1 : 0;
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);
            switch (Persistence) {
                case TriggerPersistence.Default:
                    VivHelperModule.Session.lockCamera = prevValue;
                    break;
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
