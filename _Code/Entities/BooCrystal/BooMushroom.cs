using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using System.Reflection;
using MonoMod.Utils;

namespace VivHelper.Entities {
    public class BooMushroom {
        private static float speed = 120f;
        private static Color color;
        private static PlayerSpriteMode mode;

        public static void Load() {
            On.Celeste.Player.OnCollideH += BooCollideH;
            On.Celeste.Player.OnCollideV += BooCollideV;

        }

        public static void Unload() {
            On.Celeste.Player.OnCollideH -= BooCollideH;
            On.Celeste.Player.OnCollideV -= BooCollideV;
        }

        private static void BooCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            if (self.StateMachine.State == VivHelperModule.BooState && self.CollideCheck<Solid, SolidTiles>()) {
                return;
            } else {
                orig.Invoke(self, data);
            }
        }

        private static void BooCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            if (self.StateMachine.State == VivHelperModule.BooState) {
                if (self.CollideCheck<Solid, SolidTiles>())
                    return;
                self.MuffleLanding = true;
            }
            orig.Invoke(self, data);
        }

        public static void BooBegin(Player player) {
            if (player.Speed.Y > 0f) {
                player.Speed.Y *= 0.5f;
            }
            player.Stamina = 110f;
            player.Light.Alpha = 0f;
            mode = player.Sprite.Mode;
            player.Remove(player.Sprite);
            player.Sprite = new PlayerSprite(PlayerSpriteMode.Playback);
            player.Add(player.Sprite);
            color = player.Hair.Color;
            player.Hair.Color = Extensions.ColorCopy(Color.Gray, 0.75f);
            player.ResetSpriteNextFrame(player.Sprite.Mode);

        }

        public static void BooEnd(Player player) {
            player.Light.Alpha = 1f;
            player.Remove(player.Sprite);
            player.Sprite = new PlayerSprite(mode);
            player.Add(player.Sprite);
            player.Hair.Color = color;
            player.ResetSpriteNextFrame(player.Sprite.Mode);
        }
        public static int BooUpdate(Player player) {
            foreach (VertexLight vl in player.Scene.Tracker.GetComponents<VertexLight>()) {
                if (vl.Alpha != 0f && Vector2.Subtract(vl.Center, player.Center).Length() <= vl.EndRadius) {
                    Audio.Play(SFX.char_bad_disappear);
                    return 0;
                }
            }
            Vector2 value = Input.Aim.Value.SafeNormalize();
            value = value.SafeNormalize();
            if (value.X == 0f) {
                player.Speed.X = Calc.Approach(player.Speed.X, 0f, 180f * Engine.DeltaTime);
            } else {
                if (Math.Abs(player.Speed.X) > speed && Math.Sign(player.Speed.X) == Math.Sign(value.X)) {
                    player.Speed.X = Calc.Approach(player.Speed.X, speed * value.X, 120f * Engine.DeltaTime);
                } else {
                    player.Speed.X = Calc.Approach(player.Speed.X, speed * value.X, 180f * Engine.DeltaTime);
                }
            }
            if (value.Y == 0f) {
                player.Speed.Y = Calc.Approach(player.Speed.Y, 0f, 180f * Engine.DeltaTime);
            } else {
                if (Math.Abs(player.Speed.Y) > speed && Math.Sign(player.Speed.Y) == Math.Sign(value.Y)) {
                    player.Speed.Y = Calc.Approach(player.Speed.Y, speed * value.Y, 120f * Engine.DeltaTime);
                } else {
                    player.Speed.Y = Calc.Approach(player.Speed.Y, speed * value.Y, 180f * Engine.DeltaTime);
                }
            }
            return VivHelperModule.BooState;

        }
    }
    [CustomEntity("VivHelper/BooCrystal")]
    public class BooMushroomCrystal : Refill {
        DynData<Refill> dyn;
        public BooMushroomCrystal(EntityData data, Vector2 offset) : base(data, offset) {
            Remove(Get<BloomPoint>());
            Remove(Get<VertexLight>());
            Remove(Get<PlayerCollider>());
            Add(new PlayerCollider(OnPlayer));
            dyn = new DynData<Refill>(this);
            Remove(dyn.Get<Sprite>("sprite"));
            dyn.Set<Sprite>("sprite", new Sprite(GFX.Game, "VivHelper/entities/ghostIdle"));
            Add(dyn.Get<Sprite>("sprite"));
            dyn.Get<Sprite>("sprite").AddLoop("idle", "", 0.1f);
            dyn.Get<Sprite>("sprite").Play("idle");
            dyn.Get<Sprite>("sprite").CenterOrigin();
        }

        public void OnPlayer(Player player) {
            if (player.StateMachine.State != VivHelperModule.BooState) {
                player.StateMachine.State = VivHelperModule.BooState;
                Audio.Play(SFX.game_assist_dreamblockbounce);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                dyn.Set<float>("respawnTimer", 5f);
            }
        }
    }
}
