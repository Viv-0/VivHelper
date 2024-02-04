using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace VivHelper {
    public static partial class VivHelper {

        public static Color Alpha75 = new Color(0.75f, 0.75f, 0.75f, 0.75f);
        public static Color Alpha50 = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public static Color Alpha25 = new Color(0.25f, 0.25f, 0.25f, 0.25f);

        public static Dictionary<string, Color> colorHelper;
        public static Color GetHue(Scene scene, Vector2 pos) {
            if (scene == null) {
                Logger.Log("VivHelper","Scene supplied was null!");
                return Color.White;
            }
            if (VivHelperModule.crystalSpinner == null) {
                VivHelperModule.crystalSpinner = new CrystalStaticSpinner(Vector2.Zero, false, CrystalColor.Rainbow);
            } else if (VivHelperModule.crystalSpinner.Scene == scene)
                return _getHueNoScene(pos);
            return _getHue(scene, pos);
        }

        public delegate bool PrependColorFunction(string value, out Color? color);
        public delegate bool AppendedColorFunction(ref Color? color);

        public static Color? GetColor(EntityData data, string legacyColor, string newColor, bool allowNull, Color? defaultColor = null, PrependColorFunction prependedColorFunction = null, AppendedColorFunction appendedColorFunction = null) {
            object value;
            Color? ret = null;
            if(data.Values.TryGetValue(newColor, out value) && value is string parse) {
                if (!(prependedColorFunction?.Invoke(parse, out ret) ?? false)) {
                    ret = NewColorFunction(parse);
                    if (!allowNull && ret == null)
                        ret = defaultColor ?? Color.White;
                }
            } else if(data.Values.TryGetValue(legacyColor, out value) && value is string parse2) {
                if (!(prependedColorFunction?.Invoke(parse2, out ret) ?? false)) {
                    ret = allowNull ? OldColorFunctionWithNull(parse2) : OldColorFunction(parse2);
                }
            }
            appendedColorFunction?.Invoke(ref ret);
            return ret;
        }

        // Uses the RGBA format
        public static Color? NewColorFunction(string hex) {
            if (string.IsNullOrWhiteSpace(hex))
                return null;
            if (colorHelper.ContainsKey(hex.ToLower()))
                return colorHelper[hex.ToLower()];
            // Manual HexToColor
            int num = 0;
            bool nonPreMult = true;
            if (hex.StartsWith("#")) num = 1;
            else if (hex.StartsWith("0x")) num = 2;
            else if (hex.StartsWith("pm")) { num = 2; nonPreMult = false; }
            byte HexToByte(char c) { int res = "0123456789ABCDEF".IndexOf(char.ToUpper(c)); if (res < 0) throw new IndexOutOfRangeException(); return (byte) res; }
            try {
                switch (hex.Length - num) {
                    case 6:
                        return new Color(HexToByte(hex[num++]) * 16 + HexToByte(hex[num++]),
                                         HexToByte(hex[num++]) * 16 + HexToByte(hex[num++]),
                                         HexToByte(hex[num++]) * 16 + HexToByte(hex[num++]));
                    case 8:
                        int r = HexToByte(hex[num++]) * 16 + HexToByte(hex[num++]);
                        int g = HexToByte(hex[num++]) * 16 + HexToByte(hex[num++]);
                        int b = HexToByte(hex[num++]) * 16 + HexToByte(hex[num++]);
                        int a = HexToByte(hex[num++]) * 16 + HexToByte(hex[num++]);
                        if (nonPreMult) {
                            float alpha = a / 255f;
                            return new Color(alpha * (float) r / 255f, alpha * (float) g / 255f, alpha * (float) b / 255f, alpha);
                        } else
                            return new Color(r, g, b, a);
                }
            } catch (IndexOutOfRangeException i) { }
            return null;
        }

        #region Legacy Color Functions - do not use

        [Obsolete]
        public static Color? OldColorFunctionWithNull(string s) {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            if (s == "Transparent" || s.Length == 8 && s.Substring(0, 2) == "00" && int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int _)) { return Color.Transparent; } //Do this check first because we check for any other case of transparency, which is equivalent to no other valid case in this case

            var c = OldColorFunction(s);
            if (c == Color.Transparent)
                return null;
            return c;
        }

        [Obsolete]
        public static Color OldColorFunction(string s) {
            if (colorHelper.ContainsKey(s.ToLower()))
                return colorHelper[s.ToLower()];
            return OldHexToColor(s);
        }

        [Obsolete]
        public static Color OldColorFunction(string s, float alpha) {
            if (colorHelper.ContainsKey(s.ToLower()))
                return colorHelper[s.ToLower()];
            return Extensions.ColorCopy(OldHexToColor(s), alpha);
        }

        [Obsolete]
        public static Color OldHexToColor(string hex) {
            string hexplus = hex.Trim('#');
            if (hexplus.StartsWith("0x"))
                hexplus = hexplus.Substring(2);
            uint result;
            if (hexplus.Length == 6 && uint.TryParse(hexplus, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out result)) {
                return Calc.HexToColor((int) result);
            } else if (hexplus.Length == 8 && hexplus.Substring(0, 2) == "00" && Regex.IsMatch(hexplus.Substring(2), "[^0-9a-f]")) //Optimized check to determine Regex matching for a hex number, marginally faster for a check where you dont need the end value.
              {
                return Color.Transparent;
            } else if (uint.TryParse(hexplus, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out result)) {
                return UintToColor(result);
            }
            return Color.Transparent;
        }
        public static Color UintToColor(uint hex) {
            Color result = default(Color);
            result.PackedValue = hex;
            return result;
        }
        #endregion
        public static string ColorToHex(Color c, bool rgba = false) {
            string t = c.R.ToString("X") + c.G.ToString("X") + c.B.ToString("X");
            return rgba ? t + c.A.ToString("X") : c.A.ToString("X") + t;
        }

        public static Color BlendColors(Color a, Color b) {
            return new Color((a.R / 255f) * (b.R / 255f), (a.G / 255f) * (b.G / 255f), (a.B / 255f) * (b.B / 255f), (a.A / 255f) * (b.A / 255f));
        }

        public static List<Color> OldColorsFromString(string str, char sep = ',') {
            List<Color> l = new List<Color>();
            foreach (string s in str.Split(sep)) {
                l.Add(OldColorFunction(s.Trim()));
            }
            return l;
        }

        internal static Vector4[] ColorArrToVec4Arr(Color[] colors) {
            Vector4[] ret = new Vector4[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                ret[i] = colors[i].ToVector4();
            return ret;
        }
        internal static void ColorArrToVec4Arr(Color[] colors, ref Vector4[] to) {
            for (int i = 0; i < colors.Length; i++)
                to[i] = colors[i].ToVector4();
        }


        #region IL Bullshit

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
    }
}
