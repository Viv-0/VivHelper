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
        internal static string[] SIDs = new string[] {
            "Benjamanian/Celeste_on_the_Moon/0_Prologue_moon",
            "Benjamanian/Celeste_on_the_Moon/1_Deserted_Colony",
            "Benjamanian/Celeste_on_the_Moon/2_Launch_Site",
            "Benjamanian/Celeste_on_the_Moon/3_Interstellar_Resort",
            "Benjamanian/Celeste_on_the_Moon/4_Solar_Ridge",
            "Benjamanian/Celeste_on_the_Moon/5_Lunar_Temple",
            "Benjamanian/Celeste_on_the_Moon/6_Crystal_Crater",
            "Benjamanian/Celeste_on_the_Moon/7_Summit_Stargazing",
            "Benjamanian/Celeste_on_the_Moon/8_Epilogue_moon",
            "Benjamanian/Celeste_on_the_Moon/9_Moon_Cave",
            "Benjamanian/Celeste_on_the_Moon/10_The_Milky_Way"
        };

        internal static bool FloatyFix; //Acts as a temporary restrictor on LiftSpeed when

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

        private static void FloatySpaceBlock_Awake(On.Celeste.FloatySpaceBlock.orig_Awake orig, FloatySpaceBlock self, Monocle.Scene scene) {
            orig(self, scene);
            Session s = (scene as Level)?.Session;
            if (s != null &&
                (SIDs.Contains(s.Area.SID) || s.MapData.Levels.Any(l => l.Entities.Any(e => e.Name == "VivHelper/MoonBlockFix")))) { DynamicData.For(self).Set("firstUpdateCall", true); }
        }

        private static void FloatySpaceBlock_Update(ILContext il) {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After, i => i.MatchLdloc(0))) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, FloatySpaceBlock, bool>>((t, u) => t && (DynamicData.For(u).Data.TryGetValue("firstUpdateCall", out object v) && (bool) v));
            }
        }


    }
}
