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
    [CustomEntity("VivHelper/HoldableBarrierJumpThru")]
    [Tracked]
    public class HoldableBarrierJumpThru : JumpThru {
        //Added for Color modification
        public HoldableBarrierColorController colorController;

        private int columns;
        private Color innerC, outerC;

        public HoldableBarrierJumpThru(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, false) {

            SurfaceSoundIndex = 32;
            columns = data.Width / 8;
            Visible = true;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            colorController = scene.Tracker.GetEntity<HoldableBarrierColorController>();
            Collidable = true;
            innerC = (colorController?.particleColor ?? VivHelper.OldColorFunction(VivHelperModule.Session.savedHBController?.particleColorHex ?? VivHelperModule.defaultHBController.particleColorHex)) * 0.5f;
            outerC = (colorController?.baseColor ?? VivHelper.OldColorFunction(VivHelperModule.Session.savedHBController?.baseColorHex ?? VivHelperModule.defaultHBController.baseColorHex)) * 0.5f;
            MTexture inner = GFX.Game["VivHelper/holdableJumpThru/00"];
            MTexture outer = GFX.Game["VivHelper/holdableJumpThru/01"];
            int num = inner.Width / 8;
            scene.Tracker.Entities[typeof(HoldableBarrier)].ForEach(e => e.Collidable = true);
            for (int i = 0; i < columns; i++) {
                int num2;
                int num3;
                if (i == 0) {
                    num2 = 0;
                    num3 = ((!CollideCheck<Solid, SwapBlock, ExitBlock>(Position + new Vector2(-1f, 0f))) ? 1 : 0);
                } else if (i == columns - 1) {
                    num2 = num - 1;
                    num3 = ((!CollideCheck<Solid, SwapBlock, ExitBlock>(Position + new Vector2(1f, 0f))) ? 1 : 0);
                } else {
                    num2 = 1 + Calc.Random.Next(num - 2);
                    num3 = Calc.Random.Choose(0, 1);
                }
                Image im = new Image(inner.GetSubtexture(num2 * 8, num3 * 8, 8, 8));
                im.X = i * 8;
                im.Color = innerC;
                Add(im);
                Image im2 = new Image(outer.GetSubtexture(num2 * 8, num3 * 8, 8, 8));
                im2.X = i * 8;
                im2.Color = outerC;
                Add(im2);
            }
            scene.Tracker.Entities[typeof(HoldableBarrier)].ForEach(e => e.Collidable = false);
            Collidable = false;
        }

    }
}
