using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Graphics;

namespace VivHelper.Module__Extensions__Etc {
    public static class MTextureExtViv {
        public static void DrawColoredOutline(this MTexture self, Vector2 position, Color outlineColor) {
            float scaleFix = self.ScaleFix;
            Rectangle clipRect = self.ClipRect;
            Vector2 origin = -self.DrawOffset / scaleFix;
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (i != 0 || j != 0) {
                        Monocle.Draw.SpriteBatch.Draw(self.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outlineColor, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }
            Monocle.Draw.SpriteBatch.Draw(self.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }

        public static void DrawColoredOutline(this MTexture self, Vector2 position, Color outlineColor, Color mainColor) {
            float scaleFix = self.ScaleFix;
            Rectangle clipRect = self.ClipRect;
            Vector2 origin = -self.DrawOffset / scaleFix;
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (i != 0 || j != 0) {
                        Monocle.Draw.SpriteBatch.Draw(self.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outlineColor, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }
            Monocle.Draw.SpriteBatch.Draw(self.Texture.Texture_Safe, position, clipRect, mainColor, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }

        public static void DrawColoredOutline(this MTexture self, Vector2 position, Vector2 origin, Color color, Color outlineColor, Vector2 scale, float rotation, SpriteEffects flip) {
            float scaleFix = self.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = self.ClipRect;
            Vector2 origin2 = (origin - self.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (i != 0 || j != 0) {
                        Monocle.Draw.SpriteBatch.Draw(self.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outlineColor, rotation, origin2, scale, flip, 0f);
                    }
                }
            }
            Monocle.Draw.SpriteBatch.Draw(self.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, flip, 0f);
        }

        public static Func<int, float> defaultGlowFunction = (i) => (float) Math.Pow(1.414214, -i);

        public static void DrawColoredGlow(this MTexture self, Vector2 position, Vector2 origin, Color color, Color outlineColor, Vector2 scale, float rotation, SpriteEffects flip, Func<int, float> fadeFormula = null, int GlowLayers = 2) {
            if (fadeFormula == null)
                fadeFormula = (i) => (float) Math.Pow(1.414214, -i);
            float scaleFix = self.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = self.ClipRect;
            Vector2 origin2 = (origin - self.DrawOffset) / scaleFix;
            Texture2D texture = self.Texture.Texture_Safe;
            for (int h = GlowLayers; h > 0; h--) {
                Draw.SpriteBatch.Draw(texture, position - new Vector2(h, h), clipRect, outlineColor * fadeFormula(h), rotation, origin2, scale, flip, 0f);
                Draw.SpriteBatch.Draw(texture, position + new Vector2(h, 0 - h), clipRect, outlineColor * fadeFormula(h), rotation, origin2, scale, flip, 0f);
                Draw.SpriteBatch.Draw(texture, position + new Vector2(0 - h, h), clipRect, outlineColor * fadeFormula(h), rotation, origin2, scale, flip, 0f);
                Draw.SpriteBatch.Draw(texture, position + new Vector2(h, h), clipRect, outlineColor * fadeFormula(h), rotation, origin2, scale, flip, 0f);
            }
            Draw.SpriteBatch.Draw(self.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, flip, 0f);
        }

    }
}
