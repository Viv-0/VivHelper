using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using MonoMod.Utils;

namespace VivHelper.Entities
{
	public class CustomPoem : Entity
	{
		private struct Particle
		{
			public Vector2 Direction;

			public float Percent;

			public float Duration;

			public void Reset(float percent)
			{
				Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
				Percent = percent;
				Duration = 0.5f + Calc.Random.NextFloat() * 0.5f;
			}
		}

		private const float textScale = 1.5f;

		public float Alpha = 1f;

		public float TextAlpha = 1f;

		public Vector2 Offset;

		public Sprite Heart;

		public float ParticleSpeed = 1f;

		public float Shake;

		private float timer;

		private string text;

		private bool disposed;

		private VirtualRenderTarget poem;

		private VirtualRenderTarget smoke;

		private VirtualRenderTarget temp;

		private Particle[] particles = new Particle[80];

		public Color Color
		{
			get;
			private set;
		}

		public CustomPoem(string text, float heartAlpha, Color heartColor)
		{
			if (text != null)
			{
				this.text = ActiveFont.FontSize.AutoNewline(text, 1024);
			}
			Color = heartColor;
			Heart = GFX.GuiSpriteBank.Create("heartgem3");
			Heart.Play("spin");
			Heart.Position = Celeste.Celeste.TargetCenter;
			Heart.Color = heartColor * heartAlpha;
			int num = Math.Min(1920, Engine.ViewWidth);
			int num2 = Math.Min(1080, Engine.ViewHeight);
			poem = VirtualContent.CreateRenderTarget("poem-a", num, num2);
			smoke = VirtualContent.CreateRenderTarget("poem-b", num / 2, num2 / 2);
			temp = VirtualContent.CreateRenderTarget("poem-c", num / 2, num2 / 2);
			base.Tag = (int)Tags.HUD | (int)Tags.FrozenUpdate;
			Add(new BeforeRenderHook(BeforeRender));
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Reset(Calc.Random.NextFloat());
			}
		}

		public void Dispose()
		{
			if (!disposed)
			{
				poem.Dispose();
				smoke.Dispose();
				temp.Dispose();
				RemoveSelf();
				disposed = true;
			}
		}

		private void DrawPoem(Vector2 offset, Color color)
		{
			MTexture mTexture = GFX.Gui["poemside"];
			float num = ActiveFont.Measure(text).X * 1.5f;
			Vector2 vector = new Vector2(960f, 540f) + offset;
			mTexture.DrawCentered(vector - Vector2.UnitX * (num / 2f + 64f), color);
			ActiveFont.Draw(text, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, color);
			mTexture.DrawCentered(vector + Vector2.UnitX * (num / 2f + 64f), color);
		}

		public override void Update()
		{
			timer += Engine.DeltaTime;
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Percent += Engine.DeltaTime / particles[i].Duration * ParticleSpeed;
				if (particles[i].Percent > 1f)
				{
					particles[i].Reset(0f);
				}
			}
			Heart.Update();
		}

		public void BeforeRender()
		{
			if (!disposed)
			{
				Engine.Graphics.GraphicsDevice.SetRenderTarget(poem);
				Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
				Matrix transformationMatrix = Matrix.CreateScale((float)poem.Width / 1920f);
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformationMatrix);
				Heart.Position = Offset + new Vector2(1920f, 1080f) * 0.5f;
				Heart.Scale = Vector2.One * (1f + Shake * 0.1f);
				MTexture mTexture = OVR.Atlas["snow"];
				for (int i = 0; i < particles.Length; i++)
				{
					Particle particle = particles[i];
					float num = Ease.SineIn(particle.Percent);
					Vector2 position = Heart.Position + particle.Direction * (1f - num) * 1920f;
					float x = 1f + num * 2f;
					float y = 0.25f * (0.25f + (1f - num) * 0.75f);
					float scale = 1f - num;
					mTexture.DrawCentered(position, Color * scale, new Vector2(x, y), (-particle.Direction).Angle());
				}
				Heart.Position += new Vector2(Calc.Random.Range(-1f, 1f), Calc.Random.Range(-1f, 1f)) * 16f * Shake;
				Heart.Render();
				if (!string.IsNullOrEmpty(text))
				{
					DrawPoem(Offset + new Vector2(-2f, 0f), Color.Black * TextAlpha);
					DrawPoem(Offset + new Vector2(2f, 0f), Color.Black * TextAlpha);
					DrawPoem(Offset + new Vector2(0f, -2f), Color.Black * TextAlpha);
					DrawPoem(Offset + new Vector2(0f, 2f), Color.Black * TextAlpha);
					DrawPoem(Offset + Vector2.Zero, Color * TextAlpha);
				}
				Draw.SpriteBatch.End();
				Engine.Graphics.GraphicsDevice.SetRenderTarget(smoke);
				Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
				MagicGlow.Render((RenderTarget2D)poem, timer, -1f, Matrix.CreateScale(0.5f));
				GaussianBlur.Blur((RenderTarget2D)smoke, temp, smoke);
			}
		}

		public override void Render()
		{
			if (!disposed && !base.Scene.Paused)
			{
				float num = 1920f / (float)poem.Width;
				Draw.SpriteBatch.Draw((RenderTarget2D)smoke, Vector2.Zero, smoke.Bounds, Color.White * 0.3f * Alpha, 0f, Vector2.Zero, num * 2f, SpriteEffects.None, 0f);
				Draw.SpriteBatch.Draw((RenderTarget2D)poem, Vector2.Zero, poem.Bounds, Color.White * Alpha, 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			Dispose();
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Dispose();
		}
	}
}
