using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public static partial class VivHelper {
        public static MethodInfo player_WallJumpCheck;
        public static MethodInfo player_DashCorrectCheck;
        public static MethodInfo player_DreamDashCheck;
        public static MethodInfo player_SuperJump;
        public static MethodInfo player_SuperWallJump;
        public static MethodInfo player_ClimbJump;
        public static MethodInfo player_WallJump;
        public static MethodInfo player_Pickup;
        public static MethodInfo player_DustParticleFromSurfaceIndex;
        public static MethodInfo player_IsTired;
        private static MethodInfo Player_DashBegin, Player_DashEnd;
        private static MethodInfo Player_DashUpdate;
        private static MethodInfo Player_DashCoroutine;
        public static FieldInfo player_jumpGraceTimer;
        public static FieldInfo player_onGround;
        public static FieldInfo player_wallSpeedRetained;
        public static FieldInfo player_demoDashed;
        public static FieldInfo player_boostTarget;
        public static FieldInfo player_lastAim;

        public static void CreateFastDelegates() {
            //This is always called *after* playerWallJump is defined.
            player_WallJumpCheck = VivHelperModule.playerWallJump;
            player_WallJump = typeof(Player).GetMethod("WallJump", BindingFlags.Instance | BindingFlags.NonPublic);
            player_ClimbJump = typeof(Player).GetMethod("ClimbJump", BindingFlags.Instance | BindingFlags.NonPublic);
            player_DashCorrectCheck = typeof(Player).GetMethod("DashCorrectCheck", BindingFlags.Instance | BindingFlags.NonPublic);
            player_DreamDashCheck = typeof(Player).GetMethod("DreamDashCheck", BindingFlags.Instance | BindingFlags.NonPublic);
            player_SuperJump = typeof(Player).GetMethod("SuperJump", BindingFlags.Instance | BindingFlags.NonPublic);
            player_SuperWallJump = typeof(Player).GetMethod("SuperWallJump", BindingFlags.Instance | BindingFlags.NonPublic);
            player_Pickup = typeof(Player).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.NonPublic);
            player_DustParticleFromSurfaceIndex = typeof(Player).GetMethod("DustParticleFromSurfaceIndex", BindingFlags.Instance | BindingFlags.NonPublic);
            player_IsTired = typeof(Player).GetMethod("get_IsTired", BindingFlags.Instance | BindingFlags.NonPublic);
            EntityList_toAwake = typeof(EntityList).GetField("toAwake", BindingFlags.Instance | BindingFlags.NonPublic);
            player_onGround = typeof(Player).GetField("onGround", BindingFlags.Instance | BindingFlags.NonPublic);
            player_wallSpeedRetained = typeof(Player).GetField("wallSpeedRetained", BindingFlags.Instance | BindingFlags.NonPublic);
            player_demoDashed = typeof(Player).GetField("demoDashed", BindingFlags.Instance | BindingFlags.NonPublic);
            player_boostTarget = typeof(Player).GetField("boostTarget", BindingFlags.Instance | BindingFlags.NonPublic);
            player_lastAim = typeof(Player).GetField("lastAim", BindingFlags.Instance | BindingFlags.NonPublic);
            player_jumpGraceTimer = typeof(Player).GetField("jumpGraceTimer", BindingFlags.Instance | BindingFlags.NonPublic);
            Player_DashBegin = typeof(Player).GetMethod("DashBegin", BindingFlags.Instance | BindingFlags.NonPublic);
            Player_DashEnd = typeof(Player).GetMethod("DashEnd", BindingFlags.Instance | BindingFlags.NonPublic);
            Player_DashUpdate = typeof(Player).GetMethod("DashUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            Player_DashCoroutine = typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
        }
        public static Player GetPlayer() {
            return Engine.Scene?.Tracker?.GetEntity<Player>();
        }
        //Thanks to coloursofnoise for this code.
        public static bool TryGetPlayer(out Player player) {
            player = Engine.Scene?.Tracker?.GetEntity<Player>();
            return player != null;
        }
        public static bool TryGetAlivePlayer(out Player player) {
            player = Engine.Scene?.Tracker?.GetEntity<Player>();
            return !(player?.Dead ?? true);
        }

        private static FieldInfo player_triggersInside = typeof(Player).GetField("triggersInside", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void PlayerTriggerCheck(Player player, Trigger trigger) {
            if (!trigger.Triggered) {
                trigger.Triggered = true;
                ((HashSet<Trigger>) player_triggersInside.GetValue(player)).Add(trigger);
                trigger.OnEnter(player);
            }
            trigger.OnStay(player);
        }

        private static string[] manualStates = new string[26] {
            "Normal", "Climb", "Dash", "Swim", "Boost", "RedDash", "HitSquash", "Launch", "Pickup", "DreamDash", "SummitLaunch", "Dummy",
            "IntroWalk", "IntroJump", "IntroRespawn", "IntroWakeUp", "BirdDashTutorial", "Frozen", "ReflectionFall", "StarFly",
            "TempleFall", "CassetteFly", "Attract", "IntroMoonJump", "FlingBird", "IntroThinkForABit"
        };

        public static bool DefineSetState(string input, out int output) {
            if (int.TryParse(input, out output)) {
                if (output < 0)
                    return false;
                if (output > 25)
                    throw new InvalidPropertyException("You tried to put in a custom state value over 25. I recommend retrieving the classname or using the default values provided in the dropdown.");
                return true;
            }
            output = Array.IndexOf(manualStates, input);
            if (output >= 0)
                return true;
            output = Array.IndexOf(manualStates, "St" + input);
            if (output >= 0)
                return true;
            List<string> subset = input.Split('.').ToList();
            if (subset.Count < 2)
                throw new InvalidPropertyException("You input an invalid custom state.");
            string fieldName = subset.Last();
            subset.RemoveAt(subset.Count - 1);
            string temp1 = string.Join(".", subset); //This is everything but the fieldname, which should be the classname path, Array Resizing was slower in this case.
            if (!VivHelper.TryGetType(temp1, out Type type))
                throw new InvalidPropertyException("The custom state class path was invalid!");
            FieldInfo field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic); //If its nonpublic this would make me angy.
            if (field == null)
                throw new InvalidPropertyException("The custom state field name was invalid!");
            if (field.FieldType != typeof(int))
                throw new InvalidPropertyException("The custom state field was found but not shown to be a valid field (it is not an integer)");
            output = (int) field.GetValue(null);
            return true;
        }


        internal static void _DashBegin(this Player player) => Player_DashBegin.Invoke(player, Array.Empty<object>()); 
        internal static void _DashEnd(this Player player) => Player_DashEnd.Invoke(player, Array.Empty<object>()); 

        internal static int _DashUpdate(this Player player) => (int)Player_DashUpdate.Invoke(player, Array.Empty<object>());
        internal static IEnumerator _DashCoroutine(this Player player) { yield return new SwapImmediately((IEnumerator)Player_DashCoroutine.Invoke(player, Array.Empty<object>())); }
        internal static Vector2 CorrectDashPrecision(Vector2 dir) {
            if (dir.X != 0f && Math.Abs(dir.X) < 0.001f) {
                dir.X = 0f;
                dir.Y = Math.Sign(dir.Y);
            } else if (dir.Y != 0f && Math.Abs(dir.Y) < 0.001f) {
                dir.Y = 0f;
                dir.X = Math.Sign(dir.X);
            }
            return dir;
        }

    }
}
