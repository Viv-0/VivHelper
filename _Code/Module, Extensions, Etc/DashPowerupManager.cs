using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Celeste;
using System.Collections;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using VivHelper.Module__Extensions__Etc.Helpers;
using Microsoft.Xna.Framework.Input;
using VivHelper.Module__Extensions__Etc;
using VivHelper.Entities;
// Seriously, don't do anything like this in your own helper. Feel free to ask me about why I did it this way but unless you know what you're doing this is a game-breaking endeavor.
namespace VivHelper {
    
    public enum PowerupFormat {
        Override = 0,   // By default, you can only use the most recent powerup you've obtained
        Prevent = 1,    // This option stops you from collecting any more powerups if you have one in your inventory
        Queue = 2,      // This option creates a Queue of actions, meaning if you collect A and then B, your next 2 dashes are A and then B
    }

    public class DashReplace { // Purely the data stored for a powerup
        public readonly string name, guiRef;
        public readonly Func<int> innerState;
        public readonly bool callDashListeners;
        public Action<Player> updateWhenReady, actionOnActivation, updateWhenActive;
        public Action<string, Player> actionOnCancel;
        public Func<Player, IEnumerator> routineOnComplete, unlockRoutine;
        public DashReplace(string name, Func<int> innerState, bool callDashListeners, Action<Player> updateWhenReady, Action<Player> actionOnActivation, Action<string, Player> actionOnCancel, Action<Player> updateWhenActive, Func<Player, IEnumerator> routineOnComplete, string guiRef, Func<Player, IEnumerator> unlockRoutine) {
            this.name = name;
            this.innerState = innerState;
            this.callDashListeners = callDashListeners;
            this.updateWhenReady = updateWhenReady;
            this.updateWhenActive = updateWhenActive;
            this.actionOnActivation = actionOnActivation;
            this.actionOnCancel = actionOnCancel;
            this.routineOnComplete = routineOnComplete;
            this.guiRef = guiRef;
            this.unlockRoutine = unlockRoutine;
        }
    }

    public class DashPowerupManager {
        #region Hooks
        private static ILHook hook_StateMachine_ForceState, hook_StateMachine_set_State;

        public static void Load() {
            entityDataLinks = new Dictionary<string, List<string>>();
            dashPowerups = new Dictionary<string, DashReplace>();
            convolutionMatrix = new Dictionary<UnorderedPair<string>, string>(new UnorderedPairComparer<string>());
            hook_StateMachine_ForceState = new ILHook(typeof(StateMachine).GetMethod("ForceState"), ForceSetStateOverrideOnPlayerDash);
            hook_StateMachine_set_State = new ILHook(typeof(StateMachine).GetProperty("State").GetSetMethod(), ForceSetStateOverrideOnPlayerDash);
            // Player_ctor hook appended to Module::Player_ctor
        }

        public static void Unload() {
            dashPowerups = null;
            hook_StateMachine_ForceState?.Dispose();
            hook_StateMachine_set_State?.Dispose();
        }

