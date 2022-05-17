using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Reflection;
using System.Collections;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/SeekerGate = Load")]
    public class SeekerGate : Solid {

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new SeekerGate(entityData, offset, levelData.Name);

        public string LevelID;

        private int closedHeight;

        private Sprite sprite;

        private Shaker shaker;

        private float dist1, dist2;

        private float distance => open ? dist2 : dist1;

        private float drawHeight;

        private float drawHeightMoveSpeed;

        private bool open, lockState;

        public SeekerGate(Vector2 position, int height, string levelID, float r1, float r2, string directory)
            : base(position, 8f, height, safe: true) {
            closedHeight = height;
            LevelID = levelID;
            sprite = new Sprite(GFX.Game, directory);
            //sprite.JustifyOrigin(0.5f, 0f);
            sprite.Add("open", "", 0.1f, "idle", 6, 7, 8, 9, 10, 11, 12, 13, 14);
            sprite.Add("hit", "", 0.08f, "idle", 0, 1, 2, 3, 4);
            sprite.AddLoop("idle", "", 0.1f, 0);
            Add(sprite);
            sprite.Position = new Vector2(Width / 2f, 0f);
            sprite.JustifyOrigin(0.5f, 0f);
            dist1 = r1;
            dist2 = r2;
            sprite.Play("idle");
            Add(shaker = new Shaker(on: false));
            base.Depth = -9000;
        }

        public SeekerGate(EntityData data, Vector2 offset, string levelID)
            : this(data.Position + offset, 48, levelID, data.Float("OpenRadius", 64f), data.Float("CloseRadius", 80f), data.Attr("Directory", "VivHelper/seekergate/seekerdoor")) {
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            drawHeight = Math.Max(4f, base.Height);
            open = lockState = false;
        }

        public void Open() {
            Audio.Play("event:/game/05_mirror_temple/gate_theo_open", Position);
            drawHeightMoveSpeed = 200f;
            drawHeight = base.Height;
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(0);
            sprite.Play("open");
            open = true;
        }

        public void Close() {
            Audio.Play("event:/game/05_mirror_temple/gate_theo_close", Position);
            drawHeightMoveSpeed = 300f;
            drawHeight = Math.Max(4f, base.Height);
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(closedHeight);
            sprite.Play("hit");
            open = false;
        }

        private void SetHeight(int height) {
            if ((float) height < base.Collider.Height) {
                base.Collider.Height = height;
                return;
            }
            float y = base.Y;
            int num = (int) base.Collider.Height;
            if (base.Collider.Height < 64f) {
                base.Y -= 64f - base.Collider.Height;
                base.Collider.Height = 64f;
            }
            MoveVExact(height - num);
            base.Y = y;
            base.Collider.Height = height;
        }

        public override void Update() {
            base.Update();
            float num = Math.Max(4f, base.Collider.Height);
            if (!lockState && sprite.CurrentAnimationID == "idle") {
                if (open && !SeekerIsNearby(distance)) {
                    Close();
                    CollideFirst<Player>(Position)?.Die(Vector2.Zero);
                } else if (!open && SeekerIsNearby(distance)) {
                    Open();
                }
            }
            if (drawHeight != num) {
                lockState = true;
                drawHeight = Calc.Approach(drawHeight, num, drawHeightMoveSpeed * Engine.DeltaTime);
            } else {
                lockState = false;
            }
        }

        public override void Render() {
            Vector2 value = new Vector2(Math.Sign(shaker.Value.X), 0f);
            Draw.Rect(base.X - 2f, base.Y - 8f, 14f, 10f, Color.Black);
            sprite.DrawSubrect(Vector2.Zero + value, new Rectangle(0, (int) (sprite.Height - drawHeight), (int) sprite.Width, (int) drawHeight));
        }

        private bool SeekerIsNearby(float d) {
            bool b = false;
            foreach (Actor a in Scene.Tracker.GetEntities<Actor>().Where((Entity a) => a is Seeker || a is CustomSeeker)) {
                b |= Vector2.DistanceSquared(a.Center, base.Center) < d * d;
                if (b)
                    break;
            }
            return b;
        }
    }
}
