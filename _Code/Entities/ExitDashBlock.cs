using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;
using MonoMod.Utils;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/ExitDashBlock")]
    public class ExitDashBlock : Solid {
        public enum Modes {
            Dash,
            FinalBoss,
            Crusher
        }

        private bool permanent;

        private EntityID id;

        private float width;

        private float height;

        private bool blendIn;

        private bool canDash;

        private TileGrid tiles;

        private TransitionListener tl;

        private EffectCutout cutout;

        private float startAlpha;

        private char tileType;

        public ExitDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, safe: true) {
            base.Depth = -13000;
            this.tileType = data.Char("tiletype", '3');
            tl = new TransitionListener();
            tl.OnOutBegin = OnTransitionOutBegin;
            tl.OnInBegin = OnTransitionInBegin;
            Add(tl);
            Add(cutout = new EffectCutout());
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            EnableAssistModeChecks = false;
            this.id = id;
            this.permanent = data.Bool("permanent");
            this.width = data.Width;
            this.height = data.Height;
            this.blendIn = data.Bool("blendin");
            this.canDash = data.Bool("canDash");
            OnDashCollide = OnDashed;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (!blendIn) {
                tiles = GFX.FGAutotiler.GenerateBox(tileType, (int) width / 8, (int) height / 8).TileGrid;
                Add(new LightOcclude());
            } else {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (base.X / 8f) - tileBounds.Left;
                int y = (int) (base.Y / 8f) - tileBounds.Top;
                int tilesX = (int) base.Width / 8;
                int tilesY = (int) base.Height / 8;
                tiles = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
            }
            Add(tiles);
            Add(new TileInterceptor(tiles, highPriority: true));
            if (CollideCheck<Player>()) {
                cutout.Alpha = (tiles.Alpha = 0f);
                Collidable = false;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            Celeste.Celeste.Freeze(0.05f);
        }

        public override void Update() {
            base.Update();
            if (Collidable) {
                cutout.Alpha = (tiles.Alpha = Calc.Approach(tiles.Alpha, 1f, Engine.DeltaTime));
            } else if (!CollideCheck<Player>()) {
                Collidable = true;
                Audio.Play("event:/game/general/passage_closed_behind", base.Center);
            }
        }


        private void OnTransitionOutBegin() {
            if (Collide.CheckRect(this, SceneAs<Level>().Bounds)) {
                tl.OnOut = OnTransitionOut;
                startAlpha = tiles.Alpha;
            }
        }

        private void OnTransitionOut(float percent) {
            cutout.Alpha = (tiles.Alpha = MathHelper.Lerp(startAlpha, 0f, percent));
            cutout.Update();
        }

        private void OnTransitionInBegin() {
            if (Collide.CheckRect(this, SceneAs<Level>().PreviousBounds.Value) && !CollideCheck<Player>()) {
                cutout.Alpha = 0f;
                tiles.Alpha = 0f;
                tl.OnIn = OnTransitionIn;
            }
        }

        private void OnTransitionIn(float percent) {
            cutout.Alpha = (tiles.Alpha = percent);
            cutout.Update();
        }

        public void Break(Vector2 from, Vector2 direction, bool playSound = true, bool playDebrisSound = true) {
            if (playSound) {
                if (tileType == '1') {
                    Audio.Play("event:/game/general/wall_break_dirt", Position);
                } else if (tileType == '3') {
                    Audio.Play("event:/game/general/wall_break_ice", Position);
                } else if (tileType == '9') {
                    Audio.Play("event:/game/general/wall_break_wood", Position);
                } else {
                    Audio.Play("event:/game/general/wall_break_stone", Position);
                }
            }
            for (int i = 0; (float) i < base.Width / 8f; i++) {
                for (int j = 0; (float) j < base.Height / 8f; j++) {
                    base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tileType, playDebrisSound).BlastFrom(from));
                }
            }
            Collidable = false;
            if (permanent) {
                RemoveAndFlagAsGone();
            } else {
                RemoveSelf();
            }
        }

        public void RemoveAndFlagAsGone() {
            RemoveSelf();
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }

        private DashCollisionResults OnDashed(Player player, Vector2 direction) {
            if (tiles.Alpha != 1f)
                return DashCollisionResults.Ignore;
            if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10) {
                return DashCollisionResults.NormalCollision;
            }
            Break(player.Center, direction);
            return DashCollisionResults.Rebound;
        }
    }
}
