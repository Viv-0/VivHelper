using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using MonoMod;

namespace VivTestMod.Entities.SeekerStuff
{
    public class FixedPathfinder
    {
		private struct Tile
		{
			public bool Solid;

			public int Cost;

			public Point? Parent;

			public bool tile;
		}

		private class PointMapComparer : IComparer<Point>
		{
			private Tile[,] map;

			public PointMapComparer(Tile[,] map)
			{
				this.map = map;
			}

			public int Compare(Point a, Point b)
			{
				return map[b.X, b.Y].Cost - map[a.X, a.Y].Cost;
			}
		}

		private static readonly Point[] directions = new Point[4]
		{
		new Point(1, 0),
		new Point(0, 1),
		new Point(-1, 0),
		new Point(0, -1)
		};

		private const int MapSize = 200;

		private Level level;

		private Tile[,] map;

		private List<Point> active = new List<Point>();

		private PointMapComparer comparer;

		public bool DebugRenderEnabled;

		private List<Vector2> lastPath;

		private Point debugLastStart;

		private Point debugLastEnd;

		public FixedPathfinder(Level level)
		{
			this.level = level;
			int num = level.Bounds.Width / 8;
			int num2 = level.Bounds.Height / 8;
			map = new Tile[num, num2];
			comparer = new PointMapComparer(map);
		}

		public bool Find(ref List<Vector2> path, Vector2 from, Vector2 to, bool fewerTurns = true, bool logging = false)
		{
			int num = level.Bounds.Width / 8;
			int num2 = level.Bounds.Height / 8;
			if (map.GetLength(0) < num || map.GetLength(1) < num2)
			{
				int num3 = Math.Max(map.GetLength(0), num);
				int num4 = Math.Max(map.GetLength(1), num2);
				map = new Tile[num3, num4];
				comparer = new PointMapComparer(map);
			}
			return orig_Find(ref path, from, to, fewerTurns, logging);
		}
		public void Render()
		{
			for (int i = 0; i < map.GetLength(0); i++)
			{
				for (int j = 0; j < map.GetLength(1); j++)
				{
					if (map[i, j].Solid)
					{
						Draw.Rect(level.Bounds.Left + i * 8, level.Bounds.Top + j * 8, 8f, 8f, Color.Red * 0.25f);
					}
				}
			}
			if (lastPath != null)
			{
				Vector2 start = lastPath[0];
				for (int k = 1; k < lastPath.Count; k++)
				{
					Vector2 vector = lastPath[k];
					Draw.Line(start, vector, Color.Red);
					Draw.Rect(start.X - 2f, start.Y - 2f, 4f, 4f, Color.Red);
					start = vector;
				}
				Draw.Rect(start.X - 2f, start.Y - 2f, 4f, 4f, Color.Red);
			}
			Draw.Rect(level.Bounds.Left + debugLastStart.X * 8 + 2, level.Bounds.Top + debugLastStart.Y * 8 + 2, 4f, 4f, Color.Green);
			Draw.Rect(level.Bounds.Left + debugLastEnd.X * 8 + 2, level.Bounds.Top + debugLastEnd.Y * 8 + 2, 4f, 4f, Color.Green);
		}

