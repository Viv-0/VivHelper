using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using VivHelper;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/HoldableBarrierController = OldLoad", "VivHelper/HoldableBarrierController2 = NewLoad")]
    public class HoldableBarrierColorController : Entity {
        //It doesn't do anything besides be effectively a modifiable container for the two colors we want,
        //as well as the angle the particles should go in.
        public Color particleColor = Calc.HexToColor("5a6ee1");
        public Color baseColor = Calc.HexToColor("5a6ee1");

        public Vector2 particleDir = Vector2.UnitY;

        public bool solidOnRelease = true;

        public bool saveToSession = false;

        public bool toggleBloomRendering = true;

        //Priority determinance is basically my easy workaround for if there is one loaded ColorController that is default by accident.
        public int version = 0;

        public static Entity OldLoad(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new HoldableBarrierColorController(entityData, offset, 0);
        public static Entity NewLoad(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new HoldableBarrierColorController(entityData, offset, 1);


        public HoldableBarrierColorController() : base() {
        }

        public HoldableBarrierColorController(EntityData e, Vector2 v, int version) : this() {
            particleColor = VivHelper.OldColorFunction(e.Attr("ParticleColor", "5a6ee1"));
            baseColor = VivHelper.OldColorFunction(e.Attr("EdgeColor", "5a6ee1"));

            particleDir = Vector2.UnitX.Rotate(0 - AngleVersion(e.Float("ParticleAngle", version == 1 ? 270f : (Consts.PIover2 * 3)), version));
            solidOnRelease = e.Bool("SolidOnRelease", true);
            saveToSession = e.Bool("Persistent", false);
            toggleBloomRendering = e.Bool("renderBloom", true);
        }

        internal float AngleVersion(float f, int version) {
            switch (version) {
                case 0:
                    return f;
                case 1:
                    return f * 0.0174533f; //Multiplying is easier on the computer than division.
                //The default case should never occur because I'm internally defining version to be 0 or 1 in this case. If it happens it means some other mod messed with it, or something has memory leaked which if that happens you'd have way bigger problems, and crash with a different error.
                default:
                    throw new Exception("Invalid version type. This should never appear, and if you see it DM or ping in Celestecord @Viv#1113 on Discord, and send this error log with it.");
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (saveToSession) {
                VivHelperModule.Session.savedHBController = this.Copy();
            }
        }

        private VivHelperModuleSession.HoldableBarrierCh Copy() {
            return new VivHelperModuleSession.HoldableBarrierCh() {
                particleColorHex = VivHelper.ColorToHex(particleColor),
                baseColorHex = VivHelper.ColorToHex(baseColor),
                particleDir = particleDir,
                solidOnRelease = solidOnRelease
            };
        }
    }
}
