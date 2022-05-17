using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.Utils;
using Mono.Cecil.Cil;

namespace VivHelper.PartOfMe {
    [CustomEntity("VivHelper/VariantKevin")]
    public class VariantKevin : CrushBlock {
        public static ParticleType P_Activate_Maddy;
        public static ParticleType P_Activate_Baddy;

        public static void Load() {
            IL.Celeste.CrushBlock.ctor_Vector2_float_float_Axes_bool += CrushBlock_ctor;
            IL.Celeste.CrushBlock.ActivateParticles += CrushBlock_ActivateParticles;
        }

        private static void CrushBlock_ActivateParticles(ILContext il) {

        }

        private static void CrushBlock_ctor(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(instr => instr.MatchLdarg(0), instr => instr.MatchLdcR4(0.2f))) {
                ILLabel label = cursor.MarkLabel();
                if (cursor.TryGotoPrev(instr => instr.MatchLdarg(0), instr => instr.MatchLdarg(0), instr => instr.MatchLdsfld(typeof(GFX).GetField("SpriteBank")))) {
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.EmitDelegate<Func<CrushBlock, bool>>(e => e is VariantKevin);
                    cursor.Emit(OpCodes.Brtrue, label);
                }
            }
        }

        DashCollision oldDashCollide;
        //Maddy = false, Baddy = true
        private bool MaddyBaddy;
        private DynData<CrushBlock> dyn;
        private string dir;
        private CrushBlock.Axes axes;

        public VariantKevin(EntityData data, Vector2 offset) : base(data, offset) {
            MaddyBaddy = data.Bool("Baddy", false);
            oldDashCollide = OnDashCollide;
            OnDashCollide = new DashCollision(NewDashCollide);
            axes = data.Enum<Axes>("axes", Axes.Both);
            dyn = new DynData<CrushBlock>(this);
            string temp = MaddyBaddy ? "Baddy" : "Maddy";
            dir = "VivHelper/VariantKevin/" + temp;
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(dir + "/block");
            MTexture idle;
            switch (axes) {
                default:
                    idle = atlasSubtextures[3];
                    break;
                case Axes.Horizontal:
                    idle = atlasSubtextures[1];
                    break;
                case Axes.Vertical:
                    idle = atlasSubtextures[2];
                    break;
            }
            string giant = dyn.Get<bool>("giant") ? "_giant" : "_";

            Sprite s = VivHelperModule.spriteBank.Create("VivHelper_" + temp.ToLower() + giant + "crushblock_face");
            s.Position = new Vector2(base.Width, base.Height) / 2f;
            s.Play("idle");
            s.OnLastFrame = delegate (string f) {
                if (f == "hit") {
                    s.Play(dyn.Get<string>("nextFaceDirection"));
                }
            };
            dyn.Set<Sprite>("face", s);
            Add(s);
            int num = (int) (base.Width / 8f) - 1;
            int num2 = (int) (base.Height / 8f) - 1;
            AddImage(idle, 0, 0, 0, 0, -1, -1);
            AddImage(idle, num, 0, 3, 0, 1, -1);
            AddImage(idle, 0, num2, 0, 3, -1, 1);
            AddImage(idle, num, num2, 3, 3, 1, 1);
            for (int i = 1; i < num; i++) {
                AddImage(idle, i, 0, Calc.Random.Choose(1, 2), 0, 0, -1);
                AddImage(idle, i, num2, Calc.Random.Choose(1, 2), 3, 0, 1);
            }
            for (int j = 1; j < num2; j++) {
                AddImage(idle, 0, j, 0, Calc.Random.Choose(1, 2), -1);
                AddImage(idle, num, j, 3, Calc.Random.Choose(1, 2), 1);
            }

        }

        private DashCollisionResults NewDashCollide(Player player, Vector2 dir) {
            if (MaddyBaddy == SaveData.Instance.Assists.PlayAsBadeline)
                return oldDashCollide(player, dir);
            return DashCollisionResults.NormalCollision;
        }

        private void AddImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0) {
            MTexture subtexture = idle.GetSubtexture(tx * 8, ty * 8, 8, 8);
            Vector2 vector = new Vector2(x * 8, y * 8);
            if (borderX != 0) {
                Image image = new Image(subtexture);
                image.Color = Color.Black;
                image.Position = vector + new Vector2(borderX, 0f);
                Add(image);
            }
            if (borderY != 0) {
                Image image2 = new Image(subtexture);
                image2.Color = Color.Black;
                image2.Position = vector + new Vector2(0f, borderY);
                Add(image2);
            }
            Image image3 = new Image(subtexture);
            image3.Position = vector;
            Add(image3);
            dyn.Get<List<Image>>("idleImages").Add(image3);
            if (borderX != 0 || borderY != 0) {
                if (borderX < 0) {
                    Image image4 = new Image(GFX.Game[dir + "/lit_left"].GetSubtexture(0, ty * 8, 8, 8));
                    dyn.Get<List<Image>>("activeLeftImages").Add(image4);
                    image4.Position = vector;
                    image4.Visible = false;
                    Add(image4);
                } else if (borderX > 0) {
                    Image image5 = new Image(GFX.Game[dir + "/lit_right"].GetSubtexture(0, ty * 8, 8, 8));
                    dyn.Get<List<Image>>("activeRightImages").Add(image5);
                    image5.Position = vector;
                    image5.Visible = false;
                    Add(image5);
                }
                if (borderY < 0) {
                    Image image6 = new Image(GFX.Game[dir + "/lit_top"].GetSubtexture(tx * 8, 0, 8, 8));
                    dyn.Get<List<Image>>("activeTopImages").Add(image6);
                    image6.Position = vector;
                    image6.Visible = false;
                    Add(image6);
                } else if (borderY > 0) {
                    Image image7 = new Image(GFX.Game[dir + "/lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8));
                    dyn.Get<List<Image>>("activeBottomImages").Add(image7);
                    image7.Position = vector;
                    image7.Visible = false;
                    Add(image7);
                }
            }
        }
    }
}
