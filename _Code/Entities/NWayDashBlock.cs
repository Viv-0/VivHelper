using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Reflection;
using static VivHelper.VivHelper;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/nWayDashBlock")]
    public class NWayDashBlock : DashBlock {

        public List<Vector2> viableDashDirections;
        public Color detailColor;

        public NWayDashBlock(EntityData d, Vector2 v, EntityID id) : base(d, v, id) {
            OnDashCollide = NWayDashed;
            viableDashDirections = new List<Vector2>();
            if (d.Bool("Right", true))
                viableDashDirections.Add(new Vector2(1, 0));
            if (d.Bool("Up", true))
                viableDashDirections.Add(new Vector2(0, -1));
            if (d.Bool("Left", true))
                viableDashDirections.Add(new Vector2(-1, 0));
            if (d.Bool("Down", true))
                viableDashDirections.Add(new Vector2(0, 1));
            detailColor = VivHelper.GetColorWithFix(d, "DetailColor", "detailColor", GetColorParams.None, GetColorParams.None, Color.Black).Value;
        }

        public DashCollisionResults NWayDashed(Player player, Vector2 direction) {
            if (!new DynData<DashBlock>(this).Get<bool>("canDash") && player.StateMachine.State != 5 && player.StateMachine.State != 10 || !viableDashDirections.Contains(-direction.EightWayNormal())) {
                return DashCollisionResults.NormalCollision;
            }
            Break(player.Center, direction, true, true);
            return DashCollisionResults.Rebound;
        }

        public override void Render() {
            base.Render();
            bool r = viableDashDirections.Contains(new Vector2(1, 0));
            bool u = viableDashDirections.Contains(new Vector2(0, -1));
            bool l = viableDashDirections.Contains(new Vector2(-1, 0));
            bool d = viableDashDirections.Contains(new Vector2(0, 1));
            for (int i = 1; i < 6; i++) {
                if (u) { Draw.Line(new Vector2(Left + i, Top + i - 1), new Vector2(Right - i, Top + i - 1), detailColor * (0.7f - 0.1f * i)); }
                if (d) { Draw.Line(new Vector2(Left + i, Bottom - i + 1), new Vector2(Right - i, Bottom - i + 1), detailColor * (0.7f - 0.1f * i)); }
                if (l) { Draw.Line(new Vector2(Left + i - 1, Top + i), new Vector2(Left + i - 1, Bottom - i), detailColor * (0.7f - 0.1f * i)); }
                if (r) { Draw.Line(new Vector2(Right - i + 1, Top + i), new Vector2(Right - i + 1, Bottom - i), detailColor * (0.7f - 0.1f * i)); }
            }
        }
    }
}
