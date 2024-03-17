using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using static MonoMod.InlineRT.MonoModRule;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CPP")]
    [Tracked]
    class CustomPlayerPlayback : Entity, IPreAwake {
        public Vector2 LastPosition;

        public List<Player.ChaserState> Timeline;

        public PlayerSprite Sprite;

        public PlayerHair Hair;

        private Vector2 start;

        private float time;

        private int index;

        public bool active;

        private float speedMult;

        private float loopDelay;

        private float startDelay;

        public float TrimStart;

        public float TrimEnd;

        public readonly float Duration;

        private float rangeMinX = float.MinValue;

        private float rangeMaxX = float.MaxValue;

        private bool ShowTrail;

        public string customID;

        private Color? color;

        internal string breaker; //This is a checked string that prevents loading of the object if an invalid Tutorial is entered.

        public Vector2 DashDirection {
            get;
            private set;
        }

        public float Time => time;

        public int FrameIndex => index;

        public int FrameCount => Timeline.Count;

        public CustomPlayerPlayback(EntityData e, Vector2 offset)
            : this(e.Position + offset,
                  PlayerSpriteMode.Playback,
                  e.Attr("tutorial")) {
            if (e.Nodes != null && e.Nodes.Length != 0) {
                rangeMinX = base.X;
                rangeMaxX = base.X;
                Vector2[] array = e.NodesOffset(offset);
                for (int i = 0; i < array.Length; i++) {
                    Vector2 vector = array[i];
                    rangeMinX = Math.Min(rangeMinX, vector.X);
                    rangeMaxX = Math.Max(rangeMaxX, vector.X);
                }
            }
            startDelay = e.Float("Delay", 1f);
            active = e.Bool("StartActive", true);
            speedMult = e.Float("SpeedMultiplier", 1f);
            customID = e.Attr("CustomStringID", "");
            color = null;
            if (e.Attr("Color") != "")
                color = VivHelper.GetColorWithFix(e, "Color", "color", VivHelper.GetColorParams.None, VivHelper.GetColorParams.None, Color.White).Value;
            base.Depth = e.Int("Depth", 9008);
            if (Sprite != null) {
                Sprite.Color = color ?? Hair.Color;
                Add(Sprite);
            }
        }

        public CustomPlayerPlayback(Vector2 start, PlayerSpriteMode sprite, string tutorial) {
            if (!PlaybackData.Tutorials.TryGetValue(tutorial, out Timeline)) {
                breaker = "PlayerPlayback at " + start + " errors due to no Tutorial \"" + tutorial + ".\" You may need to restart or reload Assets manually to resolve this change.";
                return;
            }
            this.start = start;
            base.Collider = new Hitbox(8f, 11f, -4f, -11f);
            Position = start;
            time = 0f;
            index = 0;
            Duration = Timeline[Timeline.Count - 1].TimeStamp;
            TrimStart = 0f;
            TrimEnd = Duration;
            Sprite = new PlayerSprite(sprite);

            Add(Hair = new PlayerHair(Sprite));

            base.Collider = new Hitbox(8f, 4f, -4f, -4f);
            if (sprite == PlayerSpriteMode.Playback) {
                ShowTrail = true;
            }
            base.Depth = 9008;
            SetFrame(0);
            for (int i = 0; i < 10; i++) {
                Hair.AfterUpdate();
            }
            Visible = false;
            index = Timeline.Count;
        }

        void IPreAwake.PreAwake(Scene scene) {
            if (breaker != null) //This is the "Active" state that is associated with Update calls, and is modified by if the Tutorial cannot be found.
            {
                VivHelperModule.SendErrorMessageThroughDebugConsole(breaker);
                Active = false;
                RemoveSelf();
            }
        }

        public void Restart() {
            Audio.Play("event:/new_content/char/tutorial_ghost/appear", Position);
            Visible = true;
            time = TrimStart;
            index = 0;
            loopDelay = 0.25f;
            while (time > Timeline[index].TimeStamp) {
                index++;
            }
            SetFrame(index);
        }

        public void SetFrame(int index) {
            Player.ChaserState chaserState = Timeline[index];
            string currentAnimationID = Sprite.CurrentAnimationID;
            bool flag = base.Scene != null && CollideCheck<Solid>(Position + new Vector2(0f, 1f));
            _ = DashDirection;
            Position = start + chaserState.Position;
            if (chaserState.Animation != Sprite.CurrentAnimationID && chaserState.Animation != null && Sprite.Has(chaserState.Animation)) {
                Sprite.Play(chaserState.Animation, restart: true);
            }
            Sprite.Scale = chaserState.Scale;
            if (Sprite.Scale.X != 0f) {
                Hair.Facing = (Facings) Math.Sign(Sprite.Scale.X);
            }
            Hair.Color = color ?? chaserState.HairColor;
            if (Sprite.Mode == PlayerSpriteMode.Playback) {
                Sprite.Color = color ?? Hair.Color;
            }
            DashDirection = chaserState.DashDirection;
            if (base.Scene == null) {
                return;
            }
            if (!flag && base.Scene != null && CollideCheck<Solid>(Position + new Vector2(0f, 1f))) {
                Audio.Play("event:/new_content/char/tutorial_ghost/land", Position);
            }
            if (!(currentAnimationID != Sprite.CurrentAnimationID)) {
                return;
            }
            string currentAnimationID2 = Sprite.CurrentAnimationID;
            int currentAnimationFrame = Sprite.CurrentAnimationFrame;
            switch (currentAnimationID2) {
                case "jumpFast":
                case "jumpSlow":
                    Audio.Play("event:/new_content/char/tutorial_ghost/jump", Position);
                    break;
                case "dreamDashIn":
                    Audio.Play("event:/new_content/char/tutorial_ghost/dreamblock_sequence", Position);
                    break;
                case "dash":
                    if (DashDirection.Y != 0f) {
                        Audio.Play("event:/new_content/char/tutorial_ghost/jump_super", Position);
                    } else if (chaserState.Scale.X > 0f) {
                        Audio.Play("event:/new_content/char/tutorial_ghost/dash_red_right", Position);
                    } else {
                        Audio.Play("event:/new_content/char/tutorial_ghost/dash_red_left", Position);
                    }
                    break;
                case "climbUp":
                case "climbDown":
                case "wallslide":
                    Audio.Play("event:/new_content/char/tutorial_ghost/grab", Position);
                    break;
                default:
                    if ((currentAnimationID2.Equals("runSlow_carry") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (currentAnimationID2.Equals("runFast") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (currentAnimationID2.Equals("runSlow") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (currentAnimationID2.Equals("walk") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (currentAnimationID2.Equals("runStumble") && currentAnimationFrame == 6) || (currentAnimationID2.Equals("flip") && currentAnimationFrame == 4) || (currentAnimationID2.Equals("runWind") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (currentAnimationID2.Equals("idleC") && Sprite.Mode == PlayerSpriteMode.MadelineNoBackpack && (currentAnimationFrame == 3 || currentAnimationFrame == 6 || currentAnimationFrame == 8 || currentAnimationFrame == 11)) || (currentAnimationID2.Equals("carryTheoWalk") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (currentAnimationID2.Equals("push") && (currentAnimationFrame == 8 || currentAnimationFrame == 15))) {
                        Audio.Play("event:/new_content/char/tutorial_ghost/footstep", Position);
                    }
                    break;
            }
        }

        public override void Update() {
            if (startDelay > 0f) {
                startDelay -= Engine.DeltaTime;
            }
            LastPosition = Position;
            base.Update();
            if (active) {
                if (index >= Timeline.Count - 1 || Time >= TrimEnd) {
                    if (Visible) {
                        Audio.Play("event:/new_content/char/tutorial_ghost/disappear", Position);
                    }
                    Visible = false;
                    Position = start;
                    loopDelay -= Engine.DeltaTime;
                    if (loopDelay <= 0f) {
                        Player player = (base.Scene == null) ? null : base.Scene.Tracker.GetEntity<Player>();
                        if (player == null || (player.X > rangeMinX && player.X < rangeMaxX)) {
                            Restart();
                        }
                    }
                } else if (startDelay <= 0f && base.Scene.OnInterval(Engine.DeltaTime / Math.Min(1f, speedMult))) {
                    SetFrame(index);
                    time += Engine.DeltaTime;
                    while (index < Timeline.Count - 1 && time >= Timeline[index + 1].TimeStamp) {
                        index += speedMult > 1 ? (int) Math.Round((double) speedMult) : 1;
                    }
                }
                if (Visible && ShowTrail && base.Scene != null && base.Scene.OnInterval(0.1f)) {
                    TrailManager.Add(Position, Sprite, Hair, Sprite.Scale, Hair.Color, base.Depth + 1);
                }
            }
        }

        public override void Render() {
            if (active)
                base.Render();
        }
    }
}
