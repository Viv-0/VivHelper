using System;
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
    [CustomEntity("VivHelper/CBDNCrackedBlock")]
    public class CBDNTempleCrackedBlock : Solid {
        private EntityID eid;

        private bool persistent;

        private MTexture[,,] tiles;

        private float frame;

        private bool broken;

        private int frames;

        public CBDNTempleCrackedBlock(EntityID eid, Vector2 position, float width, float height, bool persistent)
            : base(position, width, height, safe: true) {
            this.eid = eid;
            this.persistent = persistent;
            Collidable = (Visible = false);
            int num = (int) (width / 8f);
            int num2 = (int) (height / 8f);
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("VivHelper/CBDNTempleCrackedBlock/breakBlock");
            tiles = new MTexture[num, num2, atlasSubtextures.Count];
            frames = atlasSubtextures.Count;
            for (int i = 0; i < num; i++) {
                for (int j = 0; j < num2; j++) {
                    int num3 = ((i < num / 2 && i < 2) ? i : ((i < num / 2 || i < num - 2) ? (2 + i % 2) : (5 - (num - i - 1))));
                    int num4 = ((j < num2 / 2 && j < 2) ? j : ((j < num2 / 2 || j < num2 - 2) ? (2 + j % 2) : (5 - (num2 - j - 1))));
                    for (int k = 0; k < atlasSubtextures.Count; k++) {
                        tiles[i, j, k] = atlasSubtextures[k].GetSubtexture(num3 * 8, num4 * 8, 8, 8);
                    }
                }
            }
            Add(new LightOcclude(0.5f));
        }

        public CBDNTempleCrackedBlock(EntityData data, Vector2 offset, EntityID eid)
            : this(eid, data.Position + offset, data.Width, data.Height, data.Bool("persistent")) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (CollideCheck<Player>()) {
                if (persistent) {
                    SceneAs<Level>().Session.DoNotLoad.Add(eid);
                }
                RemoveSelf();
            } else {
                Collidable = (Visible = true);
            }
        }

        public override void Update() {
            base.Update();
            if (broken) {
                frame += Engine.DeltaTime * 15f;
                if (frame >= (float) frames) {
                    RemoveSelf();
                }
            }
        }

        public override void Render() {
            int num = (int) frame;
            if (num >= frames) {
                return;
            }
            for (int i = 0; (float) i < base.Width / 8f; i++) {
                for (int j = 0; (float) j < base.Height / 8f; j++) {
                    tiles[i, j, num].Draw(Position + new Vector2(i, j) * 8f);
                }
            }
        }

        public void Break(Vector2 from) {
            if (persistent) {
                SceneAs<Level>().Session.DoNotLoad.Add(eid);
            }
            Audio.Play("event:/game/05_mirror_temple/crackedwall_vanish", base.Center);
            broken = true;
            Collidable = false;
            for (int i = 0; (float) i < base.Width / 8f; i++) {
                for (int j = 0; (float) j < base.Height / 8f; j++) {
                    base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(i * 8 + 4, j * 8 + 4), '1', playSound: true).BlastFrom(from));
                }
            }
        }
    }
}
