using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.VivHelper {
    [CustomEntity("VivHelper/FloatyBreakBlock")]
    public class FloatyBreakBlock : FloatySpaceBlock {
        enum DelayType {
            Timer,
            Pressure,
            Sinking
        }
        private float delay;
        private float origDelay;
        private DelayType delayType;
        public bool beginBreak;
        private char tiletype;

        public FloatyBreakBlock(EntityData data, Vector2 offset)
            : base(data, offset) {
            tiletype = data.Char("tiletype", '3');

            delay = origDelay = data.Float("delay");
            delayType = data.Enum("delayType", DelayType.Timer);

        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            beginBreak = false;
            if (delayType == DelayType.Timer) { Add(new Coroutine(DelayTimer())); } else if (delayType == DelayType.Sinking) { Add(new Coroutine(DelaySinking())); } else if (delayType == DelayType.Pressure) { Add(new Coroutine(DelayPressure())); } else { }

        }

        public void Break() {
            if (!Collidable || !(Scene is Level)) {
                return;
            }
            Audio.Play("event:/new_content/game/10_farewell/quake_rockbreak", Position);
            Collidable = false;
            for (int a = 0; a < Width / 8f; a++) {
                for (int b = 0; b < Height / 8f; b++) {
                    if (!Scene.CollideCheck<Solid>(new Rectangle((int) X + a * 8, (int) Y + b * 8, 8, 8))) {
                        Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + a * 8, 4 + b * 8), tiletype).BlastFrom(TopCenter));
                    }
                }
            }
            RemoveSelf();
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            beginBreak = true;
        }

        public IEnumerator DelayTimer() {
            while (!beginBreak && !HasPlayerRider()) {
                yield return null;
            }
            while (delay > 0f) {
                delay -= Engine.DeltaTime;
                yield return null;
            }
            Break();
        }

        public IEnumerator DelaySinking() {
            while (!beginBreak && !HasPlayerRider()) {
                yield return null;
            }
            while (delay > 0f) {
                delay -= (HasPlayerRider() ? 1.5f * Engine.DeltaTime : Engine.DeltaTime);
                yield return null;
            }
            Break();
        }

        public IEnumerator DelayPressure() {
            while (!beginBreak && !HasPlayerRider()) {
                yield return null;
            }
            while (delay > 0f) {
                if (delay > (origDelay / 5f)) {
                    if (HasPlayerRider()) { delay -= Engine.DeltaTime; } else { delay += 0.5f * Engine.DeltaTime; if (delay > origDelay) { delay = origDelay; } }
                    yield return null;
                } else {
                    delay -= Engine.DeltaTime;
                    yield return null;
                }
            }
            Break();
        }
    }
}