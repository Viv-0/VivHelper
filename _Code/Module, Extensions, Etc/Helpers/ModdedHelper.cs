using Celeste.Mod;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public static partial class VivHelper {

        internal static VirtualRenderTarget CustomLight = null;
        //Code from ColoursOfNoise
        public static bool TryGetModule(EverestModuleMetadata meta, out EverestModule module) {
            foreach (EverestModule other in Everest.Modules) {
                EverestModuleMetadata otherData = other.Metadata;
                if (otherData.Name != meta.Name)
                    continue;

                Version version = otherData.Version;
                if (Everest.Loader.VersionSatisfiesDependency(meta.Version, version)) {
                    module = other;
                    return true;
                }
            }

            module = null;
            return false;
        }

        private static MethodInfo Commands_UpdateClosed = typeof(Monocle.Commands).GetMethod("UpdateClosed", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void CommandOverride(string input) {
            if (Engine.Commands.Open) {
                Engine.Commands.Log(input);
            } else {
                Commands_UpdateClosed?.Invoke(Engine.Commands, Array.Empty<object>());
                Engine.Commands.Log(input + (Celeste.Celeste.PlayMode != Celeste.Celeste.PlayModes.Debug ? "Type q and press [ENTER] to exit." : ""));
            }
        }

    }
}
