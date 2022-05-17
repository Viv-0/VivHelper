using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/DelayedMovingPlatform")]
    public class AltPlatform : JumpThru {
        private Vector2 start;

        private Vector2 end;

        private float addY;

        private float sinkTimer;

        private MTexture[] textures;

        private string lastSfx;

        private SoundSource sfx;

        public string overrideTexture;

        public Tween tween;

        public bool started = false;

        public float delay;

        public int oneshot;

        internal static Tween.TweenMode[] tweenModes = new Tween.TweenMode[]
        {
            Tween.TweenMode.YoyoLooping,
            Tween.TweenMode.YoyoOneshot,
            Tween.TweenMode.Oneshot
        };

        public AltPlatform(Vector2 position, int width, Vector2 node, float delay, int oneshot)
            : base(position, width, safe: false) {
            this.delay = delay;
            this.oneshot = oneshot;
            start = Position;
            end = node;
            Add(sfx = new SoundSource());
            SurfaceSoundIndex = 5;
            lastSfx = ((Math.Sign(start.X - end.X) > 0 || Math.Sign(start.Y - end.Y) > 0) ? "event:/game/03_resort/platform_horiz_left" : "event:/game/03_resort/platform_horiz_right");
            tween = Tween.Create(tweenModes[oneshot], Ease.SineInOut, 2f);
            tween.OnUpdate = delegate (Tween t) {
                MoveTo(Vector2.Lerp(start, end, t.Eased) + Vector2.UnitY * addY);
            };
            tween.OnStart = delegate {
                if (lastSfx == "event:/game/03_resort/platform_horiz_left") {
                    sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_right");
                } else {
                    sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_left");
                }
            };
            if (oneshot > 0)
                tween.OnComplete = delegate { started = false; };
            Add(tween);
            Add(new LightOcclude(0.2f));
        }

        public AltPlatform(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Nodes[0] + offset, Math.Max(-0.01f, data.Float("Delay")), Calc.Clamp(data.Int("LoopType"), 0, 2)) {
            overrideTexture = data.Attr("texture", "default");
        }

        public override void Added(Scene scene) {
            if (string.IsNullOrEmpty(overrideTexture)) {
                overrideTexture = AreaData.Get(scene).WoodPlatform;
            }
            MTexture platformTexture = GFX.Game["objects/woodPlatform/" + overrideTexture];
            textures = new MTexture[platformTexture.Width / 8];
            for (int i = 0; i < textures.Length; i++) {
                textures[i] = platformTexture.GetSubtexture(i * 8, 0, 8, 8);
            }
            Vector2 value = new Vector2(base.Width, base.Height + 4f) / 2f;
            scene.Add(new MovingPlatformLine(start + value, end + value));
        }

        public override void Render() {
            textures[0].Draw(Position);
            for (int i = 8; (float) i < base.Width - 8f; i += 8) {
                textures[1].Draw(Position + new Vector2(i, 0f));
            }
            textures[3].Draw(Position + new Vector2(base.Width - 8f, 0f));
            textures[2].Draw(Position + new Vector2(base.Width / 2f - 4f, 0f));
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            sinkTimer = 0.4f;
        }

        public override void Update() {
            base.Update();
            if (!started) {
                if (HasPlayerRider() && delay > 0f) {
                    started = true;
                    tween.Start(oneshot == 2 ? Vector2.DistanceSquared(Position, end) < 1 : false);
                } else {
                    delay -= Engine.DeltaTime;
                }
            }
            if (HasPlayerRider()) {
                sinkTimer = 0.2f;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            } else if (sinkTimer > 0f) {
                sinkTimer -= Engine.DeltaTime;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            } else {
                addY = Calc.Approach(addY, 0f, 20f * Engine.DeltaTime);
            }
        }
    }
}
