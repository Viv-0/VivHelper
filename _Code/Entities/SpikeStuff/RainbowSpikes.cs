using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Celeste.Mod.VivHelper;
using System.Reflection;

namespace VivHelper.Entities {
    [CustomEntity(
        "VivHelper/RainbowSpikesUp = LoadUp",
        "VivHelper/RainbowSpikesDown = LoadDown",
        "VivHelper/RainbowSpikesLeft = LoadLeft",
        "VivHelper/RainbowSpikesRight = LoadRight"
    )]
    public class RainbowSpikes : CustomSpike {
        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new RainbowSpikes(entityData, offset, Directions.Up);
        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new RainbowSpikes(entityData, offset, Directions.Down);
        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new RainbowSpikes(entityData, offset, Directions.Left);
        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new RainbowSpikes(entityData, offset, Directions.Right);

        public const string TentacleType = "tentacles";

        private PlayerCollider pc;

        private Vector2 imageOffset;

        private int size;

        private string overrideType;

        private string spikeType;

        private Vector2 offset;

        public Color oneColor;

        private Vector2 offsetDir;

        public Color EnabledColor = Color.White;

        public Color DisabledColor = Color.White;

        public bool VisibleWhenDisabled;

        private float timer;

        public RainbowSpikes(Vector2 position, Vector2 o, int size, Directions direction, string type, bool doNotAttach, bool wallbounce, bool allway)
            : base(position, direction, size, wallbounce, allway) {
            base.Depth = -1;
            this.size = size;
            overrideType = type;
            offset = o;
            Add(pc = new PlayerCollider(OnCollide));
            if (!doNotAttach)
                Add(new StaticMover {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    JumpThruChecker = IsRiding,
                    OnEnable = OnEnable,
                    OnDisable = OnDisable
                });
            AddTag(Tags.TransitionUpdate);
        }

        public RainbowSpikes(EntityData data, Vector2 offset, Directions dir)
            : this(data.Position + offset, offset, GetSize(data.Height, data.Width, dir), dir, data.Attr("type", "default"), data.Bool("DoNotAttach", false), data.Bool("OverrideWallBounce"), data.Bool("KillFromAnyDirection", false)) {
            string str = data.Attr("Color", "");
            oneColor = (str == "" ? Color.Transparent : VivHelper.ColorFix(str));

        }

        public void SetSpikeColor() {
            foreach (Component component in Components) {
                Image image = component as Image;
                if (image != null) {
                    image.Color = oneColor != Color.Transparent ? oneColor : VivHelper.GetHue(Scene, Position + image.Position);
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            AreaData areaData = AreaData.Get(scene);
            spikeType = areaData.Spike;
            if (!string.IsNullOrEmpty(overrideType) && !overrideType.Equals("default")) {
                spikeType = overrideType;
            }
            string str = Direction.ToString().ToLower();
            if (spikeType == "tentacles") { spikeType = "default"; }
            if (spikeType == "default") { spikeType = "danger/spikes/default"; } else if (spikeType == "outline") { spikeType = "danger/spikes/outline"; } else if (spikeType == "cliffside" || spikeType == "reflection" || spikeType == "whitereflection") { spikeType = "danger/spikes/whitereflection"; }
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(spikeType + "_" + str);
            for (int j = 0; j < size / 8; j++) {
                Image image = new Image(Calc.Random.Choose(atlasSubtextures));
                switch (Direction) {
                    case Directions.Up:
                        image.JustifyOrigin(0.5f, 1f);
                        image.Position = Vector2.UnitX * ((float) j + 0.5f) * 8f + Vector2.UnitY;
                        break;
                    case Directions.Down:
                        image.JustifyOrigin(0.5f, 0f);
                        image.Position = Vector2.UnitX * ((float) j + 0.5f) * 8f - Vector2.UnitY;
                        break;
                    case Directions.Right:
                        image.JustifyOrigin(0f, 0.5f);
                        image.Position = Vector2.UnitY * ((float) j + 0.5f) * 8f - Vector2.UnitX;
                        break;
                    case Directions.Left:
                        image.JustifyOrigin(1f, 0.5f);
                        image.Position = Vector2.UnitY * ((float) j + 0.5f) * 8f + Vector2.UnitX;
                        break;
                }
                Add(image);
                try { SetSpikeColor(); } catch { Console.WriteLine("Setting RainbowSpikeColor failed on first pass!"); }
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            timer = Engine.DeltaTime;
        }

        public override void Update() {
            base.Update();
            if (Scene == null) { return; }
            if (timer > 0f) { timer -= Engine.DeltaTime; return; }
            if (oneColor == Color.Transparent)
                SetSpikeColor();
        }

        private void AddTentacle(float i) {
            Sprite sprite = VivHelperModule.spriteBank.Create("tentacles");
            sprite.Play(Calc.Random.Next(3).ToString(), restart: true, randomizeFrame: true);
            sprite.Position = ((Direction == Directions.Up || Direction == Directions.Down) ? Vector2.UnitX : Vector2.UnitY) * (i + 0.5f) * 16f;
            sprite.Scale.X = Calc.Random.Choose(-1, 1);
            sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
            if (Direction == Directions.Up) {
                sprite.Rotation = -(float) Math.PI / 2f;
                float y = sprite.Y;
                sprite.Y = y + 1f;
            } else if (Direction == Directions.Right) {
                sprite.Rotation = 0f;
                float y = sprite.X;
                sprite.X = y - 1f;
            } else if (Direction == Directions.Left) {
                sprite.Rotation = (float) Math.PI;
                float y = sprite.X;
                sprite.X = y + 1f;
            } else if (Direction == Directions.Down) {
                sprite.Rotation = (float) Math.PI / 2f;
                float y = sprite.Y;
                sprite.Y = y - 1f;
            }
            sprite.Rotation += (float) Math.PI / 2f;
            Add(sprite);
        }

        private void OnEnable() {
            Active = (Visible = (Collidable = true));
            SetSpikeColor();
        }

        private void OnDisable() {
            Active = (Collidable = false);
            if (VisibleWhenDisabled) {
                foreach (Component component in base.Components) {
                    Image image = component as Image;
                    if (image != null) {
                        image.Color = DisabledColor;
                    }
                }
            } else {
                Visible = false;
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
                case Directions.Up:
                    return CollideCheckOutside(solid, Position + Vector2.UnitY);
                case Directions.Down:
                    return CollideCheckOutside(solid, Position - Vector2.UnitY);
                case Directions.Left:
                    return CollideCheckOutside(solid, Position + Vector2.UnitX);
                case Directions.Right:
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
