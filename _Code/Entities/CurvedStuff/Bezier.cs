using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Monocle;

namespace VivHelper.Entities {
    public abstract class BezierObject {
        public int tStart, tEnd;
        public abstract override string ToString();
        public abstract Vector2 GetPoint(float t);
        public abstract Vector2 GetDerivative(float t);
        public virtual Vector2[] GetOffsetPoints(float distance, float t) {
            Vector2 dt = GetDerivative(t);
            Vector2 T = GetPoint(t);
            float q = (float) (Math.Sqrt(dt.X * dt.X + dt.Y * dt.Y));
            Vector2[] ret = new Vector2[]
            {
                new Vector2(T.X + (distance * dt.Y)/q,T.Y - (distance * dt.X)/q),
                new Vector2(T.X + (-1 * distance * dt.Y)/q,T.Y - (-1 * distance * dt.X)/q),
            };
            return ret;
        }
        public abstract void RenderLine(string type, float startT, float endT, Vector2 offset, int resolution, Color color, float thickness);
        public abstract void RenderPath(float distance, float startT, float endT, string type, Vector2 offset, int resolution, Color color, float thickness);

        public virtual float GetBezierLength() { return this.GetBezierLength(10); }
        public abstract float GetBezierLength(int resolution);

        public virtual Vector2 GetPointFromLength(float length) {
            float len = 0;
            float c = -0.04f;
            do {
                c += 0.04f;
                if (!(c > 1f)) {
                    len += Vector2.Distance(GetPoint(c + 0.04f), GetPoint(c));
                }
            } while (len <= length && !(c > 1f));
            if (c > 1f) { c = 1f; }
            do {
                c -= 0.0016f;
                len -= Vector2.Distance(GetPoint(c + 0.0016f), GetPoint(c));
            } while (len > length);
            return GetPoint(c);
        }
    }

    public class Bezier2 : BezierObject {
        public static Bezier2 end = new Bezier2(Vector2.Zero, Vector2.Zero, Vector2.Zero);
        public Vector2 point1, control, point2;
        public new int tStart = 0;
        public new int tEnd = 1;
        public static string formula = "(1-t)^2 * P1 + 2*(1-t)*t * C * t^2 * P2";

        public Bezier2(Vector2 start, Vector2 controlPoint, Vector2 end) {
            point1 = start;
            point2 = end;
            control = controlPoint;
        }

        public override string ToString() {
            return ("{ " + point1 + ", " + control + ", " + point2 + " }");
        }

        public override Vector2 GetPoint(float percent) {
            float X = (1 - percent) * (1 - percent) * point1.X + 2 * (1 - percent) * percent * control.X + percent * percent * point2.X;
            float Y = (1 - percent) * (1 - percent) * point1.Y + 2 * (1 - percent) * percent * control.Y + percent * percent * point2.Y;
            return new Vector2(X, Y);
        }

        public override Vector2 GetDerivative(float t) {
            float dX = 2 * (1 - t) * (control.X - point1.X) + 2 * t * (point2.X - control.X);
            float dY = 2 * (1 - t) * (control.Y - point1.Y) + 2 * t * (point2.Y - control.Y);
            return new Vector2(dX, dY);
        }

        public override float GetBezierLength(int resolution = 10) {
            if (resolution > 100)
                resolution = 100;
            float length = 0;
            for (int i = 0; i < resolution; i++) { length += Vector2.Distance(GetPoint((i + 1) / (float) resolution), GetPoint(i / (float) resolution)); }
            return length;
        }


        public void RenderLine(Vector2 offset = default, int resolution = 20, Color color = default, float thiccness = 1.2f) {
            if (offset == default)
                offset = Vector2.Zero;
            for (int i = 0; i < resolution; i++) {
                Draw.Line(GetPoint(i / resolution) + offset, GetPoint((i + 1) / resolution) + offset, (color != default ? color : Color.White), thiccness);
            }
        }
        public override void RenderLine(string type, float startPoint = 0, float endPoint = 1, Vector2 offset = default, int resolution = 20, Color color = default, float thiccness = 0f) {
            if (thiccness > 0) {
                if (offset == default)
                    offset = Vector2.Zero;
                int i = 0;
                while (i < resolution) {
                    if (type != "dotted") {
                        Draw.Line(GetPoint(startPoint + (endPoint - startPoint) * i / resolution) + offset, GetPoint(startPoint + (endPoint - startPoint) * (i + 1) / resolution) + offset, (color != default ? color : Color.White), thiccness);
                        i += type == "dashed" ? 2 : 1;
                    } else {
                        Draw.Circle(GetPoint(i / resolution) + offset, thiccness, color != default ? color : Color.White, resolution);
                        i += 2;
                    }
                }
            }
        }

