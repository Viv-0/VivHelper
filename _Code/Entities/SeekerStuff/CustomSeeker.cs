using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.VivHelper;
using VivHelper.Entities.SeekerStuff;
using Celeste.Mod;
using System.IO;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity(
        "VivHelper/CustomSeekerS = Simple",
        "VivHelper/CustomSeeker = Custom",
        "VivHelper/CustomSeekerYaml = Yaml")]
    public class CustomSeeker : Actor {
        public static void Load() { IL.Celeste.SeekerEffectsController.Update += SeekerEffectsController_Update; }

        public static void Unload() { IL.Celeste.SeekerEffectsController.Update -= SeekerEffectsController_Update; }

        private static void SeekerEffectsController_Update(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(instr => instr.MatchLdloc(5), instr => instr.MatchLdcR4(0), instr => instr.MatchBltUn(out _))) {
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.Emit(OpCodes.Ldloca, 4);
                cursor.Emit(OpCodes.Ldloca, 5);
                cursor.Emit(OpCodes.Call, typeof(CustomSeeker).GetMethod(nameof(AddSeekerCheck)));
            }
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Tracker>("CountEntities"))) {
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.Emit(OpCodes.Call, typeof(CustomSeeker).GetMethod(nameof(AddSeekerCount)));
            }
        }

        public static void AddSeekerCheck(Player player, ref float num, ref float num2) {
            foreach (CustomSeeker entity2 in player.Scene.Tracker.GetEntities<CustomSeeker>()) {
                float num3 = Vector2.DistanceSquared(player.Center, entity2.Center);
                if (!entity2.Regenerating) {
                    num = ((!(num < 0f)) ? Math.Min(num, num3) : num3);
                }
                if (entity2.Attacking) {
                    num2 = ((!(num2 < 0f)) ? Math.Min(num2, num3) : num3);
                }
            }
        }

        public static int AddSeekerCount(int i, Player player) => i + (player?.Scene?.Tracker?.TryCountEntities<CustomSeeker>() ?? 0);

        protected struct PatrolPoint {
            public Vector2 Point;

            public float Distance;
        }

        [Pooled]
        protected class RecoverBlast : Entity {
            private Sprite sprite;
            private string CustomShockwavePath;

            public override void Added(Scene scene) {
                base.Added(scene);
                base.Depth = -199;
                if (sprite == null) {
                    sprite = GFX.SpriteBank.Create(CustomShockwavePath);
                }
                Add(sprite);
                sprite.OnLastFrame = delegate {
                    RemoveSelf();
                };
                sprite.Play("shockwave", restart: true);

            }

            public static void Spawn(Vector2 position, string t) {
                RecoverBlast recoverBlast = Engine.Pooler.Create<RecoverBlast>();
                recoverBlast.CustomShockwavePath = t != null && t != "" ? t : "seekerShockWave";
                recoverBlast.Position = position;
                Engine.Scene.Add(recoverBlast);
            }
        }

        public static ParticleType P_BreakOut = Seeker.P_BreakOut;

        public Color TrailColor = Calc.HexToColor("99e550");

        private const int StIdle = 0;

        private const int StPatrol = 1;

        private const int StSpotted = 2;

        private const int StAttack = 3;

        private const int StStunned = 4;

        private const int StSkidding = 5;

        private const int StRegenerate = 6;

        private const int StReturned = 7;

        private const int size = 12;

        private const int bounceWidth = 16;

        private const int bounceHeight = 4;

        protected float Accel = 600f;

        protected float WallCollideStunThreshold = 100f;

        protected float StunXSpeed = 100f;

        protected float BounceSpeed = 200f;

        protected float SightDistSq = 25600f;

        protected float ExplodeRadius = 40f;

        protected Hitbox physicsHitbox;

        protected Hitbox breakWallsHitbox;

        protected Hitbox attackHitbox;

        protected Hitbox bounceHitbox;

        protected Circle pushRadius;

        protected Circle breakWallsRadius;

        protected StateMachine State;

        protected Vector2 lastSpottedAt;

        protected Vector2 lastPathTo;

        protected bool spotted;

        protected bool canSeePlayer;

        protected Collision onCollideH;

        protected Collision onCollideV;

        protected Random random;

        protected Vector2 lastPosition;

        protected Shaker shaker;

        protected Wiggler scaleWiggler;

        protected bool lastPathFound;

        protected List<Vector2> path;

        protected int pathIndex;

        protected Vector2[] patrolPoints;

        protected SineWave idleSineX;

        protected SineWave idleSineY;

        public VertexLight Light;

        protected bool dead;

        protected SoundSource boopedSfx;

        protected SoundSource aggroSfx;

        protected SoundSource reviveSfx;

        protected Sprite sprite;

        protected int facing = 1;

        protected int spriteFacing = 1;

        protected string nextSprite;

        protected HoldableCollider theo;

        protected HashSet<string> flipAnimations = new HashSet<string>
    {
        "flipMouth",
        "flipEyes",
        "skid"
    };

        public Vector2 Speed;

        protected float FarDistSq = 12544f;

        protected float IdleAccel = 200f;

        protected float IdleSpeed = 50f;

        protected float PatrolSpeed = 25f;

        protected const int PatrolChoices = 3;

        protected float PatrolWaitTime = 0.4f;

        protected static PatrolPoint[] patrolChoices = new PatrolPoint[3];

        protected float patrolWaitTimer;

        protected float SpottedTargetSpeed = 60f;

        protected float SpottedMaxYDist = 24f;

        protected float AttackMinXDist = 16f;

        protected float SpottedLosePlayerTime = 0.6f;

        protected float SpottedMinAttackTime = 0.2f;

        protected float spottedLosePlayerTimer;

        protected float spottedTurnDelay;

        protected float AttackWindUpSpeed = -60f;

        protected float AttackWindUpTime = 0.3f;

        protected float AttackStartSpeed = 180f;

        protected float AttackTargetSpeed = 260f;

        protected float AttackAccel = 300f;

        protected float DirectionDotThreshold = 0.4f;

        protected const int AttackTargetUpShift = 2;

        protected float AttackMaxRotateRadians = 0.610865235f;

        protected float attackSpeed;

        protected bool attackWindUp;

        protected float StunnedAccel = 150f;

        protected float StunTime = 0.8f;

        protected float SkiddingAccel = 200f;

        protected float StrongSkiddingAccel = 400f;

        protected float StrongSkiddingTime = 0.08f;

        protected bool strongSkid;

        protected float FarSpeedMultiplier = 2f;

        protected float XDistanceFromWindUp = 36f;

        protected string FlagOnDeath = "";

        public bool legacyPathfindingBehavior;

        public bool Attacking {
            get {
                if (State.State == 3) {
                    return !attackWindUp;
                }
                return false;
            }
        }

        public bool Spotted {
            get {
                if (State.State != 3) {
                    return State.State == 2;
                }
                return true;
            }
        }

        public bool Regenerating => State.State == 6;

        protected Vector2 FollowTarget => lastSpottedAt - Vector2.UnitY * 2f;

        public int numberOfDashes, numberOfBounces, numberOfWallCollides;
        public int maxNumberOfDashes, maxNumberOfBounces, maxNumberOfWallCollides;
        public float skiddingDelay;
        public float attackCooldown;
        public bool finalDash, DisableEffects;
        protected string CustomSpritePath;
        protected string CustomShockwavePath;
        protected float ParticleEmitInterval;
        protected float TrailCreateInterval;
        protected float RegenerateTimerMult = 1f;
        protected bool disableAllParticles;
        protected bool IgnoreCamera;
        protected bool AlwaysSeePlayer;
        protected Color tint;
        protected bool RemoveBounceHitbox;
        protected Color DeathEffectColor;
        protected string boopedSFX = "event:/game/05_mirror_temple/seeker_booped";
        protected string aggroSFX = "event:/game/05_mirror_temple/seeker_aggro";
        protected string reviveSFX = "event:/game/05_mirror_temple/seeker_revive";

        protected int StartingState;
        protected float ActorDelay;
        public bool generatorTag;
        public float deLagCounter = 0;
        public float deLagValue = 0.1f;

        public static List<CustomSeeker> CustomSeekersList;

        public static Entity Simple(Level level, LevelData levelData, Vector2 offset, EntityData entityData) =>
            new CustomSeeker(entityData, offset);

        public static Entity Custom(Level level, LevelData levelData, Vector2 offset, EntityData entityData) =>
            new CustomSeeker(entityData, offset);
        public static Entity Yaml(Level level, LevelData levelData, Vector2 offset, EntityData entityData) =>
            new CustomSeeker(LoadYaml(entityData.Attr("YamlPath")).SeekerDataFromYaml(entityData.Position), offset);

        public CustomSeeker(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            Accel = data.Float("Accel", 600f);
            WallCollideStunThreshold = data.Float("WallCollideStunThreshold", 100f);
            StunXSpeed = data.Float("StunXSpeed", 100f);
            BounceSpeed = data.Float("BounceSpeed", 200f);
            SightDistSq = data.Float("SightDistance", 160f) * data.Float("SightDistance", 160f);
            ExplodeRadius = data.Float("ExplodeRadius", 40f);
            StrongSkiddingTime = data.Float("StrongSkiddingTime", 0.08f);
            FarDistSq = data.Float("FarDist", 112f) * data.Float("FarDist", 112f);
            IdleAccel = data.Float("IdleAccel", 200f);
            IdleSpeed = data.Float("IdleSpeed", 50f);
            SkiddingAccel = data.Float("SkiddingAccel", 200f);
            StrongSkiddingAccel = data.Float("StrongSkiddingAccel", 2 * SkiddingAccel);
            PatrolSpeed = data.Float("PatrolSpeed", 25f);
            PatrolWaitTime = data.Float("PatrolWaitTime", 0.4f);
            SpottedTargetSpeed = data.Float("SpottedTargetSpeed", 60f);
            FarSpeedMultiplier = data.Float("FarDistSpeedMult", 2f);
            DirectionDotThreshold = data.Float("DirectionDotThreshold", 0.4f);
            AttackMinXDist = data.Float("AttackMinXDist", 16f);
            SpottedMaxYDist = data.Float("SpottedMaxYDist", 24f);
            SpottedLosePlayerTime = data.Float("SpottedLosePlayerTime", 0.6f);
            SpottedMinAttackTime = data.Float("SpottedMinAttackTime", 0.2f);
            AttackWindUpSpeed = 0 - data.Float("AttackWindUpSpeed", 60f);
            AttackWindUpTime = data.Float("AttackWindUpTime", 0.3f);
            XDistanceFromWindUp = data.Float("XDistanceFromWindUp", 36f);
            AttackStartSpeed = data.Float("AttackStartSpeed", 180f);
            AttackTargetSpeed = data.Float("AttackTargetSpeed", 260f);
            AttackAccel = data.Float("AttackAccel", 300f);
            StunnedAccel = data.Float("StunnedAccel", 150f);
            StunTime = data.Float("StunTime", 0.8f);
            disableAllParticles = data.Bool("DisableAllParticles", false);
            ParticleEmitInterval = disableAllParticles ? 0 : data.Float("ParticleEmitInterval", 0.04f);
            TrailCreateInterval = data.Float("TrailCreateInterval", 0.06f);
            RegenerateTimerMult = data.Float("RegenerationTimerLength", 1.85f) / 1.85f;
            StrongSkiddingTime = data.Float("StrongSkiddingTime", 0.08f);
            AttackMaxRotateRadians = data.Float("AttackMaxRotateDegrees", 35f) * Calc.DegToRad;
            TrailColor = VivHelper.OldColorFunction(data.Attr("TrailColor", "99e550"));
            numberOfDashes = 0;
            maxNumberOfDashes = data.Int("MaxNumberOfDashes", -1);
            finalDash = data.Bool("FinalDash", false);
            numberOfWallCollides = 0;
            maxNumberOfWallCollides = data.Int("MaxNumberOfWallCollides", -1);
            numberOfBounces = 0;
            maxNumberOfBounces = data.Int("MaxNumberOfBounces", -1);
            legacyPathfindingBehavior = !data.Bool("NewPathfindingBehavior", false);
            AlwaysSeePlayer = data.Bool("AlwaysSeePlayer");
            IgnoreCamera = data.Bool("SpottedNoCameraLimit");
            deLagValue = Calc.Clamp((float) data.Int("aiDelag", 6), 1, 30) / 60f;
            DisableEffects = data.Bool("DisableEffects", false);

            DeathEffectColor = VivHelper.OldColorFunction(data.Attr("DeathEffectColor", "HotPink"));
            RemoveBounceHitbox = data.Bool("RemoveBounceHitbox", false);
            FlagOnDeath = data.Attr("FlagOnDeath", "");



            string a = data.Attr("boopedSFXPath", "");
            boopedSFX = a == "" ? "event:/game/05_mirror_temple/seeker_booped" : a;
            string b = data.Attr("aggroSFXPath", "");
            aggroSFX = b == "" ? "event:/game/05_mirror_temple/seeker_aggro" : b;
            string c = data.Attr("reviveSFXPath", "");
            reviveSFX = c == "" ? "event:/game/05_mirror_temple/seeker_revive" : c;


            CustomSpritePath = data.Attr("CustomSpritePath");
            CustomShockwavePath = data.Attr("CustomShockwavePath");
            sprite = CustomSpritePath == "" ? GFX.SpriteBank.Create("seeker") : GFX.SpriteBank.Create(CustomSpritePath);
            tint = VivHelper.OldColorFunction(data.Attr("SeekerColorTint", "ffffff"));

            sprite.Color = tint;
            Vector2 position = data.Position + offset;
            //Regular Constructor
            base.Depth = -200;
            this.patrolPoints = data.NodesOffset(offset);
            lastPosition = position;
            base.Collider = (physicsHitbox = new Hitbox(6f, 6f, -3f, -3f));
            breakWallsHitbox = new Hitbox(6f, 14f, -3f, -7f);
            attackHitbox = RemoveBounceHitbox ? new Hitbox(16f, 14f, -8f, -8f) : new Hitbox(12f, 8f, -6f, -2f);
            bounceHitbox = RemoveBounceHitbox ? new Hitbox(8f, 7f, -4f, -3.5f) : new Hitbox(16f, 6f, -8f, -8f);
            pushRadius = new Circle(ExplodeRadius);
            breakWallsRadius = new Circle(16f);
            Add(new PlayerCollider(OnAttackPlayer, attackHitbox));
            Add(new PlayerCollider(OnBouncePlayer, bounceHitbox));
            Add(shaker = new Shaker(on: false));
            Add(State = new StateMachine());
            State.SetCallbacks(0, IdleUpdate, IdleCoroutine);
            State.SetCallbacks(1, PatrolUpdate, null, PatrolBegin);
            State.SetCallbacks(2, SpottedUpdate, SpottedCoroutine, SpottedBegin);
            State.SetCallbacks(3, AttackUpdate, AttackCoroutine, AttackBegin);
            State.SetCallbacks(4, StunnedUpdate, StunnedCoroutine);
            State.SetCallbacks(5, SkiddingUpdate, SkiddingCoroutine, SkiddingBegin, SkiddingEnd);
            State.SetCallbacks(6, RegenerateUpdate, RegenerateCoroutine, RegenerateBegin, RegenerateEnd);
            State.SetCallbacks(7, null, ReturnedCoroutine);
            State.State = StartingState = data.Int("StartingState", 0);

            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Add(idleSineX = new SineWave(0.5f));
            Add(idleSineY = new SineWave(0.7f));
            Add(Light = new VertexLight(Color.White, 1f, 32, 64));
            Add(theo = new HoldableCollider(OnHoldable, attackHitbox));
            Add(new MirrorReflection());
            path = new List<Vector2>();
            IgnoreJumpThrus = true;
            Add(sprite);
            sprite.OnLastFrame = delegate (string f) {
                if (flipAnimations.Contains(f) && spriteFacing != facing) {
                    spriteFacing = facing;
                    if (nextSprite != null) {
                        sprite.Play(nextSprite);
                        nextSprite = null;
                    }
                }
            };
            sprite.OnChange = delegate (string last, string next) {
                nextSprite = null;
                sprite.OnLastFrame(last);
            };
            SquishCallback = delegate (CollisionData d) {
                if (!dead && !TrySquishWiggle(d)) {
                    SeekerDeath();
                }
            };
            scaleWiggler = Wiggler.Create(0.8f, 2f);
            Add(scaleWiggler);
            Add(boopedSfx = new SoundSource());
            Add(aggroSfx = new SoundSource());
            Add(reviveSfx = new SoundSource());
            CustomSeekersList = new List<CustomSeeker>();
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            random = new Random(SceneAs<Level>().Session.LevelData.LoadSeed);
            Level level = scene as Level;
            if ((level.Session.MapData.Meta?.SeekerSlowdown).GetValueOrDefault() && Scene.Entities.AmountOf<CustomSeekerEffectsController>() == 0) {
                level.Add(new CustomSeekerEffectsController());
            }
            CustomSeekersList.Add(this);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity == null || base.X == entity.X) {
                SnapFacing(1f);
            } else {
                SnapFacing(Math.Sign(entity.X - base.X));
            }
            if (FlagOnDeath != "")
                (scene as Level).Session.SetFlag(FlagOnDeath, false);
        }

        public override bool IsRiding(JumpThru jumpThru) {
            return false;
        }

        public override bool IsRiding(Solid solid) {
            return false;
        }

        protected virtual void SeekerDeath() {
            Entity entity = new Entity(Position);
            DeathEffect component = new DeathEffect(DeathEffectColor, base.Center - Position) {
                OnEnd = delegate {
                    entity.RemoveSelf();
                }
            };
            entity.Add(component);
            entity.Depth = -1000000;
            base.Scene.Add(entity);
            Audio.Play("event:/game/05_mirror_temple/seeker_death", Position);
            if (FlagOnDeath != "") {
                (Scene as Level).Session.SetFlag(FlagOnDeath);
            }
            RemoveSelf();
            dead = true;
        }

        protected virtual void OnAttackPlayer(Player player) {
            if (State.State != 4) {
                player.Die((player.Center - Position).SafeNormalize());
                return;
            }
            Collider collider = base.Collider;
            base.Collider = bounceHitbox;
            player.PointBounce(base.Center);
            Speed = (base.Center - player.Center).SafeNormalize(100f);
            scaleWiggler.Start();
            base.Collider = collider;
        }

        protected virtual void OnBouncePlayer(Player player) {
            Collider collider = base.Collider;
            base.Collider = attackHitbox;
            if (CollideCheck(player)) {
                OnAttackPlayer(player);
            } else {
                player.Bounce(base.Top);
                GotBouncedOn(player);
            }
            base.Collider = collider;
        }

        protected virtual void GotBouncedOn(Entity entity) {
            Celeste.Celeste.Freeze(0.15f);
            Speed = (base.Center - entity.Center).SafeNormalize(BounceSpeed);
            State.State = 6;
            sprite.Scale = new Vector2(1.4f, 0.6f);
            if (!disableAllParticles) {
                SceneAs<Level>().Particles.Emit(Seeker.P_Stomp, 8, base.Center - Vector2.UnitY * 5f, new Vector2(6f, 3f));
            }
        }

        protected virtual void HitSpring() {
            Speed.Y = -150f;
        }

        private bool StupidCanSeePlayerFix(Player player) {
            if ((player.Scene as Level).Session.Area.SID == "Tardigrade/WaterbearMountain/WaterbearMountain")
                return !SceneAs<Level>().InsideCamera(base.Center) || Vector2.DistanceSquared(base.Center, player.Center) > SightDistSq;
            else if (IgnoreCamera)
                return Vector2.DistanceSquared(base.Center, player.Center) > SightDistSq;
            else
                return !SceneAs<Level>().InsideCamera(base.Center) && Vector2.DistanceSquared(base.Center, player.Center) > SightDistSq;


        }

        protected virtual bool CanSeePlayer(Player player) {
            if (player == null) {
                return false;
            }
            if (State.State != 2 && StupidCanSeePlayerFix(player))
                return false;
            Vector2 value = (player.Center - base.Center).Perpendicular().SafeNormalize(2f);
            if (!base.Scene.CollideCheck<Solid>(base.Center + value, player.Center + value)) {
                return !base.Scene.CollideCheck<Solid>(base.Center - value, player.Center - value);
            }
            return false;
        }

        public override void Update() {
            Light.Alpha = Calc.Approach(Light.Alpha, 1f, Engine.DeltaTime * 2f);
            foreach (Entity entity2 in base.Scene.Tracker.GetEntities<SeekerBarrier>()) {
                entity2.Collidable = true;
            }
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
            if (State.State == 6) {
                canSeePlayer = false;
            } else {
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                canSeePlayer = CanSeePlayer(entity);
                if (canSeePlayer) {
                    spotted = true;
                    lastSpottedAt = entity.Center;
                }
            }
            if (lastPathTo != lastSpottedAt) {
                deLagCounter -= Engine.DeltaTime;
                if (deLagCounter <= 0) {
                    lastPathTo = lastSpottedAt;
                    pathIndex = 0;
                    lastPathFound = SceneAs<Level>().Pathfinder.Find(ref path, base.Center, FollowTarget);
                    deLagCounter = deLagValue;
                }

            }
            base.Update();
            lastPosition = Position;
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            Level level = SceneAs<Level>();
            if (base.Left < (float) level.Bounds.Left && Speed.X < 0f) {
                base.Left = level.Bounds.Left;
                onCollideH(CollisionData.Empty);
            } else if (base.Right > (float) level.Bounds.Right && Speed.X > 0f) {
                base.Right = level.Bounds.Right;
                onCollideH(CollisionData.Empty);
            }
            if (base.Top < (float) (level.Bounds.Top + -8) && Speed.Y < 0f) {
                base.Top = level.Bounds.Top + -8;
                onCollideV(CollisionData.Empty);
            } else if (base.Bottom > (float) level.Bounds.Bottom && Speed.Y > 0f) {
                base.Bottom = level.Bounds.Bottom;
                onCollideV(CollisionData.Empty);
            }
            foreach (CustomSeekerCollider component in base.Scene.Tracker.GetComponents<CustomSeekerCollider>()) {
                component.Check(this);
            }

            MimicSeeker();
            if (State.State == 3 && Speed.X > 0f) {
                bounceHitbox.Width = 16f;
                bounceHitbox.Position.X = -10f;
            } else if (State.State == 3 && Speed.Y < 0f) {
                bounceHitbox.Width = 16f;
                bounceHitbox.Position.X = -6f;
            } else {
                bounceHitbox.Width = 12f;
                bounceHitbox.Position.X = -6f;
            }
            foreach (Entity entity3 in base.Scene.Tracker.GetEntities<SeekerBarrier>()) {
                entity3.Collidable = false;
            }
        }

        private void MimicSeeker() {
            List<SeekerCollider> l = CollideAllByComponent<SeekerCollider>();

            if (l.Count > 0) {
                Seeker seeker = new Seeker(Position, patrolPoints);
                foreach (SeekerCollider s in l) {
                    s.Check(seeker);
                }

            }
        }

        private static Type[] ignoredTypes = new Type[] { typeof(Collider), typeof(Shaker), typeof(Collision), typeof(HashSet<>), typeof(SoundSource) };
        /*
		private static FieldInfo _seeker_aggroSfx = typeof(Seeker).GetField("aggroSfx", BindingFlags.Instance | BindingFlags.NonPublic);
		private static FieldInfo _seeker_boopedSfx = typeof(Seeker).GetField("boopedSfx", BindingFlags.Instance | BindingFlags.NonPublic);
		private static FieldInfo _seeker_reviveSfx = typeof(Seeker).GetField("reviveSfx", BindingFlags.Instance | BindingFlags.NonPublic);

		private Seeker SeekerFromCustomSeeker()
		{
			Seeker seeker = new Seeker(Position, patrolPoints);
			DynamicData dyn = new DynamicData(this);
			int a = MimicList.Count < State.State ? 0 : MimicList[State.State];
			
			DynData<Entity> dynData_Seeker = new DynData<Entity>(seeker);
			dynData_Seeker.Set<Scene>("Scene", Scene);
			if (a < 0) a = 0;
			/*Fixed a bug with soundsources without crashing the game
			new DynamicData(typeof(SoundSource), (SoundSource)_seeker_aggroSfx.GetValue(seeker)).Set("VivHelper_soundsource_seeker_hackfix", true);
			new DynamicData(typeof(SoundSource), (SoundSource)_seeker_boopedSfx.GetValue(seeker)).Set("VivHelper_soundsource_seeker_hackfix", true);
			new DynamicData(typeof(SoundSource), (SoundSource)_seeker_reviveSfx.GetValue(seeker)).Set("VivHelper_soundsource_seeker_hackfix", true);

			//You might ask why I'm doing this. a: I want to. b: Futureproofing. It's relevant to ensuring that seekercolliders that in some way affect the seeker affects the CustomSeeker in the same way
			foreach (FieldInfo b in typeof(Seeker).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				Type c = b.FieldType;
				if (ignoredTypes.Any((d) => !c.IsAssignableFrom(d)) || b.Name == "patrolPoints")
				{
					if (c == typeof(StateMachine))
					{
						StateMachine d = (StateMachine)b.GetValue(seeker);
						d.State = 0; // ****************** THIS NEEDS CHANGING!!!! this must be changed to `a` once I can fix audio problems *****************************************************************************
					}
					else
                    {
						b.SetValue(seeker, dyn.Get(b.Name));
                    }
				}
			}
			//okay how in the hell did this actually work wHAT
			seeker.Visible = false;
			seeker.Collidable = Collidable;
			
			

			return seeker;

		}

		private void CopyBackSeeker(Seeker seeker)
        {
			DynamicData dyn = new DynamicData(typeof(CustomSeeker), this);
			foreach (FieldInfo b in typeof(Seeker).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				Type c = b.FieldType;
				if (ignoredTypes.Any((d) => !c.IsAssignableFrom(d)) || b.Name == "patrolPoints" || b.Name == "Visible")
				{
					if (c == typeof(StateMachine))
					{
						StateMachine d = (StateMachine)b.GetValue(seeker);
						State.State = d.State; // ****************** THIS NEEDS CHANGING!!!! this must be changed to `a` once I can fix audio problems *****************************************************************************
					}
					else
					{
						dyn.Set(b.Name, b.GetValue(seeker));
					}
				}
			}
		}*/

        protected virtual void TurnFacing(float dir, string gotoSprite = null) {
            if (dir != 0f) {
                facing = Math.Sign(dir);
            }
            if (spriteFacing != facing) {
                if (State.State == 5) {
                    sprite.Play("skid");
                } else if (State.State == 3 || State.State == 2) {
                    sprite.Play("flipMouth");
                } else {
                    sprite.Play("flipEyes");
                }
                nextSprite = gotoSprite;
            } else if (gotoSprite != null) {
                sprite.Play(gotoSprite);
            }
        }

        protected virtual void SnapFacing(float dir) {
            if (dir != 0f) {
                spriteFacing = (facing = Math.Sign(dir));
            }
        }

        protected virtual void OnHoldable(Holdable holdable) {
            if (State.State != 6 && holdable.Dangerous(theo)) {
                Seeker s = new Seeker(this.Position, this.patrolPoints);
                holdable.HitSeeker(s);
                s.RemoveSelf();
                State.State = 4;
                Speed = (base.Center - holdable.Entity.Center).SafeNormalize(120f);
                scaleWiggler.Start();
            } else if ((State.State == 3 || State.State == 5) && holdable.IsHeld) {
                holdable.Swat(theo, Math.Sign(Speed.X));
                State.State = 4;
                Speed = (base.Center - holdable.Entity.Center).SafeNormalize(120f);
                scaleWiggler.Start();
            }
        }

        public override void Render() {
            Vector2 position = Position;
            Position += shaker.Value;
            Vector2 scale = sprite.Scale;
            sprite.Scale *= 1f - 0.3f * scaleWiggler.Value;
            sprite.Scale.X *= spriteFacing;
            base.Render();
            Position = position;
            sprite.Scale = scale;
        }

        public override void DebugRender(Camera camera) {
            Collider collider = base.Collider;
            base.Collider = attackHitbox;
            attackHitbox.Render(camera, Color.Red);
            base.Collider = bounceHitbox;
            bounceHitbox.Render(camera, Color.Aqua);
            base.Collider = collider;
        }

        protected virtual void SlammedIntoWall(CollisionData data) {
            float direction;
            float x;
            if (data.Direction.X > 0f) {
                direction = Consts.PI;
                x = base.Right;
            } else {
                direction = 0f;
                x = base.Left;
            }
            if (!disableAllParticles) { SceneAs<Level>().Particles.Emit(Seeker.P_HitWall, 12, new Vector2(x, base.Y), Vector2.UnitY * 4f, direction); }
            if (data.Hit is DashSwitch) {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            base.Collider = breakWallsHitbox;
            foreach (TempleCrackedBlock entity in base.Scene.Tracker.GetEntities<TempleCrackedBlock>()) {
                if (CollideCheck(entity, Position + Vector2.UnitX * Math.Sign(Speed.X))) {
                    entity.Break(base.Center);
                }
            }
            base.Collider = physicsHitbox;
            SceneAs<Level>().DirectionalShake(Vector2.UnitX * Math.Sign(Speed.X));
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Speed.X = (float) Math.Sign(Speed.X) * (0 - StunXSpeed);
            Speed.Y *= 0.4f;
            sprite.Scale.X = 0.6f;
            sprite.Scale.Y = 1.4f;
            shaker.ShakeFor(0.5f, removeOnFinish: false);
            scaleWiggler.Start();
            State.State = 4;
            if (data.Hit is SeekerBarrier) {
                (data.Hit as SeekerBarrier).OnReflectSeeker();
                Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
            } else {
                Audio.Play("event:/game/05_mirror_temple/seeker_hit_normal", Position);
            }
            if (numberOfWallCollides == maxNumberOfWallCollides) {
                Add(new Coroutine(seekerDeath(0.15f)));
            }
        }

        private IEnumerator seekerDeath(float f) { yield return f; SeekerDeath(); }

        protected virtual void OnCollideH(CollisionData data) {
            if (State.State == 3 && data.Hit != null) {
                int num = Math.Sign(Speed.X);
                if ((!CollideCheck<Solid>(Position + new Vector2(num, 4f)) && !MoveVExact(4)) || (!CollideCheck<Solid>(Position + new Vector2(num, -4f)) && !MoveVExact(-4))) {
                    return;
                }
            }
            if ((State.State == 3 || State.State == 5) && Math.Abs(Speed.X) >= WallCollideStunThreshold) {
                numberOfWallCollides++;
                SlammedIntoWall(data);
            } else {
                Speed.X *= -0.2f;
            }
        }

        protected virtual void OnCollideV(CollisionData data) {
            if (State.State == 3) {
                Speed.Y *= -0.6f;
            } else {
                Speed.Y *= -0.2f;
            }
        }

        protected virtual void CreateTrail() {
            Vector2 scale = sprite.Scale;
            sprite.Scale *= 1f - 0.3f * scaleWiggler.Value;
            sprite.Scale.X *= spriteFacing;
            TrailManager.Add(this, TrailColor, 0.5f);
            sprite.Scale = scale;
        }

        protected virtual int IdleUpdate() {
            if (canSeePlayer) {
                return 2;
            }
            Vector2 vector = Vector2.Zero;
            if (spotted && Vector2.DistanceSquared(base.Center, FollowTarget) > 64f) {
                float speedMagnitude = GetSpeedMagnitude(IdleSpeed);
                vector = ((!lastPathFound) ? (FollowTarget - base.Center).SafeNormalize(speedMagnitude) : GetPathSpeed(speedMagnitude));
            }
            if (vector == Vector2.Zero) {
                vector.X = idleSineX.Value * 6f;
                vector.Y = idleSineY.Value * 6f;
            }
            Speed = Calc.Approach(Speed, vector, IdleAccel * Engine.DeltaTime);
            if (Speed.LengthSquared() > 400f) {
                TurnFacing(Speed.X);
            }
            if (spriteFacing == facing) {
                sprite.Play("idle");
            }
            return 0;
        }

        protected virtual IEnumerator IdleCoroutine() {
            if (patrolPoints != null && patrolPoints.Length != 0 && spotted) {
                while (Vector2.DistanceSquared(base.Center, FollowTarget) > 64f) {
                    yield return null;
                }
                yield return 0.3f;
                State.State = 1;
            }
        }

        protected virtual Vector2 GetPathSpeed(float magnitude) {
            if (pathIndex >= path.Count) {
                return Vector2.Zero;
            }
            if (Vector2.DistanceSquared(base.Center, path[pathIndex]) < 36f) {
                pathIndex++;
                return GetPathSpeed(magnitude);
            }
            return (path[pathIndex] - base.Center).SafeNormalize(magnitude);
        }

        protected virtual float GetSpeedMagnitude(float baseMagnitude) {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null) {
                if (Vector2.DistanceSquared(base.Center, entity.Center) > FarDistSq) {
                    return baseMagnitude * 1.5f * FarSpeedMultiplier;
                }
                return baseMagnitude * 1.5f;
            }
            return baseMagnitude;
        }

        protected virtual void PatrolBegin() {
            State.State = ChoosePatrolTarget();
            patrolWaitTimer = 0f;
        }

        protected virtual int PatrolUpdate() {
            if (canSeePlayer || AlwaysSeePlayer) {
                return 2;
            }
            if (patrolWaitTimer > 0f) {
                patrolWaitTimer -= Engine.DeltaTime;
                if (patrolWaitTimer <= 0f) {
                    return ChoosePatrolTarget();
                }
            } else if (Vector2.DistanceSquared(base.Center, lastSpottedAt) < 144f) {
                patrolWaitTimer = PatrolWaitTime;
            }
            float speedMagnitude = GetSpeedMagnitude(PatrolSpeed);
            Speed = Calc.Approach(target: (!lastPathFound) ? (FollowTarget - base.Center).SafeNormalize(speedMagnitude) : GetPathSpeed(speedMagnitude), val: Speed, maxMove: Accel * Engine.DeltaTime);
            if (Speed.LengthSquared() > 100f) {
                TurnFacing(Speed.X);
            }
            if (spriteFacing == facing) {
                sprite.Play("search");
            }
            return 1;
        }

        protected virtual int ChoosePatrolTarget() {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity == null) {
                return 0;
            }
            for (int i = 0; i < 3; i++) {
                patrolChoices[i].Distance = 0f;
            }
            int num = 0;
            Vector2[] array = patrolPoints;
            foreach (Vector2 vector in array) {
                if (Vector2.DistanceSquared(base.Center, vector) < 1.5 * AttackMinXDist
                    ) {
                    continue;
                }
                float num2 = Vector2.DistanceSquared(vector, entity.Center);
                for (int k = 0; k < 3; k++) {
                    if (num2 < patrolChoices[k].Distance || patrolChoices[k].Distance <= 0f) {
                        num++;
                        for (int num3 = 2; num3 > k; num3--) {
                            patrolChoices[num3].Distance = patrolChoices[num3 - 1].Distance;
                            patrolChoices[num3].Point = patrolChoices[num3 - 1].Point;
                        }
                        patrolChoices[k].Distance = num2;
                        patrolChoices[k].Point = vector;
                        break;
                    }
                }
            }
            if (num <= 0) {
                return 0;
            }
            lastSpottedAt = patrolChoices[random.Next(Math.Min(3, num))].Point;
            lastPathTo = lastSpottedAt;
            pathIndex = 0;
            lastPathFound = SceneAs<Level>().Pathfinder.Find(ref path, base.Center, FollowTarget);
            return 1;
        }

        protected virtual void SpottedBegin() {
            aggroSfx.Play(aggroSFX);
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null) {
                TurnFacing(entity.X - base.X, "spot");
            }
            spottedLosePlayerTimer = SpottedLosePlayerTime;
            spottedTurnDelay = 1f;
        }

        protected virtual int SpottedUpdate() {
            if (!canSeePlayer && !AlwaysSeePlayer) {
                spottedLosePlayerTimer -= Engine.DeltaTime;
                if (spottedLosePlayerTimer < 0f) {
                    return 0;
                }
            } else {
                spottedLosePlayerTimer = SpottedLosePlayerTime;
            }
            float speedMagnitude = GetSpeedMagnitude(SpottedTargetSpeed);
            Vector2 vector = (!lastPathFound) ? (FollowTarget - base.Center).SafeNormalize(speedMagnitude) : GetPathSpeed(speedMagnitude);
            if (Vector2.DistanceSquared(base.Center, FollowTarget) < 2500f && base.Y < FollowTarget.Y) {
                float num = vector.Angle();
                if (base.Y < FollowTarget.Y - 2f) {
                    num = Calc.AngleLerp(num, Consts.PIover2, 0.5f);
                } else if (base.Y > FollowTarget.Y + 2f) {
                    num = Calc.AngleLerp(num, -Consts.PIover2, 0.5f);
                }
                vector = Calc.AngleToVector(num, SpottedTargetSpeed);
                Vector2 value = Vector2.UnitX * Math.Sign(base.X - lastSpottedAt.X) * 48f;
                if (Math.Abs(base.X - lastSpottedAt.X) < XDistanceFromWindUp && !CollideCheck<Solid>(Position + value) && !CollideCheck<Solid>(lastSpottedAt + value)) {
                    vector.X = Math.Sign(base.X - lastSpottedAt.X) * 60;
                }
            }
            Speed = Calc.Approach(Speed, vector, Accel * Engine.DeltaTime);
            spottedTurnDelay -= Engine.DeltaTime;
            if (spottedTurnDelay <= 0f) {
                TurnFacing(Speed.X, "spotted");
            }
            return 2;
        }

        protected virtual IEnumerator SpottedCoroutine() {
            yield return SpottedMinAttackTime;
            while (!CanAttack()) {
                yield return null;
            }
            State.State = 3;
        }

        protected virtual bool CanAttack() {
            if (Math.Abs(base.Y - lastSpottedAt.Y) > SpottedMaxYDist) {
                return false;
            }
            if (Math.Abs(base.X - lastSpottedAt.X) < AttackMinXDist) {
                return false;
            }
            Vector2 value = (FollowTarget - base.Center).SafeNormalize();
            if (Vector2.Dot(-Vector2.UnitY, value) > 0.5f || Vector2.Dot(Vector2.UnitY, value) > 0.5f) {
                return false;
            }
            if (CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(lastSpottedAt.X - base.X) * SpottedMaxYDist)) {
                return false;
            }
            if (numberOfDashes + 1 > maxNumberOfDashes && maxNumberOfDashes != -1) {
                return false;
            }
            return true;
        }

        protected virtual void AttackBegin() {
            Audio.Play("event:/game/05_mirror_temple/seeker_dash", Position);
            attackWindUp = true;
            attackSpeed = AttackWindUpSpeed;
            Speed = (FollowTarget - base.Center).SafeNormalize(AttackWindUpSpeed);
            numberOfDashes++;
        }

        protected virtual int AttackUpdate() {
            if (!attackWindUp) {
                Vector2 vector = (FollowTarget - base.Center).SafeNormalize();
                if (Vector2.Dot(Speed.SafeNormalize(), vector) < DirectionDotThreshold && !(finalDash && numberOfDashes == maxNumberOfDashes)) {
                    return 5;
                }
                attackSpeed = Calc.Approach(attackSpeed, AttackTargetSpeed, AttackAccel * Engine.DeltaTime);
                Speed = Speed.RotateTowards(vector.Angle(), AttackMaxRotateRadians * Engine.DeltaTime).SafeNormalize(attackSpeed);
                if (ParticleEmitInterval > 0 && !disableAllParticles) {
                    if (base.Scene.OnInterval(ParticleEmitInterval)) {
                        if (SceneAs<Level>().InsideCamera(Position, 10f)) {
                            Vector2 vector2 = (-Speed).SafeNormalize();
                            SceneAs<Level>().Particles.Emit(Seeker.P_Attack, 2, Position + vector2 * 4f, Vector2.One * 4f, vector2.Angle());
                        }
                    }
                }
                if (TrailCreateInterval > 0) {
                    if (base.Scene.OnInterval(TrailCreateInterval)) {
                        if (SceneAs<Level>().InsideCamera(Position, 10f)) {
                            CreateTrail();
                        }
                    }
                }

            }
            return 3;
        }

        protected virtual IEnumerator AttackCoroutine() {
            TurnFacing(lastSpottedAt.X - base.X, "windUp");
            yield return AttackWindUpTime;
            attackWindUp = false;
            attackSpeed = AttackStartSpeed;
            Speed = (lastSpottedAt - Vector2.UnitY * 2f - base.Center).SafeNormalize(180f);
            SnapFacing(Speed.X);
        }

        protected virtual int StunnedUpdate() {
            Speed = Calc.Approach(Speed, Vector2.Zero, 150f * Engine.DeltaTime);
            return 4;
        }

        protected virtual IEnumerator StunnedCoroutine() {
            if (finalDash && numberOfDashes == maxNumberOfDashes) {
                Add(new Coroutine(seekerDeath(0.15f)));
            }
            yield return 0.8f;
            State.State = 0;
        }

        protected virtual void SkiddingBegin() {
            Audio.Play("event:/game/05_mirror_temple/seeker_dash_turn", Position);
            strongSkid = false;
            TurnFacing(-facing);

        }

        protected virtual int SkiddingUpdate() {
            Speed = Calc.Approach(Speed, Vector2.Zero, (strongSkid ? StrongSkiddingAccel : SkiddingAccel) * Engine.DeltaTime);
            if (Speed.LengthSquared() < 400f) {
                if (canSeePlayer) {
                    return 2;
                }
                return 0;
            }
            return 5;
        }

        protected virtual IEnumerator SkiddingCoroutine() {
            yield return StrongSkiddingTime;
            strongSkid = true;
        }

        protected virtual void SkiddingEnd() {
            spriteFacing = facing;
        }

        protected virtual void RegenerateBegin() {
            Audio.Play("event:/game/general/thing_booped", Position);
            boopedSfx.Play(boopedSFX);
            sprite.Play("takeHit");
            Collidable = false;
            State.Locked = true;
            Light.StartRadius = 16f;
            Light.EndRadius = 32f;
            numberOfBounces++;
        }

        protected virtual void RegenerateEnd() {
            reviveSfx.Play(reviveSFX);
            Collidable = true;
            Light.StartRadius = 32f;
            Light.EndRadius = 64f;
        }

        protected virtual int RegenerateUpdate() {
            Speed.X = Calc.Approach(Speed.X, 0f, 150f * Engine.DeltaTime);
            Speed = Calc.Approach(Speed, Vector2.Zero, 150f * Engine.DeltaTime);
            return 6;
        }

        protected virtual IEnumerator RegenerateCoroutine() {
            yield return 1f * RegenerateTimerMult;
            shaker.On = true;
            if (numberOfBounces == maxNumberOfBounces) {
                yield return seekerDeath(0.15f * RegenerateTimerMult);
                yield break;
            }
            yield return 0.2f * RegenerateTimerMult;
            sprite.Play("pulse");
            yield return 0.5f * RegenerateTimerMult;
            sprite.Play("recover");
            RecoverBlast.Spawn(Position, CustomShockwavePath);
            yield return 0.15f * RegenerateTimerMult;
            base.Collider = pushRadius;
            Player player = CollideFirst<Player>();
            if (player != null && !base.Scene.CollideCheck<Solid>(Position, player.Center)) {
                player.ExplodeLaunch(Position, true);
            }
            TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
            if (theoCrystal != null && !base.Scene.CollideCheck<Solid>(Position, theoCrystal.Center)) {
                theoCrystal.ExplodeLaunch(Position);
            }
            foreach (TempleCrackedBlock entity in base.Scene.Tracker.GetEntities<TempleCrackedBlock>()) {
                if (CollideCheck(entity)) {
                    entity.Break(Position);
                }
            }
            foreach (TouchSwitch entity2 in base.Scene.Tracker.GetEntities<TouchSwitch>()) {
                if (CollideCheck(entity2)) {
                    entity2.TurnOn();
                }
            }
            base.Collider = physicsHitbox;
            Level level = SceneAs<Level>();
            level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
            level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
            level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
            if (!disableAllParticles) {
                for (float num = 0f; num < Consts.TAU; num += 0.17453292f) {
                    Vector2 position = base.Center + Calc.AngleToVector(num + Calc.Random.Range(Consts.DEG1 * -4, Consts.DEG1 * 4), Calc.Random.Range(12, 18));
                    level.Particles.Emit(Seeker.P_Regen, position, num);
                }
            }
            shaker.On = false;
            State.Locked = false;
            State.State = 7;
        }

        protected virtual IEnumerator ReturnedCoroutine() {
            yield return 0.3f;
            State.State = 0;
        }

        public override void Removed(Scene scene) {
            CustomSeekersList.Remove(this);
            base.Removed(scene);

        }

        public static List<Entity> GetList() {
            return (Engine.Scene as Level).Tracker.GetEntities<CustomSeeker>();
        }

        //In your subclass of Custom Seeker write an override to set base.GetYAMLText() to a List<string> and append/combine it as needed.
        public virtual List<string> GetYAMLText(bool Default) {
            List<string> text = new List<string>();
            if (Default) {
                //default
                text = new List<string>()
                {
                    "Accel: 600", "AttackAccel: 300", "AttackMinXDist: 16",
                    "AttackStartSpeed: 180", "AttackTargetSpeed: 260",
                    "AttackWindUpSpeed: 60", "AttackWindUpTime: 0.3", "AttackMaxRotateRadians: 35", "",
                    "aggroSFXPath: \"\"", "boopedSFXPath: \"\"", "reviveSFXPath: \"\"",
                    "CustomSpritePath: \"\"", "CustomShockwavePath: \"\"", "",
                    "BounceSpeed: 200", "DirectionDotThreshold: 0.4",
                    "ExplodeRadius: 40", "SightDistance: 160", "",
                    "FarDist: 112", "FarDistSpeedMult: 2","",
                    "IdleSpeed: 50", "IdleAccel: 200","",
                    "MaxNumberOfBounces: -1", "MaxNumberOfDashes: -1", "MaxNumberOfWallCollides: -1","",
                    "PatrolSpeed: 25", "PatrolWaitTime: 0.4","",
                    "RegenerationTimerLength: 1.85","",
                    "StrongSkiddingTime: 0.08", "SkiddingAccel: 200", "StrongSkiddingAccel: 400","",
                    "SpottedLosePlayerTime: 0.6", "SpottedMaxYDist: 24",
                    "SpottedMinAttackTime: 0.2", "SpottedTargetSpeed: 60","",
                    "StunTime: 0.8", "StunXSpeed: 100", "", "WallCollideStunThreshold: 100",
                    "DeathEffectColor: \"FFFFFF\"", "SeekerColorTint: \"FFFFFF\"", "", "TrailColor: \"FFFFFF\"", "",
                    "DisableAllParticles: False", "FinalDash: True", "RemoveBounceHitbox: False", "",
                    "ParticleEmitInterval: 0.04", "TrailCreateInterval: 0.06", "StartingState: 0", "FlagOnDeath", "\n"
                };
            } else {
                text = new List<string>()
                {
                    "Accel: " + Accel, "AttackAccel: " + AttackAccel, "AttackMinXDist: " + AttackMinXDist,
                    "AttackStartSpeed: " + AttackStartSpeed, "AttackTargetSpeed: " + AttackTargetSpeed,
                    "AttackWindUpSpeed: " + (0 - AttackWindUpSpeed), "AttackWindUpTime: " + AttackWindUpTime,
                    "AttackMaxRotateDegrees: " + (float)Math.Round(AttackMaxRotateRadians * Calc.RadToDeg), "",
                    "aggroSFXPath: \"" + aggroSFX + "\"", "boopedSFXPath: \"" + boopedSFX + "\"", "reviveSFXPath: \"" + reviveSFX + "\"",
                    "CustomSpritePath: \"" + CustomSpritePath + "\"", "CustomShockwavePath: \"" + CustomShockwavePath + "\"", "",
                    "BounceSpeed: " + BounceSpeed, "DirectionDotThreshold: " + DirectionDotThreshold,
                    "ExplodeRadius: " + ExplodeRadius, "SightDistance: " + Math.Sqrt(SightDistSq).ToString(), "",
                    "FarDist: " + Math.Sqrt(FarDistSq).ToString(), "FarDistSpeedMult: " + FarSpeedMultiplier,"",
                    "IdleSpeed: " + IdleSpeed, "IdleAccel: " + IdleAccel,"",
                    "MaxNumberOfBounces: " + maxNumberOfBounces, "MaxNumberOfDashes: " + maxNumberOfDashes, "MaxNumberOfWallCollides: " + maxNumberOfWallCollides,"",
                    "PatrolSpeed: " + PatrolSpeed, "PatrolWaitTime: " + PatrolWaitTime,"",
                    "RegenerationTimerLength: " + (RegenerateTimerMult * 1.85f).ToString(),"",
                    "StrongSkiddingTime: " + StrongSkiddingTime, "SkiddingAccel: " + SkiddingAccel, "StrongSkiddingAccel: " + StrongSkiddingAccel,"",
                    "SpottedLosePlayerTime: " + SpottedLosePlayerTime, "SpottedMaxYDist: " + SpottedMaxYDist,
                    "SpottedMinAttackTime: " + SpottedMinAttackTime, "SpottedTargetSpeed: " + SpottedTargetSpeed,"",
                    "StunTime: " + StunTime, "StunXSpeed: " + StunXSpeed, "", "WallCollideStunThreshold: " + WallCollideStunThreshold,
                    "DeathEffectColor: \"" + VivHelper.ColorToHex(DeathEffectColor) + "\"", "SeekerColorTint: \"" + VivHelper.ColorToHex(tint) + "\"", "", "TrailColor: \"" + VivHelper.ColorToHex(TrailColor) + "\"", "",
                    "DisableAllParticles: " + disableAllParticles, "FinalDash: " + finalDash, "RemoveBounceHitbox: " + RemoveBounceHitbox, "",
                    "ParticleEmitInterval: " + ParticleEmitInterval, "TrailCreateInterval: " + TrailCreateInterval, "StartingState: " + StartingState, "\n"
                };
            }
            return text;
        }

        public static CustomSeekerYaml LoadYaml(string path) {
            String fullPath = "Seekers/" + path;
            if (!Everest.Content.TryGet(fullPath, out ModAsset asset)) {
                Logger.Log("VivHelper", "Failed loading Seeker data file \"" + path + "\": The file could not be found.");
                Engine.Commands.Log("VivHelper: The game tried to load the file at \"" + path + "\" but couldn't find it. You may have input the name of the file wrong.");
                return null;
            } else {
                try {
                    using (StreamReader reader = new StreamReader(asset.Stream)) {
                        return YamlHelper.Deserializer.Deserialize<CustomSeekerYaml>(reader);
                    }
                } catch (Exception e) {
                    Logger.Log("VivHelper", "Failed loading Seeker file \"" + path + $"\": {e.Message}");
                    return null;
                }
            }
        }
    }

    public class CustomSeekerYaml {
        public int Order;

        public float Accel, AttackAccel, AttackMinXDist, XDistanceFromWindUp, AttackStartSpeed, AttackTargetSpeed, AttackWindUpSpeed, AttackWindUpTime, AttackMaxRotateDegrees;
        public string aggroSFXPath, boopedSFXPath, reviveSFXPath, CustomSpritePath, CustomShockwavePath;
        public float BounceSpeed, DirectionDotThreshold, ExplodeRadius, SightDistance;
        public float FarDist, FarDistSpeedMult;
        public float IdleSpeed, IdleAccel;
        public int MaxNumberOfBounces, MaxNumberOfDashes, MaxNumberOfWallCollides;
        public float PatrolSpeed, PatrolWaitTime;
        public float RegenerationTimerLength;
        public float StrongSkiddingTime, SkiddingAccel, StrongSkiddingAccel;
        public float SpottedLosePlayerTime, SpottedMaxYDist, SpottedMinAttackTime, SpottedTargetSpeed;
        public float StunTime, StunXSpeed, WallCollideStunThreshold;
        public string DeathEffectColor, SeekerColorTint, TrailColor, FlagOnDeath;
        public bool DisableAllParticles, FinalDash, RemoveBounceHitbox;
        public float ParticleEmitInterval, TrailCreateInterval;

        public EntityData SeekerDataFromYaml(Vector2 position) {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (var prop in this.GetType().GetProperties()) {
                if (prop.Name != "Order")
                    dict[prop.Name] = prop.GetValue(this);
            }

            EntityData data = new EntityData {
                Position = position,
                Values = dict
            };
            return data;
        }
    }

}

