using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using System.Collections;
using VivHelper.Entities;
using Celeste.Mod.VivHelper;
using Celeste.Mod.Entities;

namespace VivHelper.Entities.CurvedStuff {
    [CustomEntity(
        "VivHelper/CurvedZipMover = Legacy",
        "VivHelper/CurvedZipMover2 = Curve",
        "VivHelper/CustomZipMover = Custom",
        "VivHelper/CrumblingZipMover = CrumbleLegacy",
        "VivHelper/CustomCrumblingZipMover = CrumbleCustom",
        "VivHelper/CurvedCrumblingZipMover = CrumbleCurve"
    )]
    public class CustomCurvedZipMover : Solid {
        public static Entity Legacy(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CustomCurvedZipMover(entityData, offset, false, entityData.Bool("MoveType", true));
        public static Entity Custom(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CustomCurvedZipMover(entityData, offset, false, false);
        public static Entity Curve(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CustomCurvedZipMover(entityData, offset, false, true);
        public static Entity CrumbleLegacy(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CustomCurvedZipMover(entityData, offset, true, entityData.Bool("MoveType", true));
        public static Entity CrumbleCustom(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CustomCurvedZipMover(entityData, offset, true, false);
        public static Entity CrumbleCurve(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CustomCurvedZipMover(entityData, offset, true, true);

        public static ParticleType P_Scrape = ZipMover.P_Scrape;

        public static ParticleType P_Sparks = ZipMover.P_Sparks;

        private MTexture[,] edges = new MTexture[3, 3];

        private Sprite streetlight;

        private BloomPoint bloom;

        private CustomCurvedZipMoverPathRenderer pathRenderer1;

        private List<MTexture> innerCogs;

        private MTexture temp = new MTexture();

        private bool drawBlackBorder;

        private Vector2 start;

        private Vector2 target;

        private float percent;

        private Color ropeColor = Calc.HexToColor("663931");

        private Color ropeLightColor = Calc.HexToColor("9b6157");

        private SoundSource sfx = new SoundSource();


        private Color baseColor;
        private float speedModF, speedModB;
        public string identifier;
        private bool moveType, fixNotch;
        private string audio;
        private string Path;
        private CurveEntity curve;
        private CurvedPath curvedPath;
        private bool createSparks;
        private Ease.Easer EaseType;

        private bool Launch;
        private bool Uniform, reverse, liftSpeed_dejank;
        private float PostNyoomDelay;

        public enum DebrisAmount { Eighth, Quarter, Half, Normal, None }

        public DebrisAmount dAmt = DebrisAmount.Normal;

        private bool crumble;
        private float crumbleTimer;
        private int respawnCount;
        private float respawnDelay;

        public CustomCurvedZipMover(EntityData data, Vector2 offset, bool crumble, bool move)
            : base(data.Position + offset, data.Width, data.Height, safe: false) {
            moveType = move;
            identifier = data.Attr("CurveIdentifier", "");
            if (identifier == "") { target = data.Nodes[0] + offset; }
            Path = data.Attr("CustomSpritePath", "").TrimEnd('/');
            string t = Path;
            ropeColor = VivHelper.GetColorWithFix(data, "RopeColor", "ropeColor", VivHelper.GetColorParams.None, VivHelper.GetColorParams.None, new Color(102,57,49)).Value;
            ropeLightColor = VivHelper.GetColorWithFix(data, "RopeNotchColor", "ropeNotchColor", VivHelper.GetColorParams.None, VivHelper.GetColorParams.None, new Color(155, 97, 87)).Value;
            if (!VivHelper.TryGetEaser(data.Attr("EaseType", "SineIn"), out EaseType))
                EaseType = Ease.SineIn;
            float lightOcclusion = data.Float("LightOcclusion", 1f);
            baseColor = VivHelper.GetColorWithFix(data, "BaseColor", "baseColor", VivHelper.GetColorParams.None, VivHelper.GetColorParams.None, Color.White).Value;
            audio = data.Attr("AudioOnLaunch", "");
            t = t == "" ? "objects/zipmover" : t;
            string path, id, key;
            path = t + "/light";
            id = t + "/block";
            key = t + "/innercog";
            drawBlackBorder = data.Bool("DrawBlackBorder", true);
            speedModF = data.Float("SpeedModF", 1f);
            PostNyoomDelay = data.Float("PostLaunchPause", 0.5f);
            speedModB = data.Float("SpeedModB", 1f);
            base.Depth = -9999;
            start = Position;
            if (moveType) { Uniform = data.Bool("Uniform", false); } else { target = data.NodesOffset(offset)[0]; }
            reverse = data.Bool("Reverse", false);
            Launch = data.Bool("Slingshot", false);
            liftSpeed_dejank = data.Bool("LaunchDejank", false);
            if (!Launch) { Add(new Coroutine(SequenceDefault())); } else { Add(new Coroutine(SequenceLaunch())); }
            Add(new LightOcclude(lightOcclusion));
            innerCogs = GFX.Game.GetAtlasSubtextures(key);
            Add(streetlight = new Sprite(GFX.Game, path));
            streetlight.Add("frames", "", 1f);
            streetlight.Play("frames");
            streetlight.Active = false;
            streetlight.SetAnimationFrame(1);
            streetlight.Position = new Vector2(base.Width / 2f - streetlight.Width / 2f, 0f);
            streetlight.SetColor(baseColor);
            Add(bloom = new BloomPoint(data.Bool("RemoveBloomPoint") ? 0f : 1f, 6f));
            bloom.Position = new Vector2(base.Width / 2f, 4f);
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    edges[i, j] = GFX.Game[id].GetSubtexture(i * 8, j * 8, 8, 8);
                }
            }
            SurfaceSoundIndex = 7;
            sfx.Position = new Vector2(base.Width, base.Height) / 2f;
            Add(sfx);
            if (crumble) {
                this.crumble = crumble;
                crumbleTimer = Math.Min(Math.Max(data.Float("CrumbleTimer", 1f), 0f), 100f);
                respawnCount = data.Int("RespawnCount", -1);
                respawnDelay = data.Float("RespawnDelay", 1.5f);
                dAmt = data.Enum<DebrisAmount>("DebrisAmount", DebrisAmount.Normal);
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (!moveType) {
                scene.Add(pathRenderer1 = new CustomCurvedZipMoverPathRenderer(this));
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (moveType) {
                if (CurveEntity.curveEntities.Count > 0) {
                    if (!CurveEntity.curveEntities.ContainsKey(identifier)) { throw new Exception("No curve with the Identifier " + identifier + " was found."); }
                    curve = CurveEntity.curveEntities[identifier];
                    Position = curve.bezier.GetPoint(reverse ? 1f : 0f) - new Vector2(Width / 2f, Height / 2f);
                    VivPathLine[] lines = new VivPathLine[]
                    {
                        new VivPathLine
                        {
                            distance = fixNotch ? 6f : 5f,
                            startT = curve.bezier.tStart,
                            endT = curve.bezier.tEnd,
                            color = ropeColor,
                            offset = Vector2.Zero,
                            thickness = 1.7f,
                            type = "line",
                            addEnds = false
                        },
                        new VivPathLine
                        {
                            distance = fixNotch ? 5f : 6f,
                            startT = curve.bezier.tStart,
                            endT = curve.bezier.tEnd,
                            color = ropeLightColor,
                            offset = Vector2.Zero,
                            thickness = 1.7f,
                            type = "dashed",
                            addEnds = false
                        }
                    };
                    curvedPath = new CurvedPath(curve.bezier, lines);
                    scene.Add(curvedPath);
                    start = curve.bezier.GetPoint(0);
                }
            }
        }

        public override void Removed(Scene scene) {
            if (moveType) { scene.Remove(curvedPath); } else {
                scene.Remove(pathRenderer1);
            }
            pathRenderer1 = null;
            base.Removed(scene);
        }

        public override void Update() {
            base.Update();
            bloom.Y = streetlight.CurrentAnimationFrame * 3;
        }

        public override void Render() {
            Vector2 position = Position;
            Position += base.Shake;
            Draw.Rect(base.X + 1f, base.Y + 1f, base.Width - 2f, base.Height - 2f, Color.Black);
            int num = 1;
            float num2 = 0f;
            int count = innerCogs.Count;
            for (int i = 4; (float) i <= base.Height - 4f; i += 8) {
                int num3 = num;
                for (int j = 4; (float) j <= base.Width - 4f; j += 8) {
                    int index = (int) (VivHelper.mod((num2 + (float) num * percent * Consts.TAU * 2f) / (Consts.PIover2), 1f) * (float) count);
                    MTexture mTexture = innerCogs[index];
                    Rectangle rectangle = new Rectangle(0, 0, mTexture.Width, mTexture.Height);
                    Vector2 zero = Vector2.Zero;
                    if (j <= 4) {
                        zero.X = 2f;
                        rectangle.X = 2;
                        rectangle.Width -= 2;
                    } else if ((float) j >= base.Width - 4f) {
                        zero.X = -2f;
                        rectangle.Width -= 2;
                    }
                    if (i <= 4) {
                        zero.Y = 2f;
                        rectangle.Y = 2;
                        rectangle.Height -= 2;
                    } else if ((float) i >= base.Height - 4f) {
                        zero.Y = -2f;
                        rectangle.Height -= 2;
                    }
                    mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
                    mTexture.DrawCentered(Position + new Vector2(j, i) + zero, baseColor * ((num < 0) ? 0.5f : 1f));
                    num = -num;
                    num2 += Consts.PIover3;
                }
                if (num3 == num) {
                    num = -num;
                }
            }
            for (int k = 0; (float) k < base.Width / 8f; k++) {
                for (int l = 0; (float) l < base.Height / 8f; l++) {
                    int num4 = (k != 0) ? (((float) k != base.Width / 8f - 1f) ? 1 : 2) : 0;
                    int num5 = (l != 0) ? (((float) l != base.Height / 8f - 1f) ? 1 : 2) : 0;
                    if (num4 != 1 || num5 != 1) {
                        edges[num4, num5].Draw(new Vector2(base.X + (float) (k * 8), base.Y + (float) (l * 8)), Vector2.Zero, baseColor);
                    }
                }
            }
            base.Render();
            Position = position;
        }

        private void ScrapeParticlesCheck(Vector2 to) {
            if (!base.Scene.OnInterval(0.03f)) {
                return;
            }
            bool flag = to.Y != base.ExactPosition.Y;
            bool flag2 = to.X != base.ExactPosition.X;
            if (flag && !flag2) {
                int num = Math.Sign(to.Y - base.ExactPosition.Y);
                Vector2 value = (num != 1) ? base.TopLeft : base.BottomLeft;
                int num2 = 4;
                if (num == 1) {
                    num2 = Math.Min((int) base.Height - 12, 20);
                }
                int num3 = (int) base.Height;
                if (num == -1) {
                    num3 = Math.Max(16, (int) base.Height - 16);
                }
                if (base.Scene.CollideCheck<Solid>(value + new Vector2(-2f, num * -2))) {
                    for (int i = num2; i < num3; i += 8) {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2(0f, (float) i + (float) num * 2f), Consts.PIover4 * ((num == 1) ? -1 : 1));
                    }
                }
                if (base.Scene.CollideCheck<Solid>(value + new Vector2(base.Width + 2f, num * -2))) {
                    for (int j = num2; j < num3; j += 8) {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopRight + new Vector2(-1f, (float) j + (float) num * 2f), Consts.PIover4 * ((num == 1) ? -3 : 3));
                    }
                }
            } else {
                if (!flag2 || flag) {
                    return;
                }
                int num4 = Math.Sign(to.X - base.ExactPosition.X);
                Vector2 value2 = (num4 != 1) ? base.TopLeft : base.TopRight;
                int num5 = 4;
                if (num4 == 1) {
                    num5 = Math.Min((int) base.Width - 12, 20);
                }
                int num6 = (int) base.Width;
                if (num4 == -1) {
                    num6 = Math.Max(16, (int) base.Width - 16);
                }
                if (base.Scene.CollideCheck<Solid>(value2 + new Vector2(num4 * -2, -2f))) {
                    for (int k = num5; k < num6; k += 8) {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2((float) k + (float) num4 * 2f, -1f), Consts.PIover4 * ((num4 == 1) ? 3 : 1));
                    }
                }
                if (base.Scene.CollideCheck<Solid>(value2 + new Vector2(num4 * -2, base.Height + 2f))) {
                    for (int l = num5; l < num6; l += 8) {
                        SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.BottomLeft + new Vector2((float) l + (float) num4 * 2f, 0f), Consts.PIover4 * ((num4 == 1) ? -3 : -1));
                    }
                }
            }
        }

        private IEnumerator SequenceDefault() {
            Vector2 start = Position;
            while (true) {
                if (!HasPlayerRider()) {
                    yield return null;
                    continue;
                }
                sfx.Play(audio == "" ? "event:/game/01_forsaken_city/zip_mover" : audio);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                StartShaking(0.1f);
                yield return 0.1f;
                streetlight.SetAnimationFrame(3);
                StopPlayerRunIntoAnimation = false;

                float at2 = 0f;
                while (at2 < 1f) {
                    yield return null;
                    at2 = Calc.Approach(at2, 1f, 2f * Engine.DeltaTime * speedModF);
                    Vector2 vector;
                    percent = reverse ? 1 - EaseType(at2) : EaseType(at2);
                    if (moveType) {
                        if (Uniform) { vector = curve.bezier.GetPointFromLength(percent * curve.bezier.GetBezierLength(25)); } else { vector = curve.bezier.GetPoint(percent * curve.bezier.curves.Length); }
                        vector -= new Vector2(Width / 2f, Height / 2f);
                    } else { vector = Vector2.Lerp(start, target, percent); }
                    ScrapeParticlesCheck(vector);
                    if (Scene.OnInterval(0.1f) && !moveType) {
                        pathRenderer1.CreateSparks();
                    }
                    MoveTo(vector);
                }
                StartShaking(0.2f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().Shake();
                StopPlayerRunIntoAnimation = true;
                if (crumble) {
                    sfx.Stop();
                    yield return Crumble();
                } else {
                    yield return PostNyoomDelay;
                    StopPlayerRunIntoAnimation = false;
                    streetlight.SetAnimationFrame(2);
                    at2 = 0f;
                    while (at2 < 1f) {
                        yield return null;
                        at2 = Calc.Approach(at2, 1f, speedModB * Engine.DeltaTime / 2f);
                        Vector2 vector;
                        percent = 1f - EaseType(at2);

                        if (moveType) {
                            if (Uniform) { vector = curve.bezier.GetPointFromLength(percent * curve.bezier.GetBezierLength(25)); } else { vector = curve.bezier.GetPoint(percent * curve.bezier.curves.Length); }
                            vector -= new Vector2(Width / 2f, Height / 2f);
                        } else { vector = Vector2.Lerp(start, target, percent); }
                        MoveTo(vector);
                    }
                    StopPlayerRunIntoAnimation = true;
                    StartShaking(0.2f);
                    streetlight.SetAnimationFrame(1);
                    yield return 0.5f;
                }
            }
        }

        private IEnumerator SequenceLaunch() {
            Vector2 start = Position;
            while (true) {
                if (!HasPlayerRider()) {
                    yield return null;
                    continue;
                }
                sfx.Play(audio == "" ? "event:/game/01_forsaken_city/zip_mover" : "event:/new_content/game/10_farewell/zip_mover");
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                StartShaking(0.1f);
                yield return 0.1f;
                streetlight.SetAnimationFrame(3);
                StopPlayerRunIntoAnimation = false;
                float at2 = 0f;
                while (at2 < 1f) {
                    yield return null;
                    at2 = Calc.Approach(at2, 1f, 2f * Engine.DeltaTime * speedModF);

                    percent = reverse ? Math.Abs((EaseType(at2) * 2f) - 1f) : 1 - Math.Abs((EaseType(at2) * 2f) - 1f);

                    Vector2 vector;
                    if (moveType) {
                        if (Uniform) { vector = curve.bezier.GetPointFromLength(percent * curve.bezier.GetBezierLength(25)); } else { vector = curve.bezier.GetPoint(percent); }
                    } else { vector = Vector2.Lerp(start, target, percent); }
                    ScrapeParticlesCheck(vector);
                    if (Scene.OnInterval(0.1f) && !moveType) {
                        pathRenderer1.CreateSparks();
                    }
                    MoveTo(vector - new Vector2(Width / 2f, Height / 2f));
                }
                StopPlayerRunIntoAnimation = true;
                StartShaking(0.2f);
                streetlight.SetAnimationFrame(1);
                if (crumble) { sfx.Stop(); yield return Crumble(); } else { yield return PostNyoomDelay; }

            }
        }

        private IEnumerator Crumble() {
            yield return crumbleTimer;
            bloom.Alpha = 0f;
            Break();
            yield return Math.Max(respawnDelay - 1f, 0f);
            respawnCount -= 1;
            if (respawnCount != 0) { yield return Respawn(); }

        }

        private void Break() {
            if (!Collidable || Scene == null) {
                return;
            }
            Audio.Play("event:/new_content/game/10_farewell/quake_rockbreak", Position);
            Collidable = false;
            Visible = false;
            Vector2 dir;

            if (moveType) {
                Vector2[] t = new Vector2[] { curve.bezier.GetPoint(0.02f) - curve.bezier.GetPoint(0f), curve.bezier.GetPoint(curve.bezier.tEnd - 0.02f) - curve.bezier.GetPoint(curve.bezier.tEnd) };
                dir = Center - (reverse ? t[0] : t[1]);
            } else { dir = Vector2.Lerp(start, target, reverse ? 0.02f : 0.98f); }
            if (crumbleTimer <= .25f) { dir *= (float) (0.25f / crumbleTimer); } else { dir = TopCenter; }
            BreakAction(dir, dAmt, '7');
        }

        private void BreakAction(Vector2 direction, DebrisAmount d, char c) {
            float wN, hN;
            float iM, jM;
            iM = base.Width / 16f;
            jM = base.Height / 16f;
            switch (d) {
                case DebrisAmount.Half:
                    wN = hN = 2f;
                    break;
                case DebrisAmount.Quarter:
                    wN = hN = 4f;
                    break;
                case DebrisAmount.Eighth:
                    wN = hN = 8f;
                    break;
                default:
                    wN = hN = 1f;
                    break;
            }
            if (d != DebrisAmount.None) {
                for (float i = 0; i < iM; i += wN) {
                    for (float j = 0; j < jM; j += hN) {
                        if (!base.Scene.CollideCheck<Solid>(new Rectangle((int) base.X + (int) i * 8, (int) base.Y + (int) j * 8, 8, 8))) {
                            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(new Vector2(base.X + (float) i * 8f + 4f, base.Y + (float) j * 8f + 4f), c).BlastFrom(direction));
                        }
                        if (!base.Scene.CollideCheck<Solid>(new Rectangle((int) base.X + (int) ((base.Width / 8f) - (i * 8)), (int) base.Y + (int) ((base.Height / 8f) - (j * 8)), 8, 8))) {
                            base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2((base.Width / 8f + 4) - i * 8, (base.Height / 8f + 4) - j * 8), c).BlastFrom(direction));
                        }

                    }
                }
            }
        }

