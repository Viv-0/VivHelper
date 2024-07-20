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

        public static Color GetSimpleColor(string color, bool allowXNA = true) {
            if(allowXNA && colorHelper.TryGetValue(color, out Color result)) { return result; }
            return Calc.HexToColor(color);
        }

        #region Legacy Color Functions - do not use

        [Obsolete]
        public static Color? OldColorFunctionWithNull(string s, bool ignoreXNA = false) {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            if (s == "Transparent" || s.Length == 8 && s.Substring(0, 2) == "00" && int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int _)) { return Color.Transparent; } //Do this check first because we check for any other case of transparency, which is equivalent to no other valid case in this case

            Color c = ignoreXNA ? OldHexToColor(s) : OldColorFunction(s);
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

        [Obsolete]
        public static List<Color> OldColorsFromString(string str, char sep = ',') {
            List<Color> l = new List<Color>();
            foreach (string s in str.Split(sep)) {
                l.Add(OldColorFunction(s.Trim()));
            }
            return l;
        }

        public static Color UintToColor(uint hex) {
            Color result = default(Color);
            result.PackedValue = hex;
            return result;
        }
        #endregion

        public delegate bool ColorOverrides(string value, out Color? color);

        [Flags]
        public enum GetColorParams {
            None = 0,
            AllowNull = 1,
            ImplyEmptyAsTransparent = 2,
            DisallowXNAColors = 4
        }

#pragma warning disable CS0612
        public static Color? GetColorWithFix(EntityData data, string legacyColor, string newColor, GetColorParams old, GetColorParams @new, Color? defaultColor = null, ColorOverrides specialColorNames = null) {
            object value;
            Color? ret = null;
            if(data.Values.TryGetValue(newColor, out value) && value is string parse) {
                if (@new.HasFlag(GetColorParams.ImplyEmptyAsTransparent) && string.IsNullOrWhiteSpace(parse)) {
                    ret = Color.Transparent;
                } else if (!(specialColorNames?.Invoke(parse, out ret) ?? false)) {
                    ret = NewColorFunction(parse, @new.HasFlag(GetColorParams.DisallowXNAColors));
                    if (!@new.HasFlag(GetColorParams.AllowNull) && ret == null)
                        ret = defaultColor ?? Color.White;
                }
            } else if(data.Values.TryGetValue(legacyColor, out value) && value is string parse2) {
                if (old.HasFlag(GetColorParams.ImplyEmptyAsTransparent) && string.IsNullOrWhiteSpace(parse2)) {
                    ret = Color.Transparent;
                } else if (!(specialColorNames?.Invoke(parse2, out ret) ?? false)) {
                    Logger.Log(LogLevel.Verbose, "VivHelper-Color", "Old Color used with " + data.Name + " @ room/pos " + data.Level.Name + ": " + data.Position.ToString());
                    ret = old.HasFlag(GetColorParams.AllowNull) ? OldColorFunctionWithNull(parse2, old.HasFlag(GetColorParams.DisallowXNAColors)) :
                            (old.HasFlag(GetColorParams.DisallowXNAColors) ? OldHexToColor(parse2) : OldColorFunction(parse2));
                }
            } else if (!@new.HasFlag(GetColorParams.AllowNull) && ret == null)
                ret = defaultColor ?? Color.White;
            return ret;
        }

        public static Color? GetColor(string str, GetColorParams @params, Color? defaultColor = null, ColorOverrides specialColorNames = null) {
            Color? ret = null;
            if (@params.HasFlag(GetColorParams.ImplyEmptyAsTransparent) && str != null && string.IsNullOrWhiteSpace(str)) {
                ret = Color.Transparent;
            } else if (!(specialColorNames?.Invoke(str, out ret) ?? false)) {
                ret = NewColorFunction(str, @params.HasFlag(GetColorParams.DisallowXNAColors));
                if (!@params.HasFlag(GetColorParams.AllowNull) && ret == null)
                    ret = defaultColor ?? Color.White;
            }
            return ret;
        }
