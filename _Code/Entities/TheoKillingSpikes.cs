using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {/*
    [CustomEntity(
           "VivHelper/TheoSpikesUp = LoadUp",
           "VivHelper/TheoSpikesDown = LoadDown",
           "VivHelper/TheoSpikesLeft = LoadLeft",
           "VivHelper/TheoSpikesRight = LoadRight"
       )]*/
    public class HoldableKillingSpikes : RainbowSpikes {
        public List<Type> typesToKill, extensiblesToKill;

        public HoldableKillingSpikes(EntityData data, Vector2 offset, DirectionPlus dir) : base(data, offset, dir) {
            if (data.Bool("DisablePlayerDeath")) {
                Remove(Get<PlayerCollider>());
            }
        }
    }
}
