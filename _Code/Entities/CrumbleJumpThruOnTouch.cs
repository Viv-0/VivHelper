using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Collections;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CrumbleJumpThru")]
    public class CrumbleJumpThruOnTouch : JumpthruPlatform {
        public bool permanent;

        public float delay;

        public bool triggered;

        public CrumbleJumpThruOnTouch(EntityData data, Vector2 offset) : base(data, offset) {
            delay = data.Float("Delay", 0.1f);
            permanent = data.Bool("Permanent", false);
            Add(new Coroutine(Sequence()));

        }

        public void Break() {
            if (!Collidable || base.Scene == null) {
                return;
            }
            Audio.Play("event:/new_content/game/10_farewell/quake_rockbreak", Position);
            Collidable = false;
            for (int i = 0; (float) i < base.Width / 8f; i++) {
                if (!base.Scene.CollideCheck<Solid>(new Rectangle((int) base.X + i * 8, (int) base.Y, 8, 8))) {
                    base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4), '9', playSound: true).BlastFrom(base.TopCenter));
                }
            }
            if (permanent) {
                Level level = SceneAs<Level>();
            }
            RemoveSelf();
        }

        private IEnumerator Sequence() {
            while (!triggered && !HasPlayerRider()) {
                yield return null;
            }
            while (delay > 0f) {
                delay -= Engine.DeltaTime;
                yield return null;
            }
            Break();
        }
    }
}
