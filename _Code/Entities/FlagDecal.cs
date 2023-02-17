using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace VivHelper.Entities {
    public class CustomDecal : Entity {
        private class Banner : Component {
            public float WaveSpeed;

            public float WaveAmplitude;

            public int SliceSize;

            public float SliceSinIncrement;

            public bool EaseDown;

            public float Offset;

            public bool OnlyIfWindy;

            public float WindMultiplier = 1f;

            private float sineTimer = Calc.Random.NextFloat();

            public List<List<MTexture>> Segments;

            public CustomDecal Decal => (CustomDecal) base.Entity;

            public Banner()
                : base(active: true, visible: true) {
            }

            public override void Update() {
                if (OnlyIfWindy) {
                    float x = (base.Scene as Level).Wind.X;
                    float target = Math.Min(3f, Math.Abs(x) * 0.004f);
                    WindMultiplier = Calc.Approach(WindMultiplier, target, Engine.DeltaTime * 4f);
                    if (x != 0f) {
                        Offset = (float) Math.Sign(x) * Math.Abs(Offset);
                    }
                }
                sineTimer += Engine.DeltaTime * WindMultiplier;
                base.Update();
            }

            public override void Render() {
                MTexture mTexture = Decal.textures[(int) Decal.frame];
                List<MTexture> list = Segments[(int) Decal.frame];
                for (int i = 0; i < list.Count; i++) {
                    float num = (EaseDown ? ((float) i / (float) list.Count) : (1f - (float) i / (float) list.Count)) * WindMultiplier;
                    float x = (float) (Math.Sin(sineTimer * WaveSpeed + (float) i * SliceSinIncrement) * (double) num * (double) WaveAmplitude + (double) (num * Offset));
                    list[i].Draw(Decal.Position + new Vector2(x, 0f), new Vector2(mTexture.Width / 2, mTexture.Height / 2 - i * SliceSize), Color.White, Decal.scale);
                }
            }
        }

        private class DecalImage : Component {
            public CustomDecal Decal => (CustomDecal) base.Entity;

            public DecalImage()
                : base(active: true, visible: true) {
            }

            public override void Render() {
                Decal.textures[(int) Decal.frame].DrawCentered(Decal.Position, Color.White, Decal.scale);
            }
        }

        private class FinalFlagDecalImage : Component {
            public float Rotation;

            public CustomDecal Decal => (CustomDecal) base.Entity;

            public FinalFlagDecalImage()
                : base(active: true, visible: true) {
            }

            public override void Render() {
                MTexture mTexture = Decal.textures[(int) Decal.frame];
                mTexture.DrawJustified(Decal.Position + Vector2.UnitY * (mTexture.Height / 2), new Vector2(0.5f, 1f), Color.White, Decal.scale, Rotation);
            }
        }

        private class CoreSwapImage : Component {
            private MTexture hot;

            private MTexture cold;

            public CustomDecal Decal => (CustomDecal) base.Entity;

            public CoreSwapImage(MTexture hot, MTexture cold)
                : base(active: false, visible: true) {
                this.hot = hot;
                this.cold = cold;
            }

            public override void Render() {
                (((base.Scene as Level).CoreMode == Session.CoreModes.Cold) ? cold : hot).DrawCentered(Decal.Position, Color.White, Decal.scale);
            }
        }

        public const string Root = "decals";

        public const string MirrorMaskRoot = "mirrormasks";

        public string Name;

        public float AnimationSpeed;

        private Component image;

        public bool IsCrack;

        private List<MTexture> textures;

        private Vector2 scale;

        private float frame;

        private bool animated;

        private bool parallax;

        private float parallaxAmount;

        private bool scaredAnimal;

        private SineWave wave;

        private float hideRange;

        private float showRange;

        public Vector2 Scale {
            get {
                return scale;
            }
            set {
                scale = value;
            }
        }

        public Component Image {
            get {
                return image;
            }
            set {
                image = value;
            }
        }

        public CustomDecal(string texture, Vector2 position, Vector2 scale, int depth) {
            if (string.IsNullOrEmpty(Path.GetExtension(texture))) {
                texture += ".png";
            }
            hideRange = 32f;
            showRange = 48f;
            base.Depth = depth;
            this.scale = scale;
            string extension = Path.GetExtension(texture);
            string input = Path.Combine("decals", texture.Replace(extension, "")).Replace('\\', '/');
            Name = Regex.Replace(input, "\\d+$", string.Empty);
            textures = GFX.Game.GetAtlasSubtextures(Name);
        }

        public override void Added(Scene scene) {
            orig_Added(scene);
            string text = Name.ToLower();
            if (text.StartsWith("decals/")) {
                text = text.Substring(7);
            }
            if (!DecalRegistry.RegisteredDecals.ContainsKey(text)) {
                return;
            }
            Remove(image);
            image = null;
            if (image == null) {
                Add(image = new DecalImage());
            }
        }

        public void MakeBanner(float speed, float amplitude, int sliceSize, float sliceSinIncrement, bool easeDown, float offset = 0f, bool onlyIfWindy = false) {
            Banner banner = new Banner {
                WaveSpeed = speed,
                WaveAmplitude = amplitude,
                SliceSize = sliceSize,
                SliceSinIncrement = sliceSinIncrement,
                Segments = new List<List<MTexture>>(),
                EaseDown = easeDown,
                Offset = offset,
                OnlyIfWindy = onlyIfWindy
            };
            foreach (MTexture texture in textures) {
                List<MTexture> list = new List<MTexture>();
                for (int i = 0; i < texture.Height; i += sliceSize) {
                    list.Add(texture.GetSubtexture(0, i, texture.Width, sliceSize));
                }
                banner.Segments.Add(list);
            }
            Add(image = banner);
        }

        public void MakeFloaty() {
            Add(wave = new SineWave(Calc.Random.Range(0.1f, 0.4f), Calc.Random.NextFloat() * ((float) Math.PI * 2f)));
        }

        public void MakeSolid(float x, float y, float w, float h, int surfaceSoundIndex, bool blockWaterfalls = true) {
            Solid solid = new Solid(Position + new Vector2(x, y), w, h, safe: true);
            solid.BlockWaterfalls = blockWaterfalls;
            solid.SurfaceSoundIndex = surfaceSoundIndex;
            base.Scene.Add(solid);
        }

        public void CreateSmoke(Vector2 offset, bool inbg) {
            Level level = base.Scene as Level;
            ParticleEmitter particleEmitter = new ParticleEmitter(inbg ? level.ParticlesBG : level.ParticlesFG, ParticleTypes.Chimney, offset, new Vector2(4f, 1f), -(float) Math.PI / 2f, 1, 0.2f);
            Add(particleEmitter);
            particleEmitter.SimulateCycle();
        }

        public void MakeMirror(string path, bool keepOffsetsClose) {
            base.Depth = 9500;
            if (keepOffsetsClose) {
                MakeMirror(path, GetMirrorOffset());
                return;
            }
            foreach (MTexture mask in GFX.Game.GetAtlasSubtextures("mirrormasks/" + path)) {
                MirrorSurface surface = new MirrorSurface();
                surface.ReflectionOffset = GetMirrorOffset();
                surface.OnRender = delegate {
                    mask.DrawCentered(Position, surface.ReflectionColor, scale);
                };
                Add(surface);
            }
        }

        private void MakeMirror(string path, Vector2 offset) {
            base.Depth = 9500;
            foreach (MTexture mask in GFX.Game.GetAtlasSubtextures("mirrormasks/" + path)) {
                MirrorSurface surface = new MirrorSurface();
                surface.ReflectionOffset = offset + new Vector2(-2f + Calc.Random.NextFloat(4f), -2f + Calc.Random.NextFloat(4f));
                surface.OnRender = delegate {
                    mask.DrawCentered(Position, surface.ReflectionColor, scale);
                };
                Add(surface);
            }
        }

        private void MakeMirrorSpecialCase(string path, Vector2 offset) {
            base.Depth = 9500;
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("mirrormasks/" + path);
            for (int i = 0; i < atlasSubtextures.Count; i++) {
                Vector2 vector = new Vector2(-2f + Calc.Random.NextFloat(4f), -2f + Calc.Random.NextFloat(4f));
                switch (i) {
                    case 2:
                        vector = new Vector2(4f, 2f);
                        break;
                    case 6:
                        vector = new Vector2(-2f, 0f);
                        break;
                }
                MTexture mask = atlasSubtextures[i];
                MirrorSurface surface = new MirrorSurface();
                surface.ReflectionOffset = offset + vector;
                surface.OnRender = delegate {
                    mask.DrawCentered(Position, surface.ReflectionColor, scale);
                };
                Add(surface);
            }
        }

        private Vector2 GetMirrorOffset() {
            return new Vector2(Calc.Random.Range(5, 14) * Calc.Random.Choose(1, -1), Calc.Random.Range(2, 6) * Calc.Random.Choose(1, -1));
        }

        public void MakeParallax(float amount) {
            parallax = true;
            parallaxAmount = amount;
        }

        private void MakeScaredAnimation() {
            Sprite sprite = (Sprite) (image = new Sprite(null, null));
            sprite.AddLoop("hidden", 0.1f, textures[0]);
            sprite.Add("return", 0.1f, "idle", textures[1]);
            sprite.AddLoop("idle", 0.1f, textures[2], textures[3], textures[4], textures[5], textures[6], textures[7]);
            sprite.Add("hide", 0.1f, "hidden", textures[8], textures[9], textures[10], textures[11], textures[12]);
            sprite.Play("idle", restart: true);
            sprite.Scale = scale;
            sprite.CenterOrigin();
            Add(sprite);
            scaredAnimal = true;
        }

        public override void Update() {
            if (animated && textures.Count > 1) {
                frame += AnimationSpeed * Engine.DeltaTime;
                frame %= textures.Count;
            }
            if (scaredAnimal) {
                Sprite sprite = image as Sprite;
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null) {
                    if (sprite.CurrentAnimationID == "idle" && (entity.Position - Position).Length() < hideRange) {
                        sprite.Play("hide");
                    } else if (sprite.CurrentAnimationID == "hidden" && (entity.Position - Position).Length() > showRange) {
                        sprite.Play("return");
                    }
                }
            }
            base.Update();
        }

        public override void Render() {
            Vector2 position = Position;
            if (parallax) {
                Vector2 vector = (base.Scene as Level).Camera.Position + new Vector2(160f, 90f);
                Vector2 vector2 = (Position - vector) * parallaxAmount;
                Position += vector2;
            }
            if (wave != null) {
                Position.Y += wave.Value * 4f;
            }
            base.Render();
            Position = position;
        }

        public void FinalFlagTrigger() {
            Wiggler component = Wiggler.Create(1f, 4f, delegate (float v) {
                (image as FinalFlagDecalImage).Rotation = (float) Math.PI / 15f * v;
            }, start: true);
            Vector2 position = Position;
            position.X = Calc.Snap(position.X, 8f) - 8f;
            position.Y += 6f;
            base.Scene.Add(new SummitCheckpoint.ConfettiRenderer(position));
            Add(component);
        }

        public void MakeCoreSwap(string coldPath, string hotPath) {
            Add(image = new CoreSwapImage(GFX.Game[coldPath], GFX.Game[hotPath]));
        }

        public void MakeStaticMover(int x, int y, int w, int h, bool jumpThrus = false) {
            StaticMover staticMover = new StaticMover {
                SolidChecker = (Solid s) => s.CollideRect(new Rectangle((int) X + x, (int) Y + y, w, h)),
                OnMove = delegate (Vector2 v) {
                    X += v.X;
                    Y += v.Y;
                },
                OnShake = delegate (Vector2 v) {
                    X += v.X;
                    Y += v.Y;
                }
            };
            if (jumpThrus) {
                staticMover.JumpThruChecker = (JumpThru s) => s.CollideRect(new Rectangle((int) X + x, (int) X + y, w, h));
            }
            Add(staticMover);
        }

        public void MakeScaredAnimation(int hideRange, int showRange, int[] idleFrames, int[] hiddenFrames, int[] showFrames, int[] hideFrames) {
            Sprite sprite = (Sprite) (image = new Sprite(null, null));
            sprite.AddLoop("hidden", 0.1f, hiddenFrames.Select((int i) => textures[i]).ToArray());
            sprite.Add("return", 0.1f, "idle", showFrames.Select((int i) => textures[i]).ToArray());
            sprite.AddLoop("idle", 0.1f, idleFrames.Select((int i) => textures[i]).ToArray());
            sprite.Add("hide", 0.1f, "hidden", hideFrames.Select((int i) => textures[i]).ToArray());
            sprite.Play("idle", restart: true);
            sprite.Scale = scale;
            sprite.CenterOrigin();
            Add(sprite);
            this.hideRange = hideRange;
            this.showRange = showRange;
            scaredAnimal = true;
        }

        public void orig_Added(Scene scene) {
            base.Added(scene);
            string text = Name.ToLower().Replace("decals/", "");
            switch (text) {
                case "generic/grass_a":
                case "generic/grass_b":
                case "generic/grass_c":
                case "generic/grass_d":
                    MakeBanner(2f, 2f, 1, 0.05f, easeDown: false, -2f);
                    break;
                case "0-prologue/house":
                    CreateSmoke(new Vector2(36f, -28f), inbg: true);
                    break;
                case "1-forsakencity/rags":
                case "1-forsakencity/ragsb":
                case "3-resort/curtain_side_a":
                case "3-resort/curtain_side_d":
                    MakeBanner(2f, 3.5f, 2, 0.05f, easeDown: true);
                    break;
                case "3-resort/roofcenter":
                case "3-resort/roofcenter_b":
                case "3-resort/roofcenter_c":
                case "3-resort/roofcenter_d":
                    MakeSolid(-8f, -4f, 16f, 8f, 14);
                    break;
                case "3-resort/roofedge":
                case "3-resort/roofedge_b":
                case "3-resort/roofedge_c":
                case "3-resort/roofedge_d":
                    MakeSolid((!(scale.X < 0f)) ? (-8) : 0, -4f, 8f, 8f, 14);
                    break;
                case "3-resort/bridgecolumntop":
                    MakeSolid(-8f, -8f, 16f, 8f, 8);
                    MakeSolid(-5f, 0f, 10f, 8f, 8);
                    break;
                case "3-resort/bridgecolumn":
                    MakeSolid(-5f, -8f, 10f, 16f, 8);
                    break;
                case "3-resort/vent":
                    CreateSmoke(Vector2.Zero, inbg: false);
                    break;
                case "3-resort/brokenelevator":
                    MakeSolid(-16f, -20f, 32f, 48f, 22);
                    break;
                case "4-cliffside/bridge_a":
                    MakeSolid(-24f, 0f, 48f, 8f, 8, base.Depth != 9000);
                    break;
                case "4-cliffside/flower_a":
                case "4-cliffside/flower_b":
                case "4-cliffside/flower_c":
                case "4-cliffside/flower_d":
                    MakeBanner(2f, 2f, 1, 0.05f, easeDown: false, 2f, onlyIfWindy: true);
                    break;
                case "5-temple/bg_mirror_a":
                case "5-temple/bg_mirror_b":
                case "5-temple/bg_mirror_shard_a":
                case "5-temple/bg_mirror_shard_b":
                case "5-temple/bg_mirror_shard_c":
                case "5-temple/bg_mirror_shard_d":
                case "5-temple/bg_mirror_shard_e":
                case "5-temple/bg_mirror_shard_f":
                case "5-temple/bg_mirror_shard_g":
                case "5-temple/bg_mirror_shard_h":
                case "5-temple/bg_mirror_shard_i":
                case "5-temple/bg_mirror_shard_j":
                case "5-temple/bg_mirror_shard_k":
                case "5-temple/bg_mirror_shard_group_a":
                case "5-temple/bg_mirror_shard_group_a_b":
                case "5-temple/bg_mirror_shard_group_a_c":
                case "5-temple/bg_mirror_shard_group_b":
                case "5-temple/bg_mirror_shard_group_c":
                case "5-temple/bg_mirror_shard_group_d":
                case "5-temple/bg_mirror_shard_group_e":
                    scale.Y = 1f;
                    MakeMirror(text, keepOffsetsClose: false);
                    break;
                case "5-temple/bg_mirror_c":
                case "5-temple/statue_d":
                    MakeMirror(text, keepOffsetsClose: true);
                    break;
                case "5-temple-dark/mosaic_b":
                    Add(new BloomPoint(new Vector2(0f, 5f), 0.75f, 16f));
                    break;
                case "6-reflection/crystal_reflection":
                    MakeMirrorSpecialCase(text, new Vector2(-12f, 2f));
                    break;
                case "7-summit/cloud_a":
                case "7-summit/cloud_b":
                case "7-summit/cloud_bb":
                case "7-summit/cloud_bc":
                case "7-summit/cloud_bd":
                case "7-summit/cloud_c":
                case "7-summit/cloud_cb":
                case "7-summit/cloud_cc":
                case "7-summit/cloud_cd":
                case "7-summit/cloud_ce":
                case "7-summit/cloud_d":
                case "7-summit/cloud_db":
                case "7-summit/cloud_dc":
                case "7-summit/cloud_dd":
                case "7-summit/cloud_e":
                case "7-summit/cloud_f":
                case "7-summit/cloud_g":
                case "7-summit/cloud_h":
                case "7-summit/cloud_j":
                case "7-summit/cloud_i":
                    base.Depth = -13001;
                    MakeParallax(0.1f);
                    scale *= 1.15f;
                    break;
                case "7-summit/summitflag":
                    Add(new SoundSource("event:/env/local/07_summit/flag_flap"));
                    break;
                case "9-core/ball_a":
                    Add(image = new CoreSwapImage(textures[0], GFX.Game["decals/9-core/ball_a_ice"]));
                    break;
                case "9-core/ball_a_ice":
                    Add(image = new CoreSwapImage(GFX.Game["decals/9-core/ball_a"], textures[0]));
                    break;
                case "9-core/rock_e":
                    Add(image = new CoreSwapImage(textures[0], GFX.Game["decals/9-core/rock_e_ice"]));
                    break;
                case "9-core/rock_e_ice":
                    Add(image = new CoreSwapImage(GFX.Game["decals/9-core/rock_e"], textures[0]));
                    break;
                case "9-core/heart_bevel_a":
                case "9-core/heart_bevel_b":
                case "9-core/heart_bevel_c":
                case "9-core/heart_bevel_d":
                    scale.Y = 1f;
                    scale.X = 1f;
                    break;
                case "10-farewell/creature_a":
                case "10-farewell/creature_b":
                case "10-farewell/creature_c":
                case "10-farewell/creature_d":
                case "10-farewell/creature_e":
                case "10-farewell/creature_f":
                    base.Depth = 10001;
                    MakeParallax(-0.1f);
                    MakeFloaty();
                    break;
                case "10-farewell/coral_":
                case "10-farewell/coral_a":
                case "10-farewell/coral_b":
                case "10-farewell/coral_c":
                case "10-farewell/coral_d":
                    MakeScaredAnimation();
                    break;
                case "10-farewell/clouds/cloud_a":
                case "10-farewell/clouds/cloud_b":
                case "10-farewell/clouds/cloud_bb":
                case "10-farewell/clouds/cloud_bc":
                case "10-farewell/clouds/cloud_bd":
                case "10-farewell/clouds/cloud_c":
                case "10-farewell/clouds/cloud_cb":
                case "10-farewell/clouds/cloud_cc":
                case "10-farewell/clouds/cloud_cd":
                case "10-farewell/clouds/cloud_ce":
                case "10-farewell/clouds/cloud_d":
                case "10-farewell/clouds/cloud_db":
                case "10-farewell/clouds/cloud_dc":
                case "10-farewell/clouds/cloud_dd":
                case "10-farewell/clouds/cloud_e":
                case "10-farewell/clouds/cloud_f":
                case "10-farewell/clouds/cloud_g":
                case "10-farewell/clouds/cloud_h":
                case "10-farewell/clouds/cloud_j":
                case "10-farewell/clouds/cloud_i":
                    base.Depth = -13001;
                    MakeParallax(0.1f);
                    scale *= 1.15f;
                    break;
                case "10-farewell/glitch_a_":
                case "10-farewell/glitch_b_":
                case "10-farewell/glitch_c":
                    frame = Calc.Random.NextFloat(textures.Count);
                    break;
                case "10-farewell/cliffside":
                case "10-farewell/car":
                case "10-farewell/bed":
                case "10-farewell/floating house":
                case "10-farewell/heart_a":
                case "10-farewell/heart_b":
                case "10-farewell/reflection":
                case "10-farewell/tower":
                case "10-farewell/temple":
                case "10-farewell/giantcassete":
                    base.Depth = 10001;
                    MakeParallax(-0.15f);
                    MakeFloaty();
                    break;
                case "10-farewell/finalflag":
                    AnimationSpeed = 6f;
                    Add(image = new FinalFlagDecalImage());
                    break;
            }
            if (Name.Contains("crack")) {
                IsCrack = true;
            }
            if (image == null) {
                Add(image = new DecalImage());
            }
        }
    }
}
