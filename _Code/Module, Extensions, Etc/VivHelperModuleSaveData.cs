using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod;
using YamlDotNet.Serialization;

namespace VivHelper {
    public class VivHelperModuleSaveData : EverestModuleSaveData {
        // Bronzes[SID|checkpointLevelName] = collected bronzeberry for that map at that checkpoint
        public HashSet<string> Bronzes = new HashSet<string>();
    }
}
