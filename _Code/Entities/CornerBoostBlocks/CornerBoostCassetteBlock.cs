using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using MonoMod;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CornerBoostCassetteBlock")]
    [Tracked]
    public class CornerBoostCassetteBlock : CassetteBlock {
        #region Hooks

        private static bool attemptedLoad = false;
        private static bool createdCassetteManager = false;

        public static void Load() {
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
            On.Celeste.Level.LoadCustomEntity += Level_LoadCustomEntity;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
            On.Celeste.Level.LoadCustomEntity -= Level_LoadCustomEntity;
        }

        private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level level, Player.IntroTypes introType, bool isFromLoader = false) {
            attemptedLoad = false;
            orig(level, introType, isFromLoader);
        }

        private static bool Level_LoadCustomEntity(On.Celeste.Level.orig_LoadCustomEntity orig, EntityData entityData, Level level) {
            bool result = orig(entityData, level);
            if (!attemptedLoad) {
                createdCassetteManager = false;
                foreach (EntityData data in level.Session.LevelData.Entities) {
                    switch (data.Name) {
                        case "VivHelper/CornerBoostCassetteBlock":
                            level.HasCassetteBlocks = true;
                            if (level.CassetteBlockTempo == 1f) {
                                level.CassetteBlockTempo = data.Float("tempo", 1f);
                            }
                            level.CassetteBlockBeats = Math.Max(data.Int("index", 0) + 1, level.CassetteBlockBeats);

                            if (!createdCassetteManager) {
                                createdCassetteManager = true;
                                CassetteBlockManager manager = level.Tracker.GetEntity<CassetteBlockManager>();
                                if (manager == null && ShouldCreateCassetteManager(level)) {
                                    level.Add(new CassetteBlockManager());
                                    level.Entities.UpdateLists();
                                }
                            }
                            break;
                    }
                }
                attemptedLoad = true;
            }
            return result;
        }

        private static bool ShouldCreateCassetteManager(Level level) {
            if (level.Session.Area.Mode == AreaMode.Normal) {
                return !level.Session.Cassette;
            }
            return true;
        }

        #endregion

        public CornerBoostCassetteBlock(EntityData e, Vector2 v, EntityID id) : base(e, v, id) {
            Add(new SolidModifierComponent(e.Bool("PerfectCornerBoost", false) ? 2 : 1, false, false));
        }
    }
}
