using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivTestMod.Entities
{
    [CustomEntity("VivHelper/DiagSpike")]
    public class DiagonalSpikes : Entity
    {
		public enum Directions
		{
			UpLeft,
			UpRight,
			DownLeft,
			DownRight
		}

		public static Dictionary<Directions, Vector2> unitDiag = new Dictionary<Directions, Vector2>()
		{   { Directions.DownRight, Vector2.Normalize(Vector2.One) },
			{ Directions.DownLeft, Vector2.Normalize(new Vector2(-1, 1)) },
			{ Directions.UpLeft, Vector2.Normalize(new Vector2(-1, -1))},
			{ Directions.DownRight, Vector2.Normalize(new Vector2(1, -1))} };

		public const string TentacleType = "tentacles";

		public Directions Direction;

		private PlayerCollider pc;

		private Vector2 imageOffset;

		private string overrideType;

		private string spikeType;

		public Color EnabledColor = Color.White;

		public Color DisabledColor = Color.White;

		public bool VisibleWhenDisabled;

		public DiagonalSpikes(Vector2 position, Directions direction, string type)
		: base(position)
		{
			base.Depth = 0;
			Direction = direction;
			overrideType = type;
			switch (direction)
			{
				case Directions.UpLeft:
					base.Collider = new Hitbox(3f, 3f, -3f, -3f);
					Add(new LedgeBlocker());
					break;
				case Directions.UpRight:
					base.Collider = new Hitbox(3f, 3f, 0f, 0f);
					break;
				case Directions.DownLeft:
					base.Collider = new Hitbox(3f, size, -3f);
					Add(new LedgeBlocker());
					break;
				case Directions.DownRight:
					base.Collider = new Hitbox(3f, size);
					Add(new LedgeBlocker());
					break;
			}
			Add(pc = new PlayerCollider(OnCollide));
			Add(new StaticMover
			{
				OnShake = OnShake,
				SolidChecker = IsRiding,
				JumpThruChecker = IsRiding,
				OnEnable = OnEnable,
				OnDisable = OnDisable
			});
		}

		public void SetSpikeColor(Color color)
		{
			foreach (Component component in base.Components)
			{
				Image image = component as Image;
				if (image != null)
				{
					image.Color = color;
				}
			}
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			AreaData areaData = AreaData.Get(scene);
			spikeType = areaData.Spike;
			if (!string.IsNullOrEmpty(overrideType) && !overrideType.Equals("default"))
			{
				spikeType = overrideType;
			}
			string str = Direction.ToString().ToLower();
			if (spikeType == "tentacles")
			{
				AddTentacle(0f);
				return;
			}
			List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("danger/spikes/" + spikeType + "_" + str);
			Image image = new Image(Calc.Random.Choose(atlasSubtextures));
			switch (Direction)
			{
				case Directions.Up:
					image.JustifyOrigin(0.5f, 1f);
					image.Position = Vector2.UnitX * ((float)j + 0.5f) * 8f + Vector2.UnitY;
					break;
				case Directions.Down:
					image.JustifyOrigin(0.5f, 0f);
					image.Position = Vector2.UnitX * ((float)j + 0.5f) * 8f - Vector2.UnitY;
					break;
				case Directions.Right:
					image.JustifyOrigin(0f, 0.5f);
					image.Position = Vector2.UnitY * ((float)j + 0.5f) * 8f - Vector2.UnitX;
					break;
				case Directions.Left:
					image.JustifyOrigin(1f, 0.5f);
					image.Position = Vector2.UnitY * ((float)j + 0.5f) * 8f + Vector2.UnitX;
					break;
			}
			Add(image);
		}

		private void AddTentacle(float i)
		{
			Sprite sprite = GFX.SpriteBank.Create("tentacles");
			sprite.Play(Calc.Random.Next(3).ToString(), restart: true, randomizeFrame: true);
			sprite.Position = 
			sprite.Scale.X = Calc.Random.Choose(-1, 1);
			sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
			if (Direction == Directions.UpLeft)
			{
				sprite.Rotation = -(float)Math.PI / 1.33333f;
				sprite.Y += 0.707f; sprite.X -= 0.707f;
			}
			else if (Direction == Directions.UpRight)
			{
				sprite.Rotation = -(float)Math.PI / 4f;
				sprite.Y += 0.707f; sprite.X;
			}
			else if (Direction == Directions.DownRight)
			{
				sprite.Rotation = (float)Math.PI;
				float y = sprite.X;
				sprite.X = y + 1f;
			}
			else if (Direction == Directions.Down)
			{
				sprite.Rotation = (float)Math.PI / 2f;
				float y = sprite.Y;
				sprite.Y = y - 1f;
			}
			sprite.Rotation += (float)Math.PI / 2f;
			Add(sprite);
		}

		private void OnEnable()
		{
			Active = (Visible = (Collidable = true));
			SetSpikeColor(EnabledColor);
		}

		private void OnDisable()
		{
			Active = (Collidable = false);
			if (VisibleWhenDisabled)
			{
				foreach (Component component in base.Components)
				{
					Image image = component as Image;
					if (image != null)
					{
						image.Color = DisabledColor;
					}
				}
			}
			else
			{
				Visible = false;
			}
		}

		private void OnShake(Vector2 amount)
		{
			imageOffset += amount;
		}

		public override void Render()
		{
			Vector2 position = Position;
			Position += imageOffset;
			base.Render();
			Position = position;
		}

		public void SetOrigins(Vector2 origin)
		{
			foreach (Component component in base.Components)
			{
				Image image = component as Image;
				if (image != null)
				{
					Vector2 vector = origin - Position;
					image.Origin = image.Origin + vector - image.Position;
					image.Position = vector;
				}
			}
		}

		private void OnCollide(Player player)
		{
			switch (Direction)
			{
				case Directions.Up:
					if (player.Speed.Y >= 0f && player.Bottom <= base.Bottom)
					{
						player.Die(new Vector2(0f, -1f));
					}
					break;
				case Directions.Down:
					if (player.Speed.Y <= 0f)
					{
						player.Die(new Vector2(0f, 1f));
					}
					break;
				case Directions.Left:
					if (player.Speed.X >= 0f)
					{
						player.Die(new Vector2(-1f, 0f));
					}
					break;
				case Directions.Right:
					if (player.Speed.X <= 0f)
					{
						player.Die(new Vector2(1f, 0f));
					}
					break;
			}
		}

		private static int GetSize(EntityData data, Directions dir)
		{
			if ((uint)dir > 1u)
			{
				_ = dir - 2;
				_ = 1;
				return data.Height;
			}
			return data.Width;
		}

		private bool IsRiding(Solid solid)
		{
			switch (Direction)
			{
				default:
					return false;
				case Directions.UpLeft:
					return CollideCheckOutside(solid, Position + Vector2.UnitY);
				case Directions.UpRight:
					return CollideCheckOutside(solid, Position - Vector2.UnitY);
				case Directions.DownLeft:
					return CollideCheckOutside(solid, Position + Vector2.UnitX);
				case Directions.DownRight:
					return CollideCheckOutside(solid, Position - Vector2.UnitX);
			}
		}

		private bool IsRiding(JumpThru jumpThru)
		{
			if (Direction != 0)
			{
				return false;
			}
			return CollideCheck(jumpThru, Position + Vector2.UnitY);
		}
	}
}
