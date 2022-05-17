using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;

namespace VivHelper.Entities.SeekerStuff {
    [CustomEntity("VivHelper/CustomSeekerController")]
    public class CustomSeekerController : Entity {
        public Level level;
        public List<CustomSeeker> seekers;
        public List<CustomSeekerGenerator> generators;
    }
}
