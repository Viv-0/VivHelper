using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public class SceneryAdder {
        public static void LoadingThreadAddendum(List<LevelData> levels, Level level) {
            foreach (LevelData l in levels) {
                if (l.Entities == null)
                    continue;
                foreach (EntityData e in l.Entities) {
                    if (e.Name == "VivHelper/SceneryAdder") {
                        string t = (string) e.Values["Texture"];
                        if (!string.IsNullOrWhiteSpace(t) && GFX.Game[t] != GFX.Game.GetFallback()) //Thread-safe :)
                        {
                            int X = (int) e.Position.X / 8;
                            int Y = (int) e.Position.Y / 8;
                            int SubtextureX = e.Int("subtextureX");
                            int SubtextureY = e.Int("subtextureY");
                            if (e.Bool("Foreground")) {
                                CustomAddition(level.SolidTiles.Tiles, GFX.Game[t], X, Y, SubtextureX, SubtextureY);
                                CustomAddition(level.FgTilesLightMask, GFX.Game[t], X, Y, SubtextureX, SubtextureY);
                            } else
                                CustomAddition(level.BgTiles.Tiles, GFX.Game[t], X, Y, SubtextureX, SubtextureY);
                        }
                    }
                }
            }
        }

        public static void CustomAddition(TileGrid grid, MTexture texture, int x, int y, int subTextureX = 0, int subTextureY = 0) {
            if (x > -1 && x < grid.TilesX && y > -1 && y < grid.TilesY) {
                grid.Tiles[x, y] = texture.GetSubtexture(VivHelper.mod(subTextureX * 8, texture.Width), VivHelper.mod(subTextureY * 8, texture.Height), 8, 8);
            }
        }

    }
}
