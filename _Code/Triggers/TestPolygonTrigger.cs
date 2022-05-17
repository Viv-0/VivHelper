using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using VivHelper.Colliders;

namespace VivHelper.Triggers
{
    public class TestPolygonTrigger : Trigger
    {
        public TestPolygonTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Collider = new PolygonCollider(data.NodesWithPosition(offset));
            Visible = true;
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (Scene.OnInterval(0.1f))
            {
                Audio.Play("event:/game/07_summit/checkpoint_confetti", player.Position);
                base.Scene.Add(new SummitCheckpoint.ConfettiRenderer(player.Position));
            }
        }

        public override void Render()
        {
            PolygonCollider collider = Collider as PolygonCollider;
            for (int i = 0; i < collider.Points.Length - 1; i++)
            {
                Draw.Line(collider.Points[i], collider.Points[i + 1], Color.Red);
            }
            Draw.Line(collider.Points[0], collider.Points[collider.Points.Length - 1], Color.Red);
        }
    }
}
