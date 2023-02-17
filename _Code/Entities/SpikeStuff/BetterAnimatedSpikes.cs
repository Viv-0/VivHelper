using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using static MonoMod.InlineRT.MonoModRule;
using Celeste.Mod;

namespace VivHelper.Entities.SpikeStuff {
    public class BetterAnimatedSpikes : CustomSpike
    {
        private int size;
        private Vector2 offset;
        private PlayerCollider pc;
        private Sprite sprite;
        private Color? color;
        private bool rotation;
        private bool centerVert;
        private Vector2 imageOffset;

        public BetterAnimatedSpikes(EntityData data, Vector2 offset, DirectionPlus dir) : base(data.Position + offset, dir, GetSize(data.Height, data.Width, dir)) {
            base.Depth = -1;
            size = GetSize(data.Height, data.Width, dir);
            offset = offset;
            Add(pc = new PlayerCollider(OnCollide));
            AddTag(Tags.TransitionUpdate);
            var str = data.NoEmptyString("directory", "danger/tentacles");
            centerVert = str == "danger/tentacles" || data.Bool("centeredVertically");
            var mts = GFX.Game.orig_GetAtlasSubtextures(str + "_" + dir.ToString().ToLower());
            if(mts == null || mts.Count == 0) {
                mts = GFX.Game.GetAtlasSubtextures(str);
                rotation = true;
            }
            if(mts.Count < 1) {
                RemoveSelf();
                return;
            }
            sprite = new Sprite(GFX.Game, str);
            sprite.AddLoop("loop", data.Float("frameDelta", 0.1f), mts.ToArray());
            sprite.Texture = mts[0];
            string c = data.Attr("Color", "White");
            if (c != "Rainbow")
                color = VivHelper.ColorFixWithNull(c) ?? Color.White;
            Visible = true;
            Collidable = true;
            if (!data.Bool("DoNotAttach", false))
                Add(new StaticMover {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    JumpThruChecker = IsRiding
                });
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            int spriteLength = (!rotation && (int) Direction > 3) ? sprite.Texture.Height : sprite.Texture.Width; // Crashes due to sprite is null
            for (int i = 0; i < 3; i++) {
                AddSprite(i);
            }
        }

        protected void AddSprite(float i) {
            Sprite s = VivHelper.CloneSprite(sprite);
            if (rotation) {
                s.Play("loop", randomizeFrame: true);
                s.JustifyOrigin(0.5f, centerVert ? 0.5f : 1f);
                sprite.Position = ((Direction == DirectionPlus.Up || Direction == DirectionPlus.Down) ? Vector2.UnitX : Vector2.UnitY) * (i + 0.5f) * s.Texture.Width;
                sprite.Scale.X = Calc.Random.Choose(-1, 1);
                switch (Direction) {

                    case DirectionPlus.Up:
                        sprite.Rotation = 0f;
                        sprite.Y++;
                        break;
                    case DirectionPlus.Right:
                        sprite.Rotation = (float) Math.PI / 2f;
                        sprite.X--;
                        break;
                    case DirectionPlus.Left:
                        sprite.Rotation = (float) Math.PI / -2f;
                        sprite.X++;
                        break;
                    case DirectionPlus.Down:
                        sprite.Rotation = (float) Math.PI;
                        sprite.Y--;
                        break;
                }
            } else {
                switch (Direction) {
                    case DirectionPlus.Up:
                        s.JustifyOrigin(0.5f, centerVert ? 0.5f : 1f);
                        s.Position = Vector2.UnitX * (i + 0.5f) * s.Texture.Width + Vector2.UnitY;
                        break;
                    case DirectionPlus.Down:
                        s.JustifyOrigin(0.5f, centerVert ? 0.5f : 0f);
                        s.Position = Vector2.UnitX * (i + 0.5f) * s.Texture.Width - Vector2.UnitY;
                        break;
                    case DirectionPlus.Right:
                        s.JustifyOrigin(centerVert ? 0.5f : 0f, 0.5f);
                        s.Position = Vector2.UnitY * (i + 0.5f) * s.Texture.Height - Vector2.UnitX;
                        break;
                    case DirectionPlus.Left:
                        s.JustifyOrigin(centerVert ? 0.5f : 1f, 0.5f);
                        s.Position = Vector2.UnitY * (i + 0.5f) * s.Texture.Height + Vector2.UnitX;
                        break;
                }
            }
            if (color != null)
                s.SetColor(color.Value);
            Add(s);
        }

        public override void Update() {
            base.Update();
            if (color == null)
                SetSpikeColor();
        }
        public void SetSpikeColor() {
            foreach (Component component in Components) {
                if (component is Image image) {
                    image.Color = VivHelper.GetHue(Scene, Position + image.Position);
                }
            }
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

        public void SetOrigins(Vector2 origin) {
            foreach (Component component in base.Components) {
                Image image = component as Image;
                if (image != null) {
                    Vector2 vector = origin - Position;
                    image.Origin = image.Origin + vector - image.Position;
                    image.Position = vector;
                }
            }
        }

        private bool IsRiding(Solid solid) {
            switch (Direction) {
                default:
                    return false;
                case DirectionPlus.Up:
                    return CollideCheckOutside(solid, Position + Vector2.UnitY);
                case DirectionPlus.Down:
                    return CollideCheckOutside(solid, Position - Vector2.UnitY);
                case DirectionPlus.Left:
                    return CollideCheckOutside(solid, Position + Vector2.UnitX);
                case DirectionPlus.Right:
                    return CollideCheckOutside(solid, Position - Vector2.UnitX);
            }
        }

        private bool IsRiding(JumpThru jumpThru) {
            if (Direction != 0) {
                return false;
            }
            return CollideCheck(jumpThru, Position + Vector2.UnitY);
        }
    }
}