        public static void ForceSetStateOverrideOnPlayerDash(ILContext il) {
            ILCursor cursor = new(il);
            // checks after the state equality check "if state is already the state you're telling the game to set to, don't set it." This works for both ForceState and set_State
            if (cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0), j => j.MatchLdfld<StateMachine>("Log"))) {
                cursor.Emit(OpCodes.Ldarg, 0); // stateMachine
                cursor.Emit(OpCodes.Dup); // stateMachine, stateMachine
                cursor.Emit(OpCodes.Ldfld, typeof(StateMachine).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic)); // stateMachine, stateMachine.state
                cursor.Emit(OpCodes.Ldarg, 1); // stateMachine, stateMachine.state, newState
                cursor.Emit(OpCodes.Call, typeof(DashPowerupManager).GetMethod("OverrideDashCheck", BindingFlags.NonPublic | BindingFlags.Static));
                cursor.Emit(OpCodes.Starg, 1); // oooo rare instruction
            }
        }

        private static int OverrideDashCheck(StateMachine machine, int previousState, int newState) {
            if (!(machine.Entity is Player player))
                return newState;
            if (!(player.Get<DashPowerupController>() is DashPowerupController controller))
                return newState;
            controller.EndActivePowerup();
            if (newState != 2 ||
                VivHelperModule.Session.dashPowerupManager is not { } manager) // If there is no dashreplace manager, or the state isn't a dash, we're not changing the newState.
                return newState;
            return manager.HandleDashCheck(player, previousState, controller);
        }
        #endregion
        private const byte GLOBALQUEUECAP = 100;


        public static Dictionary<string, List<string>> entityDataLinks;
        public static Dictionary<string, DashReplace> dashPowerups;
        private static Dictionary<UnorderedPair<string>, string> convolutionMatrix;


        public PowerupFormat format;
        public List<string> validPowerups; // in Inventory format, used as the maximum pool of powerups for the level.
        public string defaultPowerup = null; // Not formatted for Inventory, optional parameter
        public bool convolve;
        // Queue specific variables
        public byte queueCap = GLOBALQUEUECAP;
        public byte storeMultipleCap = 1;
        public DashPowerupManager(PowerupFormat format, bool convolve, string defaultPowerup = null) {
            this.format = format;
            validPowerups = new List<string>();
            this.convolve = convolve;
            this.defaultPowerup = defaultPowerup;
        }


        /// <summary>
        /// Adds a powerup to the known pool of powerups. Load-specific.
        /// </summary>
        /// <param name="name">The name that the powerup is referenced by</param>
        /// <param name="linkedEntityData">This should be a list of EntityData names that should add this powerup to the valid list of powerups in a map.</param>
        /// <param name="innerState">The state that will replace Dash when Dash key is pressed</param>
        /// <param name="coroutine">When a powerup is activated, this coroutine will run in the player. See the dashreplace template for more info.</param>
        /// <param name="callDashListeners">whether or not DashListeners are called during the dash press.</param>
        public static void AddPowerup(string name, List<string> linkedEntityData, Func<int> innerState, bool callDashListeners, Action<Player> updateBefore,
                                    Action<Player> actionOnActivation, Action<string, Player> actionOnCancel, Action<Player> updateDuring, Func<Player, IEnumerator> routineAfter,
                                      string guiRef, Func<Player, IEnumerator> unlockRoutine = null, Dictionary<string, string> convolutions = null) {
            if (dashPowerups.ContainsKey(name)) {
                // This occurs when two mods have conflicting powerup names
                if (innerState != dashPowerups[name].innerState ||
                    callDashListeners != dashPowerups[name].callDashListeners ||
                    updateBefore != dashPowerups[name].updateWhenReady ||
                    actionOnActivation != dashPowerups[name].actionOnActivation ||
                    updateDuring != dashPowerups[name].updateWhenActive || 
                    routineAfter != dashPowerups[name].routineOnComplete) {
                    throw new Exception("Duplicate Powerup name was attempted to be added! Try prepending your specific powerup with helper initials or name.");
                }
                return;
            }
            DashReplace replace = new DashReplace(name, innerState, callDashListeners, updateBefore, actionOnActivation, actionOnCancel, updateDuring, routineAfter, guiRef, unlockRoutine);
            dashPowerups.Add(name, replace);
            foreach (var ed in linkedEntityData) {
                if (!entityDataLinks.TryGetValue(ed, out List<string> links)) {
                    entityDataLinks.Add(ed, new List<string>(1) { name });
                } else {
                    links.Add(name);
                }
            }
            if (convolutions != null) {
                foreach (var kvp in convolutions) {
                    var t = new UnorderedPair<string>(name, kvp.Key);
                    if (convolutionMatrix.ContainsKey(t)) {
                        convolutionMatrix.Add(t, kvp.Value);
                    }
                }
            }
        }

        public static bool GivePowerup(string name, Player _player) {
            if (!dashPowerups.ContainsKey(name))
                throw new Exception("You cannot activate the powerup \"" + name + ",\" because it's not registered!");
            DashPowerupManager manager = VivHelperModule.Session.dashPowerupManager;
            if (manager == null) {
                Logger.Log("VivHelper", "Powerup cannot activate due to no PowerupManager existing in the map.");
                return false;
            }
            if (manager.defaultPowerup == name || !manager.validPowerups.Contains(name)) {
                Logger.Log("VivHelper", "Powerup " + name + " failed to activate because it wasn't validated, or it is the default powerup");
                return false;
            }
            Player player = _player ?? VivHelper.GetPlayer();
            if (player == null) { Logger.Log("VivHelper", "Failed to get Player when powerup " + name + " was activated."); return false; }
            return manager.ActivatePowerup(name, player);
        }

        // Virtual to allow for inheritance
        public virtual bool ActivatePowerup(string name, Player player) {
            //if (format == PowerupFormat.Inventory) { return UnlockPowerup(dr); }
            DashPowerupController con = player.Get<DashPowerupController>();
            if (con == null) {
                con = new DashPowerupController(true, true);
                player.Add(con);
            }
            DashReplace cur = con.ReadyPowerup;
            switch (format) {
                case PowerupFormat.Override:
                    if (convolve) {
                        if (cur != null && convolutionMatrix.TryGetValue(new UnorderedPair<string>(cur.name, name), out var conv)) {
                            cur.actionOnCancel?.Invoke(name, player);
                            con.ReadyPowerup = dashPowerups[conv]; // If the convolution matrix supports this mixing, override the current dash powerup with the convolved one
                        }
                    } else if (name == con.ReadyPowerup?.name) {
                        return false;
                    } else {
                        cur?.actionOnCancel?.Invoke(null, player);
                        con.ReadyPowerup = dashPowerups[name];
                    }
                    break;
                case PowerupFormat.Prevent:
                    if (convolve) {
                        if (cur == null)
                            con.ReadyPowerup = dashPowerups[name]; // if there is no current powerup, add one
                        //if there is one, normally we'd just return false, but if it happens to convolve with the current powerup, mix them
                        else if (!convolutionMatrix.TryGetValue(new UnorderedPair<string>(cur.name, name), out var conv)) {
                            cur.actionOnCancel?.Invoke(conv, player);
                            con.ReadyPowerup = dashPowerups[conv];
                        } else
                            return false;
                    } else if (cur == null) {
                        con.ReadyPowerup = dashPowerups[name];
                    } else
                        return false;
                    break;
                case PowerupFormat.Queue:
                    // Code gets the queue
                    LinkedList<DashReplace> queue = con.PowerupQueue;
                    if (queue == null) {
                        Logger.Log("VivHelper", "PowerupQueue failed to instantiate during LoadingThread, instantiating now.");
                        con.PowerupQueue = new LinkedList<DashReplace>();
                    }
                    // if the queue is empty, add the powerup
                    if (queue.Count == 0)
                        queue.AddFirst(dashPowerups[name]);
                    else { // Convolution can only occur on the first entry in the queue
                        if (convolve && convolutionMatrix.TryGetValue(new UnorderedPair<string>(queue.First.Value.name, name), out string conv)) {
                            queue.First.Value.actionOnCancel?.Invoke(conv,player);
                            queue.RemoveFirst();
                            queue.AddFirst(dashPowerups[conv]); // Since we're removing 1 and adding 1, the net change is 0 so no need to check the capacity
                        } else { // Handles max multiple capacity
                            byte counter = 0;
                            foreach (var power in queue) {
                                if (power.name == name)
                                    if (++counter > storeMultipleCap)
                                        return false;
                            }
                            queue.First.Value.actionOnCancel?.Invoke(name, player);
                            queue.AddFirst(dashPowerups[name]);
                            if (queue.Count > queueCap)
                                queue.RemoveLast(); // This assumes that people are activating powerups one at a time. Which they *should* be.
                        }
                    }
                    break;
            }
            return true;
        }

        protected int HandleDashCheck(Player player, int previousState, DashPowerupController con) { // Since this only replaces Dashes, we know that the "new state" will always be Player.StDash (2)
            DashReplace _override;
            if (format == PowerupFormat.Queue) {
                LinkedList<DashReplace> queue = con.PowerupQueue;
                if(queue.First == null) return Player.StDash;
                _override = queue.First?.Value;
                queue.RemoveFirst();
            } else {
                _override = con.ReadyPowerup;
                con.ReadyPowerup = null;
            }
            if (_override == null) {
                if (defaultPowerup == null)
                    return Player.StDash; // if _override is null, and defaultPowerup is null, then treat the game as normal
                return con.ActivatePowerup(dashPowerups[defaultPowerup]);
            }
            return con.ActivatePowerup(_override);
        }

        public static void LoadDefaultPowerups() {
            AddPowerup(RedDashRefill.RedDashPowerup, new List<string> { "VivHelper/RedDashRefill" }, () => Player.StRedDash, true, RedDashRefill.EffectBefore, RedDashRefill.EffectAt, null, RedDashRefill.EffectDuring, null, "VivHelper/powerups/reddash");
            AddPowerup(WarpDashRefill.WarpDashPowerup, new List<string> { "VivHelper/WarpDashRefill" }, () => WarpDashRefill.WarpDashState, false, WarpDashRefill.EffectBefore, null, WarpDashRefill.EffectCancel, null, null, null);
            AddPowerup(BumperRefill.BumperPowerup, new List<string> { "VivHelper/BumperRefill" }, () => Player.StLaunch, false, BumperRefill.EffectBefore, BumperRefill.EffectAt, null, null, null, null);
            //AddPowerup(FireworkRefill.FireworkPowerup, new List<string> { "VivHelper/FireworkRefill" }, () => 2, false, FireworkRefill.EffectBefore, FireworkRefill.EffectAt, null, null, FireworkRefill.RoutineAfter, null, null, null);
            AddPowerup(FeatherRefill.FeatherPowerup, new List<string> { "VivHelper/FeatherRefill" }, () => Player.StStarFly, false, FeatherRefill.EffectBefore, null, null, null, null, null, null, null);
        }
    }
    // This is a test class going to be sent around with the API to automate powerup addition
    public class Powerup {
       
    }
}
