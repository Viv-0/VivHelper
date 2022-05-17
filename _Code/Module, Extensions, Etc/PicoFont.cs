using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod;

namespace VivHelper.Module__Extensions__Etc {
    /*
    public static class PicoFont
    {
		//charSet[X][0] = pico8font expanded, charSet[X][1] = pico8font expanded + outline
		public static Dictionary<char, MTexture[]> charSet;


		internal const float CharSpacing = 4f;
		internal const float LineSpacing = 6f; //extra padding

		public static void LoadContent()
        {
			MTexture font1 = GFX.Game["VivHelper/pico8font"]; //Size is 64x36 (4x16, 6x6)
			MTexture font2 = GFX.Game["VivHelper/pico8outline"]; //Size is 80x42 (5x16, 7x6)
			for(char c = ' '; c <= '~'; c++)
            {
				int i = c - 32;
				var cGroup = new MTexture[2];
				cGroup[0] = font1.GetSubtexture()
            }
		}

		public static Vector2 MeasureString(string text, bool Outline)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (text.Length == 0)
			{
				return Vector2.Zero;
			}

			Vector2 zero = Vector2.Zero;
			float num = 0f;
			bool flag = true;
			foreach (char c in text)
			{
				if(c < 32 || c > 126)
                {
					throw new Exception("Invalid Character: \'" + c + "\' not supported in font. Font supports ASCII 32 thru 126, or [space] thru ~");
                }
				switch (c)
				{
					case '\n':
						zero.X = Math.Max(zero.X, num);
						zero.Y += LineSpacing;
						num = 0f;
						flag = true;
						continue;
					case '\r':
						continue;
				}
				if (flag)
				{
					num += 3;
					flag = false;
				}
				else
				{
					num += 4;
				}
			}
			zero.X = Math.Max(zero.X, num);
			zero.Y += LineSpacing;
			return zero;
		}
	}*/
}
