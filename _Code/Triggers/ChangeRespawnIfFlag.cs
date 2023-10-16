using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/ChangeRespawnIfFlag")]
    public class ChangeRespawnIfFlag : ChangeRespawnTrigger {
        public string flag;
        public bool invert;
        public ChangeRespawnIfFlag(EntityData data, Vector2 offset) : base(data, offset) {
            flag = data.Attr("Flag", "");
            if (flag.Length > 1 && flag[0] == '!') {
                invert = true;
                flag = flag.Substring(1);
            }
        }

        [MonoMod.MonoModLinkTo("Celeste.Trigger", "System.Void OnEnter(Celeste.Player)")]
        internal void Trigger_OnEnter(Player player) { Logger.Log("VivHelper", "Trigger::OnEnter Link failed"); base.OnEnter(player); }

        public override void OnEnter(Player player) {
            Trigger_OnEnter(player);
            Session session = (base.Scene as Level).Session;
            if ((string.IsNullOrEmpty(flag) || (session.GetFlag(flag) == invert)) && SolidCheck() && (!session.RespawnPoint.HasValue || session.RespawnPoint.Value != Target)) {
                session.HitCheckpoint = true;
                session.RespawnPoint = Target;
                session.UpdateLevelStartDashes();
            }
        }
        private bool SolidCheck() {
            Vector2 point = Target + Vector2.UnitY * -4f;
            if (base.Scene.CollideCheck<Solid>(point)) {
                return base.Scene.CollideCheck<FloatySpaceBlock>(point);
            }
            return true;
        }
    }
}
