using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public static partial class VivHelper {

        public static Color Magic = new Color(1, 0, 0, 0);

        public static bool ParticleOverride(string input, out Color? output) {
            output = null;
            if (input == "discard" || input == "empty" || input == "ignore") {
                output = Magic;
                return true;
            }
            return false;
        }

        public static void ManageParticleOverride(ref ParticleType type, Color? color1, Color? color2) {
            // type needs to have at least 1 color. We assume we leave things "unchanged" if color is null though, so this assumes a default value for Color
            if (color1.HasValue && color1 != Magic)
                type.Color = color1.Value;
            if (color2.HasValue) {
                if (color2.Value == Magic)
                    type.ColorMode = ParticleType.ColorModes.Static;
                else
                    type.Color2 = color2.Value;
            }
        }
    }
}
