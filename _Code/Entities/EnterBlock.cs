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
    [CustomEntity("VivHelper/EnterBlock")]
    public class EnterBlock : Solid {
        private TileGrid tiles;

        private EffectCutout cutout;

        private float startAlpha;

        private char tileType;

        private bool primed;

        public EnterBlock(Vector2 position, float width, float height, char tileType)
            : base(position, width, height, safe: true) {
            base.Depth = -13000;
            this.tileType = tileType;
            Add(cutout = new EffectCutout());
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            EnableAssistModeChecks = false;
        }

        public EnterBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3')) {
        }


        public override void Added(Scene scene) {
            base.Added(scene);
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int) (base.X / 8f) - tileBounds.Left;
            int y = (int) (base.Y / 8f) - tileBounds.Top;
            int tilesX = (int) base.Width / 8;
            int tilesY = (int) base.Height / 8;
            tiles = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
            Add(tiles);
            Add(new TileInterceptor(tiles, highPriority: false));
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            cutout.Alpha = (tiles.Alpha = 0.1f);
            Collidable = false;
            primed = false;

        }

        public override void Update() {
            base.Update();

            if (primed) {
                if (Collidable) {
                    cutout.Alpha = (tiles.Alpha = Calc.Approach(tiles.Alpha, 1f, Engine.DeltaTime));
                } else if (!CollideCheck<Player>()) {
                    Collidable = true;
                    Audio.Play("event:/game/general/passage_closed_behind", base.Center);
                }
            } else if (CollideCheck<Player>()) {
                primed = true;
                cutout.Alpha = (tiles.Alpha = 0.175f);
            }
        }

        public override void Render() {
            if (tiles.Alpha >= 1f) {
                Level level = base.Scene as Level;
                if (level.ShakeVector.X < 0f && level.Camera.X <= (float) level.Bounds.Left && base.X <= (float) level.Bounds.Left) {
                    tiles.RenderAt(Position + new Vector2(-3f, 0f));
                }
                if (level.ShakeVector.X > 0f && level.Camera.X + 320f >= (float) level.Bounds.Right && base.X + base.Width >= (float) level.Bounds.Right) {
                    tiles.RenderAt(Position + new Vector2(3f, 0f));
                }
                if (level.ShakeVector.Y < 0f && level.Camera.Y <= (float) level.Bounds.Top && base.Y <= (float) level.Bounds.Top) {
                    tiles.RenderAt(Position + new Vector2(0f, -3f));
                }
                if (level.ShakeVector.Y > 0f && level.Camera.Y + 180f >= (float) level.Bounds.Bottom && base.Y + base.Height >= (float) level.Bounds.Bottom) {
                    tiles.RenderAt(Position + new Vector2(0f, 3f));
                }
            }
            base.Render();
        }
    }
}
