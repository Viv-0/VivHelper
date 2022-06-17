using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Celeste.Mod.VivHelper;
using Celeste.Mod;
using MonoMod.RuntimeDetour;

namespace VivHelper.Entities {
    public class RefillCancel {
        public class PlayerIndicator : Entity {
            private Level level;
            private Player player;
            private MTexture dashX = GFX.Game["VivHelper/PlayerIndicator/chevron"],
                          dashRefX = GFX.Game["VivHelper/PlayerIndicator/triangle"],
                          stamRefX = GFX.Game["VivHelper/PlayerIndicator/square"];

            public PlayerIndicator() : base() { }

            public override void Added(Scene scene) {
                base.Added(scene);
                level = SceneAs<Level>();
                player = level.Tracker.GetEntity<Player>();
                base.Depth = -20000;
            }

            public override void Render() {
                base.Render();
                if (inSpace && VivHelperModule.Session.ShowIndicator) {
                    if (DashRestrict) { dashX.DrawCentered(player.BottomCenter + new Vector2(-11f, 6f), VivHelperModule.Settings.ColorblindRCS ? Color.White : Color.Red, 1f); }
                    if (DashRefillRestrict) { dashRefX.DrawCentered(player.BottomCenter + new Vector2(0f, 6f), VivHelperModule.Settings.ColorblindRCS ? Color.White : Color.Blue, 1f); }
                    if (StaminaRefillRestrict) { stamRefX.DrawCentered(player.BottomCenter + new Vector2(12f, 6f), VivHelperModule.Settings.ColorblindRCS ? Color.White : Color.Yellow, 1f); }
                }
            }
        }

        public static bool inSpace, DashRefillRestrict, DashRestrict, StaminaRefillRestrict;
        public static PlayerIndicator p;
        public static void Load() {
            using (new DetourContext() { After = { "*" } }) {
                On.Celeste.Player.UseRefill += Player_UseRefill;

                On.Celeste.Player.Update += Player_Update;
                On.Celeste.Player.Die += Player_Die;
                Everest.Events.Level.OnExit += Level_OnExit;
            }
        }

        private static void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            DashRefillRestrict = DashRestrict = StaminaRefillRestrict = inSpace = false;
        }

        private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {

            PlayerDeadBody s = orig(self, direction, evenIfInvincible, registerDeathInStats);
            if (s != null) //bruh
            {
                VivHelperModule.Session.staminaCount = self.Stamina = Math.Max(self.Stamina, VivHelperModule.Session.staminaCount);
                if (p != null && p.Scene != null)
                    p.RemoveSelf();
            }

            return s;

        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self) {
            List<Entity> lE = new List<Entity>();
            if (!self.Scene.Tracker.Entities.ContainsKey(typeof(RefillCancelSpace))) { orig.Invoke(self); return; }
            lE = self.CollideAll<RefillCancelSpace>();
            if (inSpace) {
                if (p == null || p.Scene == null)
                    self.Scene.Add(p = new PlayerIndicator());
                if (DashRefillRestrict) {
                    if (self.Dashes > VivHelperModule.Session.dashCount) {
                        self.Dashes = VivHelperModule.Session.dashCount;
                    } else if (self.Dashes < VivHelperModule.Session.dashCount) {
                        VivHelperModule.Session.dashCount = self.Dashes;
                    }
                }
            }

            if (lE.Count <= 0 && inSpace) {

                inSpace = false;
                if (p != null) {
                    self.Scene.Remove(p);
                    p = null;
                }
                self.SceneAs<Level>().Session.Inventory.NoRefills = false;
            } else if (lE.Count > 0 && !inSpace) {
                inSpace = true;
                VivHelperModule.Session.dashCount = self.Dashes;
                VivHelperModule.Session.staminaCount = self.Stamina;
            }
            orig.Invoke(self);
            if (inSpace) {
                DashRestrict = DashRefillRestrict = StaminaRefillRestrict = false;
                foreach (RefillCancelSpace rcs in lE) {
                    DashRestrict |= rcs.DashRestrict;
                    DashRefillRestrict |= rcs.DashRefillRestrict;
                    StaminaRefillRestrict |= rcs.StaminaRefillRestrict;
                    if (DashRefillRestrict && StaminaRefillRestrict && DashRestrict)
                        break;
                }
                if (DashRestrict) { new DynData<Player>(self).Set<float>("dashCooldownTimer", Engine.DeltaTime + 0.02f); }
                if (DashRefillRestrict) {
                    if (!self.SceneAs<Level>().Session.Inventory.NoRefills) { self.SceneAs<Level>().Session.Inventory.NoRefills = true; }
                    if (self.Dashes > VivHelperModule.Session.dashCount) { self.Dashes = VivHelperModule.Session.dashCount; } else if (self.Dashes < VivHelperModule.Session.dashCount) { VivHelperModule.Session.dashCount = self.Dashes; }
                } else {
                    VivHelperModule.Session.dashCount = self.Dashes;
                    if (self.SceneAs<Level>().Session.Inventory.NoRefills) { self.SceneAs<Level>().Session.Inventory.NoRefills = false; }
                }
                if (StaminaRefillRestrict && self.Stamina > VivHelperModule.Session.staminaCount) { self.Stamina = VivHelperModule.Session.staminaCount; } else { VivHelperModule.Session.staminaCount = self.Stamina; }
            }

        }


