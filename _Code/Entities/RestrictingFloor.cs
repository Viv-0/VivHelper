using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Celeste;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using System.Reflection;
using VivHelper;

namespace VivHelper.Entities {
    public static class RestrictingEntityHooks {
        public static FieldInfo dreamJump = typeof(Player).GetField("dreamJump");
        public static FieldInfo onGround = typeof(Player).GetField("onGround");

        public static void Load() {
            /// Hooks for Dash and Stamina Refill handled at <see cref="VivHelperModule.Player_origUpdate"/>
            On.Celeste.Player.Jump += Player_Jump;
            /// Hooks for Wallbounce handled at <see cref="VivHelperModule.Player_WallJumpCheck"/>
        }

        public static void Unload() {
            /// Hooks for Dash and Stamina Refill handled at <see cref="VivHelperModule.Player_origUpdate"/>
            On.Celeste.Player.Jump -= Player_Jump;
            /// Hooks for Wallbounce and WallKick handled at <see cref="VivHelperModule.Player_WallJumpCheck"/>
        }

        private static void Player_Jump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
            if ((bool) onGround.GetValue(self) && !(bool) dreamJump.GetValue(self) && self.CollideAnyWhere<RestrictingFloor>(e => e.PreventJumps))
                return;
            orig(self, particles, playSfx);
        }

        // This resolves all of the potential code conflicts by rerunning some code in exchange for reducing custom wallbounces and wallkicks to a single hook in Celeste's base code.
        public static bool AddWallCheck(Player player, int dir) {
            Vector2 v = player.Position + Vector2.UnitX * dir * 3f;
            if (player.StateMachine.State != Player.StStarFly && Input.GrabCheck && !SaveData.Instance.Assists.NoGrabbing && player.Stamina > 0f && player.Holding == null && !ClimbBlocker.Check(player.Scene, player, v)) {
                return player.CollideAnyWhere<RestrictingWall>(r => r.DisableClimbJumps, v);
            } else if (player.DashAttacking && SuperWallJumpAngleCheck(player)) {
                return player.CollideAnyWhere<RestrictingWall>(r => r.DisableWallbounce, v);
            } else
                return player.CollideAnySetMatchWhere<RestrictingWall>(v, r => r.DisableNeutrals, r => r.DisableNonNeutrals);
        }

