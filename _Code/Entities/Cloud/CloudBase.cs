﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities.Cloud {
    public class CustomCloud : JumpThru {
        public static ParticleType P_Cloud;

        public static ParticleType P_FragileCloud;

        private Sprite sprite;

        private Wiggler wiggler;

        private ParticleType particleType;

        private SoundSource sfx;

        private bool waiting = true;

        private float speed;

        private float startY;

        private float respawnTimer;

        private bool returning;

        private bool pink;

        private bool fragile;

        private float timer;

        private Vector2 scale;

        private bool canRumble;

        public bool? Small;

        public CustomCloud(Vector2 position, bool fragile, bool small, bool pink)
            : base(position, 32, safe: false) {
            this.fragile = fragile;
            startY = base.Y;
            base.Collider.Position.X = -16f;
            Small = small;
            this.pink = pink;
            timer = Calc.Random.NextFloat() * 4f;
            Add(wiggler = Wiggler.Create(0.3f, 4f));
            particleType = (fragile ? P_FragileCloud : P_Cloud);
            SurfaceSoundIndex = 4;
            Add(new LightOcclude(0.2f));
            scale = Vector2.One;
            Add(sfx = new SoundSource());
        }

        public CustomCloud(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("fragile"), data.Bool("small"), data.Bool("pink")) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            string text = (pink ? "pinkcloud" : "whitecloud");
            if (Small ?? false) {
                base.Collider.Position.X += 2f;
                base.Collider.Width -= 6f;
                text += "Remix";
            }
            Add(sprite = GFX.SpriteBank.Create(text));
            sprite.Origin = new Vector2(sprite.Width / 2f, 8f);
            sprite.OnFrameChange = delegate (string s) {
                if (s == "spawn" && sprite.CurrentAnimationFrame == 6) {
                    wiggler.Start();
                }
            };
        }

        public override void Update() {
            base.Update();
            scale.X = Calc.Approach(scale.X, 1f, 1f * Engine.DeltaTime);
            scale.Y = Calc.Approach(scale.Y, 1f, 1f * Engine.DeltaTime);
            timer += Engine.DeltaTime;
            if (GetPlayerRider() != null) {
                sprite.Position = Vector2.Zero;
            } else {
                sprite.Position = Calc.Approach(sprite.Position, new Vector2(0f, (float) Math.Sin(timer * 2f)), Engine.DeltaTime * 4f);
            }
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f) {
                    waiting = true;
                    base.Y = startY;
                    speed = 0f;
                    scale = Vector2.One;
                    Collidable = true;
                    sprite.Play("spawn");
                    sfx.Play("event:/game/04_cliffside/cloud_pink_reappear");
                }
                return;
            }
            if (waiting) {
                Player playerRider = GetPlayerRider();
                if (playerRider != null && playerRider.Speed.Y >= 0f) {
                    canRumble = true;
                    speed = 180f;
                    scale = new Vector2(1.3f, 0.7f);
                    waiting = false;
                    if (fragile) {
                        Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
                    } else {
                        Audio.Play("event:/game/04_cliffside/cloud_blue_boost", Position);
                    }
                }
                return;
            }
            if (returning) {
                speed = Calc.Approach(speed, 180f, 600f * Engine.DeltaTime);
                MoveTowardsY(startY, speed * Engine.DeltaTime);
                if (base.ExactPosition.Y == startY) {
                    returning = false;
                    waiting = true;
                    speed = 0f;
                }
                return;
            }
            if (fragile && Collidable && !HasPlayerRider()) {
                Collidable = false;
                sprite.Play("fade");
            }
            if (speed < 0f && canRumble) {
                canRumble = false;
                if (HasPlayerRider()) {
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
            }
            if (speed < 0f && base.Scene.OnInterval(0.02f)) {
                (base.Scene as Level).ParticlesBG.Emit(particleType, 1, Position + new Vector2(0f, 2f), new Vector2(base.Collider.Width / 2f, 1f), Consts.PIover2);
            }
            if (fragile && speed < 0f) {
                sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 0f, Engine.DeltaTime * 4f);
            }
            if (base.Y >= startY) {
                speed -= 1200f * Engine.DeltaTime;
            } else {
                speed += 1200f * Engine.DeltaTime;
                if (speed >= -100f) {
                    Player playerRider2 = GetPlayerRider();
                    if (playerRider2 != null && playerRider2.Speed.Y >= 0f) {
                        playerRider2.Speed.Y = -200f;
                    }
                    if (fragile) {
                        Collidable = false;
                        sprite.Play("fade");
                        respawnTimer = 2.5f;
                    } else {
                        scale = new Vector2(0.7f, 1.3f);
                        returning = true;
                    }
                }
            }
            float num = speed;
            if (num < 0f) {
                num = -220f;
            }
            MoveV(speed * Engine.DeltaTime, num);
        }

        public override void Render() {
            Vector2 vector = scale;
            vector *= 1f + 0.1f * wiggler.Value;
            sprite.Scale = vector;
            base.Render();
        }
    }
}
