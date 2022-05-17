using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace VivHelper
{ 
    public class RespriteBank
    {
        private Atlas Atlas;
        public string XMLPath;
        private Dictionary<string, RespriteData> RespriteData;

        public RespriteBank(Atlas atlas, string xmlPath)
        {
            Atlas = atlas;
            XMLPath = xmlPath;
            
        }
    }
}
