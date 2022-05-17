using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.VivHelper;

namespace VivHelper.Entities {

    public class LaserBeam : Entity {
        private float AngleStartOffset = 100f;

        private float RotationSpeed = 200f;

        private float CollideCheckSep = 4f;

        private float BeamLength = 2000f;

        private float BeamStartDist = 12f;

        private const int BeamsDrawn = 15;

        private float SideDarknessAlpha = 0.35f;

        private LaserBlock master;

        private Player player;

        private Sprite beamSprite;

        private Sprite beamStartSprite;

        private float chargeTimer;

        private float followTimer;

        private float activeTimer;

        private float angle;

        private float beamAlpha;

        private float sideFadeAlpha;

        private VertexPositionColor[] fade = new VertexPositionColor[24];

        private bool _;

        private Vector2 BeamOrigin => master == null ? default(Vector2) : master.Center - Calc.AngleToVector(angle, 3f);

        private bool muted;

        public LaserBeam() {
            Add(beamSprite = GFX.SpriteBank.Create("badeline_beam"));
            beamSprite.OnLastFrame = delegate (string anim) {
                if (anim == "shoot") {
                    Destroy();
                }
            };
            Add(beamStartSprite = GFX.SpriteBank.Create("badeline_beam_start"));
            beamSprite.Visible = false;
            base.Depth = -9999;
        }

        public LaserBeam Init(LaserBlock master, float angle, float dark) {
            this.master = master;
            chargeTimer = master.ChargeTime;
            activeTimer = master.ActiveTime;
            SideDarknessAlpha = dark;
            beamSprite.Play("charge");
            sideFadeAlpha = 0f;
            beamAlpha = 0f;
            this.angle = angle;
            _ = false;
            base.Depth = -9999;
            return this;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

        }

