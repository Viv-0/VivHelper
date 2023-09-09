using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Editor;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Reflection;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework.Graphics;
using VivHelper.Module__Extensions__Etc;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Celeste.Mod;

namespace VivHelper.Entities {
    /// If you're looking to copy off of this code for one of your entities, please just ask me to implement it here.
    /// If you're looking to learn from this code, that's probably a bad idea too.
    public class LevelInfo {

        public Dictionary<Vector2, NoResetRespawnData> NoResetRespawns;
        public Dictionary<Vector2, InterRoomRespawnData> InterRoomRespawnSet;

        public LevelInfo() {
            NoResetRespawns = new Dictionary<Vector2, NoResetRespawnData>();
            InterRoomRespawnSet = new Dictionary<Vector2, InterRoomRespawnData>();
        }

        public LevelInfo(string levelName) {
            NoResetRespawns = new Dictionary<Vector2, NoResetRespawnData>();
            InterRoomRespawnSet = new Dictionary<Vector2, InterRoomRespawnData>();
        }

        public LevelInfo(LevelInfo orig) {
            NoResetRespawns = new Dictionary<Vector2, NoResetRespawnData>(orig.NoResetRespawns);
            InterRoomRespawnSet = new Dictionary<Vector2, InterRoomRespawnData>(orig.InterRoomRespawnSet);
        }

        public override string ToString() {
            return "NoResetRespawns:  " + string.Join(Environment.NewLine, NoResetRespawns) + "\nInterRoomRespawnSet:  " + string.Join(Environment.NewLine, InterRoomRespawnSet);
        }
    }

    public struct NoResetRespawnData {
        public bool NoResetOnRetry;
        public string Flag;

        public override string ToString() {
            return "{ NoResetOnRetry: " + NoResetOnRetry + ", " + Flag == null ? "null " : Flag + "}";
        }
    }

    public struct InterRoomRespawnData {
        public string roomName;
        public Vector2 position;
        public string Flag;
        public override string ToString() {
            return "{" + roomName + ", " + position + ", " + Flag == null ? "null " : Flag + "}";
        }
    }

    public static class SpawnPointHooks {

        private static MethodInfo Level_loadNewPlayer = typeof(Level).GetMethod("LoadNewPlayer", BindingFlags.NonPublic | BindingFlags.Static);
        private static FieldInfo MapEditor_mapData = typeof(MapEditor).GetField("mapData", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo MapEditor_levels = typeof(MapEditor).GetField("levels", BindingFlags.NonPublic | BindingFlags.Instance);
        private static IDetour hook_LevelData_orig_ctor;
        private static IDetour hook_Level_orig_Pause;
        private static IDetour hook_MapEditor_orig_Render;

        //95% of these hooks you should never ever ever do in your Celeste mods, please dont do this,
        //all of these have some awkward changes that if anyone hooks these methods I'm going to have to redesign.
        public static void Load() {
            On.Celeste.Player.Added += Player_Added;

            MethodInfo LD_C = typeof(LevelData).GetMethod("orig_ctor", BindingFlags.Public | BindingFlags.Instance);
            hook_LevelData_orig_ctor = new ILHook(LD_C, LevelData_ctor);

            On.Celeste.Editor.MapEditor.ctor += MapEditor_ctor;
            IL.Celeste.Level.Reload += Level_Reload;


            IL.Celeste.Editor.LevelTemplate.RenderContents += LevelTemplate_RenderContents;
            hook_MapEditor_orig_Render = new ILHook(typeof(MapEditor).GetMethod("orig_Render", BindingFlags.Public | BindingFlags.Instance), MapEditor_Render);

            hook_Level_orig_Pause = new ILHook(typeof(Level).GetMethod("orig_Pause", BindingFlags.Public | BindingFlags.Instance), Level_OrigPauseHook);
            IL.Celeste.Editor.MapEditor.RenderKeys += MapEditor_RenderKeys;
            //For the time being, QuickRetry button is bugged.

            //            IL.Celeste.MapData.CanTransitionTo += MapData_CanTransitionTo;
        }
        public static void Unload() {
            On.Celeste.Player.Added += Player_Added;

            hook_LevelData_orig_ctor?.Dispose();

            On.Celeste.Editor.MapEditor.ctor -= MapEditor_ctor;

            IL.Celeste.Level.Reload -= Level_Reload;

            IL.Celeste.Editor.LevelTemplate.RenderContents -= LevelTemplate_RenderContents;
            hook_MapEditor_orig_Render?.Dispose();

            hook_Level_orig_Pause?.Dispose();
            IL.Celeste.Editor.MapEditor.RenderKeys -= MapEditor_RenderKeys;
            //            IL.Celeste.MapData.CanTransitionTo -= MapData_CanTransitionTo;
        }

        internal static string[] trueSpawnPoints = new string[] { "VivHelper/Spawnpoint", "VivHelper/InterRoomSpawnTarget" };

        private static void MapData_CanTransitionTo(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, i => i.MatchCeq() && i.Next.MatchRet())) {
                cursor.Emit(OpCodes.Ldloc_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<bool, LevelData, Level, bool>>((b, ld, l) => {
                    if (!b)
                        return false;
                    List<Vector2> spawns = new List<Vector2>(ld.Spawns);
                    for (int i = 0; i < spawns.Count; i++) {
                        Vector2 v = spawns[i];
                        foreach (EntityData e in ld.Entities) {
                            if (v == ld.Position + e.Position) {
                                string str;
                                if (e.Name == "VivHelper/Spawnpoint") {
                                    str = e.Attr("Flag");
                                    if (str.Length == 0)
                                        continue;
                                    b = true;
                                    if (str[0] == '!') { b = false; str = str.Substring(1); }
                                    if (!string.IsNullOrWhiteSpace(str) && l.Session?.GetFlag(str) != b) {
                                        spawns.RemoveAt(i);
                                    }
                                } else if (e.Name == "VivHelper/InterRoomSpawner") {
                                    str = e.Attr("Flag");
                                    if (str.Length == 0)
                                        continue;
                                    b = true;
                                    if (str[0] == '!') { b = false; str = str.Substring(1); }
                                    if (!string.IsNullOrWhiteSpace(str) && l.Session?.GetFlag(str) != b) {
                                        spawns.RemoveAt(i);
                                    }
                                }
                            }
                        }
                    }
                    return spawns.Count > 0;
                });
            }
        }



