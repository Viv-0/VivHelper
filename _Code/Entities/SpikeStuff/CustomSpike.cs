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
using VivHelper.Module__Extensions__Etc;

namespace VivHelper.Entities {
    [Flags]
    public enum DirectionPlus {
        Other = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        UpLeft = 5,
        DownLeft = 6,
        Right = 8,
        UpRight = 9,
        DownRight = 10
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
        public bool CanRefillOnGroundWhenNoKill;
        public PlayerCollider playerCollider;
        public DirectionPlus Direction;
        public bool NoRefillDash = true;
        public CustomSpike(Vector2 position, DirectionPlus dir, int size, bool canWallbounce = false, bool canRefillOnGroundIfNoKill = false, bool allway = false, bool blockLedge = true) : base(position) {
            Direction = dir;
            CanWallbounce = canWallbounce;
            CanRefillOnGroundWhenNoKill = canRefillOnGroundIfNoKill;
            OverrideDirectionParity = allway;
            if (size > 0) {
                bool c = true;
                switch (dir) {
                    case DirectionPlus.Up:
                        base.Collider = new Hitbox(size, 3f, 0f, -3f);

                        break;
                    case DirectionPlus.Down:
                        base.Collider = new Hitbox(size, 3f);
                        c = false;
                        break;
                    case DirectionPlus.Left:
                        base.Collider = new Hitbox(3f, size, -3f);
                        break;
                    case DirectionPlus.Right:
                        base.Collider = new Hitbox(3f, size);
                        break;
                    default:
                        return;
                }
                if (blockLedge)
                    Add(new LedgeBlocker() { Blocking = c });
                if (VivHelperModule.gravityHelperLoaded) {
                    Add(GravityHelperAPI.CreatePlayerGravityListener((_, args, f) => {
                        var ledgeBlocker = Components.Get<LedgeBlocker>();
                        if (Direction == DirectionPlus.Up)
                            ledgeBlocker.Blocking = args == 0;
                        else if (Direction == DirectionPlus.Down)
                            ledgeBlocker.Blocking = args == 1;
                    }));
                }

            }
        }

        //This is bad code i dont recommend you do it this way
        protected virtual void OnCollide(Player player) {
            if (OverrideDirectionParity) { player.Die(Vector2.Zero); return; }
            bool b = true;
            Vector2 v = Vector2.Zero;
            var c = Direction;
            if ((c & DirectionPlus.Up) > 0) {
                if (VivHelperModule.gravityHelperLoaded && GravityHelperAPI.IsPlayerInverted())
                    b &= player.Speed.Y <= 0f && (c > DirectionPlus.Up || (player.StateMachine.State == Player.StDreamDash && player.Bottom <= Bottom));
                else
                    b &= player.Speed.Y >= 0f && (c > DirectionPlus.Up || player.Bottom <= Bottom);
                if (!b)
                    return;
                v.Y -= 1;
            }
            if ((c & DirectionPlus.Down) > 0) {
                if (VivHelperModule.gravityHelperLoaded && GravityHelperAPI.IsPlayerInverted())
                    b &= player.Speed.Y >= 0f && (c > DirectionPlus.Down || player.Top >= Top);
                else  b &= player.Speed.Y <= 0f;       
                if (!b)
                    return;
                v.Y += 1;
            }
            if ((c & DirectionPlus.Left) > 0) {
                b &= player.Speed.X >= 0;
                if (!b)
                    return;
                v.X -= 1;
            }
            if ((c & DirectionPlus.Right) > 0) {
                b &= player.Speed.X <= 0;
                if (!b)
                    return;
                v.X += 1;
            }
            player.Die(v);
        }

        public static int GetSize(int h, int w, DirectionPlus dir) => (int) dir >= 4 ? h : w;

    }
}
