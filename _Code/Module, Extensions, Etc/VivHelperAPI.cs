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

namespace VivHelper.Module__Extensions__Etc {

    [ModExportName("VivHelperAPI")]
    public static class VivHelperAPI {
        public static Collider ProducePolygonColliderFromPoints(Vector2[] pts, Entity owner) => new PolygonCollider(pts, owner);
        public static Vector2 GetCentroidOfNonComplexPolygon(Vector2[] pts) => PolygonCollider.GetCentroidOfNonComplexPolygon(pts);

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
    }
}
