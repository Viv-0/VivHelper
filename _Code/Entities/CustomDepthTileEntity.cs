﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/CustomDepthTileEntity")]
    public class CustomDepthTileEntity : Solid {
        private TileGrid tiles;

        private char tileType;
        private bool bg;

        private CustomDepthTileEntity master;

        public List<CustomDepthTileEntity> Group;

        public Point GroupBoundsMin;

        public Point GroupBoundsMax;

        public bool HasGroup {
            get;
            private set;
        }

        public bool MasterOfGroup {
            get;
            private set;
        }

        public CustomDepthTileEntity(Vector2 position, float width, float height, char tileType, int depth, bool bg, bool blockLights = true)
        : base(position, width, height, safe: true) {
            this.tileType = tileType;
            base.Depth = Calc.Clamp(depth, -300000, 20000);
            this.bg = bg;
            if(bg) {
                Collidable = false;
            }
            if(blockLights)
                Add(new LightOcclude());
            if(!SurfaceIndex.TileToIndex.TryGetValue(tileType, out SurfaceSoundIndex))
                SurfaceSoundIndex = SurfaceIndex.Brick;
        }

        public CustomDepthTileEntity(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.Int("Depth", -9000), data.Bool("BackgroundTile", false), data.Bool("BlockLights", true)) {
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (!HasGroup) {
                MasterOfGroup = true;
                Group = new List<CustomDepthTileEntity>();
                GroupBoundsMin = new Point((int) base.X, (int) base.Y);
                GroupBoundsMax = new Point((int) base.Right, (int) base.Bottom);
                AddToGroupAndFindChildren(this);
                _ = base.Scene;
                Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8 + 1, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8 + 1);
                VirtualMap<char> virtualMap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');
                foreach (CustomDepthTileEntity item in Group) {
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
                Autotiler tiler = bg ? GFX.BGAutotiler : GFX.FGAutotiler;
                tiles = tiler.GenerateMap(virtualMap, new Autotiler.Behaviour {
                    EdgesExtend = false,
                    EdgesIgnoreOutOfLevel = false,
                    PaddingIgnoreOutOfLevel = false
                }).TileGrid;
                tiles.Position = new Vector2((float) GroupBoundsMin.X - base.X, (float) GroupBoundsMin.Y - base.Y);
                Add(tiles);
            }
        }

        private void AddToGroupAndFindChildren(CustomDepthTileEntity from, List<Entity> entities = null) {
            if (from.X < (float) GroupBoundsMin.X) {
                GroupBoundsMin.X = (int) from.X;
            }
            if (from.Y < (float) GroupBoundsMin.Y) {
                GroupBoundsMin.Y = (int) from.Y;
            }
            if (from.Right > (float) GroupBoundsMax.X) {
                GroupBoundsMax.X = (int) from.Right;
            }
            if (from.Bottom > (float) GroupBoundsMax.Y) {
                GroupBoundsMax.Y = (int) from.Bottom;
            }
            from.HasGroup = true;
            Group.Add(from);
            if (from != this) {
                from.master = this;
            }
            // Implement variable entities so that it doesn't pull from hash per tileentity in the chain
            if (entities == null && !Scene.Tracker.TryGetEntities<CustomDepthTileEntity>(out entities)) 
                return;
            foreach (CustomDepthTileEntity entity in entities) {
                if (!entity.HasGroup && entity.tileType == tileType && entity.bg == bg && (base.Scene.CollideCheck(new Rectangle((int) from.X - 1, (int) from.Y, (int) from.Width + 2, (int) from.Height), entity) || base.Scene.CollideCheck(new Rectangle((int) from.X, (int) from.Y - 1, (int) from.Width, (int) from.Height + 2), entity))) {
                    AddToGroupAndFindChildren(entity, entities);
                }
            }
        }
    }
}
