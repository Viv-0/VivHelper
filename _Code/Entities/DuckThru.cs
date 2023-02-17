using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod;
using System.Collections;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/FallThru")]
    public class DuckThru : JumpthruPlatform {
        private float playerDuckTimer = 0;
        private float FallTime = 0.35f;
        private bool IgnoreOnlyThis;

        public DuckThru(EntityData data, Vector2 offset) : base(data, offset) {
            FallTime = data.Float("FallTime", 0.35f);
            IgnoreOnlyThis = data.Bool("IgnoreOnlyThis", false);
        }

        public override void Update() {
            base.Update();
            if (VivHelper.TryGetPlayer(out Player player) && player.Ducking && HasPlayerRider()) {
                playerDuckTimer += Engine.DeltaTime;
                if (playerDuckTimer >= FallTime) {
                    player.Add(new Coroutine(FallThru(player), true));
                }
            } else {
                playerDuckTimer = 0f;
            }
        }

        public IEnumerator FallThru(Player player) {
            if (player != null && !player.Dead) {
                GoThrough(player, true);
            }
            player.Speed = new Vector2(0f, 60f);
            yield return 0.25f;
            while (!VivHelper.TryGetAlivePlayer(out player)) //Fixes up some janky stuff
            {
                yield return null;
            }
            GoThrough(player, false);
        }

        public void GoThrough(Player p, bool on) {
            if (IgnoreOnlyThis) {
                Collidable = !on;
            } else {
                p.IgnoreJumpThrus = on;
            }
        }
    }
}
