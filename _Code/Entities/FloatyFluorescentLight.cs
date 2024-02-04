using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System.Reflection;
using MonoMod.Utils;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/FloatyLight")]
    [TrackedAs(typeof(FloatingDebris))]
    public class FloatyFluorescentLight : FloatingDebris {
        internal static List<Type> collidingTypes;

        private DynData<FloatingDebris> dyn;
        private VertexLight light; private BloomPoint point;
        private bool broken;

        public FloatyFluorescentLight(EntityData data, Vector2 offset) : base(data, offset) {
            Depth = Depths.PlayerDreamDashing;
            Remove(Get<Image>());
            dyn = new DynData<FloatingDebris>(this);
            MTexture t = null;
            broken = data.Bool("broken", false);
            if (broken) {
                if (Calc.Random.Next(0, 2) == 0) // 0 or 1
                {
                    t = GFX.Game["VivHelper/fluorescentLight/min"];
                } else {
                    t = GFX.Game["VivHelper/fluorescentLight/half"];
                }
            } else {
                t = GFX.Game["VivHelper/fluorescentLight/full"];
            }
            Image image = new Image(t);
            image.CenterOrigin();
            dyn.Set<Image>("image", null);
            dyn.Set<Image>("image", image);
            Add(dyn.Get<Image>("image"));
            Add(light = new VertexLight(Color.White, 0f, 48, 64));
            Add(point = new BloomPoint(0f, 36f));
            dyn.Set<float>("rotateSpeed", (float) (Calc.Random.Choose<int>(-6, -4, -3, -2, -1, 1, 2, 3, 4, 6) * 10) * Consts.DEG1);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            bool b = false;
            float c = dyn.Get<float>("rotateSpeed") * Calc.Random.Next(2, 13) * Engine.DeltaTime;
            dyn.Get<Image>("image").Rotation = c;
            Vector2 q = Calc.AngleToVector(c, 13);
            foreach (Type t in collidingTypes) {
                if (Scene.Tracker.Entities.TryGetValue(t, out var v)) {
                    foreach (Entity e in v) {
                        if (Collide.CheckPoint(e, Position + q) || (!broken && Collide.CheckPoint(e, Position - q))) {
                            b = true;
                            break;
                        }
                    }
                }
                if (b)
                    break;
            }
            light.Alpha = b ? 1f : 0f;
            point.Alpha = b ? 0.7f : 0f;
        }

        public override void Update() {
            base.Update();
            bool b = false;
            Vector2 q = Calc.AngleToVector(dyn.Get<Image>("image").Rotation, 13);
            foreach (Type t in collidingTypes) {
                if (Scene.Tracker.Entities.TryGetValue(t, out var v)) {
                    foreach (Entity e in v) {
                        if (Collide.CheckPoint(e, Position + q) || (!broken && Collide.CheckPoint(e, Position - q))) {
                            b = true;
                            break;
                        }
                    }
                }
                if (b)
                    break;
            }
            light.Alpha = Calc.Approach(light.Alpha, b ? 1f : 0f, Engine.DeltaTime * 0.6666f);
            point.Alpha = Calc.Approach(point.Alpha, b ? 0.7f : 0f, Engine.DeltaTime * 0.5f);
        }

        public override void DebugRender(Camera camera) {
            float rotation = dyn.Get<Image>("image").Rotation;
            Vector2 q = Calc.AngleToVector(rotation, 12);
            Draw.Pixel.Draw(Position + q, Vector2.One / 2f, Color.Orange, 2f);
            Draw.Pixel.Draw(Position - q, Vector2.One / 2f, Color.Orange, 2f);
        }
    }
}
