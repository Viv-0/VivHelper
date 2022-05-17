using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [CustomEntity(
        "VivHelper/FlagConditionBlock = LegacyLoad",
        "VivHelper/FlagConditionBlock2 = Load")]
    public class FlagConditionBlock : Solid {
        public string flag;
        public bool active;
        private char tileType;
        private bool blendIn, invert, startVal, ignoreStartVal;
        private float delay, timer;
        //Added Legacy functionality.
        public static Entity LegacyLoad(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new FlagConditionBlock(entityData, offset, 0);
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new FlagConditionBlock(entityData, offset, 1);


        public FlagConditionBlock(EntityData data, Vector2 offset, int legacy) : base(data.Position + offset, data.Width, data.Height, true) {
            flag = data.Attr("Flag", "");
            tileType = data.Char((legacy == 0 ? "tileType" : "tiletype"), '3');
            blendIn = data.Bool("blendIn", false);
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            EnableAssistModeChecks = false;
            ignoreStartVal = data.Bool("IgnoreStartVal", true);
            startVal = data.Bool("StartVal");
            invert = data.Bool("InvertFlag");
            delay = data.Float("Delay", 0f);

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
            timer = delay;
            if (!ignoreStartVal)
                (Scene as Level).Session.SetFlag(flag, startVal);
        }

        public override void Update() {
            base.Update();
            bool f = (Scene as Level).Session.GetFlag(flag);
            if (Collidable && (invert ? f : !f)) { EnableStaticMovers(); } else if (!Collidable && (invert ? !f : f)) { DisableStaticMovers(); }
            Collidable = Visible = invert ? !f : f;
        }
    }
}
