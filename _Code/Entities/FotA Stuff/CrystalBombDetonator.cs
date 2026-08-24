using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [Tracked(false)]
    [CustomEntity("VivHelper/CrystalBombDetonator")]
    public class CrystalBombDetonator2 : Solid {
        // Duration of flashing anim
        public float Flash = 0f;

        // Used in the shape of the "wave" on the renderer
        public float Solidify = 0f;
        private float solidifyDelay = 0f;

        // Used to propagate state
        private List<CrystalBombDetonator2> adjacent = new List<CrystalBombDetonator2>();

        public CrystalBombDetonatorController colorController;

        // If the field is currently in its flashing state (used to propagate Flashing state to adjacent fields)
        public bool Flashing = false;

        public bool inDestruction = false;
        private const float DestroyTime = 1f;
        private float timer = 0f;

        // Particle array and a list of speeds
        private List<Vector2> particles = new List<Vector2>();
        private readonly static float[] speeds = new float[] {
            12f,
            20f,
            40f
        };

        // Cached method reference, first calculated in Update
        private static MethodInfo bombExplosionMethod;

        public CrystalBombDetonator2(Vector2 position, float width, float height) : base(position, width, height, false) {
            Collidable = false;
            for (int num = 0; (float) num < Width * Height / 16f; num++)
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
        }

        public CrystalBombDetonator2(EntityData data, Vector2 offset) : this(data.Position + offset, (float) data.Width, (float) data.Height) { }

        public override void Added(Scene scene) {
            base.Added(scene);
            scene.Tracker.GetEntity<CrystalBombDetonatorRenderer>().Track(this);
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            scene.Tracker.GetEntity<CrystalBombDetonatorRenderer>().Untrack(this);
        }

        public void Destroy() // This function just sets the inDestruction variable to true. Update and Render do the rest of the visuals.
        {
            inDestruction = true;
            Scene.CollideInto<CrystalBombDetonator2>(new Rectangle((int) X, (int) Y - 2, (int) Width, (int) Height + 4), adjacent);
            Scene.CollideInto<CrystalBombDetonator2>(new Rectangle((int) X - 2, (int) Y, (int) Width + 4, (int) Height), adjacent);
            foreach (CrystalBombDetonator2 c in adjacent) {
                if (!c.inDestruction)
                    c.Destroy();
            }
            Add(new Coroutine(DestroyCoroutine()));

        }

        public IEnumerator DestroyCoroutine() {
            yield return 0.1f;
            Scene.Tracker.GetEntity<CrystalBombDetonatorRenderer>().Untrack(this);
            RemoveSelf();
        }

        public override void Update() {
            if (colorController == null) {
                colorController = Scene.Tracker.GetEntity<CrystalBombDetonatorController>();
            }
            if (Flashing) {


                Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
                if (Flash <= 0f) {
                    Flashing = false;
                }

            } else {
                if (solidifyDelay > 0f)
                    solidifyDelay -= Engine.DeltaTime;
                else if (Solidify > 0f)
                    Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
            }

            for (int i = 0; i < particles.Count; i++) {
                Vector2 newPosition = particles[i] + Vector2.UnitY * speeds[i % speeds.Length] * Engine.DeltaTime;
                newPosition.Y %= Height - 1f;
                particles[i] = newPosition;
            }
            base.Update();

            CheckForBombs();
        }

        public void OnTriggerDetonation() {
            Flash = 1f;
            Solidify = 1f;
            solidifyDelay = 1f;
            Flashing = true;
            Scene.CollideInto<CrystalBombDetonator2>(new Rectangle((int) X, (int) Y - 2, (int) Width, (int) Height + 4), adjacent);
            Scene.CollideInto<CrystalBombDetonator2>(new Rectangle((int) X - 2, (int) Y, (int) Width + 4, (int) Height), adjacent);
            foreach (CrystalBombDetonator2 crystalBombDetonator in adjacent) {
                if (!crystalBombDetonator.Flashing)
                    crystalBombDetonator.OnTriggerDetonation();
            }
            adjacent.Clear();
        }

        public override void Render() {
            Color[] colors = new Color[] { colorController?.particleColor ?? VivHelper.OldColorFunction(VivHelperModule.Session.savedCBDController?.particleColorHex ?? VivHelperModule.defaultCBDController.particleColorHex), colorController?.baseColor ?? VivHelper.OldColorFunction(VivHelperModule.Session.savedCBDController?.baseColorHex ?? VivHelperModule.defaultCBDController.baseColorHex) };
            colors[0] *= 0.5f;
            colors[1] *= 0.5f;
            foreach (Vector2 value in particles) {
                Draw.Pixel.Draw(Position + value, Vector2.Zero, colors[0] * (inDestruction ? 1 - Flash : 1));
            }
            if (Flashing)
                Draw.Rect(Collider, colors[1] * Flash);
        }

        private void CheckForBombs() {
            if (inDestruction)
                return; //If it's being destroyed, don't destroy any more bombs. This shouldn't ever crash.
            foreach (Entity bomb in this.CollideAll(s => s.Name.Contains("CrystalBomb") && !s.Name.Contains("Detonator"))) {
                bombExplosionMethod = bomb.GetType().GetMethod("Explode", BindingFlags.Instance | BindingFlags.NonPublic);
                if (bombExplosionMethod != null)
                    bombExplosionMethod.Invoke(bomb, null);
                else {
                    Logger.Log("VivHelper", "Yo why does it not work");
                }
                if (!Flashing)
                    OnTriggerDetonation();
            }
        }
    }
}
