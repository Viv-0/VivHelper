using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;

namespace VivHelper.Entities
{
    /// <summary>
    /// The class used by CustomDashController to store all of the different custom dashes
    /// </summary>
    public class CustomDashStateCh
    {
        //The refill sprite name, must be in Graphics' Sprites.xml
        public string RefillSpriteName = null;
        //The booster sprite name, must be in Graphics' Sprites.xml
        public string BoosterName = null;

        //If not null, the dash will always go this direction (in radians)
        public float? ForceDashDirection = null;

        //Changes the duration of the dash. Doubles if SuperDashing is set to true
        public float DashDuration = 0.15f;
        //Changes the speed of the dash.
        public float DashSpeed = 240f;

        //Offset from the input dash direction (in radians). Ignored if ForceDashDirection is not null
        public float AngleOffset = 0;

        //SuperDashing is the effect caused by SuperDash Variant, which doubles the duration of the dash and adds the ability to steer the dash by 4deg/f (at normal gamerate)
        public bool SuperDashing = false;
        public float SuperDashControl = 4.18879032f; //in radians, 240deg*DeltaTime by default.

        //If true, if the player is in this custom dash state and hits a wall they will die.
        public bool FragileDash = false;

        //Disables fastbubble in customDashBoosters
        public bool DisableFastBubble = false;

        //See AddDashValue below
        public int? DashValue = null;
        //If false, If DashValue is null or <0, then it will act as normal, otherwise it will set the player's Dash Count to the Inventory Dashes
        //If true, If DashValue is null, it adds Inventory Dashes to the player's Dash Count, otherwise adds DashValue to the player's Dash Count
        public bool AddDashValue = false;

        //See AddStamValue below
        public float? StamValue = null;
        //If false, If StamValue is null or <0, then it will set the player's Stamina to 110f, otherwise it will set player Stamina to StamValue
        //If true, If StamValue is null, it adds 110 stamina to the player's stamina, otherwise adds StamValue to the player's Stamina
        public bool AddStamValue = false;
    }

    public static class CustomDash
    {
        public static int CustomDashState;

        public static void CustomDashBegin()
        {

        }
    }
}
