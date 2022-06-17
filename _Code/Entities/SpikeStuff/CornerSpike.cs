using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities.SpikeStuff {

    [Tracked]
    [CustomEntity("VivHelper/CornerSpike")]
    public class CornerSpike : CustomSpike {
        internal static Dictionary<string, Action<CornerSpike>> attrToAction = new Dictionary<string, Action<CornerSpike>>(4)
        {
            {"UpLeft", c =>
            {
                c.Direction = (Directions)5; //Up = 1 + Left = 4
                c.image.JustifyOrigin(Vector2.One);
                c.image.RenderPosition = Vector2.One;
                if (c.InnerSpike)
                {
                    c.Collider = new ColliderList(new Hitbox(5, 3, -8, -3), new Hitbox(3, 5, -3, -8));
                }
                else
                {
                    c.Collider = new Hitbox(3,3,-3,-3);
                }
            }
            },
            {"DownLeft", c =>
            {
                c.Direction = (Directions)6; //Down = 2 + Left = 4
                c.image.JustifyOrigin(Vector2.UnitX);
                c.image.RenderPosition = new Vector2(1, -1);
                if (c.InnerSpike)
                {
                    c.Collider = new ColliderList(new Hitbox(5, 3, -8, 0), new Hitbox(3, 5, -3, 3));
                }
                else
                {
                    c.Collider = new Hitbox(3, 3, -3, 0);
                }
            } },
            {"UpRight", c =>
            {
                c.Direction = (Directions)9; //Up = 1 + Right = 8
                c.image.JustifyOrigin(Vector2.UnitY);
                c.image.RenderPosition = new Vector2(-1, 1);
                if (c.InnerSpike)
                {
                    c.Collider = new ColliderList(new Hitbox(5, 3, 3, -3), new Hitbox(3, 5, 0, -8));
                }
                else
                {
                    c.Collider = new Hitbox(3, 3, 0, -3);
                }
            } },
            {"DownRight", c =>
            {
                c.Direction = (Directions)10; //Down = 2 + Right = 8
                c.image.JustifyOrigin(Vector2.Zero);
                c.image.RenderPosition = -Vector2.One;
                if (c.InnerSpike)
                {
                    c.Collider = new ColliderList(new Hitbox(5, 3, 3, 0), new Hitbox(3, 5, 0, 3));
                }
                else
                {
                    c.Collider = new Hitbox(3, 3);
                }
            } }
        };

        public readonly bool InnerSpike;
        private Image image;
        private Color color;
        private bool rainbow;
        private Vector2 imageOffset;
        private bool killFormat;

        public CornerSpike(EntityData data, Vector2 offset) : base(data.Position + offset, Directions.Other, 0, false, false) {
            var a = data.Attr("EdgeDirection", "");
            string b = a;
            if (a.Trim().StartsWith("Inner")) {
                InnerSpike = true;
                b = a.Substring(5);
            }
            var s = data.Attr("type", "default");
            string t;
            switch (s) {
                case "default":
                    t = "danger/spikes/corners/default_";
                    break;
                case "outline":
                    t = "danger/spikes/corners/outline_";
                    break;
                default:
                    t = s + "_";
                    break;
            }
            t += (InnerSpike ? "Inner" + b : b);
            image = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(t)));
            attrToAction[b].Invoke(this);
            color = data.Color("Color");
            rainbow = color == Color.Transparent;
            Add(new PlayerCollider(OnCollide));
            if (!data.Bool("DoNotAttach"))
                Add(new StaticMover {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    JumpThruChecker = IsRiding,
                    OnEnable = OnEnable,
                    OnDisable = OnDisable
                });
            killFormat = data.Bool("AlternateKillFormat", false);
            AddTag(Tags.TransitionUpdate);
        }

        public void SetSpikeColor(Vector2 pos) {
            color = VivHelper.GetHue(Scene, Position + pos);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (!rainbow)
                image.Color = color;
            Add(image);
        }

        public override void Update() {
            base.Update();
            if (Scene == null) { return; }
        }

        private void OnEnable() {
            Active = (Visible = (Collidable = true));
        }

        private void OnDisable() {
            Active = Collidable = Visible = false;
        }

        private void OnShake(Vector2 amount) {
            imageOffset += amount;
        }

        public override void Render() {
            Vector2 position = Position;
            Position += imageOffset;
            if (rainbow) { SetSpikeColor(image.Position); image.Color = color; }
            image.Render();
            base.Render();
            Position = position;
        }

        private bool IsRiding(Solid solid) {
            switch ((int) Direction) {
                case 5: //UpLeft
                    return CollideCheckOutside(solid, Position + Vector2.One);
                case 6: //DownLeft
                    return CollideCheckOutside(solid, Position + new Vector2(1, -1));
                case 9: //UpRight
                    return CollideCheckOutside(solid, Position + new Vector2(-1, 1));
                case 10: //DownRight
                    return CollideCheckOutside(solid, Position - Vector2.One);
                default:
                    return false;
            }
        }

        private bool IsRiding(JumpThru jumpThru) {
            if (Direction != 0) {
                return false;
            }
            return CollideCheck(jumpThru, Position + Vector2.UnitY);
        }

        protected override void OnCollide(Player player) {
            if (InnerSpike) {
                ColliderList q = Collider as ColliderList;
                switch ((int) Direction) {
                    case 5:
                        if (Collide.CheckRect(player, q.colliders[0].Bounds) && player.Speed.Y >= 0f && player.Bottom <= q.colliders[0].AbsoluteBottom)
                            player.Die(new Vector2(0f, -1f));
                        if (Collide.CheckRect(player, q.colliders[1].Bounds) && player.Speed.X >= 0f)
                            player.Die(new Vector2(-1f, 0f));
                        return;
                    case 6:
                        if (Collide.CheckRect(player, q.colliders[0].Bounds) && player.Speed.Y <= 0f)
                            player.Die(new Vector2(0f, -1f));
                        if (Collide.CheckRect(player, q.colliders[1].Bounds) && player.Speed.X >= 0f)
                            player.Die(new Vector2(-1f, 0f));
                        return;
                    case 9:
                        if (Collide.CheckRect(player, q.colliders[0].Bounds) && player.Speed.Y >= 0f && player.Bottom <= q.colliders[0].AbsoluteBottom)
                            player.Die(new Vector2(0f, -1f));
                        if (Collide.CheckRect(player, q.colliders[1].Bounds) && player.Speed.X <= 0f)
                            player.Die(new Vector2(1f, 0f));
                        return;
                    case 10:
                        if (Collide.CheckRect(player, q.colliders[0].Bounds) && player.Speed.Y <= 0f)
                            player.Die(new Vector2(0f, -1f));
                        if (Collide.CheckRect(player, q.colliders[1].Bounds) && player.Speed.X <= 0f)
                            player.Die(new Vector2(1f, 0f));
                        return;
                }
            } else if (killFormat) {
                base.OnCollide(player);
            } else {
                Vector2 v = Vector2.One;
                switch ((int) Direction) {
                    case 5:
                        v = -Vector2.One;
                        break;
                    case 6:
                        v = new Vector2(-1, 1);
                        break;
                    case 9:
                        v = new Vector2(1, -1);
                        break;
                }
                if (player.Speed.Length() <= 0.05f || Vector2.Dot(Vector2.Normalize(v), Vector2.Normalize(player.Speed)) < 0f)
                    player.Die(v * 3);
            }
        }
    }
}