        private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig(self, scene);
            if (scene is Level level) {
                try {
                    LevelData l = level.Session.MapData.Levels.First(_l => _l.Name == level.Session.Level);
                    var e = l.Entities.First(_e => trueSpawnPoints.Contains(_e.Name) && _e.Position == self.Position);
                    if (e.Bool("forceFacing", true))
                        self.Facing = e.Bool("flipX", false) ? Facings.Left : Facings.Right;
                    return;

                } catch {
                    return;
                }
            }
        }

        //This was previously in Everest.Events.Level.OnEnter, but was moved to being a part of the LoadingThread hook in VivHelperModule 
        public static void AddLevelInfoCache(Session session) {
            MapData data = session.MapData;
            List<EntityData> InterRoomSpawners = new List<EntityData>(); //Added here for delaying the Spawners until after all SpawnTargets are registered
            Dictionary<int, Tuple<string, Vector2>> temp = new Dictionary<int, Tuple<string, Vector2>>();
            foreach (LevelData levelData in data.Levels) {
                string s = levelData.Name;
                LevelInfo info = new LevelInfo();
                foreach (EntityData entityData in levelData.Entities) {
                    if (entityData.Name == "VivHelper/Spawnpoint") {
                        if (entityData.Bool("NoResetRespawn")) {
                            info.NoResetRespawns.Add(levelData.Position + entityData.Position, new NoResetRespawnData { NoResetOnRetry = entityData.Bool("NoResetOnRetry", false), Flag = entityData.NoEmptyString("Flag") });
                        }
                    } else if (entityData.Name == "VivHelper/InterRoomSpawner") {
                        InterRoomSpawners.Add(entityData);
                    } else if (entityData.Name == "VivHelper/InterRoomSpawnTarget" || entityData.Name == "VivHelper/InterRoomSpawnTarget2") {
                        var _temp = entityData.Int("tag", 0);
                        if (_temp != 0 && !temp.ContainsKey(_temp)) {
                            temp[_temp] = new Tuple<string, Vector2>(s, levelData.Position + entityData.Position);
                        }
                    }
                }
                VivHelperModule.Session.LevelInfoCache[s] = info;
            }
            foreach (EntityData e in InterRoomSpawners) {
                LevelInfo li = new LevelInfo(VivHelperModule.Session.LevelInfoCache[e.Level.Name]);
                var _temp = e.Int("tag", 0);
                if (temp.ContainsKey(_temp)) //temp will never contain 0 which is why that check is omitted
                {

                    li.InterRoomRespawnSet.Add(e.Position + e.Level.Position, new InterRoomRespawnData {
                        roomName = temp[_temp].Item1,
                        position = temp[_temp].Item2,
                        Flag = e.NoEmptyString("Flags") ?? e.NoEmptyString("Flag")
                    });

                    VivHelperModule.Session.LevelInfoCache[e.Level.Name] = li;
                }
            }
        }



