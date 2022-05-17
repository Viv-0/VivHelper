using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    public struct VivPathLine {
        public int order; //the order is the location in the index. Higher numbers override previous lines.
        public Color color; //defaults to #a4464a
        public string type; //defaults to "line"
        public float startT, endT, thickness, distance;
        public Vector2 offset;
        public bool addEnds;
    }
}
