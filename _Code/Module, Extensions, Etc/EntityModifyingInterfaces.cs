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
            if (cursor.TryGotoNext(i1 => i1.MatchLdfld<EntityList>("toAwake"), i2 => i2.MatchCallvirt<List<Entity>>("Clear"))) {
                cursor.Index++;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<List<Entity>, EntityList, List<Entity>>>(PostAwakeCall);
            }
        }
        private static List<Entity> PostAwakeCall(List<Entity> toAwake, EntityList list) {
            Scene scene = list.Scene;
            foreach (Entity e in toAwake) {
                if (e is PostAwakeHolder postAwakeHolder)
                    postAwakeHolder.PostAwake(scene);
                foreach (Component c in e.Components) {
                    if (c is PostAwakeHolder p)
                        p.PostAwake(scene);
                }
            }
            return toAwake;
        }

        private static void ModifyAllCallsOfUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoPrev(MoveType.Before, i => i.MatchCallvirt<Entity>("Update"))) {
                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.EmitDelegate<Action<Entity>>(PostUpdateCall);
            }
        }

        private static void PostUpdateCall(Entity entity) {
            if (entity is PostUpdateHolder postAwakeHolder)
                postAwakeHolder.PostUpdate(entity);
            foreach (Component c in entity.Components) {
                if (c is PostUpdateHolder p)
                    p.PostUpdate(entity);
            }
        }
    }

    /// <summary>
    /// This is going to be added to a different helper eventually
    /// 
    /// Solely used for meta-entities as a precautionary measure to modify things that are changed in Awake after Awake is called but before the next update is called.
    /// This is modifiable and is deterministic on helper load order which is of major concern to me, but since noone else has used this tactic yet it should be fine. I'm also majorly concerned about performance issues regarding this but it hasn't been an issue for me yet.
    /// </summary>
    public interface PostAwakeHolder {

        /// <summary>
        /// A function that calls for all members of toAwake after all other objects have been awoken.
        /// </summary>
        /// <param name="scene"></param>
        void PostAwake(Scene scene);
    }
    /// <summary>
    /// This is going to be added to a different helper eventually
    /// 
    /// Solely used for meta-entities as a precautionary measure to modify things that occur after the Update call of a given entity but before the next entity's Update is called.
    /// </summary>
    public interface PostUpdateHolder {
        void PostUpdate(Entity entity);
    }
}