        private static void MapEditor_ctor(On.Celeste.Editor.MapEditor.orig_ctor orig, MapEditor self, AreaKey area, bool reloadMapData) {
            //Retrieve CurrentSession internally first because afterwards there's some bug with occasionally having the Scene be the MapEditor.
            Session currentSession = (Engine.Scene as Level)?.Session ?? SaveData.Instance?.CurrentSession_Safe;
            if (currentSession?.Area != area)
                currentSession = null;
            orig.Invoke(self, area, reloadMapData);
            MapData mapData = (MapData) MapEditor_mapData.GetValue(self);
            List<LevelTemplate> levels = (List<LevelTemplate>) MapEditor_levels.GetValue(self);
            if (levels == null) { Logger.Log("VivHelper", "This shouldn't ever happen, uhoh."); return; }

            List<EntityData> RoomToRoomRespawners = new List<EntityData>();
            //Used for RespawnTargets between rooms;
            Dictionary<string, InterRoomRespawnData> RoomToRoomRespawnTargetTags = new Dictionary<string, InterRoomRespawnData>();
            foreach (LevelData levelData in mapData.Levels) {
                LevelTemplate template = levels.FirstOrDefault((l) => l.Name == levelData.Name); //Checks if the template has previously been removed
                if (template == null) {
                    if (area.GetLevelSet() != "Into The Jungle") {
                        Logger.Log(LogLevel.Verbose, "VivHelper", "Comparing to " + levelData.Name);
                        foreach (LevelTemplate t in levels) {
                            Logger.Log(LogLevel.Verbose, "VivHelper", t.Name);
                        }
                    }
                    continue;
                }
                //Checks to see if an EntityData that hides the room from the debug map exists
                EntityData entityData = levelData.Entities.FirstOrDefault((e) => e.Name == "VivHelper/HideRoomInMap");
                if (entityData != default(EntityData)) {
                    string flag = entityData.Attr("Flag");
                    bool b = true;
                    if (!string.IsNullOrWhiteSpace(flag) && flag[0] == '!') {
                        b = false;
                        flag = flag.Substring(1);
                    }
                    if (string.IsNullOrWhiteSpace(flag) || currentSession == null || (currentSession?.GetFlag(flag) == b)) {
                        if (entityData.Bool("DummyOnly", false)) {
                            template.Spawns.Clear();
                            template.Dummy = true;
                        } else {
                            levels.Remove(template);
                            continue;
                        }
                    }
                }
                foreach (EntityData e in levelData.Entities) {
                    if (e.Name == "VivHelper/Spawnpoint") {
                        string flag = e.Attr("Flag");
                        bool b = true;
                        if (!string.IsNullOrWhiteSpace(flag) && flag[0] == '!') {
                            b = false;
                            flag = flag.Substring(1);
                        }

                        if (e.Bool("HideFromDebugMap", false) || (!string.IsNullOrWhiteSpace(flag) && currentSession?.GetFlag(flag) != b)) {
                            template.Spawns.Remove(e.Position);
                            if (template.Spawns.Count == 0) { template.Dummy = true; }
                        }
                    }
                }
            }

        }

