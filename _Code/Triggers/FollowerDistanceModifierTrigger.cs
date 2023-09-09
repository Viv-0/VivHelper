using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Celeste.Mod.VivHelper;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/FollowerDistModTrigger")]
    public class FollowerDistanceModifierTrigger : Trigger {
        private int FFD;
        private int FPD;
        public FollowerDistanceModifierTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            FFD = data.Int("FFDistance", 5);
            FPD = data.Int("FPDistance", 30);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            VivHelperModule.Session.FFDistance = Math.Min(Math.Max(0, FFD), 30);
            VivHelperModule.Session.FPDistance = Math.Min(Math.Max(1, FPD), 128);
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);
            RemoveSelf();
        }
    }
}
