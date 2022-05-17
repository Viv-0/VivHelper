using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/DeathConditionalBlock")]
    public class DeathConditionalBlock : Solid {
        public int deaths;
        private TileGrid tiles;
        private char tiletype;
        private bool invert, blendIn;

        public DeathConditionalBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, true) {
            base.Depth = -13000;
            this.tiletype = data.Char("tiletype", '3');
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tiletype];
            blendIn = data.Bool("blendIn", false);
            invert = data.Bool("DisappearOnDeaths");
            deaths = Math.Max(data.Int("DeathCount", 25), 0);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (((scene as Level).Session.DeathsInCurrentLevel < deaths) != invert)
            //if we do appear on Deaths (not DisappearOnDeaths) and Deaths is less than the req deaths, remove.
            //if we DisappearOnDeaths and Deaths is geq to the req deaths, remove.
            {
                RemoveSelf();
                return;
            }
            TileGrid tileGrid;
            if (!blendIn) {
                tileGrid = GFX.FGAutotiler.GenerateBox(tiletype, (int) Width / 8, (int) Height / 8).TileGrid;
                Add(new LightOcclude());
            } else {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (base.X / 8f) - tileBounds.Left;
                int y = (int) (base.Y / 8f) - tileBounds.Top;
                int tilesX = (int) base.Width / 8;
                int tilesY = (int) base.Height / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tiletype, x, y, tilesX, tilesY, solidsData).TileGrid;
                Add(new EffectCutout());
                base.Depth = -10501;
            }
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
        }




    }
}
