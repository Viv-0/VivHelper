using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Module__Extensions__Etc {

    public static class TeleportV2Hooks {

        public static Vector2? HackedFocusPoint;
        public static Vector2 HackfixFocusPoint => HackedFocusPoint ?? SpotlightWipe.FocusPoint;
        public static void Load() {
            On.Celeste.SpotlightWipe.ctor += SpotlightWipe_ctor;
            IL.Celeste.SpotlightWipe.Render += SpotlightWipe_Render;
            On.Monocle.Scene.BeforeUpdate += Scene_BeforeUpdate;
            IL.Celeste.Level.Render += Level_Render;

        }

        public static void Unload() {
            On.Celeste.SpotlightWipe.ctor -= SpotlightWipe_ctor;
            IL.Celeste.SpotlightWipe.Render -= SpotlightWipe_Render;
            On.Monocle.Scene.BeforeUpdate -= Scene_BeforeUpdate;
            IL.Celeste.Level.Render -= Level_Render;
        }

        private static void Scene_BeforeUpdate(On.Monocle.Scene.orig_BeforeUpdate orig, Scene self) {
            orig(self);
            if (self is Level level && self.Tracker.TryGetEntity<Player>(out Player player) && VivHelperModule.Session.TeleportState) {
                VivHelperModule.Session.TeleportAction?.Invoke(player, level);
                VivHelperModule.Session.TeleportAction = null;
            }
        }

        private static void Level_Render(ILContext il) {
            ILCursor cursor = new(il);
            if(cursor.TryGotoNext(MoveType.AfterLabel, i=>i.MatchLdarg(0) && i.Next.MatchLdfld<Level>("HiresSnow") && i.Next.Next.MatchBrfalse(out _))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Level>>((level) => {
                    if (VivHelperModule.Session.StallScreenWipe) {
                        VivHelperModule.Session.StallScreenWipe = false;
                        GameplayRenderer.Begin();
                        Draw.Rect(level.Camera.X - 2, level.Camera.Y - 2, 1924, 1084, Color.Black);
                        GameplayRenderer.End();
                    }
                });
            }
        }

        private static void SpotlightWipe_Render(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<SpotlightWipe>("FocusPoint"))) {
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Vector2, Scene, Vector2>>(ModifyFocusPoint);
            }
        }
        private static Vector2 ModifyFocusPoint(Vector2 orig, Scene scene) {
            if (!(scene is Level level))
                return orig;
            if (!VivHelperModule.Session.TeleportState)
                return orig;
            if (scene.Tracker.TryGetEntity<Player>(out Player player)) {
                return player.Position - level.Camera.Position - new Vector2(0, 8);
            }
            return HackfixFocusPoint;
        }

        private static void SpotlightWipe_ctor(On.Celeste.SpotlightWipe.orig_ctor orig, SpotlightWipe self, Scene scene, bool wipeIn, Action onComplete) {
            orig(self, scene, wipeIn, onComplete);
            if (scene is Level level && VivHelperModule.Session.TeleportState) {
                self.Linear = true;
                self.Duration = 0.5f;
            }
        }
    }

    public class DefaultTeleportOptions {
        public Func<Vector2, Vector2> posModifier;
        public Func<Vector2, Vector2> speedModifier;
        public string activateTriggersOfGivenName;
        public int setState;
    }

    internal static class TeleporterFunctions {
        
        public static void ClearTeleportFunction(Level level, Action<Level> extra = null) {
            Level _level = level ?? HelperEntities.AllUpdateHelperEntity.Scene as Level ?? Engine.Scene as Level;
            extra?.Invoke(_level);
            _level.PauseLock = false;
            VivHelperModule.Session.TeleportState = false;
            VivHelperModule.Session.TeleportAction = null;
        }

        /// <summary>
        /// Makes a teleport to a given room within the span of a frame, controlling the player's position and camera position. See parameter summaries for more detail.
        /// </summary>
        /// <param name="player">The player in the level</param>
        /// <param name="level">The level the player is in</param>
        /// <param name="newRoom">The name of the room according to MapData you want to teleport to</param>
        /// <param name="warpPosition">The world-scope position you want to warp the player to</param>
        /// <param name="endCutscene">If the player is in a cutscene, whether or not to end it. Usually prevents some crashes in cutscenes.</param>
        /// <param name="preteleport">Functions to run before the player teleports. Returns a list of objects you can access after the teleport in postteleport</param>
        /// <param name="postteleport">method to run after the player teleports, given the player, the level, and the list of objects if any from the preteleport method.</param>
        /// <param name="teleportFailed">A function to run to prevent crashes if the Teleport fails. Useful in scenarios where you have previously done something to softlock the game before the teleport, and don't have postteleport to fall back on.</param>
        /// <param name="spawnPosition">If the player should have a different spawn point for teleporting into the room. Default places it at the nearest available spawnpoint for that room.</param>
        public static void Teleport(Player player, Level level, string newRoom, Vector2 warpPosition, Vector2? spawnPosition = null, bool endCutscene = true, bool actAsTransition = false, Func<Player, Level, List<object>> preteleport = null, Action<Player, Level, List<object>> postteleport = null, Action<Level> teleportFailed = null) {
            if (player?.Dead ?? true) //Cancels on death
            {
                ClearTeleportFunction(level);
                return;
            }
            VivHelperModule.Session.TeleportAction = (p, l) => PerfectTeleport(p, l, newRoom, warpPosition, endCutscene, actAsTransition, preteleport, postteleport, teleportFailed, spawnPosition);
            level.PauseLock = true;
        }

        private static void PerfectTeleport(Player player, Level level, string newRoom, Vector2 warpPosition, bool endCutscene = true, bool actAsTransition = false, Func<Player, Level, List<object>> preteleport = null, Action<Player, Level, List<object>> postteleport = null, Action<Level> teleportFailed = null, Vector2? spawnPosition = null) {
            // Trust: new room is a room that exists in the Level by name
            // If player is dead, trying to teleport is fruitless.
            if (player?.Dead ?? true) {
                Logger.Log("VivHelper","Attempted to Teleport but player is either dead or null.");
                ClearTeleportFunction(level, teleportFailed);
                return; //Cancels on death
            }

            // Step 0: Copy down all the attributes in the player that could change
            Vector2 pPos = player.Position;
                // Enables custom teleporting functionality via a pre-teleport and post-teleport function. Order can be important in this method, so event wouldn't work here.
            List<object> storage = preteleport?.Invoke(player, level);
            Facings pFacing = player.Facing; int pDashes = player.Dashes; Vector2 pDashDir = player.DashDir;
                //It's crazy how little you need if you base your camera code on CameraTargets
            Vector2 cameraDifferential = level.Camera.Position - player.CameraTarget; 
            // Step 1: Clean up the player's relationship to other objects in the scene.
            if (endCutscene)
                level.EndCutscene();
            player.CleanUpTriggers();


            string prevRoom = level.Session.Level;

            Leader leader = player.Leader;
            for (int i = 0; i < leader.PastPoints.Count; i++) {
                leader.PastPoints[i] -= pPos;
            }
            foreach (Follower follower in leader.Followers) {
                if (follower.Entity != null) {
                    follower.Entity.Position -= pPos;
                    follower.Entity.AddTag(Tags.Global);
                    if (!VivHelper.CompareEntityIDs(follower.ParentEntityID, EntityID.None))
                        level.Session.DoNotLoad.Add(follower.ParentEntityID);
                }
            }

            // Transition handler pt1
            List<Component> transitionOut = new List<Component>();
            List<Component> transitionIn = new List<Component>();
            if (actAsTransition && newRoom != prevRoom && level.Tracker.Components.TryGetValue(typeof(TransitionListener), out var u)) {
                transitionOut = new List<Component>(u);
            }

            //Removes the current level we are in without breaking everything
            level.Remove(player);
            level.Entities.Remove(level.Entities.FindAll(VivHelperModule.UnloadTypesWhenTeleporting)); //There's some weird stuff that goes on in awake for these classes.
            //Clears the burst info
            level.Displacement.Clear();
            //Clears the TrailManager if the room doesn't match the previous room
            if (prevRoom != newRoom) {
                level.ParticlesBG.Clear();
                level.Particles.Clear();
                level.ParticlesFG.Clear();
                TrailManager.Clear();
            }

            // Reloads the level in place
            level.UnloadLevel();
            level.Session.Level = newRoom;
            level.Session.RespawnPoint = spawnPosition ?? (level.Session.LevelData.Spawns.Count == 0 ? warpPosition : level.Session.GetSpawnPoint(warpPosition));
            level.Session.FirstLevel = false;
            // player's position is now still in the old room but the room he is purportedly in is the new room.
            // Once we load the level, we can bring the player back to the appropriate position.
            level.Add(player); 
            level.LoadLevel(Player.IntroTypes.Transition);

            // Set the player's position and other base values
            player.Position = warpPosition;
            player.Hair.MoveHairBy(warpPosition - pPos);
            player.Facing = pFacing; player.Dashes = pDashes; player.DashDir = pDashDir;

            //Copies over the follower data.
            foreach (Follower follower in leader.Followers) {
                if (follower.Entity != null) {
                    follower.Entity.Position += player.TopLeft;
                    follower.Entity.RemoveTag(Tags.Global);
                    //Prevents loading in of follower entities if you have collected them.
                    if (!VivHelper.CompareEntityIDs(follower.ParentEntityID, EntityID.None) && !level.Session.Keys.Contains(follower.ParentEntityID))
                        level.Session.DoNotLoad.Remove(follower.ParentEntityID);
                }
            }
            for (int i = 0; i < leader.PastPoints.Count; i++) {
                leader.PastPoints[i] += player.Position;
            }
            // Handles transition listeners if the value is set
            if (actAsTransition && newRoom != prevRoom && level.Tracker.Components.TryGetValue(typeof(TransitionListener), out var w)) {
                transitionIn = new List<Component>(w);
                transitionIn.RemoveAll((Component c) => transitionOut.Contains(c));
                foreach (TransitionListener item in transitionOut) {
                    item?.OnOutBegin?.Invoke();
                    item?.OnOut?.Invoke(1f); // We want to instantly set to the values in
                }
                foreach (TransitionListener item in transitionIn) {
                    item?.OnInBegin?.Invoke();
                    item?.OnIn?.Invoke(1f);
                    item?.OnInEnd?.Invoke();
                }
            }

            leader.TransferFollowers();
            //Magic sandwich lava fix for atpx8
            if(level != null) {
                LavaRect t = null, b = null;
                if (level.Tracker.TryGetEntity(typeof(SandwichLava), out Entity _a) && _a is SandwichLava a) {
                    a.Y = (float) a.SceneAs<Level>().Camera.Bottom - 10f;
                    DynamicData d = DynamicData.For(a);
                    t = d.Get<LavaRect>("topRect");
                    t.Position.Y = d.Get<float>("TopOffset") - t.Height;
                    b = d.Get<LavaRect>("bottomRect");
                    b.Position.Y = 0;
                }
                if (VivHelper.TryGetType("Celeste.Mod.MaxHelpingHand.Entities.CustomSandwichLava", out Type _t) && level.Tracker.TryGetEntity(_t, out Entity c)) {
                    c.Y = (float) c.SceneAs<Level>().Camera.Bottom - 10f;
                    DynamicData d = new DynamicData(_t, c);
                    t = d.Get<LavaRect>("topRect");
                    t.Position.Y = d.Get<float>("TopOffset") - t.Height + d.Get<float>("sandwichDisplacement");
                    b = d.Get<LavaRect>("bottomRect");
                    b.Position.Y = -d.Get<float>("sandwichDisplacement");
                }
            }

            //Camera magic, this is such a dumb strategy but it works
            foreach (Trigger trigger in level.Tracker.GetEntities<Trigger>().Where(t => Collide.Check(player, t) && t.GetType().Name.IndexOf("camera", StringComparison.OrdinalIgnoreCase) > -1)) {
                VivHelper.PlayerTriggerCheck(player, trigger);
            }
            level.Camera.Position = player.CameraTarget + cameraDifferential;

            // Custom post-teleport processors
            postteleport?.Invoke(player, level, storage);

            level.PauseLock = false;
            VivHelperModule.Session.TeleportState = false;
            VivHelperModule.Session.TeleportAction = null;
        }

    }
}
