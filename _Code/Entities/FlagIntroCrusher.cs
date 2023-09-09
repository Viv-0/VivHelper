using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/FlagIntroCrusher")]
    public class FlagIntroCrusher : Solid {
        private Vector2 shake;

        private Vector2 start;

        private Vector2 end;

        private TileGrid tilegrid;

        private SoundSource shakingSfx;

        public string[] flags = null;

        private bool manualTrigger;

        private float delay;

        private bool triggered;

        private float speed;

        public FlagIntroCrusher(Vector2 position, int width, int height, Vector2 node, char tt)
            : base(position, width, height, safe: true) {
            start = position;
            end = node;
            base.Depth = -10501;
            SurfaceSoundIndex = 4;
            Add(tilegrid = GFX.FGAutotiler.GenerateBox(tt, width / 8, height / 8).TileGrid);
            Add(shakingSfx = new SoundSource());
        }

        public FlagIntroCrusher(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Char("tileType", '3')) {
            if (!string.IsNullOrWhiteSpace(data.Attr("flags")))
                flags = data.Attr("flags").Split(',');
            delay = data.Float("delay", 1.2f);
            speed = data.Float("speed", 2f);

        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            triggered = false;
            if (flags != null ? VivHelperModule.OldGetFlags(scene as Level, flags, "and") : false) {
                Position = end;
            } else {
                Add(new Coroutine(Sequence()));
            }
        }

        public override void Update() {
            tilegrid.Position = shake;
            base.Update();
        }

        private IEnumerator Sequence() {
            Player entity = null;
            do {
                yield return null;
                entity = Scene.Tracker.GetEntity<Player>();
            } while (!triggered && (flags != null ? !VivHelperModule.OldGetFlags(SceneAs<Level>(), flags, "and") : entity == null || (!(entity.X >= X + 30f) || !(entity.X <= Right + 8f))));

            shakingSfx.Play("event:/game/00_prologue/fallblock_first_shake");
            float time2 = delay;
            Shaker shaker = new Shaker(time2, removeOnFinish: true, delegate (Vector2 v) {
                shake = v;
            });
            if (!(delay <= 0f)) {
                Add(shaker);
            }
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            while (time2 > 0f) {
                Player entity2 = Scene.Tracker.GetEntity<Player>();
                if (!manualTrigger && entity2 != null && (entity2.X >= X + Width - 8f || entity2.X < X + 28f)) {
                    shaker.RemoveSelf();
                    break;
                }
                yield return null;
                time2 -= Engine.DeltaTime;
            }
            for (int i = 2; (float) i < Width; i += 4) {
                SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + (float) i, Y), Vector2.One * 4f, (float) Math.PI / 2f);
                SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + (float) i, Y), Vector2.One * 4f);
            }
            shakingSfx.Param("release", 1f);
            time2 = 0f;
            do {
                yield return null;
                time2 = Calc.Approach(time2, 1f, speed * Engine.DeltaTime);
                MoveTo(Vector2.Lerp(start, end, Ease.CubeIn(time2)));
            }
            while (!(time2 >= 1f));
            for (int j = 0; (float) j <= Width; j += 4) {
                SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(X + (float) j, Bottom), Vector2.One * 4f, -(float) Math.PI / 2f);
                float direction = ((!((float) j < Width / 2f)) ? 0f : ((float) Math.PI));
                SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(X + (float) j, Bottom), Vector2.One * 4f, direction);
            }
            shakingSfx.Stop();
            Audio.Play("event:/game/00_prologue/fallblock_first_impact", Position);
            SceneAs<Level>().Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Add(new Shaker(0.25f, removeOnFinish: true, delegate (Vector2 v) {
                shake = v;
            }));
        }
    }
}
