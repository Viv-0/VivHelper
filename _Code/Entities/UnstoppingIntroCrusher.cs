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
    [CustomEntity("VivHelper/UnstoppingFallIntroCrusher")]
    public class UnstoppingFallIntroCrusher : Solid, PostAwakeHolder {
        private Vector2 shake;
        public bool KillPlayerOnTouch;
        public bool DestroyCustomSpinners;
        public string flag;
        public bool moveInstant, resetOnAdded;
        public Vector2 StartNode, EndNode;
        public float shakeTime, moveTimeInv;
        private TileGrid tilegrid;

        private SoundSource shakingSfx;

        public ShatterCustomSpinnerOnTouchComponent shatterCustom;
        public UnstoppingFallIntroCrusher(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false) {
            if (data.Bool("KillPlayerOnTouch"))
                Add(new PlayerCollider(KillPlayer));
            Vector2 temp = data.Position + offset;

            DestroyCustomSpinners = data.Bool("DestroyCustomSpinner", false);
            StartNode = data.Position + offset;
            EndNode = data.NodesOffset(offset)[0];
            shakeTime = Math.Max(0f, data.Float("shakeTime", 1.2f));
            moveTimeInv = data.Float("moveTime", 0.5f);
            if (moveTimeInv <= 0)
                moveTimeInv = 2f;
            else
                moveTimeInv = 1 / moveTimeInv;
            resetOnAdded = data.Bool("ResetOnAdded");

        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (!string.IsNullOrEmpty(flag) && ((scene as Level)?.Session?.GetFlag(flag) ?? false)) {
                if (resetOnAdded) {
                    (scene as Level).Session.SetFlag(flag, false);
                } else {
                    moveInstant = true;
                }
            }
            if (DestroyCustomSpinners) {
                Add(shatterCustom = new ShatterCustomSpinnerOnTouchComponent());
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (!moveInstant) {
                Add(new Coroutine(Sequence()));
            }
        }

        public void PostAwake(Scene scene) {
            if (moveInstant) {
                if (DestroyCustomSpinners) {
                    int i = 0;
                    while (i++ <= 100) {
                        MoveTo(Vector2.Lerp(StartNode, EndNode, Ease.Linear(i / 100)));
                        shatterCustom.DestroySpinners(false);
                    }
                } else {
                    Position = EndNode;
                }
            }
        }

        public override void Update() {
            tilegrid.Position = shake;
            base.Update();
        }

        private IEnumerator Sequence() {

            shakingSfx.Play("event:/game/00_prologue/fallblock_first_shake");
            float time2 = shakeTime;
            while (!SceneAs<Level>().Session.GetFlag(flag))
                yield return null;
            Shaker shaker = new Shaker(time2, removeOnFinish: true, delegate (Vector2 v) {
                shake = v;
            });
            Add(shaker);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            while (time2 > 0f) {
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
                time2 = Calc.Approach(time2, 1f, moveTimeInv * Engine.DeltaTime);
                MoveTo(Vector2.Lerp(StartNode, EndNode, time2));
                shatterCustom.DestroySpinners(true);
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

        public void KillPlayer(Player p) {
            p.Die(Speed.SafeNormalize(Vector2.Zero));
        }
    }
}
