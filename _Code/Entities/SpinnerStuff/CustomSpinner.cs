using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using System.Collections;
using System.Reflection;
using Celeste.Mod.Entities;
using VHM = VivHelper.VivHelper;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/CustomSpinner = Load")]
    public class CustomSpinner : Entity {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new CustomSpinner(entityData, offset);
        }

        public enum Types {
            White = 0, //default
            RainbowClassic, CustomRainbow
        }

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

                    if ((parent as CustomSpinner).customBorder) {
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
                        Image image = component as Image;
                        if (image != null) {
                            Color color1 = image.Color;
                            Vector2 position = image.Position;
                            image.Color = color;
                            image.Position = position - Vector2.UnitY;
                            image.Render();
                            image.Position = position + Vector2.UnitY;
                            image.Render();
                            image.Position = position - Vector2.UnitX;
                            image.Render();
                            image.Position = position + Vector2.UnitX;
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
                            image.Position = position - Vector2.UnitY;
                            image.Render();
                            image.Position = position + Vector2.UnitY;
                            image.Render();
                            image.Position = position - Vector2.UnitX;
                            image.Render();
                            image.Position = position + Vector2.UnitX;
                            image.Render();
                            image.Color = color1;
                            image.Position = position;
                        }
                    }
                }
            }
        }

        public bool AttachToSolid;

        protected Entity filler;

        private Border border;

        protected float offset;

        protected bool expanded;

        protected int randomSeed;

        protected bool customBorder = false;

        protected Types type;

        private string[] hitboxString;

        protected Color color;
        public Color borderColor;

        public float scale, imageScale;

        public bool debrisToScale;

        public bool customDebris;

        protected int ID;

        protected string shatterFlag;

        protected DashCollision OnDashCollide;

        protected bool shatterDash;

        protected Color shatterColor;

        protected string directory, bgdirectory, fgdirectory;
        private string subdirectory;

        protected bool isSeeded;
        protected int seed = -1;

        protected string flagToggle;
        protected bool flagToggleInvert;
        protected bool createConnectors;

        /* Directory Structure:
		 * (fg/animatedfg): Folder containing the N sprites, which it either runs through or animates through 
		 * (bg/animatedbg): defines the background tiling for spinners. (randomization on animations done on
		 *  list of strings passed into the subdirectory info in the prop "Subdirectory" (Animated only))
		 */

        public CustomSpinner(EntityData data, Vector2 offset) : base(data.Position + offset) {
            ID = data.ID;

            type = data.Enum<Types>("Type", Types.White);
            AttachToSolid = data.Bool("AttachToSolid");
            if (data.Has("ref")) {
                string s = data.Attr("ref", "VivHelper/customSpinner/white/fg_white00");
                int q = s.LastIndexOf('/');
                directory = s.Substring(0, q);
                subdirectory = s.Substring(q + 3);
            } else {
                directory = data.Attr("Directory").TrimStart(' ').TrimEnd('/', ' ');
                if (directory == "") { directory = "VivHelper/customSpinner/white"; }
                subdirectory = data.Attr("Subdirectory", "");
                if (data.Bool("FrostHelper", false) || (data.Has("CurrentVersion") && subdirectory == "")) {
                    subdirectory = "";
                } else {
                    if (subdirectory == "") {
                        subdirectory = "white";
                    }
                    subdirectory = "_" + subdirectory;
                }
            }
            string hitboxS = data.Attr("HitboxType", data.Bool("removeRectHitbox", false) ? "C:6" : "C:6|R:16,4;-8,*1@-4");
            if (hitboxS == "")
                hitboxS = "C:6|R:16,4;-8,*1@-4";
            hitboxString = hitboxS.Split('|'); //Splits SMaster into string[] S, see Parsing Instructions below

            bgdirectory = directory + "/bg";
            fgdirectory = directory + "/fg";
            shatterDash = data.Bool("shatterOnDash");
            string t = data.Attr("Color", "");
            if (t == "")
                t = "ffffff";
            color = VHM.ColorFix(t);
            string u = data.Attr("ShatterColor", "");
            if (u == "")
                u = t;
            shatterColor = VHM.ColorFix(u);
            t = data.Attr("BorderColor", "");
            if (t == "")
                t = "000000";
            borderColor = VHM.ColorFix(t);
            this.offset = Calc.Random.NextFloat();
            base.Tag = Tags.TransitionUpdate;
            scale = data.Float("Scale", 1f);
            scale = scale == -1f ? 1f : Math.Max(1 / 3f, scale);
            imageScale = data.Float("ImageScale", 1f);
            imageScale = imageScale == -1f ? 1f : Math.Max(1 / 3f, imageScale);
            debrisToScale = data.Bool("DebrisToScale", true);
            customDebris = data.Bool("CustomDebris", false);
            //hitbox

            base.Collider = ParseHitboxType(hitboxString, scale);

            Visible = false;
            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            Add(new LedgeBlocker());
            base.Depth = data.Int("Depth", -8500);
            if (AttachToSolid) {
                Add(new StaticMover {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    OnDestroy = base.RemoveSelf
                });
            }
            shatterFlag = data.Attr("ShatterFlag");
            randomSeed = Calc.Random.Next();

            seed = data.Int("seed", -1);
            isSeeded = data.Bool("isSeeded");
            flagToggle = data.Attr("flagToggle");
            if (flagToggle.StartsWith("!")) {
                flagToggleInvert = true;
                flagToggle = flagToggle.Substring(1);
            }
            createConnectors = !data.Bool("ignoreConnection", false);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (InView()) {
                CreateSprites();
            }
        }

        protected virtual void ForceInstantiate() {
            CreateSprites();
            Visible = true;
        }

        protected virtual bool InView() {
            Camera camera = (base.Scene as Level).Camera;
            if (base.Right > camera.X - 16f && base.Bottom > camera.Y - 16f && base.Left < camera.X + 320 * camera.Zoom + 16f) {
                return base.Top < camera.Y + 180 * camera.Zoom + 16f;
            }
            return false;
        }

        protected virtual void UpdateHue() {

            foreach (Component component in base.Components) {
                Image image = component as Image;
                if (image != null) {
                    image.Color = VivHelper.GetHue(Scene, Position + image.Position);
                }
            }
            if (filler != null) {
                foreach (Component component2 in filler.Components) {
                    Image image2 = component2 as Image;
                    if (image2 != null) {
                        image2.Color = VivHelper.GetHue(Scene, Position + image2.Position);
                    }
                }
            }
        }

        public override void Update() {
            if (!string.IsNullOrWhiteSpace(flagToggle) && SceneAs<Level>().Session.GetFlag(flagToggle) == flagToggleInvert) {
                Visible = Collidable = false;
            } else if (!Visible) {
                Collidable = false;
                if (InView()) {
                    Visible = true;
                    if (!expanded) {
                        CreateSprites();
                    }
                    if (type == Types.RainbowClassic || type == Types.CustomRainbow) {
                        UpdateHue();
                    }
                }
            } else {
                base.Update();
                if (type == Types.RainbowClassic || type == Types.CustomRainbow && Scene.OnInterval(0.08f, offset)) {
                    UpdateHue();
                }
                if (Scene.OnInterval(0.25f, offset) && !InView()) {
                    Visible = false;
                }
                if (Collider != null && Scene.OnInterval(0.05f, offset)) {
                    Player entity = base.Scene.Tracker.GetEntity<Player>();
                    if (entity != null) {
                        Collidable = (Math.Abs(entity.X - base.X) < 128f && Math.Abs(entity.Y - base.Y) < 128f);
                    }
                }
                if (Visible && shatterFlag != "" && (Scene as Level).Session.GetFlag(shatterFlag)) {
                    Destroy();
                }
            }
            if (filler != null) {
                filler.Position = Position;
            }
        }

        private void OnPlayer(Player player) {
            if (shatterDash && VivHelperModule.MatchDashState(player.StateMachine.State)) {
                Destroy();
                Celeste.Celeste.Freeze(0.02f);
            } else
                player.Die((player.Position - Position).SafeNormalize());
        }

        protected virtual void OnHoldable(Holdable h) {
            h.HitSpinner(this);
        }

        protected virtual void CreateSprites() {
            if (!expanded) {
                Calc.PushRandom(randomSeed);
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(fgdirectory + subdirectory);
                MTexture mTexture = Calc.Random.Choose(atlasSubtextures);
                if (type == Types.RainbowClassic || type == Types.CustomRainbow) {
                    color = VivHelper.GetHue(Scene, Position);
                }
                if (createConnectors && !(this is MovingSpinner)) {
                    foreach (CustomSpinner entity in base.Scene.Tracker.GetEntities<CustomSpinner>()) {
                        if (entity.createConnectors && entity.ID > ID && !(entity is MovingSpinner) && entity.AttachToSolid == AttachToSolid && (entity.Position - Position).LengthSquared() < (float) Math.Pow((double) (12 * (scale + entity.scale)), 2)) {
                            float e = Calc.Angle(entity.Position, Position);
                            AddSprite(Vector2.Lerp(Position + Vector2.UnitX.RotateTowards(-e, 6.3f), entity.Position + Vector2.UnitX.RotateTowards(e, 6.3f), 0.5f) - Position, (entity.scale + scale) / 2f, Color.Lerp(entity.color, color, 0.5f), isSeeded);
                        }
                    }
                }
                int h = 0;
                Image i;
                //Top Left Check
                if (!SolidCheck(new Vector2(base.X - 4f * scale, base.Y - 4f * scale))) {
                    h++;

                }
                //Top Right Check
                if (!SolidCheck(new Vector2(base.X + 4f * scale, base.Y - 4f * scale))) {
                    h += 2;
                }
                //Bottom Left Check
                if (!SolidCheck(new Vector2(base.X - 4f * scale, base.Y + 4f * scale))) {
                    h += 4;
                }
                //Bottom Right Check
                if (!SolidCheck(new Vector2(base.X + 4f * scale, base.Y + 4f * scale))) {
                    h += 8;

                }


                expanded = true;
                Calc.PopRandom();
                //So I now have a bool[]-integer h
                //1 = TopLeft, 2 = TopRight, 4 = BottomLeft, 8 = BottomRight
                //From here I use binary searching because it's more efficient than the switch case argument.
                //Remember that even if this is slow, this occurs once per load of the entity and technically will always be faster, since it's only being called once.
                //First check is the case of a full image, in which case we use the "full image" assuming everything was set up properly.
                if (h == 15) {
                    i = new Image(mTexture).CenterOrigin().SetColor(color);
                    i.Scale = Vector2.One * scale / imageScale;
                    Add(i);
                } else {
                    if ((h & 1) > 0) {
                        i = new Image(mTexture.GetSubtexture(0, 0, (int) (14 * imageScale), (int) (14 * imageScale))).SetOrigin(12f * imageScale, 12f * imageScale).SetColor(color);
                        i.Scale = Vector2.One * scale / imageScale;
                        Add(i);
                    }
                    if ((h & 2) > 0) {
                        i = new Image(mTexture.GetSubtexture((int) (10 * imageScale), 0, (int) (14 * imageScale), (int) (14 * imageScale))).SetOrigin(2f * imageScale, 12f * imageScale).SetColor(color);
                        i.Scale = Vector2.One * scale / imageScale;
                        Add(i);
                    }
                    if ((h & 8) > 0) {
                        i = new Image(mTexture.GetSubtexture((int) (10 * imageScale), (int) (10 * imageScale), (int) (14 * imageScale), (int) (14 * imageScale))).SetOrigin(2f * imageScale, 2f * imageScale).SetColor(color);
                        i.Scale = Vector2.One * scale / imageScale;
                        Add(i);
                    }
                    if ((h & 4) > 0) {
                        i = new Image(mTexture.GetSubtexture(0, (int) (10 * imageScale), (int) (14 * imageScale), (int) (14 * imageScale))).SetOrigin(12f * imageScale, 2f * imageScale).SetColor(color);
                        i.Scale = Vector2.One * scale / imageScale;
                        Add(i);
                    }
                }
                if (borderColor != Color.Transparent)
                    base.Scene.Add(border = new Border(this, filler, borderColor));
            }
        }

        private void AddSprite(Vector2 offset, float scale, Color c, bool seeded) {
            if (filler == null) {
                base.Scene.Add(filler = new Entity(Position));
                filler.Depth = base.Depth + 1;
            }
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(bgdirectory + subdirectory);
            Level l = Scene as Level;
            Image image;
            if (seeded) {
                int r = seed;
                if (r < 0) {
                    r = l.Session.MapData.Levels.FindIndex((s) => l.Session.LevelData.Name == s.Name) + 977;
                    r = VivHelper.mod(r * r * r, 30977);
                    //This is bijective to 30977 rooms, and if you're making more than that, may god rest your soul in peace
                }
                int a = (int) Position.X;
                int b = (int) Position.Y;
                image = new Image(Extensions.ConsistentChooser<MTexture>(a, b, r, atlasSubtextures));
                image.Rotation = Extensions.ConsistentChooser<float>(a, b, r, 0, 1, 2, 3) * ((float) Math.PI / 2f);
            } else {
                image = new Image(Calc.Random.Choose(atlasSubtextures));
                image.Rotation = (float) Calc.Random.Choose(0, 1, 2, 3) * ((float) Math.PI / 2f);
            }
            image.Position = offset;
            image.CenterOrigin();
            image.Scale = Vector2.One * scale / imageScale;
            image.Color = c;
            if (type == Types.RainbowClassic || type == Types.CustomRainbow) {
                image.Color = VivHelper.GetHue(Scene, Position + offset);
            }
            filler.Add(image);
        }



        protected virtual void OnShake(Vector2 pos) {
            foreach (Component component in base.Components) {
                if (component is Image) {
                    (component as Image).Position = pos;
                }
            }
        }

        protected virtual bool IsRiding(Solid solid) {
            return CollideCheck(solid);
        }

        private bool SolidCheck(Vector2 position) {
            if (AttachToSolid) {
                return false;
            }
            foreach (Solid item in base.Scene.CollideAll<Solid>(position)) {
                if (item is SolidTiles) {
                    return true;
                }
            }
            return false;
        }

        protected virtual void ClearSprites() {
            if (filler != null) {
                filler.RemoveSelf();
            }
            filler = null;
            if (border != null) {
                border.RemoveSelf();
            }
            border = null;
            foreach (Image item in base.Components.GetAll<Image>()) {
                item.RemoveSelf();
            }
            expanded = false;
        }

        public virtual void Destroy(bool boss = false) {
            if (InView()) {
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                Color color = shatterColor;
                CustomCrystalDebris.Burst(Position, color, boss, (int)(8f*scale), customDebris ? directory + "/debris" : "particles/shard", debrisToScale ? scale : 1f);
            }
            border?.RemoveSelf();
            filler?.RemoveSelf();
            RemoveSelf();
        }

        #region ParseHitbox

        public ColliderList ParseHitboxType(string[] S, float scale) {
            if(S.Length == 0 || (S.Length == 1 && string.IsNullOrWhiteSpace(S[0]))) {
                Collidable = false;
                return null;
            }
            List<Collider> colliders = new List<Collider>();
            /*At this point, string[] S is a string where each string should be formatted like this:
             * SMaster = A1|A2|A3|A4...|An, S[k] = Ak
             * where Ak => T:U:V
             * where:
             *	: ; = Separators
             *	T = Type: C for circle, R for Rect.
             *	U = AudioParam: for C: r for radius, for R: <w,h>
             *	V = Position offset from Center.
             *	using * before a number as an ignore scale definer.
             *	using a p @ before a number n means (p + n)

             */
            foreach (string s in S) {
                string[] k = s.Split(':', ';'); //Splits Ak into T (k[0]), U (k[1]), and V (k[2])
                                                //We assume that people are going to use this correctly, for now.
                if (k[0][0] == 'C') {
                    if (k.Length == 2) { colliders.Add(ParseCircle(scale, k[1])); } else { colliders.Add(ParseCircle(scale, k[1], k[2])); }
                } else if (k[0][0] == 'R') {
                    if (k.Length == 2) { colliders.Add(ParseRectangle(scale, k[1])); } else { colliders.Add(ParseRectangle(scale, k[1], k[2])); }
                }
            }
            return new ColliderList(colliders.ToArray());

        }

        private Collider ParseCircle(float scale, string rad, string off = "0,0") {
            int radius;
            int[] offset = new int[2];
            radius = ParseInt(rad, scale);
            string[] offs = off.Split(',');
            for (int i = 0; i < 2; i++) {
                offset[i] = ParseInt(offs[i], scale);
            }
            return new Circle(radius, offset[0], offset[1]);
        }

        private Collider ParseRectangle(float scale, string Wh) {
            int[] wh = new int[2];
            int[] offset = new int[2];
            string[] a = Wh.Split(',');
            wh[0] = ParseInt(a[0], scale);
            wh[1] = ParseInt(a[1], scale);
            offset[0] = 0 - Math.Abs((int) Math.Round(wh[0] / 2f));
            offset[1] = Math.Min(-3, 0 - Math.Abs((int) Math.Round(wh[1] / 2f)));
            return new Hitbox(wh[0], wh[1], offset[0], offset[1]);
        }

        private Collider ParseRectangle(float scale, string Wh, string off) {
            int[] wh = new int[2];
            int[] offset = new int[2];
            string[] a = Wh.Split(',');
            string[] b = off.Split(',');
            for (int i = 0; i < 2; i++) {
                wh[i] = ParseInt(a[i], scale);
                offset[i] = ParseInt(b[i], scale);
            }
            return new Hitbox(wh[0], wh[1], offset[0], offset[1]);
        }

        private int ParseInt(string k, float scale) {
            if (string.IsNullOrEmpty(k)) {
                throw new Exception("Integer was empty.");
            }
            if (k.Contains("@")) {
                string[] q = k.Split('@');
                int p = 0;
                for (int s = 0; s < q.Length; s++) { p += ParseInt(q[s], scale); }
                return p;
            }
            if (k[0] == '*') {
                return int.Parse(k.Substring(1));
            } else {
                return (int) Math.Round(int.Parse(k) * (double) scale);
            }
        }
        #endregion


    }
}
