using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System.Reflection;
using Celeste.Mod.Helpers;
using MonoMod.Utils;
using Celeste.Mod;
using Mono.Cecil.Cil;
using Mono.Cecil;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using System.Text.RegularExpressions;

namespace VivHelper {
    /// <summary>
    /// Helper functions for VivHelper
    /// </summary>
    public static class VivHelper {
        #region ParsingGroupsFromString


        public static T[] ParseArrayFromString<T>(string val, char groupSeparator, Func<string, T> tParser) {
            string[] s = val.Split(groupSeparator);
            T[] ts = new T[s.Length];
            for (int i = 0; i < s.Length; i++) {
                ts[i] = tParser(s[i]);
            }
            return ts;
        }

        public static Dictionary<string, T> ParseDictFromString<T>(string val, char groupSeparator, char keyValSeparator, Func<string, T> tParser) {
            string[] _s = val.Split(groupSeparator);
            Dictionary<string, T> dict = new Dictionary<string, T>();
            foreach (string s in _s) {
                string[] r = s.Split(keyValSeparator);
                if (r.Length != 2)
                    throw new Exception("Invalid Key-Value Pair in string!");
                T t = tParser(r[1]);
                dict[r[0]] = t;
            }
            return dict;
        }

        #endregion

        public static bool isPrime(int number) {
            if (number <= 1)
                return false;
            if (number == 2)
                return true;
            if (number % 2 == 0)
                return false;

            var boundary = (int) Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }
        public static int nearestPrime(int number, bool below) {
            while (true) {
                if (isPrime(number))
                    return number;
                number += below ? -1 : 1;
            }
        }

        public static Sprite CloneSprite(Sprite input) {
            return _CloneSprite(input);
        }

        public static bool IsValidRoomName(string input, List<LevelData> levels) {
            return levels.Any(l => l.Name == input);
        }


        public static Color GetHue(Scene scene, Vector2 pos) {
            if (scene == null) {
                Console.WriteLine("Scene supplied was null!");
                return Color.White;
            }
            if (VivHelperModule.crystalSpinner == null) {
                VivHelperModule.crystalSpinner = new CrystalStaticSpinner(Vector2.Zero, false, CrystalColor.Rainbow);
            } else if (VivHelperModule.crystalSpinner.Scene == scene)
                return _getHueNoScene(pos);
            return _getHue(scene, pos);
        }

        public static Dictionary<string, Ease.Easer> EaseHelper;
        public static Dictionary<string, Color> ColorHelper;

        public static Color? ColorFixWithNull(string s) {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            if (s == "Transparent" || s.Length == 8 && s.Substring(0, 2) == "00" && int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int _)) { return Color.Transparent; } //Do this check first because we check for any other case of transparency, which is equivalent to no other valid case in this case

            var c = ColorFix(s);
            if (c == Color.Transparent)
                return null;
            return c;
        }

        public static Color ColorFix(string s) {
            if (ColorHelper.ContainsKey(s))
                return ColorHelper[s];
            return AdvHexToColor(s);
        }

        public static Color ColorFix(string s, float alpha) {
            if (ColorHelper.ContainsKey(s))
                return ColorHelper[s];
            return Extensions.ColorCopy(AdvHexToColor(s), alpha);
        }

        public static Color AdvHexToColor(string hex, bool nullIfInvalid = false) {
            string hexplus = hex.Trim('#');
            if (hexplus.StartsWith("0x"))
                hexplus = hexplus.Substring(2);
            int result;
            if (hexplus.Length == 6 && int.TryParse(hexplus, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out result)) {
                return Calc.HexToColor(result);
            } else if (hexplus.Length == 8 && hexplus.Substring(0, 2) == "00" && Regex.IsMatch(hexplus.Substring(2), "[^0-9a-f]")) //Optimized check to determine Regex matching for a hex number, marginally faster for a check where you dont need the end value.
              {
                return Color.Transparent;
            } else if (int.TryParse(hexplus, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out result)) {
                return AdvHexToColor(result);
            }
            return Color.Transparent;
        }

