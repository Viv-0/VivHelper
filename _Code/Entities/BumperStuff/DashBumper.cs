using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/DashBumper")]
    public class DashBumper : Entity {
        public static ParticleType P_Ambience = new ParticleType {
            Source = GFX.Game["particles/rect"],
            Color = Calc.HexToColor("c247cc"),
            Color2 = Calc.HexToColor("f789ff"),
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.InAndOut,
            Size = 0.5f,
            SizeRange = 0.2f,
            RotationMode = ParticleType.RotationModes.SameAsDirection,
            LifeMin = 0.2f,
            LifeMax = 0.4f,
            SpeedMin = 10f,
            SpeedMax = 20f,
            DirectionRange = (float) Math.PI / 6f
        };

        public static ParticleType P_Hit = new ParticleType {

            Color = Calc.HexToColor("c247cc"),
            Color2 = Calc.HexToColor("f789ff"),
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.5f,
            SizeRange = 0.2f,
            RotationMode = ParticleType.RotationModes.Random,
            LifeMin = 0.6f,
            LifeMax = 1.2f,
            SpeedMin = 40f,
            SpeedMax = 140f,
            SpeedMultiplier = 0.1f,
            Acceleration = new Vector2(0f, 10f),
            DirectionRange = 0.6981317f
        };

        public static ParticleType P_Launch = new ParticleType(P_Hit) { Source = GFX.Game["particles/rect"] };

        private float RespawnTime = 0.6f;

        private float MoveCycleTime = 1.81818187f;

        private const float SineCycleFreq = 0.44f;

        private Sprite sprite;

        private VertexLight light;

        private BloomPoint bloom;

        private Vector2? node;
        private Vector2 spriteOffset;

        private bool goBack;

        private Vector2 anchor;

        private SineWave sine;
        private bool wobble = false;

        private float respawnTimer;

        private Wiggler hitWiggler;

        private Vector2 hitDir;

        private SoundSource sfx;
        private enum ReflectTypes { DashDir = 0, Angle4, AltAngle4, Angle8, DashDir4Way }
        private ReflectTypes reflectType;
        private const float strength = 280f;
        private const float CoyoteTime = 0.1f;
        private float coyote = 0;

        public DashBumper(EntityData data, Vector2 offset)
            : base(data.Position + offset) {

            Collider = new Circle(12f);
            Add(new PlayerCollider(OnPlayer));
            if (data.Bool("Wobble", true)) { wobble = true; Add(sine = new SineWave(0.44f, 0f).Randomize()); }
            reflectType = data.Enum<ReflectTypes>("ReflectType", ReflectTypes.DashDir);
            string directory = "VivHelper/dashBumper"; //removing customization option for now
            sprite = new Sprite(GFX.Game, directory);

            sprite.AddLoop("idle", 0.06f, GFX.Game.GetAtlasSubtextures(directory + "/idle").ToArray());
            sprite.AddLoop("off", 0.06f, GFX.Game.GetAtlasSubtextures(directory + "/off").ToArray());
            sprite.Add("on", 0.06f, "idle", GFX.Game.GetAtlasSubtextures(directory + "/on").ToArray());
            sprite.Add("hit", 0.06f, "off", GFX.Game.GetAtlasSubtextures(directory + "/hit").ToArray());
            sprite.Scale = Vector2.One;
            sprite.Position = spriteOffset = Vector2.One * (-32f);

            Add(sprite);
            sprite.Play("idle");
            Add(light = new VertexLight(Color.Purple, 1f, 16, 32));
            Add(bloom = new BloomPoint(0.5f, 16f));
            node = data.FirstNodeNullable(offset);
            anchor = Position;
            if (node.HasValue) {
                Vector2 start = Position;
                Vector2 end = node.Value;
                Tween tween = Tween.Create(Tween.TweenMode.Looping, VivHelper.EaseHelper[data.Attr("EaseType", "CubeInOut")], MoveCycleTime, start: true);
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
                sprite.Position = hitDir * hitWiggler.Value * 8f + spriteOffset;
            }));
            RespawnTime = Math.Max(0f, data.Float("RespawnTime", 0.6f));
            MoveCycleTime = Math.Max(0f, data.Float("MoveTime", 1.81818f));
            Add(sfx = new SoundSource());
        }

        public override void Added(Scene scene) {
            base.Added(scene);
        }

        private void UpdatePosition() {
            if (wobble)
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
                    Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
                }
            } else if (base.Scene.OnInterval(0.03f)) {
                float num = Calc.Random.NextAngle();
                float direction = num;
                SceneAs<Level>().Particles.Emit(P_Ambience, 1, base.Center + Calc.AngleToVector(num, 10), Vector2.One * 2f, direction);
            }
            if (sine != null)
                UpdatePosition();
            // Dash Coyote Time
            if (VivHelper.TryGetAlivePlayer(out Player p) && VivHelperModule.MatchDashState(p.StateMachine.State)) {
                coyote = CoyoteTime;
            } else if (coyote > 0) {
                coyote -= Engine.DeltaTime;
            }
        }


        private void OnPlayer(Player player) {

            if (respawnTimer <= 0f) {
                if (coyote <= 0) {
                    if (!SaveData.Instance.Assists.Invincible) {
                        Vector2 vector = (player.Center - base.Center).SafeNormalize();
                        hitDir = -vector;
                        hitWiggler.Start();
                        sfx.Play("event:/game/09_core/hotpinball_activate");
                        respawnTimer = RespawnTime;
                        player.Die(vector);
                        SceneAs<Level>().Particles.Emit(P_Hit, 12, base.Center + vector * 12f, Vector2.One * 3f, vector.Angle());
                    }
                } else {
                    sfx.Play("event:/game/09_core/pinballbumper_hit");
                    respawnTimer = RespawnTime;
                    Vector2 vector2 = Reflect(player);
                    sprite.Play("hit", restart: true);
                    light.Visible = false;
                    bloom.Visible = false;
                    SceneAs<Level>().DirectionalShake(vector2, 0.15f);
                    SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f);
                    SceneAs<Level>().Particles.Emit(P_Launch, 12, base.Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
                }
            }
        }

        private Vector2 Reflect(Player player) {
            Vector2 ret;
            switch (reflectType) {
                case ReflectTypes.Angle4:
                    ret = (player.Center - Center).FourWayNormal();
                    break;
                case ReflectTypes.AltAngle4:
                    ret = Alt4WayNormal(player.Center - Center);
                    break;
                case ReflectTypes.Angle8:
                    ret = (player.Center - Center).EightWayNormal();
                    break;
                case ReflectTypes.DashDir4Way:
                    ret = DashDir4WayNormal(player.DashDir, Center - player.Center) * -1f;
                    break;
                default:
                    ret = player.DashDir.SafeNormalize(Vector2.UnitY) * -1;
                    break;
            }
            new DynData<Player>(player).Set<float>("dashCooldownTimer", 0.2f);
            player.StateMachine.State = 7;
            player.Speed = ret * strength;
            if (!player.Inventory.NoRefills) {
                player.RefillDash();
            }
            player.RefillStamina();
            return ret;
        }

        private Vector2 Alt4WayNormal(Vector2 q) {
            float r = q.Angle();
            if (r >= 1.0472f && r <= 2.0944f) //down angle: if 60 < r < 120 degrees, r => 90 degrees
                r = 1.5708f; //r = 90 deg
            else if (-0.6545f <= r && r < 1.0472f) //right angle: if -37.5 <= r < 60 degrees, r => -25 degrees
                r = -0.436332f;
            else if ((r > 2.0944f && r <= 3.141593f) || (r > -3.1416f && r < -2.4871f)) //left angle: if 120 < r < 217.5, r => 205 degrees
                r = -2.70526f;
            else {
                r = -1.5708f;
            }

            return Calc.AngleToVector(r, 1f);
        }


        private Vector2 DashDir4WayNormal(Vector2 D, Vector2 F) // d is safely normalized
        {
            /* We cut out the diagonals. If D isn't a diagonal, then it uses FourWayNormal
			 * If D is a diagonal, we modify D to rotate it based off of the angle difference found in F (player Center - bumper Center, see Angle4)
			 */
            float e = D.Angle(); //Local variables are efficient.
            Vector2 ret = D; //This should always work.
            if (e == 0.785398f || e == 2.35619f || e == 3.92699f || e == 5.49779f) // 45, 135, 225, 315 deg, the diagonals
            {
                float g = F.Angle();
                //This code nudges the Vector2 D in the way that we want.
                if (g == e)
                    ret = D.RotateTowards(e > 3.14159f ? 4.7124f : 1.5708f, 0.02f); //this defaults us to the vertical axes
                else
                    ret = D.RotateTowards(g, 0.02f); //a little more than 1 degree
            }
            return ret.FourWayNormal();
        }

        private void OnShake(Vector2 pos) {
            foreach (Component component in base.Components) {
                if (component is Image) {
                    (component as Image).Position = pos;
                }
            }
        }

        private bool IsRiding(Solid solid) {
            Collider temp = Collider;
            if (node != null)
                Collider = new Hitbox(8, 8, node?.X ?? 0, node?.Y ?? 0);
            bool ret = CollideCheck(solid);
            Collider = temp;
            return ret;
        }
    }
}
