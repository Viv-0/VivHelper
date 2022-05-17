using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using MonoMod.Utils;


namespace VivTestMod.Entities
{
    public class LongSpring : Spring
    {
        DynData<Spring> dyn;
        public LongSpring(EntityData data, Vector2 offset, Orientations orientation) : base(data, offset, orientation)
        {
            dyn = new DynData<Spring>(this);
            Remove(dyn.Get<Sprite>("sprite"));
            dyn.Get<StaticMover>("staticMover")
            
        }
    }
}
