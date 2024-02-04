using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/SeekerStatueOnFlag")]
    public class SeekerStatueOnFlag : Entity {
        private Sprite sprite;
        private string flag;

        public SeekerStatueOnFlag(EntityData data, Vector2 offset) {
            SeekerStatueOnFlag seekerStatue = this;
            base.Depth = 8999;
            Add(sprite = GFX.SpriteBank.Create("seeker"));
            sprite.Play("statue");
            sprite.OnLastFrame = delegate (string f) {
                if (f == "hatch") {
                    Seeker entity = new Seeker(data, offset) {
                        Light =
                    {
                        Alpha = 0f
                    }
                    };
                    seekerStatue.Scene.Add(entity);
                    seekerStatue.RemoveSelf();
                }
            };
            flag = data.Attr("flag", "");
        }

        public override void Update() {
            base.Update();
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null && sprite.CurrentAnimationID == "statue") {
                if (SceneAs<Level>().Session.GetFlag(flag)) {
                    BreakOutParticles();
                    sprite.Play("hatch");
                    Audio.Play("event:/game/05_mirror_temple/seeker_statue_break", Position);
                    Alarm.Set(this, 0.8f, BreakOutParticles);
                }
            }
        }

        private void BreakOutParticles() {
            Level level = SceneAs<Level>();
            for (float num = 0f; num < Consts.TAU; num += 0.17453292f) {
                Vector2 position = base.Center + Calc.AngleToVector(num + Calc.Random.Range(Consts.DEG1 * -4, Consts.DEG1 * 4), Calc.Random.Range(12, 20));
                level.Particles.Emit(Seeker.P_BreakOut, position, num);
            }
        }
    }
}
