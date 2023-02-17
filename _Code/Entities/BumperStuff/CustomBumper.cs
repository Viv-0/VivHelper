using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;

namespace VivHelper.Entities {/*
    [CustomEntity("VivHelper/CustomBumper")]
    public class CustomBumper : Entity {
        public static ParticleType P_Ambience;

        public static ParticleType P_Launch;

        private Sprite sprite;

        private Sprite spriteEvil;

        private VertexLight light;

        private BloomPoint bloom;

        private Vector2? node;

        private bool goBack;

        private Vector2 anchor;

        private SineWave sine;

        private float respawnTimer;

        private bool fireMode;

        private Wiggler hitWiggler;

        private Vector2 hitDir;

        public Bumper(Vector2 position, Vector2? node)
            : base(position) {
            base.Collider = new Circle(12f);
            Add(new PlayerCollider(OnPlayer));
            Add(sine = new SineWave(0.44f, 0f).Randomize());
            Add(sprite = GFX.SpriteBank.Create("bumper"));
            Add(spriteEvil = GFX.SpriteBank.Create("bumper_evil"));
            spriteEvil.Visible = false;
            Add(light = new VertexLight(Color.Teal, 1f, 16, 32));
            Add(bloom = new BloomPoint(0.5f, 16f));
            this.node = node;
            anchor = Position;
            if (node.HasValue) {
                Vector2 start = Position;
                Vector2 end = node.Value;
                Tween tween = Tween.Create(Tween.TweenMode.Looping, Ease.CubeInOut, 1.81818187f, start: true);
                tween.OnUpdate = delegate (Tween t) {
                    if (goBack) {
                        anchor = Vector2.Lerp(end, start, t.Eased);
                    } else {
                        anchor = Vector2.Lerp(start, end, t.Eased);
                    }
                };
                tween.OnComplete = delegate {
                    goBack = !goBack;
                };
                Add(tween);
            }

            UpdatePosition();
            Add(hitWiggler = Wiggler.Create(1.2f, 2f, delegate {
                spriteEvil.Position = hitDir * hitWiggler.Value * 8f;
            }));
            Add(new CoreModeListener(OnChangeMode));
        }

        public Bumper(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.FirstNodeNullable(offset)) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            fireMode = SceneAs<Level>().CoreMode == Session.CoreModes.Hot;
            spriteEvil.Visible = fireMode;
            sprite.Visible = !fireMode;
        }

        private void OnChangeMode(Session.CoreModes coreMode) {
            fireMode = coreMode == Session.CoreModes.Hot;
            spriteEvil.Visible = fireMode;
            sprite.Visible = !fireMode;
        }

        private void UpdatePosition() {
            Position = anchor + new Vector2(sine.Value * 3f, sine.ValueOverTwo * 2f);
        }

        public override void Update() {
            base.Update();
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f) {
                    light.Visible = true;
                    bloom.Visible = true;
                    sprite.Play("on");
                    spriteEvil.Play("on");
                    if (!fireMode) {
                        Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
                    }
                }
            } else if (base.Scene.OnInterval(0.05f)) {
                float num = Calc.Random.NextAngle();
                ParticleType type = (fireMode ? P_FireAmbience : P_Ambience);
                float direction = (fireMode ? (-(float) Math.PI / 2f) : num);
                float length = (fireMode ? 12 : 8);
                SceneAs<Level>().Particles.Emit(type, 1, base.Center + Calc.AngleToVector(num, length), Vector2.One * 2f, direction);
            }

            UpdatePosition();
        }

        private void OnPlayer(Player player) {
            if (fireMode) {
                if (!SaveData.Instance.Assists.Invincible) {
                    Vector2 vector = (player.Center - base.Center).SafeNormalize();
                    hitDir = -vector;
                    hitWiggler.Start();
                    Audio.Play("event:/game/09_core/hotpinball_activate", Position);
                    respawnTimer = 0.6f;
                    player.Die(vector);
                    SceneAs<Level>().Particles.Emit(P_FireHit, 12, base.Center + vector * 12f, Vector2.One * 3f, vector.Angle());
                }
            } else if (respawnTimer <= 0f) {
                if ((base.Scene as Level).Session.Area.ID == 9) {
                    Audio.Play("event:/game/09_core/pinballbumper_hit", Position);
                } else {
                    Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);
                }

                respawnTimer = 0.6f;
                Vector2 vector2 = player.ExplodeLaunch(Position, snapUp: false, sidesOnly: false);
                sprite.Play("hit", restart: true);
                spriteEvil.Play("hit", restart: true);
                light.Visible = false;
                bloom.Visible = false;
                SceneAs<Level>().DirectionalShake(vector2, 0.15f);
                SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f);
                SceneAs<Level>().Particles.Emit(P_Launch, 12, base.Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
            }
        }
    }*/
}