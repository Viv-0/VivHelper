using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod;
using Mono.Cecil.Cil;
using Celeste.Mod;

namespace VivHelper.Entities {
    [TrackedAs(typeof(CassetteBlock))]
    [Tracked]
    public class CassetteTileEntity : CassetteBlock {
        #region Hooks
        public static void Load() {
            On.Celeste.Platform.EnableStaticMovers += Platform_EnableStaticMovers;

            IL.Celeste.CassetteBlock.FindInGroup += CassetteBlock_FindInGroup;
            On.Celeste.CassetteBlock.TryActorWiggleUp += CassetteBlock_TryActorWiggleUp;
            On.Celeste.CassetteBlock.UpdateVisualState += CassetteBlock_UpdateVisualState;
        }

        public static void Unload() {
            On.Celeste.Platform.EnableStaticMovers -= Platform_EnableStaticMovers;

            IL.Celeste.CassetteBlock.FindInGroup -= CassetteBlock_FindInGroup;
            On.Celeste.CassetteBlock.TryActorWiggleUp -= CassetteBlock_TryActorWiggleUp;
        }
        private static void Platform_EnableStaticMovers(On.Celeste.Platform.orig_EnableStaticMovers orig, Celeste.Platform self) {
            if (self is CassetteTileEntity && !self.Visible)
                return; // do nothing
            orig(self);
        }

