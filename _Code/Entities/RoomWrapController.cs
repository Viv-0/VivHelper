using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.VivHelper;
using VivHelper.Triggers;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/RoomWrapController")]
    public class RoomWrapController : Entity {
        public bool scrollT, scrollR, scrollB, scrollL;
        public bool allEntities;
        public bool setByCamera;
        public float[] playerOffsets;
        public Level level;
        public Vector2[] a_ES;
        public static float m_ES = 2400f;
        public string[] flag;
        public string flagstring;
        public bool automateCTT, lockCamera;

        public RoomWrapController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            playerOffsets = new float[4];
            a_ES = new Vector2[4];
            scrollT = data.Bool("Top", false);
            flagstring = data.Attr("ZFlagsData", "");
            flag = data.Attr("ZFlagsData", "").Split(',');
            playerOffsets[0] = data.Float("TopOffset", 4f);
            if (scrollT) {

                a_ES[0] = new Vector2(0f, (Math.Abs(data.Float("TopExitSpeedAdd", 0f)) < m_ES ? Math.Abs(data.Float("TopExitSpeedAdd", 0f)) : m_ES));
            }
            scrollR = data.Bool("Right", false);
            playerOffsets[1] = 0 - data.Float("RightOffset", 5f);
            if (scrollR) {

                a_ES[1] = new Vector2((Math.Abs(data.Float("RightExitSpeedAdd", 0f)) < m_ES ? 0f - Math.Abs(data.Float("RightExitSpeedAdd", 0f)) : -m_ES), 0f);
            }
            scrollB = data.Bool("Bottom", false);
            playerOffsets[2] = 0 - data.Float("BottomOffset", 8f);
            if (scrollB) {

                a_ES[2] = new Vector2(0f, (Math.Abs(data.Float("BottomExitSpeedAdd", 0f)) < m_ES ? 0f - Math.Abs(data.Float("BottomExitSpeedAdd", 0f)) : -m_ES));
            }
            scrollL = data.Bool("Left", false);
            playerOffsets[3] = data.Float("LeftOffset", 5f);
            if (scrollL) {

                a_ES[3] = new Vector2((Math.Abs(data.Float("LeftExitSpeedAdd", 0f)) < m_ES ? Math.Abs(data.Float("LeftExitSpeedAdd", 0f)) : m_ES), 0f);
            }
            setByCamera = data.Bool("setByCamera", false);
            allEntities = data.Bool("allEntities", false);
            automateCTT = data.Bool("AutomateCameraTriggers", false);
            lockCamera = data.Bool("LockCameraOnTeleport", false); //false because legacy behavior

        }
        public override void Awake(Scene scene) {
            base.Awake(scene);
            level = SceneAs<Level>();
            if (automateCTT) { AutomateCTT(level.Bounds, flag); }
        }
        public void AutomateCTT(Rectangle B, string[] c) {
            List<Entity> automatedCameraTriggers = new List<Entity>();
            if (scrollT) {
                EntityData temp = new EntityData {
                    Position = new Vector2(B.X, B.Y),
                    Width = B.Width,
                    Height = 8 * (13 + (int) playerOffsets[0]),
                    Nodes = new Vector2[1],
                    Values = new Dictionary<string, object> {
                        {"xOnly", false },
                        {"yOnly", true },
                        {"lerpStrength", 2f } }
                };
                temp.Nodes[0] = new Vector2(B.X + B.Width / 2f, B.Y + 8 * (14 + (int) playerOffsets[0]));
                automatedCameraTriggers.Add(new MultiflagCameraTargetTrigger(temp, new Vector2(0, 0), c));
            }
            if (scrollR) {
                EntityData temp = new EntityData {
                    Position = new Vector2(B.X + B.Width - 8 * (21 + (int) playerOffsets[1]), B.Y),
                    Width = 20 + (int) playerOffsets[1],
                    Height = B.Height,
                    Nodes = new Vector2[1],
                    Values = new Dictionary<string, object> {
                        {"xOnly", true },
                        {"yOnly", false },
                        {"lerpStrength", 2f } }
                };
                temp.Nodes[0] = new Vector2(B.X + B.Width - 8 * (21 + (int) playerOffsets[1]), B.Y + B.Height / 2f);
                automatedCameraTriggers.Add(new MultiflagCameraTargetTrigger(temp, new Vector2(0, 0), c));
            }
            if (scrollB) {
                EntityData temp = new EntityData {
                    Position = new Vector2(B.X, B.Y + B.Height + 8 * (14 + (int) playerOffsets[2])),
                    Width = B.Width,
                    Height = 13 + (int) playerOffsets[2],
                    Nodes = new Vector2[1],
                    Values = new Dictionary<string, object> {
                        {"xOnly", false },
                        {"yOnly", true },
                        {"lerpStrength", 2f } }
                };
                temp.Nodes[0] = new Vector2(B.X + B.Width / 2f, B.Y + B.Height + 8 * (14 + (int) playerOffsets[0]));
                automatedCameraTriggers.Add(new MultiflagCameraTargetTrigger(temp, new Vector2(0, 0), c));
            }
            if (scrollL) {
                EntityData temp = new EntityData {
                    Position = new Vector2(B.X, B.Y),
                    Width = 20 + (int) playerOffsets[3],
                    Height = B.Height,
                    Nodes = new Vector2[1],
                    Values = new Dictionary<string, object> {
                        {"xOnly", true },
                        {"yOnly", false },
                        {"lerpStrength", 2f } }
                };
                temp.Nodes[0] = new Vector2(B.X + 8 * (21 + (int) playerOffsets[3]), B.Y + B.Height / 2f);
                automatedCameraTriggers.Add(new MultiflagCameraTargetTrigger(temp, new Vector2(0, 0), c));
            }
            foreach (Entity t in automatedCameraTriggers) {
                base.SceneAs<Level>().Add(t);
            }
        }

        public override void Update() {
            base.Update();
            Rectangle bounds = level?.Bounds ?? Rectangle.Empty;
            if (bounds == Rectangle.Empty) {
                level = SceneAs<Level>();
                return;
            }
            if (setByCamera) {

                Camera camera = level.Camera;
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && VivHelperModule.OldGetFlags(level, flag, "and")) {
                    if (scrollB && player.Top > camera.Bottom - playerOffsets[2]) {
                        level.OnEndOfFrame += delegate {
                            player.Bottom = bounds.Top + 8f + playerOffsets[0];
                            player.Speed = Vector2.Add(player.Speed, a_ES[0]);
                            if (lockCamera) {
                                //Camera magic, this is such a dumb strategy but it works
                                foreach (Trigger trigger in level.Tracker.GetEntities<Trigger>().Where(t => Collide.Check(player, t) && t.GetType().Name.IndexOf("camera", StringComparison.OrdinalIgnoreCase) > -1)) {
                                    VivHelper.PlayerTriggerCheck(player, trigger);
                                }
                                VivHelperModule.Session.lockCamera = 1;
                            }
                        };
                    }
                    if (scrollR && player.Left > camera.Right - playerOffsets[1]) {
                        level.OnEndOfFrame += delegate {
                            player.Right = bounds.Left + 8f + playerOffsets[3];
                            player.Speed = Vector2.Add(player.Speed, a_ES[3]);
                            if (lockCamera) {
                                //Camera magic, this is such a dumb strategy but it works
                                foreach (Trigger trigger in level.Tracker.GetEntities<Trigger>().Where(t => Collide.Check(player, t) && t.GetType().Name.IndexOf("camera", StringComparison.OrdinalIgnoreCase) > -1)) {
                                    VivHelper.PlayerTriggerCheck(player, trigger);
                                }
                                VivHelperModule.Session.lockCamera = 1;
                            }
                        };
                    }
                    if (scrollT && player.Bottom < camera.Top - playerOffsets[0]) {
                        level.OnEndOfFrame += delegate {
                            player.Top = bounds.Bottom - 8f + playerOffsets[2];
                            player.Speed = Vector2.Add(player.Speed, a_ES[2]);
                            if (lockCamera) {
                                //Camera magic, this is such a dumb strategy but it works
                                foreach (Trigger trigger in level.Tracker.GetEntities<Trigger>().Where(t => Collide.Check(player, t) && t.GetType().Name.IndexOf("camera", StringComparison.OrdinalIgnoreCase) > -1)) {
                                    VivHelper.PlayerTriggerCheck(player, trigger);
                                }
                                VivHelperModule.Session.lockCamera = 1;
                            }
                        };
                    }
                    if (scrollL && player.Right < camera.Left - playerOffsets[3]) {
                        level.OnEndOfFrame += delegate {
                            player.Left = bounds.Right - 8f + playerOffsets[1];
                            player.Speed = Vector2.Add(player.Speed, a_ES[1]);
                            if (lockCamera) {
                                //Camera magic, this is such a dumb strategy but it works
                                foreach (Trigger trigger in level.Tracker.GetEntities<Trigger>().Where(t => Collide.Check(player, t) && t.GetType().Name.IndexOf("camera", StringComparison.OrdinalIgnoreCase) > -1)) {
                                    VivHelper.PlayerTriggerCheck(player, trigger);
                                }
                                VivHelperModule.Session.lockCamera = 1;
                            }
                        };
                    }
                }
            } else {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && VivHelperModule.OldGetFlags(level, flag, "and")) {
                    if (scrollB && player.Top > bounds.Bottom - 8f + playerOffsets[2]) { level.OnEndOfFrame += delegate { player.Bottom = bounds.Top + 8f + playerOffsets[0]; player.Speed = Vector2.Add(player.Speed, a_ES[0]); }; }
                    if (scrollR && player.Left > bounds.Right - 8f + playerOffsets[1]) { level.OnEndOfFrame += delegate { player.Right = bounds.Left + 8f + playerOffsets[3]; player.Speed = Vector2.Add(player.Speed, a_ES[3]); }; }
                    if (scrollT && player.Bottom < bounds.Top + 8f + playerOffsets[0]) { level.OnEndOfFrame += delegate { player.Top = bounds.Bottom - 8f + playerOffsets[2]; player.Speed = Vector2.Add(player.Speed, a_ES[2]); }; }
                    if (scrollL && player.Right < bounds.Left + 8f + playerOffsets[3]) { level.OnEndOfFrame += delegate { player.Left = bounds.Right - 8f + playerOffsets[1]; player.Speed = Vector2.Add(player.Speed, a_ES[1]); }; }
                }
            }

        }

        public override void DebugRender(Camera camera) {
            if (VivHelperModule.OldGetFlags(level, flag, "and")) {
                Rectangle bounds = level.Bounds;
                if (scrollT) {
                    Draw.Line(new Vector2(bounds.Left + 8f + playerOffsets[3], bounds.Top + 8f + playerOffsets[0]),
                              new Vector2(bounds.Right - 8f + playerOffsets[1], bounds.Top + 8f + playerOffsets[0]),
                              Color.LightSeaGreen * 0.5f);
                }
                if (scrollR) {
                    Draw.Line(new Vector2(bounds.Right - 8f + playerOffsets[1], bounds.Top + 8f + playerOffsets[0]),
                              new Vector2(bounds.Right - 8f + playerOffsets[1], bounds.Bottom - 8f + playerOffsets[2]),
                              Color.LightSeaGreen * 0.5f);
                }
                if (scrollB) {
                    Draw.Line(new Vector2(bounds.Left + 8f + playerOffsets[3], bounds.Bottom - 8f + playerOffsets[2]),
                              new Vector2(bounds.Right - 8f + playerOffsets[1], bounds.Bottom - 8f + playerOffsets[2]),
                              Color.LightSeaGreen * 0.5f);
                }
                if (scrollL) {
                    Draw.Line(new Vector2(bounds.Left + 8f + playerOffsets[3], bounds.Top + 8f + playerOffsets[0]),
                              new Vector2(bounds.Left + 8f + playerOffsets[3], bounds.Bottom - 8f + playerOffsets[2]),
                              Color.LightSeaGreen * 0.5f);
                }
            }

        }

    }
}
