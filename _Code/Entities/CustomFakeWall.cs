using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CustomFakeWall")]
    internal class CustomFakeWall : Entity {
        public enum Modes {
            Wall,
            Block
        }

        private enum RevealType {
            Never,
            NotOnTransition,
            Always
        }

        private Modes mode;

        private char fillTile;

        private TileGrid tiles;

        private bool fade, permanent, freezeGame;

        private EffectCutout cutout;

        private float transitionStartAlpha;

        private bool transitionFade;

        private EntityID eid;

        private RevealType playReveal;

        private string audioEvent;

        public CustomFakeWall(EntityID eid, Vector2 position, char tile, float width, float height, Modes mode)
            : base(position) {
            this.mode = mode;
            this.eid = eid;
            fillTile = tile;
            base.Collider = new Hitbox(width, height);
            Add(cutout = new EffectCutout());
        }

        public CustomFakeWall(EntityData data, Vector2 offset, EntityID eid)
            : this(eid, data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, data.Enum("mode", Modes.Wall)) {
            playReveal = data.Enum<RevealType>("playReveal");
            audioEvent = data.NoEmptyString("audioEvent", "event:/game/general/secret_revealed");
            Depth = data.Int("depth", -13000);
            permanent = data.Bool("permanent", true);

        }

        public override void Added(Scene scene) {
            base.Added(scene);
            int tilesX = (int) base.Width / 8;
            int tilesY = (int) base.Height / 8;
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int) base.X / 8 - tileBounds.Left;
            int y = (int) base.Y / 8 - tileBounds.Top;
            tiles = GFX.FGAutotiler.GenerateOverlay(fillTile, x, y, tilesX, tilesY, solidsData).TileGrid;
            Add(tiles);
            Add(new TileInterceptor(tiles, highPriority: false));
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (CollideCheck<Player>()) {
                tiles.Alpha = 0f;
                fade = true;
                cutout.Visible = false;
                if (playReveal == RevealType.Always) {
                    Audio.Play(audioEvent, base.Center);
                }
                if(permanent)
                    SceneAs<Level>().Session.DoNotLoad.Add(eid);
            } else {
                TransitionListener transitionListener = new TransitionListener();
                transitionListener.OnOut = OnTransitionOut;
                transitionListener.OnOutBegin = OnTransitionOutBegin;
                transitionListener.OnIn = OnTransitionIn;
                transitionListener.OnInBegin = OnTransitionInBegin;
                Add(transitionListener);
            }
        }

        private void OnTransitionOutBegin() {
            if (Collide.CheckRect(this, SceneAs<Level>().Bounds)) {
                transitionFade = true;
                transitionStartAlpha = tiles.Alpha;
            } else {
                transitionFade = false;
            }
        }

        private void OnTransitionOut(float percent) {
            if (transitionFade) {
                tiles.Alpha = transitionStartAlpha * (1f - percent);
            }
        }

        private void OnTransitionInBegin() {
            Level level = SceneAs<Level>();
            if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value)) {
                transitionFade = true;
                tiles.Alpha = 0f;
            } else {
                transitionFade = false;
            }
        }

        private void OnTransitionIn(float percent) {
            if (transitionFade) {
                tiles.Alpha = percent;
            }
        }

        public override void Update() {
            base.Update();
            if (fade) {
                tiles.Alpha = Calc.Approach(tiles.Alpha, 0f, 2f * Engine.DeltaTime);
                cutout.Alpha = tiles.Alpha;
                if (tiles.Alpha <= 0f) {
                    RemoveSelf();
                }
                return;
            }
            Player player = CollideFirst<Player>();
            if (player != null && player.StateMachine.State != 9) {
                if(permanent)
                    SceneAs<Level>().Session.DoNotLoad.Add(eid);
                fade = true;
                if(playReveal != RevealType.Never)
                    Audio.Play(audioEvent, base.Center);
            }
        }

        public override void Render() {
            if (mode == Modes.Wall) {
                Level level = base.Scene as Level;
                if (level.ShakeVector.X < 0f && level.Camera.X <= (float) level.Bounds.Left && base.X <= (float) level.Bounds.Left) {
                    tiles.RenderAt(Position + new Vector2(-3f, 0f));
                }
                if (level.ShakeVector.X > 0f && level.Camera.X + 320f >= (float) level.Bounds.Right && base.X + base.Width >= (float) level.Bounds.Right) {
                    tiles.RenderAt(Position + new Vector2(3f, 0f));
                }
                if (level.ShakeVector.Y < 0f && level.Camera.Y <= (float) level.Bounds.Top && base.Y <= (float) level.Bounds.Top) {
                    tiles.RenderAt(Position + new Vector2(0f, -3f));
                }
                if (level.ShakeVector.Y > 0f && level.Camera.Y + 180f >= (float) level.Bounds.Bottom && base.Y + base.Height >= (float) level.Bounds.Bottom) {
                    tiles.RenderAt(Position + new Vector2(0f, 3f));
                }
            }
            base.Render();
        }
    }
}
