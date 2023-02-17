using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VivHelper.Entities.Spinner2;

//  Shader Context:
// Solid Color: No shader. Color context set with color param in draw
/// If Color is set to 0,0,0,0, simply dont render the object

// Connector Gradient: Shader. Solid Gradient between two spinners. 
///  Shader. Color context is a bit tricky.
///  Prerequisite Information: There is a Texture2D stored in memory that is a uvmap: the texture is 64x64
///  R,G,B: maps to two colors on the Texture 
// Pixel Rainbow: Shader.
///  If Color or source pixel is set to 0,0,0,0, simply dont render the object
///  else if Color set to (1,0,0,0), treats it as vanilla default rainbow spinners no matter what
///  else if Color.A == 0, treats it as modded rainbow spinners
///  else, treat it as a gradient between modded rainbow and default
internal enum GroupControlOrder {
    BorderSolid = 0,
    ConnectorBorderGradient = 1,
    BorderRainbow = 2,
    ConnectorSolid = 3,
    ConnectorGradient = 4,
    ConnectorRainbow = 5,
    SpinnerSolidColor = 6,
    SpinnerRainbow = 8, //Spinner cannot have Gradient because the gradient lerps between two colors for things like connectors or borders for connectors, spinner borders cannot have gradients.
}

/// <summary>
/// Spinner Connectors are 
/// </summary>
public class SpinnerConnector : GroupVisualElement {
    //If you're using 2048 different instances of connectors, fuck you.
    //Now uses VivHelperModule.Session.SpinnerGradientColorSet

    public Color color, borderColor;
    public bool texRainbow;

    public float rotation, scale;
    public Spinner A, B;
    internal Spinner M, N; //I want this to be a pointer but bleh

    public SpinnerConnector(Spinner _A, Spinner _B) : base(GetPosition(_A, _B), Spinner.grouperType, (int) GetPriority(_A, _B)) {
        id = "Connector" + _A._ID.ID + "," + _B._ID.ID;
        Visible = true;
        A = _A;
        B = _B;
        rotation = Rotation();
        scale = Scale();
        HandleBorder(A, B);
    }

    private void HandleBorder(Spinner a, Spinner b) {
        if (a.border == null) {
            if (b.border == null)
                return;
            border = new GroupBorderElement(GetPosition(A, B), Spinner.grouperType, (int) (B.IsPixelRainbow(true) ? GroupControlOrder.BorderRainbow : (B.connectorBorderGradient ? GroupControlOrder.ConnectorBorderGradient : GroupControlOrder.BorderSolid)), this);
        } else if (b.border == null)
            border = new GroupBorderElement(GetPosition(A, B), Spinner.grouperType, (int) (A.IsPixelRainbow(true) ? GroupControlOrder.BorderRainbow : (A.connectorBorderGradient ? GroupControlOrder.ConnectorBorderGradient : GroupControlOrder.BorderSolid)), this);
        else
            border = new GroupBorderElement(GetPosition(A, B), Spinner.grouperType, (int) (A.IsPixelRainbow(true) || B.IsPixelRainbow(true) ? GroupControlOrder.BorderRainbow : A.connectorBorderGradient || B.connectorBorderGradient ? GroupControlOrder.ConnectorBorderGradient : GroupControlOrder.BorderSolid), this);
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (A == null && B == null) { RemoveSelf(); return; }
        else if (A == null) {
            M = B;
            color = B.color;
            if(B.border != null || border != null) borderColor = B.borderColor;
        }
        else if (B == null){
            M = A;
            color = A.color;
            if(A.border != null || border != null) borderColor = A.borderColor;
        }
        else { 
            M = Calc.Random.Choose(A, B);
            N = M == A ? B : A;
            if((GroupControlOrder)(int)A.priority == GroupControlOrder.SpinnerRainbow){
                if((GroupControlOrder)(int)B.priority == GroupControlOrder.SpinnerRainbow) {
                    color = M.color;
                }
                color = A.color;
            } else if((GroupControlOrder)(int)B.priority == GroupControlOrder.SpinnerRainbow) {
                color = B.color;
            } else if(A.texRainbow || B.texRainbow) texRainbow = true;
            else {
                color = Color.Lerp(A.color, B.color, 0.5f);
            }
            if(border != null){
                borderColor = M.border == null ? N.borderColor : M.borderColor;
            }
        }
        scale = Scale();
        
    }
    /// <summary>
    /// Override this with custom values as needed.
    /// </summary>
    /// <returns>The texture that the Connector will be drawn with</returns>
    public virtual MTexture GetTexture() {
        return M.ConnectorTex() ?? N?.ConnectorTex();
    }