        private IEnumerator Respawn() {
            Position = start;
            VertexLight vl = new VertexLight(Color.Lerp(baseColor, Color.White, 0.8f), 0.02f, 64, 96);
            Add(vl);
            while (vl.Alpha < 1f) {
                vl.Alpha += 0.02f;
                yield return 0.01f;
            }
            yield return 0.1f;
            vl.Alpha = 0.7f;
            while (vl.Alpha < 1f) {
                vl.Alpha += 0.02f;
                yield return 0.01f;
            }
            vl.Alpha = 0.7f;
            while (vl.Alpha < 1f) {
                vl.Alpha += 0.02f;
                vl.Color = Color.Lerp(baseColor, Color.White, 1.5f - vl.Alpha);
                yield return 0.01f;
            }
            vl.Alpha = 1f;
            bloom.Alpha = 1f;
            yield return 0.05;
            Visible = Collidable = true;
            Audio.Play("event:/char/badeline/disappear");
            Remove(vl);


        }

        private class CustomCurvedZipMoverPathRenderer : Entity {
            public CustomCurvedZipMover zipMover;

            private MTexture cog;

            private Vector2 from;

            private Vector2 to;

            private Vector2 sparkAdd;

            private float sparkDirFromA;

            private float sparkDirFromB;

            private float sparkDirToA;

