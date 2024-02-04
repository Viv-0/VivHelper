using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [Tracked(false)]
    public class CrystalBombDetonatorRenderer : Entity {

        private class Edge {
            public CrystalBombDetonator2 Parent;
            public bool Visible = true;
            public Vector2 A;
            public Vector2 B;
            public Vector2 Min;
            public Vector2 Max;
            public Vector2 Normal;
            public Vector2 Perpendicular;
            public float[] Wave;
            public float Length;

            public Edge(CrystalBombDetonator2 parent, Vector2 a, Vector2 b) {
                Parent = parent;
                A = a;
                B = b;
                Min = new Vector2(Math.Min(A.X, B.X), Math.Min(A.Y, B.Y));
                Max = new Vector2(Math.Max(A.X, B.X), Math.Max(A.Y, B.Y));
                Normal = (B - A).SafeNormalize();
                Perpendicular = -Normal.Perpendicular();
                Length = (A - B).Length();
            }

            public void UpdateWave(float time) {
                if (Wave == null || (float) Wave.Length <= Length)
                    Wave = new float[(int) Length + 2];

                for (int num = 0; num <= Length; num++)
                    Wave[num] = GetWaveAt(time, num, Length);
            }

            private float GetWaveAt(float offset, float along, float length) {
                if (along <= 1f || along >= length - 1f)
                    return 0f;

                if (Parent.Solidify >= 1f)
                    return 0f;

                float num = offset + along * 0.25f;
                float num2 = (float) (Math.Sin((double) num) * 2.0 + Math.Sin((double) (num * 0.25f)));
                return (1f + num2 * Ease.SineInOut(Calc.YoYo(along / length))) * (1f - Parent.Solidify);
            }

            public bool InView(ref Rectangle view) {
                return (float) view.Left < Parent.X + Max.X && (float) view.Right > Parent.X + Min.X && (float) view.Top < Parent.Y + Max.Y && (float) view.Bottom > Parent.Y + Min.Y;
            }
        }

        private List<CrystalBombDetonator2> trackedFields = new List<CrystalBombDetonator2>();
        private List<CrystalBombDetonatorRenderer.Edge> edges = new List<CrystalBombDetonatorRenderer.Edge>();
        private VirtualMap<bool> tiles;
        private Rectangle levelTileBounds;
        private bool dirty;
        public CrystalBombDetonatorController colorController;
        public CrystalBombDetonatorRenderer() {
            Tag = (Tags.Global | Tags.TransitionUpdate);
            Depth = 0;
            Add(new CustomBloom(OnRenderBloom));
        }

        // Hooks to add ourself to the level at the very start
        public static void Load() {
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
        }

        static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            self.Level.Add(new CrystalBombDetonatorRenderer());
            orig(self);
        }

        // Add a field to the tracker
        public void Track(CrystalBombDetonator2 block) {
            trackedFields.Add(block);
            if (tiles == null) {
                levelTileBounds = (Scene as Level).TileBounds;
                tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height, false);
            }

            for (int xTile = (int) block.X / 8; xTile < (block.Right / 8f); xTile++)
                for (int yTile = (int) block.Y / 8; yTile < (block.Bottom / 8f); yTile++)
                    tiles[xTile - levelTileBounds.X, yTile - levelTileBounds.Y] = true;

            dirty = true;
        }

        public void Untrack(CrystalBombDetonator2 block) {
            trackedFields.Remove(block);
            if (trackedFields.Count <= 0)
                tiles = null;
            else
                for (int xTile = (int) block.X / 8; xTile < (block.Right / 8f); xTile++)
                    for (int yTile = (int) block.Y / 8; yTile < (block.Bottom / 8f); yTile++)
                        tiles[xTile - levelTileBounds.X, yTile - levelTileBounds.Y] = false;

            dirty = true;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            colorController = Scene.Tracker.GetEntity<CrystalBombDetonatorController>();
        }

        public override void Update() {
            if (colorController == null) {
                colorController = Scene.Tracker.GetEntity<CrystalBombDetonatorController>();
            }
            if (dirty)
                RebuildEdges();
            UpdateEdges();
        }

        public void UpdateEdges() {
            Camera camera = (Scene as Level).Camera;
            Rectangle cameraRect = new Rectangle((int) camera.Left - 4, (int) camera.Top - 4, (int) (camera.Right - camera.Left) + 8, (int) (camera.Bottom - camera.Top) + 8);
            for (int i = 0; i < edges.Count; i++) {
                if (edges[i].Visible && Scene.OnInterval(0.25f, i * 0.01f) && !edges[i].InView(ref cameraRect))
                    edges[i].Visible = false;
                else
                    if (Scene.OnInterval(0.05f, i * 0.01f) && edges[i].InView(ref cameraRect))
                    edges[i].Visible = true;

                if (edges[i].Visible && (Scene.OnInterval(0.05f, i * 0.01f) || edges[i].Wave == null))
                    edges[i].UpdateWave(Scene.TimeActive * 3f);
            }
        }

        private void RebuildEdges() {
            dirty = false;
            edges.Clear();
            if (trackedFields.Count > 0) {
                Level level = Scene as Level;
                int left = level.TileBounds.Left;
                int top = level.TileBounds.Top;
                int right = level.TileBounds.Right;
                int bottom = level.TileBounds.Bottom;
                Point[] array = new Point[]
                {
                    new Point(0, -1),
                    new Point(0, 1),
                    new Point(-1, 0),
                    new Point(1, 0)
                };

                /* Responding to Natalie's comment from SC2020, happy to explain! (Yes this is mostly so I don't forget but thought i'd give a shoutout.)
                 * 
                 * This Point code is slightly more useful, as in every case the Points will be integers and never floats, which is what Vector2 uses.
                 * Slightly more optimized code, probably because originally it was very inefficient.
                 * 
                 * It also uses tile-by-tile recognition, as shown by the `* 8f`s down below, and that also gives us the VirtualMap option
                 * 
                 * The code itself is running through all available tiles with a CrystalBombDetonator in the tracked list, and then computing edges if there isn't one around it (see the array above)
                 */
                foreach (CrystalBombDetonator2 crystalBombDetonator in trackedFields) {
                    for (int xTile = (int) crystalBombDetonator.X / 8; xTile < (crystalBombDetonator.Right / 8f); xTile++)
                        for (int yTile = (int) crystalBombDetonator.Y / 8; yTile < (crystalBombDetonator.Bottom / 8f); yTile++)
                            foreach (Point point in array) {
                                Point point2 = new Point(-point.Y, point.X);
                                if (!Inside(xTile + point.X, yTile + point.Y) && (!Inside(xTile - point2.X, yTile - point2.Y) || Inside(xTile + point.X - point2.X, yTile + point.Y - point2.Y))) {
                                    Point point3 = new Point(xTile, yTile);
                                    Point point4 = new Point(xTile + point2.X, yTile + point2.Y);
                                    Vector2 value = new Vector2(4f) + new Vector2((float) (point.X - point2.X), (float) (point.Y - point2.Y)) * 4f;
                                    while (Inside(point4.X, point4.Y) && !Inside(point4.X + point.X, point4.Y + point.Y)) {
                                        point4.X += point2.X;
                                        point4.Y += point2.Y;
                                    }
                                    Vector2 a = new Vector2((float) point3.X, (float) point3.Y) * 8f + value - crystalBombDetonator.Position;
                                    Vector2 b = new Vector2((float) point4.X, (float) point4.Y) * 8f + value - crystalBombDetonator.Position;
                                    edges.Add(new CrystalBombDetonatorRenderer.Edge(crystalBombDetonator, a, b));
                                }
                            }
                }
            }
        }

        private bool Inside(int tx, int ty) {
            return tiles[tx - levelTileBounds.X, ty - levelTileBounds.Y];
        }

        private void OnRenderBloom() {
            foreach (CrystalBombDetonator2 crystalBombDetonator in trackedFields) {
                if (crystalBombDetonator.Visible)
                    Draw.Rect(crystalBombDetonator.X, crystalBombDetonator.Y, crystalBombDetonator.Width, crystalBombDetonator.Height, Color.Purple);
            }
            foreach (CrystalBombDetonatorRenderer.Edge edge in edges) {
                if (edge.Visible) {
                    Vector2 value = edge.Parent.Position + edge.A;
                    Vector2 vector = edge.Parent.Position + edge.B;
                    for (int num = 0; num <= edge.Length; num++) {
                        Vector2 vector2 = value + edge.Normal * num;
                        Draw.Line(vector2, vector2 + edge.Perpendicular * edge.Wave[num], Color.Purple);
                    }
                }
            }
        }

        public override void Render() {
            if (trackedFields.Count <= 0)
                return;

            Color color = (colorController?.baseColor ?? VivHelper.OldColorFunction(VivHelperModule.Session.savedCBDController?.baseColorHex ?? VivHelperModule.defaultCBDController.baseColorHex)) * 0.5f;
            foreach (CrystalBombDetonator2 crystalBombDetonator in trackedFields) {
                if (crystalBombDetonator.Visible)
                    Draw.Rect(crystalBombDetonator.Collider, color);
            }
            if (edges.Count > 0)
                foreach (CrystalBombDetonatorRenderer.Edge edge in edges) {
                    if (edge.Visible) {
                        Vector2 value2 = edge.Parent.Position + edge.A;
                        Vector2 vector = edge.Parent.Position + edge.B;
                        for (int num = 0; num <= edge.Length; num++) {
                            Vector2 vector2 = value2 + edge.Normal * num;
                            Draw.Line(vector2, vector2 + edge.Perpendicular * edge.Wave[num], color * 0.9f);
                        }
                    }
                }
        }


    }
}
