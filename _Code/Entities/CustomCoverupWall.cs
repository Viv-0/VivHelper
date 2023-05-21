using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CustomCoverupWall")]
    public class CustomCoverupWall : Entity {
        protected char fillTile;

        protected TileGrid tiles;

        protected EffectCutout cutout;

        public float alpha;
        public float currentAlpha;

        public string flag;
        public bool inverted;
        public bool renderPlayerOver;
        public bool instant;
        public bool blendIn;

        public CustomCoverupWall(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            fillTile = data.Char("tiletype", '3');
            base.Depth = data.Int("Depth", -13000);
            base.Collider = new Hitbox(data.Width, data.Height);
            Add(cutout = new EffectCutout());
            alpha = Calc.Clamp(data.Float("alpha", 1f), 0f, 1f);
            flag = data.Attr("flag", "");
            inverted = data.Bool("inverted", false);
            instant = data.Bool("instant", true);
            renderPlayerOver = data.Bool("RenderPlayerOver");
            blendIn = data.Bool("blendIn");
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (!blendIn) {
                tiles = GFX.FGAutotiler.GenerateBox(fillTile, (int) base.Width / 8, (int) base.Height / 8).TileGrid;
            } else {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (base.X / 8f) - tileBounds.Left;
                int y = (int) (base.Y / 8f) - tileBounds.Top;
                int tilesX = (int) base.Width / 8;
                int tilesY = (int) base.Height / 8;
                tiles = GFX.FGAutotiler.GenerateOverlay(fillTile, x, y, tilesX, tilesY, solidsData).TileGrid;
            }
            Add(tiles);
            Add(new TileInterceptor(tiles, highPriority: true));
            tiles.Alpha = cutout.Alpha = flag != "" && (Scene as Level).Session.GetFlag(flag) == inverted ? 0f : alpha;
        }

        public override void Update() {
            base.Update();
            currentAlpha = flag != "" && (Scene as Level).Session.GetFlag(flag) == inverted ? 0f : alpha;
            tiles.Alpha = cutout.Alpha = Calc.Approach(tiles.Alpha, currentAlpha, instant ? 2f : Engine.DeltaTime / 3f);
        }

        public override void Render() {
            base.Render();
            if (renderPlayerOver && VivHelper.TryGetAlivePlayer(out Player p) && CollideCheck(p)) {
                p.Render();
            }
        }
    }
}
