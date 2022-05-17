using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;


namespace VivHelper.Entities
{
	[CustomEntity("VivHelper/LinkedExitBlock")]
    public class LinkedExitBlock : Solid
    {
		private TileGrid tiles;

		private TransitionListener tl;

		private EffectCutout cutout;

		private float startAlpha;

		private char tileType;

		private string groupID;
		private bool master;
		private bool FallType;
		private LinkedExitBlock Master;
		public static List<string> strings;
		public static Dictionary<string, List<LinkedExitBlock>> Group;

		public LinkedExitBlock(Vector2 position, float width, float height, char tileType)
			: base(position, width, height, safe: true)
		{
			base.Depth = -13000;
			this.tileType = tileType;
			tl = new TransitionListener();
			tl.OnOutBegin = OnTransitionOutBegin;
			tl.OnInBegin = OnTransitionInBegin;
			Add(tl);
			Add(cutout = new EffectCutout());
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
			EnableAssistModeChecks = false;
		}

		public LinkedExitBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Char("tileType", '3'))
		{
			groupID = data.Attr("GroupID", "");
			if (!strings.Contains(groupID)) { strings.Add(groupID); }
			FallType = data.Bool("FallType", true);
			startAlpha = data.Float("startAlpha", 0f);
		}

		private void OnTransitionOutBegin()
		{
			if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
			{
				tl.OnOut = OnTransitionOut;
				startAlpha = tiles.Alpha;
			}
		}

		private void OnTransitionOut(float percent)
		{
			cutout.Alpha = (tiles.Alpha = MathHelper.Lerp(startAlpha, 0f, percent));
			cutout.Update();
		}

		private void OnTransitionInBegin()
		{
			if (Collide.CheckRect(this, SceneAs<Level>().PreviousBounds.Value) && !CollideCheck<Player>())
			{
				cutout.Alpha = 0f;
				tiles.Alpha = 0f;
				tl.OnIn = OnTransitionIn;
			}
		}

		private void OnTransitionIn(float percent)
		{
			cutout.Alpha = (tiles.Alpha = percent);
			cutout.Update();
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Level level = SceneAs<Level>();
			Rectangle tileBounds = level.Session.MapData.TileBounds;
			VirtualMap<char> solidsData = level.SolidsData;
			int x = (int)(base.X / 8f) - tileBounds.Left;
			int y = (int)(base.Y / 8f) - tileBounds.Top;
			int tilesX = (int)base.Width / 8;
			int tilesY = (int)base.Height / 8;
			tiles = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
			Add(tiles);
			Add(new TileInterceptor(tiles, highPriority: false));

			if (strings.Count > 0)
			{
				foreach (string s in strings.Distinct()) { if (!Group.ContainsKey(s)) { Group.Add(s, new List<LinkedExitBlock>());} }
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
			cutout.Alpha = (tiles.Alpha = 0f);
			Collidable = false;

			if (groupID != "")
			{
				foreach (LinkedExitBlock c in Group[groupID])
				{
					if (c.master) { this.Master = c; this.FallType = c.FallType; break; }
				}
				if (this.Master == null)
				{
					FallType = true;
				}
			}
		}

		public override void Update()
		{
			base.Update();
			if (Collidable)
			{
				cutout.Alpha = (tiles.Alpha = Calc.Approach(tiles.Alpha, 1f, Engine.DeltaTime));
			}
			else if (!CollideCheck<Player>())
			{
				if (FallType)
				{
					if (master) Fill(); else Master.Fill();
				}
				else
				{
					if (master) { Fill(); }
				}
				
			}
		}

		private void Fill()
        {
			if (master)
			{
				foreach (LinkedExitBlock c in Group[groupID]) { if (c != this) { c.Fill2(); } }
				Fill2();
			}
			else { Fill2(); }
		}

		private void Fill2()
        {
			Collidable = true;
			Audio.Play("event:/game/general/passage_closed_behind", base.Center);
		}

		public override void Render()
		{
			if (tiles.Alpha >= 1f)
			{
				Level level = base.Scene as Level;
				if (level.ShakeVector.X < 0f && level.Camera.X <= (float)level.Bounds.Left && base.X <= (float)level.Bounds.Left)
				{
					tiles.RenderAt(Position + new Vector2(-3f, 0f));
				}
				if (level.ShakeVector.X > 0f && level.Camera.X + 320f >= (float)level.Bounds.Right && base.X + base.Width >= (float)level.Bounds.Right)
				{
					tiles.RenderAt(Position + new Vector2(3f, 0f));
				}
				if (level.ShakeVector.Y < 0f && level.Camera.Y <= (float)level.Bounds.Top && base.Y <= (float)level.Bounds.Top)
				{
					tiles.RenderAt(Position + new Vector2(0f, -3f));
				}
				if (level.ShakeVector.Y > 0f && level.Camera.Y + 180f >= (float)level.Bounds.Bottom && base.Y + base.Height >= (float)level.Bounds.Bottom)
				{
					tiles.RenderAt(Position + new Vector2(0f, 3f));
				}
			}
			base.Render();
		}
	}
}