    internal static Vector2 GetPosition(Spinner A, Spinner B) {
        if (A == null) {
            if (B == null)
                return Vector2.Zero;
            return B.Position + B.Height * Calc.AngleToVector(B.Position.Angle(), 1f);
        } else if (B == null) {
            return A.Position + A.Height * Calc.AngleToVector(B.Position.Angle(), 1f);
        }
        return Vector2.Lerp(A.Position, B.Position, 0.5f);
    }

    internal static GroupControlOrder GetPriority(Spinner A, Spinner B) =>
        (A.IsPixelRainbow(false) || B.IsPixelRainbow(false)) ? GroupControlOrder.ConnectorRainbow :
        (A.connectorGradient || B.connectorGradient) ? GroupControlOrder.ConnectorGradient : GroupControlOrder.ConnectorSolid;

    internal float Rotation() {
        if (A == null) {
            return B == null ? 0 : (float) Math.PI + B.Position.Angle();
        } else
            return B == null ? (float) Math.PI + A.Position.Angle() : Calc.Angle(A.Position, B.Position);
        //By default we want this to point towards B, since that will ensure that our gradients always work.
    }

    public override void Update() {
        base.Update();
        Position = GetPosition(A, B);
        if (texRainbow) {
            color = VivHelper.GetHue(Scene, Position);
        }
    }

    public float Scale() {
        if (A == null || B == null)
            return M?.Scale ?? 1f; // We've already done this nullcheck, so we know that M will be the valid one, unless both are null. If both are null then we're fine anyways and default to 1.
        return MathHelper.Lerp(A.Scale, B.Scale, 0.5f);
    }

    public override void RenderAtom() {
        if (!(A.Visible && B.Visible))
            return;
        MTexture tex = GetTexture();
        if(tex == null) return;
        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, Position, tex.ClipRect, color, rotation, (tex.Center - tex.DrawOffset), scale, SpriteEffects.None, 0);
    }

    public override void RenderBorder() {
        if (!(A.Visible || B.Visible) || border == null)
            return;
        MTexture tex = GetTexture();
        if(tex == null) return;
        Color c = borderColor;
        float s = scale;
        Texture2D sourceTex = tex.Texture.Texture_Safe;
        var clipRect = tex.ClipRect;
        var center = tex.Center - tex.DrawOffset;
        Draw.SpriteBatch.Draw(sourceTex, Position - Vector2.UnitX, clipRect, c, rotation, center, s, SpriteEffects.None, 0);
        Draw.SpriteBatch.Draw(sourceTex, Position - Vector2.UnitY, clipRect, c, rotation, center, s, SpriteEffects.None, 0);
        Draw.SpriteBatch.Draw(sourceTex, Position + Vector2.UnitX, clipRect, c, rotation, center, s, SpriteEffects.None, 0);
        Draw.SpriteBatch.Draw(sourceTex, Position + Vector2.UnitY, clipRect, c, rotation, center, s, SpriteEffects.None, 0);
    }

    public override Type GetGrouperType() => Spinner.grouperType;
}

[Tracked]
public class Spinner : GroupVisualElement {

    public static Type grouperType = typeof(SpinnerGrouper);
    /// <summary>
    /// Takes in a set of values and outputs the corresponding Modded Standard Rainbow Color
    /// </summary>
    /// <param name="initH"></param>
    /// <param name="amplH"></param>
    /// <param name="initS"></param>
    /// <returns></returns>
    private static Color moddedStandardRainbowModifier(float initH, float amplH, float initS) {
        return new Color(initH, amplH, initS / 2, 0f);
    }

