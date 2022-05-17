using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.VivHelper;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities.SeekerStuff {
    public class CustomSeekerEffectsController : Entity {
        private float randomAnxietyOffset;

        public bool enabled = true;

        public CustomSeekerEffectsController() {
            base.Tag = Tags.Global;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Level obj = scene as Level;
            obj.Session.Audio.Music.Layer(3, 0f);
            obj.Session.Audio.Apply(forceSixteenthNoteHack: false);

        }

        public override void Update() {
            base.Update();
            if (enabled) {
                if (base.Scene.OnInterval(0.05f)) {
                    randomAnxietyOffset = Calc.Random.Range(-0.2f, 0.2f);
                }
                Vector2 position = (base.Scene as Level).Camera.Position;
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                float target;
                float num4;
                if (entity != null && !entity.Dead) {
                    float num = -1f;
                    float num2 = -1f;
                    foreach (CustomSeeker entity2 in base.Scene.Tracker.GetEntities<CustomSeeker>()) {
                        float num3 = Vector2.DistanceSquared(entity.Center, entity2.Center);
                        if (!entity2.Regenerating) {
                            num = ((!(num < 0f)) ? Math.Min(num, num3) : num3);
                        }
                        if (entity2.Attacking) {
                            num2 = ((!(num2 < 0f)) ? Math.Min(num2, num3) : num3);
                        }
                    }
                    target = ((!(num2 >= 0f)) ? 1f : Calc.ClampedMap(num2, 256f, 4096f, 0.5f));
                    Distort.AnxietyOrigin = new Vector2((entity.Center.X - position.X) / 320f, (entity.Center.Y - position.Y) / 180f);
                    num4 = ((!(num >= 0f)) ? 0f : Calc.ClampedMap(num, 256f, 16384f, 1f, 0f));
                } else {
                    target = 1f;
                    num4 = 0f;
                }
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, target, 4f * Engine.DeltaTime);
                Distort.GameRate = Calc.Approach(Distort.GameRate, Calc.Map(Engine.TimeRate, 0.5f, 1f), Engine.DeltaTime * 2f);
                Distort.Anxiety = Calc.Approach(Distort.Anxiety, (0.5f + randomAnxietyOffset) * num4, 8f * Engine.DeltaTime);
                if (Engine.TimeRate == 1f && Distort.GameRate == 1f && Distort.Anxiety == 0f && base.Scene.Tracker.CountEntities<CustomSeeker>() == 0) {
                    enabled = false;
                }
            } else if (base.Scene.Tracker.CountEntities<CustomSeeker>() > 0) {
                enabled = true;
            }
        }
    }
}
