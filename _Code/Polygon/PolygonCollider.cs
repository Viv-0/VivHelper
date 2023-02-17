using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VivHelper.Colliders {
    public class PolygonCollider : Collider {
        public static void Load() {
            On.Monocle.Collider.Collide_Collider += Collider_Collide_Collider;
        }
        public static void Unload() {
            On.Monocle.Collider.Collide_Collider += Collider_Collide_Collider;
        }

        private static bool Collider_Collide_Collider(On.Monocle.Collider.orig_Collide_Collider orig, Collider self, Collider collider) {
            if (collider is PolygonCollider shape) {
                if (self is Hitbox hitbox)
                    return shape.Collide(hitbox);
                else if (self is Circle circle)
                    return shape.Collide(circle);
                else
                    throw new Exception($"Collider not implemented between Polygon and {self.GetType().FullName}");
            } else if (self is PolygonCollider shape2) {
                if (collider is Hitbox hitbox)
                    return shape2.Collide(hitbox);
                else if (collider is Circle circle)
                    return shape2.Collide(circle);
                else
                    throw new Exception($"Collider not implemented between Polygon and {self.GetType().FullName}");
            } else {
                return orig(self, collider);
            }
        }

        public Vector2 offset;
        public bool convex;
        public readonly Vector2[] Points;
        public readonly Vector2[] TriangulatedPoints;
        public readonly int[] Indices;

        public Rectangle AABB; //Represents the AABB for the Polygon at the time of construction. To Update the Collider, you can effectively clone it.
        //Currently this is super slow but I intend to optimize it in the near future.

        public override float Width { get => AABB.Width; set => throw new NotImplementedException(); }
        public override float Height { get => AABB.Height; set => throw new NotImplementedException(); }
        public override float Top { get => AABB.Top - Entity.Position.Y; set => throw new NotImplementedException(); }
        public override float Bottom { get => AABB.Bottom - Entity.Position.Y; set => throw new NotImplementedException(); }
        public override float Left { get => AABB.Left - Entity.Position.X; set => throw new NotImplementedException(); }
        public override float Right { get => AABB.Right - Entity.Position.X; set => throw new NotImplementedException(); }

        private Vector2 _center;

        public new Vector2 Center { get => _center; set => throw new NotImplementedException(); }


        internal PolygonCollider(Vector2[] points, Vector2[] triangulatedpoints, int[] indices) {
            Array.Copy(points, Points, points.Length);
            Array.Copy(triangulatedpoints, TriangulatedPoints, triangulatedpoints.Length);
            Array.Copy(indices, Indices, indices.Length);
        }

        /// <summary>
        /// Creates a Collider out of the list of points input by the system. Supports Convex and Concave polygons but not Complex, or self-intersecting, Polygons.
        /// </summary>
        /// <param name="vectors">Put in the Vector2 of nodes, with the offset of the room in here</param>
        /// <param name="startPos">Put in the Entity</param>
        public PolygonCollider(Vector2[] vectors, Entity owner, bool setPositionAsCenter) {
            /// Defines the "center point" as the centroid of the given polygon. Currently, there is 0 check for centroid outside the convex hull which for sure means that the polygon is noncomplex.
            /// I believe the exact limit uses some definition for a concave hull, if someone wants to math this out and let me know go for it
            if (vectors.Length < 3)
                throw new ArgumentException($"Vector2 array contains {vectors.Length} points, which is less than the 3 minimum.");
            bool c = false;
            int sign = 0;
            float[] z = new float[4];
            _center = GetCentroidOfNonComplexPolygon(vectors);
            if (setPositionAsCenter)
                owner.Position = _center;
            convex = GetConvexity(vectors, ref z);
            offset = _center - owner.Position;
            AABB = new Rectangle((int) (offset.X + z[0]), (int) (offset.Y + z[2]), (int) (z[1] - z[0]), (int) (z[3] - z[2]));
            Points = vectors;
            Triangulator.Triangulator.Triangulate(vectors, Triangulator.WindingOrder.Clockwise, out TriangulatedPoints, out Indices);
        }

        internal static bool GetConvexity(Vector2[] _vertices, ref float[] z) {
            bool escape = _vertices.Length == 4;
            bool sign = false;
            int n = _vertices.Length;
            bool endvalue = true;
            z[0] = float.MaxValue;
            z[1] = float.MinValue;
            z[2] = float.MaxValue;
            z[3] = float.MinValue;
            for (int i = 0; i < n; i++) {
                Vector2 v = _vertices[i];
                if (v.X < z[0])
                    z[0] = v.X;
                else if (v.X > z[1])
                    z[1] = v.X;
                if (v.Y < z[2])
                    z[2] = v.Y;
                else if (v.Y > z[3])
                    z[3] = v.Y;
                if (!escape) {
                    Vector2 d1 = _vertices[(i + 2) % n] - _vertices[(i + 1) % n];
                    Vector2 d2 = v - _vertices[(i + 1) % n];
                    float zcrossproduct = d1.X * d2.Y - d1.Y * d2.X;

                    if (i == 0)
                        sign = zcrossproduct > 0;
                    else if (sign != (zcrossproduct > 0)) {
                        endvalue = false;
                        escape = true;
                    }
                }
            }
            return endvalue;
        }
        public override Collider Clone() {
            return new PolygonCollider(Points, TriangulatedPoints, Indices);
        }

        public override void Render(Camera camera, Color color) {
            Vector2[] q = Points;
            for (int i = 0; i < q.Length; i++) {
                Vector2 a = q[i];
                Vector2 b = q[(i + 1) % q.Length];
                if (Monocle.Collide.RectToLine(camera.Left, camera.Top, 320 * camera.Zoom, 180 * camera.Zoom, a, b))
                    Draw.Line(a, b, color);
            }
        }

        #region Collision Mechanisms

        public override bool Collide(Vector2 point) => Monocle.Collide.RectToPoint(this.AABB, point - Entity.Position) && PolygonPoint(point);

        public override bool Collide(Rectangle rect) {
            Vector2 v0;
            Vector2 v1;
            Vector2[] q = convex ? Points : TriangulatedPoints;
            for (int i = 0; i < q.Length; i++) {
                v0 = q[i];
                v1 = q[(i == q.Length - 1 ? 0 : i + 1)];
                if (Monocle.Collide.RectToLine(rect.Left, rect.Top, rect.Width, rect.Height, v0, v1))
                    return true;
            }
            return PolygonPoint(new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f));
        }

        public override bool Collide(Vector2 from, Vector2 to) {
            Vector2 v0;
            Vector2 v1;
            Vector2[] q = convex ? Points : TriangulatedPoints;

            for (int i = 0; i < q.Length; i++) {
                v0 = q[i];
                v1 = q[(i == q.Length - 1 ? 0 : i + 1)];
                if (Monocle.Collide.LineCheck(v0, v1, from, to))
                    return true;
            }
            return PolygonPoint(from);
        }

        public override bool Collide(Hitbox hitbox) => Collide(hitbox.Bounds);

        public override bool Collide(Grid grid) {
            throw new NotImplementedException("Collision with Grids are not functional.");
        }

        public override bool Collide(Circle circle) {
            Vector2 v0;
            Vector2 v1;
            Vector2[] q = convex ? Points : TriangulatedPoints;
            for (int i = 0; i < q.Length; i++) {
                v0 = Entity.Position + offset + q[i];
                v1 = Entity.Position + offset + q[(i == q.Length - 1 ? 0 : i + 1)];
                if (Monocle.Collide.CircleToLine(circle.Center - Position, circle.Radius, v0, v1))
                    return true;
            }
            return PolygonPoint(circle.Center);
        }

        public override bool Collide(ColliderList list) {
            if (list.colliders.Any(c => c is not Hitbox && c is not Circle && c is not ColliderList)) { throw new NotImplementedException("Collision with Grids are not functional."); }
            foreach (Collider c in list.colliders) {
                if (Collide(c)) { return true; }
            }
            return false;
        }

        public bool PolygonPoint(Vector2 point) {
            if (TriangulatedPoints.Length == 3)
                return PointInsideTriangle(Points[0], Points[1], Points[2], point);
            if (convex) {
                bool collision = false;
                for (int i = 0; i < Points.Length; i++) {
                    var vc = Points[i];
                    var vn = Points[(i + 1) % Points.Length];
                    if (((vc.Y >= point.Y && vn.Y < point.Y) || (vc.Y < point.Y && vn.Y >= point.Y)) &&
                        (point.X < (vn.X - vc.X) * (point.Y - vc.Y) / (vn.Y - vc.Y) + vc.X)) {
                        collision = !collision;
                    }
                }
                return collision;
            } else {
                for (int i = 0; i < Indices.Length; i += 3) {
                    if (PointInsideTriangle(TriangulatedPoints[Indices[i]], TriangulatedPoints[Indices[i + 1]], TriangulatedPoints[Indices[i + 2]], point))
                        return true;
                }
                return false;
            }
        }

        public static bool PointInsideTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 pt) {
            float areaOrig = Math.Abs((v2.X - v1.X) * (v3.Y - v1.Y) - (v3.X - v1.X) * (v2.Y - v1.Y));

            // get the area of 3 triangles made between the point
            // and the corners of the triangle
            float area1 = Math.Abs((v1.X - pt.X) * (v2.Y - pt.Y) - (v2.X - pt.X) * (v1.Y - pt.Y));
            float area2 = Math.Abs((v2.X - pt.X) * (v3.Y - pt.Y) - (v3.X - pt.X) * (v2.Y - pt.Y));
            float area3 = Math.Abs((v3.X - pt.X) * (v1.Y - pt.Y) - (v1.X - pt.X) * (v3.Y - pt.Y));

            // if the sum of the three areas equals the original,
            // we're inside the triangle!
            return Math.Abs(area1 + area2 + area3 - areaOrig) < 0.001f; //Wonky tolerance bug.
        }

        /// I couldn't get this bit to work. The math checks out so something must be going wrong with the floating point precision.
        /// If you're reading this from the future, feel free to bug me if you find the fix.
        /// code ref: https://www.gamedev.net/forums/topic/295943-is-this-a-better-point-in-triangle-test-2d/
        //internal static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) { return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y); }
        //public static bool TrianglePoint(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 pt) { bool b1, b2, b3; b1 = Sign(pt, v1, v2) < 0.0f; b2 = Sign(pt, v2, v3) < 0.0f; b3 = Sign(pt, v3, v1) < 0.0f; return ((b1 == b2) && (b2 == b3)); }
        #endregion


        public static Vector2 GetCentroidOfNonComplexPolygon(Vector2[] pts) {
            var points = new List<Vector2>(pts);
            Vector2 first = pts[0];
            Vector2 last = pts[pts.Length - 1];
            if (first != last) {
                points.Add(first);
            }
            float twicearea = 0;
            Vector2 v = Vector2.Zero;
            int nPts = points.Count;
            int i = 0;
            int j = nPts - 1;
            float f = 0;
            while (i < nPts) {
                var p = points[i];
                var q = points[j];
                f = (p.Y - first.Y) * (q.X - first.X) - (q.Y - first.Y) * (p.X - first.X);
                twicearea += f;
                v += (p + q - (2 * first)) * f;
                j = i++;
            }
            f = 0.33333f / twicearea;
            Vector2 ret = Calc.Round((v * f) + first);
            return ret;


        }

    }
}
