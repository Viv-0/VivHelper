using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using System.Reflection;
using Celeste.Mod;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace VivHelper.Entities.Spinner2 {
    [Tracked]
    public class SpinnerGrouper : Grouper {

        private struct Target {
            public bool rendered;
            public VirtualRenderTarget target;
        }

        public static void SetTargets() { targets = new Target[1]; targets[0] = new Target() { rendered = false, target = VirtualContent.CreateRenderTarget("VH_Spinner", 320, 180) }; }
        private static Target[] targets;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        public struct MHHRainbowInfo {
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
            public Vector4[] Colors;
            public float GradientSize, GradientSpeed;
            public bool LoopColors;
            public Vector2 Center;

            public static MHHRainbowInfo Create(string colors, float gradientSize, float gradientSpeed, bool loopColors, Vector2 center) => new MHHRainbowInfo(colors, gradientSize, gradientSpeed, loopColors, center);
            public MHHRainbowInfo(string colors, float gradientSize, float gradientSpeed, bool loopColors, Vector2 center) {
                Colors = new Vector4[16];
                VivHelper.ColorArrToVec4Arr(VivHelper.ColorsFromString(colors).ToArray(), ref Colors);
                GradientSize = gradientSize;
                GradientSpeed = gradientSpeed;
                LoopColors = loopColors;
                Center = center;
            }
            public MHHRainbowInfo(Color[] colors, float gradientSize, float gradientSpeed, bool loopColors, Vector2 center) {
                Colors = new Vector4[16];
                VivHelper.ColorArrToVec4Arr(colors, ref Colors);
                GradientSize = gradientSize;
                GradientSpeed = gradientSpeed;
                LoopColors = loopColors;
                Center = center;
            }

            public void SetValues(Effect eff) {
                if (Colors == null || Colors.Length == 0) {
                    eff.Parameters["ColorsLength"].SetValue(0);
                    return;
                }
                eff.Parameters["center"].SetValue(Center);
                if (Colors != null) {
                    eff.Parameters["Colors"].SetValue(Colors);
                }
                eff.Parameters["ColorsLength"].SetValue(Colors?.Length ?? 0);
                Console.WriteLine(Colors?.Length ?? 0);
                eff.Parameters["loop"].SetValue(LoopColors);
                eff.Parameters["gradientSize"].SetValue(GradientSize);
                eff.Parameters["gradientSpeed"].SetValue(GradientSpeed);
            }

            public override bool Equals(object obj) {
                if (obj is MHHRainbowInfo m) {
                    if ((Colors == null || Colors.Length == 0) && (m.Colors == null || m.Colors.Length == 0)) {
                        return true;
                    }
                    if ((m.Colors == null && Colors != null) || (Colors == null && m.Colors != null))
                        return false;
                    return m.Colors.SequenceEqual(Colors) &&
                           m.LoopColors == LoopColors &&
                           m.GradientSize == GradientSize &&
                           m.GradientSpeed == GradientSpeed &&
                           m.Center == Center;

                }
                return false;
            }

            public override string ToString() {
                return $"Colors: { (Colors == null ? "null" : string.Join(",", Colors)) } GradSize: {GradientSize} GradSpeed: {GradientSpeed} Loop: {LoopColors} Center: {Center}";
            }
        }

        public static MHHRainbowInfo defaultRainbowInfo = new MHHRainbowInfo();
        public static Type mhhRainbowColorController, scRainbowColorController;

        public static MHHRainbowInfo currentRainbowInfo { get { return VivHelperModule.Session.currentRainbowInfo; } set { VivHelperModule.Session.currentRainbowInfo = value; } }


        internal List<int>[] DepthTargets; //This is effectively static since there will only be one per map
        private Effect effect;
        public SpinnerGrouper() : base() {
            Depth = int.MaxValue;
            DepthTargets = new List<int>[1] { new List<int> { -8500, -11500 } };
            effect = Shaders.GetEffect("VH_spinner");
            Add(new BeforeRenderHook(OnBeforeRender));
        }

        public override void RenderAtDepth(int depth, bool prevRenderOverride = false) {
            try {
                int j = -1;
                for (int i = 0; i < targets.Length; i++) {
                    if (DepthTargets[i].Contains(depth)) {
                        Target t = targets[i];
                        if (!t.rendered) {
                            t.rendered = true;
                            Draw.SpriteBatch.Draw(t.target, SceneAs<Level>().Camera.Position, Color.White);
                            j = i;
                        }
                        break;
                    }
                }
                if (j == -1) {
                    base.RenderAtDepth(depth);
                }
            } catch {
                base.RenderAtDepth(depth);
            }
        }

        public override void Update() {
            Effect effect = Shaders.GetEffect("VH_spinner");
            effect.Parameters["Dimensions"].SetValue(new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height));
            effect.Parameters["WindowScale"].SetValue(Settings.Instance.WindowScale);
            effect.Parameters["Time"].SetValue(Scene.TimeActive);
            //dimension set in awake - we are currently assuming noone is zooming out the camera.
            if (mhhRainbowColorController == null) {
                effect.Parameters["center"].SetValue(Vector2.Zero);
                return;
            }
            MHHRainbowInfo prevRainbowInfo = currentRainbowInfo; //pass-by-value
            Entity e = null;
            if (Scene.Tracker.TryGetEntity(mhhRainbowColorController, out e)) {
                DynamicData d = DynamicData.For(e);
                string _flag = d.Get<string>("flag");
                bool flag = string.IsNullOrEmpty(_flag) || !SceneAs<Level>().Session.GetFlag(_flag);
                currentRainbowInfo = new MHHRainbowInfo() {
                    Colors = VivHelper.ColorArrToVec4Arr(SelectFromState(d.Get<Tuple<Color[], Color[]>>("colors"), flag)),
                    GradientSize = SelectFromState(d.Get<Tuple<float, float>>("gradientSize"), flag),
                    GradientSpeed = SelectFromState(d.Get<Tuple<float, float>>("gradientSpeed"), flag),
                    LoopColors = SelectFromState(d.Get<Tuple<bool, bool>>("loopColors"), flag),
                    Center = SelectFromState(d.Get<Tuple<Vector2, Vector2>>("center"), flag)
                };
            } else
                currentRainbowInfo = RainbowFromMHHSession(Scene as Level);
            effect.Parameters["CamPos"].SetValue((Scene as Level).Camera.Position);
            if (!currentRainbowInfo.Equals(prevRainbowInfo))
                currentRainbowInfo.SetValues(effect);
            VivHelperModule.Session.ResolveGradientMapDifferential(); // Handles management of the GradientMap
        }

        public void OnBeforeRender() {
            for (int i = 0; i < targets.Length; i++) {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(targets[i].target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Engine.Graphics.GraphicsDevice.Textures[1] = VivHelperModule.Session.gradientMap;
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, Shaders.GetEffect("VH_spinner"), SceneAs<Level>().Camera.Matrix * Matrix.CreateScale(GameplayBuffers.Gameplay.Width / 320));
                foreach (var depth in DepthTargets[i]) {
                    if (RenderSet.TryGetValue(depth, out RenderingSet a)) {
                        a.designatedRenderer = Grouper.emptyAtom;
                        foreach (Atom atom in a.set) {
                            atom.RenderAtom();
                        }
                    }
                }
                Draw.SpriteBatch.End();
                Engine.Graphics.GraphicsDevice.Textures[1] = null;
                targets[i].rendered = false;
            }
        }

        internal static T SelectFromState<T>(Tuple<T, T> t, bool b) => b ? t.Item1 : t.Item2;


        /// Output color:
        /// R,G,B,A
        /// 0bRRRRSSSS, 0bGGGGGGGG, 0bBBBBBBBB, 0bCCCCDDDD
        /// =
        /// c1: 0bRRRRGG|GGGGGG ; c2: 0bSSSSBB|BBBBBB           ; c3: 0b CCCC|DDDD
        ///     c1X      c1Y          c2X      c2Y              ;      alpha1|alpha2
        /// X,Y = position on texture for given color           ; lerps between the two colors
        internal static Color GetDrawColorForGradient(Color a, Color b) {
            Vector3 aVec = a.ToVector3();
            Vector3 bVec = b.ToVector3();
            int _a = VivHelperModule.Session.GetOrAddColorToGradientMap(aVec);
            int _b = VivHelperModule.Session.GetOrAddColorToGradientMap(bVec);

            int G = _a % 256; //Yes, this will optimize to the & statement on every computer
            int B = _b % 256; //Yes, this will optimize to the & statement

            int R_a = _a / 256;
            int R_b = _b / 256;
            int R = R_b + 16 * R_a; //0b|bbbbaaaa

            int A = (a.A / 16) + 16 * (b.A / 16); // in parentheses = truncation

            return new Color(R, G, B, A);
        }

        public static MHHRainbowInfo RainbowFromMHHSession(Level level) {
            return _RainbowInfoFromMHHSession.Invoke(level);
        }


        #region Cursed IL black magic -- enter at own risk

        private static Entity _1 = null;

        //IL blackmagic, do not attempt to make this yourself if you're trying to code-reference. It is very cursed.
        private static Func<Level, MHHRainbowInfo> _RainbowInfoFromMHHSession;

        public static MHHRainbowInfo __RainbowInfoFromMHHSession(Level level) {
            if (_1 == null) {
                return defaultRainbowInfo;

            }
            string flag = "retriever1";
            if (level.Session.GetFlag(flag)) {
                return MHHRainbowInfo.Create("retriever2", 0f, 0f, false, Vector2.Zero);
            }
            return MHHRainbowInfo.Create("retriever3", 0f, 0f, false, Vector2.Zero);
        }

        private static string[] propsInOrder = new string[] { "Colors", "GradientSize", "GradientSpeed", "LoopColors", "Center" };

        // C# equivalent source
        /// public static MHHRainbowInfo RainbowInfoFromMHHSession(Level level) {
        ///     var a = VivHelperModule.mhhModule.Session.RainbowSpinnerCurrentColors;
        ///     if(a == null) {
        ///         
        ///     }
        ///     if(level.Session.GetFlag(a.Flag)) {
        ///         return MHHRainbowInfo.Create(a.ColorsWithFlag, a.GradientSizeWithFlag, a.GradientSpeedWithFlag, a.LoopColorsWithFlag, a.CenterWithFlag);
        ///     }
        ///     return MHHRainbowInfo.Create(a.Colors, a.GradientSize, a.GradientSpeed, a.LoopColors, a.Center);
        /// }
        internal static void CreateRIFMS(EverestModule mhhModule) {

            DynamicMethodDefinition method = new DynamicMethodDefinition(typeof(SpinnerGrouper).GetMethod("__RainbowInfoFromMHHSession", BindingFlags.Static | BindingFlags.Public));

            Type mhhModuleType = mhhModule.GetType();
            Type SessionType = mhhModule.SessionType;
            Type RainbowSpinnerColorState = SessionType.GetNestedType("RainbowSpinnerColorState");
            if (RainbowSpinnerColorState == null)
                throw new Exception("RainbowSpinnerColorState is null");

            ILProcessor gen = method.GetILProcessor();
            method.Definition.Body.Variables.Add(new VariableDefinition(gen.Import(RainbowSpinnerColorState)));
            int rainbowspinnerIndex = method.Definition.Body.Variables.Count - 1;

            Dictionary<string, MethodInfo> rainbowspinnercolorstate_Props = new Dictionary<string, MethodInfo>();
            foreach (PropertyInfo prop in RainbowSpinnerColorState.GetProperties()) {
                rainbowspinnercolorstate_Props[prop.Name] = prop.GetGetMethod();
            }

            MethodInfo m_mhhModule_getSession = mhhModuleType.GetProperty("Session").GetGetMethod();
            MethodInfo m_mhhModuleSession_getRainbowSpinnerCurrentColors = SessionType.GetProperty("RainbowSpinnerCurrentColors").GetGetMethod();
            MethodInfo mhhrainbowinfoCreator = typeof(MHHRainbowInfo).GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            Instruction first = method.Definition.Body.Instructions.First();
            gen.InsertBefore(first, gen.Create(OpCodes.Ldsfld, typeof(VivHelperModule).GetField("mhhModule")));
            gen.InsertBefore(first, gen.Create(OpCodes.Callvirt, m_mhhModule_getSession));
            gen.InsertBefore(first, gen.Create(OpCodes.Callvirt, m_mhhModuleSession_getRainbowSpinnerCurrentColors));
            gen.InsertBefore(first, gen.Create(OpCodes.Stloc, rainbowspinnerIndex));

            Instruction i = method.Definition.Body.Instructions.First(i => i.MatchLdsfld(typeof(SpinnerGrouper).GetField("_1", BindingFlags.NonPublic | BindingFlags.Static)));
            gen.InsertAfter(i, gen.Create(OpCodes.Ldloc, rainbowspinnerIndex)); //Inverse because we're inserting after a static point
            gen.InsertAfter(i, gen.Create(OpCodes.Pop));

            i = method.Definition.Body.Instructions.First(i => i.MatchLdstr("retriever1"));
            gen.InsertAfter(i, gen.Create(OpCodes.Callvirt, rainbowspinnercolorstate_Props["Flag"]));
            gen.InsertAfter(i, gen.Create(OpCodes.Ldloc, rainbowspinnerIndex));
            gen.InsertAfter(i, gen.Create(OpCodes.Pop));

            i = method.Definition.Body.Instructions.First(i => i.MatchLdstr("retriever2")).Next;
            bool b = true;
            foreach (string p in propsInOrder) {
                if (b) {
                    gen.InsertBefore(i, gen.Create(OpCodes.Pop));
                    gen.InsertBefore(i, gen.Create(OpCodes.Ldloc, rainbowspinnerIndex));
                    b = false;
                } else
                    gen.Replace(i.Previous, gen.Create(OpCodes.Ldloc, rainbowspinnerIndex));
                gen.InsertBefore(i, gen.Create(OpCodes.Callvirt, rainbowspinnercolorstate_Props[p + "WithFlag"]));
                i = i.Next;
                while (i.OpCode == OpCodes.Nop)
                    i = i.Next; // Iterate until the next non-nop instruction. Resolves some debug-build conflicts.
            }
            i = method.Definition.Body.Instructions.First(i => i.MatchLdstr("retriever3")).Next;
            b = true;
            foreach (string p in propsInOrder) {
                if (b) {
                    gen.InsertBefore(i, gen.Create(OpCodes.Pop));
                    gen.InsertBefore(i, gen.Create(OpCodes.Ldloc, rainbowspinnerIndex));
                    b = false;
                } else
                    gen.Replace(i.Previous, gen.Create(OpCodes.Ldloc, rainbowspinnerIndex));
                gen.InsertBefore(i, gen.Create(OpCodes.Callvirt, rainbowspinnercolorstate_Props[p]));
                i = i.Next;
                while (i.OpCode == OpCodes.Nop)
                    i = i.Next; // Iterate until the next non-nop instruction. Resolves some debug-build conflicts.
            }
            _RainbowInfoFromMHHSession = (Func<Level, MHHRainbowInfo>) method.Generate().CreateDelegate(typeof(Func<Level, MHHRainbowInfo>));
        }

        #endregion
    }
}