    internal readonly EntityID _ID;
    public bool created; //Replacing Expanded

    public Color color, borderColor;
    public bool connectorGradient, connectorBorderGradient;
    public bool texRainbow, borderTexRainbow;
    public readonly int randomSeed;

    public MTexture tex;
    public MTexture connectorTex;
    public MTexture debrisTex;

    public IEnumerable<int> connectorGroups = null;

    public bool AttachToSolid;
    public string Directory, Suffix;

    public float Scale = -1f;
    private readonly float legacyScale = -1f, legacyiScale = -1f;
    public readonly bool debrisToScale, customDebris;

    private string hitboxString;

    public Spinner(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, grouperType) {
        //Basic construction info
        _ID = id;
        this.id = "Spinner:" + id.ToString();

        Depth = data.Int("Depth", Depths.CrystalSpinners); // Depth will be important during the Awake Phase

        //BorderColor deterministic behavior
        bool b = data.Attr("BorderColor") == "Transparent";
        b = true;
        if (!b)
            border = new GroupBorderElement(Position, grouperType, this);
        SetColors(data, b); //Sets all Color info for the Spinner
       

        // NEW - Connection Groups: instead of forcing an "ignore connections" parameter, new version will have "ConnectorGroups" which it will attempt to connect to.
        string conGrp = data.Attr("ConnectorGroups", "0");
        if (!data.Bool("ignoreConnection") && !string.IsNullOrEmpty(conGrp))
            connectorGroups = conGrp.Split(',').Select(t => int.Parse(t));
        AttachToSolid = data.Bool("AttachToSolid");
        Directory = data.Attr("Directory", "VivHelper/customSpinner/white").TrimEnd('/');
        Suffix = data.NoEmptyString("Subdirectory", "white");
        if (!string.IsNullOrWhiteSpace(Suffix) && Suffix.Trim()[0] != '_')
            Suffix = "_" + Suffix.Trim();


        //Scale handling behavior
        //`scale` is the new variable
        //`ImageScale` and `Scale` are the two legacy variables
        if (data.Float("ImageScale") > 0) {
            legacyScale = data.Float("Scale", 1f);
            legacyScale = legacyScale == -1f ? 1f : Math.Max(1 / 3f, legacyScale);
            legacyiScale = data.Float("ImageScale", 1f);
            legacyiScale = legacyiScale == -1f ? 1f : Math.Max(1 / 3f, legacyiScale);
        } else {

        }
        debrisToScale = data.Bool("DebrisToScale", true);
        customDebris = data.Bool("CustomDebris", false);

        // STRETCH GOAL : Redefine Hitbox processor
        //Handles custom hitbox processing
        hitboxString = data.Attr("HitboxType", data.Bool("removeRectHitbox", false) ? "C:6" : "C:6|R:16,4;-8,*1@-4");
        if (string.IsNullOrWhiteSpace(hitboxString))
            hitboxString = "C:6|R:16,4;-8,*1@-4";
        base.Collider = SpinnerHelperFunctions.OldParseHitboxType(hitboxString, Scale < 0 ? legacyScale : Scale);

        randomSeed = data.Bool("isSeeded") ? data.Int("seed") : Calc.Random.Next();
    }

