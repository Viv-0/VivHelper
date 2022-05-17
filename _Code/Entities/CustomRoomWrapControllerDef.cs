using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace VivTestMod.Entities
{
    [Tracked]
    [CustomEntity("VivTest1/CustomRoomWrapControllerDef")]
    class CustomRoomWrapControllerDef : Entity
    {
        public bool scrollT, scrollR, scrollB, scrollL;
        public bool allEntities;
        public bool setByCamera;
        private static float[] playerOffsets = { -4f, -8f, 12f, 8f };
        private Level level;

        public CustomRoomWrapControllerDef(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            scrollT = data.Bool("Top", false);
            scrollR = data.Bool("Right", false);
            scrollB = data.Bool("Bottom", false);
            scrollL = data.Bool("Left", false);
            setByCamera = data.Bool("setByCamera", true);
            allEntities = data.Bool("allEntities", false);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            Rectangle bounds = level.Bounds;
            if (setByCamera)
            {
                Camera camera = level.Camera;
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    if (scrollB) { if (player.Top > camera.Bottom - 12f) { player.Bottom = bounds.Top + 4f; } }
                    if (scrollR) { if (player.Left > camera.Right - 9f) { player.Right = bounds.Left + 15f; } }
                    if (scrollT) { if (player.Bottom < camera.Top - 4f) { player.Top = bounds.Bottom - 12f; } }
                    if (scrollL) { if (player.Right < camera.Left + 9f) { player.Left = bounds.Right - 15f; } }
                }
            }
            else
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    if (scrollB) { if (player.Top > bounds.Bottom - 12f) { player.Bottom = bounds.Top + 4f; } }
                    if (scrollR) { if (player.Left > bounds.Right - 9f) { player.Right = bounds.Left + 10f; } }
                    if (scrollT) { if (player.Bottom < bounds.Top + 2f) { player.Top = bounds.Bottom - 12f; } }
                    if (scrollL) { if (player.Right < bounds.Left + 9f) { player.Left = bounds.Right - 10f; } }
                }
            }
            
        }
    }
}
