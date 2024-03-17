using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using MonoMod.Utils;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/ReskinnablePuffer")]
    public class ReskinnablePuffer : Puffer {
        DynData<Puffer> dyn;

        public ReskinnablePuffer(EntityData e, Vector2 v) : base(e, v) {
            dyn = new DynData<Puffer>(this);
            Sprite sprite = new Sprite(GFX.Game, e.Attr("Directory").TrimEnd('/') + "/");
            //We calmly assume that it was set up right
            sprite.AddLoop("idle", "idle", 0.08f);
            sprite.AddLoop("alerted", "alerted", 0.08f);
            sprite.AddLoop("hidden", "hidden", 0.08f);
            sprite.Add("alert", "alert", 0.08f, "alerted");
            sprite.Add("explode", "explode", 0.08f, "hidden");
            MTexture[] _ = sprite.Animations["alert"].Frames.Reverse().ToArray();
            sprite.Add("unalert", 0.08f, "idle", _);
            sprite.Add("recover", "recover", 0.05f, "idle");
            sprite.CenterOrigin();
            Remove(Get<Sprite>()); //Removes the Sprite from the Puffer
            dyn.Set<Sprite>("sprite", sprite); //Sets it to our new Puffer skin, again, this will probably crash if it isn't set up properly.
            Add(dyn.Get<Sprite>("sprite")); //Readds the reskin to the Puffer, keeping everything else vanilla.
            dyn.Get<Sprite>("sprite").Play("idle");

        }
    }
}