        public override void RenderPath(float distance, float startPoint = 0, float endPoint = 1, string type = "line", Vector2 offset = default, int resolution = 20, Color color = default, float thiccness = 0f) {
            if (thiccness > 0) {
                if (offset == default)
                    offset = Vector2.Zero;
                int i = 0;
                while (i < resolution) {
                    Vector2[] v0 = GetOffsetPoints(distance, startPoint + (endPoint - startPoint) * i / resolution);
                    Vector2[] v1 = GetOffsetPoints(distance, startPoint + (endPoint - startPoint) * (i + 1) / resolution);
                    Draw.Line(v0[0] + offset, v1[0] + offset, (color != default ? color : Color.White), thiccness);
                    Draw.Line(v0[1] + offset, v1[1] + offset, (color != default ? color : Color.White), thiccness);
                    i += type == "dashed" ? 2 : 1;
                }
            }
        }
    }

    public class Bezier3 : BezierObject {
        public static Bezier3 end = new Bezier3(Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero);
        public Vector2 point1, point2, control1, control2;
        public new int tStart = 0;
        public new int tEnd = 1;
        public static string Formula = "(1-t)^3 * P1 + 3*(1-t)^2*t * C1 + 3*t^2*(1-t) * C2 + t^3 * P2";

        public Bezier3(Vector2 start, Vector2 controlStart, Vector2 controlEnd, Vector2 end) {
            point1 = start;
            point2 = end;
            control1 = controlStart;
            control2 = controlEnd;

        }

        public override string ToString() {
            return ("{ " + point1 + ", " + control1 + ", " + control2 + ", " + point2 + " }");
        }

        public override Vector2 GetPoint(float j) {
            float i = 1f - j;
            float x = i * i * i * point1.X + 3 * i * i * j * control1.X + 3 * i * j * j * control2.X + j * j * j * point2.X;
            float y = i * i * i * point1.Y + 3 * i * i * j * control1.Y + 3 * i * j * j * control2.Y + j * j * j * point2.Y;
            return new Vector2(x, y);
        }

        public static Vector2 GetPoint(float percent, Vector2 start, Vector2 controlStart, Vector2 controlEnd, Vector2 end) {
            Bezier3 b = new Bezier3(start, controlStart, controlEnd, end);
            return b.GetPoint(percent);
        }

        public override Vector2 GetDerivative(float t) {
            float dX = 3 * (1 - t) * (1 - t) * (control1.X - point1.X) +
                       6 * (1 - t) * t * (control2.X - control1.X) +
                       3 * t * t * (point2.X - control2.X);
            float dY = 3 * (1 - t) * (1 - t) * (control1.Y - point1.Y) +
                      6 * (1 - t) * t * (control2.Y - control1.Y) +
                      3 * t * t * (point2.Y - control2.Y);
            return new Vector2(dX, dY);
        }

        public override float GetBezierLength(int resolution = 10) {
            if (resolution > 100)
                resolution = 100;
            float length = 0;
            for (int i = 0; i < resolution; i++) { length += Vector2.Distance(GetPoint((i + 1) / (float) resolution), GetPoint(i / (float) resolution)); }
            return length;
        }

        public static float GetBezierLength(Vector2 start, Vector2 controlStart, Vector2 controlEnd, Vector2 end, int resolution = 10) {
            Bezier3 b = new Bezier3(start, controlStart, controlEnd, end);
            return b.GetBezierLength(resolution);
        }

        public void RenderLine(Vector2 offset = default, int resolution = 20, Color color = default, float thiccness = 1.2f) {
            if (thiccness > 0) {
                if (offset == default)
                    offset = Vector2.Zero;
                for (int i = 0; i < resolution; i++) {
                    Draw.Line(GetPoint(i / resolution) + offset, GetPoint((i + 1) / resolution) + offset, (color != default ? color : Color.White), thiccness);
                }
            }
        }

        public override void RenderLine(string type, float startPoint = 0, float endPoint = 1, Vector2 offset = default, int resolution = 20, Color color = default, float thiccness = 1.2f) {
            if (thiccness > 0) {
                if (offset == default)
                    offset = Vector2.Zero;
                int i = 0;
                while (i < resolution) {
                    if (type != "dotted") {
                        Draw.Line(GetPoint(startPoint + (endPoint - startPoint) * i / resolution) + offset, GetPoint(startPoint + (endPoint - startPoint) * (i + 1) / resolution) + offset, (color != default ? color : Color.White), thiccness);
                        i += type == "dashed" ? 2 : 1;
                    } else {
                        Draw.Circle(GetPoint(i / resolution) + offset, thiccness, color != default ? color : Color.White, resolution);
                        i += 2;
                    }
                }
            }
        }

