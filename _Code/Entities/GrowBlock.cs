﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using System.Reflection;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CelsiusGrowBlock")]
    [Tracked]
    public class GrowBlock : Solid {
        private static FieldInfo lightOccludeBounds = typeof(LightOcclude).GetField("bounds", BindingFlags.Instance | BindingFlags.NonPublic);

        private char tileType;

        private RenderTarget2D buffer;

        private float width;

        private float height;

        private Vector2 move;

        private TileGrid tileGrid;

        private LightOcclude lightOcclude;

        private float alphaA;

        private float alphaB;

        private float alphaC;

        private Vector2 offset;

        private string flag;
        private Level level;
        private int factor;
        private int baseDist;

        public GrowBlock(Vector2 position, char tileType, float width, float height, float moveX, float moveY, int factor, int baseDist, string flag)
            : base(position, width, height, safe: true) {
            base.Depth = -9999;
            this.width = width;
            this.height = height;
            this.tileType = tileType;
            move = new Vector2(moveX, moveY);
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            BlockWaterfalls = false;
            this.flag = flag;
            this.factor = factor * factor;
            Tag = Tags.TransitionUpdate;
            Add(new BeforeRenderHook(BeforeRender));
        }

        public GrowBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, data.Int("moveX"), data.Int("moveY", 1), data.Int("factor", 56), data.Int("baseDist", 24), data.Attr("flag", "")) {
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (scene is Level l)
                level = l;
            Add(tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int) width / 8, (int) height / 8).TileGrid);
            Add(lightOcclude = new LightOcclude());
            Resize();
            if (CollideCheck<Player>()) {
                RemoveSelf();
            }
        }

        public override void Update() {
            base.Update();
            if (level != null && !string.IsNullOrEmpty(flag))
                Visible = Collidable = level.Session?.GetFlag(flag) ?? true;
            Resize();
        }

        public void Resize() {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null) {
                float num = (Position + new Vector2(width * 0.5f, height * 0.5f) - entity.ExactPosition).Abs().LengthSquared();
                float num2 = Math.Max(width, height) + baseDist;
                num2 *= num2;
                float val = Math.Max(0f, num - num2) / factor;
                val = Math.Min(1f, Math.Max(val, 0f));
                val = 1f - val;
                alphaA = Ease.QuadOut(val);
                alphaB = Ease.SineInOut(val);
                alphaC = Math.Min(1f, 2f - 2f * Ease.SineOut(1f - val));
                offset = move * (1f - alphaA) * 0.25f * new Vector2(width, height);
                lightOcclude.Alpha = val;
                lightOccludeBounds.SetValue(lightOcclude, (Rectangle?) new Rectangle((int) ((1f - alphaB) * 0.5f * width + offset.X), (int) ((1f - alphaB) * 0.5f * height + offset.Y), (int) (width * alphaB), (int) (height * alphaB)));
                base.Hitbox.Width = width * alphaB;
                base.Hitbox.Height = height * alphaB;
                base.Hitbox.Position = (1f - alphaB) * 0.5f * new Vector2(width, height) + offset;
            }
        }
        public void BeforeRender() {
            if (buffer != null || tileGrid == null)
                return;
            buffer = new RenderTarget2D(Engine.Graphics.GraphicsDevice, (int) width, (int) height);  
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Matrix identity = Matrix.Identity;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, identity);
            tileGrid.RenderAt(Vector2.Zero);
            Draw.SpriteBatch.End();
        }
        public override void Render() {
            // After thoroughly trying further optimizations without the Buffer, I've come to the recognition that the Buffer is the best way to do this.
            // The only much better variation that results in the same outcome is baking the Texture to the MapData directly, which i should probably do
            if (CullHelper.IsRectangleVisible(Hitbox.AbsoluteLeft, Hitbox.AbsoluteTop, Hitbox.Width, Hitbox.Height, 4) && buffer != null && alphaC > 0f) {
                Draw.SpriteBatch.Draw(buffer, Position + 0.5f * new Vector2(width, height) + offset, buffer.Bounds, Color.White * alphaC, (1f - alphaA) * 0.25f, new Vector2(width, height) * 0.5f, alphaB, SpriteEffects.None, 0f);
            }
        }
    }
}
