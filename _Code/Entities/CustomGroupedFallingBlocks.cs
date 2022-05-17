using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using Celeste.Mod.Entities;

namespace VivHelper.Entities
{
    [TrackedAs(typeof(FallingBlock))]
	[CustomEntity("VivHelper/LinkedFallingBlock")]
    public class CustomGroupedFallingBlock : Solid
    {
		public static ParticleType P_FallDustA;

		public static ParticleType P_FallDustB;

		public static ParticleType P_LandDust;

		public bool Triggered;

		public float FallDelay;

		private char TileType;

		private TileGrid tiles;

		private TileGrid highlight;

		private bool finalBoss;

		private bool climbFall;

		private float maxSpeed;
		private float accel;
		private bool disableParticles;
		private bool disableShake;
		private string groupID;
		private bool master;
		private bool FallType;
		/*	FallType: 
		 *	true => Any block falling will trigger all to fall
		 *	false => Only the block designated Master will trigger all to fall
		 */
		private CustomGroupedFallingBlock Master;
		private static List<string> strings = new List<string>();
		public static Dictionary<string, List<CustomGroupedFallingBlock>> Group = new Dictionary<string, List<CustomGroupedFallingBlock>>();
		protected static bool check = false;

		public bool HasStartedFalling
		{
			get;
			private set;
		}

		public CustomGroupedFallingBlock(Vector2 position, char tile, int width, int height, bool finalBoss, bool behind, bool climbFall)
			: base(position, width, height, safe: false)
		{
			Triggered = false;
			this.finalBoss = finalBoss;
			this.climbFall = climbFall;
			int newSeed = Calc.Random.Next();
			Calc.PushRandom(newSeed);
			Add(tiles = GFX.FGAutotiler.GenerateBox(tile, width / 8, height / 8).TileGrid);
			Calc.PopRandom();
			if (finalBoss)
			{
				Calc.PushRandom(newSeed);
				Add(highlight = GFX.FGAutotiler.GenerateBox('G', width / 8, height / 8).TileGrid);
				Calc.PopRandom();
				highlight.Alpha = 0f;
			}
			Add(new LightOcclude());
			Add(new TileInterceptor(tiles, highPriority: false));
			TileType = tile;
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[tile];
			if (behind)
			{
				base.Depth = 5000;
			}
		}

		public CustomGroupedFallingBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, finalBoss: false, data.Bool("behind"), data.Bool("climbFall", defaultValue: true))
		{
			FallDelay = Math.Max(data.Float("FallDelay", 0.2f) - 0.2f, 0f);
			maxSpeed = data.Float("MaxSpeed", 160f);
			accel = data.Float("Accel", 500f);
			disableParticles = data.Bool("DisableParticles", false);
			disableShake = data.Bool("DisableShake", false);
			groupID = data.Attr("GroupID", "");
			if (!strings.Contains(groupID)) { strings.Add(groupID); }
			FallType = data.Bool("FallType", true);
			master = data.Bool("Master", false);
		}

        public override void Added(Scene scene)
        {
            base.Added(scene);
			if(strings.Count > 0)
            {
				foreach(string s in strings.Distinct()) { if (!Group.ContainsKey(s)){ Group.Add(s, new List<CustomGroupedFallingBlock>()); } }
				strings.Clear();
            }
			if (groupID != "")
			{
				Group[groupID].Add(this);
			}
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
			if (groupID != "")
			{
				foreach (CustomGroupedFallingBlock c in Group[groupID])
				{
					if (c.master) { this.Master = c; this.FallType = c.FallType; break; }
				}
				if (this.Master == null)
				{
					this.master = true;
				}
			}
        }

        public override void OnShake(Vector2 amount)
		{
			base.OnShake(amount);
			tiles.Position += amount;
			if (highlight != null)
			{
				highlight.Position += amount;
			}
		}

		private bool PlayerFallCheck()
		{
			if (climbFall)
			{
				return HasPlayerRider();
			}
			return HasPlayerOnTop();
		}

		private bool PlayerWaitCheck()
		{
			if (Triggered)
			{
				return true;
			}
			if (PlayerFallCheck())
			{
				return true;
			}
			if (climbFall)
			{
				if (!CollideCheck<Player>(Position - Vector2.UnitX))
				{
					return CollideCheck<Player>(Position + Vector2.UnitX);
				}
				return true;
			}
			return false;
		}

