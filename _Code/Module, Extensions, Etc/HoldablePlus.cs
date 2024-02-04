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
using Celeste.Mod;

namespace VivHelper {
    public class HoldablePlus : Holdable {

        public static FieldInfo onGround = typeof(Player).GetField("onGround", BindingFlags.NonPublic | BindingFlags.Instance);
        public static FieldInfo maxFall = typeof(Player).GetField("maxFall", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void Load() {
            //pre-Core: using (new DetourContext { After = { "*" } }) {
            using (new DetourConfigContext(new DetourConfig("VivHelper", before: new[] { "*" })).Use()) {
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
            ILLabel label1 = null;
            // Handles climbing with HoldablePlus if canClimbWith is true
            if(!(cursor.TryGotoNext(MoveType.After, i => i.MatchLdarg(0), i=>i.MatchCallvirt<Player>("get_Holding")) && cursor.TryGotoNext(i=>i.MatchBrtrue(out _)))) {
                Logger.Log("VivHelper", "epic fail - error @ 1st subhook - 1st check");
                return;
            }
            ILCursor clone = cursor.Clone();
            if(!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0), i=>i.MatchCall<Entity>("get_Scene"), i => i.MatchCallvirt<Scene>("get_Tracker"))) {
                Logger.Log("VivHelper", "epic fail - error @ 1st subhook - 2nd check");
                return;
            }
            ILCursor clone2 = cursor.Clone();
            clone2.Index = cursor.Index; // assurance
            if (!clone2.TryGotoNext(i => i.MatchLeave(out label1))) {
                Logger.Log("VivHelper", "epic fail - error @ 1st subhook - 3rd check");
                return;
            }
            clone.Emit(OpCodes.Ldarg_0);
            clone.EmitDelegate<Func<Holdable, Player, Holdable>>((h, p) => h is HoldablePlus hp && (bool)hp.CanClimbWith(p) ? null : h);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<Player, bool>>(p => p.Holding is HoldablePlus hp && (bool) hp.CanClimbWith(p));
            cursor.Emit(OpCodes.Brtrue, label1);

            // Handles overwriting of `num2` and `num3` in source - modifies the X accel and max X speed of the Player in Normal Update
            ILLabel label3 = null, label2 = null;
            if(!(cursor.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),i=>i.MatchCallvirt<Player>("get_Ducking")) && cursor.TryGotoNext(MoveType.After, i=>i.MatchBrfalse(out label3)) &&
                 cursor.TryGotoNext(MoveType.After, i => i.MatchLdarg(0), i => i.MatchLdfld<Player>("onGround")) && cursor.TryGotoNext(MoveType.After, i => i.MatchBrfalse(label3)) && 
                 cursor.TryGotoNext(MoveType.After, i => i.MatchLdarg(0), i => i.MatchLdfld<Player>("level"), i => i.MatchLdfld<Level>("InSpace")) && cursor.TryGotoNext(MoveType.After, i=>i.MatchBrfalse(out label2)))) {
                Logger.Log("VivHelper", "epic fail - error @ 2nd subhook");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloca, 5); // "num2" -> Xaccel
            cursor.Emit(OpCodes.Ldloca, 6); // "num3" -> maxXSpeed
            cursor.Emit(OpCodes.Call, typeof(HoldablePlus).GetMethod("NormalUpdateX", BindingFlags.NonPublic | BindingFlags.Static));
            cursor.Emit(OpCodes.Brtrue, label3); // Jumps to after all of the Holdable-specific variables have been cleared, if true, this "skips" over past all of the code we want to overwrite, so we can ship our own code and then skip.
            
            while (cursor.TryGotoNext(MoveType.Before, i=>i.MatchStfld<Player>("maxFall"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Call, typeof(HoldablePlus).GetMethod("modifyMaxFall", BindingFlags.NonPublic | BindingFlags.Static));
                
            }
        }

        private static void Player_Throw(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if(cursor.TryGotoNext(i=>i.MatchLdarg(0), i => i.MatchLdstr("event:/char/madeline/crystaltheo_throw"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Call, typeof(HoldablePlus).GetMethod("appendThrow", BindingFlags.NonPublic | BindingFlags.Static));
            }
        }

        private static bool NormalUpdateX(Player player, ref float xAccel, ref float maxXspeed) {
            if (player.Holding is not HoldablePlus hold) { return false; } // do "normal behavior"
            float orig_xAccel = xAccel;
            float orig_maxXspeed = maxXspeed;
            hold.ModifyXSpeed(player, ref maxXspeed, ref xAccel);
            return orig_maxXspeed != maxXspeed || orig_xAccel != xAccel;
        }
        private static float modifyMaxFall(float @in, Player player) {
            if (player.Holding is not HoldablePlus plus) return @in;
            return @in * plus.maxFallMult;
        }
        private static void appendThrow(Player player) {

        }

        protected DynamicData holdableData;

        public float maxFallMult = 1;

        public HoldablePlus(float cannotHoldTimer = 0.1f) : base(cannotHoldTimer) {
            holdableData = new DynamicData(typeof(Holdable), this);
        }

        public void SetCannotHoldTimer(float time) {
            holdableData.Set("cannotHoldTimer", time);
        }

        /// <summary>
        /// A check to enable/disable the ability to start a climb on walls with this Holdable
        /// </summary>
        /// <param name="player">The player holding this Holdable</param>
        /// <returns>Whether or not to</returns>
        public virtual bool CanClimbWith(Player player) {
            return false;
        }

        /// <summary>
        /// This enables you to modify the speed and acceleration of the player while in "Normal State" (i.e. not dashing/feather/etc.)
        /// If you change either value, the default behavior of setting the X speed will not run for either one, so make sure you either set none or both.
        /// </summary>
        public virtual void ModifyXSpeed(Player player, ref float xSpeed, ref float xAccel) {}

        /// <summary>
        /// This enables you to modify the speed and acceleration of the player while in "Normal State" (i.e. not dashing/feather/etc.)
        /// If you change either value, the default behavior of setting the Y speed will not run for either one, so make sure you either set none or both,
        /// this also overrides the maxFall multiplier in HoldablePlus.
        /// </summary>
        public virtual void ModifyYSpeed(Player player, ref float maxFall, ref float yAccel) {}
    }
}