        public static Color AdvHexToColor(int hex) {
            Color result = default(Color);
            result.A = (byte) (hex >> 24);
            result.R = (byte) (hex >> 16);
            result.G = (byte) (hex >> 8);
            result.B = (byte) hex;
            return result;
        }

        public static string ColorToHex(Color c) {
            return c.R.ToString("X") + c.G.ToString("X") + c.B.ToString("X");
        }

        public static int mod(int x, int m) => (x % m + m) % m;
        public static float mod(float x, float m) => (x % m + m) % m;
        public static double mod(double x, double m) => (x % m + m) % m;
        public static long mod(long x, long m) => (x % m + m) % m;

        public static Vector2 mod(Vector2 x, Vector2 m) => new Vector2(mod(x.X, m.X), mod(x.Y, m.Y));


        public static void ParticleSourceApplier(ref ParticleType pt, string particleSource) {
            string[] sources = particleSource.Trim().Split(',');
            if (sources.Length == 0) { return; }
            if (sources.Length == 1) {
                pt.Source = GFX.Game[particleSource];
            } else {
                if (particleSource.Contains(":")) {
                    Chooser<MTexture> chooser = new Chooser<MTexture>();
                    foreach (string s in sources) {
                        string[] t = s.Split(':');
                        chooser.Add(GFX.Game[t[0].Trim()], float.Parse(t[1].Trim()));
                    }
                    pt.SourceChooser = chooser;
                } else {
                    Chooser<MTexture> chooser = new Chooser<MTexture>();
                    foreach (string s in sources)
                        chooser.Add(GFX.Game[s.Trim()], 1f);
                    pt.SourceChooser = chooser;
                }
            }
        }

        /// <summary>
        /// tries to get an easer from the Easer Dictionary
        /// </summary>
        public static bool TryGetEaser(string easeName, out Ease.Easer easer) {
            easer = null;
            if (!EaseHelper.ContainsKey(easeName)) {
                return false;
            }
            easer = EaseHelper[easeName];
            return true;
        }

        public static Player GetPlayer() {
            return Engine.Scene?.Tracker?.GetEntity<Player>();
        }
        //Thanks to coloursofnoise for this code.
        public static bool TryGetPlayer(out Player player) {
            player = Engine.Scene?.Tracker?.GetEntity<Player>();
            return player != null;
        }
        public static bool TryGetAlivePlayer(out Player player) {
            player = Engine.Scene?.Tracker?.GetEntity<Player>();
            return !(player?.Dead ?? true);
        }

        /// <summary>
        /// retrieves the number of on state values from any uint, in other words, the boolean 1s from the binary number
        /// </summary>
        /// <param name="u">number</param>
        /// <returns></returns>
        public static int Get1s(uint u) {
            uint uCount;

            uCount = (uint) (u - ((u >> 1) & 033333333333) - ((u >> 2) & 011111111111));
            return (int) ((uCount + (uCount >> 3)) & 030707070707) % 63;
        }

        public static Type GetType(string typeName, bool throwOnNotFound, bool store = true) {
            if (VivHelperModule.StoredTypesByName.TryGetValue(typeName, out Type value))
                return value;
            Type type = FakeAssembly.GetFakeEntryAssembly().GetType(typeName, throwOnNotFound); //bruh I been stupids
            if (type == null) { return null; } //if throwOnNotFound is true, then it will get here, otherwise it throws
            //At this point the type was found so we just add it to the StoredTypesByName (since this is significantly faster)
            if (store)
                VivHelperModule.StoredTypesByName.Add(typeName, type);
            return type;
        }
        public static bool TryGetType(string typeName, out Type type, bool store = true) {
            type = GetType(typeName, false, store);
            return type != null;
        }

