﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using MonoMod;
using MonoMod.Utils;
using System.Reflection;
using static VivHelper.VivHelper;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/RefillWallWrapper")]
    public class WrapperRefillWall : Entity {
        DynamicData dynRefill;
        bool isRefillSubclass;

        string typeName;
        string spriteVarname;
        string respawnMethodName;
        string respawnTimerName;

        Entity tiedEntity;

        Image texture;
        bool forceRender;
        private bool drawWithOutline;
        public bool sprite_or_image => texture == null;

        Color color1, color2;

        private Action<Player> refillUse;

        private float respawnTimer;
        private float respawnTime;

        private float alpha;
        private int depth;
        // -1 = default behavior, 0 = not one use, 1 = one use
        private int oneUse;
        public WrapperRefillWall(EntityData e, Vector2 v) : base(e.Position + v) {
            Collider = new Hitbox(e.Width, e.Height);
            typeName = e.Attr("TypeName", "Refill");
            spriteVarname = e.Attr("ImageVariableName", "sprite").Trim();
            if (string.IsNullOrWhiteSpace(spriteVarname))
                spriteVarname = "sprite";
            respawnMethodName = e.Attr("RespawnMethodName", "Respawn");
            if (string.IsNullOrWhiteSpace(respawnMethodName))
                respawnMethodName = "Respawn";
            color1 = VivHelper.GetColorWithFix(e, "InnerColor", "innerColor", GetColorParams.None, GetColorParams.None, new Color(32, 128, 32, 255)).Value;
            color2 = VivHelper.GetColorWithFix(e, "OuterColor", "outerColor", GetColorParams.None, GetColorParams.None, new Color(147, 189, 64, 255)).Value;
            alpha = Calc.Clamp(e.Float("Alpha", 1f), 0f, 1f);
            respawnTime = e.Float("RespawnTime", -1f);
            Depth = depth = e.Int("Depth", 100);
            drawWithOutline = e.Bool("textureOutline");
            if (e.Has("oneUse")) {
                string o = e.Values["oneUse"].ToString();
                if (!int.TryParse(o, out oneUse)) {
                    if (!bool.TryParse(o, out bool b)) {
                        oneUse = -1;
                    } else {
                        oneUse = b ? 1 : -1;
                    }
                }
            } else { oneUse = -1; }

        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            bool b = false;
            foreach (PlayerCollider tie in CollideAllByComponent<PlayerCollider>()) {
                if (CheckEntity(tie.Entity.GetType(), tie.Entity, tie)) {
                    b = true;
                    break;
                } else {
                    tie.Entity.GetType();
                }
            }
            if (!b) {
                RemoveSelf();
            }
        }

        public override void Update() {
            base.Update();
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f) {
                    Respawn();
                }
            }

        }

        private void Respawn() {
            if (!Collidable) {
                Collidable = true;
            }
            tiedEntity.Collidable = false;
            dynRefill.Invoke(respawnMethodName, Array.Empty<object>);
            tiedEntity.Collidable = false;
        }

        private bool CheckEntity(Type type, Entity entity, PlayerCollider pc) {
            if (type.ToString() != typeName) {
                Logger.Log("VivHelper","Refill Wall @ " + Position + ": Invalid Type Name: " + type.ToString() + "does not match " + typeName);
                return false;
            }
            if (pc.OnCollide == null) {
                Logger.Log("VivHelper", "Refill Wall @ " + Position + ": Invalid Player Collider");
                return false;
            }
            isRefillSubclass = type.IsSubclassOf(typeof(Refill));
            dynRefill = new DynamicData(isRefillSubclass ? typeof(Refill) : type, entity);
            object o = null;
            if (spriteVarname.StartsWith("$")) {
                switch (spriteVarname.ToLowerInvariant()) {
                    case "$render":
                        o = null;
                        forceRender = true;
                        break;
                    case "$sprite":
                        o = entity.Get<Sprite>();
                        break;
                    default:
                        o = entity.Get<Image>();
                        break;
                }
            }
            else o = dynRefill.Get(spriteVarname);
            if (!forceRender) {
                if (o == null) {
                    o = entity.Get<Image>();
                    if (o == null) {
                        return false;
                    }
                    texture = o as Image;
                } else if ((o as Sprite) != null) {
                    texture = new Image((o as Sprite).Animations["idle"].Frames[0]);
                } else if ((o as Image) != null) {
                    texture = (o as Image);
                } else if ((o as MTexture) != null) {
                    texture = new Image(o as MTexture);
                } else { return false; }
                texture.CenterOrigin();
                texture.RenderPosition = Center;
            }
            if (dynRefill.Get("respawnTimer") != null) {
                respawnTimerName = "respawnTimer";
            } else if (dynRefill.Get("_respawnTimeRemaining") != null) {
                respawnTimerName = "_respawnTimeRemaining";
            } else
                return false;
            //Everything is good so we continue with making the wall work

            if (dynRefill.Get("oneUse") != null && oneUse > -1)
                dynRefill.Set("oneUse", oneUse == 1);
            else if (dynRefill.Get("OneUse") != null && oneUse > -1)
                dynRefill.Set("OneUse", oneUse == 1);
            tiedEntity = entity;
            tiedEntity.Position = Center;
            tiedEntity.Collidable = tiedEntity.Visible = false;
            refillUse = pc.OnCollide;
            Add(new PlayerCollider(OnPlayer));
            return true;

        }

        private void OnPlayer(Player player) {
            tiedEntity.Collidable = true;
            refillUse(player);
            if (tiedEntity.Collidable) {
                tiedEntity.Collidable = false;
            } else {
                Collidable = false;
                respawnTimer = respawnTime > 0 ? respawnTime : dynRefill.Get<float>(respawnTimerName);
                dynRefill.Set(respawnTimerName, 0f);
            }
            if (dynRefill.Get("oneUse") is { } b && (bool)b) {
                RemoveSelf();
            } else if (dynRefill.Get("OneUse") is { } c && (bool) c) {
                RemoveSelf();
            }
        }

        public override void Render() {
            Camera camera = SceneAs<Level>()?.Camera;
            if (base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom) {
                return;
            }
            if (respawnTimer > 0) {
                int i;
                for (i = 0; i < Width; i += 8) {
                    Draw.Line(TopLeft + Vector2.UnitX * (i + 2), TopLeft + Vector2.UnitX * (i + 6), color2);
                    Draw.Line(BottomLeft + Vector2.UnitX * (i + 2), BottomLeft + Vector2.UnitX * (i + 6), color2);
                }
                for (i = 0; i < Height; i += 8) {
                    Draw.Line(TopLeft + Vector2.UnitY * (i + 2), TopLeft + Vector2.UnitY * (i + 6), color2);
                    Draw.Line(TopRight + Vector2.UnitY * (i + 2), TopRight + Vector2.UnitY * (i + 6), color2);
                }
                if (texture != null) texture.Color = Color.White * 0.25f;
            } else {
                Draw.HollowRect(X - 1, Y - 1, Width + 2, Height + 2, color2);
                Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, color1);
                if (texture != null) texture.Color = Color.White;
            }
            if (forceRender) {
                tiedEntity.Render();
            } else {
                if (drawWithOutline)
                    texture.DrawOutline();
                texture.Render();
            }

        }
    }
}
