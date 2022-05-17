
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {
    [Tracked]
    public class CurvedPath : Entity {
        public float startPoint, endPoint;
        public int resolution;
        public BezierSystem bezierObject;
        public VivPathLine[] lines;

        public CurvedPath(BezierSystem bezier, int numOfLines, int resolution, float[] distances, Color[] colors, string[] types, float[] thicknesses, Vector2[] offsets, float[] startPoints, float[] endPoints, bool[] addEnds) : base(bezier.GetPoint(0)) {
            this.resolution = resolution;
            lines = new VivPathLine[numOfLines];
            for (int i = 0; i < numOfLines; i++) {
                lines[i] = new VivPathLine {
                    distance = distances[i],
                    color = colors[i],
                    type = types[i],
                    thickness = thicknesses[i],
                    startT = startPoints[i],
                    endT = endPoints[i],
                    offset = offsets[i],
                    addEnds = addEnds[i],
                    order = i
                };
            }
            this.bezierObject = bezier;
            base.Depth = 9001;
        }
        public CurvedPath(BezierSystem bezier, VivPathLine[] paths, int resolution = 20) : base(bezier.GetPoint(0)) {
            bezierObject = bezier;
            lines = paths;
            this.resolution = resolution;
            base.Depth = 9001;
        }

        public override void Render() {
            for (int i = 0; i < lines.Length; i++) {
                VivPathLine vpl = lines[i];
                if (vpl.distance == 0) {
                    if (bezierObject.single)
                        bezierObject.curves[0].RenderLine(vpl.type, vpl.startT, vpl.endT, vpl.offset, resolution, vpl.color, vpl.thickness);
                    else
                        bezierObject.RenderLine(vpl.type, vpl.startT, vpl.endT, vpl.offset, resolution, vpl.color, vpl.thickness);
                } else {
                    if (bezierObject.single)
                        bezierObject.curves[0].RenderPath(vpl.distance, vpl.startT, vpl.endT, vpl.type, vpl.offset, resolution, vpl.color, vpl.thickness);
                    else
                        bezierObject.RenderPath(vpl.distance, vpl.startT, vpl.endT, vpl.type, vpl.offset, resolution, vpl.color, vpl.thickness);
                    if (vpl.addEnds) {
                        Vector2[] v = bezierObject.GetOffsetPoints(vpl.distance, vpl.startT);
                        Draw.Line(v[0], v[1], vpl.color);
                        v = bezierObject.GetOffsetPoints(vpl.distance, vpl.endT);
                        Draw.Line(v[0], v[1], vpl.color);
                    }
                }
            }
        }




    }
}
