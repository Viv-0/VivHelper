using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using MonoMod;


namespace VivHelper.Entities {
    [CustomEntity(
        "VivHelper/AnimatedSpikesUp = LoadUp",
        "VivHelper/AnimatedSpikesDown = LoadDown",
        "VivHelper/AnimatedSpikesLeft = LoadLeft",
        "VivHelper/AnimatedSpikesRight = LoadRight"
    )]
    public class AnimatedSpikes : Spikes {

        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new AnimatedSpikes(entityData, offset, Directions.Up);
        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new AnimatedSpikes(entityData, offset, Directions.Down);
        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new AnimatedSpikes(entityData, offset, Directions.Left);
        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new AnimatedSpikes(entityData, offset, Directions.Right);


        public Sprite sprite;
        protected DynData<Spikes> dyn;
        private bool randomScale;

        public AnimatedSpikes(EntityData data, Vector2 offset, Directions dir)
        : base(data.Position + offset, GetSize(data, dir), dir, data.Attr("directory", "animDefault")) {
            dyn = new DynData<Spikes>(this);
        }

        [MonoModLinkTo("Celeste.Entity", "System.Void Added(Monocle.Scene)")]
        public void Entity_Added(Scene scene) { base.Added(scene); }

        public override void Added(Scene scene) {
            Entity_Added(scene);

            string temp = "";
            if (!string.IsNullOrEmpty(dyn.Get<string>("overrideType")) && !dyn.Get<string>("overrideType").Equals("default")) {
                temp = dyn.Get<string>("overrideType");
                dyn.Set<string>("spikeType", temp);
            } else {
                temp = "animDefault";
            }
            string str = Direction.ToString().ToLower();

            for (int i = 0; i < dyn.Get<int>("size") / 16; i++) {
                AddSprite(temp, i);
            }
            if (dyn.Get<int>("size") / 8 % 2 == 1) {
                AddSprite(temp, (float) (dyn.Get<int>("size") / 16) - 0.5f);
            }
        }

        private void AddSprite(string reference, float i) {
            sprite = GFX.SpriteBank.Create(reference);
            sprite.Play(Calc.Random.Next(sprite.Animations.Count).ToString(), restart: true, randomizeFrame: true);
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

        private static int GetSize(EntityData data, Directions dir) {
            if ((uint) dir > 1u) {
                _ = dir - 2;
                _ = 1;
                return data.Height;
            }
            return data.Width;
        }
    }
}
