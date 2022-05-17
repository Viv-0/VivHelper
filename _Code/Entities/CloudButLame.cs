using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;

namespace VivTestMod.Entities
{
    [CustomEntity("VivTest1/CloudButLame")]
    class CloudButLame : Entit
    {
        public 
        public CloudButLame(EntityData data, Vector2 offset) : base(data, offset)
        {

        }

        public override void Awake(Scene scene) { base.Awake(scene); }

        
    }
}
