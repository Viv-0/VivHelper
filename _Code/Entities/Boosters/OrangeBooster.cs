using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using VivHelper;
using Celeste.Mod.VivHelper;
using System.Collections;
using System.Reflection;
using MonoMod.Utils;
using Celeste.Mod;

namespace VivHelper.Entities.Boosters {
    public class OrangeBoost {
        private static float timer;


        public static void Begin(Player player) {
            player.RefillDash();
            player.RefillStamina();
            timer = 0.25f;
        }

        public static IEnumerator Coroutine(Player player) {
            DynamicData dyn = DynamicData.For(player);
            yield return DashFix();
            player.Speed = CustomBooster.CorrectDashPrecision(dyn?.Get<Vector2>("lastAim") ?? Vector2.Zero) * 220f;
            //We assume that at this point the code has successfully obtained DynData on the Player object.
            while (true) {
                Vector2 v = player.Speed;
                player.DashDir = v;
                dyn.Set("gliderBoostDir", v);
                (player.Scene as Level).DirectionalShake(player.DashDir, 0.2f);
                if (player.DashDir.X != 0f) {
                    player.Facing = (Facings) Math.Sign(player.DashDir.X);
                }
                yield return null;
                float a = player.Speed.Angle();
                if (player.CanDash) {
                    Vector2 value = Input.Aim.Value;
                    if (value != Vector2.Zero) {
                        a = Calc.EightWayNormal(CustomBooster.CorrectDashPrecision(value)).Angle();
                        player.Speed = new Vector2(VivHelperModule.Session.OrangeSpeed * (float) Math.Cos(a), VivHelperModule.Session.OrangeSpeed * (float) Math.Sin(a));
                    }
                }

            }
        }

        public static int Update(Player player) {
            player.LastBooster = null;
            DynamicData dyn = DynamicData.For(player);
            Vector2 v = dyn.Get<Vector2>("boostTarget");
            while (timer > 0) {
                //timer will be lowered via DashFix() run in OrangeCoroutine, DashFix ends when timer = 0, this is to fix the fastbubbling bug.
                player.Center = v;
                return VivHelperModule.OrangeState;
            }

            if (Engine.Scene.OnInterval(0.02f)) {
                (Engine.Scene as Level).ParticlesBG.Emit(OrangeBooster.P_Burst, 2, player.Center + new Vector2(0f, -2f), new Vector2(3f, 3f), Consts.PIover2);
            }


            int j = (int) BoostFunctions.rdU.Invoke(player, Everest._EmptyObjectArray);
            j = j == 5 ? VivHelperModule.OrangeState : j;

            return j;

        }

        public static void End(Player player) {
            player.Sprite.Visible = true;
            player.Position.Y = (float) Math.Round(player.Position.Y);
            VivHelperModule.Session.OrangeSpeed = 220f;
        }



        private static IEnumerator DashFix() {
            Input.Dash.ConsumePress();
            Input.CrouchDash.ConsumePress();
            timer = CustomBooster.timerStart;
            while (timer >= Engine.DeltaTime) {
                timer -= Engine.DeltaTime;
                yield return null;
                if (Input.Dash.Pressed) { timer = 0; Input.Dash.ConsumePress(); break; }
                if (Input.CrouchDashPressed) { timer = 0; Input.CrouchDash.ConsumePress(); break; }
            }
            timer = 0;

        }
    }

    [TrackedAs(typeof(CustomBooster))]
    [CustomEntity("VivHelper/OrangeBooster")]
    public class OrangeBooster : CustomBooster {
        private const float RespawnTime = 1f;

        public static ParticleType P_Burst = new ParticleType {
            Source = GFX.Game["particles/blob"],
            Color = Calc.HexToColor("f07848"),
            FadeMode = ParticleType.FadeModes.None,
            LifeMin = 0.5f,
            LifeMax = 0.8f,
            Size = 0.7f,
            SizeRange = 0.25f,
            ScaleOut = true,
            Direction = 4.712389f,
            DirectionRange = 0.17453292f,
            SpeedMin = 10f,
            SpeedMax = 20f,
            SpeedMultiplier = 0.01f,
            Acceleration = new Vector2(0f, 90f)
        };

        public static ParticleType P_Appear = new ParticleType {
            Size = 1f,
            Color = Color.Orange,
            DirectionRange = Consts.PI / 30f,
            LifeMin = 0.6f,
            LifeMax = 1f,
            SpeedMin = 40f,
            SpeedMax = 50f,
            SpeedMultiplier = 0.25f,
            FadeMode = ParticleType.FadeModes.Late
        };


        public static readonly Vector2 playerOffset = new Vector2(0f, -2f);

        private Sprite sprite;

        private Entity outline;

        private Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private DashListener dashListener;

        private ParticleType particleType;

        private float respawnTimer;

        private float cannotUseTimer;

        private bool red;

        private Vector2 direction = Vector2.Zero;

        private SoundSource loopingSfx;

        public bool Ch9HubBooster;

        public bool Ch9HubTransition;

        private float speed;

