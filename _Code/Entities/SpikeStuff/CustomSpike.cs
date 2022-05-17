using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace VivHelper.Entities {
    [Flags]
    public enum Directions {
        Other = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }

    //A wrapper class for Custom Spikes so that we can add one IL hook to deal with all spiked wall wallbounces. Does not work for non-cardinal spikes.
    [Tracked(true)]
    public class CustomSpike : Entity {
        public static bool AddWallCheck(Player player, bool flag, int dir) {
            if (!player.Scene.Tracker.Entities.ContainsKey(typeof(CustomSpike)))
                return flag;
            int q = dir <= 0 ? 8 : 4;
            foreach (CustomSpike entity in player.Scene.Tracker.GetEntities<CustomSpike>()) {
                if ((((int) entity.Direction & q) > 0) && player.CollideCheck(entity, player.Position + Vector2.UnitX * dir * 5f) && !entity.CanWallbounce) {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        public bool CanWallbounce;
        public bool OverrideDirectionParity;
        public PlayerCollider playerCollider;
        public Directions Direction;
        public bool NoRefillDash = true;
        public CustomSpike(Vector2 position, Directions dir, int size, bool canWallbounce = false, bool allway = false, bool blockLedge = true) : base(position) {
            Direction = dir;
            CanWallbounce = canWallbounce;
            OverrideDirectionParity = allway;
            if (size > 0) {
                switch (dir) {
                    case Directions.Up:
                        base.Collider = new Hitbox(size, 3f, 0f, -3f);
                        if (blockLedge)
                            Add(new LedgeBlocker());
                        break;
                    case Directions.Down:
                        base.Collider = new Hitbox(size, 3f);
                        break;
                    case Directions.Left:
                        base.Collider = new Hitbox(3f, size, -3f);
                        if (blockLedge)
                            Add(new LedgeBlocker());
                        break;
                    case Directions.Right:
                        base.Collider = new Hitbox(3f, size);
                        if (blockLedge)
                            Add(new LedgeBlocker());
                        break;
                }
            }
        }

        protected virtual void OnCollide(Player player) {
            if (OverrideDirectionParity) { player.Die(Vector2.Zero); return; }

            bool b = true;
            Vector2 v = Vector2.Zero;
            var c = (int) Direction;
            if ((c & 1) > 0) {
                b &= player.Speed.Y >= 0f && (c > 1 || player.Bottom <= Bottom);
                v.Y -= 1;
                if (!b)
                    return;
            }
            if ((c & 2) > 0) {
                b &= player.Speed.Y <= 0;
                v.Y += 1;
                if (!b)
                    return;
            }
            if ((c & 4) > 0) {
                b &= player.Speed.X >= 0;
                v.X -= 1;
                if (!b)
                    return;
            }
            if ((c & 8) > 0) {
                b &= player.Speed.X <= 0;
                v.X += 1;
                if (!b)
                    return;
            }
            player.Die(v);
        }
        public static int GetSize(int h, int w, Directions dir) => (int) dir >= 4 ? h : w;
    }
}
