using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using MonoMod;

namespace VivHelper.Entities {
    [TrackedAs(typeof(DashBlock))]
    [CustomEntity("VivHelper/CustomDashBlock = Load")]
    public class CustomDashBlock : DashBlock {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => entityData.Bool("disableFallingBlocksBreak", false) ? new CustomDashBlockAlt(entityData, offset, new EntityID(levelData.Name, entityData.ID)) : new CustomDashBlock(entityData, offset, new EntityID(levelData.Name, entityData.ID));

        protected bool permanent;

        protected EntityID id;

        private char tileType;

        private float width;

        private float height;

        private bool blendIn;
        private bool breakStaticMovers;
        private bool canDash;
        private string audioEvent;
        private string flagBreak, flagDisable;
        private bool disableByDefault;
        private bool disableFallingBlockBreak;

        public CustomDashBlock(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset, id) {
            base.Depth = Depths.Solids;
            this.id = id;
            permanent = data.Bool("permanent", defaultValue: true);
            width = data.Width;
            height = data.Height;
            blendIn = data.Bool("blendin");
            canDash = data.Bool("canDash", defaultValue: true);
            tileType = data.Char("tiletype", '3');
            OnDashCollide = null;
            OnDashCollide = OnDashed;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            flagBreak = data.Attr("FlagOnBreak", "");
            flagDisable = data.Attr("FlagToDisable", "");
            audioEvent = data.Attr("AudioEvent", "gameDefault");
            breakStaticMovers = data.Bool("BreakStaticMovers");
        }

        public override void Awake(Scene scene) {
            if (flagBreak != "") {
                (Scene as Level)?.Session?.SetFlag(flagBreak, false);
            }
            if (flagDisable != "") {
                (Scene as Level)?.Session?.SetFlag(flagDisable, false);
            }
            TileGrid tileGrid;
            if (!blendIn) {
                tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int) width / 8, (int) height / 8).TileGrid;
                Add(new LightOcclude());
            } else {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (base.X / 8f) - tileBounds.Left;
                int y = (int) (base.Y / 8f) - tileBounds.Top;
                int tilesX = (int) base.Width / 8;
                int tilesY = (int) base.Height / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
                Add(new EffectCutout());
                base.Depth = -10501;
            }
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
            if (CollideCheck<Player>()) {
                RemoveSelf();
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            Celeste.Celeste.Freeze(0.05f);
        }

        public void Break2(Vector2 from, Vector2 direction, bool playSound = true, bool playDebrisSound = true) {
            if (playSound) {
                if (audioEvent == "gameDefault") {
                    if (tileType == '1') {
                        Audio.Play("event:/game/general/wall_break_dirt", Position);
                    } else if (tileType == '3') {
                        Audio.Play("event:/game/general/wall_break_ice", Position);
                    } else if (tileType == '9') {
                        Audio.Play("event:/game/general/wall_break_wood", Position);
                    } else {
                        Audio.Play("event:/game/general/wall_break_stone", Position);
                    }
                } else
                    Audio.Play(audioEvent, Position);
            }
            Collidable = false;
            if (VivHelperModule.Session.DebrisLimiter < 1f) {
                for (int i = 0; (float) i < base.Width / 8f; i++) {
                    for (int j = 0; (float) j < base.Height / 8f; j++) {
                        Vector2 v = Position + new Vector2(4 + i * 8, 4 + j * 8);
                        if (!Scene.CollideCheck<Solid>(v))
                            Scene.Add(Engine.Pooler.Create<Debris>().Init(v, tileType, playDebrisSound).BlastFrom(from));
                    }
                }
            }
            if (breakStaticMovers)
                DestroyStaticMovers();
            else
                DisableStaticMovers();
            if (flagBreak != "") {
                (Scene as Level)?.Session?.SetFlag(flagBreak);
            }
            if (permanent) {
                RemoveAndFlagAsGone();
            } else {
                RemoveSelf();
            }
        }

        protected virtual DashCollisionResults OnDashed(Player player, Vector2 direction) {
            if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10 || (flagDisable != "" && ((Scene as Level)?.Session?.GetFlag(flagBreak) ?? false))) {
                return DashCollisionResults.NormalCollision;
            }
            if (new int[] { VivHelperModule.OrangeState, VivHelperModule.WindBoostState, VivHelperModule.PinkState }.Contains(player.StateMachine.State)) {
                Break(player.Center, direction, audioEvent != "");
                return DashCollisionResults.Ignore;
            }

