using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Celeste;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [CustomEntity(
        "VivHelper/CoreToggleSpikesUp = LoadUp",
        "VivHelper/CoreToggleSpikesDown = LoadDown",
        "VivHelper/CoreToggleSpikesLeft = LoadLeft",
        "VivHelper/CoreToggleSpikesRight = LoadRight"
    )]
    public class CoreToggleSpikes : CustomSpike {
        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CoreToggleSpikes(entityData, offset, DirectionPlus.Up);
        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CoreToggleSpikes(entityData, offset, DirectionPlus.Down);
        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CoreToggleSpikes(entityData, offset, DirectionPlus.Left);
        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new CoreToggleSpikes(entityData, offset, DirectionPlus.Right);

        public Session.CoreModes coreMode;
        public Color hotTint, coldTint;
        private Vector2 imageOffset;

        public CoreToggleSpikes(EntityData data, Vector2 offset, DirectionPlus dir)
            : base(data.Position + offset, dir, GetSize(data.Height, data.Width, dir), data.Bool("OverrideWallBounce"), data.Bool("groundRefill", false), data.Bool("KillFromAnyDirection", false)) {
            coreMode = data.Enum("coreMode", Session.CoreModes.None);
            hotTint = VivHelper.OldColorFunction(data.Attr("hotColor", "eb2a3a"));
            coldTint = VivHelper.OldColorFunction(data.Attr("coldColor", "a6fff4"));
            if(data.Bool("AttachToSolid", true)) {
                Add(new StaticMover {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    JumpThruChecker = IsRiding
                });
            }
            Add(new CoreModeListener(OnChange));
            var spikeType = data.Attr("type");
            var directionText = dir.ToString().ToLower();
            var size = GetSize(data.Height, data.Width, dir);
            if (spikeType == "tentacles") {
                for (int i = 0; i < size / 16; i++) {
                    AddTentacle(i);
                }
                if (size / 8 % 2 == 1) {
                    AddTentacle((float) (size / 16) - 0.5f);
                }
            } else {
                if (spikeType == "default") { spikeType = "danger/spikes/default"; } else if (spikeType == "outline") { spikeType = "danger/spikes/outline"; } else if (spikeType == "cliffside" || spikeType == "reflection" || spikeType == "whitereflection") { spikeType = "danger/spikes/whitereflection"; }
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(spikeType + "_" + directionText);
                for (int j = 0; j < size / 8; j++) {
                    Image image = new Image(Calc.Random.Choose(atlasSubtextures));
                    switch (Direction) {
                        case DirectionPlus.Up:
                            image.JustifyOrigin(0.5f, 1f);
                            image.Position = Vector2.UnitX * ((float) j + 0.5f) * 8f + Vector2.UnitY;
                            break;
                        case DirectionPlus.Down:
                            image.JustifyOrigin(0.5f, 0f);
                            image.Position = Vector2.UnitX * ((float) j + 0.5f) * 8f - Vector2.UnitY;
                            break;
                        case DirectionPlus.Right:
                            image.JustifyOrigin(0f, 0.5f);
                            image.Position = Vector2.UnitY * ((float) j + 0.5f) * 8f - Vector2.UnitX;
                            break;
                        case DirectionPlus.Left:
                            image.JustifyOrigin(1f, 0.5f);
                            image.Position = Vector2.UnitY * ((float) j + 0.5f) * 8f + Vector2.UnitX;
                            break;
                    }
                    Add(image);
                }
            }
        }

        private void OnChange(Session.CoreModes coreMode) {
            if (coreMode != Session.CoreModes.None) {
                foreach (Component component in base.Components) {
                    Image image = component as Image;
                    if (image != null) {
                        image.Color = coreMode == Session.CoreModes.Cold ? coldTint : hotTint;
                    }
                }
                Collidable = this.coreMode == coreMode;
            }
        }

        private void AddTentacle(float i) {
            Sprite sprite = GFX.SpriteBank.Create("tentacles");
            sprite.Play(Calc.Random.Next(3).ToString(), restart: true, randomizeFrame: true);
            sprite.Position = ((Direction == DirectionPlus.Up || Direction == DirectionPlus.Down) ? Vector2.UnitX : Vector2.UnitY) * (i + 0.5f) * 16f;
            sprite.Scale.X = Calc.Random.Choose(-1, 1);
            sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
            if (Direction == DirectionPlus.Up) {
                sprite.Rotation = -Consts.PIover2;
                sprite.Y++;
            } else if (Direction == DirectionPlus.Right) {
                sprite.Rotation = 0f;
                sprite.X--;
            } else if (Direction == DirectionPlus.Left) {
                sprite.Rotation = Consts.PI;
                sprite.X++;
            } else if (Direction == DirectionPlus.Down) {
                sprite.Rotation = Consts.PIover2;
                sprite.Y--;
            }
            sprite.Rotation += Consts.PIover2;
            Add(sprite);
        }


        public override void Render() {
            Vector2 position = Position;
            Position += imageOffset;
            base.Render();
            Position = position;
        }

        private void OnShake(Vector2 amount) {
            imageOffset += amount;
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
