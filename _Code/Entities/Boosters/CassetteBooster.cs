using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities.Boosters;

namespace VivHelper.Entities {
    public class CassetteBooster : CustomBooster {

        private static FieldInfo CassetteBlockManager_currentIndex = typeof(CassetteBlockManager).GetField("currentIndex", BindingFlags.Instance | BindingFlags.NonPublic);

        public bool ignoreSwitch;
        public int flagIndices;
        public Circle hitboxDistanceDifferential;
        public int index = 0;
        private bool red;
        private Sprite spriteGreen, spriteRed;

        public CassetteBooster(EntityData data, Vector2 offset) : base(data.Position + offset)  {
            flagIndices = data.Int("log2idx", 1);
            int dist = data.Int("distance", 0);
            if (dist > 0) {
                hitboxDistanceDifferential = new Circle(10f + dist, 0f, 2f);
            }
            Depth = -8500;
            string xmlPath = data.Attr("spriteXML", "booster");
            if (string.IsNullOrWhiteSpace(xmlPath))
                xmlPath = "booster";
            spriteGreen = GFX.SpriteBank.Create(xmlPath);
            spriteRed = GFX.SpriteBank.Create(xmlPath + "Red");
        }

    /*public override void Awake(Scene scene) {
            base.Awake(scene);

            var orig = Get<PlayerCollider>().OnCollide;
            Get<PlayerCollider>().OnCollide = delegate (Player player) {
                boosterData.Set("red", red);
                orig(player);
            };
    }*/

    public override void Update() {

            base.Update();
            if(hitboxDistanceDifferential != null) {
                Collider oldCollider = base.Collider;
                base.Collider = hitboxDistanceDifferential;
                ignoreSwitch = CollideCheck<Player>();
                base.Collider = oldCollider;
            }
            if (!ignoreSwitch) red = ((1 << (int) CassetteBlockManager_currentIndex.GetValue(Scene.Tracker.GetEntity<CassetteBlockManager>())) & flagIndices) > 0;

        }
    }
}
