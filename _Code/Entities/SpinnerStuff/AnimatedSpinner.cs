using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Reflection;


namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/AnimatedSpinner")]
    public class AnimatedSpinner : CustomSpinner {
        private class Border : Entity {
            public Entity parent, filler;

            public Color color = Color.Black;

            public Border(Entity parent, Entity filler, Color color) {
                this.parent = parent;
                this.filler = filler;
                base.Depth = parent.Depth + 2;
                this.color = color;
            }

            public override void Render() {
                if (parent.Visible) {

                    if ((parent as AnimatedSpinner).customBorder) {
                        DrawBorder_Custom(parent);
                        DrawBorder_Custom(filler);
                    } else {
                        DrawBorder(parent);
                        DrawBorder(filler);
                    }
                }
            }

            private void DrawBorder(Entity entity) {
                if (entity != null) {
                    foreach (Component component in entity.Components) {
                        Sprite image = component as Sprite;
                        if (image != null) {
                            Color color1 = image.Color;
                            Vector2 position = image.Position;
                            image.Color = color;
                            image.Position = position + new Vector2(0f, -1f);
                            image.Render();
                            image.Position = position + new Vector2(0f, 1f);
                            image.Render();
                            image.Position = position + new Vector2(-1f, 0f);
                            image.Render();
                            image.Position = position + new Vector2(1f, 0f);
                            image.Render();
                            image.Color = color1;
                            image.Position = position;
                        }
                    }
                }
            }

            private void DrawBorder_Custom(Entity entity) {
                if (entity != null) {
                    foreach (Component component in entity.Components) {
                        Image image = component as Image;
                        if (image != null) {
                            Color color1 = image.Color;
                            Vector2 position = image.Position;
                            image.Color = color;
                            image.Position = position + new Vector2(0f, -1f);
                            image.Render();
                            image.Position = position + new Vector2(0f, 1f);
                            image.Render();
                            image.Position = position + new Vector2(-1f, 0f);
                            image.Render();
                            image.Position = position + new Vector2(1f, 0f);
                            image.Render();
                            image.Color = color1;
                            image.Position = position;
                        }
                    }
                }
            }
        }
        //private static MethodInfo spriteClone = typeof(Sprite).GetMethod("CloneInto", BindingFlags.NonPublic | BindingFlags.Instance);

        private List<string> subdirectory;

        private Border border;

        private string path;

        private bool shatterAnim, killAnim, borders, bgRotate;

        private Sprite fgSprite, bgSprite, fgbSprite, bgbSprite;

        private float timeBetweenFrames;

        public AnimatedSpinner(EntityData data, Vector2 offset) : base(data, offset) {
            Remove(Get<PlayerCollider>());
            Add(new PlayerCollider(OnPlayer));
            if (directory == "VivHelper/customSpinner/white")
                directory = "VivHelper/customSpinner/animated";
            string temp = data.Attr("Subdirectories", "").Trim().TrimEnd('/');
            subdirectory = new List<string>();
            if (temp == "") { subdirectory.Add("spin"); } //Add stuff here
            else {
                foreach (string s in temp.Split(',')) {
                    subdirectory.Add(s);
                }
            }
            timeBetweenFrames = data.Float("TimeBetweenFrames", 0.1f);

        }

        protected override void CreateSprites() {
            if (!expanded) {
                //Get the directory and all sprites organized pre-creation
                Calc.PushRandom(randomSeed);
                path = directory + "/";
                if (isSeeded) {
                    Level l = Scene as Level;
                    int r = seed;
                    if (r < 0) {
                        r = l.Session.MapData.Levels.FindIndex((s) => l.Session.LevelData.Name == s.Name) + 977;
                        r = VivHelper.mod(r * r * r, 30977);
                        //This is bijective to 30977 rooms, and if you're making more than that, may god rest your soul in peace
                    }
                    path += Extensions.ConsistentChooser<string>((int) Position.X, (int) Position.Y, seed, subdirectory);
                } else
                    path += Calc.Random.Choose<string>(subdirectory);
                path += "/";
                DefineSprite(ref fgSprite, killAnim, "fg");
                /*//Incorporates conditionals to simplify my work later.
                //if customBorder is set to true, it searches for the border textures,
                //otherwise it copies the same format as fg/bgSprite.
                fgbSprite = new Sprite(GFX.Game, temp);
                fgbSprite.CenterOrigin();
                fgbSprite.AddLoop("idle", customBorder ? "idle_fgb" : "idle_fg", 0.1f);
                if (killAnim) { fgbSprite.Add("kill", customBorder ? "kill_fgb" : "kill_fg", 0.1f, "idle"); }
                fgbSprite.SetColor(borderColor);
                bgbSprite = new Sprite(GFX.Game, temp);
                bgbSprite.CenterOrigin();
                bgbSprite.AddLoop("idle", customBorder ? "idle_bgb" : "idle_bg", 0.1f);
                bgbSprite.SetColor(borderColor);*/
                //CreateSprites as normal but now with removed image subtexture stuff
                fgSprite.CenterOrigin();
                Add(fgSprite);

                fgSprite.Play("idle");
                foreach (AnimatedSpinner entity in base.Scene.Tracker.GetEntities<AnimatedSpinner>()) {
                    if (entity.ID > ID && entity.AttachToSolid == AttachToSolid && (entity.Position - Position).LengthSquared() < (float) Math.Pow((double) (12 * (scale + entity.scale)), 2)) {
                        float e = Calc.Angle(entity.Position, Position);
                        float t = Calc.Angle(Position, entity.Position);
                        AddSprite(Vector2.Lerp(Position + Vector2.UnitX.RotateTowards(t, 6.3f), entity.Position + Vector2.UnitX.RotateTowards(e, 6.3f), 0.5f) - Position, (entity.scale + scale) / 2f, Color.Lerp(entity.color, color, 0.5f), t, isSeeded);
                    }
                }
                base.Scene.Add(border = new Border(this, filler, borderColor));
                expanded = true;
                Calc.PopRandom();
            }
        }

        private void AddSprite(Vector2 offset, float scale, Color color, float angle, bool seeded) {
            if (filler == null) {
                base.Scene.Add(filler = new Entity(Position));
                filler.Depth = base.Depth + 1;
            }
            Sprite sprite = null;
            DefineSprite(ref sprite, false, "bg");
            sprite.Position = offset;
            if (bgRotate) {
                if (seeded) {

                }
            }
            sprite.Rotation = bgRotate ? (float) Calc.Random.Choose(0, 1, 2, 3) * ((float) Math.PI / 2f) : angle;
            sprite.CenterOrigin();
            sprite.Scale = Vector2.One * scale * imageScale;
            sprite.Color = color;
            if (type == Types.RainbowClassic || type == Types.CustomRainbow) {
                sprite.Color = VivHelper.GetHue(Scene, Position + offset);
            }
            sprite.Play("idle");
            filler.Add(sprite);
        }

        private void OnPlayer(Player player) {
            player.Die((player.Position - Position).SafeNormalize());
            if (killAnim) {
                fgSprite.Play("kill");
            }
        }

        private void DefineSprite(ref Sprite sprite, bool kill, string subtext) {
            sprite = new Sprite(GFX.Game, path);
            sprite.CenterOrigin();
            sprite.Scale = Vector2.One * scale * imageScale;
            sprite.AddLoop("idle", "idle_" + subtext, timeBetweenFrames);
            if (killAnim) { sprite.Add("kill", "kill_" + subtext, timeBetweenFrames, "idle"); }
            fgSprite.SetColor(color);
        }

        public override void Destroy(bool boss = false) {
            if (InView()) {
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                Color color = shatterColor;
                CustomCrystalDebris.Burst(Position, color, boss, 8, customDebris ? directory + subdirectory + "/debris" : "particles/shard", debrisToScale ? scale : 1f);
            }
            RemoveSelf();
        }

    }
}
