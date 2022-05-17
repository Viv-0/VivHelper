using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Celeste.Mod.VivHelper;

namespace VivHelper.PartOfMe {
    [CustomEntity("VivHelper/VariantChangingMirror")]
    public class VariantChangingMirror : ResortMirror {
        private MTexture newGlassBG;
        public bool MaddyBaddy;
        protected bool onlyOnce;
        private bool Disabled, PrevDisabled;
        private DynData<ResortMirror> val;

        public VariantChangingMirror(EntityData data, Vector2 offset)
            : base(data, offset) {
            MaddyBaddy = data.Bool("BadelineMirror", false);
            onlyOnce = data.Bool("OnlyOnce", false);
            newGlassBG = GFX.Game[MaddyBaddy ? "VivHelper/MaddyBaddyMirror/purpMirror00" : "VivHelper/MaddyBaddyMirror/redMirror00"];
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            val = new DynData<ResortMirror>((ResortMirror) this);
            Remove(val.Get<Image>("bg"));
            Image image = val.Get<Image>("frame");
            int num = (int) image.Width - 2;
            int num2 = (int) image.Height - 12;
            val.Set<Image>("bg", new Image(newGlassBG.GetSubtexture((newGlassBG.Width - num) / 2, newGlassBG.Height - num2, num, num2)));
            Add((Image) val.Get<Image>("bg"));
            val.Get<Image>("bg").JustifyOrigin(0.5f, 1f);
            Add(new PlayerCollider(OnEnter, new Hitbox(28, 32, -14, -34)));
        }

        public void OnEnter(Player player) {
            if (MaddyBaddy && !SaveData.Instance.Assists.PlayAsBadeline && !Disabled) { SaveData.Instance.Assists.PlayAsBadeline = true; Pt2(player); }
            if (!MaddyBaddy && SaveData.Instance.Assists.PlayAsBadeline && !Disabled) { SaveData.Instance.Assists.PlayAsBadeline = false; Pt2(player); }
        }

        private void Pt2(Player player) {
            if (player != null) {
                PlayerSpriteMode mode = SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode;
                if (player.Active) {
                    player.ResetSpriteNextFrame(mode);
                } else {
                    player.ResetSprite(mode);
                }
            }
            if (onlyOnce) { Remove(Get<PlayerCollider>()); }
        }

        public override void Update() {
            base.Update();
            if (Disabled ^ PrevDisabled) {
                Remove(val.Get<Image>("bg"));
                Image image = val.Get<Image>("frame");
                int num = (int) image.Width - 2;
                int num2 = (int) image.Height - 12;
                MTexture glass;
                if (Disabled) { glass = GFX.Game["VivHelper/MaddyBaddyMirror/grayMirror00"]; } else { glass = newGlassBG; }
                val.Set<Image>("bg", new Image(glass.GetSubtexture((glass.Width - num) / 2, glass.Height - num2, num, num2)));
                Add((Image) val.Get<Image>("bg"));
                val.Get<Image>("bg").JustifyOrigin(0.5f, 1f);
                PrevDisabled = Disabled;
            }

            Disabled = (base.Scene as Level).Session.GetFlag("PoM/mirror_disabled");
        }
    }

    [CustomEntity("VivHelper/CutscenelessDreamMirror")]
    public class CutscenelessDreamMirror : Entity {
        public enum ReflectionTypes {
            MaddyOnly,
            BaddyOnly,
            InversePlayer,
            SameAsPlayer,
            None
        }
        public static ParticleType P_Shatter = DreamMirror.P_Shatter;

        private Image frame;

        private MTexture glassbg = GFX.Game["objects/mirror/glassbg"];

        private MTexture glassfg = GFX.Game["objects/mirror/glassfg"];

        private Sprite breakingGlass;

        private VirtualRenderTarget mirror;

        private float shineAlpha = 0.5f;

        private float shineOffset;

        private Entity reflection;

        private PlayerSprite reflectionSprite;

        private PlayerHair reflectionHair;

        private float reflectionAlpha = 0.7f;

        private bool autoUpdateReflection = true;

        private bool smashed;

        private bool smashEnded;

        private bool updateShine = true;

        public ReflectionTypes reflectionType;

