using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Mono.Cecil.Cil;

namespace VivHelper.Entities {

    public class GoldenBerryFlagController : Entity {

        public GoldenBerryFlagController() {
            Tag = Tags.Global | Tags.Persistent;
        }

        public override void Update() {
            base.Update();
            if (Scene is Level level && Scene.Tracker.TryGetEntity<Player>(out Player player) && !player.Dead) {
                level.Session?.SetFlag("VivHelper/PlayerHasGoldenBerry", level.Session.GrabbedGolden); //If Session is null it skips the func call.
                level.Session?.SetFlag("VivHelper/PlayerHasExactGoldenBerry", level.Session.GrabbedGolden && player.Leader.Followers.FirstOrDefault(f => f.Entity.GetType() == typeof(Strawberry) && (f.Entity as Strawberry).Golden && !(f.Entity as Strawberry).Winged) != null);
            }
        }
    }
}
