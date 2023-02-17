/// An implementation of a group renderer dependent completely on depth

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using VivHelper.Module__Extensions__Etc;

namespace VivHelper.Entities.Spinner2 {

    /// Just because we make Atom an Entity type (which we need to do because of how C# handles multiple inheritance), doesn't mean we actually have to add it to the scene.
    /// <summary>
    /// An atom is an entity that should be grouped via a grouper
    /// </summary>

    public abstract class Atom : Entity {

        public string id;

        public Grouper grouper;

        public readonly Type type;

        private int prevDepth;

        /// <summary>
        /// The Priority of rendering for this Atom. Higher = later = "in front"
        /// </summary>
        internal float priority;

        private int prevPriority;


        public void SetPriority(uint priority) { this.priority = priority; }
        public void SetExactSortValue(float value) { priority = value; }
        public float GetSortValue() { return priority; }
        public int GetPriority() { return (int) priority; }

        /// <summary>
        /// Creates an entity to be rendered by the Grouper.
        /// </summary>
        /// <param name="pos">the Position of the entity, as standard</param>
        /// <param name="grouperType">the Type used to identify the Grouper from the Session.</param>
        public Atom(Vector2 pos, Type grouperType) : base(pos) { type = grouperType; priority = 0f; }

        /// <summary>
        /// Creates an entity to be rendered by the Grouper.
        /// </summary>
        /// <param name="pos">the Position of the entity, as standard</param>
        /// <param name="grouperType">the Type used to identify the Grouper from the Session.</param>
        /// <param name="priority">When, relative to other Atoms of the same depth, the entity renders. Used for group-specific rendering schemes.</param>
        public Atom(Vector2 pos, Type grouperType, int priority) : base(pos) { type = grouperType; this.priority = Math.Max(0f, priority); }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Grouper g = (Grouper) scene.Entities.FindFirst(type); //TO-DO : ensure Tracker can catch this grouper at first load, or implement pre-caching.
            grouper = g ?? throw new Exception("No grouper found for the grouperType " + type.FullName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddToGroup(Grouper grouper) => grouper.AddAtom(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RemoveFromGroup(Grouper grouper) => grouper.RemoveAtom(this);

        protected void MoveWithinGroup(Grouper grouper, int? newDepth = null, int? newPriority = null) {
            if (newDepth != null)
                Depth = newDepth.Value;
            if (newPriority != null)
                priority = newPriority.Value;
            if (Depth == prevDepth && priority == prevPriority)
                return;
            float nP = priority;
            int nD = Depth;
            Depth = prevDepth;
            priority = prevPriority;
            grouper.RemoveAtom(this);
            Depth = nD;
            priority = nP;
            grouper.AddAtom(this);
        }

        public override void Update() {
            prevDepth = Depth;
            base.Update();
        }

        public sealed override void Render() {
            PreRender();
            if (grouper.RenderSet.TryGetValue(Depth, out RenderingSet r) && id == r.designatedRenderer.id)
                grouper.RenderAtDepth(Depth);
        }

        public override void Removed(Scene scene) {
            grouper.RemoveAtom(this);
            base.Removed(scene);
        }

        public virtual void PreRender() { }

        public abstract Type GetGrouperType();

        /// <summary>
        /// Render your object here. DO NOT END THE SPRITEBATCH
        /// </summary>
        public abstract void RenderAtom();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ChangedDepths() => MoveWithinGroup(grouper);
    }

    //You literally never need to change elements of the border object so we just seal it here and then it magically works:tm:
    public sealed class GroupBorderElement : Atom {

        public GroupVisualElement parent;
        public GroupBorderElement(Vector2 pos, Type type, GroupVisualElement parent) : base(pos, type) {
            this.parent = parent;
            id = "Border: " + parent.id;
        }
        public GroupBorderElement(Vector2 pos, Type type, int Priority, GroupVisualElement parent) : base(pos, type, Priority) {
            this.parent = parent;
            id = "Border: " + parent.id;
        }

        public sealed override void RenderAtom() {
            parent.RenderBorder();
        }

        public override Type GetGrouperType() => parent.GetGrouperType();
    }

    [Tracked]
    public abstract class GroupVisualElement : Atom {

        public GroupVisualElement(Vector2 pos, Type type) : base(pos, type) {
            PostUpdate += (entity) => { if (border != null) border.Visible = Visible; };
        }

        public GroupVisualElement(Vector2 pos, Type type, int Priority) : base(pos, type, Priority) {
            PostUpdate += (entity) => { if (border != null) border.Visible = Visible; };
        }

        public GroupBorderElement border;

        public override void Awake(Scene scene) {
            base.Awake(scene);
            AddToGroup(grouper);
            if (border != null) {
                border.AddToGroup(grouper);
            }
        }
        public override void Removed(Scene scene) {
            if(border != null)
                grouper.RemoveAtom(border);
            base.Removed(scene);
        }

        public virtual void RenderBorder() {
            Position += Vector2.UnitX;
            RenderAtom();
            Position -= Vector2.One;
            RenderAtom();
            Position += pseudoconsts.DL;
            RenderAtom();
            Position += Vector2.One;
            RenderAtom();
            Position -= Vector2.UnitY;
        }
    }
    internal struct RenderingSet {
        public Atom designatedRenderer;
        public SortedSet<Atom> set;

        public void DesignateNewRenderer() {
            if (set.Count == 0)
                designatedRenderer = null;
            else
                designatedRenderer = set.First();
        }
    }

    internal class AtomComparer : IComparer<Atom> { public int Compare(Atom x, Atom y) { return x.GetSortValue().CompareTo(y.GetSortValue()); } }

    internal class EmptyAtom : Atom {
        public EmptyAtom(Vector2 pos, Type grouperType, int priority) : base(pos, grouperType, priority) { }

        public override Type GetGrouperType() {
            throw new NotImplementedException();
        }

        public override void RenderAtom() {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Acts as the renderer for all objects in the Group. any Group Render function will 
    /// </summary>
    [Tracked(true)]
    public abstract class Grouper : Entity {

        internal static Atom emptyAtom = new EmptyAtom(Vector2.Zero, null, -1);


        public bool EndPrevRendering;

        public Grouper() {
            Tag = Tags.Global | Tags.Persistent;
            Depth = int.MaxValue; //We want this to "render" first thing. The render method is actually being used as a afterupdate function call.
            Visible = true;
            PostUpdate += _PostUpdate;
        }

        public sealed override void Added(Scene scene) {
            base.Added(scene);
            VivHelperModule.Session.groupers[GetType()] = this;
            RenderSet = new();
        }
        /// Key : Depth, Value : (bool renderedThisFrame, SortedSet<Atom> - stores all Atoms rendered for this depth in a sorted list by Priority
        internal Dictionary<int, RenderingSet> RenderSet;

        public void _PostUpdate(Entity e) {
            foreach (var k in RenderSet) {
                var l = k.Value;
                if (l.designatedRenderer != emptyAtom && (l.designatedRenderer == null || l.designatedRenderer.Scene != Scene)) {
                    l.DesignateNewRenderer();
                }
            }
        }

        internal void AddAtom(Atom atom) {
            SortedSet<Atom> _set;
            if (!RenderSet.ContainsKey(atom.Depth)) {
                _set = new SortedSet<Atom>(new AtomComparer());
                RenderSet[atom.Depth] = new RenderingSet() {
                    set = _set,
                    designatedRenderer = atom
                };
                _set.Add(atom);
                return;
            }
            var a = RenderSet[atom.Depth];
            _set = RenderSet[atom.Depth].set;
            float temp = atom.GetSortValue();
            int overfloat = (int) temp + 1;
            do {
                atom.priority = VivHelper.NextAfter(atom.priority, overfloat); //The cap for this is >3000/room, so this is a reasonable cap.
            } while (!_set.Add(atom) && atom.priority < overfloat);

        }

        public void RemoveAtom(Atom atom) {
            if (!RenderSet.TryGetValue(atom.Depth, out RenderingSet v)) { return; }
            if (v.designatedRenderer != emptyAtom && v.set.Remove(atom) && (v.designatedRenderer == atom || v.designatedRenderer.Scene == atom.Scene)) {
                v.DesignateNewRenderer();
            }
        }

        public virtual void RenderAtDepth(int depth, bool prevRenderOverride = false) {
            RenderingSet a = RenderSet[depth];
            SortedSet<Atom> set = a.set;
            int count = -1;
            if (EndPrevRendering && !prevRenderOverride)
                GameplayRenderer.End();
            foreach (Atom atom in set) {
                if (atom.GetPriority() > count) {
                    if (count > -1)
                        AfterRenderPriority(depth, count);
                    count = atom.GetPriority();
                    BeforeRenderPriority(depth, count);
                }
                atom.RenderAtom();
            }
            AfterRenderPriority(depth, set.Last().GetPriority());
            if (EndPrevRendering && !prevRenderOverride)
                GameplayRenderer.Begin();
        }

        public virtual void BeforeRenderPriority(int depth, int priority) { }
        public virtual void AfterRenderPriority(int depth, int priority) { }
    }
}