        public CutscenelessDreamMirror(Vector2 position, string Glass, string Frame, bool broken, ReflectionTypes rt)
            : base(position) {
            base.Depth = 9500;
            reflectionType = rt;
            if (Glass == "default") { breakingGlass = GFX.SpriteBank.Create("glass"); } else { breakingGlass = VivHelperModule.spriteBank.Create(Glass); string t = broken ? "01" : "00"; glassbg = GFX.Game["VivHelper/MaddyBaddyMirror/" + Glass + t]; }
            Add(breakingGlass);
            breakingGlass.Play("idle");
            smashed = broken;

            Add(new BeforeRenderHook(BeforeRender));
            foreach (MTexture shard in GFX.Game.GetAtlasSubtextures("objects/mirror/mirrormask")) {
                MirrorSurface surface = new MirrorSurface();
                surface.OnRender = delegate {
                    shard.DrawJustified(Position, new Vector2(0.5f, 1f), surface.ReflectionColor * (smashEnded ? 1 : 0));
                };
                surface.ReflectionOffset = new Vector2(9 + Calc.Random.Range(-4, 4), 4 + Calc.Random.Range(-2, 2));
                Add(surface);
            }
            frame = Frame == "default" ? new Image(GFX.Game["objects/mirror/frame"]) : new Image(GFX.Game["VivHelper/MaddyBaddyMirror/dream_frame_" + Frame]);
        }

        public CutscenelessDreamMirror(EntityData data, Vector2 offset) :
            this(data.Position + offset, data.Attr("GlassType", "default"), data.Attr("FrameType", "default"), data.Bool("Broken", true), data.Enum<ReflectionTypes>("Reflection", ReflectionTypes.InversePlayer)) { }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (smashed) {
                breakingGlass.Play("broken");
                smashEnded = true;
            } else if (reflectionType != ReflectionTypes.None) {
                reflection = new Entity();
                reflectionSprite = new PlayerSprite((reflectionType == ReflectionTypes.BaddyOnly || reflectionType == ReflectionTypes.InversePlayer) ? PlayerSpriteMode.Badeline : PlayerSpriteMode.Madeline);
                reflectionHair = new PlayerHair(reflectionSprite);
                reflectionHair.Color = (reflectionType == ReflectionTypes.BaddyOnly || reflectionType == ReflectionTypes.InversePlayer) ? BadelineOldsite.HairColor : Player.NormalHairColor;
                reflectionHair.Border = Color.Black;
                reflection.Add(reflectionHair);
                reflection.Add(reflectionSprite);
                reflectionHair.Start();
                reflectionSprite.OnFrameChange = delegate (string anim) {
                    if (!smashed && CollideCheck<Player>()) {
                        int currentAnimationFrame = reflectionSprite.CurrentAnimationFrame;
                        if ((anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runSlow" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runFast" && (currentAnimationFrame == 0 || currentAnimationFrame == 6))) {
                            Audio.Play("event:/char/badeline/footstep", base.Center);
                        }
                    }
                };
            }
            Entity entity = new Entity(Position);
            entity.Depth = 9000;
            entity.Add(frame);
            frame.JustifyOrigin(0.5f, 1f);
            base.Scene.Add(entity);
            Collidable = false;
        }

        public override void Update() {
            base.Update();
            if (reflection != null) {
                reflection.Update();
                if (reflectionSprite.Mode == PlayerSpriteMode.Badeline &&
                        ((reflectionType == ReflectionTypes.InversePlayer && SaveData.Instance.Assists.PlayAsBadeline) ||
                        (reflectionType == ReflectionTypes.SameAsPlayer && !SaveData.Instance.Assists.PlayAsBadeline))) {
                    reflection.Remove(reflectionSprite);
                    reflection.Remove(reflectionHair);
                    reflectionSprite = new PlayerSprite(PlayerSpriteMode.Madeline);
                    reflectionHair = new PlayerHair(reflectionSprite);
                    reflectionHair.Color = Player.NormalHairColor;
                    reflection.Add(reflectionHair);
                    reflection.Add(reflectionSprite);
                    reflectionHair.Start();
                }
                if (reflectionSprite.Mode == PlayerSpriteMode.Madeline &&
                        ((reflectionType == ReflectionTypes.InversePlayer && !SaveData.Instance.Assists.PlayAsBadeline) ||
                        (reflectionType == ReflectionTypes.SameAsPlayer && SaveData.Instance.Assists.PlayAsBadeline))) {
                    reflection.Remove(reflectionSprite);
                    reflection.Remove(reflectionHair);
                    reflectionSprite = new PlayerSprite(PlayerSpriteMode.Badeline);
                    reflectionHair = new PlayerHair(reflectionSprite);
                    reflectionHair.Color = BadelineOldsite.HairColor;
                    reflection.Add(reflectionHair);
                    reflection.Add(reflectionSprite);
                    reflectionHair.Start();
                }
                reflectionHair.Facing = (Facings) Math.Sign(reflectionSprite.Scale.X);
                reflectionHair.AfterUpdate();
            }
        }