    private void SetColors(EntityData data, bool borderTransparent) {
        string c0 = data.Attr("Color");
        if (data.Has("Type") && data.Enum<CustomSpinner.Types>("Type", CustomSpinner.Types.White) != CustomSpinner.Types.White) { //Determines if it is the old type of Rainbow Spinner
            texRainbow = true;
            SetPriority((uint)GroupControlOrder.SpinnerSolidColor);
        } else if (c0.StartsWith("Rainbow")) {
            color = SpinnerHelperFunctions.ColorToDrawColor(c0 == "RainbowAlt" ? 3u : 2u, SpinnerHelperFunctions.standardPixelRainbowColor);
            SetPriority((uint)GroupControlOrder.SpinnerRainbow);
        } else if (c0.StartsWith("VanillaRainbow")) {
            color = SpinnerHelperFunctions.ColorToDrawColor(c0 == "VanillaRainbowAlt" ? 3u : 2u, SpinnerHelperFunctions.vanillaPixelRainbowColor);
            SetPriority((uint)GroupControlOrder.SpinnerRainbow);
        } else {
            color = SpinnerHelperFunctions.ColorToDrawColor(0u, VivHelper.ColorFix(c0));
            SetPriority((uint)GroupControlOrder.SpinnerSolidColor);
        }
        if (borderTransparent)
            return;
        string c1 = data.Attr("BorderColor");
        if (c1.StartsWith("Rainbow")) {
            borderColor = SpinnerHelperFunctions.ColorToDrawColor(c1 == "RainbowAlt" ? 3u : 2u, SpinnerHelperFunctions.standardPixelRainbowColor);
            border.SetPriority((uint)GroupControlOrder.BorderRainbow);
        } else if (c1.StartsWith("VanillaRainbow")) {
            borderColor = SpinnerHelperFunctions.ColorToDrawColor(c1 == "VanillaRainbowAlt" ? 3u : 2u, SpinnerHelperFunctions.vanillaPixelRainbowColor);
            border.SetPriority((uint)GroupControlOrder.BorderRainbow);
        } else {
            borderColor = VivHelper.ColorFix(c1);
            border.SetPriority((uint)GroupControlOrder.BorderSolid);
        }
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        if (border != null)
            scene.Add(border);
        Visible = true;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (connectorGroups != null) {
            CreateConnectors(scene);
        }
        if (InView() && !created) {
            CreateSprites();
            created = true;
        }
    }

    public override void Update() {
        base.Update();
        if (InView() && !created) {    
            CreateSprites();
            created = true;
        }
    }

    /// <summary>
    /// Should set the values for tex and connectorTex
    /// </summary>
    protected virtual void CreateSprites() {
        Calc.PushRandom(randomSeed);
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(Directory + "/fg" + Suffix);
        tex = Calc.Random.Choose(atlasSubtextures);
        atlasSubtextures = GFX.Game.GetAtlasSubtextures(Directory + "/bg" + Suffix);
        connectorTex = Calc.Random.Choose(atlasSubtextures);
        Calc.PopRandom();

    }

    protected virtual void CreateConnectors(Scene scene) {
        foreach (Spinner entity in Scene.Tracker.GetEntities<Spinner>()) {
            float s = 12 * (Scale + entity.Scale);
            float t = (entity.Position - Position).LengthSquared();
            if (_ID.ID > entity._ID.ID && entity.AttachToSolid == AttachToSolid &&
              t <= s*s && entity.Depth == Depth && entity.connectorGroups.Any(i => connectorGroups.Contains(i)))
                scene.Add(new SpinnerConnector(this, entity));
        }
    }

    public virtual MTexture ConnectorTex() { return connectorTex; }

    public override Type GetGrouperType() => grouperType;

    protected const float SpinnerPadding = 16f;

    protected virtual bool InView() {
        if (base.Scene == null) {
            RemoveFromGroup(grouper);
            return false;
        }
        Camera camera = (base.Scene as Level).Camera;
        if (camera == null)
            return false;
        var t = camera.Zoom;
        if (base.Right > camera.X - (16f * t) && base.Bottom > camera.Y - (16f * t) && base.Left < camera.X + (336f * t)) {
            return base.Top < camera.Y + (196f * t);
        }
        return false;
    }

