using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using FMOD;
using System.Collections;

namespace VivHelper.Entities {
    [Tracked]
    public class TravelingFlame : Entity {


        public Vector2[] Nodes, CurvePoints;
        public string identifier;
        public int point, r1, r2;
        public float[] apothems;
        public int read;
        public int rotateType;
        public float alpha;
        public Sprite sprite;
        public Color color;
        public VertexLight vLight;
        public int currentNode;
        public float leaveDelay, speed;
        public bool onCycle;
        public float cycleDelay = 1f;
        public bool killCycle;
        protected Vector2 offset;
        public bool isActive;

        public TravelingFlame(EntityData data, Vector2 offset) : base(data.Position + offset) {
            List<Vector2> nodes = data.Nodes.ToList<Vector2>();
            this.offset = offset;
            nodes.Insert(0, data.Position);
            Nodes = nodes.ToArray();
            currentNode = 0;
            identifier = data.Attr("StringID");

            read = data.Int("CurveGen", -1);
            rotateType = data.Int("RotationType");
            color = VivHelper.ColorFix(data.Attr("ColorTint", "White").Trim());
            Add(sprite = VivHelperModule.spriteBank.Create("floatingFlame"));
            sprite.CenterOrigin();
            sprite.SetColor(color);
            r1 = data.Int("LightFadePoint", 48);
            r2 = data.Int("LightRadius", 64);
            alpha = data.Float("LightAlpha", 1f);
            leaveDelay = data.Float("Delay", 0.1f);
            speed = 1.6f * data.Float("SpeedMultiplier", 1f);
            if (speed == 0) { speed = 1.6f; }
            onCycle = data.Bool("onCycle", false);
            cycleDelay = data.Float("CycleDelay", 0f);
            killCycle = false;
            base.Depth = -9001;
        }

        public virtual void MoveToNextNode() { }
        public virtual void Lights(bool onoff) {
            Visible = onoff;
            vLight.Alpha = onoff ? 1 : 0;
        }
    }

    [TrackedAs(typeof(TravelingFlame))]
    [CustomEntity("VivHelper/TravelingFlame")]
    public class TravelingFlameAutoCurves : TravelingFlame {

        private static Dictionary<string, float> apothemVals = new Dictionary<string, float>
        {
            {"Very Shallow", 0.025f},
            {"Shallow", 0.075f},
            {"Normal", 0.15f},
            {"Deep", .3f},
            {"Very Deep", .4f},
            {"Radial", .5f },
            {"Wide", .6f },
            {"Very Wide", .75f },
        };
        public string DefaultCurveGenData;
        private SimpleCurve currentFlightCurve;
        private float currentFlightLerp;
        public TravelingFlameAutoCurves(EntityData data, Vector2 offset) : base(data, offset) { DefaultCurveGenData = data.Attr("CurveData"); }
        public override void Added(Scene scene) {
            base.Added(scene);
            isActive = false;
            Add(vLight = new VertexLight(color, alpha, r1, r2));
            vLight.InSolidAlphaMultiplier = 0.25f;
            CurvePoints = ApothemConvert(new string[] { DefaultCurveGenData }).ToArray();
            if (onCycle) {
                isActive = true;
                Add(new Coroutine(CycleSequence()));
            }
        }

        protected List<Vector2> ApothemConvert(string[] a) {
            List<Vector2> curvePoints = new List<Vector2>();
            bool b = false;
            for (int i = 0; i < Nodes.Length; i++) {
                string s = a.Length == 1 ? a[0] : a[i % a.Length];
                s.Trim();
                float length;
                if (apothemVals.Keys.Contains<string>(s)) { length = apothemVals[s]; } else { length = apothemVals[Calc.Choose(Calc.Random, apothemVals.Keys.ToArray<string>())]; }
                length *= Vector2.Distance(Nodes[(i + 1) % Nodes.Length], Nodes[i]);
                float angle = Calc.Angle(Nodes[(i + 1) % Nodes.Length], Nodes[i]);
                int sign = 90 * Math.Sign(angle - Calc.Angle(Nodes[(i + 2) % Nodes.Length], Nodes[i]));
                sign *= rotateType == 1 || b ? -1 : 1;
                if (rotateType == 0) { b = !b; }
                Vector2 midpoint = Vector2.Lerp(Nodes[(i + 1) % Nodes.Length], Nodes[i], .5f);
                Vector2 curvePoint = midpoint + new Vector2(length * (float) Math.Cos(angle + sign), length * (float) Math.Sin(angle + sign));
                curvePoints.Add(curvePoint);
            }
            return curvePoints;
        }
        //This is the 
        public override void MoveToNextNode() {
            isActive = true;
            currentFlightCurve = new SimpleCurve(Nodes[currentNode] + offset, Nodes[(currentNode + 1) % Nodes.Length] + offset, CurvePoints[currentNode] + offset);
            currentFlightLerp = 0f;
            Add(new Coroutine(NodeSequence()));
        }

        private IEnumerator NodeSequence() {
            vLight.StartRadius *= 1.5f;
            vLight.EndRadius *= 1.5f;
            yield return leaveDelay;
            currentNode++;
            currentNode %= Nodes.Length;
            while (currentFlightLerp < 1f) {
                currentFlightLerp = Calc.Approach(currentFlightLerp, 1f, speed * Engine.DeltaTime);
                Position = currentFlightCurve.GetPoint(Ease.SineInOut(currentFlightLerp));
                yield return null;
            }
            Position = currentFlightCurve.End;
            vLight.StartRadius /= 1.5f;
            vLight.EndRadius /= 1.5f;
            isActive = onCycle;
        }

        private IEnumerator CycleSequence() {
            while (!killCycle) {
                currentFlightCurve = new SimpleCurve(Nodes[currentNode] + offset, Nodes[(currentNode + 1) % Nodes.Length] + offset, CurvePoints[currentNode] + offset);
                currentFlightLerp = 0f;
                yield return NodeSequence();
                yield return cycleDelay;
            }
            RemoveSelf();
        }

    }

    [TrackedAs(typeof(TravelingFlame))]
    [CustomEntity("VivHelper/TravelingFlameCurve")]
    public class TravelingFlameSpecificCurves : TravelingFlame {
        private float currentCurveT;
        public string curveIdentifier;
        protected CurveEntity curve;

        public TravelingFlameSpecificCurves(EntityData data, Vector2 offset) : base(data, offset) {
            curveIdentifier = data.Attr("CurveID", "");
            currentCurveT = 0f;
            currentNode = 0;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Add(vLight = new VertexLight(color, alpha, r1, r2));
            vLight.InSolidAlphaMultiplier = 1f;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            isActive = false;
            List<Entity> pe = SceneAs<Level>().Tracker.GetEntities<CurveEntity>();
            if (pe.Count > 0) {
                foreach (CurveEntity p in pe) {
                    if (p.identifier == identifier) { curve = p; break; }
                }
                if (curve == null) { throw new Exception("No curve with the Identifier " + identifier + " was found."); }
                if (onCycle) {
                    isActive = true;
                    Add(new Coroutine(CycleSequence()));
                }
            } else { throw new Exception("No CurveEntity in room."); }
        }

        public override void MoveToNextNode() {
            isActive = true;
            Add(new Coroutine(NodeSequence()));
        }


        private IEnumerator CycleSequence() {
            while (!killCycle) {
                yield return NodeSequence();
                yield return cycleDelay;
            }
            RemoveSelf();
        }

        private IEnumerator NodeSequence() {
            yield return null;
            isActive = onCycle;
        }

    }
}
