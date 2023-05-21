using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Microsoft.Xna.Framework;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace VivHelper {
    public class HoldablePlus : Holdable {

        public static FieldInfo onGround = typeof(Player).GetField("onGround", BindingFlags.NonPublic | BindingFlags.Instance);
        public static FieldInfo maxFall = typeof(Player).GetField("maxFall", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void Load() {
            using (new DetourContext { After = { "*" } }) {
                IL.Celeste.Player.NormalUpdate += Player_NormalUpdate;
                IL.Celeste.Player.Throw += Player_Throw;
            }
        }

        public static void Unload() {
            IL.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
            IL.Celeste.Player.Throw -= Player_Throw;
        }
        private static void Player_NormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            int x = 5;
            int y = 6;
            ILLabel label1 = null, label2 = null;
            if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(70), j => j.MatchStloc(out y) && j.Next.MatchBr(out label1)) &&
               cursor.TryGotoPrev(MoveType.After, i => i.MatchMul(), j => j.MatchStloc(out x))) { //This may explode with the release of Lunaris, but I do not have a test build to run with.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloca, x); // "num2" -> Xaccel
                cursor.Emit(OpCodes.Ldloca, y); // "num3" -> maxXSpeed
                cursor.Emit(OpCodes.Call, typeof(HoldablePlus).GetMethod("NormalUpdateX", BindingFlags.Public|BindingFlags.Static));
                cursor.Emit(OpCodes.Brtrue, label1); // Jumps to after all of the Holdable-specific variables have been cleared, if true, this "skips" over past all of the code we want to overwrite, so we can ship our own code and then skip.
            }
            while(cursor.TryGotoNext(MoveType.After, i=>i.MatchLdfld<Player>("maxFall"), j=>j.MatchLdloc(out int _))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Call, typeof(HoldablePlus).GetMethod("modifyMaxFall",BindingFlags.Public|BindingFlags.Static));
            }
        }

        private static void Player_Throw(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if(cursor.TryGotoNext(MoveType.Before, i=>i.MatchAdd())) { // Damn this is gonna break at some point
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Call, typeof(HoldablePlus).GetMethod("modifyRecoil", BindingFlags.Public | BindingFlags.Static));
            }
        }

        public static bool NormalUpdateX(Player player, ref float xAccel, ref float maxXspeed) {
            if (player.Holding is not HoldablePlus hold) { return false; } // do "normal behavior"
            xAccel *= (bool) onGround.GetValue(player) ? hold.xGroundAccelMult : hold.xAirAccelMult;
            maxXspeed *= hold.maxXRunMult;
            return true;
        }
        public static float modifyMaxFall(float @in, Player player) {
            if (player.Holding is not HoldablePlus plus)
                return @in;
            return @in * plus.maxFallMult;
        }
        public static float modifyRecoil(float @in, Player player) {
            if (player.Holding is not HoldablePlus plus)
                return @in;
            return @in * plus.recoilMult;
        }

        protected DynamicData holdableData;

        public float xGroundAccelMult = 1, xAirAccelMult = 1, maxXRunMult = 1;
        public float maxFallMult = 1;
        public float jumpMult = 1;
        public float recoilMult = 1; // Player.ThrowRecoil
        public bool? IgnoreSpace = null;


        public HoldablePlus(float cannotHoldTimer = 0.1f) : base(cannotHoldTimer) {
            holdableData = new DynamicData(typeof(Holdable), this);
        }

        public void SetCannotHoldTimer(float time) {
            holdableData.Set("cannotHoldTimer", time);
        }
    }
}
