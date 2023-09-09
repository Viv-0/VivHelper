using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.ModInterop;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using VivHelper.Colliders;
using VivHelper.Entities.Boosters;
using System.Reflection;
using VivHelper.Entities;
using Mono.Cecil.Cil;
using FMOD.Studio;
using System.Collections;

namespace VivHelper {

    [ModExportName("VivHelperAPI")]
    public static class VivHelperAPI {
        #region Audio Muting
        public static void SimpleEntityMuteMethod(MethodInfo info, bool isCoroutine) => EntityMuterComponent.HookMethodInfoWithAudioMute(info, isCoroutine, null);
        public static void AdvancedEntityMuteMethod(MethodInfo info, bool isCoroutine, bool takeFromLocal, int argIdentifier) {

            EntityMuterComponent.HookMethodInfoWithAudioMute(info, isCoroutine, new Tuple<Mono.Cecil.Cil.OpCode, object>[1] { new Tuple<OpCode, object>(takeFromLocal ? OpCodes.Ldloc : OpCodes.Ldarg, argIdentifier) });
        }

        public static void MuteAllAudioPoints(bool mute) => EntityMuterComponent.overrideMute = mute;

        public static EventInstance AudioPlayWithMuteControl(Entity entity, string path, Vector2? position) {
            EventInstance i = null;
            if (position.HasValue) {
                EntityMuterComponent.objPlayingAudio = entity;
                i = Audio.Play(path, position.Value);
                EntityMuterComponent.objPlayingAudio = null;
            } else {
                EntityMuterComponent.objPlayingAudio = entity;
                i = Audio.Play(path);
                EntityMuterComponent.objPlayingAudio = null;
            }
            return i;
        }
        #endregion

        #region Polygons
        public static Collider ProducePolygonColliderFromPoints(Vector2[] pts, Entity owner) => new PolygonCollider(pts, owner, true);
        public static Vector2 GetCentroidOfNonComplexPolygon(Vector2[] pts) => PolygonCollider.GetCentroidOfNonComplexPolygon(pts);
        #endregion

        #region Custom Booster Interface
        /// <summary>
        /// Handles adding a CustomAttribute to an UltimateCustomBooster.
        /// </summary>
        /// <param name="name">The "name" reference you'd want for your custom mechanic. For a simple example, a custom parameter for something like ShadowDash might be just `Shadow`</param>
        /// <param name="onCustomDashBegin">An Action that occurs when the Custom Dash starts</param>
        /// <param name="onCustomDashEnd">An Action that occurs when the Custom Dash ends</param>
        /// <param name="onCustomDashRefill">An Action that occurs when the Custom Dash refills dashes. This is a parameter set by the custom booster, so this accounts for that timing difference.</param>
        /// <param name="onCustomBoostBegin">An Action that occurs when the Custom Boost starts, as soon as the player enters the Booster. If CustomDash is being used dynamically as the player dash, this has no effect.</param>
        /// <param name="onCustomBoostEnd">An Action that occurs when the Custom Boost ends, as soon as the player starts to CustomDash. If CustomDash is being used dynamically as the player dash, this has no effect.</param>
        /// <returns>Whether or not the CustomAttribute was successfully added. Always is determinant on the name of the Booster Mechanic.</returns>
        public static bool AddCustomAttributeToUltimateCustomBooster(string name, Action<Player> onCustomDashBegin, Action<Player> onCustomDashEnd, Action<Player> onCustomDashRefill, Action<Player> onCustomBoostBegin, Action<Player> onCustomBoostEnd, Action<Vector2> onCustomBoosterRender) {
            if (UltraCustomDash.customDashSpecialHandlers.ContainsKey(name))
                return false;
            var cda = new CustomDashActions();
            if (onCustomBoostBegin != null)
                cda.onCustomBoostBegin = onCustomBoostBegin;
            if (onCustomBoostEnd != null)
                cda.onCustomBoostEnd = onCustomBoostEnd;
            if (onCustomDashBegin != null)
                cda.onCustomDashBegin = onCustomDashBegin;
            if (onCustomDashRefill != null)
                cda.onCustomDashRefill = onCustomDashRefill;
            if (onCustomDashEnd != null)
                cda.onCustomDashEnd = onCustomDashEnd;
            if (onCustomBoosterRender != null)
                cda.onCustomBoosterRender = onCustomBoosterRender;
            UltraCustomDash.customDashSpecialHandlers.Add(name, cda);
            return true;
        }
        #endregion

        /*#region Powerup Interface 
        public static void AddDashPowerup(string name, List<string> linkedEntityData, int innerState, bool callDashListeners, PowerupCoroutine coroutine, Func<Player, IEnumerator> unlockRoutine, string guiDirectory, Dictionary<string, string> convolutions) =>
            DashPowerupManager.AddPowerup(name, linkedEntityData, innerState, callDashListeners, coroutine, guiDirectory, unlockRoutine, convolutions);

        public static void ActivateDashPowerup(string name, Player player) => VivHelperModule.Session.dashPowerupManager?.ActivatePowerup(name, player);
        #endregion*/
    }
}
