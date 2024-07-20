using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;

namespace VivHelper.Entities {
    [Tracked]
    public class CustomFallingBlock_140 : Solid {
        private static Dictionary<string, float> angles = new Dictionary<string, float>() { { "right", 0f }, { "down", Consts.PIover2 }, { "left", Consts.PI }, { "up", (float) Consts.PIover2 * 3 } };

        private Vector2 ImpactCenter {
            get {
                switch (angle) {
                    case 0f:
                        return TopLeft + new Vector2(base.Width, base.Height / 2f);
                    case Consts.PIover2:
                        return TopLeft + new Vector2(base.Width / 2f, base.Height);
                    case Consts.PI:
                        return TopLeft + new Vector2(0, base.Height / 2f);
                    case Consts.PIover2 * 3:
                        return TopLeft + new Vector2(base.Width / 2f, 0);
                    default:
                        return TopLeft + new Vector2(base.Width / 2f, base.Height);
                }
            }
        }
        public bool Triggered;

        public float FallDelay;

        private char TileType;

        private TileGrid tiles;

        private TileGrid highlight;

        private bool finalBoss;

        private bool climbFall;

        private bool DashBlock;

        private string flagTrigger;

        private string flagOnFall;

        private string flagOnGround;

        private string ShakeSFX = "event:/game/general/fallblock_shake";

        private string ImpactSFX = "event:/game/general/fallblock_impact";

        private float angle;
        private string angletext;
        private float accel = 500f;
        private float maxSpeed = 160f;

        public bool HasStartedFalling {
            get;
            private set;
        }

