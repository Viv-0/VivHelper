using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivTestMod.Entities
{
	[CustomEntity("VivHelper/ConnectedDashBlock")]
	[Tracked]
	public class ConnectedDashBlock : Solid
	{
		public enum Modes
		{
			Dash,
			FinalBoss,
			Crusher
		}

		public enum DebrisAmount { Eighth, Quarter, Half, Normal, Sixteen, Eight, Four, None }

		public DebrisAmount dAmt = DebrisAmount.Normal;

		private bool permanent;

		private EntityID id;

		private TileGrid tiles;

		private char tileType;

		private float width;

		private float height;

		private bool blendIn;

		public Dictionary<Platform, Vector2> Moves;

		private bool canDash;

		private bool awake;

		public bool masterIsBroken = false;

		public ConnectedDashBlock master;

		public List<ConnectedDashBlock> Group;

		public Point GroupBoundsMin;

		public Point GroupBoundsMax;

		public bool HasGroup
		{
			get;
			private set;
		}

		public bool MasterOfGroup
		{
			get;
			private set;
		}

		public ConnectedDashBlock(Vector2 position, char tiletype, float width, float height, bool blendIn, bool permanent, bool canDash, EntityID id)
			: base(position, width, height, safe: true)
		{
			base.Depth = -12999;
			this.id = id;
			this.permanent = permanent;
			this.width = width;
			this.height = height;
			this.blendIn = blendIn;
			this.canDash = canDash;
			tileType = tiletype;
			OnDashCollide = OnDash;
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
		}

		public ConnectedDashBlock(EntityData data, Vector2 offset, EntityID id)
			: this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, data.Bool("blendin"), data.Bool("permanent", defaultValue: true), data.Bool("canDash", defaultValue: true), id)
		{
			dAmt = data.Enum<DebrisAmount>("DebrisAmount", DebrisAmount.Normal);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			awake = true;
			if (!HasGroup)
			{
				MasterOfGroup = true;
				Moves = new Dictionary<Platform, Vector2>();
				Group = new List<ConnectedDashBlock>();
				GroupBoundsMin = new Point((int)base.X, (int)base.Y);
				GroupBoundsMax = new Point((int)base.Right, (int)base.Bottom);
				AddToGroupAndFindChildren(this);
				_ = base.Scene;
				Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8 + 1, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8 + 1);
				VirtualMap<char> virtualMap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');
				foreach (ConnectedDashBlock item in Group)
				{
					int num = (int)(item.X / 8f) - rectangle.X;
					int num2 = (int)(item.Y / 8f) - rectangle.Y;
					int num3 = (int)(item.Width / 8f);
					int num4 = (int)(item.Height / 8f);
					for (int i = num; i < num + num3; i++)
					{
						for (int j = num2; j < num2 + num4; j++)
						{
							virtualMap[i, j] = tileType;
						}
					}
				}
				tiles = GFX.FGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour
				{
					EdgesExtend = false,
					EdgesIgnoreOutOfLevel = false,
					PaddingIgnoreOutOfLevel = false
				}).TileGrid;
				tiles.Position = new Vector2((float)GroupBoundsMin.X - base.X, (float)GroupBoundsMin.Y - base.Y);
				Add(tiles);
			};
			Add(new TileInterceptor(tiles, highPriority: true));
			if (CollideCheck<Player>())
			{
				RemoveSelf();
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			Celeste.Celeste.Freeze(0.05f);
		}

		public void BreakMaster(Vector2 from, Vector2 dir, bool pS = true, bool pDS = true)
        {
            if (MasterOfGroup)
            {
				foreach (ConnectedDashBlock item in Group)
				{
					item.Break(from, dir, dAmt, this.MasterOfGroup, this.MasterOfGroup);
				}
			}
			else
			{
				master.BreakMaster(from,dir,pS,pDS);
			}
		}

		public void Break(Vector2 from, Vector2 direction, DebrisAmount debrisAmt, bool playSound = true, bool playDebrisSound = true)
		{
			if (playSound)
			{
				if (tileType == '1')
				{
					Audio.Play("event:/game/general/wall_break_dirt", Position);
				}
				else if (tileType == '3')
				{
					Audio.Play("event:/game/general/wall_break_ice", Position);
				}
				else if (tileType == '9')
				{
					Audio.Play("event:/game/general/wall_break_wood", Position);
				}
				else
				{
					Audio.Play("event:/game/general/wall_break_stone", Position);
				}
			}
			float wN, hN;
			float iM, jM;
			iM = base.Width / 16f;
			jM = base.Height / 16f;
			switch (dAmt)
            {
				case DebrisAmount.Normal:
					wN = hN = 1f; break;
				case DebrisAmount.Half:
					wN = hN = 2f; break;
				case DebrisAmount.Quarter:
					wN = hN = 4f; break;
				case DebrisAmount.Eighth:
					wN = hN = 8f; break;
				case DebrisAmount.Sixteen:
					wN = iM / 16f;
					hN = jM / 16f;
					break;
				case DebrisAmount.Eight:
					wN = iM / 8f;
					hN = jM / 8f;
					break;
				case DebrisAmount.Four:
					wN = iM / 4f;
					hN = jM / 4f;
					break;
				case DebrisAmount.None:
					wN = iM;
					hN = jM;
					break;
				default:
					throw new Exception("Debris Amount definition error"); 

			}
			if (dAmt != DebrisAmount.None)
			{
				for (float i = 0; i < iM; i += wN)
				{
					for (float j = 0; j < jM; j += hN)
					{
						if (!base.Scene.CollideCheck<Solid>(new Rectangle((int)base.X + (int)i * 8, (int)base.Y + (int)j * 8, 8, 8)))
						{
							base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tileType, playDebrisSound).BlastFrom(from));
						}
						if (!base.Scene.CollideCheck<Solid>(new Rectangle((int)base.X + (int)((base.Width / 8f) - (i * 8)), (int)base.Y + (int)((base.Height / 8f) - (j * 8)), 8, 8)))
						{
							base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2((base.Width / 8f + 4) - i * 8, (base.Height / 8f + 4) - j * 8), tileType, playDebrisSound).BlastFrom(from));
						}
						
					}
				}
			}
			Collidable = false;
			if (permanent)
			{
				RemoveAndFlagAsGone();
			}
			else
			{
				RemoveSelf();
			}
		}

		public void RemoveAndFlagAsGone()
		{
			RemoveSelf();
			SceneAs<Level>().Session.DoNotLoad.Add(id);
		}

		private DashCollisionResults OnDash(Player player, Vector2 direction)
		{
			if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10)
			{
				return DashCollisionResults.NormalCollision;
			}
			BreakMaster(player.Center, direction);
			return DashCollisionResults.Rebound;
		}

		private void AddToGroupAndFindChildren(ConnectedDashBlock from)
		{
			if (from.X < (float)GroupBoundsMin.X)
			{
				GroupBoundsMin.X = (int)from.X;
			}
			if (from.Y < (float)GroupBoundsMin.Y)
			{
				GroupBoundsMin.Y = (int)from.Y;
			}
			if (from.Right > (float)GroupBoundsMax.X)
			{
				GroupBoundsMax.X = (int)from.Right;
			}
			if (from.Bottom > (float)GroupBoundsMax.Y)
			{
				GroupBoundsMax.Y = (int)from.Bottom;
			}
			from.HasGroup = true;
			from.OnDashCollide = OnDash;
			Group.Add(from);
			if (from != this)
			{
				from.master = this;
			}
			foreach (ConnectedDashBlock entity in base.Scene.Tracker.GetEntities<ConnectedDashBlock>())
			{
				if (!entity.HasGroup && entity.tileType == tileType && (base.Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), entity) || base.Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), entity)))
				{
					AddToGroupAndFindChildren(entity);
				}
			}
		}
	}

}
