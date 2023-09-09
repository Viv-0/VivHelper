using Celeste;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Module__Extensions__Etc {
    public class DashPowerupController : Component {

        public DashReplace ActivePowerup;
        // Mutually exclusive
        public DashReplace ReadyPowerup;
        public LinkedList<DashReplace> PowerupQueue;

        public DashPowerupController(bool active, bool visible) : base(active, visible) {

        }

        public int ActivatePowerup(DashReplace powerup = null) {
            ActivePowerup = powerup;
            powerup.actionOnActivation?.Invoke(Entity as Player);
            return ActivePowerup.innerState();
        }

        public void EndActivePowerup() {
            if(ActivePowerup?.routineOnComplete != null) {
                Entity.Add(new Coroutine(ActivePowerup.routineOnComplete(Entity as Player)));
            }
            ActivePowerup = null;
        }

        public override void Update() {
            base.Update();
            if (!(VivHelperModule.Session.dashPowerupManager is DashPowerupManager m))
                return;
            bool slotEmpty = true;
            if (m.format == PowerupFormat.Queue) {
                if (PowerupQueue.Count > 0) {
                    PowerupQueue.First.Value?.updateWhenReady?.Invoke(Entity as Player);
                    slotEmpty = false;
                }
            } else {
                if (ReadyPowerup != null) {
                    ReadyPowerup?.updateWhenReady?.Invoke(Entity as Player);
                    slotEmpty = false;
                }
            }
            if (slotEmpty & m.defaultPowerup != null) {
                DashPowerupManager.dashPowerups[m.defaultPowerup].updateWhenReady?.Invoke(Entity as Player);
            }
            if (ActivePowerup != null) {
                ActivePowerup?.updateWhenActive?.Invoke(Entity as Player);
            }
        }
    }
}