        private static bool SuperWallJumpAngleCheck(Player player) {
            if (Math.Abs(player.DashDir.X) <= 0.2f) {
                return player.DashDir.Y <= -0.75f;
            }
            return false;
        }
    }

    [CustomEntity("VivHelper/RestrictingFloor")]
    [Tracked]
    public class RestrictingFloor : Entity {
        public bool PreventDashRefill;
        public bool PreventStaminaRefill;
        public bool PreventJumps;
        public MTexture[] tiles;
        public Color color;
        public Shaker shaker;
        public Vector2 imageOffset;
        public new float Width;

        public RestrictingFloor(EntityData data, Vector2 offset) {
            Width = data.Width;
            base.Collider = new Hitbox(data.Width, 3, 0, -3);
            PreventDashRefill = data.Bool("NoGroundRefillDash");
            PreventStaminaRefill = data.Bool("NoGroundRefillStamina");
            PreventJumps = data.Bool("BlockJumps");
            string customTexture = data.Attr("TextureOverride", "");
            if (string.IsNullOrWhiteSpace(customTexture)) {
                customTexture = "VivHelper/stickyUp";
            }
            color = data.Color("color", Color.White);
            tiles = new MTexture[3];
            MTexture texture = GFX.Game[customTexture];
            for (int i = 0; i < 3; i++) {
                tiles[i] = texture.GetSubtexture(8 * i, 0, 8, 8);
            }
            if (data.Bool("AttachToSolid", true)) {
                Add(new StaticMover() {
                    OnEnable = OnEnable,
                    OnDisable = OnDisable,
                    SolidChecker = IsRiding,
                    JumpThruChecker = IsRiding,
                    OnShake = OnShake
                });
            }
        }

        public void SetColor(Color color) => this.color = color;

        private void OnDisable() {
            SetColor(Color.Lerp(Color.Gray, color, 0.5f));
            Collidable = false;
        }

        private void OnEnable() {
            SetColor(color);
            Collidable = true;
        }
        private void OnShake(Vector2 amount) {
            imageOffset += amount;
        }

        private bool IsRiding(Solid solid) {
            return CollideCheckOutside(solid, Position + Vector2.UnitY);
        }

        private bool IsRiding(JumpThru jumpThru) {
            return CollideCheck(jumpThru, Position + Vector2.UnitY);
        }

        public override void Render() {
            Vector2 position = Position;
            Position += imageOffset;
            if (Width < 0)
                throw new Exception("Width of Restriction Floor is less than 0. why did you do this. Bruh.");
            if (Width <= 8) {
                tiles[1].Draw(Position, Vector2.Zero, color, 1f, 0f);
            } else if (Width <= 16) {
                tiles[0].Draw(Position, Vector2.Zero, color, 1f, 0f);
                tiles[2].Draw(Position + new Vector2(8, 0), Vector2.Zero, color, 1f, 0f);
            } else {
                tiles[0].Draw(Position, Vector2.Zero, color, 1f, 0f);
                for (int i = 1; i < (Width / 8) - 1; i++) {
                    tiles[1].Draw(Position + new Vector2(i * 8, 0f), Vector2.Zero, color, 1f, 0f, (Microsoft.Xna.Framework.Graphics.SpriteEffects) (i % 2));
                }
                tiles[2].Draw(Position + new Vector2(Width - 8, 0), Vector2.Zero, color, 1f, 0f);
            }
            Position = position;
        }
    }

    [CustomEntity("VivHelper/RestrictingWall")]
    [Tracked]
    public class RestrictingWall : Entity {
        //Value less than 0 = no Grabbing the wall. Clean setup.
        public float StaminaLossMultiplier;
        //Value <= 0 = no climbing on the wall.
        public float StaminaMoveMultiplier;
        public bool DisableNeutrals;
        public bool DisableNonNeutrals;
        public bool DisableClimbJumps;
        public bool DisableWallbounce;
        public bool DisableWallboost;
        public Image[] tiles;
        public Color color;
        public Vector2 imageOffset;
        public bool left;

        public RestrictingWall(EntityData data, Vector2 offset) : base(data.Position) {
            StaminaLossMultiplier = data.Bool("NoGrabbing", false) ? -1 : Math.Max(data.Float("StaminaLossMultiplier", 1f), 0);
            StaminaMoveMultiplier = data.Bool("NoClimbing", false) ? 0 : data.Float("StaminaPowerMultiplier", 1f);
            DisableNeutrals = data.Bool("DisableNeutralWallkicks");
            DisableNonNeutrals = data.Bool("DisableAngledWallkicks");
            DisableClimbJumps = data.Bool("DisableClimbJumping");
            DisableWallbounce = data.Bool("DisableWallbounce");
            DisableWallboost = data.Bool("DisableWallBoost");
            color = data.Color("color", Color.White);
            left = data.Bool("Left", false);
            if (data.Bool("AttachToSolid", true)) {
                Add(new StaticMover() {
                    OnEnable = OnEnable,
                    OnDisable = OnDisable,
                    SolidChecker = IsRiding,
                    OnShake = OnShake
                });
            }

        }

        public void SetColor(Color color) {
            foreach (Image img in tiles) {
                img.Color = color;
            }
        }

        private void OnDisable() {
            SetColor(Color.Lerp(Color.Gray, color, 0.5f));
            Collidable = false;
        }

        private void OnEnable() {
            SetColor(color);
            Collidable = true;
        }

        private bool IsRiding(Solid solid) {
            return left ? CollideCheckOutside(solid, Position + Vector2.UnitX) : CollideCheckOutside(solid, Position - Vector2.UnitX);
        }

        private void OnShake(Vector2 amount) {
            imageOffset += amount;
        }
        public override void Render() {
            Vector2 position = Position;
            Position += imageOffset;
            base.Render();
            Position = position;
        }
    }
}
