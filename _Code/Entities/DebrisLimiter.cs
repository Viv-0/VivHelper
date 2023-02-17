using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/DebrisLimiter")]
    public class DebrisLimiter : Entity {
        public float limiter;
        public bool random;
        public static bool locker;
        public DebrisLimiter(EntityData data, Vector2 offset) : base() {
            limiter = Calc.Clamp(data.Int("limiter", 0), 0, 32) / 32f;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            locker = false;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (locker)
                RemoveSelf();
            else if (scene is Level) {
                VivHelperModule.Session.DebrisLimiter = limiter;
                locker = true;
            }
        }
    }
}
