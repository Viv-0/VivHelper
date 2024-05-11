using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public static partial class Consts {
        public const float PIover8 = MathF.PI / 8f;
        public const float PIover6 = MathF.PI / 6f;
        public const float PIover4 = MathF.PI / 4f;
        public const float PIover3 = MathF.PI / 3f;
        public const float PIover2 = MathF.PI / 2f;
        public const float PI = MathF.PI;
        public const float TAU = MathF.PI * 2f;
        internal static Vector2 DL = new Vector2(-1, 1);
        internal static Vector2 UR = new Vector2(1, -1);
    }

    public static partial class VivHelper {

        public static int mod(int x, int m) => (x % m + m) % m;
        public static float mod(float x, float m) => (x % m + m) % m;
        public static double mod(double x, double m) => (x % m + m) % m;
        public static long mod(long x, long m) => (x % m + m) % m;

        public static Vector2 mod(Vector2 x, Vector2 m) => new Vector2(mod(x.X, m.X), mod(x.Y, m.Y));

        public const double _12root2 = 1.05946309436;
        // Used for faster approximations of Math.Pow, mainly added because of its low error component within the range ideal for EventInstance::setPitch
        public static double approxPow(double a, double b) {
            int tmp = (int) (BitConverter.DoubleToInt64Bits(a) >> 32);
            int tmp2 = (int) (b * (tmp - 1072632447) + 1072632447);
            return BitConverter.Int64BitsToDouble(((long) tmp2) << 32);
        }

        public static Vector2 CleanUpVector(Vector2 v) {
            if (Math.Abs(v.X) < 0.01f)
                v.X = 0;
            if (Math.Abs(v.Y) < 0.01f)
                v.Y = 0;
            return v;
        }

        public static bool isPrime(int number) {
            if (number <= 1)
                return false;
            if (number == 2)
                return true;
            if (number % 2 == 0)
                return false;

            var boundary = (int) Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }
        public static int nearestPrime(int number, bool below) {
            while (true) {
                if (isPrime(number))
                    return number;
                number += below ? -1 : 1;
            }
        }
        
        public static float Determinant(Vector2 a, Vector2 b) => a.X * b.Y - b.X * a.Y;

        /// <summary>
        /// retrieves the number of on state values from any uint, in other words, the boolean 1s from the binary number
        /// </summary>
        /// <param name="u">number</param>
        /// <returns></returns>
        public static int Get1s(uint u) {
            uint uCount;

            uCount = (uint) (u - ((u >> 1) & 3681400539) - ((u >> 2) & 1227133513));
            return (int) ((uCount + (uCount >> 3)) & 3340530119) % 63;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int bitlog2(uint value) => System.Numerics.BitOperations.Log2(value);
        public static uint int2uint(int i) {
            FloatIntUnion u;
            u.u = 0;
            u.i = i;
            return u.u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct FloatIntUnion { // This works by defining 
            [FieldOffset(0)]
            public int i;
            [FieldOffset(0)]
            public float f;
            [FieldOffset(0)]
            public uint u;

            private FloatIntUnion(float f) {
                i = 0;
                u = 0;
                this.f = f;
            }

            private FloatIntUnion NextAfter(float y) {
                if (float.IsNaN(f) || float.IsNaN(y))
                    return new FloatIntUnion(f + y);
                if (f == y)
                    return new FloatIntUnion(y);  // nextafter(0, -0) = -0

                if (f == 0) {
                    i = 1;
                    return new FloatIntUnion(y > 0 ? f : -f);
                }
                if ((f > 0) == (y > f))
                    i++;
                else
                    i--;
                return new FloatIntUnion(f);
            }

            public static float FloatsDistanceAway(float x, int dist) {
                FloatIntUnion u = FloatsDistAway(VivHelper.NextAfter(x, dist > 0 ? x + 1 : x - 1), dist > 0 ? dist - 1 : dist + 1);
                return u.f;
            }

            private static FloatIntUnion FloatsDistAway(float f, int dist) => FloatsDistAway(new FloatIntUnion(f), dist);

            private static FloatIntUnion FloatsDistAway(FloatIntUnion u, int dist) {
                if (dist == 0)
                    return u;
                return FloatsDistAway(u.NextAfter(dist > 0 ? u.f + 1 : u.f - 1), dist > 0 ? dist - 1 : dist + 1);
            }
        }

        public static float NextAfter(float x, float y) {
            if (float.IsNaN(x) || float.IsNaN(y))
                return x + y;
            if (x == y)
                return y;  // nextafter(0, -0) = -0

            FloatIntUnion u;
            u.i = 0;
            u.f = x;  // shut up the compiler

            if (x == 0) {
                u.i = 1;
                return y > 0 ? u.f : -u.f;
            }
            if ((x > 0) == (y > x))
                u.i++;
            else
                u.i--;
            return u.f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FloatsDistanceAway(float x, int dist) { return FloatIntUnion.FloatsDistanceAway(x, dist); }

        // get the intersection of grid points that collide with a given rectangle.
        public static bool GridRectIntersection(Grid grid, Rectangle rect, out Grid ret, out Rectangle scope) {
            ret = null;
            scope = new Rectangle();
            if (!rect.Intersects(grid.Bounds))
                return false;
            int x = (int) (((float) rect.Left - grid.AbsoluteLeft) / grid.CellWidth);
            int y = (int) (((float) rect.Top - grid.AbsoluteTop) / grid.CellHeight);
            int width = (int) (((float) rect.Right - grid.AbsoluteLeft - 1f) / grid.CellWidth) - x + 1;
            int height = (int) (((float) rect.Bottom - grid.AbsoluteTop - 1f) / grid.CellHeight) - y + 1;
            if (x < 0) {
                width += x;
                x = 0;
            }
            if (y < 0) {
                height += y;
                y = 0;
            }
            if (x + width > grid.CellsX) {
                width = grid.CellsX - x;
            }
            if (y + height > grid.CellsY) {
                height = grid.CellsY - y;
            }
            bool[,] map = new bool[width, height];
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    map[i, j] = grid.Data[x + i, y + j];
                }
            }
            ret = new Grid(grid.CellWidth, grid.CellHeight, map);
            scope = new Rectangle((int)(x * grid.CellWidth + grid.AbsoluteLeft), (int)(y * grid.CellHeight + grid.AbsoluteTop), width, height);
            return true;
        }

        // TO-DO: Implement Rectilinear Decomposition - see https://github.com/mikolalysenko/rectangle-decomposition for reference

    }
}
