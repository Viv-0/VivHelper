using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod;
using Monocle;


namespace VivHelper {
    public static partial class VivHelper {

        public static FieldInfo EntityList_toAwake;

        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Render()")]
        public static void Entity_Render(Entity entity) {
            Logger.Log("VivHelper","link to Entity::Render failed");
        }

        [MonoMod.MonoModLinkTo("Monocle.Entity", "System.Void Awake(Monocle.Scene)")]
        public static void Entity_Awake(Entity entity, Scene scene) {
            Logger.Log("VivHelper","link to Entity::Awake failed");
        }
    }
}
