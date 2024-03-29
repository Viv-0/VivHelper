﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public static partial class VivHelper {
        public static int mod(int x, int m) => (x % m + m) % m;
        public static float mod(float x, float m) => (x % m + m) % m;
        public static double mod(double x, double m) => (x % m + m) % m;
        public static long mod(long x, long m) => (x % m + m) % m;

        public static Vector2 mod(Vector2 x, Vector2 m) => new Vector2(mod(x.X, m.X), mod(x.Y, m.Y));

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


        // I would argue that Geometry is math
        public static bool RectToRect(Rectangle a, Rectangle b) {
            if (a.Right > b.Left && a.Bottom > b.Top && a.Left < b.Right) {
                return a.Top < b.Bottom;
            }
            return false;
        }
    }
}
