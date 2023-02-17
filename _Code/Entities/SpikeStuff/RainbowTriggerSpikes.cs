using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace VivHelper.Entities {
    [CustomEntity(
        "VivHelper/RainbowTriggerSpikesUp = LoadUp",
        "VivHelper/RainbowTriggerSpikesDown = LoadDown",
        "VivHelper/RainbowTriggerSpikesLeft = LoadLeft",
        "VivHelper/RainbowTriggerSpikesRight = LoadRight"
    )]
    public class RainbowTriggerSpikes : CustomSpike {

        protected struct SpikeInfo {
            public RainbowTriggerSpikes Parent;

            public int Index;

            public int TextureIndex;

            public Vector2 Position;

            public bool Triggered;

            public float RetractTimer;

            public float Lerp;

            public Color color;

            public float DelayTimer {
                get { return Parent.Grouped ? Parent.delayTimer : RetractTimer; }
                set { if (Parent.Grouped) { Parent.delayTimer = value; } else { RetractTimer = value; } }
            }
            public void SetColor() {
                color = Parent.oneColor != Color.Transparent ? Parent.oneColor : VivHelper.GetHue(Parent.Scene, Parent.Position + Position);
            }

            public void Update() {
                if (Parent.Grouped ? Parent.Triggered : Triggered) {
                    if (DelayTimer > 0f) {
                        DelayTimer -= Engine.DeltaTime;
                        if (DelayTimer <= Engine.DeltaTime) {
                            if (Parent.Grouped ? Parent.PlayerCheck() : PlayerCheck()) {
                                DelayTimer = 0.05f;
                            } else {
                                Audio.Play("event:/game/03_resort/fluff_tendril_emerge", Parent.Position + Position);
                            }
                        }
                    } else {
                        Lerp = Calc.Approach(Lerp, 1f, 8f * Engine.DeltaTime);
                    }
                } else {
                    Lerp = Calc.Approach(Lerp, 0f, 4f * Engine.DeltaTime);
                    if (Lerp <= 0f) {
                        Triggered = false;
                    }
                }
                if (Parent.oneColor != null)
                    SetColor();
            }

            public bool PlayerCheck() {
                return Parent.PlayerCheck(Index);
            }

            public bool OnPlayer(Player player, Vector2 outwards) {
                if (Parent.Grouped ? !Parent.Triggered : !Triggered) {
                    Audio.Play("event:/game/03_resort/fluff_tendril_touch", Parent.Position + Position);
                    if (Parent.Grouped) { Parent.Triggered = true; } else { Triggered = true; }
                    DelayTimer = 0.05f;

                    return false;
                }
                if (Lerp >= 1f) {
                    player.Die(outwards);
                    return true;
                }
                return false;
            }
        }

        private Vector2 offset;

        private const float RetractTime = 6f;

        protected float Delay = 0.4f;

        protected float delayTimer = 0.05f;

        private int size;

        private Directions direction;

        private string overrideType;

        private PlayerCollider pc;

        private Vector2 outwards;

        private Vector2 shakeOffset;

        private string spikeType;

        private SpikeInfo[] spikes;

        private List<MTexture> spikeTextures;

        public Color oneColor;

        protected bool Triggered = false;

        private bool grouped = false;
        protected bool Grouped {
            get {
                return grouped && VivHelperModule.maxHelpingHandLoaded;
            }
        }

        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new RainbowTriggerSpikes(entityData, offset, Directions.Up, entityData.Width);
        }

        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new RainbowTriggerSpikes(entityData, offset, Directions.Down, entityData.Width);
        }

        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new RainbowTriggerSpikes(entityData, offset, Directions.Left, entityData.Height);
        }

        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new RainbowTriggerSpikes(entityData, offset, Directions.Right, entityData.Height);
        }

        public RainbowTriggerSpikes(EntityData data, Vector2 offset, Directions dir, int size)
            : this(data.Position + offset, offset, size, dir, data.Attr("type", "default"), data.Bool("Grouped", false), data.Bool("DoNotAttach", false)) {

            string str = data.Attr("Color", "");
            oneColor = (str == "" ? Color.Transparent : VivHelper.ColorFix(str));
        }

        public RainbowTriggerSpikes(Vector2 position, Vector2 offset, int size, Directions direction, string overrideType, bool grouped, bool doNotAttach)
            : base(position, direction, size, blockLedge: false) {
            this.size = size;
            if (grouped && !VivHelperModule.maxHelpingHandLoaded) {
                throw new Exception("Grouped Rainbow Trigger Spikes attempted to load without Max's Helping Hand as a dependency.");
            }
            this.direction = direction;
            this.overrideType = overrideType;
            this.offset = offset;
            switch (direction) {
                case Directions.Up:
                    outwards = new Vector2(0f, -1f);
                    base.Collider = new Hitbox(size, 3f, 0f, -3f);
                    Add(new SafeGroundBlocker());
                    Add(new LedgeBlocker(UpSafeBlockCheck));
                    break;
                case Directions.Down:
                    outwards = new Vector2(0f, 1f);
                    base.Collider = new Hitbox(size, 3f);
                    break;
                case Directions.Left:
                    outwards = new Vector2(-1f, 0f);
                    base.Collider = new Hitbox(3f, size, -3f);
                    Add(new SafeGroundBlocker());
                    Add(new LedgeBlocker(SideSafeBlockCheck));
                    break;
                case Directions.Right:
                    outwards = new Vector2(1f, 0f);
                    base.Collider = new Hitbox(3f, size);
                    Add(new SafeGroundBlocker());
                    Add(new LedgeBlocker(SideSafeBlockCheck));
                    break;
            }
            Add(pc = new PlayerCollider(OnCollide));
            if (!doNotAttach)
                Add(new StaticMover {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    JumpThruChecker = IsRiding
                });
            base.Depth = -50;
            this.grouped = grouped;


        }

        public override void Added(Scene scene) {
            base.Added(scene);
            this.NoRefillDash = false;
            AreaData areaData = AreaData.Get(scene);
            spikeType = areaData.Spike;
            if (!string.IsNullOrEmpty(overrideType) && overrideType != "default") {
                if (overrideType == "cliffside" || overrideType == "reflection")
                    spikeType = "whitereflection";
                else
                    spikeType = overrideType;
            }
            string str = direction.ToString().ToLower();
            spikes = new SpikeInfo[size / 8];
            if (GFX.Game["danger/spikes/" + spikeType + "_" + str + "00"] != null)
                spikeTextures = GFX.Game.GetAtlasSubtextures("danger/spikes/" + spikeType + "_" + str);
            else
                spikeTextures = GFX.Game.GetAtlasSubtextures(spikeType + "_" + str);
            for (int i = 0; i < spikes.Length; i++) {
                spikes[i].Parent = this;
                spikes[i].Index = i;
                switch (direction) {
                    case Directions.Up:
                        spikes[i].Position = Vector2.UnitX * ((float) i + 0.5f) * 8f + Vector2.UnitY;
                        break;
                    case Directions.Down:
                        spikes[i].Position = Vector2.UnitX * ((float) i + 0.5f) * 8f - Vector2.UnitY;
                        break;
                    case Directions.Left:
                        spikes[i].Position = Vector2.UnitY * ((float) i + 0.5f) * 8f + Vector2.UnitX;
                        break;
                    case Directions.Right:
                        spikes[i].Position = Vector2.UnitY * ((float) i + 0.5f) * 8f - Vector2.UnitX;
                        break;
                }
                spikes[i].color = Color.White;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
        }

        private void OnShake(Vector2 amount) {
            shakeOffset += amount;
        }

        private bool UpSafeBlockCheck(Player player) {
            int num = 8 * (int) player.Facing;
            int num2 = (int) ((player.Left + (float) num - base.Left) / 4f);
            int num3 = (int) ((player.Right + (float) num - base.Left) / 4f);
            if (num3 < 0 || num2 >= spikes.Length) {
                return false;
            }
            num2 = Math.Max(num2, 0);
            num3 = Math.Min(num3, spikes.Length - 1);
            for (int i = num2; i <= num3; i++) {
                if (spikes[i].Lerp >= 1f) {
                    return true;
                }
            }
            return false;
        }

        private bool SideSafeBlockCheck(Player player) {
            int num = (int) ((player.Top - base.Top) / 4f);
            int num2 = (int) ((player.Bottom - base.Top) / 4f);
            if (num2 < 0 || num >= spikes.Length) {
                return false;
            }
            num = Math.Max(num, 0);
            num2 = Math.Min(num2, spikes.Length - 1);
            for (int i = num; i <= num2; i++) {
                if (spikes[i].Lerp >= 1f) {
                    return true;
                }
            }
            return false;
        }

        protected override void OnCollide(Player player) {
            GetPlayerCollideIndex(player, out int minIndex, out int maxIndex);
            if (maxIndex >= 0 && minIndex < spikes.Length) {
                minIndex = Math.Max(minIndex, 0);
                maxIndex = Math.Min(maxIndex, spikes.Length - 1);
                for (int i = minIndex; i <= maxIndex && !spikes[i].OnPlayer(player, outwards); i++) {
                }
            }
        }

        private bool PlayerCheck() {
            bool b = false;
            foreach (SpikeInfo spike in spikes) { b |= spike.PlayerCheck(); if (b) break; }
            return b;
        }

        private void GetPlayerCollideIndex(Player player, out int minIndex, out int maxIndex) {
            minIndex = (maxIndex = -1);
            switch (direction) {
                case Directions.Up:
                    if (player.Speed.Y >= 0f) {
                        minIndex = (int) ((player.Left - base.Left) / 8f);
                        maxIndex = (int) ((player.Right - base.Left) / 8f);
                    }
                    break;
                case Directions.Down:
                    if (player.Speed.Y <= 0f) {
                        minIndex = (int) ((player.Left - base.Left) / 8f);
                        maxIndex = (int) ((player.Right - base.Left) / 8f);
                    }
                    break;
                case Directions.Left:
                    if (player.Speed.X >= 0f) {
                        minIndex = (int) ((player.Top - base.Top) / 8f);
                        maxIndex = (int) ((player.Bottom - base.Top) / 8f);
                    }
                    break;
                case Directions.Right:
                    if (player.Speed.X <= 0f) {
                        minIndex = (int) ((player.Top - base.Top) / 8f);
                        maxIndex = (int) ((player.Bottom - base.Top) / 8f);
                    }
                    break;
            }
        }

        private bool PlayerCheck(int spikeIndex) {
            Player player = CollideFirst<Player>();
            if (player == null) {
                return false;
            }
            GetPlayerCollideIndex(player, out int minIndex, out int maxIndex);
            if (minIndex <= spikeIndex + 1) {
                return maxIndex >= spikeIndex - 1;
            }
            return false;
        }

        public override void Update() {

            base.Update();
            for (int i = 0; i < spikes.Length; i++) {
                spikes[i].Update();
            }
        }

        public override void Render() {
            base.Render();
            Vector2 justify = Vector2.One * 0.5f;
            switch (direction) {
                case Directions.Up:
                    justify = new Vector2(0.5f, 1f);
                    break;
                case Directions.Down:
                    justify = new Vector2(0.5f, 0f);
                    break;
                case Directions.Left:
                    justify = new Vector2(1f, 0.5f);
                    break;
                case Directions.Right:
                    justify = new Vector2(0f, 0.5f);
                    break;
            }
            for (int i = 0; i < spikes.Length; i++) {
                MTexture mTexture = spikeTextures[spikes[i].TextureIndex];
                Vector2 position = Position + shakeOffset + spikes[i].Position + outwards * (-4f + spikes[i].Lerp * 4f);
                mTexture.DrawJustified(position, justify, spikes[i].color);
            }
        }

        private bool IsRiding(Solid solid) {
            switch (direction) {
                case Directions.Up:
                    return CollideCheckOutside(solid, Position + Vector2.UnitY);
                case Directions.Down:
                    return CollideCheckOutside(solid, Position - Vector2.UnitY);
                case Directions.Left:
                    return CollideCheckOutside(solid, Position + Vector2.UnitX);
                case Directions.Right:
                    return CollideCheckOutside(solid, Position - Vector2.UnitX);
                default:
                    return false;
            }
        }

        private bool IsRiding(JumpThru jumpThru) {
            if (direction == Directions.Up) {
                return CollideCheck(jumpThru, Position + Vector2.UnitY);
            }
            return false;
        }
    }
}