        public override void RenderPath(float distance, float startPoint = 0, float endPoint = 1, string type = "line", Vector2 offset = default, int resolution = 20, Color color = default, float thiccness = 1.2f) {
            if (thiccness > 0) {
                if (offset == default)
                    offset = Vector2.Zero;
                int i = 0;
                while (i < resolution) {
                    Vector2[] v0 = GetOffsetPoints(distance, startPoint + (endPoint - 0.00001f - startPoint) * i / resolution);
                    Vector2[] v1 = GetOffsetPoints(distance, startPoint + (endPoint - 0.00001f - startPoint) * (i + 1) / resolution);
                    Draw.Line(v0[0] + offset, v1[0] + offset, (color != default ? color : Color.White), thiccness);
                    Draw.Line(v0[1] + offset, v1[1] + offset, (color != default ? color : Color.White), thiccness);
                    i += type == "dashed" ? 3 : 1;
                }
            }
        }

    }

    public class BezierSystem : BezierObject {
        public BezierObject[] curves;
        public new int tStart = 0;
        public new int tEnd;
        public bool single = false;

        public BezierSystem(BezierObject[] b) { curves = b; tEnd = curves.Length; single = tEnd == 1; }
        public override string ToString() {
            string t = "";
            foreach (BezierObject b in curves) { t += b.ToString() + "\n"; }
            return t;
        }

        public override Vector2 GetPoint(float t) {
            if (0 <= t && t <= tEnd) {
                if (single) { return curves[0].GetPoint(t); } else
                    return curves[(int) (t == tEnd ? t - 0.01f : t)].GetPoint(t == tEnd ? 1 : t - (int) t);
            } else { throw new IndexOutOfRangeException("Point Beyond Range"); }
        }

        public override Vector2 GetDerivative(float t) {
            if (0 <= t && t <= tEnd) {
                if (single) { return curves[0].GetDerivative(t); } else
                    return curves[(int) (t == tEnd ? t - 0.01f : t)].GetDerivative(t == tEnd ? 1 : t - (int) t);
            } else { throw new IndexOutOfRangeException("Point Beyond Range"); }
        }

        public override float GetBezierLength(int resolution = 10) {
            float len = 0;
            foreach (BezierObject b in curves) { len += b.GetBezierLength(resolution); }
            return len;
        }

        public override Vector2 GetPointFromLength(float length) {
            if (curves.Length == 1) { return curves[0].GetPointFromLength(length); }
            if (length > this.GetBezierLength(25)) { return curves[curves.Length - 1].GetPoint(1f); }
            float sum = 0f;
            for (int i = 0; i < curves.Length; i++) {
                sum += curves[i].GetBezierLength(25);
                if (sum > length) { sum -= curves[i].GetBezierLength(25); return curves[i].GetPointFromLength(length - sum); }
            }
            return curves[curves.Length - 1].GetPoint(1f);
        }

        public override Vector2[] GetOffsetPoints(float distance, float t) {
            if (single) { return curves[0].GetOffsetPoints(distance, t); }
            if (0 <= t && t <= tEnd) {
                return curves[(int) (t == tEnd ? t - 0.01f : t)].GetOffsetPoints(distance, t == tEnd ? 1 : t - (int) t);
            } else { throw new IndexOutOfRangeException("Point Beyond Range"); }
        }

        public override void RenderLine(string type, float startPoint = 0, float endPoint = -1, Vector2 offset = default, int resolution = 20, Color color = default, float thiccness = 1.2f) {
            if (thiccness > 0) {
                if (offset == default)
                    offset = Vector2.Zero;
                if (color == default)
                    color = Color.White;
                if (endPoint == -1)
                    endPoint = tEnd;
                if (tEnd == 1) {
                    curves[0].RenderLine(type, startPoint, endPoint < 1 ? endPoint : 1, offset, resolution, color, thiccness);
                } else {
                    for (int i = (int) startPoint; i < endPoint; i++) {
                        curves[i].RenderLine(type, (startPoint - i > 0 ? startPoint - i : 0), (endPoint - i < 1 ? endPoint - i : 1), offset, resolution, color, thiccness);
                    }
                }
            }
        }

        public override void RenderPath(float distance, float startPoint = 0, float endPoint = -1, string type = "line", Vector2 offset = default, int resolution = 20, Color color = default, float thiccness = 1.2f) {
            if (thiccness > 0) {
                if (offset == default)
                    offset = Vector2.Zero;
                if (color == default)
                    color = Color.White;
                if (endPoint == -1)
                    endPoint = tEnd;
                if (tEnd == 1) {
                    curves[0].RenderPath(distance, startPoint, endPoint < 1 ? endPoint : 1, type, offset, resolution, color, thiccness);
                }
                for (int i = (int) startPoint; i < endPoint; i++) {
                    curves[i].RenderPath(distance, (startPoint - i > 0 ? startPoint - i : 0), (endPoint - i < 1 ? endPoint - i : 1), type, offset, resolution, color, thiccness);
                }
            }
        }
    }
}