        public override void Update() {
            Active = Collidable = master.enabled;
            base.Update();
            player = base.Scene.Tracker.GetEntity<Player>();
            beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);
            Position = BeamOrigin;
            if (chargeTimer > 0f) {
                if (!_) { if (!master.muted) { master.sfx.Play("event:/char/badeline/boss_laser_charge"); } _ = true; }
                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
                if (player != null && !player.Dead) {
                    followTimer -= Engine.DeltaTime;
                    chargeTimer -= Engine.DeltaTime;
                    if (followTimer > 0f && player.Center != BeamOrigin) {
                        Vector2 val = Calc.ClosestPointOnLine(BeamOrigin, BeamOrigin + Calc.AngleToVector(angle, 2000f), player.Center);
                        Vector2 center = player.Center;
                        val = Calc.Approach(val, center, 200f * Engine.DeltaTime);
                        angle = Calc.Angle(BeamOrigin, val);
                    } else if (beamSprite.CurrentAnimationID == "charge") {
                        beamSprite.Play("lock");
                    }
                    if (chargeTimer <= 0f) {
                        SceneAs<Level>().DirectionalShake(Calc.AngleToVector(angle, 1f), 0.15f);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        DissipateParticles();
                    }
                }
            } else if (activeTimer > 0f) {
                if (VivHelper.TryGetPlayer(out Player player)) {
                    if (LaserBlock.InvalidStates.Contains(player.StateMachine.State)) { RemoveSelf(); }
                }
                if (_) { if (!master.muted) { master.sfx.Play("event:/char/badeline/boss_laser_fire"); } _ = false; }
                sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0f, Engine.DeltaTime * 8f);
                if (beamSprite.CurrentAnimationID != "shoot") {
                    beamSprite.Play("shoot");
                    beamStartSprite.Play("shoot", restart: true);
                }
                activeTimer -= Engine.DeltaTime;
                if (activeTimer > 0f) {
                    PlayerCollideCheck();
                }
            } else {
                RemoveSelf();
            }
        }

        private void DissipateParticles() {
            Level level = SceneAs<Level>();
            Vector2 vector = level.Camera.Position + new Vector2(160f, 90f);
            Vector2 vector2 = BeamOrigin;
            Vector2 vector3 = BeamOrigin + Calc.AngleToVector(angle, BeamLength);
            Vector2 vector4 = (vector3 - vector2).Perpendicular().SafeNormalize();
            Vector2 value = (vector3 - vector2).SafeNormalize();
            Vector2 min = -vector4 * 1f;
            Vector2 max = vector4 * 1f;
            float direction = vector4.Angle();
            float direction2 = (-vector4).Angle();
            float num = Vector2.Distance(vector, vector2);
            vector = Calc.ClosestPointOnLine(vector2, vector3, vector);
            for (int i = 0; i < 200; i += 12) {
                for (int j = -1; j <= 1; j += 2) {
                    level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, vector + value * i + vector4 * 2f * j + Calc.Random.Range(min, max), direction);
                    level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, vector + value * i - vector4 * 2f * j + Calc.Random.Range(min, max), direction2);
                    if (i != 0 && (float) i < num) {
                        level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, vector - value * i + vector4 * 2f * j + Calc.Random.Range(min, max), direction);
                        level.ParticlesFG.Emit(FinalBossBeam.P_Dissipate, vector - value * i - vector4 * 2f * j + Calc.Random.Range(min, max), direction2);
                    }
                }
            }
        }

        private void PlayerCollideCheck() {
            Vector2 vector = BeamOrigin;
            Vector2 vector2 = BeamOrigin + Calc.AngleToVector(angle, BeamLength);
            Vector2 value = (vector2 - vector).Perpendicular().SafeNormalize(CollideCheckSep);
            Player player = base.Scene.CollideFirst<Player>(vector + value, vector2 + value);
            if (player == null) {
                player = base.Scene.CollideFirst<Player>(vector - value, vector2 - value);
            }
            if (player == null) {
                player = base.Scene.CollideFirst<Player>(vector, vector2);
            }
            player?.Die((player.Center - BeamOrigin).SafeNormalize());
        }

        public override void Render() {
            Vector2 beamOrigin = BeamOrigin;
            Vector2 vector = Calc.AngleToVector(angle, beamSprite.Width);
            beamSprite.Rotation = angle;
            beamSprite.Color = Color.White * beamAlpha * (master.enabled ? 1f : 0.25f);
            beamStartSprite.Rotation = angle;
            beamStartSprite.Color = Color.White * beamAlpha * (master.enabled ? 1f : 0.25f);
            if (beamSprite.CurrentAnimationID == "shoot") {
                beamOrigin += Calc.AngleToVector(angle, 8f);
            }
            for (int i = 0; i < BeamsDrawn; i++) {
                beamSprite.RenderPosition = beamOrigin;
                beamSprite.Render();
                beamOrigin += vector;
            }
            if (beamSprite.CurrentAnimationID == "shoot") {
                beamStartSprite.RenderPosition = BeamOrigin;
                beamStartSprite.Render();
            }
            GameplayRenderer.End();
            Vector2 vector2 = vector.SafeNormalize();
            Vector2 vector3 = vector2.Perpendicular();
            Color color = Color.Black * sideFadeAlpha * SideDarknessAlpha;
            Color transparent = Color.Transparent;
            vector2 *= 4000f;
            vector3 *= 120f;
            int v = 0;
            Quad(ref v, beamOrigin, -vector2 + vector3 * 2f, vector2 + vector3 * 2f, vector2 + vector3, -vector2 + vector3, color, color);
            Quad(ref v, beamOrigin, -vector2 + vector3, vector2 + vector3, vector2, -vector2, color, transparent);
            Quad(ref v, beamOrigin, -vector2, vector2, vector2 - vector3, -vector2 - vector3, transparent, color);
            Quad(ref v, beamOrigin, -vector2 - vector3, vector2 - vector3, vector2 - vector3 * 2f, -vector2 - vector3 * 2f, color, color);
            GFX.DrawVertices((base.Scene as Level).Camera.Matrix, fade, fade.Length);
            GameplayRenderer.Begin();
        }

        private void Quad(ref int v, Vector2 offset, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ab, Color cd) {
            fade[v].Position.X = offset.X + a.X;
            fade[v].Position.Y = offset.Y + a.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + b.X;
            fade[v].Position.Y = offset.Y + b.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + c.X;
            fade[v].Position.Y = offset.Y + c.Y;
            fade[v++].Color = cd;
            fade[v].Position.X = offset.X + a.X;
            fade[v].Position.Y = offset.Y + a.Y;
            fade[v++].Color = ab;
            fade[v].Position.X = offset.X + c.X;
            fade[v].Position.Y = offset.Y + c.Y;
            fade[v++].Color = cd;
            fade[v].Position.X = offset.X + d.X;
            fade[v].Position.Y = offset.Y + d.Y;
            fade[v++].Color = cd;
        }

        public void Destroy() {
            RemoveSelf();
        }
    }

    [CustomEntity("VivHelper/LaserBlock = LaserBlockNormal", "VivHelper/LaserBlockMoving = LaserBlockMoving")]
    [Tracked]
    public class LaserBlock : Solid {
        public static int[] InvalidStates = new int[] { 11, 12, 13, 14, 15, 17, 23, 25 };
        public enum BlockTypes {
            Normal,
            AttachToSolid,
            Moving,
        }

        public static Entity LaserBlockNormal(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new LaserBlock(entityData, offset, entityData.Bool("AttachToSolid") ? BlockTypes.AttachToSolid : BlockTypes.Normal);
        public static Entity LaserBlockMoving(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new LaserBlock(entityData, offset, BlockTypes.Moving);

        public float ChargeTime = 1.4f;

        public float ActiveTime = 0.12f;

        public float Delay = 1.4f;

        public float dark = 0.35f;
        public string Flag;
        public int direction;
        public BloomPoint bloom;
        public MTexture texture;
        public StaticMover sm;
        private float timer;
        public SoundSource sfx;

        public bool enabled {
            get {
                if (Flag == "") { return startedBlasting; } else {
                    return (Scene as Level).Session.GetFlag(Flag);
                }
            }
        }

        public bool muted;
        private BlockTypes type;
        private bool startedBlasting;
        private struct MoverData {
            public Ease.Easer Easer;
            public Vector2 startPoint, endPoint;
            public float moveTime;
            public string flag;
            public bool startMoving;
        }
        private MoverData mover;


        public LaserBlock(EntityData data, Vector2 offset, BlockTypes type) : base(data.Position + offset, 16, 16, true) {
            base.Depth = -13000;
            direction = Calc.Clamp(data.Int("Direction", 4), 1, 15);
            texture = GFX.Game[data.Attr("Directory", "VivHelper/laserblock/techno") + (direction < 10 ? "0" : "") + direction];
            ChargeTime = data.Float("ChargeTime", 1.4f);
            ActiveTime = data.Float("ActiveTime", 0.12f);
            Delay = data.Float("Delay", 1.4f);
            timer = data.Float("StartDelay", -1f);
            timer = timer < 0f ? Delay : timer;
            Add(new Image(texture));
            Add(sfx = new SoundSource());
            startedBlasting = data.Bool("StartShooting", true); //So anyways, I started blasting. Bang! Bang!
            Flag = data.Attr("Flag", "");
            muted = data.Bool("Muted", false);
            dark = data.Float("DarknessAlpha", 0.35f);
            this.type = type;
            if (type == BlockTypes.AttachToSolid) {
                Add(sm = new StaticMover {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    OnMove = OnMove
                });
            }
            if (type == BlockTypes.Moving) {
                mover = new MoverData {
                    startPoint = Position,
                    endPoint = data.Nodes[0],
                    moveTime = data.Float("MoveTime", 1f),
                    Easer = VivHelper.TryGetEaser(data.Attr("EaseType", "CubeInOut"), out Ease.Easer e) ? e : Ease.CubeInOut,
                    flag = data.Attr("MoveFlag", ""),
                    startMoving = data.Bool("StartMoving", true)
                };

                Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, mover.Easer, mover.moveTime);
                tween.OnUpdate = delegate (Tween t) {
                    if ((bool) (base.Scene as Level)?.Session?.GetFlag(mover.flag) || t.Eased == 1 || t.Eased == 0)
                        MoveToNaive(Vector2.Lerp(mover.startPoint, mover.endPoint, t.Eased));
                };
                Add(tween);
                tween.Start(reverse: false);
            }

        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (Flag != "")
                (Scene as Level).Session.SetFlag(Flag, startedBlasting);
            if (type == BlockTypes.Moving && mover.startMoving && mover.flag != "")
                (Scene as Level).Session.SetFlag(mover.flag);
        }

        public override void Update() {
            base.Update();
            if (VivHelper.TryGetPlayer(out Player player) && !InvalidStates.Contains(player.StateMachine.State) && (Flag == "" ? startedBlasting : (Scene as Level).Session.GetFlag(Flag))) {
                if (timer > 0f) {
                    timer -= Engine.DeltaTime;
                } else {
                    timer = Delay;
                    if ((direction & 1) > 0) {
                        (Scene as Level).Add(Engine.Pooler.Create<LaserBeam>().Init(this, 0f, dark / VivHelper.Get1s((uint) direction)));
                    }
                    if ((direction & 2) > 0) {
                        (Scene as Level).Add(Engine.Pooler.Create<LaserBeam>().Init(this, -1.571f, dark / VivHelper.Get1s((uint) direction)));
                    }
                    if ((direction & 4) > 0) {
                        (Scene as Level).Add(Engine.Pooler.Create<LaserBeam>().Init(this, 3.142f, dark / VivHelper.Get1s((uint) direction)));
                    }
                    if ((direction & 8) > 0) {
                        (Scene as Level).Add(Engine.Pooler.Create<LaserBeam>().Init(this, 1.571f, dark / VivHelper.Get1s((uint) direction)));
                    }

                }
            }
        }

        private bool IsRiding(Solid solid) {
            bool b = false;
            if ((direction & 1) < 1) {
                b |= CollideCheckOutside(solid, Position + Vector2.UnitX);
            }
            if ((direction & 2) < 1 && !b) {
                b |= CollideCheckOutside(solid, Position - Vector2.UnitY);
            }
            if ((direction & 4) < 1 && !b) {
                b |= CollideCheckOutside(solid, Position - Vector2.UnitX);
            }
            if ((direction & 8) < 1 && !b) {
                b |= CollideCheckOutside(solid, Position + Vector2.UnitY);
            }
            return b;
        }

        private void OnMove(Vector2 amount) {
            MoveTo(Position + amount);
        }
    }
}
