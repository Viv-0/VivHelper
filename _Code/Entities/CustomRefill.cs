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

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CustomRefill = CR1", "VivHelper/CustomRefillString = CR2")]
    public class CustomRefill : Entity {
        private enum WobbleTypes { None = -1, YOnly = 0, XOnly = 1, Circle = 2 }

        public static Entity CR1(Level level, LevelData levelData, Vector2 offset, EntityData entityData) { return new CustomRefill(entityData, offset, 0); }
        public static Entity CR2(Level level, LevelData levelData, Vector2 offset, EntityData entityData) { return new CustomRefill(entityData, offset, 1); }

        private int CurrentDashCount => (Engine.Scene as Level)?.Tracker.GetEntity<Player>()?.Dashes ?? 0;
        private float CurrentStaminaCount => (Engine.Scene as Level)?.Tracker.GetEntity<Player>()?.Stamina ?? 0f;
        //iParser operates here
        public float iStamina; //float
        public bool iStamRef; //bool      false == less than or equal to, true == greater than
        public bool logic; //bool         false == ||, true == &&
        public int iDashes; //int           iDashes < 0 ? Inventory.Dashes
        public short iDashRef = -3; //short -3 = less than -2 = less than or equal to, -1 = not equal to, 0 = equal to, 1 = greater than or equal to, 2 = greater than.

        private void iParser(string[] SMaster) {
            //SMaster => stam [&/|] dash
            //if the length
            string S, D;
            if (SMaster.Length == 2) { logic = false; S = SMaster[0]; D = SMaster[1]; } else { logic = (SMaster[1].Length == 0 ? false : SMaster[1][0] == '&'); S = SMaster[0]; D = SMaster[2]; }
            iStamRef = S[0] == '>';
            if (!FloatParser(S[0] == '>' || S[0] == '<' ? S.Substring(1) : S, false, out iStamina)) { iStamina = 20f; }
            int k = 0;
            if (D[0] == '<') {
                k++;
                if (D[1] == '=') { iDashRef = -2; k++; } else { iDashRef = -3; }
            } else if (D[0] == '>') {
                k++;
                if (D[1] == '=') { iDashRef = 1; k++; } else { iDashRef = 2; }
            } else if (D[0] == '=') { k++; iDashRef = 0; } else if (D.StartsWith("!=")) { k += 2; iDashRef = -1; } else { iDashRef = -3; }

            if (!IntParser(D.Substring(k), true, out iDashes)) { iDashes = -1; iDashRef = -1; }

        }

        private bool IntParser(string number, bool _, out int k) {
            if (string.IsNullOrEmpty(number)) {
                throw new Exception("Integer was empty.");
            }
            //+ refers to addition
            if (number.Contains("+")) {
                string[] q = number.Split('+');
                int p = 0;
                for (int s = 0; s < q.Length; s++) { if (!IntParser(q[s], _, out int o)) { k = 0; return false; } p += o; }
                k = p;
                return true;
            }
            // * refers to multiplication
            if (number.Contains("*")) {
                string[] q = number.Split('*');
                int p = 1;
                for (int s = 0; s < q.Length; s++) { if (!IntParser(q[s], _, out int o)) { k = 0; return false; } p *= o; }
                k = p;
                return true;
            }
            // / refers to division
            if (number.Contains("/")) {
                string[] q = number.Split('/');
                int p = 1;
                for (int s = 0; s < q.Length; s++) { if (!IntParser(q[s], _, out int o)) { k = 0; return false; } if (o == 0) { k = 0; return false; } p /= o; }
                k = p;
                return true;
            }
            if (number.Trim() == "U") { k = UseNumber; return true; }
            if (number.Trim() == "u") { k = count; return true; }
            if (number.Trim() == "D" && _) { k = SceneAs<Level>()?.Session?.Inventory.Dashes ?? 1; return true; }
            return int.TryParse(number.Trim(), out k);
        }

        private bool FloatParser(string number, bool _, out float k) {
            if (string.IsNullOrEmpty(number)) {
                throw new Exception("Float was empty.");
            }
            //+ refers to addition
            if (number.Contains("+")) {
                string[] q = number.Split('+');
                float p = 0;
                for (int s = 0; s < q.Length; s++) { if (!FloatParser(q[s], _, out float o)) { k = 0; return false; } p += o; }
                k = p;
                return true;
            }
            // * refers to multiplication
            if (number.Contains("*")) {
                string[] q = number.Split('*');
                float p = 1;
                for (int s = 0; s < q.Length; s++) { if (!FloatParser(q[s], _, out float o)) { k = 0; return false; } p *= o; }
                k = p;
                return true;
            }
            // / refers to division
            if (number.Contains("/")) {
                string[] q = number.Split('/');
                float p = 1;
                for (int s = 0; s < q.Length; s++) { if (!FloatParser(q[s], _, out float o)) { k = 0; return false; } if (o == 0) { k = 0; return false; } p /= o; }
                k = p;
                return true;
            }
            if (number.Trim() == "D") { k = _ ? 110f : 20f; return true; }
            return float.TryParse(number.Trim(), out k);
        }

        //oParser operates here
        public float oStamina; //float, if oStamina < 0 then oStamina == 110
        public bool oStamRef; //false = set player.Stamina to oStamina, //true == add oStamina to player.Stamina
        public int oDashes; //int, if oDashes < 0 then oDashes == Inventory.Dashes
        public bool oDashRef; //false = set player.Dashes to oDashes, //true = add oDashes to player.Dashes

        private void oParser(string S, string D) {
            oStamRef = false;
            if (S[0] == '+') { oStamRef = true; S = S.Substring(1); }
            if (!FloatParser(S, true, out oStamina)) { oStamina = 110f; oStamRef = false; }
            oDashRef = false;
            if (D[0] == '+') { oDashRef = true; D = D.Substring(1); }
            if (!IntParser(D, true, out oDashes)) { oDashes = SceneAs<Level>().Session.Inventory.Dashes; oDashRef = false; }
        }

        public int count = 0;
        public int UseNumber = int.MaxValue;
        public bool usecustomoutline = false;
        public bool UseCustomOutline => usecustomoutline || VivHelperModule.Settings.ShowOneUseOnCustomRefills;

        public string directory; //str => directory
        public float scale, imageScale; //Custom Scaling because its a cool idea.

        public ParticleType P_Shatter, P_Glow, P_Regen; //Particles set up with magic ParticleParser

        public ParticleType particle;

        //Sprites
        private Sprite sprite; //directory + "idle"
        private Sprite flash; //directory + "flash"
        private Image outline; //directory + "outline"

        private BloomPoint bloom;
        private VertexLight light;
        private string[] CustomAudio; //CustomAudio[0] == touch, CustomAudio[1] == return

        protected Wiggler wiggler;
        private WobbleTypes wobble;

        private SineWave[] sine;

        private Level level;
        public float RespawnTime;
        private float respawnTimer;
        private int[] Depths = new int[2];
        public string flagToggle; public bool flagInvert;
        public float FreezeTime;
        internal string mapName;
        //CustomOutline == 0
        public float OutlineColorLerp() {
            float q = Math.Min(1f, (float) (count + 1) / (float) UseNumber); // Division is highly inefficient, so I utilize a local variable for the cubic
            return q * q * q; //Using the cubic curve from [0,1] because it's nicer view-wise. I wanted to do a Lorenz curve but it's computationally heavy.
        }

        public CustomRefill(EntityData e, Vector2 v, int type) {
            Position = e.Position + v;
            //Custom Refill Mechanics
            string iS, iL, iD, oS, oD;
            switch (type) {
                case 0:
                    iS = e.Float("iStamina", 20f).ToString();
                    iL = e.Bool("iLogic") ? "&" : "|";
                    iD = e.Int("iDashes", -1) < 0 ? "D" : e.Int("iDashes").ToString();
                    oS = e.Float("oStamina", 110f).ToString();
                    oD = e.Int("oDashes", -1) < 0 ? "D" : e.Int("oDashes").ToString();
                    UseNumber = e.Int("UseNumber", -1);
                    if (UseNumber < 0)
                        UseNumber = int.MaxValue;
                    break;
                case 1:
                    iS = e.Attr("iStam", "20");
                    iL = string.IsNullOrWhiteSpace(e.Attr("iLog")) ? "|" : e.Attr("iLog");
                    iD = e.Attr("iDash", "D");
                    oS = e.Attr("oStam", "110");
                    oD = e.Attr("oDash", "D");
                    if (!IntParser(e.Attr("UseNum", "-1"), false, out UseNumber)) { UseNumber = int.MaxValue; } else { if (UseNumber < 1) UseNumber = int.MaxValue; }

                    break;
                default:
                    throw new Exception("How did this happen? Please report what you were doing to Viv when this error occurred.");
            }
            iParser(new string[] { iS, iL, iD });
            oParser(oS, oD);
            usecustomoutline = e.Bool("ShowColoredUseOutline", false);

            // Custom Refill base Constructor, modded out

            //Scale & Colliders
            scale = e.Float("Scale", 1f);
            imageScale = (float) e.Int("ImageSize", 16) * 0.0625f;
            base.Collider = new Hitbox(16f * scale, 16f * scale, -8f * scale, -8f * scale);

            flagToggle = e.Attr("FlagToggle");
            if (!string.IsNullOrWhiteSpace(flagToggle) && flagToggle[0] == '!') {
                flagInvert = true;
                flagToggle = flagToggle.Substring(1);
            }
            Add(new PlayerCollider(OnPlayer));

            //Dealing with Particle stuff.
            string[] sources = ModString(e.Attr("ParticleSources"), 0, v);
            //IDK why i went so code-inefficient, but I did, I don't care unless it freezes up the game. And this won't, probably :sweat:
            string[] temp1 = ModString(e.Attr("ParticleColors"), 1, v);
            string[][] temp2 = new string[3][];
            for (int i = 0; i < 2; i++)
                temp2[i] = temp1[i].Split(':');
            temp2[2] = temp1.Length == 2 ? temp2[1] : temp2[2] = temp1[2].Split(':');
            Color?[][] colors = new Color?[3][];
            for (int i = 0; i < 3; i++) { colors[i] = new Color?[2]; } //This is gross. I shouldve used a single array of area 2x3, whatever.
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 2; j++) // is there a better way to numerically iterate over a 2d array in C#? I like this way, but it's not *good*, technically an iterable is more efficient.
{
                    string a = temp2[i][j];
                    if (string.IsNullOrWhiteSpace(a))
                        colors[i][j] = null;
                    else
                        colors[i][j] = VivHelper.OldColorFunction(a);
                }
            ParticleParser(sources, colors, oDashes > 1);

            //Wobble Type
            wobble = e.Enum<WobbleTypes>("WobbleType", WobbleTypes.YOnly);

            //Sprites and Images
            directory = e.Attr("Directory", "objects/refill/").TrimEnd('/') + "/";
            Add(outline = new Image(GFX.Game[directory + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, directory + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, directory + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate {
                flash.Visible = false;
            };
            flash.CenterOrigin();

            //WiggleEffect
            float f = e.Float("ScaleWiggleEffect", 0.2f);
            if (f > 0f)
                Add(wiggler = Wiggler.Create(1f, 4f, delegate (float w) { sprite.Scale = (flash.Scale = Vector2.One * (1f + w * f)); }));

            //Mirror Reflection, Bloom Point, Vertex Light, and Audio
            Add(new MirrorReflection());

            string o = e.Attr("BloomPoint", "0.8,16").Replace('|', ',');
            if (!string.IsNullOrWhiteSpace(o) && o.Contains(",")) {
                string[] o_ = o.Split(',');
                if (float.TryParse(o_[0], out float f1) && float.TryParse(o_[1], out float f2)) {
                    Add(bloom = new BloomPoint(f1, f2));
                }
            }

            string[] p = e.Attr("VertexLight", "White,1,16,48").Split(',');
            if (p.Length == 4) {
                p[0].Trim();
                Color _c = VivHelper.OldColorFunction(p[0]);
                if (float.TryParse(p[1].Trim(), out float f_1) && int.TryParse(p[2].Trim(), out int f_2) && int.TryParse(p[3].Trim(), out int f_3)) {
                    Add(light = new VertexLight(_c, f_1, f_2, f_3));
                }
            }
            if (string.IsNullOrWhiteSpace(e.Attr("CustomAudio")))
                CustomAudio = new string[2] { "", "" };
            else
                CustomAudio = e.Attr("CustomAudio", ",").Split(',');
            if (CustomAudio.Length != 2)
                throw new Exception("Invalid CustomAudio parameter for Custom Refill at " + (Position - v).ToString() + ".");
            CustomAudio[0] = string.IsNullOrWhiteSpace(CustomAudio[0]) ? (oDashes > 1 ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch") : CustomAudio[0];
            CustomAudio[1] = string.IsNullOrWhiteSpace(CustomAudio[1]) ? (oDashes > 1 ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return") : CustomAudio[1];
            //Wobble Functions
            sine = new SineWave[2];
            sine[0] = new SineWave(0.6f);
            sine[1] = new SineWave(0.6f);
            Add(sine[0]);
            Add(sine[1]);
            sine[0].Randomize();
            sine[1].Counter = (sine[0].Value + Consts.PIover2) % Consts.PIover2;
            //it is 1 quarterrotation off of sine[0]'s random value. Used very cleanly because in isolation, rand(0, 1) + .25 % 1.0 == rand(0, 1),
            // assuming pure randomness, and in the case of the circle works well. Was gonna implement parametrics because I'm masochistic.

            string[] z = e.Attr("Depths", "-100,8999").Split(',');
            Depths[0] = int.Parse(z[0]);
            Depths[1] = int.Parse(z[1]);
            base.Depth = Depths[0];
            UpdateWobble();
            RespawnTime = e.Float("RespawnTime", 2.5f);
            FreezeTime = e.Float("FreezeTime", 0.05f);

        }

        //A BS way I thought to simply the Foreach process instead of using an Enumerable. I did most of this code on no ADHD meds and no sleep.
        private string[] ModString(string s, int t, Vector2 v) {
            string[] u;
            switch (t) {
                case 0:
                    if (string.IsNullOrWhiteSpace(s))
                        s = "||";
                    u = s.Trim().Split('|');
                    if (u.Length != 3)
                        throw new Exception("Invalid ParticleSources parameter for Custom Refill at " + (Position - v).ToString() + ".");
                    return u;
                case 1:
                    if (string.IsNullOrWhiteSpace(s))
                        s = oDashes > 1 ? "FFD3F9:EF94E3|FFA5AA:DD6CCA" : "d3ffd4:85fc87|a5fff7:6de081";
                    u = s.Trim().Split('|');
                    if (u.Length < 2 || u.Length > 3)
                        throw new Exception("Invalid ParticleColors parameter for Custom Refill at " + (Position - v).ToString() + ".");
                    return u;
                default:
                    throw new Exception("HOW");
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
            mapName = level.Session?.Area.GetSID() ?? string.Empty;
        }

        public override void Update() {
            base.Update();
            if (respawnTimer > 0f && (string.IsNullOrEmpty(flagToggle) || flagInvert != (Scene as Level).Session.GetFlag(flagToggle))) {
                respawnTimer = Math.Max(0f, respawnTimer - Engine.DeltaTime);
                if (respawnTimer == 0f) {
                    Respawn();
                }
            } else if (base.Scene.OnInterval(0.1f)) {
                level.ParticlesFG.Emit(P_Glow, 1, Position, Vector2.One * 5f);
            }
            UpdateWobble();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
            if (base.Scene.OnInterval(2f) && sprite.Visible) {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }

        private void Respawn() {
            if (!Collidable && !(count > UseNumber)) //Fixes our UseNumber problems
            {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                base.Depth = Depths[0];
                wiggler.Start();
                Audio.Play(CustomAudio[1], Position);
                level.ParticlesFG.Emit(P_Regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateWobble() {
            Sprite obj = flash;
            Sprite obj2 = sprite;
            if (wobble == WobbleTypes.XOnly || wobble == WobbleTypes.Circle) { float num1 = obj.X = obj2.X = sine[0].TwoValue; if (bloom != null) bloom.X = num1; }
            if (wobble == WobbleTypes.YOnly || wobble == WobbleTypes.Circle) { float num2 = obj.Y = obj2.Y = sine[1].TwoValue; if (bloom != null) bloom.Y = num2; }

        }

        public override void Render() {
            if (sprite.Visible) {
                if (UseCustomOutline) {
                    sprite.DrawOutline(Color.Lerp(Color.White, Color.Red, OutlineColorLerp()));
                    outline.Color = Color.Lerp(Color.White, Color.Red, OutlineColorLerp());
                } else { outline.Color = Color.White; }
                sprite.DrawOutline();
            }
            base.Render();
        }

        private void OnPlayer(Player player) {
            if (VivHelperModule.OldGetFlags(Scene as Level, flagToggle.Split(','), "and") && AnalyzeRefill(player)) {
                Audio.Play(CustomAudio[0], Center);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));

            }
        }

        public bool AnalyzeRefill(Player P) {
            bool iS, iD;
            iS = (iStamRef ? P.Stamina > iStamina : P.Stamina <= iStamina);
            switch (iDashRef) {
                case -2:
                    iD = P.Dashes <= iDashes;
                    break;
                case -1:
                    iD = P.Dashes != iDashes;
                    break;
                case 0:
                    iD = P.Dashes == iDashes;
                    break;
                case 1:
                    iD = P.Dashes >= iDashes;
                    break;
                case 2:
                    iD = P.Dashes > iDashes;
                    break;
                default:
                    iD = P.Dashes < iDashes;
                    break;
            }
            bool ret = logic ? iS && iD : iS || iD;
            if (ret) {
                P.Stamina = oStamRef ? P.Stamina + oStamina : oStamina < 0 ? 110f : oStamina;
                P.Dashes = oDashRef ? (mapName == "KAERRA/FurryWeek/day3" ? P.Dashes + oDashes : Math.Max(0, P.Dashes + oDashes)) : oDashes < 0 ? SceneAs<Level>().Session.Inventory.Dashes : oDashes;
            }
            return ret;
        }

        private IEnumerator RefillRoutine(Player player) {
            if (FreezeTime > 0)
                Celeste.Celeste.Freeze(FreezeTime);
            yield return null;
            level.Shake();
            sprite.Visible = (flash.Visible = false);
            if (count++ <= UseNumber) {
                outline.Visible = true;
                respawnTimer = RespawnTime;
            }

            Depth = Depths[1];
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - Consts.PIover2);
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + Consts.PIover2);
            SlashFx.Burst(Position, num);
            if (count > UseNumber) {
                RemoveSelf();
            }
        }

        private void ParticleParser(string[] sources, Color?[][] colors, bool twodash) //always Color?[3][2]
        {
            P_Shatter = new ParticleType(Refill.P_Shatter);
            if (!string.IsNullOrWhiteSpace(sources[0]))
                P_Shatter.Source = GFX.Game[sources[0]];
            if (colors[0][0] != null)
                P_Shatter.Color = colors?[0][0] ?? Calc.HexToColor("d3ffd4");
            if (colors[0][1] != null)
                P_Shatter.Color2 = colors?[0][1] ?? Calc.HexToColor("85fc87");

            P_Glow = new ParticleType(Refill.P_Glow);
            if (!string.IsNullOrWhiteSpace(sources[1]))
                P_Glow.Source = GFX.Game[sources[1]];
            if (colors[1][0] != null)
                P_Glow.Color = colors?[1][0] ?? Calc.HexToColor("a5fff7");
            if (colors[1][1] != null)
                P_Glow.Color2 = colors?[1][1] ?? Calc.HexToColor("6de081");

            P_Regen = new ParticleType(Refill.P_Glow);
            if (!string.IsNullOrWhiteSpace(sources[2]))
                P_Regen.Source = GFX.Game[sources[2]];
            if (colors[2][0] != null)
                P_Regen.Color = colors?[2][0] ?? Calc.HexToColor("a5fff7");
            else if (colors[1][0] != null)
                P_Regen.Color = colors?[1][0] ?? Calc.HexToColor("a5fff7");
            if (colors[2][1] != null)
                P_Regen.Color2 = colors?[2][1] ?? Calc.HexToColor("6de081");
            else if (colors[1][1] != null)
                P_Regen.Color2 = colors?[1][1] ?? Calc.HexToColor("6de081");
        }
    }
}
