using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CustomBirdPath")]
    public class CustomBirdPath : BirdPath {
        //This is boutta be a 5 minute speedcode.
        DynData<BirdPath> dyn;

        public CustomBirdPath(EntityData data, Vector2 offset, EntityID id) : base(id, data, offset) {
            dyn = new DynData<BirdPath>(this);
            //Removes the old sprite
            Remove(Get<Sprite>());
            //retrieves the string data necessary to create sprite
            string t = data.Attr("SpritePath", "characters/bird/flyup");
            //Constructs the new sprite
            Sprite sprite = new Sprite(GFX.Game, t);
            sprite.AddLoop("flyupIdle", "", 0.1f, 10, 11, 12, 13, 14, 15);
            sprite.Add("flyupRoll", "", 0.1f, "flyupIdle");
            //Replaces the sprite data from BirdPath to our new values.
            dyn.Set<Sprite>("sprite", sprite);
            //Readds the sprite as a Component, to fix any instances of sprite.Play to actually play.
            Add(dyn.Get<Sprite>("sprite"));
            //in the constructor it starts by playing flyupRoll so since we removed the old one we need to do it again.
            dyn.Get<Sprite>("sprite").Play("flyupRoll");

            //Sets the new bird's trail color.
            dyn.Set<Color>("trailColor", VivHelper.OldColorFunction(data.Attr("TrailColor", "639bff")));
        }
    }
}
