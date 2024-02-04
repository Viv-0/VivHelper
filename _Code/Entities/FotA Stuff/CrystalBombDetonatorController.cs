using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;


namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/CrystalBombDetonatorController")]
    public class CrystalBombDetonatorController : Entity {
        //It doesn't do anything besides be effectively a modifiable container for the two colors we want,
        //as well as the angle the particles should go in.
        public Color particleColor { get; private set; } = Color.Yellow;
        public Color baseColor { get; private set; } = Color.Purple;

        public Vector2 particleDir { get; private set; } = Vector2.UnitY;

        public float DetonationDelay { get; private set; } = 0f;

        public bool CanBeNeutralized { get; private set; } = true;

        public bool saveToSession { get; private set; } = false;

        public CrystalBombDetonatorController() : base() {
        }

        public CrystalBombDetonatorController(EntityData e, Vector2 v) : this() {
            particleColor = VivHelper.OldColorFunction(e.Attr("ParticleColor", "Yellow"));
            baseColor = VivHelper.OldColorFunction(e.Attr("BaseColor", "Purple"));
            particleDir = Vector2.UnitX.Rotate(0 - (e.Float("ParticleAngle", (float) 270f) * 0.0174533f)); //More efficient to multiply versus division, PI/180
            DetonationDelay = e.Float("DetonationDelay");
            CanBeNeutralized = e.Bool("CanBeNeutralized", true);
            saveToSession = e.Bool("Persistent", false);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (saveToSession) {
                VivHelperModule.Session.savedCBDController = this.Copy();
            }
        }

        private VivHelperModuleSession.CrystalBombDetonatorCh Copy() {
            return new VivHelperModuleSession.CrystalBombDetonatorCh() {
                particleColorHex = VivHelper.ColorToHex(particleColor),
                baseColorHex = VivHelper.ColorToHex(baseColor),
                particleDir = particleDir,
                DetonationDelay = DetonationDelay,
                CanBeNeutralized = CanBeNeutralized
            };
        }
    }
}
