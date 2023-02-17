using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace VivHelper {
    /// <summary>
    /// Spritesheet revolving around using a single image spritesheet with a formatting of horizontal -> frame # and vertical -> subsprites, with ClipRects dynamically to draw the equivalent of multiple sprites with one MTexture, while also enabling palette magic.
    /// </summary>
    public class CompositeSpritesheet : GraphicsComponent {



        /// <summary>
        /// The animation speed modifier.
        /// </summary>
        public float Rate = 1f;

        /// <summary>
        /// Whether to update animations based on <see cref="Engine.RawDeltaTime"/>.
        /// </summary>
        public bool UseRawDeltaTime;

        public Vector2? Justify;

        public Action<string> OnFinish;
        public Action<string> OnLoop;
        public Action<string> OnFrameChange;
        public Action<string> OnLastFrame;
        public Action<string, string> OnChange;


        /// <summary>
        /// Animations are based on
        /// </summary>
        private Dictionary<string, FrameAnimation> animations;

        private FrameAnimation currentAnimation;
        private int currentFrame;

        private float animationTimer;

        private int frameWidth, frameHeight;
        private int spacingX, spacingY; //defined as frameWidth + kerningX or frameHeight + kerningY
        private int frames, subsprites;

        public int Subsprites => subsprites;
        public int Frames => frames;
        public Vector2 SheetSpacing => new Vector2(spacingX, spacingY);

        public float Width => frameWidth;
        public float Height => frameHeight;
        public Vector2 Center => new Vector2(Width / 2f, Height / 2f);
        public Dictionary<string, FrameAnimation> Animations => animations;

        MTexture source;

        public Color[] colorSet;

        public bool Animating { get; private set; }

        public string CurrentAnimationID { get; private set; }

        public string LastAnimationID { get; private set; }

        public int CurrentAnimationFrame => currentAnimation.Frames[currentFrame];

        public int CurrentAnimationTotalFrames {
            get {
                if (currentAnimation != null) {
                    return currentAnimation.Frames.Length;
                }
                return 0;
            }
        }

        /// <summary>
        /// Constructs a CompositeSpritesheet. Is constructed from a single image with frames by # on the x-axis and subframes, different segments of the image on the y-axis.
        /// </summary>
        /// <param name="active">Whether or not the sprite is active</param>
        /// <param name="source">The source for the image.</param>
        /// <param name="frameWidth">the width of 1 frame of the image. This is equivalent to the framewidth of the original image</param>
        /// <param name="frameHeight">the height of 1 subframe of the image. This is equivalent to the frameheight of the original image</param>
        /// <param name="frame_xStep">the spacing between the left edge of 1 frame and the left edge of the next frame. Equivalent to x-axis kerning</param>
        /// <param name="frame_yStep">the spacing between the top edge of 1 subframe and the top edge of the next subframe. Equivalent to y-axis kerning</param>
        public CompositeSpritesheet(bool active, MTexture source, int frameWidth, int frameHeight, int frame_xStep = 0, int frame_yStep = 0) : base(active) {
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            spacingX = frameWidth + frame_xStep;
            spacingY = frameHeight + frame_yStep;
            frames = source.Width / spacingX;
            subsprites = source.Height / spacingY;
            this.source = source;
            animations = new Dictionary<string, FrameAnimation>();
            colorSet = new Color[subsprites];
            for (int i = 0; i < subsprites; i++)
                colorSet[i] = Color.White;
        }

        public void DefineColorSet(List<Color> colors) {
            for (int i = 0; i < Math.Min(colors.Count, subsprites); i++) {
                if (colors[i] != null)
                    colorSet[i] = colors[i];
            }
        }

        public void TintColor(Color color) {
            Color = color;
        }

        public void Reset() {
            source = null;
            animations = new Dictionary<string, FrameAnimation>(StringComparer.OrdinalIgnoreCase);
            frames = 0;
            subsprites = 0;
            spacingX = 0;
            spacingY = 0;
            frameHeight = 0;
            frameWidth = 0;
            currentAnimation = null;
            CurrentAnimationID = "";
            OnFinish = null;
            OnLoop = null;
            OnFrameChange = null;
            OnChange = null;
            Animating = false;
            colorSet = null;
        }

        public override void Update() {
            if (!Animating) {
                return;
            }
            if (UseRawDeltaTime) {
                animationTimer += Engine.RawDeltaTime * Rate;
            } else {
                animationTimer += Engine.DeltaTime * Rate;
            }
            if (!(Math.Abs(animationTimer) >= currentAnimation.Delay)) {
                return;
            }
            currentFrame += Math.Sign(animationTimer);
            animationTimer -= (float) Math.Sign(animationTimer) * currentAnimation.Delay;
            if (currentFrame < 0 || currentFrame >= currentAnimation.Frames.Length) {
                string currentAnimationID = CurrentAnimationID;
                OnLastFrame?.Invoke(CurrentAnimationID);
                if (!(currentAnimationID == CurrentAnimationID)) {
                    return;
                }
                if (currentAnimation.Goto != null) {
                    CurrentAnimationID = currentAnimation.Goto.Choose();
                    OnChange?.Invoke(LastAnimationID, CurrentAnimationID);
                    LastAnimationID = CurrentAnimationID;
                    currentAnimation = animations[LastAnimationID];
                    if (currentFrame < 0) {
                        currentFrame = currentAnimation.Frames.Length - 1;
                    } else {
                        currentFrame = 0;
                    }
                    OnLoop?.Invoke(CurrentAnimationID);
                } else {
                    if (currentFrame < 0) {
                        currentFrame = 0;
                    } else {
                        currentFrame = currentAnimation.Frames.Length - 1;
                    }
                    Animating = false;
                    string currentAnimationID2 = CurrentAnimationID;
                    CurrentAnimationID = "";
                    currentAnimation = null;
                    animationTimer = 0f;
                    OnFinish?.Invoke(currentAnimationID2);
                }
            }
        }

        public MTexture[] GetSubframes(string animation, int frame) {
            if (!animations.TryGetValue(animation, out FrameAnimation _anim))
                return null;
            var i = _anim.Frames[frame];
            var ret = new MTexture[subsprites];
            for (int j = 0; j < subsprites; j++)
                ret[j] = source.GetSubtexture(spacingX * i, j, frameWidth, frameHeight);
            return ret;
        }

        public void SetAnimationID(string id) {
            if (animations.TryGetValue(id, out FrameAnimation value)) {
                animationTimer = 0f;
                currentAnimation = value;
            }
        }

        public void SetAnimationFrame(int frame) {
            animationTimer = 0f;
            currentFrame = frame == 0 ? 0 : frame % currentAnimation.Frames.Length;
        }

        public void Play(string id, bool restart = false, bool randomizeFrame = false) {
            if (CurrentAnimationID != id || restart) {
                OnChange?.Invoke(LastAnimationID, id);
                string text3 = (LastAnimationID = (CurrentAnimationID = id));
                currentAnimation = animations[id];
                Animating = currentAnimation.Delay > 0f;
                if (randomizeFrame) {
                    animationTimer = Calc.Random.NextFloat(currentAnimation.Delay);
                    SetAnimationFrame(Calc.Random.Next(currentAnimation.Frames.Length));
                } else {
                    animationTimer = 0f;
                    SetAnimationFrame(0);
                }

            }

        }

        public void Add(string id, float delay, params int[] frames) {
            animations.Add(id, new FrameAnimation {
                Delay = delay,
                Frames = frames,
                Goto = null
            });
        }

        public void Add(string id, float delay, string into, params int[] frames) {
            animations.Add(id, new FrameAnimation {
                Delay = delay,
                Frames = frames,
                Goto = Chooser<string>.FromString<string>(into)
            });
        }

        public void AddLoop(string id, float delay, params int[] frames) {
            animations.Add(id, new FrameAnimation {
                Delay = delay,
                Frames = frames,
                Goto = new Chooser<string>(id, 1f)
            });
        }

        public override void Render() {
            if (Animating) {
                var clipRect = new Rectangle(currentAnimation.Frames[currentFrame] * spacingX, 0, frameWidth, frameHeight);
                if (source == null || Color.A != 0) //If it's transparent, we don't draw anything because optimization
                {
                    for (int i = 0; i < subsprites; i++) {
                        var cs = colorSet[i];
                        if (cs.A != 0) //If it's transparent, we don't push the sprite to the RenderTarget because optimization.
                        {
                            clipRect.Y = i * spacingY;
                            Draw.SpriteBatch.Draw(source.Texture.Texture_Safe, RenderPosition, clipRect, VivHelper.BlendColors(cs, Color), Rotation, Origin, Scale, Effects, 0);
                        }
                    }
                }
            }

        }

        public void CenterOrigin() {
            Origin.X = frameWidth / 2f;
            Origin.Y = frameHeight / 2f;
        }
    }
}
