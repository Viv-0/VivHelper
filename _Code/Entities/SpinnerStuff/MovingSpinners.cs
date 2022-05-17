using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;


namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/MovingCustomSpinner")]
    public class MovingSpinner : CustomSpinner {
        public Vector2[] AllPoints; //NodesWithPosition
        public float MoveTime, WaitTime;
        public bool LoopType; // false = A B C B A, true = A B C A

        public MovingSpinner(EntityData e, Vector2 v) : base(e, v) {
            MoveTime = Math.Max(0.02f, e.Float("MoveTime", 0.4f));
            WaitTime = Math.Max(0f, e.Float("WaitTime", 0.2f));
            LoopType = e.Bool("LoopType", false);
            AllPoints = e.NodesWithPosition(v);
        }


    }
}