    public override void RenderAtom() {
        if (!created || !Visible || !InView())
            return;
        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, Position, tex.ClipRect, color, 0f, (tex.Center - tex.DrawOffset), 1f, SpriteEffects.None, 0);
    }
    public override void RenderBorder() {
        if (!created || !Visible || !InView() || border == null)
            return;
        Texture2D sourceTex = tex.Texture.Texture_Safe;
        var clipRect = tex.ClipRect;
        var center = tex.Center - tex.DrawOffset;
        Draw.SpriteBatch.Draw(sourceTex, Position - Vector2.UnitX, clipRect, borderColor, 0f, center, 1f, SpriteEffects.None, 0);
        Draw.SpriteBatch.Draw(sourceTex, Position - Vector2.UnitY, clipRect, borderColor, 0f, center, 1f, SpriteEffects.None, 0);
        Draw.SpriteBatch.Draw(sourceTex, Position + Vector2.UnitX, clipRect, borderColor, 0f, center, 1f, SpriteEffects.None, 0);
        Draw.SpriteBatch.Draw(sourceTex, Position + Vector2.UnitY, clipRect, borderColor, 0f, center, 1f, SpriteEffects.None, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPixelRainbow(bool Border) { return (Border ? border?.GetPriority() : GetPriority()) % 3 == 2; }
}

public static class SpinnerHelperFunctions {
    public static Color standardPixelRainbowColor = new Color(1f, 0, 1f, 0f);
    public static Color vanillaPixelRainbowColor = new Color(102, 102, 51, 0);


    /// Shader color functions:
    /// Color source: 32-bit unsigned integer
    /// 0bRRRRRRRRGGGGGGGGBBBBBBBBAAAAAAXX
    /// XX = "type" control
    /// Alpha is defined as AAAAAA11, or, Alpha is remapped to 64th divisions instead of 256th divisions, so Alpha of 252,253,254,255 is mapped to 255
    /// The color evaluation of the remainder of the values is deterministic on the type control.
    /// XX = 00: Default coloration, RGBA is just RGBA
    /// XX = 01: GradientMap:
    ///   Alpha: average between the two alpha values of the spinners, done C# side
    ///   R,G,B: map to two values U,V such that:
    ///     U: 12 bit integer: R: 0bKJHFEDCA / 16 = 0b0000KJHF * 0b11111111 + 0bGGGGGGGG = 0bKJHFGGGGGGGG
    ///     V: 12 bit integer: R: 0bKJHFEDCA % 16 = 0b0000EDCA * 0b11111111 + 0bBBBBBBBB = 0bEDCABBBBBBBB
    ///   split U,V into Ux,Uy, Vx,Vy:
    ///     U: 0bKJHFGGGGGGGG => Ux: 0b000000KJHFGG (/64), Uy: 0b000000GGGGGG (%64)
    ///     V: 0bEDCABBBBBBBB => Vx: 0b000000EDCABB (/64), Vy: 0b000000BBBBBB (%64)
    /// XX = 02,03: Rainbow Function:
    ///   XX == 3 : show with grayscale, i.e. show the "detail" of the texture, not what values are colored in
    ///   Special Cases: 
    ///     if color has 0 alpha but some red/green/blue component, treat as the common rainbow behavior.
    ///       if green is 0, treat as "default rainbow" behavior, which amounts to either vanilla rainbow or modded rainbow if one exists.
    ///       stock Vanilla rainbow: R = 102, G = 102, B = 51, A = 0
    ///       color is defined as HSV, and then translated to RGB:
    ///       H: mod({R/255} + yoyo(([distance from position of pixel in world coordinates to (0,0)] + [TimeElapsed] * 50) % 280 / 280) * {G/255})
    ///       S: 
    ///       V: B & 15
    public static Color ColorToDrawColor(uint priority, params Color[] colors) {
        switch (priority % 3) {
            case 1: //gradient
                if (colors?.Length > 1) {
                    return ColorToShaderValidColor(SpinnerGrouper.GetDrawColorForGradient(colors[0], colors[1]), 1);
                }
                throw new ArgumentException("you must input at least 2 colors into the `ColorToDrawColor` method with input of \"Gradient.\"");
            case 2: //Rainbow
                switch (colors?.Length ?? 0) {
                    case 0:
                        return ColorToShaderValidColor(standardPixelRainbowColor, 2);
                    default:
                        switch (colors[0].A % 4) {
                            case 1:
                                return ColorToShaderValidColor(colors[0], 3);
                            case 2:
                                return ColorToShaderValidColor(colors[0], 2);
                            default:
                                return ColorToShaderValidColor(colors[0], 2);
                        }
                        return ColorToShaderValidColor(colors[0], 2);
                }
            default: //Solid Color
                if (colors?.Length > 0) {
                    return ColorToShaderValidColor(colors[0], 0);
                }
                throw new ArgumentException("you must input at least 1 color into the `ColorToDrawColor` method, unless the input is \"Rainbow[Alt]\" or \"VanillaRainbow[Alt]\" (note: [Alt] means either put nothing at the end or \"Alt\" at the end)");
        }
    }

    /// <summary>
    /// If you dont know what this is, use "ColorToDrawColor"
    /// </summary>
    private static Color ColorToShaderValidColor(Color color, int group, byte? alpha = null) {
        return new Color(color.R, color.G, color.B, ((alpha ?? color.A) & 252) + (group & 3)); // 0bXXXXXXXX & 0b11111100 = 0bXXXXXX00 + 0b000000YY = 0bXXXXXXYY
    }

    public static ColliderList OldParseHitboxType(string _S, float scale) {
        List<Collider> colliders = new List<Collider>();
        /*At this point, string[] S is a string where each string should be formatted like this:
         * SMaster = A1|A2|A3|A4...|An, S[k] = Ak
         * where Ak => T:U:V
         * where:
         *	: ; = Separators
         *	T = Type: C for circle, R for Rect.
         *	U = Parameter: for C: r for radius, for R: <w,h>
         *	V = Position offset from Center.
         *	using * before a number as an ignore scale definer.
         *	using a p @ before a number n means (p + n)

         */
        foreach (string s in _S.Split('|')) {
            string[] k = s.Trim().Split(':', ';'); //Splits Ak into T (k[0]), U (k[1]), and V (k[2])
                                            //We assume that people are going to use this correctly, for now.
            if (k[0][0] == 'C') {
                if (k.Length == 2) { colliders.Add(ParseCircle(scale, k[1])); } else { colliders.Add(ParseCircle(scale, k[1], k[2])); }
            } else if (k[0][0] == 'R') {
                if (k.Length == 2) { colliders.Add(ParseRectangle(scale, k[1])); } else { colliders.Add(ParseRectangle(scale, k[1], k[2])); }
            }
        }
        return new ColliderList(colliders.ToArray());

    }

    private static Collider ParseCircle(float scale, string rad, string off = "0,0") {
        int radius;
        int[] offset = new int[2];
        radius = ParseInt(rad, scale);
        string[] offs = off.Split(',');
        for (int i = 0; i < 2; i++) {
            offset[i] = ParseInt(offs[i], scale);
        }
        return new Circle(radius, offset[0], offset[1]);
    }

    private static Collider ParseRectangle(float scale, string Wh) {
        int[] wh = new int[2];
        int[] offset = new int[2];
        string[] a = Wh.Split(',');
        wh[0] = ParseInt(a[0], scale);
        wh[1] = ParseInt(a[1], scale);
        offset[0] = 0 - Math.Abs((int) Math.Round(wh[0] / 2f));
        offset[1] = Math.Min(-3, 0 - Math.Abs((int) Math.Round(wh[1] / 2f)));
        return new Hitbox(wh[0], wh[1], offset[0], offset[1]);
    }

    private static Collider ParseRectangle(float scale, string Wh, string off) {
        int[] wh = new int[2];
        int[] offset = new int[2];
        string[] a = Wh.Split(',');
        string[] b = off.Split(',');
        for (int i = 0; i < 2; i++) {
            wh[i] = ParseInt(a[i], scale);
            offset[i] = ParseInt(b[i], scale);
        }
        return new Hitbox(wh[0], wh[1], offset[0], offset[1]);
    }

    private static int ParseInt(string k, float scale) {
        if (string.IsNullOrEmpty(k)) {
            throw new Exception("Integer was empty.");
        }
        if (k.Contains("@")) {
            string[] q = k.Split('@');
            int p = 0;
            for (int s = 0; s < q.Length; s++) { p += ParseInt(q[s], scale); }
            return p;
        }
        if (k[0] == '*') {
            return int.Parse(k.Substring(1));
        } else {
            return (int) Math.Round(int.Parse(k) * (double) scale);
        }
    }
}