        private static void Level_OrigPauseHook(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("menu_pause_retry")) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj<Action>())) {
                cursor.EmitDelegate<Func<Action, Action>>(orig => delegate {
                    VivHelperModule.Session.PausedRetryCheck = true;
                    orig();
                });
            }
        }

        private static void Level_Reload(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            /* call void Celeste.TrailManager::Clear() # clears the TrailManager
\\\\DETAIL\\\                                           (((((cursor2 instruction point)))))
////PATCH////  ldarg.0
////PATCH////  call bool VivHelper.Entities.SpawnpointHooks::SkipReload(class Celeste.Level) # calls Level::LoadNewPlayer if true to resolve the future conflict
////PATCH////  brtrue.s <TARGET> # skips over UnloadLevel and LoadLevel
	           ldarg.0
	           callvirt instance void Celeste.Level::UnloadLevel() # runs UnloadLevel
	           call void [mscorlib]System.GC::Collect()
	           call void [mscorlib]System.GC::WaitForPendingFinalizers()
	           ldarg.0
\\\\DETAIL\\\                                           (((((cursor final instruction point)))))
////PATCH////  call class Celeste.Level VivHelper.Entities.SpawnpointHooks::ModifyRoomToRespawnTo(class Celeste.Level) # modifies the contents of Level and replants it to the stack
	           ldc.i4.1
	           ldc.i4.0
	           callvirt instance void Celeste.Level::LoadLevel(valuetype Celeste.Player/IntroTypes, bool) # loads the level
\\\\DETAIL\\\                                           ((((( cursor starting instruction point)))))
\\\\DETAIL\\\  IL Label <TARGET> set here
             */
            if (cursor.TryGotoNext(instr => instr.MatchLdarg(0), instr => instr.MatchCallvirt<Level>("UnloadLevel"))) {
                ILCursor cursor2 = cursor.Clone();
                if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Level>("LoadLevel"))) {
                    ILLabel target = cursor.MarkLabel();
                    cursor.GotoPrev(MoveType.After, instr => instr.MatchLdarg(0));
                    cursor.EmitDelegate<Func<Level, Level>>(ModifyRoomToRespawnTo);

                    cursor2.Emit(OpCodes.Ldarg_0);
                    cursor2.EmitDelegate<Func<Level, bool>>(SkipReload);
                    cursor2.Emit(OpCodes.Brtrue, target);
                }
            }

        }

        private static Level ModifyRoomToRespawnTo(Level level) {
            Session session = level.Session;
            string levelName = session.Level;
            Vector2 pos = session.RespawnPoint ?? session.LevelData.Spawns[0];
            if (VivHelperModule.Session.LevelInfoCache.ContainsKey(levelName)) {
                if (VivHelperModule.Session.LevelInfoCache[levelName].InterRoomRespawnSet.ContainsKey(pos)) {
                    InterRoomRespawnData irrd = VivHelperModule.Session.LevelInfoCache[levelName].InterRoomRespawnSet[pos];
                    if (string.IsNullOrWhiteSpace(irrd.Flag) || (session?.GetFlag(irrd.Flag) ?? false)) {
                        session.Level = irrd.roomName;
                        session.RespawnPoint = irrd.position;
                    }
                }
            }
            return level;
        }

        private static bool SkipReload(Level level) {
            if (VivHelperModule.Session.LevelInfoCache.TryGetValue(level.Session.Level, out var val)) {
                Dictionary<Vector2, NoResetRespawnData> NoResetRespawns = val.NoResetRespawns;
                bool b = VivHelperModule.Session.PausedRetryCheck;
                VivHelperModule.Session.PausedRetryCheck = false;
                Vector2? v = level.Session.RespawnPoint;
                if (!v.HasValue)
                    return false;
                if (NoResetRespawns.ContainsKey(v.Value)) {
                    // if NoResetRespawns is onretry, always return true, otherwise, only act if this is not from a Retry
                    if (NoResetRespawns[level.Session.RespawnPoint.Value].NoResetOnRetry || !b) {
                        PlayerSpriteMode playerSpriteMode = ((!level.Session.Inventory.Backpack) ? PlayerSpriteMode.MadelineNoBackpack : PlayerSpriteMode.Madeline);
                        Player player = (Player) Level_loadNewPlayer.Invoke(level, new object[2] { v.Value, playerSpriteMode });
                        player.IntroType = Player.IntroTypes.Respawn;
                        level.Add(player);
                        level.DoScreenWipe(true);
                        return true;
                    }
                }
            }
            return false;

        }


        private static void LevelData_ctor(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdstr("player"))) {
                ILCursor clone = cursor.Clone();
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Brfalse || instr.OpCode == OpCodes.Brfalse_S)) {
                    clone.Emit(OpCodes.Dup);
                    cursor.EmitDelegate<Func<string, bool, bool>>((a, b) => {
                        return b || a == "VivHelper/Spawnpoint" || a == "VivHelper/InterRoomSpawner" || a == "VivHelper/InterRoomSpawnTarget";
                    });
                }
            }

        }

        private static Type[] ffffc = new Type[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(Color) };

        private static void LevelTemplate_RenderContents(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            MethodInfo spawnChecker = typeof(SpawnPointHooks).GetMethod("SpawnChecker", BindingFlags.NonPublic | BindingFlags.Static);
            int accessor = -1;
            ILLabel jumpBack = null; //our jumpback to the first ldloca.s $n call.
            ILLabel jumpNext = null; //jump to [ldloca.s, MoveNext, brtrue] if we actually find the scenario we want to ignore that spawn for. (We use an ender-hook for that at the end :) )
            //The second statement is: callvirt instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<valuetype [FNA]Microsoft.Xna.Framework.Vector2>::GetEnumerator()
            //No shot someone actually fucked with the callvirt there, noone is that stupid.
            //If you need to fuck with this, you really don't please don't please please please
            if (cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<LevelTemplate>("Spawns"), i2 => true, i3 => i3.MatchStloc(out accessor))) {
                //The second statement is: callvirt instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<valuetype [FNA]Microsoft.Xna.Framework.Vector2>::MoveNext()
                //No shot someone actually fucked with the callvirt there, noone is that stupid.
                if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloca(accessor), i2 => true, i3 => i3.MatchBrtrue(out jumpBack))) {
                    jumpNext = cursor.MarkLabel();
                    cursor.GotoLabel(jumpBack);
                    if (cursor.TryGotoNext(MoveType.After, i => i.MatchLdloca(accessor), i2 => true, i3 => i3.MatchStloc(out accessor))) //Review the accessor value and then retrieve the next value. In theory this *should* always be accessor+1 but because of ILhooks we can't be sure :(
                    {
                        cursor.Emit(OpCodes.Ldarg_0); // LevelTemplate template
                        cursor.Emit(OpCodes.Ldloc, accessor); // Vector2 spawn
                        cursor.Emit(OpCodes.Ldarg_2); // List<LevelTemplate> levels
                        // call SpawnChecker(template, spawn, levels)
                        cursor.Emit(OpCodes.Call, spawnChecker); // I'm doing my best to limit the ReferenceBag size because that gets yucky
                        // jump if true <MoveNext sequence>
                        cursor.Emit(OpCodes.Brtrue, jumpNext);
                    }
                }
            }
        }

        internal static bool SpawnChecker(LevelTemplate template, Vector2 spawn, List<LevelTemplate> levels) {
            if (VivHelperModule.Session.LevelInfoCache == null)
                return false;
            LevelInfo info = null;
            if (!VivHelperModule.Session.LevelInfoCache.TryGetValue(template.Name, out info))
                return false;
            Vector2 pos = new Vector2(template.X + spawn.X, template.Y + spawn.Y);
            if (info.InterRoomRespawnSet.TryGetValue(pos * 8, out var roomRespawnData)) {
                LevelTemplate otherRoom = levels.FirstOrDefault(l => l.Name == roomRespawnData.roomName);
                if (otherRoom != default(LevelTemplate)) {
                    Draw.Line(roomRespawnData.position / 8f - Vector2.UnitY, pos - Vector2.UnitY, Color.PowderBlue * 0.5f);
                }
                return true;
            }
            return info.NoResetRespawns.ContainsKey(pos);
        }

        internal static void MapEditor_Render(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            MethodInfo postRenderContents = typeof(SpawnPointHooks).GetMethod("PostRenderContents", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo template_RenderOutline = typeof(LevelTemplate).GetMethod("RenderOutline", BindingFlags.Public | BindingFlags.Instance);
            if (cursor.TryGotoNext(i => i.MatchCallvirt(template_RenderOutline))) {
                cursor.GotoPrev(i => i.OpCode == OpCodes.Ldsfld);
                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, MapEditor_levels);
                cursor.Emit(OpCodes.Call, postRenderContents);
            }
        }

        internal static void PostRenderContents(LevelTemplate template, List<LevelTemplate> levels) {
            if (VivHelperModule.Session?.LevelInfoCache == null)
                return;
            if (!VivHelperModule.Session.LevelInfoCache.TryGetValue(template.Name, out LevelInfo info) || template.Spawns == null)
                return;
            foreach (Vector2 spawn in template.Spawns) {
                Vector2 pos = new Vector2(template.X + spawn.X, template.Y + spawn.Y);
                if (info.NoResetRespawns.TryGetValue(pos * 8, out _)) {
                    Draw.Rect(pos - Vector2.UnitY, 1f, 1f, Calc.HexToColor("ae8cfc"));
                } else if (info.InterRoomRespawnSet.TryGetValue(pos * 8, out var roomRespawnData)) {
                    LevelTemplate otherRoom = levels.FirstOrDefault(l => l.Name == roomRespawnData.roomName);
                    if (otherRoom != default(LevelTemplate)) {
                        Draw.Rect(pos - Vector2.UnitY, 1f, 1f, Color.Orange);
                    }
                }
            }
        }



        private static void MapEditor_RenderKeys(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdarg(0), instr => instr.MatchLdfld<MapEditor>("mapData"), instr => instr.MatchLdfld<MapData>("Levels"))) {
                cursor.Emit(OpCodes.Call, typeof(SpawnPointHooks).GetMethod("LimitKeyRendering"));
            }
        }

        public static List<LevelData> LimitKeyRendering(List<LevelData> origLevels) => new List<LevelData>(origLevels).Where(l => !l.Entities.Any(e => e.Name == "VivHelper/HideRoomInMap")).ToList();


    }

    [Tracked]
    [CustomEntity("VivHelper/Spawnpoint")]
    public class SpawnPoint : Entity {

        public static MTexture _texture;

        public MTexture texture;

        public bool HideFromDebugMap, NoResetRespawn, flipX;

        public Color color, outlineColor;

        public SpawnPoint(EntityData data, Vector2 offset) : base(data.Position + offset) {
            texture = null;
            if (data.Bool("ShowTexture", true)) {
                string t = data.Attr("Texture");
                if (string.IsNullOrWhiteSpace(t)) {
                    texture = _texture;
                } else {
                    GFX.Game.PushFallback(null);
                    texture = GFX.Game[t];
                    if (texture == null) {
                        texture = _texture;
                    }
                    GFX.Game.PopFallback();
                }
            }
            NoResetRespawn = data.Bool("NoResetRespawn");
            HideFromDebugMap = data.Bool("HideFromDebugMap");
            Depth = data.Int("Depth", 5000);
            color = VivHelper.ColorFix(data.Attr("Color"));
            outlineColor = VivHelper.ColorFix(data.Attr("OutlineColor"));
            flipX = data.Bool("flipX");
        }

        public bool InView() {
            Camera camera = SceneAs<Level>().Camera;
            if (X > camera.X - 16 && Position.Y > camera.Y - 16 && X < camera.X + camera.Viewport.Width + 16) {
                return Y < camera.Y + camera.Viewport.Height + 16;
            }
            return false;
        }



        public override void Render() {

            if (texture != null && InView()) {
                if (outlineColor == Color.Transparent)
                    texture.DrawJustified(Position + Vector2.UnitY, new Vector2(0.5f, 1f), color, Vector2.One, 0f, (SpriteEffects) (flipX ? 1 : 0));
                else if (outlineColor == Color.Black)
                    texture.DrawOutlineJustified(Position + Vector2.UnitY, new Vector2(0.5f, 1f), color, Vector2.One, 0f, (SpriteEffects) (flipX ? 1 : 0));
                else
                    texture.DrawColoredOutline(Position + Vector2.UnitY, new Vector2(texture.Width * 0.5f, texture.Height), color, outlineColor, Vector2.One, 0f, (SpriteEffects) (flipX ? 1 : 0));
            }
        }

        public override void DebugRender(Camera camera) {
            Draw.Circle(Position - Vector2.UnitY, 5f - (Scene.TimeActive * 5) % 5f, NoResetRespawn ? Color.Lavender : Color.Cyan, 8);
        }
    }

    [CustomEntity("VivHelper/InterRoomSpawner")]
    public class BetweenRoomRespawn : SpawnPoint {
        public int tag;

        public BetweenRoomRespawn(EntityData data, Vector2 offset) : base(data, offset) {
            tag = data.Int("tag");
        }

        public override void DebugRender(Camera camera) {
            Draw.Circle(Position - Vector2.UnitY, 5f - (Scene.TimeActive * 5) % 5f, Color.LightPink, 8);
        }
    }

    [CustomEntity("VivHelper/InterRoomSpawnTarget = Load", "VivHelper/InterRoomSpawnTarget2 = Load")]
    public class BetweenRoomRespawnTarget : SpawnPoint {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new BetweenRoomRespawnTarget(entityData, offset);

        public int tag;
        public BetweenRoomRespawnTarget(EntityData data, Vector2 offset) : base(data, offset) {
            tag = data.Int("tag");
        }
    }
}