#pragma warning restore CS0612

        // Uses the RGBA format
        public static Color? NewColorFunction(string hex, bool disableXNAColors) {
            if (string.IsNullOrWhiteSpace(hex))
                return null;
            if (!disableXNAColors && colorHelper.ContainsKey(hex.ToLower()))
                return colorHelper[hex.ToLower()];
            // Manual HexToColor
            int num = 0;
            bool nonPreMult = true;
            if (hex.StartsWith("#")) num = 1;
            else if (hex.StartsWith("0x")) num = 2;
            else if (hex.StartsWith("pm")) { num = 2; nonPreMult = false; }
            byte HexToByte(char c) { int res = "0123456789ABCDEF".IndexOf(char.ToUpperInvariant(c)); if (res < 0) throw new IndexOutOfRangeException(); return (byte) res; }
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
            } catch (IndexOutOfRangeException) { return null; }
            return null;
        }

        public static List<Color?> NewColorsFromString(string str, char sep = ',', GetColorParams colorParameters = GetColorParams.AllowNull, bool ignoreInvalidColors = false, ColorOverrides specialColorNames = null) {
            List<Color?> l = new List<Color?>();
            foreach (string s in str.Split(sep)) {
                Color? c = null;
                if (!(specialColorNames?.Invoke(str, out c) ?? false))
                    c = NewColorFunction(s.Trim(), colorParameters.HasFlag(GetColorParams.DisallowXNAColors));
                if (c == null) {
                    if (ignoreInvalidColors)
                        continue;
                    else if(!colorParameters.HasFlag(GetColorParams.AllowNull))
                        throw new Exception($"no Color matches the string {s.Trim()}, found in the string \"{str}\"");
                }
                l.Add(c);
            }
            return l;
        }

        public static string ColorToHex(Color c, bool rgba = true) {
            string t = c.R.ToString("X") + c.G.ToString("X") + c.B.ToString("X");
            return rgba ? t + c.A.ToString("X") : c.A.ToString("X") + t;
        }

        public static Color BlendColors(Color a, Color b) => ColorLerp(a, b, 0.5f);

        public static Color ColorLerp(Color a, Color b, float t) => HSVAtoRGBA(Vector4.Lerp(RGBAtoHSVA(a), RGBAtoHSVA(b), t));

        public static float GetHue(this Color c, int min = -1, int max = -1) {
            if (c.R == c.G && c.G == c.B)
                return 0f;
            if(min == -1 && max == -1) {
                if (c.R > c.G) {
                    max = c.R;
                    min = c.G;
                } else {
                    max = c.G;
                    min = c.R;
                }
                if (c.B > max) {
                    max = c.B;
                } else if (c.B < min) {
                    min = c.B;
                }
            }
            float delta = max - min;
            float hue;

            if (c.R == max)
                hue = (c.G - c.B) / delta;
            else if (c.G == max)
                hue = (c.B - c.R) / delta + 2f;
            else
                hue = (c.R - c.G) / delta + 4f;

            hue *= 60f;
            if (hue < 0f)
                hue += 360f;

            return hue;
        }

        public static Vector4 RGBAtoHSVA(Color color) {
            float max = Calc.Max(color.R, color.G, color.B);
            float min = Calc.Min(color.R, color.G, color.B);
            float hue = color.GetHue((int)max, (int)min);
            float saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            float value = max / 255f;
            return new Vector4(hue, saturation, value, color.A);
        }
        public static Color HSVAtoRGBA(Vector4 hsva) {
            int hi = Convert.ToInt32(MathF.Floor(hsva.X / 60)) % 6;
            float f = hsva.X / 60 - MathF.Floor(hsva.X / 60);

            hsva.Z *= 255;
            int v = Convert.ToInt32(hsva.Z);
            int p = Convert.ToInt32(hsva.Z * (1 - hsva.Y));
            int q = Convert.ToInt32(hsva.Z * (1 - f * hsva.Y));
            int t = Convert.ToInt32(hsva.Z * (1 - (1 - f) * hsva.Y));

            if (hi == 0)
                return new Color(v, t, p, hsva.W);
            else if (hi == 1)
                return new Color(q, v, p, hsva.W);
            else if (hi == 2)
                return new Color(p, v, t, hsva.W);
            else if (hi == 3)
                return new Color(p, q, v, hsva.W);
            else if (hi == 4)
                return new Color(t, p, v, hsva.W);
            else
                return new Color(v, p, q, hsva.W);
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
