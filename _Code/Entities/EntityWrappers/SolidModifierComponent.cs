using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using Mono.Cecil;
using System.Runtime.InteropServices.ComTypes;

namespace VivHelper {
    [Tracked]
    public class SolidModifierComponent : Component {

        private readonly static string[] LegacyMapSIDs = new string[10] {
            "MOCE/issy/MOCE_CPCL", "MOCE/issy/MOCE_LXVI",
            "Tardigrade/WaterbearMountain/WaterbearMountain",
            "Cabob/AmberTerminus/1-AmberTerminus", "Cabob/AmberTerminus/1-AmberTerminus-B",
            "AnarchyCollab2022/1-Submissions/02_ellatas", "AnarchyCollab2022/1-Submissions/22_pogseal", "AnarchyCollab2022/1-Submissions/ZZ_heartside",
            "WinterCollab2021/1-Maps/Jackal", "WinterCollab2021/1-Maps/Jackal_KAERRA" };

#region Hooks
        public static void Load() {
            On.Celeste.Player.IsRiding_Solid += Player_IsRiding_Solid;
            On.Celeste.Player.ClimbJump += Player_ClimbJump;
            On.Celeste.Player.WallJump += Player_WallJump;
            On.Celeste.Player.Update += Player_Update;
            IL.Celeste.Solid.GetPlayerClimbing += Solid_GetPlayerClimbing;
            using (new DetourContext { After = { "MaxHelpingHand" } }) IL.Celeste.Player.WallJumpCheck += Player_WallJumpCheck;
            On.Monocle.StateMachine.Update += StateMachine_Update;
        }

        public static void Unload() {
            On.Celeste.Player.IsRiding_Solid -= Player_IsRiding_Solid;
            On.Celeste.Player.ClimbJump -= Player_ClimbJump;
            On.Celeste.Player.Update -= Player_Update;
            IL.Celeste.Solid.GetPlayerClimbing -= Solid_GetPlayerClimbing;
            IL.Celeste.Player.WallJumpCheck -= Player_WallJumpCheck;
            On.Monocle.StateMachine.Update -= StateMachine_Update;
        }

        private static void StateMachine_Update(On.Monocle.StateMachine.orig_Update orig, StateMachine self) {
            orig(self); // currentActiveSolidModifier should be gotten within the StateMachine's codebase, i.e. ClimbJump/WallJump/etc., so we nullify it afterwards.
            if (self.Entity is Player player)
                VivHelperModule.Session.currentActiveSolidModifier = null;
        }