        /// <summary>
        /// Appends Types to a Type list for setup for Meta-entities. This is coded as a comma separated string list, with exact type matching as regular text, and assignable type matching as *(assignableType)
        /// Exact type matching means that the Types need to be equal, in other words, you cannot say that the type Solid matches to type Platform, even though Solid can be evaluated as type Platform.
        /// </summary>
        /// <param name="TypeSet">The string you need to input, generally EntityData "Types" parameter</param>
        /// <param name="exactList">The list of types that need to match exactly, not assignable</param>
        /// <param name="assignableList">The list of types that need to match or extend from the given type</param>
        public static void AppendTypesToList(string TypeSet, ref List<Type> exactList, ref List<Type> assignableList, Type minimumAssignableSubset = null) {
            if (minimumAssignableSubset == null)
                minimumAssignableSubset = typeof(Entity);
            if (exactList == null)
                exactList = new List<Type>();
            if (assignableList == null)
                assignableList = new List<Type>();
            if (string.IsNullOrWhiteSpace(TypeSet)) {
                assignableList.Add(minimumAssignableSubset);
            } else {
                string[] strings = TypeSet.Split(',');
                foreach (string s in strings) {
                    if (s.StartsWith("*")) {
                        Type t = VivHelper.GetType(s.Substring(1), false);
                        if (t != null && t.IsAssignableFrom(minimumAssignableSubset)) {
                            assignableList.Add(t);
                        }
                    } else {
                        Type t = VivHelper.GetType(s, false);
                        if (t != null && t.IsAssignableFrom(minimumAssignableSubset)) {
                            exactList.Add(t);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Appends Types to a Type array for setup for Meta-entities. This is coded as a comma separated string list, with exact type matching as regular text, and assignable type matching as *(assignableType)
        /// Exact type matching means that the Types need to be equal, in other words, you cannot say that the type Solid matches to type Platform, even though Solid can be evaluated as type Platform.
        /// </summary>
        /// <param name="TypeSet">The string you need to input, generally EntityData "Types" parameter</param>
        /// <param name="exactList">The array of types that need to match exactly, not assignable</param>
        /// <param name="assignableList">The array of types that need to match or extend from the given type</param>
        public static void AppendTypesToArray(string TypeSet, ref Type[] exactList, ref Type[] assignableList) {
            List<Type>[] lists = new List<Type>[] { exactList?.ToList<Type>() ?? null, assignableList?.ToList<Type>() ?? null };
            AppendTypesToList(TypeSet, ref lists[0], ref lists[1]);
            exactList = lists[0].ToArray();
            assignableList = lists[1].ToArray();
            return;
        }

        public static Dictionary<string, Func<int>> IntParserEntityValues_Default(this Entity e) {
            if (e.Scene == null) { return null; }
            return new Dictionary<string, Func<int>>() { { "D", () => e.SceneAs<Level>().Session?.Inventory.Dashes ?? 1 }, { "Default", () => e.SceneAs<Level>().Session?.Inventory.Dashes ?? 1 } };
        }
        public static Dictionary<string, Func<float>> FloatParserEntityValues_Default(this Entity e) {
            if (e.Scene == null) { return null; }
            return new Dictionary<string, Func<float>>() { { "D", () => 110f }, { "Default", () => 110f } };
        }

        public static bool IntParserV1(string number, Dictionary<string, Func<int>> parseSpecialValues, out int value) {

            if (string.IsNullOrWhiteSpace(number)) {
                value = default(int);
                return false;
            }
            if (parseSpecialValues.ContainsKey(number)) {
                value = parseSpecialValues[number].Invoke();
                return true;
            }
            return IntParserV1(number, parseSpecialValues, false, out value);
        }

        private static bool IntParserV1(string number, Dictionary<string, Func<int>> parseSpecialValues, bool _, out int value) {
            if (string.IsNullOrEmpty(number)) {
                throw new Exception("Integer was empty.");
            }
            //+ refers to addition
            if (number.Contains("+")) {
                string[] q = number.Split('+');
                int p = 0;
                for (int s = 0; s < q.Length; s++) { if (!IntParserV1(q[s], parseSpecialValues, false, out int o)) { value = 0; return false; } p += o; }
                value = p;
                return true;
            }

            // * refers to multiplication
            if (number.Contains("*")) {
                string[] q = number.Split('*');
                int p = 1;
                for (int s = 0; s < q.Length; s++) { if (!IntParserV1(q[s], parseSpecialValues, false, out int o)) { value = 0; return false; } p *= o; }
                value = p;
                return true;
            }
            // / refers to division
            if (number.Contains("/")) {
                string[] q = number.Split('/');
                int p = 1;
                for (int s = 0; s < q.Length; s++) { if (!IntParserV1(q[s], parseSpecialValues, false, out int o)) { value = 0; return false; } if (o == 0) { value = 0; return false; } p /= o; }
                value = p;
                return true;
            }
            if (parseSpecialValues.ContainsKey(number.Trim())) { value = parseSpecialValues[number.Trim()].Invoke(); return true; }
            return int.TryParse(number.Trim(), out value);
        }

        public static bool FloatParserV1(string number, Dictionary<string, Func<float>> parseSpecialValues, out float value) {

            if (string.IsNullOrWhiteSpace(number)) {
                value = default(int);
                return false;
            }
            if (parseSpecialValues.ContainsKey(number)) {
                value = parseSpecialValues[number].Invoke();
                return true;
            }
            return FloatParserV1(number, parseSpecialValues, false, out value);
        }

        private static bool FloatParserV1(string number, Dictionary<string, Func<float>> parseSpecialValues, bool _, out float value) {
            if (string.IsNullOrEmpty(number)) {
                throw new Exception("Floating-point number was empty.");
            }
            //+ refers to addition
            if (number.Contains("+")) {
                string[] q = number.Split('+');
                float p = 0;
                for (int s = 0; s < q.Length; s++) { if (!FloatParserV1(q[s], parseSpecialValues, false, out float o)) { value = 0; return false; } p += o; }
                value = p;
                return true;
            }
            // * refers to multiplication
            if (number.Contains("*")) {
                string[] q = number.Split('*');
                float p = 1;
                for (int s = 0; s < q.Length; s++) { if (!FloatParserV1(q[s], parseSpecialValues, false, out float o)) { value = 0; return false; } p *= o; }
                value = p;
                return true;
            }
            // / refers to division
            if (number.Contains("/")) {
                string[] q = number.Split('/');
                float p = 1;
                for (int s = 0; s < q.Length; s++) { if (!FloatParserV1(q[s], parseSpecialValues, false, out float o)) { value = 0; return false; } if (o == 0) { value = 0; return false; } p /= o; }
                value = p;
                return true;
            }
            //if (number.Trim() == "U") { k = UseNumber; return true; }
            //if (number.Trim() == "u") { k = count; return true; }
            if (parseSpecialValues.ContainsKey(number.Trim())) { value = parseSpecialValues[number.Trim()].Invoke(); return true; }
            return float.TryParse(number.Trim(), out value);
        }




        private static FieldInfo player_triggersInside = typeof(Player).GetField("triggersInside", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void PlayerTriggerCheck(Player player, Trigger trigger) {
            if (!trigger.Triggered) {
                trigger.Triggered = true;
                ((HashSet<Trigger>) player_triggersInside.GetValue(player)).Add(trigger);
                trigger.OnEnter(player);
            }
            trigger.OnStay(player);
        }

        public static bool DefineSetState(string input, out int output) {
            if (int.TryParse(input, out output)) {
                if (output < 0)
                    return false;
                if (output > 25)
                    throw new InvalidPropertyException("You tried to put in a custom state value over 25. I recommend retrieving the classname or using the default values provided in the dropdown.");

            } else {
                List<string> subset = input.Split('.').ToList();
                if (subset.Count < 2)
                    throw new InvalidPropertyException("You input an invalid custom state.");
                string fieldName = subset.Last();
                subset.RemoveAt(subset.Count - 1);
                string temp1 = string.Join(".", subset); //This is everything but the fieldname, which should be the classname path, Array Resizing was slower in this case.
                if (!VivHelper.TryGetType(temp1, out Type type))
                    throw new InvalidPropertyException("The custom state class path was invalid!");
                FieldInfo field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic); //If its nonpublic this would make me angy.
                if (field == null)
                    throw new InvalidPropertyException("The custom state field name was invalid!");
                if (field.FieldType != typeof(int))
                    throw new InvalidPropertyException("The custom state field was found but not shown to be a valid field (it is not an integer)");
                output = (int) field.GetValue(null);
                return true;
            }
            return false;
        }

        public static bool CompareEntityIDs(EntityID a, EntityID b) {
            return a.Level == b.Level && a.ID == b.ID;
        }
        public static List<Entity> getListOfEntities(this EntityList self) => _RetrieveEntityList_entities(self);

        //Code from ColoursOfNoise
        public static bool TryGetModule(EverestModuleMetadata meta, out EverestModule module) {
            foreach (EverestModule other in Everest.Modules) {
                EverestModuleMetadata otherData = other.Metadata;
                if (otherData.Name != meta.Name)
                    continue;

                Version version = otherData.Version;
                if (Everest.Loader.VersionSatisfiesDependency(meta.Version, version)) {
                    module = other;
                    return true;
                }
            }

            module = null;
            return false;
        }

        public static Color BlendColors(Color a, Color b) {
            return new Color((a.R / 255f) * (b.R / 255f), (a.G / 255f) * (b.G / 255f), (a.B / 255f) * (b.B / 255f), (a.A / 255f) * (b.A / 255f));
        }

        public static List<Color> ColorsFromString(string str, char sep = ',') {
            List<Color> l = new List<Color>();
            foreach (string s in str.Split(sep)) {
                l.Add(ColorFix(s.Trim()));
            }
            return l;
        }

        internal static FieldInfo autotiler_lookup = typeof(Autotiler).GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static MethodInfo Ignore = typeof(Autotiler).GetNestedType("TerrainType", BindingFlags.NonPublic).GetMethod("Ignore", BindingFlags.Public | BindingFlags.Instance);
        public static bool TilesetConnect(char Base, char Compare) {
            var lookup = (System.Collections.IDictionary) autotiler_lookup.GetValue(GFX.FGAutotiler);
            var terrainType = lookup[Base];
            return !(bool) Ignore.Invoke(terrainType, new object[] { Compare });
        }

        private static MethodInfo Commands_UpdateClosed = typeof(Monocle.Commands).GetMethod("UpdateClosed", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void CommandOverride(string input) {
            if (Engine.Commands.Open) {
                Engine.Commands.Log(input);
            } else {
                Commands_UpdateClosed?.Invoke(Engine.Commands, new object[] { });
                Engine.Commands.Log(input + (Celeste.Celeste.PlayMode != Celeste.Celeste.PlayModes.Debug ? "Type q and press [ENTER] to exit." : ""));
            }
        }

        public static void ForceEntitySetScene(Entity self) => _forceEntitySetScene(self);
        public static Action<Entity> _forceEntitySetScene => ForceEntitySetSceneIL();


        #region ILMethods

        private static Action<Entity> ForceEntitySetSceneIL() {
            string methodName = "VivHelper.ForceEntitySetScene";
            DynamicMethodDefinition method = new DynamicMethodDefinition(methodName, typeof(void), new Type[] { typeof(Entity) });
            var gen = method.GetILProcessor();

            MethodInfo entitySetScene = typeof(Entity).GetProperty("Scene", BindingFlags.Public | BindingFlags.Instance).GetSetMethod();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, entitySetScene);
            return (Action<Entity>) method.Generate().CreateDelegate(typeof(Action<Entity>));
        }

        private static Func<EntityList, List<Entity>> _RetrieveEntityList_entities = EntityList_EntitiesIL();

        private static Func<EntityList, List<Entity>> EntityList_EntitiesIL() {
            string methodName = "VivHelper._RetrieveEntityList_entities";
            DynamicMethodDefinition method = new DynamicMethodDefinition(methodName, typeof(List<Entity>), new Type[] { typeof(EntityList) });
            var gen = method.GetILProcessor();

            FieldInfo EntityList_entities = typeof(EntityList).GetField("entities", BindingFlags.NonPublic | BindingFlags.Instance);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, EntityList_entities);
            gen.Emit(OpCodes.Ret);
            return (Func<EntityList, List<Entity>>) method.Generate().CreateDelegate(typeof(Func<EntityList, List<Entity>>));
        }

        private static Func<Sprite, Sprite> _CloneSprite = CloneSpriteIL();
        private static Func<Sprite, Sprite> CloneSpriteIL() {
            string methodName = "VivHelper._CloneSprite";

            DynamicMethodDefinition method = new DynamicMethodDefinition(methodName, typeof(Sprite), new Type[] { typeof(Sprite) });
            var gen = method.GetILProcessor();

            MethodInfo cloneSprite = typeof(Sprite).GetMethod("CreateClone", BindingFlags.NonPublic | BindingFlags.Instance);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, cloneSprite);
            gen.Emit(OpCodes.Ret);
            return (Func<Sprite, Sprite>) method.Generate().CreateDelegate(typeof(Func<Sprite, Sprite>));
        }

        /// Code written by JaThePlayer

        private static Func<Scene, Vector2, Color> _getHue = GetHueIL();
        private static Func<Vector2, Color> _getHueNoScene = GetHueNoSceneIL();

        private static Func<Scene, Vector2, Color> GetHueIL() {
            string methodName = "VivHelper._getHue";

            DynamicMethodDefinition method = new DynamicMethodDefinition(methodName, typeof(Color), new[] { typeof(Scene), typeof(Vector2) });

            var gen = method.GetILProcessor();

            FieldInfo CrystalSpinner = typeof(VivHelperModule).GetField(nameof(VivHelperModule.crystalSpinner), BindingFlags.Public | BindingFlags.Static);
            // VivHelperModule.crystalSpinner.Scene = scene;
            gen.Emit(OpCodes.Ldsfld, CrystalSpinner);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, typeof(Entity).GetProperty("Scene").GetSetMethod(true));

            // return VivHelper.crystalSpinner.GetHue(position);
            gen.Emit(OpCodes.Ldsfld, CrystalSpinner);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, typeof(CrystalStaticSpinner).GetMethod("GetHue", BindingFlags.NonPublic | BindingFlags.Instance));
            gen.Emit(OpCodes.Ret);

            return (Func<Scene, Vector2, Color>) method.Generate().CreateDelegate(typeof(Func<Scene, Vector2, Color>));
        }
        private static Func<Vector2, Color> GetHueNoSceneIL() {
            string methodName = "VivHelper._getHueNoScene";

            DynamicMethodDefinition method = new DynamicMethodDefinition(methodName, typeof(Color), new[] { typeof(Vector2) });

            var gen = method.GetILProcessor();
            FieldInfo CrystalSpinner = typeof(VivHelperModule).GetField(nameof(VivHelperModule.crystalSpinner), BindingFlags.Public | BindingFlags.Static);
            // return ColorHelper.crystalSpinner.GetHue(position);
            gen.Emit(OpCodes.Ldsfld, CrystalSpinner);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, typeof(CrystalStaticSpinner).GetMethod("GetHue", BindingFlags.NonPublic | BindingFlags.Instance));
            gen.Emit(OpCodes.Ret);

            return (Func<Vector2, Color>) method.Generate().CreateDelegate(typeof(Func<Vector2, Color>));
        }
        #endregion

        //CustomDashState magic
        public static FastReflectionDelegate player_WallJumpCheck;
        public static FastReflectionDelegate player_DashCorrectCheck;
        public static FastReflectionDelegate player_DreamDashCheck;
        public static FastReflectionDelegate player_SuperJump;
        public static FastReflectionDelegate player_SuperWallJump;
        public static FastReflectionDelegate player_ClimbJump;
        public static FastReflectionDelegate player_WallJump;
        public static FastReflectionDelegate player_Pickup;
        public static FastReflectionDelegate player_DustParticleFromSurfaceIndex;

        public static bool RectToRect(Rectangle a, Rectangle b) {
            if (a.Right > b.Left && a.Bottom > b.Top && a.Left < b.Right) {
                return a.Top < b.Bottom;
            }
            return false;
        }

        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Render()")]
        public static void Entity_Render(Entity entity) {
            Console.WriteLine("link to Entity::Render failed");
        }
    }
}