            Break(player.Center, direction, audioEvent != "");
            return DashCollisionResults.Rebound;
        }
    }

    public class CustomDashBlockAlt : Solid {
        protected bool permanent;

        protected EntityID id;

        private char tileType;

        private float width;

        private float height;

        private bool blendIn;
        private bool breakStaticMovers;
        private bool canDash;
        private string audioEvent;
        private string flagBreak, flagDisable;
        private bool disableByDefault;
        private bool disableFallingBlockBreak;

        public CustomDashBlockAlt(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, data.Width, data.Height, true) {
            base.Depth = Depths.Solids;
            this.id = id;
            permanent = data.Bool("permanent", defaultValue: true);
            width = data.Width;
            height = data.Height;
            blendIn = data.Bool("blendin");
            canDash = data.Bool("canDash", defaultValue: true);
            tileType = data.Char("tiletype", '3');
            OnDashCollide = null;
            OnDashCollide = OnDashed;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            flagBreak = data.Attr("FlagOnBreak", "");
            flagDisable = data.Attr("FlagToDisable", "");
            audioEvent = data.Attr("AudioEvent", "gameDefault");
            breakStaticMovers = data.Bool("BreakStaticMovers");
        }

        public override void Awake(Scene scene) {
            if (flagBreak != "") {
                (Scene as Level)?.Session?.SetFlag(flagBreak, false);
            }
            if (flagDisable != "") {
                (Scene as Level)?.Session?.SetFlag(flagDisable, false);
            }
            TileGrid tileGrid;
            if (!blendIn) {
                tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int) width / 8, (int) height / 8).TileGrid;
                Add(new LightOcclude());
            } else {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (base.X / 8f) - tileBounds.Left;
                int y = (int) (base.Y / 8f) - tileBounds.Top;
                int tilesX = (int) base.Width / 8;
                int tilesY = (int) base.Height / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
                Add(new EffectCutout());
                base.Depth = -10501;
            }
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
            if (CollideCheck<Player>()) {
                RemoveSelf();
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            Celeste.Celeste.Freeze(0.05f);
        }

        public void Break2(Vector2 from, Vector2 direction, bool playSound = true, bool playDebrisSound = true) {
            if (playSound) {
                if (audioEvent == "gameDefault") {
                    if (tileType == '1') {
                        Audio.Play("event:/game/general/wall_break_dirt", Position);
                    } else if (tileType == '3') {
                        Audio.Play("event:/game/general/wall_break_ice", Position);
                    } else if (tileType == '9') {
                        Audio.Play("event:/game/general/wall_break_wood", Position);
                    } else {
                        Audio.Play("event:/game/general/wall_break_stone", Position);
                    }
                } else
                    Audio.Play(audioEvent, Position);
            }
            Collidable = false;
            if (VivHelperModule.Session.DebrisLimiter < 1f) {
                for (int i = 0; (float) i < base.Width / 8f; i++) {
                    for (int j = 0; (float) j < base.Height / 8f; j++) {
                        Vector2 v = Position + new Vector2(4 + i * 8, 4 + j * 8);
                        if (!Scene.CollideCheck<Solid>(v))
                            Scene.Add(Engine.Pooler.Create<Debris>().Init(v, tileType, playDebrisSound).BlastFrom(from));
                    }
                }
            }
            if (breakStaticMovers)
                DestroyStaticMovers();
            else
                DisableStaticMovers();
            if (flagBreak != "") {
                (Scene as Level)?.Session?.SetFlag(flagBreak);
            }
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

        protected virtual DashCollisionResults OnDashed(Player player, Vector2 direction) {
            if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10 || (flagDisable != "" && ((Scene as Level)?.Session?.GetFlag(flagBreak) ?? false))) {
                return DashCollisionResults.NormalCollision;
            }
            if (new int[] { VivHelperModule.OrangeState, VivHelperModule.WindBoostState, VivHelperModule.PinkState }.Contains(player.StateMachine.State)) {
                Break(player.Center, direction, audioEvent != "");
                return DashCollisionResults.Ignore;
            }

            Break(player.Center, direction, audioEvent != "");
            return DashCollisionResults.Rebound;
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
    }
}
