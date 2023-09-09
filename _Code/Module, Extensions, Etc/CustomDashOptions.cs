using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    // A rewrite of CustomDashStateCh to allow for far more customization
    public class CustomDashOptions {
        // Global factors
        public float FreezeTime = 0.05f; // How many seconds the game freezes for upon the dash being initialized

        // Movement Modifiers
        public float DashSpeed = 240f; // The speed in px/s the player moves. A good estimate is division by 60 to get px/frame
        
    }
}
