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
    [CustomEntity("VivHelper/TemplePortalTorch")]
    public class TemplePortalTorchV2 : Entity {
        private enum LightTypes {
            ByTrigger,
            ByRadius,
            BySwitch,
            AlwaysOn
        }
        private Sprite sprite;

        private VertexLight light;

        private BloomPoint bloom;

        private LightTypes lt;
        private float lightRadius;
        private float triggerRadius;

        private SoundSource loopSfx;

        private string flagTag;

        public TemplePortalTorchV2(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            Add(sprite = new Sprite(GFX.Game, "objects/temple/portal/portaltorch"));
            sprite.AddLoop("idle", "", 0f, default(int));
            sprite.AddLoop("lit", "", 0.08f, 1, 2, 3, 4, 5, 6);
            sprite.Play("idle");
            sprite.Origin = new Vector2(32f, 64f);
            base.Depth = 8999;

            lt = data.Enum<LightTypes>("LightTypes", LightTypes.AlwaysOn);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (lt == LightTypes.AlwaysOn) { Light(true, false, false); }
        }

        public void Light(bool a = true, bool b = true, bool c = true) {
            sprite.Play("lit");
            Add(bloom = new BloomPoint(1f, 16f));
            Add(light = new VertexLight(Color.LightSeaGreen, 0f, 32, 128));
            if (b)
                Audio.Play(a ? "event:/game/05_mirror_temple/mainmirror_torch_lit_1" : "event:/game/05_mirror_temple/mainmirror_torch_lit_2", Position);
            if (c) {
                Add(loopSfx = new SoundSource());
                loopSfx.Play("event:/game/05_mirror_temple/mainmirror_torch_loop");
            }
        }

        public override void Update() {
            base.Update();
            if (bloom != null && bloom.Alpha > 0.5f) {
                bloom.Alpha -= Engine.DeltaTime;
            }
            if (light != null && light.Alpha < 1f) {
                light.Alpha = Calc.Approach(light.Alpha, 1f, Engine.DeltaTime);
            }
            if (SceneAs<Level>().Session.GetFlag(flagTag)) {
                Light();
            }
        }
    }
}
