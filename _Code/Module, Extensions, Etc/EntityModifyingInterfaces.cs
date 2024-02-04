using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using VivHelper.Entities;

namespace VivHelper {

    public static class EntityChangingInterfaces {

        public static void Load() {
            IL.Monocle.EntityList.UpdateLists += EntityList_UpdateLists;
        }
        public static void Unload() {
            IL.Monocle.EntityList.UpdateLists -= EntityList_UpdateLists;
        }
        private static void EntityList_UpdateLists(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            // PreAwake caller
            if (cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<EntityList>("adding"), j=>j.OpCode == OpCodes.Callvirt && j.Operand == typeof(EntityList).GetField("adding", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).FieldType.GetMethod("Clear"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldfld, typeof(EntityList).GetField("toAwake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                cursor.Emit(OpCodes.Call, typeof(EntityChangingInterfaces).GetMethod("PreAwakeCall"));
            }

            // PostAwake caller
            if (cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<EntityList>("toAwake") && i.Next.OpCode == OpCodes.Callvirt && i.Next.Operand == typeof(EntityList).GetField("toAwake",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).FieldType.GetMethod("Clear"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Call, typeof(EntityChangingInterfaces).GetMethod("PostAwakeCall"));
            }
        }

        private static void Level_AfterRender(ILContext il) {
            ILCursor cursor = new(il);
            if(cursor.TryGotoNext(MoveType.After, i => i.MatchCall<Scene>("AfterRender"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Call, typeof(EntityChangingInterfaces).GetMethod("PostRenderCall"));
            }
        }

        public static void PreAwakeCall(EntityList list, List<Entity> toAwake) {
            Scene scene = list.Scene;
            foreach (Entity e in toAwake) {
                if (e is IPreAwake postAwakeHolder)
                    postAwakeHolder.PreAwake(scene);
                foreach (Component c in e.Components) {
                    if (c is IPreAwake p)
                        p.PreAwake(scene);
                }
            }
        }

        public static List<Entity> PostAwakeCall(List<Entity> toAwake, EntityList list) {
            Scene scene = list.Scene;
            foreach (Entity e in toAwake) {
                if (e is IPostAwake postAwakeHolder)
                    postAwakeHolder.PostAwake(scene);
                foreach (Component c in e.Components) {
                    if (c is IPostAwake p)
                        p.PostAwake(scene);
                }
            }
            return toAwake;
        }

    }
    ///<summary>
    /// Solely used for meta-entities as a precautionary measure to modify things that are changed in Awake before Awake is called. Useful in scenarios where you need to modify the contents of some function before its Awake is called.
    /// This is modifiable and is deterministic on helper load order which is of major concern to me, but since noone else has used this tactic yet it should be fine. I'm also majorly concerned about performance issues regarding this but it hasn't been an issue for me yet.
    ///</summary>
    public interface IPreAwake {
        /// <summary>
        /// A function that calls for all members of toAwake before all other objects have been awoken.
        /// </summary>
        void PreAwake(Scene scene);
    }

    /// <summary>
    /// Solely used for meta-entities as a precautionary measure to modify things that are changed in Awake after Awake is called but before the next update is called.
    /// This is modifiable and is deterministic on helper load order which is of major concern to me, but since noone else has used this tactic yet it should be fine. I'm also majorly concerned about performance issues regarding this but it hasn't been an issue for me yet.
    /// </summary>
    public interface IPostAwake {

        /// <summary>
        /// A function that calls for all members of toAwake after all other objects have been awoken.
        /// </summary>
        void PostAwake(Scene scene);
    }
    
}
