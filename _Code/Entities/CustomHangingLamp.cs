using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CustomHangingLamp")]
    public class CustomHangingLamp : Entity {
        public readonly int Length;

        private List<Sprite> sprites = new List<Sprite>();

        private BloomPoint bloom;

        private VertexLight light;

        private float speed;

        private float rotation;

        private float soundDelay;

        private SoundSource sfx;

        private string AudioPath;

        private float InvWeight; //Inverse Weight, we divide at constructor for efficiency

        private float AnimSpeed;

        private float lightDistance;

        private bool drawOutline;

        public CustomHangingLamp(EntityData e, Vector2 position) {
            Position = e.Position + position + Vector2.UnitX * 4f;
            Length = Math.Max(16, e.Height);
            base.Depth = 2000;
            string directory = e.Attr("directory", "VivHelper/customHangingLamp/").Trim().TrimEnd('/') + "/";
            AnimSpeed = Math.Max(e.Float("AnimationSpeed", 0.2f), 1 / 60);
            //Sprites
            Sprite sprite;
            string q = "";
            if (e.Has("Suffix")) {
                q = "/" + Chooser<string>.FromString<string>(e.Attr("Suffix")).Choose();
            }
            //Addendum: Added checking the size of each texture in width and height, trusting that the player will be smart and make each sprite for the animation the same size.
            GFX.Game.PushFallback(null);
            MTexture a = GFX.Game.GetAtlasSubtextures(directory + "base" + q)[0];
            if (a == null)
                throw new Exception("Missing file at Graphics/Atlases/Gameplay/" + directory + "base" + q + "00");
            int aW = a.Width;
            int aH = a.Height;
            MTexture b = GFX.Game.GetAtlasSubtextures(directory + "chain" + q)[0];
            if (b == null)
                throw new Exception("Missing file at Graphics/Atlases/Gameplay/" + directory + "chain" + q + "00");
            int bW = b.Width;
            int bH = b.Height;
            MTexture c = GFX.Game.GetAtlasSubtextures(directory + "lamp" + q)[0];
            if (c == null)
                throw new Exception("Missing file at Graphics/Atlases/Gameplay/" + directory + "lamp" + q + "00");
            int cW = c.Width;
            int cH = c.Height;
            GFX.Game.PopFallback();


            //Base
            Sprite @base = new Sprite(GFX.Game, directory + "base"); //this is necessary to not produce one of the same sprite.
            @base.Position = Position;
            @base.AddLoop("main", q, AnimSpeed);
            @base.Origin.X = aW / 2;
            @base.Play("main");
            for (int i = 0; i < Length - 8; i += bH) //Chains
            {
                sprite = new Sprite(GFX.Game, directory + "chain");
                sprite.Position = Position;
                sprite.AddLoop("main", q, AnimSpeed);
                sprite.Origin = new Vector2((bW / 2f), -i);
                sprite.Play("main");
                sprites.Add(sprite);
            }

            //Lamp
            sprite = new Sprite(GFX.Game, directory + "lamp");
            sprite.Position = Position;
            sprite.AddLoop("main", "", AnimSpeed);
            sprite.Origin.X = cW / 2;
            sprite.Origin.Y = -(Length - cH);
            sprite.Play("main");
            sprites.Add(@base);
            sprites.Add(sprite);

            //Other
            Add(bloom = new BloomPoint(Vector2.UnitY * (Length - cH), Calc.Clamp(e.Float("BloomAlpha", 1f), 0f, 1f), Calc.Clamp(e.Float("BloomRadius", 48f), 0f, 128f)));
            Add(light = new VertexLight(Vector2.UnitY * (Length - cH), VivHelper.ColorFix(e.Attr("LightColor", "White")), Calc.Clamp(e.Float("LightAlpha", 1f), 0f, 1f), Calc.Clamp(e.Int("LightFadeIn", 24), 0, 120), Calc.Clamp(e.Int("LightFadeOut", 48), 0, 120)));
            AudioPath = e.Attr("AudioPath", "event:/game/02_old_site/lantern_hit");
            InvWeight = 1f / Math.Max(e.Float("WeightMultiplier", 1f), 0.025f); //Efficiency good
            Add(sfx = new SoundSource());
            if (bH == cH) {
                base.Collider = new Hitbox(bW, Length, -(bW / 2f));
            } else {
                Hitbox h1, h2;
                h1 = new Hitbox(bW, Length - cH, -(bW / 2f));
                h2 = new Hitbox(cW, cH, -(cW / 2f), Length - cH);
                base.Collider = new ColliderList(h1, h2);
            }
            lightDistance = Length - cH / 2f;
            light.Position = Vector2.UnitY * lightDistance;
            drawOutline = e.Bool("DrawOutline", true);
        }

        public override void Update() {
            base.Update();
            soundDelay -= Engine.DeltaTime;
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null && base.Collider.Collide(entity)) {
                speed = (0f - entity.Speed.X) * 0.005f * ((entity.Y - base.Y) / (float) Length) * InvWeight;
                if (Math.Abs(speed) < 0.1f) {
                    speed = 0f;
                } else if (soundDelay <= 0f) {
                    sfx.Play(AudioPath);
                    soundDelay = 0.25f;
                }
            }
            float num = ((Math.Sign(rotation) == Math.Sign(speed)) ? 8f : 6f);
            if (Math.Abs(rotation) < 0.5f) {
                num *= 0.5f;
            }
            if (Math.Abs(rotation) < 0.25f) {
                num *= 0.5f;
            }
            float value = rotation;
            speed += (float) (-Math.Sign(rotation)) * num * Engine.DeltaTime;
            rotation += speed * Engine.DeltaTime;
            rotation = Calc.Clamp(rotation, -0.4f, 0.4f);
            if (Math.Abs(rotation) < 0.02f && Math.Abs(speed) < 0.2f) {
                rotation = (speed = 0f);
            } else if (Math.Sign(rotation) != Math.Sign(value) && soundDelay <= 0f && Math.Abs(speed) > 0.5f) {
                sfx.Play(AudioPath);
                soundDelay = 0.25f;
            }
            if (sprites.Count > 1) {
                //Skip over Base rotation :)
                for (int i = 1; i < sprites.Count; i++) {
                    sprites[i].Rotation = rotation;
                }
            }
            Vector2 vector = Calc.AngleToVector(rotation + (float) Math.PI / 2f, lightDistance);
            bloom.Position = light.Position = vector + Position.Round() - Position;
            sfx.Position = vector;
        }

        public override void Render() {
            if (sprites.Count > 0) {
                if(drawOutline)
                    foreach(Image i in sprites)
                        i.DrawOutline();
                foreach (Image i in sprites) {
                    i.Render();
                }
            }
        }
    }
}