        public override void Update()
        {
            base.Update();
			if (groupID != "" && !Triggered)
			{
				if (PlayerFallCheck())
				{
					if (FallType)
					{
						if (master) Fall(); else Master.Fall();
					}
					else
					{
						if (master) { Fall(); }
					}
				}
			}
        }

        public void Fall()
        {
			if (master)
			{
				foreach (CustomGroupedFallingBlock c in Group[groupID]) { if (c != this) { c.Fall();  } }
				Add(new Coroutine(Sequence())); Triggered = true;
			}
			else { Add(new Coroutine(Sequence())); Triggered = true; }
        }

		private IEnumerator Sequence()
		{
			Triggered = true;
			while (FallDelay > 0f)
			{
				FallDelay -= Engine.DeltaTime;
				yield return null;
			}
			HasStartedFalling = true;
			while (true)
			{
				ShakeSfx();
				StartShaking();
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				if (finalBoss)
				{
					Add(new Coroutine(HighlightFade(1f)));
				}
				yield return 0.2f;
				float timer = 0.4f;
				if (finalBoss)
				{
					timer = 0.2f;
				}
				while (timer > 0f && PlayerWaitCheck())
				{
					yield return null;
					timer -= Engine.DeltaTime;
				}
				StopShaking();
				for (int i = 2; (float)i < Width; i += 4)
				{
					if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
					{
						SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f, (float)Math.PI / 2f);
					}
					SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f);
				}
				float speed = 0f;
				float maxSpeed = finalBoss ? 130f : 160f;
				while (true)
				{
					Level level = SceneAs<Level>();
					speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
					if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
					{
						break;
					}
					if (Top > (float)(level.Bounds.Bottom + 16) || (Top > (float)(level.Bounds.Bottom - 1) && CollideCheck<Solid>(Position + new Vector2(0f, 1f))))
					{
						Collidable = (Visible = false);
						yield return 0.2f;
						if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
						{
							yield return 0.2f;
							SceneAs<Level>().Shake();
							Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
						}
						RemoveSelf();
						DestroyStaticMovers();
						yield break;
					}
					yield return null;
				}
				ImpactSfx();
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
				SceneAs<Level>().DirectionalShake(Vector2.UnitY, finalBoss ? 0.2f : 0.3f);
				if (finalBoss)
				{
					Add(new Coroutine(HighlightFade(0f)));
				}
				StartShaking();
				LandParticles();
				yield return 0.2f;
				StopShaking();
				if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f)))
				{
					break;
				}
				while (CollideCheck<Platform>(Position + new Vector2(0f, 1f)))
				{
					yield return 0.1f;
				}
			}
			Safe = true;
		}

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        private IEnumerator HighlightFade(float to)
		{
			float from = highlight.Alpha;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.5f)
			{
				highlight.Alpha = MathHelper.Lerp(from, to, Ease.CubeInOut(p));
				tiles.Alpha = 1f - highlight.Alpha;
				yield return null;
			}
			highlight.Alpha = to;
			tiles.Alpha = 1f - to;
		}

		private void LandParticles()
		{
			for (int i = 2; (float)i <= base.Width; i += 4)
			{
				if (base.Scene.CollideCheck<Solid>(base.BottomLeft + new Vector2(i, 3f)))
				{
					SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, -(float)Math.PI / 2f);
					float direction = (!((float)i < base.Width / 2f)) ? 0f : ((float)Math.PI);
					SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, direction);
				}
			}
		}

		private void ShakeSfx()
		{
			if (TileType == '3')
			{
				Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", base.Center);
			}
			else if (TileType == '9')
			{
				Audio.Play("event:/game/03_resort/fallblock_wood_shake", base.Center);
			}
			else if (TileType == 'g')
			{
				Audio.Play("event:/game/06_reflection/fallblock_boss_shake", base.Center);
			}
			else
			{
				Audio.Play("event:/game/general/fallblock_shake", base.Center);
			}
		}

		private void ImpactSfx()
		{
			if (TileType == '3')
			{
				Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", base.BottomCenter);
			}
			else if (TileType == '9')
			{
				Audio.Play("event:/game/03_resort/fallblock_wood_impact", base.BottomCenter);
			}
			else if (TileType == 'g')
			{
				Audio.Play("event:/game/06_reflection/fallblock_boss_impact", base.BottomCenter);
			}
			else
			{
				Audio.Play("event:/game/general/fallblock_impact", base.BottomCenter);
			}
		}
	}
}