        public CustomFallingBlock_140(Vector2 position, char tile, int width, int height, bool finalBoss, bool behind, bool climbFall)
            : base(position, width, height, safe: false) {
            this.finalBoss = finalBoss;
            this.climbFall = climbFall;
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(tile, width / 8, height / 8).TileGrid);
            Calc.PopRandom();
            if (finalBoss) {
                Calc.PushRandom(newSeed);
                Add(highlight = GFX.FGAutotiler.GenerateBox('G', width / 8, height / 8).TileGrid);
                Calc.PopRandom();
                highlight.Alpha = 0f;
            }
            Add(new Coroutine(Sequence()));
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, highPriority: false));
            TileType = tile;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tile];
            if (behind) {
                base.Depth = 5000;
            }
        }

        public CustomFallingBlock_140(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, finalBoss: false, data.Bool("behind"), data.Bool("climbFall", defaultValue: true)) {
            accel = data.Float("Accel", 500f);
            maxSpeed = data.Float("MaxSpeed", 160f);
            ShakeSFX = data.Attr("ShakeSFX", "event:/game/general/fallblock_shake");
            ImpactSFX = data.Attr("ImpactSFX", "event:/game/general/fallblock_impact");
            angletext = data.Attr("Direction", "Down").ToLower();
            angle = angles[angletext];
            flagOnFall = data.Attr("FlagOnFall", "");
            flagTrigger = data.Attr("FlagTrigger", "");
            flagOnGround = data.Attr("FlagOnGround", "");
        }

        public static CustomFallingBlock_140 CreateFinalBossBlock(EntityData data, Vector2 offset) {
            return new CustomFallingBlock_140(data.Position + offset, 'g', data.Width, data.Height, finalBoss: true, behind: false, climbFall: false);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (flagOnGround != "" && (base.Scene as Level).Session.GetFlag(flagOnGround))
                (base.Scene as Level).Session.SetFlag(flagOnGround, false);
            if (flagOnFall != "" && (base.Scene as Level).Session.GetFlag(flagOnFall))
                (base.Scene as Level).Session.SetFlag(flagOnFall, false);
            if (flagTrigger != "" && (base.Scene as Level).Session.GetFlag(flagTrigger))
                (base.Scene as Level).Session.SetFlag(flagTrigger, false);

        }

        public override void Update() {
            base.Update();
            if (flagTrigger != "" && (base.Scene as Level).Session.GetFlag(flagTrigger) && !Triggered)
                Triggered = true;
            if (Triggered && flagOnFall != "" && !(base.Scene as Level).Session.GetFlag(flagOnFall))
                (base.Scene as Level).Session.SetFlag(flagOnFall, true);

        }

        public override void OnShake(Vector2 amount) {
            base.OnShake(amount);
            tiles.Position += amount;
            if (highlight != null) {
                highlight.Position += amount;
            }
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            if (!finalBoss) {
                Triggered = true;
            }
        }

        private bool PlayerFallCheck() {
            if (climbFall) {
                return HasPlayerRider();
            }
            return HasPlayerOnTop();
        }

        private bool PlayerWaitCheck() {
            if (Triggered) {
                return true;
            }
            if (PlayerFallCheck()) {
                return true;
            }
            if (climbFall) {
                if (!CollideCheck<Player>(Position - Vector2.UnitX)) {
                    return CollideCheck<Player>(Position + Vector2.UnitX);
                }
                return true;
            }
            return false;
        }

        private IEnumerator Sequence() {
            while (!Triggered && (finalBoss || !PlayerFallCheck())) {
                yield return null;
            }
            while (FallDelay > 0f) {
                FallDelay -= Engine.DeltaTime;
                yield return null;
            }
            HasStartedFalling = true;
            while (true) {
                ShakeSfx();
                StartShaking();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                if (finalBoss) {
                    Add(new Coroutine(HighlightFade(1f)));
                }
                yield return 0.2f;
                float timer = 0.4f;
                if (finalBoss) {
                    timer = 0.2f;
                }
                while (timer > 0f && PlayerWaitCheck()) {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }
                StopShaking();
                for (int i = 2; (float) i < Width; i += 4) {
                    if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f))) {
                        SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + (float) i, Y), Vector2.One * 4f, Consts.PIover2);
                    }
                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + (float) i, Y), Vector2.One * 4f);
                }
                float speed = 0f;
                while (true) {
                    Level level = SceneAs<Level>();
                    speed = Calc.Approach(speed, maxSpeed, accel * Engine.DeltaTime);
                    if (MoveVCollideSolids((float) Math.Sin(angle) * speed * Engine.DeltaTime, thruDashBlocks: true)) {
                        (base.Scene as Level).Session.SetFlag(flagOnGround, true);
                        break;
                    }
                    if (MoveHCollideSolids((float) Math.Cos(angle) * speed * Engine.DeltaTime, thruDashBlocks: true)) {
                        (base.Scene as Level).Session.SetFlag(flagOnGround, true);
                        break;
                    }
                    if (Top > (float) (level.Bounds.Bottom + 16) || (Top > (float) (level.Bounds.Bottom - 1) && CollideCheck<Solid>(ImpactCenter))) {
                        Collidable = (Visible = false);
                        yield return 0.2f;
                        if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f))) {
                            yield return 0.2f;
                            SceneAs<Level>().Shake();
                            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                        }
                        RemoveSelf();
                        DestroyStaticMovers();
                        yield break;
                    }
                    yield return null;
                }

                ImpactSfx();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().DirectionalShake(Vector2.UnitX.Rotate(angle), finalBoss ? 0.2f : 0.3f);
                if (finalBoss) {
                    Add(new Coroutine(HighlightFade(0f)));
                }
                StartShaking();
                LandParticles();
                yield return 0.2f;
                StopShaking();
                if (CollideCheck<SolidTiles>(Position + Vector2.UnitX.Rotate(angle))) {
                    break;
                }
                while (CollideCheck<Platform>(Position + Vector2.UnitX.Rotate(angle))) {
                    yield return 0.1f;
                }
            }
            Safe = true;
        }

        private IEnumerator HighlightFade(float to) {
            float from = highlight.Alpha;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.5f) {
                highlight.Alpha = MathHelper.Lerp(from, to, Ease.CubeInOut(p));
                tiles.Alpha = 1f - highlight.Alpha;
                yield return null;
            }
            highlight.Alpha = to;
            tiles.Alpha = 1f - to;
        }

        private void LandParticles() {
            for (int i = 2; (float) i <= base.Width; i += 4) {
                if (base.Scene.CollideCheck<Solid>(base.BottomLeft + new Vector2(i, 3f))) {
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(base.X + (float) i, base.Bottom), Vector2.One * 4f, -Consts.PIover2);
                    float direction = (!((float) i < base.Width / 2f)) ? 0f : Consts.PI;
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(base.X + (float) i, base.Bottom), Vector2.One * 4f, direction);
                }
            }
        }

        private void ShakeSfx() {
            Audio.Play(ShakeSFX, base.Center);
        }

        private void ImpactSfx() {
            Audio.Play(ImpactSFX, ImpactCenter);
        }
    }
}
