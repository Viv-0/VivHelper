using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;


namespace VivHelper.Entities {
    [CustomEntity("VivHelper/LightningMuter")]
    [Tracked]
    public class LightningMuter : Entity {
        public string flag;
        public LightningRenderer lr;

        public LightningMuter(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flag = data.Attr("flag");
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (string.IsNullOrWhiteSpace(flag))
                return;
            lr = scene.Tracker.GetEntity<LightningRenderer>();

        }

        public override void Update() {
            base.Update();
            if (lr == null) {
                Console.WriteLine("LightningRenderer not found");
                lr = Scene.Tracker.GetEntity<LightningRenderer>();
                if (lr == null)
                    return;
            }
            bool b = (Scene as Level).Session.GetFlag(flag);

            if (b) {
                if (lr.AmbientSfx.Playing) {
                    lr.AmbientSfx.Stop(false);
                }
            } else if (!lr.AmbientSfx.Playing) {
                lr.StartAmbience();
            }


        }
    }
}
