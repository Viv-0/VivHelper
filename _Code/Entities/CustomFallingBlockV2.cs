using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Iced.Intel;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {

    // This is a complete restructuring of FallingBlock's codebase to enable an easier time reading modified components, chock full of comments (hopefully)

    // The main difference is I've removed the Coroutine Sequence from the FallingBlock in favor of locking it into an UpdateChain with States. This just allows me to break up the code and read what's going on when.
    // I've also added way more technical options to this version
    [CustomEntity("VivHelper/CustomFallingBlock3")]
    public class CustomFallingBlockV2 : Solid {
        // States cycle through Waiting -> (Trigger) PreShake -> Fall -> PostShake -> Waiting
        // Could I have used Flags with Shaking as a Flag? Yes. Does it matter at all? No. Who cares if it's 3 ASM cycles faster noone's counting the CPU cycle
        public enum States {
            Waiting = 0,
            BeforeShake = 1,
            Fall = 2,
            AfterShake = 3
        }

        public enum EndBehavior {
            RepeatLast = 0,
            Stop = 1,
            Loop = 2,
            Shatter = 3,
        }


        // The MovementOperator is a unique struct which defines exactly how it falls
        public struct MovementOperator {
            public Vector2 speed; // The speed + direction the block falls
            public bool needsTrigger; // Whether it needs a "manual trigger" or if it "falls on its own" (either from the player or other methods)
            public float ShakeTime; // How long it shakes for (before & after) when the Trigger is called

        }
        public bool Triggered;
        public int TriggerIndex = 0;
        public bool? FirstTrigger = null;
        public MovementOperator[] movementOperators;
        public EndBehavior endBehavior;
        private char tile, fallTile;
        private TileGrid tiles, fallTiles;
        private States state;
        private bool thruDashBlocks;
        private Vector2 speed;
        private string ShakeSFX, ImpactSFX;
        private bool @lock;
        public MovementOperator activeOperator => movementOperators[TriggerIndex];

        public bool IsActive() => state != States.Waiting;


        public CustomFallingBlockV2(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, data.Bool("safe", false)) {
            tile = data.Char("tiletype", '3');
            fallTile = data.Char("fallTiletype", '\0');
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(tile, (int) Width / 8, (int) Height / 8).TileGrid);
            if(fallTile != '\0') {
                Add(fallTiles = GFX.FGAutotiler.GenerateBox(fallTile, (int) Width / 8, (int) Height / 8).TileGrid);
                fallTiles.Alpha = 0;
            }
            Calc.PopRandom();
            state = States.Waiting; // Replaces the Sequence 
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, highPriority: false));
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tile];
            Depth = data.Int("Depth", -9000);
            if (bool.TryParse(data.Attr("firstTrigger", ""), out bool val))
                FirstTrigger = val;
            thruDashBlocks = data.Bool("thruDashBlocks", true);
            endBehavior = data.Enum<EndBehavior>("finalBehavior", EndBehavior.RepeatLast);
            string param = data.NoEmptyString("operations0", "270,180,0.4,true");
            movementOperators = GetOperatorsFromString_BETA(param, id);
        }

        private static MovementOperator[] GetOperatorsFromString_BETA(string input, EntityID id) {
            string[] splits = input.Split('|');
            MovementOperator[] outs = new MovementOperator[splits.Length];
            for(int i = 0; i < splits.Length; i++) {
                string sub = splits[i];
                if (string.IsNullOrWhiteSpace(sub))
                    continue;
                string[] sub2 = sub.Split(',');
                if (!(sub2.Length == 4 &&
                    float.TryParse(sub2[0].Trim(), out float angle) &&
                    float.TryParse(sub2[1].Trim(), out float pxPERs) &&
                    float.TryParse(sub2[2].Trim(), out float shakeTime) &&
                    bool.TryParse(sub2[3].Trim(), out bool needsTrigger)))
                    throw new InvalidParameterException($"Falling Block in room {id.Level} ID {id.ID} has invalid parameter `Move Sequence`: {{{sub}}}, {{{sub2}}}");
                outs[i] = new MovementOperator { speed = VivHelper.CleanUpVector(Calc.AngleToVector(-angle * Calc.DegToRad, pxPERs)), needsTrigger = needsTrigger, ShakeTime = shakeTime };
            }
            return outs;
        }

        public override void Update() {
            base.Update();
            bool needsTrigger = Triggered ? activeOperator.needsTrigger : FirstTrigger ?? activeOperator.needsTrigger;
            if (@lock)
                return;
            switch (state) {
                case States.Waiting:
                    speed = Vector2.Zero;
                    if (!needsTrigger || HasPlayerRider()) {
                        TryTrigger();
                    }
                    break;
                case States.BeforeShake:
                    if (!shaking) {
                        StopShaking();
                        shakeTimer = 0;
                        shaking = false;
                        state = States.Fall;
                        break;
                    }
                    break;
                case States.Fall:
                    if(speed == Vector2.Zero) {
                        ProduceFallParticles(activeOperator.speed);
                    }
                    speed = Calc.Approach(speed, activeOperator.speed, 500f * Engine.DeltaTime);
                    MoveHCollideSolids(speed.X * Engine.DeltaTime, thruDashBlocks, OnCollideFalling);
                    MoveVCollideSolids(speed.Y * Engine.DeltaTime, thruDashBlocks, OnCollideFalling);
                    Level level = Scene as Level;
                    if (!level.IsInBounds(this, 8) && (Collidable || Visible)) {
                        Collidable = Visible = false;
                        // Not identical to vanilla, but i dont think it makes a huge difference. This is more for immersion than anything else.
                        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => {
                            level.Shake();
                            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                            RemoveSelf();
                            DestroyStaticMovers();
                        }, 0.3f, true));
                        @lock = true;
                    }
                    break;
                case States.AfterShake:
                    if (!shaking) {
                        StopShaking();
                        shakeTimer = 0;
                        shaking = false;
                        state = States.Waiting;
                        Increment();
                    }
                    break;
            }
        }

        public void OnCollideFalling(Vector2 direction, Vector2 traveledDistance, Platform landedOn) {
            ImpactSfx(direction);
            if (activeOperator.ShakeTime <= 0) {
                state = States.Waiting;
                Increment();
            } else {
                state = States.AfterShake;
                StartShaking(activeOperator.ShakeTime);
            }
        }

        private void ShakeSfx() {
            Audio.Play(ShakeSFX, base.Center);
        }

        private void ImpactSfx(Vector2 direction) {
            Audio.Play(ImpactSFX, ImpactCenter(direction));
        }
        private Vector2 ImpactCenter(Vector2 direction) => direction.Angle() switch {
            Calc.Right => CenterRight,
            Calc.Up => TopCenter,
            Calc.Left => CenterLeft,
            _ => BottomCenter,
        };

        private void ProduceFallParticles(Vector2 direction) {
            Level level = Scene as Level;
            Vector2 nDir = Vector2.Normalize(direction);
            if (MathF.Abs(nDir.Y) > 0.1f) {
                for (float u =2; u <= Width - 2; u += 4) {
                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(Left + u, MathF.Sign(nDir.Y) < 0 ? Bottom : Top), new Vector2(2, 2));
                }
            }
            if (MathF.Abs(nDir.X) > 0.1f) {
                for (float v = 2; v <= Height - 2; v += 4) {
                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(MathF.Sign(nDir.X) > 0 ? Left : Right, Top + v), new Vector2(2, 2));
                }
            }
        }

        public bool TryTrigger() {
            if (TriggerIndex > movementOperators.Length || state != States.Waiting || !CanMove())
                return false;
            Triggered = true;
            if(activeOperator.ShakeTime <= 0f) {
                state = States.Fall;
            } else {
                state = States.BeforeShake;
                StartShaking(activeOperator.ShakeTime);
                ShakeSfx();
            }
            return true;
        }

        private bool CanMove() {
            Vector2 v = Vector2.Normalize(activeOperator.speed);
            float a = MathF.Max(Math.Abs(v.X), Math.Abs(v.Y));
            return !CollideCheck<Platform>(Position + (v / a)); // This should account for the next viable position to fall
        }

        public override void OnShake(Vector2 amount) {
            base.OnShake(amount);
            tiles.Position += amount;
            if (fallTiles != null) {
                fallTiles.Position += amount;
            }
        }

        public void Increment() {
            if (++TriggerIndex >= movementOperators.Length) {
                switch (endBehavior) {
                    case EndBehavior.Stop: break;
                    case EndBehavior.RepeatLast: TriggerIndex = movementOperators.Length - 1; break;
                    case EndBehavior.Loop: TriggerIndex = 0; break;
                    case EndBehavior.Shatter:
                        StartShaking(0.25f);
                        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, ()=>Break(Center, true, true), 0.25f, true));
                        @lock = true;
                        break;
                }
            }
        }
        public void Break(Vector2 from, bool playSound = true, bool playDebrisSound = true) {
            if (playSound) {
                if (tile == '1') {
                    Audio.Play("event:/game/general/wall_break_dirt", Position);
                } else if (tile == '3') {
                    Audio.Play("event:/game/general/wall_break_ice", Position);
                } else if (tile == '9') {
                    Audio.Play("event:/game/general/wall_break_wood", Position);
                } else {
                    Audio.Play("event:/game/general/wall_break_stone", Position);
                }
            }
            for (int i = 0; (float) i < base.Width; i+=8) {
                for (int j = 0; (float) j < base.Height; j+=8) {
                    base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i, 4 + j), tile, playDebrisSound).BlastFrom(from, (80,100)));
                }
            }
            Collidable = false;
            RemoveSelf();
        }

    }
}
