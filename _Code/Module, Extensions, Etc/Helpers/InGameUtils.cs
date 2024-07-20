using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System.Reflection;
using MonoMod.Utils;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace VivHelper {
    public static partial class VivHelper {

        public static object[] oneOne = new object[1] { 1 };
        public static object[] negOne = new object[1] { -1 };
        
        internal static bool DialogTryGet(string dialogRef, out string temp) {
            temp = null;
            if (Dialog.Has(dialogRef)) { temp = Dialog.Get(dialogRef); return true; }
            return false;
        }
        public static Sprite CloneSprite(Sprite input) {
            return _CloneSprite(input);
        }
        public static bool IsValidRoomName(string input, List<LevelData> levels) {
            return levels.Any(l => l.Name == input);
        }

        public static bool CompareEntityIDs(EntityID a, EntityID b) {
            return a.Level == b.Level && a.ID == b.ID;
        }
        public static List<Entity> getListOfEntities(this EntityList self) => _RetrieveEntityList_entities(self);

        public static void ParticleSourceApplier(ref ParticleType pt, string particleSource) {
            string[] sources = particleSource.Trim().Split(',');
            if (sources.Length == 0) { return; }
            if (sources.Length == 1) {
                pt.Source = GFX.Game[particleSource];
            } else {
                if (particleSource.Contains(":")) {
                    Chooser<MTexture> chooser = new Chooser<MTexture>();
                    foreach (string s in sources) {
                        string[] t = s.Split(':');
                        chooser.Add(GFX.Game[t[0].Trim()], float.Parse(t[1].Trim()));
                    }
                    pt.SourceChooser = chooser;
                } else {
                    Chooser<MTexture> chooser = new Chooser<MTexture>();
                    foreach (string s in sources)
                        chooser.Add(GFX.Game[s.Trim()], 1f);
                    pt.SourceChooser = chooser;
                }
            }
        }

        internal static FieldInfo level_windController = typeof(Level).GetField("windController", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static FieldInfo autotiler_lookup = typeof(Autotiler).GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static MethodInfo Ignore = typeof(Autotiler).GetNestedType("TerrainType", BindingFlags.NonPublic).GetMethod("Ignore", BindingFlags.Public | BindingFlags.Instance);
        public static bool DoesTilesetConnect(char Base, char Compare) {
            var lookup = (System.Collections.IDictionary) autotiler_lookup.GetValue(GFX.FGAutotiler);
            var terrainType = lookup[Base];
            return !(bool) Ignore.Invoke(terrainType, new object[] { Compare });
        }

        public static Vector2 ScreenToHiResCamera(Vector2 position, Camera camera) {
            // Camera is always in the low res group, so we need to scale the matrix data from the matrix data we have
            // Inversion is a distributive property
            return Vector2.Transform(position, camera.Matrix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForceEntitySetScene(Entity self) => _forceEntitySetScene(self);

        #region ILMethods
        public static Action<Entity> _forceEntitySetScene => ForceEntitySetSceneIL();
        public static Func<Player, int, int> player_WallJumpCheck_getNum;
        private static Action<Entity> ForceEntitySetSceneIL() {
            string methodName = "VivHelper.ForceEntitySetScene";
            DynamicMethodDefinition method = new DynamicMethodDefinition(methodName, typeof(void), new Type[] { typeof(Entity) });
            var gen = method.GetILProcessor();

            MethodInfo entitySetScene = typeof(Entity).GetProperty("Scene", BindingFlags.Public | BindingFlags.Instance).GetSetMethod();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, entitySetScene);
            return (Action<Entity>) method.Generate().CreateDelegate(typeof(Action<Entity>));
        }

        private static Func<EntityList, List<Entity>> _RetrieveEntityList_entities = EntityList_EntitiesIL();

        private static Func<EntityList, List<Entity>> EntityList_EntitiesIL() {
            string methodName = "VivHelper._RetrieveEntityList_entities";
            DynamicMethodDefinition method = new DynamicMethodDefinition(methodName, typeof(List<Entity>), new Type[] { typeof(EntityList) });
            var gen = method.GetILProcessor();

            FieldInfo EntityList_entities = typeof(EntityList).GetField("entities", BindingFlags.NonPublic | BindingFlags.Instance);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, EntityList_entities);
            gen.Emit(OpCodes.Ret);
            return (Func<EntityList, List<Entity>>) method.Generate().CreateDelegate(typeof(Func<EntityList, List<Entity>>));
        }

        private static Func<Sprite, Sprite> _CloneSprite = CloneSpriteIL();
        private static Func<Sprite, Sprite> CloneSpriteIL() {
            string methodName = "VivHelper._CloneSprite";

            DynamicMethodDefinition method = new DynamicMethodDefinition(methodName, typeof(Sprite), new Type[] { typeof(Sprite) });
            var gen = method.GetILProcessor();

            MethodInfo cloneSprite = typeof(Sprite).GetMethod("CreateClone", BindingFlags.NonPublic | BindingFlags.Instance);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, cloneSprite);
            gen.Emit(OpCodes.Ret);
            return (Func<Sprite, Sprite>) method.Generate().CreateDelegate(typeof(Func<Sprite, Sprite>));
        }
        #endregion 
    }
}
