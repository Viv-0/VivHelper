using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Celeste;
using Celeste.Mod.Entities;
using VivHelper.Colliders;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/PolygonKillbox")]
    public class PolygonalKillbox : Trigger {
        public PolygonCollider polygonCollider => (PolygonCollider) Collider;
        public VertexPositionColor[] shanpe;
        public Color RenderColor { get; set; } = Color.Transparent;
        public string flag;
        internal Vector2 prevPos; internal Color prevColor;

        public PolygonalKillbox(EntityData data, Vector2 offset) : base(data, offset) {
            Collider = new PolygonCollider(data.NodesWithPosition(offset), this, true);
            flag = data.Attr("Flag", null);
            Visible = true;
            Collidable = true;
        }

        public override void OnStay(Player player) {
            if (string.IsNullOrWhiteSpace(flag) || (Scene as Level).Session.GetFlag(flag)) {
                base.OnEnter(player);
                player.Die(Vector2.Normalize(-player.Speed));
            }
        }

        public override void Update() {
            base.Update();
            if (Visible && (prevPos != Position || RenderColor != prevColor) || shanpe == null) {
                if (shanpe == null)
                    shanpe = new VertexPositionColor[polygonCollider.Indices.Length];
                for (int i = 0; i < polygonCollider.Indices.Length; i += 3) {
                    shanpe[i].Position = new Vector3(Position + polygonCollider.TriangulatedPoints[polygonCollider.Indices[i]], 0f);
                    shanpe[i].Color = RenderColor;
                    shanpe[i + 1].Position = new Vector3(Position + polygonCollider.TriangulatedPoints[polygonCollider.Indices[i + 1]], 0f);
                    shanpe[i + 1].Color = RenderColor;
                    shanpe[i + 2].Position = new Vector3(Position + polygonCollider.TriangulatedPoints[polygonCollider.Indices[i + 2]], 0f);
                    shanpe[i + 2].Color = RenderColor;
                }
            }
            prevPos = Position;
            prevColor = RenderColor;
        }
        public override void Render() {
            if (RenderColor != Color.Transparent)
                GFX.DrawVertices((Scene as Level)!.Camera.Matrix, shanpe, polygonCollider.Indices.Length);
        }

    }
}