        private void BeforeRender() {
            if (smashed) {
                return;
            }
            Level level = base.Scene as Level;
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity == null) {
                return;
            }
            if (autoUpdateReflection && reflection != null) {
                reflection.Position = new Vector2(base.X - entity.X, entity.Y - base.Y) + breakingGlass.Origin;
                reflectionSprite.Scale.X = (float) (0 - entity.Facing) * Math.Abs(entity.Sprite.Scale.X);
                reflectionSprite.Scale.Y = entity.Sprite.Scale.Y;
                if (reflectionSprite.CurrentAnimationID != entity.Sprite.CurrentAnimationID && entity.Sprite.CurrentAnimationID != null && reflectionSprite.Has(entity.Sprite.CurrentAnimationID)) {
                    reflectionSprite.Play(entity.Sprite.CurrentAnimationID);
                }
            }
            if (mirror == null) {
                mirror = VirtualContent.CreateRenderTarget("dream-mirror", glassbg.Width, glassbg.Height);
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(mirror);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            if (updateShine) {
                shineOffset = glassfg.Height - (int) (level.Camera.Y * 0.8f % (float) glassfg.Height);
            }
            glassbg.Draw(Vector2.Zero, Vector2.Zero, Calc.HexToColor("d0d0d0"));
            if (reflection != null) {
                reflection.Render();
            }
            glassfg.Draw(new Vector2(0f, shineOffset), Vector2.Zero, Color.White * shineAlpha);
            glassfg.Draw(new Vector2(0f, shineOffset - (float) glassfg.Height), Vector2.Zero, Color.White * shineAlpha);
            Draw.SpriteBatch.End();
        }

