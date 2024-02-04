using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Collections;
using System.Reflection;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CrystalBombDetonatorNeutralizer")]
    public class CrystalBombDetonationNeutralizer : Actor //This name is extremely important, because of the Detonator recognizing that the FUllName contains CrystalBomb and not Detonator (Detonation != Detonator)
    {
        private enum RespawnTimes {
            Short = 5,
            Medium = 10,
            Long = 0xF
        }

        private enum ExplodeTimes {
            Short = 1,
            Medium = 2,
            Long = 5
        }

        [Pooled]
        private class RecoverBlast : Entity {
            private Sprite sprite;

            public override void Added(Scene scene) {
                base.Added(scene);
                base.Depth = -199;
                if (sprite == null) {
                    Add(sprite = GFX.SpriteBank.Create("seekerShockWave"));
                    sprite.OnLastFrame = delegate {
                        RemoveSelf();
                    };
                }
                sprite.Play("shockwave", restart: true);
                sprite.Rate = 5f;
            }

            public static void Spawn(Vector2 position) {
                RecoverBlast recoverBlast = Engine.Pooler.Create<RecoverBlast>();
                recoverBlast.Position = position;
                Engine.Scene.Add(recoverBlast);
            }
        }

        public Holdable Hold;

        private Sprite sprite;

        private Collision onCollideH;

        private Collision onCollideV;

        public Vector2 Speed;

        private float noGravityTimer;

        private Vector2 prevLiftSpeed;

        private float hardVerticalHitSoundCooldown;

        private Level Level;

        private bool exploded;

        private Vector2 previousPosition;

        private List<Spring> springs;

        private List<CassetteBlock> cassetteBlocks;
        private List<Entity> icyFloors;

        private Player playerEntity = null;

        private Circle pushRadius;

        private Hitbox hitBox;

        private Vector2 startPos;

        private float maxRespawnTime;

        private float respawnTime = 0f;

        private float frameCount;

        private float maxExplodeTimer;

        private float explodeTimer = 0f;

        private bool exploding = false;

        private bool explodeOnSpawn = false;

        private bool respawnOnExplode = true;

        private bool breakDashBlocks = false;
        private bool breakTempleCrackedBlocks = false;

        private BirdTutorialGui tutorialGui;

        private bool shouldShowTutorial = true;

        private bool playedFuseSound = false;

        private Level level => (Level) base.Scene;

        public CrystalBombDetonationNeutralizer(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            base.Depth = 100;
            maxRespawnTime = data.Float("respawnTime", 5f);
            maxExplodeTimer = data.Float("explodeTime", 1f);
            explodeOnSpawn = data.Bool("explodeOnSpawn");
            respawnOnExplode = data.Bool("respawnOnExplode", defaultValue: true);
            breakDashBlocks = data.Bool("breakDashBlocks");
            pushRadius = new Circle(40f);
            hitBox = new Hitbox(8f, 10f, -4f, -10f);
            base.Collider = hitBox;
            Add(sprite = VivHelperModule.spriteBank.Create("cbdn"));
            Add(Hold = new Holdable());
            Hold.PickupCollider = new Hitbox(16f, 16f, -8f, -16f);
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Hold.SpeedGetter = () => Speed;
            Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
            Add(new MirrorReflection());
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Level = SceneAs<Level>();
            if (level != null) {
            }
        }

        private void OnCollideSpring(Spring spring) {
            Sprite sprite = spring.Get<Sprite>();
            Spring.Orientations springOrientation = ((sprite.Rotation == Consts.PIover2) ? Spring.Orientations.WallLeft : ((sprite.Rotation == -Consts.PIover2) ? Spring.Orientations.WallRight : Spring.Orientations.Floor));
            Audio.Play("event:/game/general/spring", spring.BottomCenter);
            spring.Get<StaticMover>().TriggerPlatform();
            spring.Get<Wiggler>().Start();
            spring.Get<Sprite>().Play("bounce", restart: true);
            HitSpring(springOrientation);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            GetPlayer();
            springs = scene.Entities.OfType<Spring>().ToList();
            cassetteBlocks = scene.Entities.OfType<CassetteBlock>().ToList();
            startPos = Position;
            if (explodeOnSpawn) {
                exploding = true;
            }
            icyFloors = scene.Entities.Where((e) => e.GetType().Name.Contains("IcyFloor")).ToList();
        }

        private void OnPickup() {
            if (tutorialGui != null) {
                shouldShowTutorial = false;
            }
            Speed = Vector2.Zero;
            exploding = true;
        }

        private bool CheckForIcyFloor() {
            bool result = false;

            for (int i = 0; i < icyFloors.Count; i++) {
                if (CollideCheck(icyFloors[i])) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public void HitSpring(Spring.Orientations springOrientation) {
            if (!Hold.IsHeld) {
                if (springOrientation == Spring.Orientations.Floor) {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                } else {
                    Speed.X = 240 * ((springOrientation == Spring.Orientations.WallLeft) ? 1 : (-1));
                    Speed.Y = -140f;
                    noGravityTimer = 0.15f;
                }
            }
        }

        private void OnRelease(Vector2 force) {
            if (force.X != 0f && force.Y == 0f) {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero) {
                noGravityTimer = 0.1f;
            }
        }

        private void OnCollideH(CollisionData data) {
            if (data.Hit is DashSwitch) {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            if (Math.Abs(Speed.X) > 100f) {
                ImpactParticles(data.Direction);
            }
            Speed.X *= -0.4f;
        }

        private void OnCollideV(CollisionData data) {
            if (data.Hit is DashSwitch) {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0f && frameCount > 15f) {
                if (hardVerticalHitSoundCooldown <= 0f) {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                } else {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
                }
            }
            if (Speed.Y > 160f) {
                ImpactParticles(data.Direction);
            }
            if (CheckForIcyFloor()) {
                Speed.Y = 0f;
            } else if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch)) {
                Speed.Y *= -0.6f;
            } else {
                Speed.Y = 0f;
            }
        }

        private void ImpactParticles(Vector2 dir) {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f) {
                direction = Consts.PI;
                position = new Vector2(base.Right, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if (dir.X < 0f) {
                direction = 0f;
                position = new Vector2(base.Left, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if (dir.Y > 0f) {
                direction = -Consts.PIover2;
                position = new Vector2(base.X, base.Bottom);
                positionRange = Vector2.UnitX * 6f;
            } else {
                direction = Consts.PIover2;
                position = new Vector2(base.X, base.Top);
                positionRange = Vector2.UnitX * 6f;
            }
            Level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        private void Explode() {
            if (exploded) {
                return;
            }
            base.Collider = new Circle(10f);
            exploding = false;
            explodeTimer = 0f;
            bool flag = false;
            for (int i = 0; i < cassetteBlocks.Count; i++) {
                if (CollideCheck(cassetteBlocks[i])) {
                    flag = true;
                    break;
                }
            }
            base.Collider = pushRadius;
            sprite.Play("idle", restart: true);
            level.Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
            RecoverBlast.Spawn(Position);
            for (int j = 0; j < 10; j++) {
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
            }
            if (!flag) {
                CrystalDebris.Burst(Position + Vector2.UnitY * -10f, Calc.HexToColor("c763ff"), boss: false, 32);
            }
            Player player = CollideFirst<Player>();
            if (player != null && !base.Scene.CollideCheck<Solid>(Position + Vector2.UnitY * -10f, player.Center)) {
                player.ExplodeLaunch(Position, snapUp: true);
            }
            foreach (Entity entity1 in base.Scene.Tracker.GetEntities<CBDNTempleCrackedBlock>()) {
                CBDNTempleCrackedBlock templeCrackedBlock = (CBDNTempleCrackedBlock) entity1;
                if (CollideCheck(templeCrackedBlock)) {
                    templeCrackedBlock.Break(Position);
                }
            }
            foreach (CrystalBombDetonator2 entity2 in Scene.Tracker.GetEntities<CrystalBombDetonator2>()) {
                entity2.Collidable = true;
                if (CollideCheck(entity2)) {
                    entity2.Collidable = false;
                    entity2.Destroy();
                }
                entity2.Collidable = false;
            }
            if (breakTempleCrackedBlocks) {
                foreach (Entity entity in base.Scene.Tracker.GetEntities<TempleCrackedBlock>()) {
                    TempleCrackedBlock templeCrackedBlock = (TempleCrackedBlock) entity;
                    if (CollideCheck(templeCrackedBlock)) {
                        templeCrackedBlock.Break(Position);
                    }
                }
            }
            if (breakDashBlocks) {
                foreach (Entity entity2 in base.Scene.Tracker.GetEntities<DashBlock>()) {
                    DashBlock dashBlock = (DashBlock) entity2;
                    if (CollideCheck(dashBlock)) {
                        dashBlock.Break(Position, Position - dashBlock.Position, true);
                    }
                }
            }
            if (!respawnOnExplode) {
                RemoveSelf();
            }
            exploded = true;
            base.Collider = hitBox;
            Visible = false;
            Collidable = false;
            Speed = Vector2.Zero;
            if (player != null && player.Holding != null && player.Holding == Hold) {
                Hold.Release(Vector2.Zero);
                player.Holding = null;
                player.Get<Sprite>().Update();
            }
            Position = startPos;
        }

        protected override void OnSquish(CollisionData data) {
            if (!TrySquishWiggle(data)) {
                Explode();
            }
        }

        private void GetPlayer() {
            List<Player> list = level.Entities.OfType<Player>().ToList();
            if (list.Count > 0) {
                playerEntity = list[0];
            }
        }

        public override void Update() {
            base.Update();
            frameCount += 1f;
            if (playerEntity == null) {
                GetPlayer();
            }
            if (!exploded) {
                if (!Hold.IsHeld) {
                    foreach (Spring spring in springs) {
                        if (CollideCheck(spring)) {
                            OnCollideSpring(spring);
                        }
                    }
                    if (tutorialGui != null) {
                        if (shouldShowTutorial) {
                            tutorialGui.Open = (playerEntity.Position - Position).Length() < 64f;
                        } else {
                            tutorialGui.Open = false;
                        }
                    }
                }
                if (exploding) {
                    if (!playedFuseSound) {
                        Audio.Play("event:/game/04_cliffside/arrowblock_debris", Position);
                        playedFuseSound = true;
                    }
                    explodeTimer += Engine.DeltaTime;
                    sprite.Play("crbomb");
                    sprite.SetAnimationFrame((int) Math.Floor(explodeTimer / maxExplodeTimer * 60f));
                    if (explodeTimer >= maxExplodeTimer) {
                        Explode();
                    }
                }
                hardVerticalHitSoundCooldown -= Engine.DeltaTime;
                base.Depth = 100;
                if (Hold.IsHeld) {
                    prevLiftSpeed = Vector2.Zero;
                    return;
                }
                if (OnGround()) {
                    float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime * (CheckForIcyFloor() ? 0.01f : 1f));
                    Vector2 liftSpeed = base.LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero) {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f) {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f) {
                            noGravityTimer = 0.15f;
                        }
                    } else {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f) {
                            Speed.Y = 0f;
                        }
                    }
                } else if (Hold.ShouldHaveGravity) {
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f) {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f) {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f) {
                        noGravityTimer -= Engine.DeltaTime;
                    } else {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                previousPosition = base.ExactPosition;
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                if (base.Center.X > (float) Level.Bounds.Right) {
                    MoveH(32f * Engine.DeltaTime);
                    if (base.Right > (float) Level.Bounds.Right) {
                        base.Right = Level.Bounds.Right;
                        Speed.X *= -0.4f;
                    }
                } else if (base.Left < (float) Level.Bounds.Left) {
                    base.Left = Level.Bounds.Left;
                    Speed.X *= -0.4f;
                } else if (base.Top < (float) (Level.Bounds.Top - 4)) {
                    base.Top = Level.Bounds.Top + 4;
                    Speed.Y = 0f;
                } else if (base.Bottom > (float) Level.Bounds.Bottom + 4f) {
                    if (!exploded) {
                        Explode();
                    }
                    base.Bottom = Level.Bounds.Bottom - 4;
                }
                if (base.X < (float) (Level.Bounds.Left + 10)) {
                    MoveH(32f * Engine.DeltaTime);
                }
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                TempleGate templeGate = CollideFirst<TempleGate>();
                if (templeGate != null && entity != null) {
                    templeGate.Collidable = false;
                    MoveH((float) (Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime);
                    templeGate.Collidable = true;
                }
            } else if (respawnTime < maxRespawnTime) {
                respawnTime += Engine.DeltaTime;
            } else {
                respawnTime = 0f;
                exploded = false;
                Visible = true;
                Collidable = true;
                playedFuseSound = false;
                sprite.Play("idle", restart: true);
                Position = startPos;
                if (explodeOnSpawn) {
                    exploding = true;
                }
            }
        }
    }
}
