using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Utils;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using System.Collections;
using System.Reflection;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/DashCodeHeartController")]
    public class DashCodeHeartController : Entity {
        private class LevelUpOrb : Entity {
            public Image Sprite;

            public BloomPoint Bloom;

            private float ease;

            public Vector2 Target;

            public Coroutine Routine;

            public float Ease {
                get {
                    return ease;
                }
                set {
                    ease = value;
                    Sprite.Scale = Vector2.One * ease;
                    Bloom.Alpha = ease;
                }
            }

            public LevelUpOrb(Vector2 position, Color color)
                : base(position) {
                Add(Sprite = new Image(GFX.Game["characters/badeline/orb"]));
                Add(Bloom = new BloomPoint(0f, 32f));
                Add(Routine = new Coroutine(FloatRoutine()));
                Sprite.CenterOrigin();
                base.Depth = -10001;
            }

            public IEnumerator FloatRoutine() {
                Vector2 speed = Vector2.Zero;
                Ease = 0.2f;
                while (true) {
                    Vector2 target = Target + Calc.AngleToVector(Calc.Random.NextFloat((float) Math.PI * 2f), 16f + Calc.Random.NextFloat(40f));
                    float reset = 0f;
                    while (reset < 1f && (target - Position).Length() > 8f) {
                        Vector2 value = (target - Position).SafeNormalize();
                        speed += value * 420f * Engine.DeltaTime;
                        if (speed.Length() > 90f) {
                            speed = speed.SafeNormalize(90f);
                        }
                        Position += speed * Engine.DeltaTime;
                        reset += Engine.DeltaTime;
                        Ease = Calc.Approach(Ease, 1f, Engine.DeltaTime * 4f);
                        yield return null;
                    }
                }
            }

            public IEnumerator CircleRoutine(float offset) {
                Vector2 from = Position;
                float ease = 0f;
                while (true) {
                    float angleRadians = Scene.TimeActive * 2f + offset;
                    Vector2 value = Target + Calc.AngleToVector(angleRadians, 24f);
                    ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 2f);
                    Position = from + (value - from) * Monocle.Ease.CubeInOut(ease);
                    yield return null;
                }
            }

            public IEnumerator AbsorbRoutine() {
                Vector2 from = Position;
                Vector2 to = Target;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime) {
                    float num = Monocle.Ease.BigBackIn(p);
                    Position = from + (to - from) * num;
                    Ease = 0.2f + (1f - num) * 0.8f;
                    yield return null;
                }
            }
        }

        private readonly string[] key;

        private string flag;

        private bool enabled = false;

        private List<string> currentInputs = new List<string>();

        private DashListener dashListener;
        private bool removeDL = false;

        private Type type;
        private Entity retrievedHeartGem;
        private bool ScaleWigglerExists;


        public enum SpawnTypes {
            None = 0,
            LevelUp, //This is the orbs that spawn during the LevelUp cutscene.
            ForsakenCity, //ForsakenCity
            Reflection, //Reflection
            FlashSpawn, //FlashSpawn is Flash(Color)
            GlitchSpawn, //GlitchSpawn is arbitrary length glitch
            Custom //This requires that you know what you're doing and can easily crash the game if you don't.
        }

        public SpawnTypes spawnType;
        public Vector2? node; //ForsakenCity starts at the position it is placed, and travels to this node as its final position.
        public Color color = Color.White; //FlashSpawn flashes with a color, LevelUp orbs are this color, null == White
        private float rumble; //For GlitchSpawn, nonGlyph
        public enum GlitchDuration { Short, Medium, Long, Glyph }
        public GlitchDuration duration; //Glitch length.

        //CustomIEnumeratorHelper
        private MethodInfo customIEnumerator;
        private Vector2[] CustomNodes;
        private Dictionary<string, string> CustomParameters = null;
        private int paramNum;


        private static int MatchParameterInfo(ParameterInfo[] pI, bool a) {
            int i = pI.Length;
            switch (i) {
                case 0:
                    throw new InvalidPropertyException("DashCodeHeartController: Your Custom DashCodeHeartController IEnumerator has no parameters, " +
                "and it needs at minimum 1 Entity parameter. See the documentation for details on how to structure your Custom Coroutine.");
                case 1:
                    if (pI[0].ParameterType != typeof(Entity))
                        throw new InvalidPropertyException("DashCodeHeartController: Your Custom DashCodeHeartController IEnumerator has " +
                            "one parameter which is not of type Entity. See the documentation for details on how to structure your Custom Coroutine.");
                    return 1;
                case 2:
                    if (pI[0].ParameterType != typeof(Entity) || pI[1].ParameterType != typeof(Vector2[]))
                        throw new InvalidPropertyException("DashCodeHeartController: Your Custom DashCodeHeartController IEnumerator has two parameters. However, it is formatted improperly. The parameters should be ordered:" +
                            "\"Monocle.Entity, Microsoft.Xna.Framework.Vector2[]\", and you had: \"" + pI[0].ParameterType.ToString() + ", " + pI[1].ParameterType.ToString() + "\".");
                    if (a) {
                        throw new InvalidPropertyException("DashCodeHeartController: Your Custom DashCodeHeartController IEnumerator *should* work, but you also didn't actually add any nodes.");
                    }
                    return 2;
                case 3:
                    if (pI[0].ParameterType != typeof(Entity) || pI[1].ParameterType != typeof(Vector2[]) || pI[2].ParameterType != typeof(Dictionary<string, string>))
                        throw new InvalidPropertyException("DashCodeHeartController: Your Custom DashCodeHeartController IEnumerator has 3 parameters. However, it is formatted improperly. The parameters should be ordered:" +
                            "\"Monocle.Entity, Microsoft.Xna.Framework.Vector2[], System.Collections.Generic.Dictionary`2[System.String,System.String]\", and you had: \"" + pI[0].ParameterType.ToString() + ", " + pI[1].ParameterType.ToString() + ", " + pI[2].ParameterType.ToString() + "\".");
                    if (!a) {
                        throw new InvalidPropertyException("DashCodeHeartController: Your Custom DashCodeHeartController IEnumerator *should* work, but you also didn't actually add any parameters in the property menu.");
                    }
                    return 3;
                default:
                    throw new InvalidPropertyException("DashCodeHeartController: Your Custom DashCodeHeartController IEnumerator has too many parameters. See the " +
               "documentation for details on how to structure your Custom Coroutine.");
            }
        }

        public DashCodeHeartController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            string k = data.Attr("key", "");
            if (string.IsNullOrWhiteSpace(k)) {
                RemoveSelf();
                return;
            }
            key = k.Split(',').ToArray();
            foreach (string a in key)
                a.Trim(); //I did this with single line but it didnt work.
            spawnType = data.Enum<SpawnTypes>("spawnType", SpawnTypes.LevelUp);
            switch (spawnType) {
                case SpawnTypes.LevelUp:
                    color = VivHelper.ColorFix(data.Attr("Color", "White"), 1f);
                    if (data.Nodes.Length > 0)
                        node = data.Nodes[0];
                    break;
                case SpawnTypes.FlashSpawn:
                    color = VivHelper.ColorFix(data.Attr("Color", "White"), 1f);
                    break;
                case SpawnTypes.ForsakenCity:
                    if (data.Nodes.Length > 0)
                        node = data.Nodes[0];
                    break;
                case SpawnTypes.GlitchSpawn:
                    duration = data.Enum<GlitchDuration>("GlitchLength", GlitchDuration.Medium);
                    break;
                case SpawnTypes.Reflection:
                    if (data.Nodes.Length > 0)
                        node = data.Nodes[0];
                    break;
                case SpawnTypes.Custom:
                    //Checking to see if the user messed up and reporting all of the Exceptions in English
                    string ClassName = data.Attr("ClassName", "");
                    if (string.IsNullOrWhiteSpace(ClassName)) { throw new InvalidPropertyException("DashCodeHeartController: You added a Custom DashCodeHeartController with no ClassName to call your IEnumerator from."); }
                    Type type = VivHelper.GetType(ClassName, false);
                    if (type == null) {
                        throw new InvalidPropertyException("DashCodeHeartController: You added a Custom DashCodeHeartController with a ClassName \"" + ClassName + "\" which could not be found.");
                    }
                    string MethodName = data.Attr("MethodName", "");
                    if (string.IsNullOrWhiteSpace(MethodName)) { throw new InvalidPropertyException("DashCodeHeartController: You added a Custom DashCodeHeartController with a ClassName but no IEnumerator MethodName from which we can run the coroutine."); }
                    MethodInfo methodInfo = type.GetMethod(MethodName, BindingFlags.Static | BindingFlags.Public);
                    if (methodInfo == null)
                        throw new InvalidPropertyException("DashCodeHeartController: You added a Custom DashCodeHeartController with a ClassName and a MethodName, however the code was not able to find your Method in the class. This could be because the IEnumerator method was not static.");
                    if (methodInfo.ReturnType != typeof(IEnumerator))
                        throw new InvalidPropertyException("DashCodeHeartController: You added a Custom DashCodeHeartController with a ClassName and a MethodName, however, the Method that was found was not an IEnumerator. (If you used IEnumerator<T>, you can't do that, sorry :/)");
                    bool b1 = false;
                    if (data.Nodes.Length > 0) { CustomNodes = data.NodesOffset(offset); b1 = true; }
                    string CustomParams = data.Attr("CustomParameters", "");
                    CustomParameters = new Dictionary<string, string>();
                    if (!string.IsNullOrWhiteSpace(CustomParams)) {
                        if (b1)
                            b1 = false;
                        string[] s1 = CustomParams.TrimEnd('|').Split('|'); //Trim added in case people misinterpret my instructions.
                        if (s1.Length > 0) {

                            foreach (string s2 in s1) {
                                string[] s3 = s2.Split(':');
                                if (s3.Length < 2) { throw new InvalidPropertyException("Invalid Custom Parameter in DashCodeHeartController, each Parameter must be formatted as \"Tag:Value|\"."); }
                                CustomParameters.Add(s3[0], s3[1]);
                            }
                        }
                    }

                    paramNum = MatchParameterInfo(methodInfo.GetParameters(), b1); //There are more exceptions here, if this passes the test then it most definitely should be the right method;
                    customIEnumerator = methodInfo;
                    break;
                default:
                    break;
            }
            flag = data.Attr("CompleteFlag", "");
            Console.WriteLine("Flag: " + flag);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            foreach (string s in (scene as Level).Session.Flags)
                Console.WriteLine(s);
            if (string.IsNullOrWhiteSpace(flag) || (scene as Level).Session.GetFlag(flag)) { RemoveSelf(); return; }
            bool b = false;
            //We cannot check for a player collider since that is added to the HeartGem in Awake, so we have to check HoldableCollider, since that's added at start.
            foreach (Component tie in scene.Tracker.GetComponents<HoldableCollider>()) {
                if (CheckEntity(tie.Entity.GetType(), tie.Entity)) {
                    b = true;
                    break;
                }
            }
            if (!b) {
                Console.WriteLine("Hey! This didn't find any valid entities.");
                RemoveSelf();
                return;
            }
            Add(dashListener = new DashListener());
            dashListener.OnDash = DashListenerFunction;

        }

        private bool CheckEntity(Type t, Entity e) {
            if (t.IsAssignableFrom(typeof(HeartGem)) || t.ToString() == "Celeste.Mod.CollabUtils2.Entities.MiniHeart") {
                type = t;
                retrievedHeartGem = e;
                e.RemoveSelf();

                return true;
            }
            return false;

        }

        public override void Update() {
            base.Update();
            if (removeDL) {
                dashListener.RemoveSelf();
                removeDL = false;
            }
        }

        public void DashListenerFunction(Vector2 dir) {

            string text = "";
            if (dir.Y < 0f) {
                text = "U";
            } else if (dir.Y > 0f) {
                text = "D";
            }
            if (dir.X < 0f) {
                text += "L";
            } else if (dir.X > 0f) {
                text += "R";
            }
            currentInputs.Add(text);
            if (currentInputs.Count > key.Length) {
                currentInputs.RemoveAt(0);
            }
            if (currentInputs.Count == key.Length) {
                bool flag = true;
                for (int i = 0; i < key.Length; i++) {
                    if (!currentInputs[i].Equals(key[i])) {
                        flag = false;
                    }
                }
                if (flag) {
                    MasterCutsceneController();
                    removeDL = true;
                }
            }
        }

        public void MasterCutsceneController() {
            retrievedHeartGem.Remove(retrievedHeartGem.Get<Sprite>());
            switch (spawnType) {
                case SpawnTypes.ForsakenCity:
                    Add(new Coroutine(ForsakenCitySpawn()));
                    break;
                case SpawnTypes.Reflection:
                    Add(new Coroutine(ReflectionSpawn()));
                    break;
                case SpawnTypes.LevelUp:
                    Add(new Coroutine(LevelUpSpawn()));
                    break;
                case SpawnTypes.FlashSpawn:
                    Add(new Coroutine(FlashSpawn()));
                    break;
                case SpawnTypes.GlitchSpawn:
                    Add(new Coroutine(GlitchMaster()));
                    break;
                case SpawnTypes.Custom:
                    switch (paramNum) {
                        case 1:
                            Add(new Coroutine((IEnumerator) customIEnumerator.Invoke(null, new object[] { retrievedHeartGem })));
                            break;
                        case 2:
                            Add(new Coroutine((IEnumerator) customIEnumerator.Invoke(null, new object[] { retrievedHeartGem, CustomNodes })));
                            break;
                        case 3:
                            Add(new Coroutine((IEnumerator) customIEnumerator.Invoke(null, new object[] { retrievedHeartGem, CustomNodes, CustomParameters })));
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    Add(new Coroutine(Spawn()));
                    break;
            }
        }

        public IEnumerator Spawn() {
            yield return null;
            Scene.Add(retrievedHeartGem);
            if (!string.IsNullOrWhiteSpace(flag))
                (Scene as Level).Session.SetFlag(flag);
        }

        public IEnumerator ForsakenCitySpawn() {
            ParticleSystem particles = new ParticleSystem(-10000, 100);
            Vector2 alternatePosition = retrievedHeartGem.Position - new Vector2(96f, 56f);
            particles.Emit(BirdNPC.P_Feather, 24, node ?? alternatePosition, new Vector2(4f, 4f));
            Level level = Scene as Level;
            level.Add(particles);
            Vector2 gemSpawnPosition = retrievedHeartGem.Position;
            retrievedHeartGem.Position = node ?? alternatePosition;
            retrievedHeartGem.Collidable = false;

            level.Add(retrievedHeartGem);
            yield return 0.85f;
            Vector2 extraYoffset = new Vector2(0f, Math.Abs(retrievedHeartGem.Position.Y - gemSpawnPosition.Y) * -1.15f);
            SimpleCurve curve = new SimpleCurve(retrievedHeartGem.Position, gemSpawnPosition, (retrievedHeartGem.Position + gemSpawnPosition) / 2f + extraYoffset);
            for (float t = 0f; t < 1f; t += Engine.DeltaTime) {
                yield return null;
                retrievedHeartGem.Position = curve.GetPoint(Ease.CubeInOut(t));
            }
            yield return 0.5f;
            retrievedHeartGem.Collidable = true;
            particles.RemoveSelf();
            if (!string.IsNullOrWhiteSpace(flag))
                (Scene as Level).Session.SetFlag(flag);
            yield return 0.1f;
            RemoveSelf();
        }

        public IEnumerator ReflectionSpawn() {
            Audio.Play("event:/game/06_reflection/supersecret_heartappear");
            Vector2 pos = retrievedHeartGem.Position;
            Entity dummy = new Entity(pos) {
                Depth = 1
            };
            Scene.Add(dummy);
            Image white = new Image(GFX.Game["collectables/heartgem/white00"]);
            white.CenterOrigin();
            white.Scale = Vector2.Zero;
            dummy.Add(white);
            BloomPoint glow = new BloomPoint(0f, 16f);
            dummy.Add(glow);
            List<Entity> absorbs = new List<Entity>();
            for (int i = 0; i < 20; i++) {
                AbsorbOrb absorbOrb = new AbsorbOrb(node ?? pos + Calc.AngleToVector(i * 0.31416f, 8f), dummy);
                Scene.Add(absorbOrb);
                absorbs.Add(absorbOrb);
                yield return null;
            }
            yield return 0.8f;
            float duration = 0.6f;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration) {
                white.Scale = Vector2.One * p;
                glow.Alpha = p;
                (Scene as Level).Shake();
                yield return null;
            }
            foreach (Entity item in absorbs) {
                item.RemoveSelf();
            }
            (Scene as Level).Flash(Color.White);
            Scene.Remove(dummy);
            Scene.Add(retrievedHeartGem);
            if (!string.IsNullOrWhiteSpace(flag))
                (Scene as Level).Session.SetFlag(flag);
            yield return 0.1f;
            RemoveSelf();
        }

        public IEnumerator FlashSpawn() {
            yield return null;
            (Scene as Level).Flash(color, false);
            Scene.Add(retrievedHeartGem);
            if (!string.IsNullOrWhiteSpace(flag))
                (Scene as Level).Session.SetFlag(flag);
            yield return 0.1f;
            RemoveSelf();
        }

        public IEnumerator LevelUpSpawn() {
            Audio.Play("event:/VivHelper/levelUpAudio");
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            float size = 1f;
            List<LevelUpOrb> orbs = new List<LevelUpOrb>();
            while (orbs.Count < 8) {
                float to = size - 0.125f;
                while (size > to) {
                    size -= Engine.DeltaTime;
                    yield return null;
                }
                LevelUpOrb orb = new LevelUpOrb(node ?? retrievedHeartGem.Position, color);
                orb.Target = retrievedHeartGem.Position;
                orb.Routine.Replace(orb.CircleRoutine((orbs.Count + 1) / 8f * ((float) Math.PI * 2f)));
                Scene.Add(orb);
                orbs.Add(orb);
            }
            yield return 2.716666666f;
            foreach (LevelUpOrb orb3 in orbs) {
                orb3.Routine.Replace(orb3.AbsorbRoutine());
            }
            yield return 1f;
            Scene.Add(retrievedHeartGem);
            foreach (LevelUpOrb orb in orbs) { orb.RemoveSelf(); }
            orbs.Clear();
            if (!string.IsNullOrWhiteSpace(flag))
                (Scene as Level).Session.SetFlag(flag);
            yield return 0.1f;
            RemoveSelf();
        }

        private IEnumerator GlitchMaster() {
            if (duration == GlitchDuration.Glyph) {
                yield return GlyphSpawn();
            } else {
                Tag = Tags.Persistent;
                float num;
                if (duration == GlitchDuration.Short) {
                    num = 0.2f;
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    Audio.Play("event:/new_content/game/10_farewell/glitch_short");
                } else if (duration == GlitchDuration.Medium) {
                    num = 0.5f;
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    Audio.Play("event:/new_content/game/10_farewell/glitch_medium");
                } else {
                    num = 1.25f;
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                    Audio.Play("event:/new_content/game/10_farewell/glitch_long");
                }
                yield return GlitchSpawn(num);
            }
            if (!string.IsNullOrWhiteSpace(flag))
                (Scene as Level).Session.SetFlag(flag);
            yield return 0.1f;
            RemoveSelf();
        }

        private IEnumerator GlitchSpawn(float duration) {
            if (Settings.Instance.DisableFlashes) {
                rumble = 1f;
                DisplacementRenderHook d;
                Add(d = new DisplacementRenderHook(RenderDisplacement));
                float i = duration;
                float j = duration;
                bool b = false;
                while (rumble > 0f) {
                    i -= Engine.DeltaTime;
                    rumble = i / j;
                    if (rumble < 0.5f && !b) {
                        b = true;
                        Scene.Add(retrievedHeartGem);
                    }
                }
                Remove(d);
            } else if (duration > 0.4f) {
                Glitch.Value = 0.3f;
                yield return 0.2f;
                Glitch.Value = 0f;
                yield return (duration - 0.4f);
                Glitch.Value = 0.45f;
                Scene.Add(retrievedHeartGem);
                yield return 0.2f;
                Glitch.Value = 0f;
            } else {
                Glitch.Value = 0.3f;
                yield return duration / 3;
                Scene.Add(retrievedHeartGem);
                yield return duration / 1.5;
                Glitch.Value = 0f;
            }

        }

        private IEnumerator GlyphSpawn() {
            Level level = SceneAs<Level>();
            level.Displacement.Clear();
            yield return 0.2f;
            level.Frozen = true;
            base.Tag = Tags.FrozenUpdate;
            Vector2 pos = retrievedHeartGem.Position;
            BloomPoint bloom = new BloomPoint(pos, 1f, 32f);
            Glitch.Value = 0.1f;
            yield return 0.6f;
            ParticleSystem particles = new ParticleSystem(-10000, 100) {
                Tag = Tags.FrozenUpdate
            };
            level.Add(particles);
            retrievedHeartGem.Tag = Tags.FrozenUpdate;
            retrievedHeartGem.Collidable = false;
            retrievedHeartGem.Visible = false;
            level.Add(retrievedHeartGem);
            yield return null;
            Glitch.Value = 0.3f;
            yield return 1.2f;
            level.Displacement.AddBurst(pos, 20f, 0f, 3000f);
            Glitch.Value = 0.7f;
            yield return 2f;
            retrievedHeartGem.Visible = true;
            yield return 2f;
            Glitch.Value = 0.3f;
            yield return 2f;
            Glitch.Value = 0.1f;
            yield return 0.5f;
            particles.RemoveSelf();
            Remove(bloom);
            Glitch.Value = 0f;
            level.Frozen = false;
            retrievedHeartGem.Collidable = true;
        }

        private void RenderDisplacement() {
            if (rumble > 0) {
                Camera camera = (base.Scene as Level).Camera;
                int num = (int) (camera.Left / 8f) - 1;
                int num2 = (int) (camera.Right / 8f) + 1;
                for (int i = num; i <= num2; i++) {
                    float num3 = (float) Math.Sin(base.Scene.TimeActive * 60f + (float) i * 0.4f) * 0.06f * rumble;
                    Draw.Rect(color: new Color(0.5f, 0.5f + num3, 0f, 1f), x: i * 8, y: camera.Top - 2f, width: 8f, height: 184f);
                }
            }
        }
    }
}