        private static void Solid_GetPlayerClimbing(ILContext il) {
            ILCursor cursor = new(il);
            ILLabel l = null;
            if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(1), i => i.MatchStloc(2), i => i.MatchLeave(out _) && i.Next.MatchLdloca(1))) {
                cursor.MarkLabel(l);
                cursor.GotoNext(MoveType.After, i => i.MatchLeave(out _) && i.Next.MatchLdloca(1));
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.Emit(OpCodes.Call, typeof(SolidModifierComponent).GetMethod("GetPlayerClimbingExtension"));
                cursor.Emit(OpCodes.Brtrue, l);
            }
        }

        private static bool Player_IsRiding_Solid(On.Celeste.Player.orig_IsRiding_Solid orig, Player self, Solid solid) {
            if (orig(self, solid)) return true;
            SolidModifierComponent solidModifierComponent = solid.Get<SolidModifierComponent>();
            if (solidModifierComponent != null) {
                if (solidModifierComponent.bufferClimbJump && solidModifierComponent.HasBeenBufferJumpedOn) {
                    solidModifierComponent.HasBeenBufferJumpedOn = false;
                    return true;
                }
                return (solidModifierComponent.triggerClimbOnTouch && self.CollideCheck(solid, self.Position + Vector2.UnitX * (float) self.Facing)) ||
                       (solidModifierComponent.OnTouchFromBelow && self.CollideCheck(solid, self.Position - Vector2.UnitY));
            }
            return false;
        }

        private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            if (VivHelperModule.Session.currentActiveSolidModifier == null) {
                orig.Invoke(self);
                return;
            }
            VivHelperModule.Session.currentActiveSolidModifier.HasBeenBufferJumpedOn = true;
            orig.Invoke(self);
            if (VivHelperModule.Session.currentActiveSolidModifier.CornerBoostBlock == -2 || VivHelperModule.Session.currentActiveSolidModifier.CornerBoostRetention) {
                DynamicData.For((object) self).Set("wallSpeedRetained", (object) self.Speed.X);
            }
            VivHelperModule.Session.currentActiveSolidModifier = null;
        }

        private static void Player_WallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
            if (VivHelperModule.Session.currentActiveSolidModifier != null) {
                VivHelperModule.Session.currentActiveSolidModifier.HasBeenBufferJumpedOn = true;
                VivHelperModule.Session.currentActiveSolidModifier = null;
            }
            orig.Invoke(self, dir);
        }
        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self) {
            if (self.Scene == null)
                return;
            if (self.Scene.Tracker.Components.TryGetValue(typeof(SolidModifierComponent), out var q))
                foreach (SolidModifierComponent smc in q)
                    smc.HasBeenBufferJumpedOn = false;
            orig(self);

        }

        //I'm rewriting this whole fucking thing, i'm sick of this shit code
        private static void Player_WallJumpCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            VariableDefinition varDef = new VariableDefinition(il.Import(typeof(Vector2)));
            il.Body.Variables.Add(varDef);
            if (cursor.TryGotoNext(MoveType.Before, i=>(i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) && (i.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)")) {
                cursor.Emit(OpCodes.Stloc, varDef);
                cursor.Emit(OpCodes.Ldloc, varDef);
                cursor.GotoNext(MoveType.After, i => (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) && (i.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Ldloc, varDef);
                cursor.Emit(OpCodes.Call, typeof(SolidModifierComponent).GetMethod("ModifyWallJumpCheck"));
            }
        }
#endregion
        public static bool ModifyWallJumpCheck(bool prev, Player player, int dir, Vector2 legacyAt) { // We want this to be relatively "variable" 
            if (!player.Scene.Tracker.Components.TryGetValue(typeof(SolidModifierComponent), out var q) || q.Count == 0)
                return false; // Counting the components list is faster than counting the Solids list because they should always be attached to a solid
            if (LegacyMapSIDs.Contains(player.SceneAs<Level>().Session.Area.SID)) { // Legacy code, this is literally copy-pasted you cannot tell me this is broken
                if (prev) return true; // We can confirm that previous works here, but not in the general case.
                bool a = false;
                Vector2 position = player.Position;
                player.Position = legacyAt;
                foreach(var smc in q) {
                    if (smc.Entity is Solid solid) {
                        a |= LegacyCollideCheck(solid, player, (smc as SolidModifierComponent).CornerBoostBlock);
                        if (a)
                            break;
                    }
                }
                player.Position = position;
                return a;
            }
            return WJ_CollideCheck(q, player, dir, false) || prev; // We need WJ_CollideCheck to run first to ensure the active Solid Modifier is settable.
        }

        public static bool WJ_CollideCheck(List<Component> q, Player player, int dir, bool includeOrig) {
            int end = Math.Max(VivHelper.player_WallJumpCheck_getNum?.Invoke(player, dir) ?? 3,
                               (int)Math.Ceiling(Math.Abs(player.Speed.X) * Engine.DeltaTime) + 1);
            var scene = player.Scene;
            var s = includeOrig ? 4 : 0;
            if (end < s)
                return false;
            foreach (SolidModifierComponent smc in q) {
                if(smc.Entity is Solid solid) {
                    int _end = (smc.CornerBoostBlock > 0) ? smc.CornerBoostBlock : end;
                    for (int i = s; i <= _end; i++) {
                        if (player.ClimbBoundsCheck(dir) && !ClimbBlocker.EdgeCheck(scene, player, dir * i) &&
                            Collide.Check(player, solid, player.Position + Vector2.UnitX * (dir * i))) { // If the CornerBoostBlock state is not changing the behavior of the walljumpcheck, continue on
                            VivHelperModule.Session.currentActiveSolidModifier = smc;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        // Method is just "check every horizontal position"
        // Replaces standard CollideCheck by checking every valid point up to the next moved frame
        private static bool LegacyCollideCheck(Solid solid, Player player, int a) {
            bool b = false;
            int c = a < 0 ? Math.Max(3, (int) Math.Ceiling(Math.Abs(player.Speed.X) * Engine.DeltaTime)) : 3;
            for (int i = 0; i <= c; i++) {

                b |= (a > 1) ? solid.CollideCheck(player, solid.Position - Vector2.UnitX * ((int) player.Facing * i)) : player.CollideCheck(solid, player.Position + Vector2.UnitX * ((int) player.Facing * i));
                if (b)
                    break;
            }
            return b;
        }
        

        // 0 = Normal, -1 = CBB, -2 = CBB + Wall Retention
        public int CornerBoostBlock;

        public bool CornerBoostRetention;

        public bool bufferClimbJump;

        public bool triggerClimbOnTouch;

        public bool OnTouchFromBelow;

        public bool Legacy;

        public bool HasBeenBufferJumpedOn;

        public SolidModifierComponent(int cornerBoostBlock, bool bufferClimbJump, bool triggerClimbOnTouch, bool onTouchFromBelow = false)
        : base(active: true, visible: false) {
            CornerBoostBlock = -cornerBoostBlock;
            this.bufferClimbJump = bufferClimbJump;
            this.triggerClimbOnTouch = triggerClimbOnTouch;
            OnTouchFromBelow = onTouchFromBelow;
        }

        public SolidModifierComponent(SolidModifierComponent c)
            : base(active: true, visible: false) {
            CornerBoostBlock = c.CornerBoostBlock;
            bufferClimbJump = c.bufferClimbJump;
            triggerClimbOnTouch = c.triggerClimbOnTouch;
            OnTouchFromBelow = c.OnTouchFromBelow;
        }

    }
}