        public override void Render() {
            if (smashed) {
                breakingGlass.Render();
            } else {
                Draw.SpriteBatch.Draw(mirror.Target, Position - breakingGlass.Origin, Color.White * reflectionAlpha);
            }
            frame.Render();
        }
    }

    /*
	[CustomEntity("VivHelper/WarpMirror")]
	public class WarpDreamMirror : Entity
	{
		public static ParticleType P_Shatter;
		private Image frame;
		private MTexture glassbg = GFX.Game["objects/mirror/glassbg"];
		private MTexture glassfg = GFX.Game["objects/mirror/glassfg"];
		private Sprite breakingGlass;
		private Hitbox hitbox;
		private VirtualRenderTarget mirror;
		private float shineAlpha = 0.5f;
		private float shineOffset;
		private Entity reflection;
		private PlayerSprite reflectionSprite;
		private PlayerHair reflectionHair;
		private float reflectionAlpha = 0.7f;
		private bool autoUpdateReflection = true;
		private BadelineDummy badeline;
		private bool smashed;
		private bool smashEnded;
		private bool updateShine = true;
		private Coroutine smashCoroutine;
		private SoundSource sfx;
		private SoundSource sfxSting;

		public enum charReflected
		{
			Player,
			InversePlayer,
			Madeline,
			Badeline,
			Shadow
		}
		public enum MirrorType
        {
			AlwaysShattered,
			NeverShattered,
			Cutscene,
			ShatterOnEntry,
			FlashShatter
        }
		public bool Cutscene = false;
		public Vector2 WarpMirror = default(Vector2);
		public MirrorType mirrorType = MirrorType.AlwaysShattered;
		public charReflected charReflect = charReflected.Player;

		public WarpDreamMirror(Vector2 position)
		: base(position)
		{
			base.Depth = 9500;
			breakingGlass = GFX.SpriteBank.Create("glass");
			Add(breakingGlass);
			breakingGlass.Play("idle");
			Add(new BeforeRenderHook(BeforeRender));
			foreach (MTexture shard in GFX.Game.GetAtlasSubtextures("objects/mirror/mirrormask"))
			{
				MirrorSurface surface = new MirrorSurface();
				surface.OnRender = delegate
				{
					shard.DrawJustified(Position, new Vector2(0.5f, 1f), surface.ReflectionColor * (smashEnded ? 1 : 0));
				};
				surface.ReflectionOffset = new Vector2(9 + Calc.Random.Range(-4, 4), 4 + Calc.Random.Range(-2, 2));
				Add(surface);
			}
		}

		public WarpDreamMirror(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
			mirrorType = data.Enum<MirrorType>("MirrorType", MirrorType.AlwaysShattered);
			charReflect = data.Enum<charReflected>("Character Reflection", charReflected.InversePlayer);
			shineAlpha = data.Float("ShineTransparency", 0.5f);
			reflectionAlpha = data.Float("ReflectionTransparency", 0.7f);

        }

		public override void Added(Scene scene)
		{
			base.Added(scene);
			smashed = SceneAs<Level>().Session.Inventory.DreamDash || mirrorType == MirrorType.AlwaysShattered;
			if (smashed)
			{
				breakingGlass.Play("broken");
				smashEnded = true;
			}
			else
			{
				reflection = new Entity();
				reflectionSprite = new PlayerSprite(PlayerSpriteMode.Badeline);
				reflectionHair = new PlayerHair(reflectionSprite);
				reflectionHair.Color = BadelineOldsite.HairColor;
				reflectionHair.Border = Color.Black;
				reflection.Add(reflectionHair);
				reflection.Add(reflectionSprite);
				reflectionHair.Start();
				reflectionSprite.OnFrameChange = delegate (string anim)
				{
					if (!smashed && CollideCheck<Player>())
					{
						int currentAnimationFrame = reflectionSprite.CurrentAnimationFrame;
						if ((anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runSlow" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "runFast" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)))
						{
							Audio.Play("event:/char/badeline/footstep", base.Center);
						}
					}
				};
				if (mirrorType == MirrorType.Cutscene)
				{
					Add(smashCoroutine = new Coroutine(InteractRoutine()));
				}
			}
			Entity entity = new Entity(Position);
			entity.Depth = 9000;
			entity.Add(frame = new Image(GFX.Game["objects/mirror/frame"]));
			frame.JustifyOrigin(0.5f, 1f);
			base.Scene.Add(entity);
			base.Collider = (hitbox = new Hitbox((int)frame.Width - 16, (int)frame.Height + 32, -(int)frame.Width / 2 + 8, -(int)frame.Height - 32));
		}

		public override void Update()
		{
			base.Update();
			if (reflection != null)
			{
				reflection.Update();
				reflectionHair.Facing = (Facings)Math.Sign(reflectionSprite.Scale.X);
				reflectionHair.AfterUpdate();
			}
		}

		private void BeforeRender()
		{
			if (smashed)
			{
				return;
			}
			Level level = base.Scene as Level;
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			if (entity == null)
			{
				return;
			}
			if (autoUpdateReflection && reflection != null)
			{
				reflection.Position = new Vector2(base.X - entity.X, entity.Y - base.Y) + breakingGlass.Origin;
				reflectionSprite.Scale.X = (float)(0 - entity.Facing) * Math.Abs(entity.Sprite.Scale.X);
				reflectionSprite.Scale.Y = entity.Sprite.Scale.Y;
				if (reflectionSprite.CurrentAnimationID != entity.Sprite.CurrentAnimationID && entity.Sprite.CurrentAnimationID != null && reflectionSprite.Has(entity.Sprite.CurrentAnimationID))
				{
					reflectionSprite.Play(entity.Sprite.CurrentAnimationID);
				}
			}
			if (mirror == null)
			{
				mirror = VirtualContent.CreateRenderTarget("dream-mirror", glassbg.Width, glassbg.Height);
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(mirror);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
			if (updateShine)
			{
				shineOffset = glassfg.Height - (int)(level.Camera.Y * 0.8f % (float)glassfg.Height);
			}
			glassbg.Draw(Vector2.Zero);
			if (reflection != null)
			{
				reflection.Render();
			}
			glassfg.Draw(new Vector2(0f, shineOffset), Vector2.Zero, Color.White * shineAlpha);
			glassfg.Draw(new Vector2(0f, shineOffset - (float)glassfg.Height), Vector2.Zero, Color.White * shineAlpha);
			Draw.SpriteBatch.End();
		}

		private IEnumerator InteractRoutine()
		{
			Player player = null;
			while (player == null)
			{
				player = base.Scene.Tracker.GetEntity<Player>();
				yield return null;
			}
			while (!hitbox.Collide(player))
			{
				yield return null;
			}
			hitbox.Width += 32f;
			hitbox.Position.X -= 16f;
			Audio.SetMusic(null);
			while (hitbox.Collide(player))
			{
				yield return null;
			}
			base.Scene.Add(new Custom_CS02_Mirror(player, this, false));
		}

		public IEnumerator BreakRoutine(int direction)
		{
			autoUpdateReflection = false;
			reflectionSprite.Play("runFast");
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
			while (Math.Abs(reflection.X - breakingGlass.Width / 2f) > 3f)
			{
				reflection.X += (float)(direction * 32) * Engine.DeltaTime;
				yield return null;
			}
			reflectionSprite.Play("idle");
			yield return 0.65f;
			Add(sfx = new SoundSource());
			sfx.Play("event:/game/02_old_site/sequence_mirror");
			yield return 0.15f;
			Add(sfxSting = new SoundSource("event:/music/lvl2/dreamblock_sting_pt2"));
			Input.Rumble(RumbleStrength.Light, RumbleLength.FullSecond);
			updateShine = false;
			while (shineOffset != 33f || shineAlpha < 1f)
			{
				shineOffset = Calc.Approach(shineOffset, 33f, Engine.DeltaTime * 120f);
				shineAlpha = Calc.Approach(shineAlpha, 1f, Engine.DeltaTime * 4f);
				yield return null;
			}
			smashed = true;
			breakingGlass.Play("break");
			yield return 0.6f;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			(base.Scene as Level).Shake();
			for (float num = (0f - breakingGlass.Width) / 2f; num < breakingGlass.Width / 2f; num += 8f)
			{
				for (float num2 = 0f - breakingGlass.Height; num2 < 0f; num2 += 8f)
				{
					if (Calc.Random.Chance(0.5f))
					{
						(base.Scene as Level).Particles.Emit(P_Shatter, 2, Position + new Vector2(num + 4f, num2 + 4f), new Vector2(8f, 8f), new Vector2(num, num2).Angle());
					}
				}
			}
			smashEnded = true;
			badeline = new BadelineDummy(reflection.Position + Position - breakingGlass.Origin);
			badeline.Floatness = 0f;
			for (int i = 0; i < badeline.Hair.Nodes.Count; i++)
			{
				badeline.Hair.Nodes[i] = reflectionHair.Nodes[i];
			}
			base.Scene.Add(badeline);
			badeline.Sprite.Play("idle");
			badeline.Sprite.Scale = reflectionSprite.Scale;
			reflection = null;
			yield return 1.2f;
			float speed = (float)(-direction) * 32f;
			badeline.Sprite.Scale.X = -direction;
			badeline.Sprite.Play("runFast");
			while (Math.Abs(badeline.X - base.X) < 60f)
			{
				speed += Engine.DeltaTime * (float)(-direction) * 128f;
				badeline.X += speed * Engine.DeltaTime;
				yield return null;
			}
			badeline.Sprite.Play("jumpFast");
			while (Math.Abs(badeline.X - base.X) < 128f)
			{
				speed += Engine.DeltaTime * (float)(-direction) * 128f;
				badeline.X += speed * Engine.DeltaTime;
				badeline.Y -= Math.Abs(speed) * Engine.DeltaTime * 0.8f;
				yield return null;
			}
			badeline.RemoveSelf();
			badeline = null;
			yield return 1.5f;
		}

		public void Broken(bool wasSkipped)
		{
			updateShine = false;
			smashed = true;
			smashEnded = true;
			breakingGlass.Play("broken");
			if (wasSkipped && badeline != null)
			{
				badeline.RemoveSelf();
			}
			if (wasSkipped && sfx != null)
			{
				sfx.Stop();
			}
			if (wasSkipped && sfxSting != null)
			{
				sfxSting.Stop();
			}
		}

		public override void Render()
		{
			if (smashed)
			{
				breakingGlass.Render();
			}
			else
			{
				Draw.SpriteBatch.Draw(mirror.Target, Position - breakingGlass.Origin, Color.White * reflectionAlpha);
			}
			frame.Render();
		}

		public override void SceneEnd(Scene scene)
		{
			Dispose();
			base.SceneEnd(scene);
		}

		public override void Removed(Scene scene)
		{
			Dispose();
			base.Removed(scene);
		}

		private void Dispose()
		{
			if (mirror != null)
			{
				mirror.Dispose();
			}
			mirror = null;
		}
	}
	*/
}

