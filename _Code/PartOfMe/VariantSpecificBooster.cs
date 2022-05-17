using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {
    [TrackedAs(typeof(Booster))]
    [CustomEntity("VivHelper/VariantSpecificBooster")]
    public class VariantSpecificBooster : Booster {
        private DynData<Booster> dyn;
        public bool MaddyBaddy;
        public bool killIfWrong;


        public VariantSpecificBooster(EntityData data, Vector2 offset) : base(data, offset) {
            dyn = new DynData<Booster>(this);
            MaddyBaddy = data.Bool("BadelineBooster", false);
            killIfWrong = data.Bool("killIfWrong", true);
            Remove(dyn.Get<Sprite>("sprite"));
            dyn.Set<Sprite>("sprite", VivHelperModule.spriteBank.Create("VivHelperGrayBooster"));
            Color color = MaddyBaddy ? Calc.HexToColor("9B3FB5") : Calc.HexToColor("AC3232");
            color = Color.Lerp(color, Color.White, (bool) dyn["red"] ? 0.15f : 0f);
            dyn.Get<Sprite>("sprite").SetColor(color);
            Add((Sprite) dyn["sprite"]);
            Remove(this.Get<PlayerCollider>());
            Add(new PlayerCollider(OnPlayer2));
            dyn.Set<ParticleType>("particleType", new ParticleType(Booster.P_Burst) { Color = color });
        }

        public override void Added(Scene scene) {
            base.Added(scene);
        }

        private void OnPlayer2(Player player) {
            if (SaveData.Instance.Assists.PlayAsBadeline == MaddyBaddy) {
                if (dyn.Get<float>("respawnTimer") <= 0f && dyn.Get<float>("cannotUseTimer") <= 0f && !BoostingPlayer) {
                    dyn.Set<float>("cannotUseTimer", 0.45f);
                    if ((bool) dyn["red"]) {
                        player.RedBoost(this);
                    } else {
                        player.Boost(this);
                    }
                    Audio.Play((bool) dyn["red"] ? "event:/game/05_mirror_temple/redbooster_enter" : "event:/game/04_cliffside/greenbooster_enter", Position);
                    dyn.Get<Wiggler>("wiggler").Start();
                    dyn.Get<Sprite>("sprite").Play("inside", true);
                    dyn.Get<Sprite>("sprite").FlipX = (player.Facing == Facings.Left);
                }
            } else {
                if (killIfWrong) {
                    player.Die(Vector2.Zero, true);
                }
            }
        }

        public override void Update() {
            base.Update();
            if (!killIfWrong) {
                if (SaveData.Instance.Assists.PlayAsBadeline != MaddyBaddy) {
                    dyn.Get<Sprite>("sprite").Play("outline");
                }
            }
            if (SaveData.Instance.Assists.PlayAsBadeline == MaddyBaddy && dyn.Get<Sprite>("sprite").CurrentAnimationID == "outline") {
                dyn.Get<Sprite>("sprite").Play("loop");
            }
            if (BoostingPlayer && SaveData.Instance.Assists.PlayAsBadeline != MaddyBaddy) { PlayerReleased(); }


        }
    }
}