        private static void CassetteBlock_FindInGroup(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            ILLabel label = null;
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchBr(out label)) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(1))) {
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate<Func<CassetteBlock, bool>>(c => c is CassetteTileEntity);
                cursor.Emit(OpCodes.Brtrue, label);
            }
        }
        private static bool CassetteBlock_TryActorWiggleUp(On.Celeste.CassetteBlock.orig_TryActorWiggleUp orig, CassetteBlock _self, Entity actor) {
            if (_self is not CassetteTileEntity self)
                return orig(_self, actor);
            return self._TryActorWiggleUp(actor);
        }
        private static void CassetteBlock_UpdateVisualState(On.Celeste.CassetteBlock.orig_UpdateVisualState orig, CassetteBlock _self) {
            if (_self is not CassetteTileEntity self) { orig(_self); return; }
            self._UpdateVisualState();

        }

        #endregion

        internal static System.Reflection.FieldInfo color = typeof(CassetteBlock).GetField("color", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        internal static System.Reflection.MethodInfo shiftSize = typeof(CassetteBlock).GetMethod("ShiftSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public char tileType;
        public bool blendin;
        public Color enabledColor, disabledColor;


        public List<CassetteTileEntity> group;
        public bool groupLeader;
        public bool connectOnTileset;
        public TileGrid tileGrid;


        public CassetteTileEntity(EntityData data, Vector2 offset) : base(data, offset) {
            enabledColor = data.ColorOrNull("enabledTint") ?? (Color) color.GetValue(this); //Optimized since we don't always need to act on reflection.
            disabledColor = data.ColorOrNull("disabledTint") ?? Color.Lerp(Color.LightGray, (Color) color.GetValue(this), 0.5f);
            tileType = data.Char("tiletype", '3');
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            blendin = data.Bool("blendin", false);
            connectOnTileset = data.Bool("ConnectTilesets");
            

        }

        [MonoModLinkTo("Celeste.Solid", "System.Void Awake(Monocle.Scene)")]
        internal void Solid_Awake(Scene scene) { Logger.Log("VivHelper","Link to Solid::Awake(Monocle.Scene) not found."); }

        [MonoModLinkTo("Celeste.Solid", "System.Void Update()")]
        internal void Solid_Update() { Logger.Log("VivHelper", "Link to Solid::Update() not found."); }

        public override void Awake(Scene scene) {
            Solid_Awake(scene);
            foreach (StaticMover staticMover in staticMovers) {
                Spikes spikes = staticMover.Entity as Spikes;
                if (spikes != null) {
                    spikes.EnabledColor = enabledColor;
                    spikes.DisabledColor = disabledColor;
                    spikes.VisibleWhenDisabled = true;
                    spikes.SetSpikeColor(Color.White);
                }
                Spring spring = staticMover.Entity as Spring;
                if (spring != null) {
                    spring.DisabledColor = disabledColor;
                    spring.VisibleWhenDisabled = true;
                }
            }
            if (group != null)
                return;
            groupLeader = true;
            group = new List<CassetteTileEntity>();
            group.Add(this);
            var GroupBoundsMin = new Point((int) base.Left, (int) base.Top);
            var GroupBoundsMax = new Point((int) base.Right, (int) base.Bottom);
            _FindInGroup(this, ref GroupBoundsMin, ref GroupBoundsMax);
            if (blendin) {
                Level level = scene as Level;
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int) (GroupBoundsMin.X / 8f) - tileBounds.Left;
                int y = (int) (GroupBoundsMin.Y / 8f) - tileBounds.Top;
                int tilesX = (int) (GroupBoundsMax.X - GroupBoundsMin.X) / 8;
                int tilesY = (int) (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
                Add(tileGrid);
                return;
            }
            Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8 + 1, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8 + 1);
            VirtualMap<char> virtualMap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');
            foreach (CassetteTileEntity item in group) {
                int num = (int) (item.X / 8f) - rectangle.X;
                int num2 = (int) (item.Y / 8f) - rectangle.Y;
                int num3 = (int) (item.Width / 8f);
                int num4 = (int) (item.Height / 8f);
                for (int i = num; i < num + num3; i++) {
                    for (int j = num2; j < num2 + num4; j++) {
                        virtualMap[i, j] = tileType;
                    }
                }
            }
            tileGrid = GFX.FGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour {
                EdgesExtend = false,
                EdgesIgnoreOutOfLevel = false,
                PaddingIgnoreOutOfLevel = false
            }).TileGrid;
            tileGrid.Position = new Vector2((float) GroupBoundsMin.X - base.X, (float) GroupBoundsMin.Y - base.Y);
            Add(tileGrid);
            return;
        }

        private void _FindInGroup(CassetteTileEntity block, ref Point min, ref Point max) {
            foreach (CassetteTileEntity entity in base.Scene.Tracker.GetEntities<CassetteTileEntity>()) {
                if (entity != this && entity != block && entity.Index == Index && (connectOnTileset ? VivHelper.DoesTilesetConnect(block.tileType, entity.tileType) : block.tileType == entity.tileType) && !group.Contains(entity) &&
                    (entity.CollideRect(new Rectangle((int) block.X - 1, (int) block.Y, (int) block.Width + 2, (int) block.Height)) || entity.CollideRect(new Rectangle((int) block.X, (int) block.Y - 1, (int) block.Width, (int) block.Height + 2)))) {
                    if (entity.X < (float) min.X) {
                        min.X = (int) entity.Left;
                    }
                    if (entity.Y < (float) min.Y) {
                        min.Y = (int) entity.Top;
                    }
                    if (entity.Right > (float) max.X) {
                        max.X = (int) entity.Right;
                    }
                    if (entity.Bottom > (float) max.Y) {
                        max.Y = (int) entity.Bottom;
                    }
                    group.Add(entity);
                    _FindInGroup(entity, ref min, ref max);
                    entity.group = group;
                }
            }
        }

        public override void Update() {
            Solid_Update();
            if (groupLeader && Activated && !Collidable) {
                bool flag = false;
                foreach (CassetteTileEntity item in group) {
                    if (item.BlockedCheck()) {
                        flag = true;
                        break;
                    }
                }
                if (!flag) {
                    foreach (CassetteTileEntity item2 in group) {
                        item2.Collidable = true;
                        item2.EnableStaticMovers();
                        shiftSize.Invoke(this, VivHelper.negOne);

                    }
                }
            } else if (!Activated && Collidable) {
                shiftSize.Invoke(this, VivHelper.oneOne);
                Collidable = false;
                DisableStaticMovers();
            }
            _UpdateVisualState();
        }

        public bool _TryActorWiggleUp(Entity actor) {
            foreach (CassetteTileEntity item in group) {
                if (item != this && item.CollideCheck(actor, item.Position + Vector2.UnitY * 4f)) {
                    return false;
                }
            }
            bool collidable = Collidable;
            Collidable = true;
            for (int i = 1; i <= 4; i++) {
                if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i)) {
                    actor.Position -= Vector2.UnitY * i;
                    Collidable = collidable;
                    return true;
                }
            }
            Collidable = collidable;
            return false;
        }

        /// <summary>
        /// Honestly, we're just using this as a blatant replacement to UpdateVisualState, we really don't want this at all but we may as well use it well since we're here.
        /// </summary>
        public void _UpdateVisualState() {
            if (!Collidable) {
                base.Depth = 8990;
            } else {
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.Top >= base.Bottom - 1f) {
                    base.Depth = 10;
                } else {
                    base.Depth = -10;
                }
            }
            foreach (StaticMover staticMover in staticMovers) {
                staticMover.Entity.Depth = base.Depth + 1;
            }
            Get<LightOcclude>().Visible = Collidable;
        }

        public override void Render() {
            if (tileGrid == null)
                return;
            tileGrid.Color = Collidable ? enabledColor : disabledColor;
            tileGrid.Render();
        }
    }
}
