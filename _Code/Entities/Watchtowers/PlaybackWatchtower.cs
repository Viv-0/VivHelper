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
using VivHelper.Entities.Watchtowers;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CustomPlaybackWatchtower")]
    public class CustomPlaybackWatchtower : Entity {

        private TalkComponent talk;

        private Hud hud;

        private Sprite sprite;

        private Tween lightTween;

        private bool interacting;

        private bool onlyY;

        private List<Vector2> nodes;

        private int node;

        private float nodePercent;

        private bool summit;

        protected bool toggle, prev, instantStart;

        private string animPrefix = "";

        public string tag;

        public string flagWhenInHud;

        public bool invertFlag;

        public bool ignoreBind;

        private CustomPlayerPlayback playback;

        private bool setOnAwake;

        #region Custom Fields
        private float accel;
        private float maxSpeed;
        private string xmlPath;
        #endregion

        public CustomPlaybackWatchtower(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            base.Depth = -8500;
            Add(talk = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(-0.5f, -20f), Interact));
            talk.PlayerMustBeFacing = false;
            summit = data.Bool("summit");
            onlyY = data.Bool("onlyY");
            base.Collider = new Hitbox(4f, 4f, -2f, -4f);
            VertexLight vertexLight = new VertexLight(new Vector2(-1f, -11f), Color.White, 0.8f, 16, 24);
            Add(vertexLight);
            lightTween = vertexLight.CreatePulseTween();
            Add(lightTween);
            xmlPath = data.Attr("XMLName", "");
            if (xmlPath == "")
                xmlPath = "lookout";
            Add(sprite = GFX.SpriteBank.Create(xmlPath));
            sprite.OnFrameChange = delegate (string s) {
                if ((s == "idle" || s == "badeline_idle" || s == "nobackpack_idle") && sprite.CurrentAnimationFrame == sprite.CurrentAnimationTotalFrames - 1) {
                    lightTween.Start();
                }
            };
            Vector2[] array = data.NodesOffset(offset);
            if (array != null && array.Length != 0) {
                nodes = new List<Vector2>(array); //bro how did I miss this shit
            }
            tag = data.Attr("CustomPlaybackTag");
            accel = data.Float("Accel", 800f);
            maxSpeed = data.Float("maxSpeed", 240f);
            invertFlag = (data.Attr("FlagWhileInHud", "") == "" ? false : data.Attr("FlagWhileInHud", "")[0] == '!');
            flagWhenInHud = invertFlag ? data.Attr("FlagWhileInHud", "").Substring(1) : data.Attr("FlagWhileInHud", "");
            ignoreBind = data.Bool("IgnoreBind", false);
            setOnAwake = data.Bool("SetOnAwake", true);
            instantStart = data.Bool("InstantStart", false);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            playback = null;
            foreach (CustomPlayerPlayback cpp in Scene.Tracker.GetEntities<CustomPlayerPlayback>()) {
                if (cpp.customID == tag) { playback = cpp; playback.active = false; break; }
            }
            toggle = prev = false;
            if (flagWhenInHud != "" && setOnAwake)
                (scene as Level).Session.SetFlag(flagWhenInHud, invertFlag);
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            if (interacting) {
                Player entity = scene.Tracker.GetEntity<Player>();
                if (entity != null) {
                    entity.StateMachine.State = 0;
                }
            }
        }

        private void Interact(Player player) {
            if (player.DefaultSpriteMode == PlayerSpriteMode.MadelineAsBadeline || SaveData.Instance.Assists.PlayAsBadeline) {
                animPrefix = "badeline_";
            } else if (player.DefaultSpriteMode == PlayerSpriteMode.MadelineNoBackpack) {
                animPrefix = "nobackpack_";
            } else {
                animPrefix = "";
            }
            Coroutine coroutine = new Coroutine(LookRoutine(player));
            coroutine.RemoveOnComplete = true;
            Add(coroutine);
            interacting = true;
        }

        public void StopInteracting() {
            interacting = false;
            sprite.Play(animPrefix + "idle");
        }

        public override void Update() {
            if (talk.UI != null) {
                talk.UI.Visible = !CollideCheck<Solid>();
            }
            base.Update();
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null) {
                sprite.Active = (interacting || entity.StateMachine.State != 11);
                if (!sprite.Active) {
                    sprite.SetAnimationFrame(0);
                }
            }
        }

        private IEnumerator LookRoutine(Player player) {
            Level level = SceneAs<Level>();
            SandwichLava sandwichLava = Scene.Entities.FindFirst<SandwichLava>();
            if (sandwichLava != null) {
                sandwichLava.Waiting = true;
            }
            if (player.Holding != null) {
                player.Drop();
            }
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int) X, walkBackwards: false, 1f, cancelOnFall: true);
            if (Math.Abs(X - player.X) > 4f || player.Dead || !player.OnGround()) {
                if (!player.Dead) {
                    player.StateMachine.State = 0;
                }
                yield break;
            }
            Audio.Play("event:/game/general/lookout_use", Position);
            if (player.Facing == Facings.Right) {
                sprite.Play(animPrefix + "lookRight");
            } else {
                sprite.Play(animPrefix + "lookLeft");
            }
            player.Sprite.Visible = (player.Hair.Visible = false);
            yield return 0.2f;
            Scene.Add(hud = new Hud());
            hud.TrackMode = (nodes != null);
            hud.OnlyY = onlyY;
            nodePercent = 0f;
            node = 0;
            Audio.Play("event:/ui/game/lookout_on");
            while ((hud.Easer = Calc.Approach(hud.Easer, 1f, Engine.DeltaTime * 3f)) < 1f) {
                level.ScreenPadding = (int) (Ease.CubeInOut(hud.Easer) * 16f);
                yield return null;
            }
            if (flagWhenInHud != "") { (Scene as Level).Session.SetFlag(flagWhenInHud, !invertFlag); }
            Vector2 cam = level.Camera.Position;
            Vector2 speed = Vector2.Zero;
            Vector2 lastDir = Vector2.Zero;
            Vector2 camStart = level.Camera.Position;
            Vector2 camStartCenter = camStart + new Vector2(160f, 90f);
            if (playback != null && (ignoreBind || instantStart)) {
                playback.Restart();
                playback.active = true;
            }
            while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !Input.Jump.Pressed && interacting) {
                if (playback != null && !ignoreBind) {
                    if (VivHelperModule.Settings.DisplayPlaybacksInLookout.Pressed)
                        toggle = !toggle;
                    if (toggle != prev) {
                        if (toggle) {
                            while (!VivHelperModule.Settings.DisplayPlaybacksInLookout.Released)
                                yield return null;
                            playback.Restart();
                            playback.active = true;
                        } else {
                            while (!VivHelperModule.Settings.DisplayPlaybacksInLookout.Released)
                                yield return null;
                            playback.Visible = playback.active = false;
                        }
                    }
                }
                prev = toggle;
                Vector2 value = Input.Aim.Value;
                if (onlyY) {
                    value.X = 0f;
                }
                if (Math.Sign(value.X) != Math.Sign(lastDir.X) || Math.Sign(value.Y) != Math.Sign(lastDir.Y)) {
                    Audio.Play("event:/game/general/lookout_move", Position);
                }
                lastDir = value;
                if (sprite.CurrentAnimationID != "lookLeft" && sprite.CurrentAnimationID != "lookRight") {
                    if (value.X == 0f) {
                        if (value.Y == 0f) {
                            sprite.Play(animPrefix + "looking");
                        } else if (value.Y > 0f) {
                            sprite.Play(animPrefix + "lookingDown");
                        } else {
                            sprite.Play(animPrefix + "lookingUp");
                        }
                    } else if (value.X > 0f) {
                        if (value.Y == 0f) {
                            sprite.Play(animPrefix + "lookingRight");
                        } else if (value.Y > 0f) {
                            sprite.Play(animPrefix + "lookingDownRight");
                        } else {
                            sprite.Play(animPrefix + "lookingUpRight");
                        }
                    } else if (value.X < 0f) {
                        if (value.Y == 0f) {
                            sprite.Play(animPrefix + "lookingLeft");
                        } else if (value.Y > 0f) {
                            sprite.Play(animPrefix + "lookingDownLeft");
                        } else {
                            sprite.Play(animPrefix + "lookingUpLeft");
                        }
                    }
                }
                if (nodes == null) {
                    speed += accel * value * Engine.DeltaTime;
                    if (value.X == 0f) {
                        speed.X = Calc.Approach(speed.X, 0f, accel * 2f * Engine.DeltaTime);
                    }
                    if (value.Y == 0f) {
                        speed.Y = Calc.Approach(speed.Y, 0f, accel * 2f * Engine.DeltaTime);
                    }
                    if (speed.Length() > maxSpeed) {
                        speed = speed.SafeNormalize(maxSpeed);
                    }
                    Vector2 vector = cam;
                    List<Entity> entities = Scene.Tracker.GetEntities<LookoutBlocker>();
                    cam.X += speed.X * Engine.DeltaTime;
                    if (cam.X < (float) level.Bounds.Left || cam.X + 320f > (float) level.Bounds.Right) {
                        speed.X = 0f;
                    }
                    cam.X = Calc.Clamp(cam.X, level.Bounds.Left, level.Bounds.Right - 320);
                    foreach (Entity item in entities) {
                        if (cam.X + 320f > item.Left && cam.Y + 180f > item.Top && cam.X < item.Right && cam.Y < item.Bottom) {
                            cam.X = vector.X;
                            speed.X = 0f;
                        }
                    }
                    cam.Y += speed.Y * Engine.DeltaTime;
                    if (cam.Y < (float) level.Bounds.Top || cam.Y + 180f > (float) level.Bounds.Bottom) {
                        speed.Y = 0f;
                    }
                    cam.Y = Calc.Clamp(cam.Y, level.Bounds.Top, level.Bounds.Bottom - 180);
                    foreach (Entity item2 in entities) {
                        if (cam.X + 320f > item2.Left && cam.Y + 180f > item2.Top && cam.X < item2.Right && cam.Y < item2.Bottom) {
                            cam.Y = vector.Y;
                            speed.Y = 0f;
                        }
                    }
                    level.Camera.Position = cam;
                } else {
                    Vector2 vector2 = (node <= 0) ? camStartCenter : nodes[node - 1];
                    Vector2 vector3 = nodes[node];
                    float num = (vector2 - vector3).Length();
                    (vector3 - vector2).SafeNormalize();
                    if (nodePercent < 0.25f && node > 0) {
                        Vector2 begin = Vector2.Lerp((node <= 1) ? camStartCenter : nodes[node - 2], vector2, 0.75f);
                        Vector2 end = Vector2.Lerp(vector2, vector3, 0.25f);
                        SimpleCurve simpleCurve = new SimpleCurve(begin, end, vector2);
                        level.Camera.Position = simpleCurve.GetPoint(0.5f + nodePercent / 0.25f * 0.5f);
                    } else if (nodePercent > 0.75f && node < nodes.Count - 1) {
                        Vector2 value2 = nodes[node + 1];
                        Vector2 begin2 = Vector2.Lerp(vector2, vector3, 0.75f);
                        Vector2 end2 = Vector2.Lerp(vector3, value2, 0.25f);
                        SimpleCurve simpleCurve2 = new SimpleCurve(begin2, end2, vector3);
                        level.Camera.Position = simpleCurve2.GetPoint((nodePercent - 0.75f) / 0.25f * 0.5f);
                    } else {
                        level.Camera.Position = Vector2.Lerp(vector2, vector3, nodePercent);
                    }
                    level.Camera.Position += new Vector2(-160f, -90f);
                    nodePercent -= value.Y * (maxSpeed / num) * Engine.DeltaTime;
                    if (nodePercent < 0f) {
                        if (node > 0) {
                            node--;
                            nodePercent = 1f;
                        } else {
                            nodePercent = 0f;
                        }
                    } else if (nodePercent > 1f) {
                        if (node < nodes.Count - 1) {
                            node++;
                            nodePercent = 0f;
                        } else {
                            nodePercent = 1f;
                            if (summit) {
                                break;
                            }
                        }
                    }
                    float num2 = 0f;
                    float num3 = 0f;
                    for (int i = 0; i < nodes.Count; i++) {
                        float num4 = (((i == 0) ? camStartCenter : nodes[i - 1]) - nodes[i]).Length();
                        num3 += num4;
                        if (i < node) {
                            num2 += num4;
                        } else if (i == node) {
                            num2 += num4 * nodePercent;
                        }
                    }
                    hud.TrackPercent = num2 / num3;
                }
                yield return null;
            }
            if (playback != null) {
                playback.Visible = playback.active = false;
            }
            player.Sprite.Visible = (player.Hair.Visible = true);
            sprite.Play(animPrefix + "idle");
            Audio.Play("event:/ui/game/lookout_off");
            while ((hud.Easer = Calc.Approach(hud.Easer, 0f, Engine.DeltaTime * 3f)) > 0f) {
                level.ScreenPadding = (int) (Ease.CubeInOut(hud.Easer) * 16f);
                yield return null;
            }
            bool atSummitTop = summit && node >= nodes.Count - 1 && nodePercent >= 0.95f;
            if (atSummitTop) {
                yield return 0.5f;
                float duration2 = 3f;
                float approach2 = 0f;
                Coroutine component = new Coroutine(level.ZoomTo(new Vector2(160f, 90f), 2f, duration2));
                Add(component);
                while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !Input.Jump.Pressed && interacting) {
                    approach2 = Calc.Approach(approach2, 1f, Engine.DeltaTime / duration2);
                    Audio.SetMusicParam("escape", approach2);
                    yield return null;
                }
            }
            if ((camStart - level.Camera.Position).Length() > 600f) {
                Vector2 was = level.Camera.Position;
                Vector2 direction = (was - camStart).SafeNormalize();
                float approach2 = atSummitTop ? 1f : 0.5f;
                new FadeWipe(Scene, wipeIn: false).Duration = approach2;
                for (float duration2 = 0f; duration2 < 1f; duration2 += Engine.DeltaTime / approach2) {
                    level.Camera.Position = was - direction * MathHelper.Lerp(0f, 64f, Ease.CubeIn(duration2));
                    yield return null;
                }
                level.Camera.Position = camStart + direction * 32f;
                new FadeWipe(Scene, wipeIn: true);
            }
            if (flagWhenInHud != "")
                (Scene as Level).Session.SetFlag(flagWhenInHud, invertFlag);
            Audio.SetMusicParam("escape", 0f);
            level.ScreenPadding = 0f;
            level.ZoomSnap(Vector2.Zero, 1f);
            Scene.Remove(hud);
            if (playback != null)
                playback.Visible = false;
            interacting = false;
            player.StateMachine.State = 0;
            yield return null;
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            if (interacting) {
                Player entity = scene.Tracker.GetEntity<Player>();
                if (entity != null) {
                    entity.StateMachine.State = 0;
                    entity.Sprite.Visible = (entity.Hair.Visible = true);
                }
            }
        }
    }
}
