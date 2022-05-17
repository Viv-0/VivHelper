using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Collections;
using System.Reflection;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CustomFallingBlock")]
    [Tracked]
    public class CustomFallingBlock : Solid {
        private static Dictionary<string, float> angles = new Dictionary<string, float>() { { "right", 0f }, { "down", (float) Math.PI / 2f }, { "left", (float) Math.PI }, { "up", (float) Math.PI * 1.5f } };


        private Vector2 ImpactCenter {
            get {
                switch (angle) {
                    case 0f:
                        return TopLeft + new Vector2(base.Width, base.Height / 2f);
                    case (float) Math.PI / 2f:
                        return TopLeft + new Vector2(base.Width / 2f, base.Height);
                    case (float) Math.PI:
                        return TopLeft + new Vector2(0, base.Height / 2f);
                    case (float) Math.PI * 1.5f:
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

        private bool respawn;
        private float respawnTime;

        private bool DashBlock;

        private string flagTrigger;

        private string flagOnFall;

        private string flagOnGround;

        private bool flagOnly;

        private string ShakeSFX = "event:/game/general/fallblock_shake";

        private string ImpactSFX = "event:/game/general/fallblock_impact";

        private float angle;
        private string angletext;
        private float accel = 500f;
        private float maxSpeed = 160f;

        public bool IsTriggeredByCrushBlock;
        public bool metaTriggered;
        public bool legacy;

        public bool HasStartedFalling {
            get;
            private set;
        }

        public CustomFallingBlock(Vector2 position, EntityID id, char tile, int width, int height, bool finalBoss, bool behind, int climbFall, float respawnT, bool itbcb)
            : base(position, width, height, safe: false) {
            IsTriggeredByCrushBlock = itbcb;
            this.finalBoss = finalBoss;
            this.climbFall = climbFall > 0;
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
            if (climbFall == 2) {

                this.AddOrAddToSolidModifierComponent(new SolidModifierComponent(0, true, false));
            }
            if (respawnT >= 0) {
                respawn = true;
                respawnTime = respawnT;
            } else {
                respawn = false;
            }
        }

        public CustomFallingBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, new EntityID(data.Level.Name, data.ID), data.Char("tiletype", '3'), data.Width, data.Height, finalBoss: false, data.Bool("behind"), data.Bool("bufferClimbFall", false) ? 2 : data.Bool("climbFall", defaultValue: true) ? 1 : 0, data.Float("RespawnTime", -1), data.Bool("CrushBlockTrigger", false)) {
            accel = data.Float("Accel", 500f);
            maxSpeed = data.Float("MaxSpeed", 160f);
            ShakeSFX = data.Attr("ShakeSFX", "event:/game/general/fallblock_shake");
            ImpactSFX = data.Attr("ImpactSFX", "event:/game/general/fallblock_impact");
            angletext = data.Attr("Direction", "Down").ToLower();
            angle = angles[angletext];
            flagOnFall = data.Attr("FlagOnFall", "");
            flagTrigger = data.Attr("FlagTrigger", "");
            flagOnGround = data.Attr("FlagOnGround", "");
            flagOnly = data.Bool("flagOnly");
            legacy = !data.Bool("Legacy"); //well I fucked up the code but it's resolvable by just inverting this, 1.6.0.2 -> 1.6.0.3
            DashBlock = data.Bool("BreakDashBlocks", true);
        }

        public static CustomFallingBlock CreateFinalBossBlock(EntityData data, Vector2 offset) {
            return new CustomFallingBlock(data.Position + offset, new EntityID(data.Level.Name, data.ID), 'g', data.Width, data.Height, finalBoss: true, behind: false, climbFall: 1, -1, true);
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
            if (flagTrigger != "" && (base.Scene as Level).Session.GetFlag(flagTrigger) && !Triggered && !metaTriggered) { if (legacy) Triggered = true; else metaTriggered = true; }
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
                metaTriggered = true;
            }
        }

        private bool PlayerFallCheck() {
            if (metaTriggered && !legacy)
                return true;
            return flagOnly ? false : climbFall ? HasPlayerRider() : HasPlayerOnTop(); //Solid.HasPlayerRider, Solid.HasPlayerOnTop

        }
        //I'm gonna be real I don't know what I did to resolve this bug but it works
        private bool PlayerWaitCheck() {
            if (legacy ? metaTriggered : Triggered)
                return true;
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
            Triggered = true;
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
                        SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + (float) i, Y), Vector2.One * 4f, (float) Math.PI / 2f);
                    }
                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + (float) i, Y), Vector2.One * 4f);
                }
                float speed = 0f;
                while (true) {
                    Level level = SceneAs<Level>();
                    speed = Calc.Approach(speed, maxSpeed, accel * Engine.DeltaTime);
                    if (MoveVCollideSolids((float) Math.Sin(angle) * speed * Engine.DeltaTime, thruDashBlocks: DashBlock)) {
                        (base.Scene as Level).Session.SetFlag(flagOnGround, true);
                        break;
                    }
                    if (MoveHCollideSolids((float) Math.Cos(angle) * speed * Engine.DeltaTime, thruDashBlocks: DashBlock)) {
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
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(base.X + (float) i, base.Bottom), Vector2.One * 4f, -(float) Math.PI / 2f);
                    float direction = (!((float) i < base.Width / 2f)) ? 0f : ((float) Math.PI);
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