        private static bool Player_UseRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes) {
            List<Entity> lE = self.CollideAll<RefillCancelSpace>();
            if (lE.Count > 0) {
                bool dashRefStop = false, stamRefStop = false;
                foreach (RefillCancelSpace rcs in lE) {
                    dashRefStop |= rcs.DashRefillRestrict;
                    stamRefStop |= rcs.StaminaRefillRestrict;
                    if (dashRefStop && stamRefStop)
                        break;
                }
                if (dashRefStop && stamRefStop) { return false; }
                if (dashRefStop && !stamRefStop) {
                    if (self.Stamina < 20f) {
                        self.RefillStamina();
                        return true;
                    }
                    return false;
                }
            }
            return orig.Invoke(self, twoDashes);
        }

        public static void Unload() {
            On.Celeste.Player.UseRefill -= Player_UseRefill;
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Player.Die -= Player_Die;
            Everest.Events.Level.OnExit -= Level_OnExit;
        }
    }

    [CustomEntity("VivHelper/RefillCancelSpace")]
    [Tracked]
    public class RefillCancelSpace : Entity {

        private static Camera camera = null;
        public bool DashRestrict, DashRefillRestrict, StaminaRefillRestrict;
        protected Color color;
        protected float width, height;
        private List<Vector2> particles = new List<Vector2>();
        private int timer;
        private float[] speeds = new float[4]
        {
            29f,
            12f,
            20f,
            40f
        };
        private readonly float altAlpha;
        private readonly bool alt;
        protected bool draw;
        public int TypeAsInt => (DashRestrict ? 1 : 0) + (DashRefillRestrict ? 2 : 0) + (StaminaRefillRestrict ? 4 : 0);

        public static Dictionary<int, Color> colorEval = new Dictionary<int, Color>()
        {
            {1, Calc.HexToColor("8b0000") },
            {2, Calc.HexToColor("6f81f4") },
            {3, Calc.HexToColor("a06ff4") },
            {4, Calc.HexToColor("8b8b00") },
            {5, Calc.HexToColor("8b4500") },
            {6, Calc.HexToColor("00b900") },
            {7, Color.LightGray }
        };

        public static Dictionary<int, Color> colorEval2 = new Dictionary<int, Color>()
        {
            {1, Calc.HexToColor("ff0000") },
            {2, Calc.HexToColor("87ceeb") },
            {3, Calc.HexToColor("9370db") },
            {4, Calc.HexToColor("dcdc00") },
            {5, Calc.HexToColor("ff9500") },
            {6, Calc.HexToColor("00dc00") },
            {7, Color.LightGray }
        };
        //Use the get1s function on the integer before parsing.
        public static Dictionary<int, Color> BWEval = new Dictionary<int, Color>()
        {
            {1, Color.Gray },
            {2, Color.Gray },
            {3, Color.LightGray },
            {4, Color.Gray },
            {5, Color.LightGray },
            {6, Color.LightGray },
            {7, Color.White }
        };

        public RefillCancelSpace(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Depth = data.Int("Depth", 1);
            draw = data.Bool("Draw", true);
            DashRestrict = data.Bool("NoDash");
            DashRefillRestrict = data.Bool("NoDashRefill");
            StaminaRefillRestrict = data.Bool("NoStaminaRefill");
            //ID = data.Attr("ID");
            width = data.Width;
            height = data.Height;
            base.Collider = new Hitbox(data.Width, data.Height);
            alt = data.Bool("Alt", false);
            for (int i = 0; (float) i < width * height / 64f; i++) {
                particles.Add(new Vector2(Calc.Random.NextFloat(width - 1f), Calc.Random.NextFloat(height - 1f)));
            }


        }

        public override void Update() {
            base.Update();
            if (draw) {
                color = VivHelperModule.Settings.ColorblindRCS ? BWEval[TypeAsInt] : ((int) VivHelperModule.Settings.DecreaseParticles != 3 ? colorEval[TypeAsInt] : colorEval2[TypeAsInt]);
                if ((int) VivHelperModule.Settings.DecreaseParticles != 3) {
                    List<Vector2> vector = new List<Vector2>();
                    switch (TypeAsInt) {
                        case 1:
                            vector.Add(Vector2.UnitY);
                            break;
                        case 2:
                            vector.Add(Vector2.UnitY.Rotate(0.66667f * (float) Math.PI));
                            break;
                        case 3:
                            vector.Add(Vector2.UnitY.Rotate(0.33333f * (float) Math.PI));
                            break;
                        case 4:
                            vector.Add(Vector2.UnitY.Rotate(1.33333f * (float) Math.PI));
                            break;
                        case 5:
                            vector.Add(Vector2.UnitY.Rotate(1.66667f * (float) Math.PI));
                            break;
                        case 6:
                            vector.Add(Vector2.UnitY.Rotate((float) Math.PI));
                            break;
                        case 7:
                            for (int h = 0; h < 6; h++) { vector.Add(Vector2.UnitY.Rotate((float) Math.PI * h / 3f)); }
                            break;
                        default:
                            throw new Exception("Temp was invalid: " + TypeAsInt);
                    }
                    int num = speeds.Length;
                    int i = 0;
                    for (int count = particles.Count; i < count; i += 1 + 2 * (int) VivHelperModule.Settings.DecreaseParticles) {
                        Vector2 value = particles[i];

                        if (TypeAsInt == 7) { value += vector[Calc.Random.Next(0, vector.Count)] * speeds[i % num] * 1.5f * Engine.DeltaTime; } else { value += vector[Math.Abs((i - 24) * (i + 19) * (i + 21) * (i - 4)) % vector.Count] * speeds[i % num] * Engine.DeltaTime; }
                        value.Y = mod(value.Y, height - 1f);
                        value.X = mod(value.X, width - 1f);
                        particles[i] = value;
                    }
                    base.Update();
                }
            }
            if (camera != (Scene as Level).Camera) {
                camera = (Scene as Level).Camera;
            }
        }

        protected float mod(float x, float m) {
            return (x % m + m) % m;
        }

        public override void Render() {
            if (camera == null) {
                if ((Engine.Scene as Level) != null && (Engine.Scene as Level).Camera != null) {
                    camera = (Engine.Scene as Level).Camera;
                } else {
                    return;
                }
            }
            if (Right > camera.Left - 8 && Left < camera.Right + 8 && Top < camera.Bottom + 16 && Bottom > camera.Top - 8) {
                base.Render();
                if (draw) {
                    if (VivHelperModule.Settings.RCSLines)
                        AltDraw();
                    else
                        BaseDraw();

                }
            }
        }

        private void AltDraw() {
            Draw.Rect(base.Collider, (int) VivHelperModule.Settings.DecreaseParticles == 3 ? color * 0.35f : Color.Lerp(color, Color.White, 0.133f * ((int) VivHelperModule.Settings.DecreaseParticles + 1)) * 0.1f);
            if ((int) VivHelperModule.Settings.DecreaseParticles != 3) {
                for (int i = 0; i < particles.Count; i += 1 + (int) (2 * (float) VivHelperModule.Settings.DecreaseParticles)) {
                    Vector2 vec = Position + particles[i];
                    Vector2 v;
                    switch (TypeAsInt) {
                        case 1:
                            v = Vector2.UnitY;
                            break;
                        case 2:
                            v = Vector2.UnitY.Rotate(0.75f * (float) Math.PI) * 1.414f;
                            break;
                        case 3:
                            v = Vector2.UnitY.Rotate(0.25f * (float) Math.PI) * 1.414f;
                            break;
                        case 4:
                            v = Vector2.UnitY.Rotate(1.25f * (float) Math.PI) * 1.414f;
                            break;
                        case 5:
                            v = Vector2.UnitY.Rotate(1.75f * (float) Math.PI) * 1.414f;
                            break;
                        case 6:
                            v = Vector2.UnitY.Rotate((float) Math.PI);
                            break;
                        default:
                            v = Vector2.Zero;
                            break;
                    }
                    if (TypeAsInt == 7)
                        Draw.Pixel.Draw(Position + particles[i], Vector2.Zero, VivHelperModule.Settings.ColorblindRCS ? color : Extensions.ColorCopy(color, 0.25f + (float) ((int) VivHelperModule.Settings.DecreaseParticles - 1) * 0.125f));
                    else
                        Draw.Line(vec - v, vec + v, VivHelperModule.Settings.ColorblindRCS ? color : Extensions.ColorCopy(color, 0.25f + (float) ((int) VivHelperModule.Settings.DecreaseParticles - 1) * 0.125f));
                }
            }
        }

        private void BaseDraw() {
            Draw.Rect(base.Collider, (int) VivHelperModule.Settings.DecreaseParticles == 3 ? color * 0.35f : Color.Lerp(Color.White, color, 0.133f * ((int) VivHelperModule.Settings.DecreaseParticles + 1)) * 0.1f);
            if ((int) VivHelperModule.Settings.DecreaseParticles != 3) {
                for (int i = 0; i < particles.Count; i += 1 + (2 * (int) VivHelperModule.Settings.DecreaseParticles)) {
                    Vector2 vec = Position + particles[i];
                    Draw.Pixel.Draw(Position + particles[i], Vector2.Zero, VivHelperModule.Settings.ColorblindRCS ? color : Extensions.ColorCopy(color, 0.25f + (float) ((int) VivHelperModule.Settings.DecreaseParticles - 1) * 0.125f));
                }
            }
        }
    }
}
