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



namespace VivHelper {
    [Tracked]
    public class SolidModifierComponent : Component {
        public static void Load() {
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.ClimbJump += Player_ClimbJump;
            On.Celeste.Solid.HasPlayerClimbing += Solid_HasPlayerClimbing;
            On.Celeste.Solid.HasPlayerRider += Solid_HasPlayerRider;
        }



        public static void Unload() {
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.ClimbJump -= Player_ClimbJump;
            On.Celeste.Solid.HasPlayerClimbing -= Solid_HasPlayerClimbing;
            On.Celeste.Solid.HasPlayerRider -= Solid_HasPlayerRider;
        }

        private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            orig(self);
            foreach (Solid solid in self.Scene.Tracker.GetEntities<Solid>()) {
                SolidModifierComponent smc = solid?.Get<SolidModifierComponent>() ?? null;
                if (smc != null && WJ_CollideCheck(solid, self, smc.CornerBoostBlock)) {
                    if ((smc.ContactMod & 1) > 0) {

                        smc.HasBeenClimbJumpedOn = 1;

                    }
                    if (smc.CornerBoostBlock == 2) {
                        DynData<Player> dyn = new DynData<Player>(self);
                        dyn.Set<float>("WallSpeedRetentionTimer", dyn.Get<float>("WallSpeedRetentionTime"));
                    }
                }
            }
        }



        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self) {
            if (self.Scene == null)
                return;
            if (self.Scene.Tracker.Components.TryGetValue(typeof(SolidModifierComponent), out var q))
                foreach (SolidModifierComponent smc in q)
                    smc.HasBeenClimbJumpedOn = Math.Max(0, smc.HasBeenClimbJumpedOn - 1);
            orig(self);

        }

        private static bool Solid_HasPlayerClimbing(On.Celeste.Solid.orig_HasPlayerClimbing orig, Solid self) {
            bool b = orig(self);
            if (b) {
                return true;
            }
            //At this point we know the climb check has failed, so now we can mess with the actual BufferClimbJump or add HasPlayerLeaning as needed.
            if (self.Get<SolidModifierComponent>() == null)
                return false;
            //At this point we know the climb check failed + there is a SolidModifierComponent on the object

            if (!VivHelper.TryGetAlivePlayer(out Player p))
                return false;
            // we know that the climb check failed noramlly, there is a SolidModifierComponent on the object, and the player exists.
            SolidModifierComponent smc = self.Get<SolidModifierComponent>();
            switch (smc.ContactMod) {
                case 1:
                    return smc.HasBeenClimbJumpedOn > 0;
                case 2:
                    return smc.OnTouchFromBelow ? self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing)) || self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing) + Vector2.UnitY) : self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing));
                case 3:
                    return smc.HasBeenClimbJumpedOn > 0 || smc.OnTouchFromBelow ? self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing)) || self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing) + Vector2.UnitY) : self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing));
                default:
                    return false;
            }


        }

        private static bool Solid_HasPlayerRider(On.Celeste.Solid.orig_HasPlayerRider orig, Solid self) {
            bool b = orig(self);
            if (b) {
                return true;
            }
            //At this point we know the climb check has failed, so now we can mess with the actual BufferClimbJump or add HasPlayerLeaning as needed.
            if (self.Get<SolidModifierComponent>() == null)
                return false;
            //At this point we know the climb check failed + there is a SolidModifierComponent on the object

            if (!VivHelper.TryGetAlivePlayer(out Player p))
                return false;
            // we know that the climb check failed noramlly, there is a SolidModifierComponent on the object, and the player exists.
            SolidModifierComponent smc = self.Get<SolidModifierComponent>();
            switch (smc.ContactMod) {
                case 1:
                    return smc.HasBeenClimbJumpedOn > 0;
                case 2:
                    return smc.OnTouchFromBelow ? self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing)) || self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing) + Vector2.UnitY) : self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing));
                case 3:
                    return smc.HasBeenClimbJumpedOn > 0 || smc.OnTouchFromBelow ? self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing)) || self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing) + Vector2.UnitY) : self.CollideCheck(p, self.Position - (Vector2.UnitX * (int) p.Facing));
                default:
                    return false;
            }
        }

        // Method is just "check every horizontal position"
        // Replaces standard CollideCheck by checking every valid point up to the next moved frame
        public static bool WJ_CollideCheck(Solid solid, Player player, int a) {
            bool b = false;
            int c = a > 0 ? Math.Max(3, (int) Math.Ceiling(Math.Abs(player.Speed.X) * Engine.DeltaTime)) : 3;
            for (int i = 0; i <= c; i++) {

                b |= (a > 1) ? solid.CollideCheck(player, solid.Position - Vector2.UnitX * ((int) player.Facing * i)) : player.CollideCheck(solid, player.Position + Vector2.UnitX * ((int) player.Facing * i));
                if (b)
                    break;
            }
            return b;
        }


        // 0 = Default, 1 = CornerBoostBlock (legacy behavior), 2 = Perfect CornerBoostBlock
        public int CornerBoostBlock;
        //0 = No, 1 = Climb or BufferClimbJump only, 2 = On Touch, 3 = BufferClimbJump + On Touch
        public int ContactMod;
        public bool OnTouchFromBelow;

        public int HasBeenClimbJumpedOn = 0;

        public SolidModifierComponent(int cornerBoostBlock, bool bufferClimbJump, bool triggerClimbOnTouch, bool onTouchFromBelow = false) : base(true, false) {
            CornerBoostBlock = cornerBoostBlock;
            ContactMod = 0;
            if (bufferClimbJump)
                ContactMod++;
            if (triggerClimbOnTouch)
                ContactMod += 2;
            OnTouchFromBelow = onTouchFromBelow;
        }

        public SolidModifierComponent(SolidModifierComponent c) : base(true, false) {
            CornerBoostBlock = c.CornerBoostBlock;
            ContactMod = c.ContactMod;
            OnTouchFromBelow = c.OnTouchFromBelow;
        }


    }
}
