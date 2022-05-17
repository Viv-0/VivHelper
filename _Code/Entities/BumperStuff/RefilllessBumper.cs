using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/RefilllessBumper")]
    public class RefilllessBumper : Bumper {
        Action<Player> oldOnCollide;

        public RefilllessBumper(EntityData data, Vector2 offset) : base(data, offset) {
            oldOnCollide = Get<PlayerCollider>().OnCollide;
            Remove(Get<PlayerCollider>());
            Add(new PlayerCollider(OnCollide));
        }

        public void OnCollide(Player player) {
            int dashes = player.Dashes;
            float stamina = player.Stamina;
            oldOnCollide(player);
            player.Dashes = dashes;
            player.Stamina = stamina;
        }
    }
}