        public OrangeBooster(Vector2 position)
            : base(position) {
            base.Depth = -8500;
            base.Collider = new Circle(10f, 0f, 2f);
            this.red = true;
            Add(sprite = VivHelperModule.spriteBank.Create("VivHelperOrangeBooster"));

            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(Calc.HexToColor("ffd27f"), 1f, 16, 32));
            Add(bloom = new BloomPoint(0.1f, 16f));
            Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f) {
                sprite.Scale = Vector2.One * (1f + f * 0.25f);
            }));
            Add(dashRoutine = new Coroutine(removeOnComplete: false));
            Add(dashListener = new DashListener());
            Add(new MirrorReflection());
            Add(loopingSfx = new SoundSource());
            dashListener.OnDash = OnPlayerDashed;

        }

        public OrangeBooster(EntityData data, Vector2 offset)
            : this(data.Position + offset) {
            speed = data.Float("speed", 220f);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Image image = new Image(GFX.Game["objects/booster/outline"]);
            image.CenterOrigin();
            image.Color = Color.Lerp(Color.White, Calc.HexToColor("ffd27f"), 0.2f) * 0.75f;
            outline = new Entity(Position);
            outline.Depth = 8999;
            outline.Visible = false;
            outline.Add(image);
            outline.Add(new MirrorReflection());
            scene.Add(outline);

        }

        public void Appear() {
            Audio.Play(SFX.game_05_redbooster_reappear, Position);
            sprite.Play("appear");
            wiggler.Start();
            Visible = true;
            AppearParticles();
        }

        private void AppearParticles() {
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
            for (int i = 0; i < 360; i += 30) {
                particlesBG.Emit(P_Appear, 1, base.Center, Vector2.One * 2f, i * Consts.DEG1);
            }
        }

        private void OnPlayer(Player player) {
            if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer) {
                cannotUseTimer = 0.45f;
                Boost(player, this);
                Audio.Play(SFX.game_05_redbooster_enter, Position);
                wiggler.Start();
                sprite.Play("inside");
                sprite.FlipX = (player.Facing == Facings.Left);
            }
        }

        public static void Boost(Player player, OrangeBooster booster) {
            VivHelper.player_boostTarget.SetValue(player, booster.Center);
            VivHelperModule.Session.OrangeSpeed = booster.speed;
            player.StateMachine.State = VivHelperModule.OrangeState;
            player.Position = booster.Center;
            player.Speed = Vector2.Zero;

            booster.BoostingPlayer = true;
            booster.PlayerBoosted(player);
        }

        public override void PlayerBoosted(Player player) {
            player.Center = Center;
            Audio.Play(SFX.game_05_redbooster_dash, Position);
            loopingSfx.Play(SFX.game_05_redbooster_move_loop);
            loopingSfx.DisposeOnTransition = false;
            base.PlayerBoosted(player);
            sprite.Play("spin");
            sprite.FlipX = (player.Facing == Facings.Left);
            outline.Visible = true;
            wiggler.Start();
            dashRoutine.Replace(BoostRoutine(player, player.DashDir));
        }

        private IEnumerator BoostRoutine(Player player, Vector2 dir) {
            direction = dir;
            float angle = (-dir).Angle();
            while (BoostingPlayer && player.StateMachine.State == VivHelperModule.OrangeState && !player.Dead) {
                sprite.RenderPosition = player.Center + playerOffset;
                loopingSfx.Position = sprite.Position;
                yield return null;

            }
            PlayerReleased();
            if (new int[] { VivHelperModule.OrangeState, VivHelperModule.PinkState, VivHelperModule.WindBoostState, 4 }.Contains(player.StateMachine.State))
                sprite.Visible = false;
            while (SceneAs<Level>().Transitioning) {
                yield return null;
            }
            Tag = 0;
        }

        public void OnPlayerDashed(Vector2 direction) {
            if (BoostingPlayer && (VivHelper.TryGetPlayer(out Player player) ? player.StateMachine.State != VivHelperModule.OrangeState : true)) {
                BoostingPlayer = false;
            }
        }

        public override void PlayerReleased() {
            Audio.Play(SFX.game_05_redbooster_end, sprite.RenderPosition);
            sprite.Play("pop");
            cannotUseTimer = 0f;
            respawnTimer = 1f;
            base.PlayerReleased();
            wiggler.Stop();
            loopingSfx.Stop();
        }

        public void Respawn() {
            Audio.Play(SFX.game_05_redbooster_reappear, Position);
            sprite.Position = Vector2.Zero;
            sprite.Play("loop", restart: true);
            wiggler.Start();
            sprite.Visible = true;
            outline.Visible = false;
            AppearParticles();
        }

        public override void Update() {
            base.Update();
            if (cannotUseTimer > 0f) {
                cannotUseTimer -= Engine.DeltaTime;
            }
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f) {
                    Respawn();
                }
            }
            if (!dashRoutine.Active && respawnTimer <= 0f) {
                Vector2 target = Vector2.Zero;
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null && CollideCheck(entity)) {
                    target = entity.Center + playerOffset - Position;
                }
                sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
            }
            if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>()) {
                sprite.Play("loop");
            }
        }

        public override void Render() {
            Vector2 position = sprite.Position;
            sprite.Position = position.Floor();
            if (sprite.CurrentAnimationID != "pop" && sprite.Visible) {
                sprite.DrawOutline();
            }
            base.Render();
            sprite.Position = position;
        }



        public override void Removed(Scene scene) {
            if (Ch9HubTransition) {
                Level level = scene as Level;
                foreach (Backdrop item in level.Background.GetEach<Backdrop>("bright")) {
                    item.ForceVisible = false;
                    item.FadeAlphaMultiplier = 1f;
                }
                level.Bloom.Base = AreaData.Get(level).BloomBase + 0.25f;
                level.Session.BloomBaseAdd = 0.25f;
            }
            base.Removed(scene);
        }
    }
}
