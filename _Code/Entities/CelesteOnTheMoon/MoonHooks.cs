using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace VivHelper {
    public static class MoonHooks {

        public static void Load() {
            On.Celeste.JumpThru.MoveHExact += JumpThru_MoveHExact;
            On.Celeste.JumpThru.MoveVExact += JumpThru_MoveVExact;
            On.Celeste.JumpThru.GetPlayerRider += JumpThru_GetPlayerRider;
            On.Celeste.JumpThru.HasRider += JumpThru_HasRider;
            On.Celeste.JumpThru.HasPlayerRider += JumpThru_HasPlayerRider;
        }
        public static void Unload() {
            On.Celeste.JumpThru.MoveHExact -= JumpThru_MoveHExact;
            On.Celeste.JumpThru.MoveVExact -= JumpThru_MoveVExact;
            On.Celeste.JumpThru.GetPlayerRider -= JumpThru_GetPlayerRider;
            On.Celeste.JumpThru.HasRider -= JumpThru_HasRider;
            On.Celeste.JumpThru.HasPlayerRider -= JumpThru_HasPlayerRider;
        }



        private static bool Actor_MoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher) {
            if (self.Scene == null)
                return false;
            if (self.Scene.Tracker == null)
                return false;
            return orig(self, moveV, onCollide, pusher);
        }

        private static bool Actor_MoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher) {
            if (self.Scene == null)
                return false;
            if (self.Scene.Tracker == null)
                return false;
            return orig(self, moveH, onCollide, pusher);
        }

        private static void JumpThru_MoveHExact(On.Celeste.JumpThru.orig_MoveHExact orig, JumpThru self, int move) {
            if (self.Scene == null)
                return;
            if (self.Scene.Tracker == null)
                return;
            orig(self, move);
        }

        private static void JumpThru_MoveVExact(On.Celeste.JumpThru.orig_MoveVExact orig, JumpThru self, int move) {
            if (self.Scene?.Tracker == null)
                return;
            orig(self, move);
        }

        private static Player JumpThru_GetPlayerRider(On.Celeste.JumpThru.orig_GetPlayerRider orig, JumpThru self) {
            if (self.Scene == null)
                return null;
            if (self.Scene.Tracker == null)
                return null;
            if (self.Scene.Tracker.CountEntities<Actor>() == 0)
                return null;
            return orig(self);
        }

        private static bool JumpThru_HasRider(On.Celeste.JumpThru.orig_HasRider orig, JumpThru self) {
            if (self.Scene == null)
                return false;
            if (self.Scene.Tracker == null)
                return false;
            if (self.Scene.Tracker.CountEntities<Player>() == 0)
                return false;
            return orig(self);
        }

        private static bool JumpThru_HasPlayerRider(On.Celeste.JumpThru.orig_HasPlayerRider orig, JumpThru self) {
            if (self.Scene == null)
                return false;
            if (self.Scene.Tracker == null)
                return false;
            if (self.Scene.Tracker.CountEntities<Player>() == 0)
                return false;
            return orig(self);
        }


    }
}
