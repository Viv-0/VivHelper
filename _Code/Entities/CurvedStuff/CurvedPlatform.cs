using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CustomPlatform")]
    public class CustomMovingPlatform : JumpThru {
        public string identifier;
        public CurveEntity curve;
        public Vector2 start, end;
        public MTexture[] textures;
        private string tempTexturePath;
        public float speedMod;
        public bool moveType;
        public bool reverse;
        public Color[] lineCols;
        public bool uniform;

        private float curvesLength;
        private Ease.Easer EaseType;


        private float addY;
        private float sinkTimer;
        private string lastSfx;
        private SoundSource sfx, upSfx, downSfx;
        private Shaker shaker;


        public CustomMovingPlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, safe: false) {
            tempTexturePath = data.Attr("TexturePath", "default");
            moveType = data.Bool("PathType", false);
            if (tempTexturePath == "default" || tempTexturePath == "cliffside")
                tempTexturePath = "objects/woodPlatform/" + tempTexturePath;
            if (!moveType) { start = data.Position + offset; end = data.Nodes[0] + offset; } else { Position = offset - 20 * Vector2.One; }
            identifier = data.Attr("Identifier").Trim();
            speedMod = data.Float("SpeedMod", 1f);
            reverse = data.Bool("Reverse", false);
            uniform = data.Bool("UniformMovement", false);
            if (!VivHelper.TryGetEaser(data.Attr("EaseType", "SineInOut"), out EaseType)) { EaseType = Ease.SineInOut; }
            Add(sfx = new SoundSource());
            Add(downSfx = new SoundSource());
            Add(upSfx = new SoundSource());
            Add(shaker = new Shaker(on: false));
            SurfaceSoundIndex = 5;
            lineCols = new Color[] { VivHelper.OldColorFunction(data.Attr("InnerLineColor", "a4464a")), VivHelper.OldColorFunction(data.Attr("OuterLineColor", "2a1923")) };
            lastSfx = ((Math.Sign(start.X - end.X) > 0 || Math.Sign(start.Y - end.Y) > 0) ? "event:/game/03_resort/platform_horiz_left" : "event:/game/03_resort/platform_horiz_right");
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            MTexture mTexture = GFX.Game[tempTexturePath];
            textures = new MTexture[mTexture.Width / 8];
            for (int i = 0; i < textures.Length; i++) {
                textures[i] = mTexture.GetSubtexture(i * 8, 0, 8, 8);
            }
            Vector2 value = new Vector2(base.Width, base.Height + 4f) / 2f;
            if (!moveType)
                scene.Add(new MovingPlatformLine(start + value, end + value));
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Level level = SceneAs<Level>();
            if (moveType) {
                if (CurveEntity.curveEntities.Count > 0) {
                    if (!CurveEntity.curveEntities.ContainsKey(identifier)) { throw new Exception("No curve with the Identifier " + identifier + " was found."); }
                    curve = CurveEntity.curveEntities[identifier];
                    VivPathLine[] lines = new VivPathLine[]
                            {
                    new VivPathLine
                    {
                        distance = 2f,
                        startT = curve.bezier.tStart,
                        endT = curve.bezier.tEnd,
                        color = lineCols[1],
                        offset = Vector2.Zero,
                        thickness = 2.5f,
                        type = "line",
                        addEnds = true,
                        order = 1
                    },
                    new VivPathLine
                    {
                        distance = 0,
                        startT = curve.bezier.tStart,
                        endT = curve.bezier.tEnd,
                        color = lineCols[0],
                        offset = Vector2.Zero,
                        thickness = 1f,
                        type = "line",
                        addEnds = false,
                        order = 0
                    }
                    };
                    scene.Add(new CurvedPath(curve.bezier, lines));
                    Tween tween = Tween.Create(curve.spline ? Tween.TweenMode.Looping : Tween.TweenMode.YoyoLooping, EaseType, 2f / speedMod);
                    tween.OnUpdate = delegate (Tween t) {
                        float v = reverse ? 1 - t.Eased : t.Eased;
                        if (uniform) {
                            MoveTo(curve.bezier.GetPointFromLength(v * curve.bezier.GetBezierLength(10)) - (new Vector2(base.Width, base.Height + 4f) / 2f) + Vector2.UnitY * addY);
                        } else {
                            MoveTo(curve.bezier.GetPoint(v * (curve.getNumOfCurves() == 1 ? 1 : (curve.bezier.tEnd - curve.bezier.tStart))) - (new Vector2(base.Width, base.Height + 4f) / 2f) + Vector2.UnitY * addY);
                        }
                    };
                    tween.OnStart = delegate {
                        if (lastSfx == "event:/game/03_resort/platform_horiz_left") {
                            sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_right");
                        } else {
                            sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_left");
                        }
                    };
                    Add(tween);
                    tween.Start(reverse: false);
                    Add(new LightOcclude(0.2f));
                } else { throw new Exception("No CurveEntity in room."); }
            } else {
                Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, EaseType, 2f / speedMod);
                tween.OnUpdate = delegate (Tween t) {
                    MoveTo(Vector2.Lerp(start, end, t.Eased) + Vector2.UnitY * addY);
                };
                tween.OnStart = delegate {
                    if (lastSfx == "event:/game/03_resort/platform_horiz_left") {
                        sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_right");
                    } else {
                        sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_left");
                    }
                };
                Add(tween);
                tween.Start(reverse: false);
                Add(new LightOcclude(0.2f));
            }

        }




        public override void Render() {
            textures[0].Draw(Position);
            for (int i = 8; (float) i < base.Width - 8f; i += 8) {
                textures[1].Draw(Position + new Vector2(i, 0f));
            }
            textures[3].Draw(Position + new Vector2(base.Width - 8f, 0f));
            textures[2].Draw(Position + new Vector2(base.Width / 2f - 4f, 0f));
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            sinkTimer = 0.4f;
        }
        public override void Update() {
            base.Update();
            if (HasPlayerRider()) {
                sinkTimer = 0.2f;
                addY = Calc.Approach(addY, 2f, 50f * Engine.DeltaTime);
            } else if (sinkTimer > 0f) {
                sinkTimer -= Engine.DeltaTime;
                addY = Calc.Approach(addY, 2f, 50f * Engine.DeltaTime);
            } else {
                addY = Calc.Approach(addY, -1f, 20f * Engine.DeltaTime);
            }
        }
    }
}

