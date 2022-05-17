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
    [CustomEntity("VivHelper/TouchSwitchWall")]
    public class TouchSwitchWall : Entity {
        public static ParticleType P_Fire = new ParticleType(TouchSwitch.P_Fire) {
            Color = Calc.HexToColor("f141df") * 0.1f,
            Color2 = Color.White * 0.1f
        };


        public Switch Switch;

        private SoundSource touchSfx;

        protected Sprite icon;

        protected Color inactiveColor = Calc.HexToColor("5fcde4");

        protected Color activeColor = Color.White;

        protected Color finishColor = Calc.HexToColor("f141df");

        private float ease;

        private Wiggler wiggler;

        private Vector2 pulse = Vector2.One;

        private float timer;

        private BloomPoint bloom;

        private bool disableParticles;

        private Level level => (Level) Scene;

        public TouchSwitchWall(EntityData data, Vector2 offset) : base(data.Position + offset) {
            base.Depth = 100;
            Add(Switch = new Switch(groundReset: false));
            Collider = new Hitbox(data.Width, data.Height);
            Add(new PlayerCollider(OnPlayer, Collider));
            Add(bloom = new BloomPoint(0f, 16f));
            bloom.Alpha = 0f;
            icon = new Sprite(GFX.Game, data.Attr("IconPath", "objects/touchswitch/icon"));
            icon.Add("idle", "", 0f, 0);
            icon.Add("spin", "", 0.1f, "spin", 0, 1, 2, 3, 4, 5);
            icon.Play("spin");
            icon.Color = inactiveColor;
            icon.CenterOrigin();
            icon.Position = Collider.Center;
            Add(icon);
            if (data.Bool("AllowHoldables", true))
                Add(new HoldableCollider(OnHoldable, Collider));
            if (data.Bool("AllowSeeker", true))
                Add(new SeekerCollider(OnSeeker, Collider));
            disableParticles = data.Bool("DisableParticles", false);
            Switch.OnActivate = delegate {
                wiggler.Start();

                if (!disableParticles)
                    level.Particles.Emit(TouchSwitch.P_FireWhite, 32, Center, new Vector2(Width / 2, Height / 2));
                icon.Rate = 4f;
            };
            Switch.OnFinish = delegate {
                ease = 0f;
            };
            Switch.OnStartFinished = delegate {
                icon.Rate = 0.1f;
                icon.Play("idle");
                icon.Color = finishColor;
                ease = 1f;
            };
            Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float v) {
                pulse = Vector2.One * (1f + v * 0.25f);
            }));
            Add(new VertexLight(Color.White, 0.8f, 16, 32));
            Add(touchSfx = new SoundSource());


        }

        public void TurnOn() {
            if (!Switch.Activated) {
                touchSfx.Play("event:/game/general/touchswitch_any");
                if (Switch.Activate()) {
                    SoundEmitter.Play("event:/game/general/touchswitch_last_oneshot");
                    Add(new SoundSource("event:/game/general/touchswitch_last_cutoff"));
                }
            }
        }

        private void OnPlayer(Player player) {
            TurnOn();
        }

        private void OnHoldable(Holdable h) {
            TurnOn();
        }

        private void OnSeeker(Seeker seeker) {
            if (SceneAs<Level>().InsideCamera(Position, 10f)) {
                TurnOn();
            }
        }

        public override void Update() {
            timer += Engine.DeltaTime * 8f;
            ease = Calc.Approach(ease, (Switch.Finished || Switch.Activated) ? 1f : 0f, Engine.DeltaTime * 2f);
            icon.Color = Color.Lerp(inactiveColor, Switch.Finished ? finishColor : activeColor, ease);
            icon.Color *= 0.5f + ((float) Math.Sin(timer) + 1f) / 2f * (1f - ease) * 0.5f + 0.5f * ease;
            bloom.Alpha = ease;
            if (Switch.Finished) {
                if (icon.Rate > 0.1f) {
                    icon.Rate -= 2f * Engine.DeltaTime;
                    if (icon.Rate <= 0.1f) {
                        icon.Rate = 0.1f;
                        wiggler.Start();
                        icon.Play("idle");
                        level.Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                    }
                } else if (base.Scene.OnInterval(0.03f) && !disableParticles) {
                    level.ParticlesBG.Emit(P_Fire, 1, Center, new Vector2(Width / 2, Height / 2));
                }
            }
            base.Update();
        }

        public override void Render() {
            Draw.HollowRect(X - 1, Y - 1, Width + 2, Height + 2, Extensions.ColorCopy(icon.Color, 0.7f));
            Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, Color.Lerp(icon.Color, Calc.HexToColor("0a0a0a"), 0.5f) * 0.3f);
            base.Render();
        }
    }
}