		public bool orig_Find(ref List<Vector2> path, Vector2 from, Vector2 to, bool fewerTurns = true, bool logging = false)
		{
			lastPath = null;
			int num = level.Bounds.Left / 8;
			int num2 = level.Bounds.Top / 8;
			int num3 = level.Bounds.Width / 8;
			int num4 = level.Bounds.Height / 8;
			foreach (Entity entity in level.Tracker.GetEntities<Solid>())
			{
				if (entity.Collidable && entity.Collider is Hitbox)
				{
					int k = (int)Math.Floor(entity.Left / 8f);
					for (int num5 = (int)Math.Ceiling(entity.Right / 8f); k < num5; k++)
					{
						int l = (int)Math.Floor(entity.Top / 8f);
						for (int num6 = (int)Math.Ceiling(entity.Bottom / 8f); l < num6; l++)
						{
							int num7 = k - num;
							int num8 = l - num2;
							if (num7 >= 0 && num8 >= 0 && num7 < num3 && num8 < num4)
							{
								map[num7, num8].Solid = true;
							}
						}
					}
				}
			}
			Point point = debugLastStart = new Point((int)Math.Floor(from.X / 8f) - num, (int)Math.Floor(from.Y / 8f) - num2);
			Point point2 = debugLastEnd = new Point((int)Math.Floor(to.X / 8f) - num, (int)Math.Floor(to.Y / 8f) - num2);
			if (point.X < 0 || point.Y < 0 || point.X >= num3 || point.Y >= num4 || point2.X < 0 || point2.Y < 0 || point2.X >= num3 || point2.Y >= num4)
			{
				if (logging)
				{
					Calc.Log("PF: FAILED - Start or End outside the level bounds");
				}
				return false;
			}
			if (map[point.X, point.Y].Solid)
			{
				if (logging)
				{
					Calc.Log("PF: FAILED - Start inside a solid");
				}
				return false;
			}
			if (map[point2.X, point2.Y].Solid)
			{
				if (logging)
				{
					Calc.Log("PF: FAILED - End inside a solid");
				}
				return false;
			}
			active.Clear();
			active.Add(point);
			map[point.X, point.Y].Cost = 0;
			bool flag = false;
			while (active.Count > 0 && !flag)
			{
				Point value = active[active.Count - 1];
				active.RemoveAt(active.Count - 1);
				for (int m = 0; m < 4; m++)
				{
					Point point3 = new Point(directions[m].X, directions[m].Y);
					Point point4 = new Point(value.X + point3.X, value.Y + point3.Y);
					int num9 = 1;
					if (point4.X < 0 || point4.Y < 0 || point4.X >= num3 || point4.Y >= num4 || map[point4.X, point4.Y].Solid)
					{
						continue;
					}
					for (int n = 0; n < 4; n++)
					{
						Point point5 = new Point(point4.X + directions[n].X, point4.Y + directions[n].Y);
						if (point5.X >= 0 && point5.Y >= 0 && point5.X < num3 && point5.Y < num4 && map[point5.X, point5.Y].Solid)
						{
							num9 = 7;
							break;
						}
					}
					if (fewerTurns && map[value.X, value.Y].Parent.HasValue && point4.X != map[value.X, value.Y].Parent.Value.X && point4.Y != map[value.X, value.Y].Parent.Value.Y)
					{
						num9 += 4;
					}
					int cost = map[value.X, value.Y].Cost;
					if (point3.Y != 0)
					{
						num9 += (int)((float)cost * 0.5f);
					}
					int num10 = cost + num9;
					if (map[point4.X, point4.Y].Cost > num10)
					{
						map[point4.X, point4.Y].Cost = num10;
						map[point4.X, point4.Y].Parent = value;
						int num11 = active.BinarySearch(point4, comparer);
						if (num11 < 0)
						{
							num11 = ~num11;
						}
						active.Insert(num11, point4);
						if (point4 == point2)
						{
							flag = true;
							break;
						}
					}
				}
			}
			if (!flag)
			{
				if (logging)
				{
					Calc.Log("PF: FAILED - ran out of active nodes, can't find ending");
				}
				return false;
			}
			path.Clear();
			Point a = point2;
			int num12 = 0;
			while (a != point && num12++ < 1000)
			{
				path.Add(new Vector2((float)a.X + 0.5f, (float)a.Y + 0.5f) * 8f + level.LevelOffset);
				a = map[a.X, a.Y].Parent.Value;
			}
			if (num12 >= 1000)
			{
				Console.WriteLine("WARNING: Pathfinder 'succeeded' but then was unable to work out its path?");
				return false;
			}
			for (int num13 = 1; num13 < path.Count - 1; num13++)
			{
				if (path.Count <= 2)
				{
					break;
				}
				if ((path[num13].X == path[num13 - 1].X && path[num13].X == path[num13 + 1].X) || (path[num13].Y == path[num13 - 1].Y && path[num13].Y == path[num13 + 1].Y))
				{
					path.RemoveAt(num13);
					num13--;
				}
			}
			path.Reverse();
			lastPath = path;
			if (logging)
			{
				Calc.Log("PF: SUCCESS");
			}
			return true;
		}
	}
}
