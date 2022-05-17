using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using System.Reflection;
using System.Collections;
using Celeste.Mod.Entities;


namespace VivHelper.Entities {
    [TrackedAs(typeof(CornerBoostSolid))]
    [Tracked(true)]
    public class CornerBoostSolid : Solid {
        public CornerBoostSolid(Vector2 position, float width, float height, bool safe, bool perfectCB) : base(position, width, height, safe) {
            Add(new SolidModifierComponent(perfectCB ? 2 : 1, false, false));
        }
    }

    [CustomEntity("VivHelper/CornerBoostBlock")]
    [TrackedAs(typeof(CornerBoostSolid))]

    public class CornerBoostBlock : CornerBoostSolid {

        private EntityID id;

        private char tileType;

        private bool blendIn;

        public CornerBoostBlock(Vector2 position, char tileType, float width, float height, bool blendIn, EntityID id, bool perfectCB)
            : base(position, width, height, safe: true, perfectCB) {
            base.Depth = -12999;
            this.id = id;
            this.tileType = tileType;
            this.blendIn = blendIn;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[this.tileType];
        }

        public CornerBoostBlock(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, data.Char("tiletype", 'm'), data.Width, data.Height, data.Bool("blendin"), id, data.Bool("perfectCornerBoost", false)) {
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            TileGrid tileGrid;
            if (!blendIn) {
                tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int) base.Width / 8, (int) base.Height / 8).TileGrid;
            } else {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (base.X / 8f) - tileBounds.Left;
                int y = (int) (base.Y / 8f) - tileBounds.Top;
                int tilesX = (int) base.Width / 8;
                int tilesY = (int) base.Height / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
                base.Depth = -10501;
            }
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
            Add(new LightOcclude());
            if (CollideCheck<Player>()) {
                RemoveSelf();
            }
        }


    }
}
