using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    internal class BoostDashRefill : RefillBase {
        public BoostDashRefill(EntityData data, Vector2 offset) : base(data, offset) {
            p_shatter = Bumper.P_Launch;
            p_glow = Bumper.P_Ambience;
            p_regen = Bumper.P_Ambience;
            outline = new Image(GFX.Game["VivHelper/TSSboostrefill/outline"]);
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, "VivHelper/TSSboostrefill/"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            /*
            Add(flash = new Sprite(GFX.Game, "VivHelper/TSSboostrefill/"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate {
                flash.Visible = false;
            };
            flash.CenterOrigin();*/
        }

        protected override void OnPlayer(Player player) {
        }
    }
}
