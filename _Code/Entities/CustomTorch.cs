using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {

    [TrackedAs(typeof(CustomTorch))]
    public class CustomTorch : Torch {
        ParticleType P_OnLight2;
        public Color color;
        public float alpha;
        public int startFade, endFade;
        public string FlagName;
        public bool lit;
        public bool startLit;
        public bool unlightOnDeath;
        private DynData<Torch> torchData;
        public VertexLight light;
        public BloomPoint bloom;
        public CustomTorch(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id) {
            base.Depth = 2000;
            startLit = data.Bool("startLit", false);
            unlightOnDeath = data.Bool("unlightOnDeath", false);
            color = VivHelper.ColorFix(data.Attr("Color", "Cyan"));
            alpha = data.Float("Alpha", 1f);
            FlagName = "torch_" + id.Key;
            if (alpha < 0 || alpha > 1) { alpha = 1f; }
            startFade = Math.Abs(data.Int("startFade", 48));
            endFade = Math.Abs(data.Int("endFade", 64));
            torchData = new DynData<Torch>(this);
            base.Collider = new Hitbox(4f, 4f, -2f, -2f);
            Remove(torchData.Get<Sprite>("sprite"));
            Remove(torchData.Get<VertexLight>("light"));
            Remove(torchData.Get<BloomPoint>("bloom"));
            Add(light = new VertexLight(color, 1f, startFade, endFade));
            Add(bloom = new BloomPoint(alpha / 2f, 8f));
            torchData["sprite"] = VivHelperModule.spriteBank.Create("CustomTorch");
            torchData.Get<Sprite>("sprite").Color = VivHelper.ColorFix(data.Attr("spriteColor", "ffffff"));
            Add(torchData.Get<Sprite>("sprite"));
            bloom.Visible = false;
            light.Visible = false;
            Collider = new Circle(data.Float("RegisterRadius", 4f));
            Add(new PlayerCollider(OnPlayer));
            P_OnLight2 = new ParticleType(P_OnLight);
        }

        [MonoModLinkTo("Celeste.Entity", "System.Void Added(Monocle.Scene)")]
        public void Entity_Added(Scene scene) { }

        public override void Added(Scene scene) {

            if (startLit || SceneAs<Level>().Session.GetFlag(FlagName)) {
                bloom.Visible = (light.Visible = true);
                lit = true;
                Collidable = false;
                torchData.Get<Sprite>("sprite").Play("on");
            } else if (unlightOnDeath) { bloom.Visible = false; light.Visible = false; }
            Entity_Added(scene);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Vector4 v = P_OnLight.Color.ToVector4();
            v.X *= .8f;
            v.Y *= 0.6f;
            Color c = new Color(v);
            P_OnLight2.Color = c;

        }

        private void OnPlayer(Player player) {
            if (!lit) {
                Audio.Play("event:/game/05_mirror_temple/torch_activate", Position);
                lit = true;
                bloom.Visible = true;
                light.Visible = true;
                Collidable = false;
                torchData.Get<Sprite>("sprite").Play("turnOn");
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
                tween.OnUpdate = delegate (Tween t) {
                    light.StartRadius = startFade + (1f - t.Eased) * 32f;
                    light.EndRadius = endFade + (1f - t.Eased) * 32f;
                    bloom.Alpha = alpha + alpha * (1f - t.Eased);
                };
                Add(tween);
                if (!unlightOnDeath)
                    SceneAs<Level>().Session.SetFlag(FlagName);
                SceneAs<Level>().ParticlesFG.Emit(P_OnLight2, 12, Position, new Vector2(3f, 3f));
            }
        }
    }
}