            private float sparkDirToB;

            public CustomCurvedZipMoverPathRenderer(CustomCurvedZipMover CustomCurvedZipMover) {
                base.Depth = 5000;
                zipMover = CustomCurvedZipMover;
                from = zipMover.start + new Vector2(zipMover.Width / 2f, zipMover.Height / 2f);
                to = zipMover.target + new Vector2(zipMover.Width / 2f, zipMover.Height / 2f);
                sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
                float num = (from - to).Angle();
                sparkDirFromA = num + Consts.PIover8;
                sparkDirFromB = num - Consts.PIover8;
                sparkDirToA = num + Consts.PIover8 * 7;
                sparkDirToB = num + Consts.PIover8 * 9;

                cog = GFX.Game[zipMover.Path + "/cog"];
            }

            public void CreateSparks() {
                if (zipMover.createSparks)
                    SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
                SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
            }

            public override void Render() {
                DrawCogs(Vector2.UnitY, Color.Black);
                DrawCogs(Vector2.Zero, null);
                if (zipMover.drawBlackBorder) {
                    Draw.Rect(new Rectangle((int) (zipMover.X + zipMover.Shake.X - 1f), (int) (zipMover.Y + zipMover.Shake.Y - 1f), (int) zipMover.Width + 2, (int) zipMover.Height + 2), Color.Black);
                }
            }

            private void DrawCogs(Vector2 offset, Color? colorOverride = null) {
                Vector2 vector = (to - from).SafeNormalize();
                Vector2 value = vector.Perpendicular() * 3f;
                Vector2 value2 = -vector.Perpendicular() * 4f;
                float rotation = zipMover.percent * Consts.TAU;
                Draw.Line(from + value + offset, to + value + offset, colorOverride.HasValue ? colorOverride.Value : zipMover.ropeColor);
                Draw.Line(from + value2 + offset, to + value2 + offset, colorOverride.HasValue ? colorOverride.Value : zipMover.ropeColor);
                for (float num = 4f - zipMover.percent * (float) Math.PI * 8f % 4f; num < (to - from).Length(); num += 4f) {
                    Vector2 value3 = from + value + vector.Perpendicular() + vector * num;
                    Vector2 value4 = to + value2 - vector * num;
                    Draw.Line(value3 + offset, value3 + vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : zipMover.ropeLightColor);
                    Draw.Line(value4 + offset, value4 - vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : zipMover.ropeLightColor);
                }
                cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : zipMover.baseColor, 1f, rotation);
                cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : zipMover.baseColor, 1f, rotation);
            }
        }
    }
}
