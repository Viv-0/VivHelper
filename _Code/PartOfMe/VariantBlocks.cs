using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.PartOfMe {
    class VariantBlocks : Solid {
        public enum Modes {
            Solid,
            Leaving,
            Disabled,
            Returning
        }

        private class BoxSide : Entity {
            private VariantBlocks block;

            private Color color;

            public BoxSide(VariantBlocks block, Color color) {
                this.block = block;
                this.color = color;
            }

            public override void Render() {
                Draw.Rect(block.X, block.Y + block.Height - 8f, block.Width, 8 + block.blockHeight, color);
            }
        }

        public int Index;

        public float Tempo;

        public bool Activated;

        public Modes Mode;

        public EntityID ID;

        private int blockHeight = 2;

        private List<VariantBlocks> group;

        private bool groupLeader;

        private Vector2 groupOrigin;

        private Color color;

        private List<Image> pressed = new List<Image>();

        private List<Image> solid = new List<Image>();

        private List<Image> all = new List<Image>();

        private LightOcclude occluder;

        private Wiggler wiggler;

        private Vector2 wigglerScaler;

        private BoxSide side;

        public VariantBlocks(Vector2 position, EntityID id, float width, float height, int index, float tempo)
            : base(position, width, height, safe: false) {
            SurfaceSoundIndex = 35;
            Index = index;
            Tempo = tempo;
            Collidable = false;
            ID = id;
            switch (Index) {
                default:
                    color = Calc.HexToColor("49aaf0");
                    break;
                case 1:
                    color = Calc.HexToColor("f049be");
                    break;
                case 2:
                    color = Calc.HexToColor("fcdc3a");
                    break;
                case 3:
                    color = Calc.HexToColor("38e04e");
                    break;
            }
            Add(occluder = new LightOcclude());
        }

        public VariantBlocks(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, id, data.Width, data.Height, data.Int("index"), data.Float("tempo", 1f)) {
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Color color = Calc.HexToColor("667da5");
            Color disabledColor = new Color((float) (int) color.R / 255f * ((float) (int) this.color.R / 255f), (float) (int) color.G / 255f * ((float) (int) this.color.G / 255f), (float) (int) color.B / 255f * ((float) (int) this.color.B / 255f), 1f);
            scene.Add(side = new BoxSide(this, disabledColor));
            foreach (StaticMover staticMover in staticMovers) {
                Spikes spikes = staticMover.Entity as Spikes;
                if (spikes != null) {
                    spikes.EnabledColor = this.color;
                    spikes.DisabledColor = disabledColor;
                    spikes.VisibleWhenDisabled = true;
                    spikes.SetSpikeColor(this.color);
                }
                Spring spring = staticMover.Entity as Spring;
                if (spring != null) {
                    spring.DisabledColor = disabledColor;
                    spring.VisibleWhenDisabled = true;
                }
            }
            if (group == null) {
                groupLeader = true;
                group = new List<VariantBlocks>();
                group.Add(this);
                FindInGroup(this);
                float num = float.MaxValue;
                float num2 = float.MinValue;
                float num3 = float.MaxValue;
                float num4 = float.MinValue;
                foreach (VariantBlocks item in group) {
                    if (item.Left < num) {
                        num = item.Left;
                    }
                    if (item.Right > num2) {
                        num2 = item.Right;
                    }
                    if (item.Bottom > num4) {
                        num4 = item.Bottom;
                    }
                    if (item.Top < num3) {
                        num3 = item.Top;
                    }
                }
                groupOrigin = new Vector2((int) (num + (num2 - num) / 2f), (int) num4);
                wigglerScaler = new Vector2(Calc.ClampedMap(num2 - num, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(num4 - num3, 32f, 96f, 1f, 0.2f));
                Add(wiggler = Wiggler.Create(0.3f, 3f));
                foreach (VariantBlocks item2 in group) {
                    item2.wiggler = wiggler;
                    item2.wigglerScaler = wigglerScaler;
                    item2.groupOrigin = groupOrigin;
                }
            }
            foreach (StaticMover staticMover2 in staticMovers) {
                (staticMover2.Entity as Spikes)?.SetOrigins(groupOrigin);
            }
            for (float num5 = base.Left; num5 < base.Right; num5 += 8f) {
                for (float num6 = base.Top; num6 < base.Bottom; num6 += 8f) {
                    bool flag = CheckForSame(num5 - 8f, num6);
                    bool flag2 = CheckForSame(num5 + 8f, num6);
                    bool flag3 = CheckForSame(num5, num6 - 8f);
                    bool flag4 = CheckForSame(num5, num6 + 8f);
                    if ((flag && flag2) & flag3 & flag4) {
                        if (!CheckForSame(num5 + 8f, num6 - 8f)) {
                            SetImage(num5, num6, 3, 0);
                        } else if (!CheckForSame(num5 - 8f, num6 - 8f)) {
                            SetImage(num5, num6, 3, 1);
                        } else if (!CheckForSame(num5 + 8f, num6 + 8f)) {
                            SetImage(num5, num6, 3, 2);
                        } else if (!CheckForSame(num5 - 8f, num6 + 8f)) {
                            SetImage(num5, num6, 3, 3);
                        } else {
                            SetImage(num5, num6, 1, 1);
                        }
                    } else if ((flag && flag2 && !flag3) & flag4) {
                        SetImage(num5, num6, 1, 0);
                    } else if (((flag && flag2) & flag3) && !flag4) {
                        SetImage(num5, num6, 1, 2);
                    } else if ((flag && !flag2) & flag3 & flag4) {
                        SetImage(num5, num6, 2, 1);
                    } else if ((!flag && flag2) & flag3 & flag4) {
                        SetImage(num5, num6, 0, 1);
                    } else if ((flag && !flag2 && !flag3) & flag4) {
                        SetImage(num5, num6, 2, 0);
                    } else if ((!flag && flag2 && !flag3) & flag4) {
                        SetImage(num5, num6, 0, 0);
                    } else if (((flag && !flag2) & flag3) && !flag4) {
                        SetImage(num5, num6, 2, 2);
                    } else if (((!flag && flag2) & flag3) && !flag4) {
                        SetImage(num5, num6, 0, 2);
                    }
                }
            }
            UpdateVisualState();
        }

        private void FindInGroup(VariantBlocks block) {
            foreach (VariantBlocks entity in base.Scene.Tracker.GetEntities<VariantBlocks>()) {
                if (entity != this && entity != block && entity.Index == Index && (entity.CollideRect(new Rectangle((int) block.X - 1, (int) block.Y, (int) block.Width + 2, (int) block.Height)) || entity.CollideRect(new Rectangle((int) block.X, (int) block.Y - 1, (int) block.Width, (int) block.Height + 2))) && !group.Contains(entity)) {
                    group.Add(entity);
                    FindInGroup(entity);
                    entity.group = group;
                }
            }
        }

        private bool CheckForSame(float x, float y) {
            foreach (VariantBlocks entity in base.Scene.Tracker.GetEntities<VariantBlocks>()) {
                if (entity.Index == Index && entity.Collider.Collide(new Rectangle((int) x, (int) y, 8, 8))) {
                    return true;
                }
            }
            return false;
        }

        private void SetImage(float x, float y, int tx, int ty) {
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/cassetteblock/pressed");
            pressed.Add(CreateImage(x, y, tx, ty, atlasSubtextures[Index % atlasSubtextures.Count]));
            solid.Add(CreateImage(x, y, tx, ty, GFX.Game["objects/cassetteblock/solid"]));
        }

        private Image CreateImage(float x, float y, int tx, int ty, MTexture tex) {
            Vector2 value = new Vector2(x - base.X, y - base.Y);
            Image image = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
            Vector2 vector = groupOrigin - Position;
            image.Origin = vector - value;
            image.Position = vector;
            image.Color = color;
            Add(image);
            all.Add(image);
            return image;
        }

        public override void Update() {
            base.Update();
            if (groupLeader && Activated && !Collidable) {
                bool flag = false;
                foreach (VariantBlocks item in group) {
                    if (item.BlockedCheck()) {
                        flag = true;
                        break;
                    }
                }
                if (!flag) {
                    foreach (VariantBlocks item2 in group) {
                        item2.Collidable = true;
                        item2.EnableStaticMovers();
                        item2.ShiftSize(-1);
                    }
                    wiggler.Start();
                }
            } else if (!Activated && Collidable) {
                ShiftSize(1);
                Collidable = false;
                DisableStaticMovers();
            }
            UpdateVisualState();
        }

        public bool BlockedCheck() {
            TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
            if (theoCrystal != null && !TryActorWiggleUp(theoCrystal)) {
                return true;
            }
            Player player = CollideFirst<Player>();
            if (player != null && !TryActorWiggleUp(player)) {
                return true;
            }
            return false;
        }

        private void UpdateVisualState() {
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
            side.Depth = base.Depth + 5;
            side.Visible = (blockHeight > 0);
            occluder.Visible = Collidable;
            foreach (Image item in solid) {
                item.Visible = Collidable;
            }
            foreach (Image item2 in pressed) {
                item2.Visible = !Collidable;
            }
            if (groupLeader) {
                Vector2 scale = new Vector2(1f + wiggler.Value * 0.05f * wigglerScaler.X, 1f + wiggler.Value * 0.15f * wigglerScaler.Y);
                foreach (VariantBlocks item3 in group) {
                    foreach (Image item4 in item3.all) {
                        item4.Scale = scale;
                    }
                    foreach (StaticMover staticMover2 in item3.staticMovers) {
                        Spikes spikes = staticMover2.Entity as Spikes;
                        if (spikes != null) {
                            foreach (Component component in spikes.Components) {
                                Image image = component as Image;
                                if (image != null) {
                                    image.Scale = scale;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetActivatedSilently(bool activated) {
            Activated = (Collidable = activated);
            UpdateVisualState();
            if (activated) {
                EnableStaticMovers();
                return;
            }
            ShiftSize(2);
            DisableStaticMovers();
        }

        public void Finish() {
            Activated = false;
        }

        public void WillToggle() {
            ShiftSize(Collidable ? 1 : (-1));
            UpdateVisualState();
        }

        private void ShiftSize(int amount) {
            MoveV(amount);
            blockHeight -= amount;
        }

        private bool TryActorWiggleUp(Entity actor) {
            foreach (VariantBlocks item in group) {
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

        public VariantBlocks(EntityData data, Vector2 offset)
            : this(data, offset, new EntityID(data.Level.Name, data.ID)) {
        }
    }
}
