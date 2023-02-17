using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Security.Cryptography.X509Certificates;
using System.Collections;

namespace VivHelper.Entities {
    //This class is just a wrapper for a Bezier System, but also creates a BezierSystem in a second, more useful way.
    [Tracked(true)]
    [CustomEntity("VivHelper/CurveEntity")]
    public class CurveEntity : Entity {
        public BezierSystem bezier;
        public int pointsLength;
        public bool spline;
        public string identifier;
        public int tStart, tEnd;

        public static Dictionary<string, CurveEntity> curveEntities = new Dictionary<string, CurveEntity>();

        public CurveEntity(EntityData data, Vector2 offset) : base(data.NodesWithPosition(offset)[0]) {
            base.Depth = 1000000;
            string degreeString = data.Attr("CurvesNumberOfPoints", "Automatic");
            spline = data.Bool("Spline", false);
            identifier = data.Attr("Identifier", "").Trim();
            Console.WriteLine(identifier);
            Vector2[] points = data.NodesWithPosition(offset);
            pointsLength = points.Length;
            if (pointsLength < 3) { throw new Exception("Not enough points to produce a curve."); } else if (pointsLength == 3) { if (spline) throw new Exception("Not enough points to produce a spline."); else bezier = new BezierSystem(new Bezier2[] { new Bezier2(points[0], points[1], points[2]) }); } else if (pointsLength == 4) {
                if (spline) {
                    List<BezierObject> l = new List<BezierObject>();
                    l.Add(new Bezier2(points[0], points[1], points[2]));
                    l.Add(new Bezier2(points[2], points[3], points[0]));
                    bezier = new BezierSystem(l.ToArray());
                } else {
                    List<BezierObject> l = new List<BezierObject>();
                    l.Add(new Bezier3(points[0], points[1], points[2], points[3]));
                    bezier = new BezierSystem(l.ToArray());
                }
            } else {
                switch (degreeString) {
                    case "All Simple":
                        //you want Even number if you are using Spline, otherwise odd.
                        if (pointsLength % 2 == (spline ? 1 : 0)) { throw new Exception("Number of Points do not add up to the necessary amount. If the following says \"true\" then you may want to disable Spline.\t" + spline.ToString()); } else {
                            List<BezierObject> bezier2s = new List<BezierObject>();
                            for (int i = 0; i < pointsLength; i += 2) {
                                bezier2s.Add(new Bezier2(points[i], points[i + 1], points[(i + 2) % pointsLength]));
                            }
                            bezier = new BezierSystem(bezier2s.ToArray());
                        }
                        break;
                    case "All Cubic":
                        //You want Threeven number if you're using Spline, otherwise you want the 1st ring. (n % 3 = 1)
                        if (!(pointsLength % 3 == (spline ? 0 : 1))) { throw new Exception("Number of Points do not add up to the necessary amount. If the following says \"true\" then you may want to disable Spline.\t" + spline.ToString()); } else {
                            BezierObject[] bezier3s = new BezierObject[pointsLength / 3];
                            for (int i = 0; i < pointsLength; i += 3) {
                                bezier3s[i / 3] = new Bezier3(points[i], points[i + 1], points[i + 2], points[(i + 3) % pointsLength]);
                            }
                            bezier = new BezierSystem(bezier3s);
                        }
                        break;
                    default:
                        if (degreeString == "Automatic") {
                            int q = pointsLength - (spline ? 0 : 1);
                            List<char> C = new List<char>();
                            int c = 0;
                            while (q >= 2) {

                                if (q == 3) { C.Add('3'); } else { C.Add('2'); }
                                q -= 2;
                                c += 1;
                            }
                            degreeString = new string(C.ToArray());
                        }
                        char[] chars = degreeString.ToCharArray();
                        List<int> Degrees = new List<int>();
                        foreach (char c in chars) { Degrees.Add(int.Parse(c.ToString())); }
                        List<BezierObject> beziers = new List<BezierObject>();
                        int count = 0;
                        foreach (int i in Degrees) {
                            if (i == 2) { beziers.Add(new Bezier2(points[count], points[count + 1], points[(count + 2) % pointsLength])); } else if (i == 3) { beziers.Add(new Bezier3(points[count], points[count + 1], points[count + 2], points[(count + 3) % pointsLength])); count += 1; } else { throw new Exception("Invalid Variable: Check your CurvesNumberOfPoints variable."); }
                            count += 2;
                        }
                        bezier = new BezierSystem(beziers.ToArray());
                        break;
                }
                tStart = bezier.tStart;
                tEnd = bezier.tEnd;
            }



        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (curveEntities.ContainsKey(identifier)) { curveEntities.Remove(identifier); }
            curveEntities[identifier] = this;
        }


        public override void Removed(Scene scene) {
            curveEntities.Remove(this.identifier);
            base.Removed(scene);

        }

        public int getNumOfCurves() { return bezier.curves.Length; }

        public static void GetCurveTotalLength(string Identifier) {
            if (curveEntities.Count > 0) {
                if (curveEntities.ContainsKey(Identifier)) { Engine.Commands.Log("Length: " + curveEntities[Identifier].bezier.GetBezierLength(25).ToString()); return; }
                Engine.Commands.Log("It seems there is no curve with Identifier" + Identifier + ". Try again with a different identifier.");
                return;
            } else { Engine.Commands.Log("There are no curves currently loaded."); return; }
        }

        public static void GetCurveIDs() {
            Engine.Commands.Log(curveEntities.Keys.ToArray<string>().ToString());
        }
    }
}
