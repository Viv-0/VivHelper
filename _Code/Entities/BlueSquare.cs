using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    public class BlueSquare : Entity {

        private static MTexture texture;

        public float radiusSq;
        public float slingSpeed;

        private float visualAngle;
        private float cooldownTimer;
        private bool launching;


        public BlueSquare(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Depth = -8500;
            radiusSq = data.Int("radius", 24);
            radiusSq = radiusSq * radiusSq;
            slingSpeed = data.Float("speed", 240f);
            visualAngle = Calc.Random.NextAngle();
            if(texture == null) {
                texture = GFX.Game["VivHelper/shape/square"];
            }
        }

        public override void Update() {
            visualAngle += Engine.DeltaTime; //1 rad/s
            base.Update();
            if (launching || cooldownTimer > 0) {

            } else if(Scene.Tracker.GetNearestEntity<Player>(Position, out float distSq) is Player player && distSq <= radiusSq) {
                // Visual indicator toggle
                if (Input.Jump.Pressed && !player.OnGround()) {
                    
                }
            }
        }

        public override void Render() {
            base.Render();
            texture.DrawOutlineCentered(Position, Color.LightBlue, 1f, visualAngle);
        }
    }
}
