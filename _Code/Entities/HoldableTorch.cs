using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities
{
    public class HoldableTorch : Holdable
    {
        public static readonly Dictionary<string, Color[]> colortypes = new Dictionary<string, Color[]>
        {
            {"Default", new Color[] {Color.LightYellow, Color.Lerp(Color.Yellow, Color.LightYellow, .9f), Color.Lerp(Color.LightYellow, Color.White, .1f)} },
            {"Green", new Color[] {Color.LightGreen, Color.Lerp(Color.Green, Color.LightGreen, .9f), Color.Lerp(Color.LightGreen, Color.White, .1f) } },
            {"Red", new Color[] { Calc.HexToColor("e44040"), Calc.HexToColor("f76868"), Calc.HexToColor("fa1818") } },
            {"Blue", new Color[] {Color.SkyBlue, Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, .7f), Color.Lerp(Color.SkyBlue, Color.LightSkyBlue, .4f) } },
            {"Purple", new Color[] {Color.Lerp(Color.Purple, Color.White, .4f), Color.Lerp(Color.Violet, Color.Purple, .85f), Color.Lavender } },
            {"Orange", new Color[] {Color.Orange, Color.Lerp(Color.Red, Color.Orange, .9f), Color.Lerp(Color.Orange, Color.Yellow, .2f) } },
            {"Sunset", new Color[] {Color.Lerp(Calc.HexToColor("e07597"), Color.White, .25f), Calc.HexToColor("b62956"), Calc.HexToColor("eeb5c7") } },
            {"Gray", new Color[] { Calc.HexToColor("d8d8d8"), Calc.HexToColor("c1c1c1"), Calc.HexToColor("eaeaea") } } };
        public Sprite sprite;
        private Color[] color;
        public VertexLight vLight;
        //CustomVars
        public int r1 = 48;
        public int r2 = 64;
        public float alpha = 1f;



    }
}
