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
	[Tracked]
	[CustomEntity("VivHelper/HoldableBarrier")]
    public class HoldableBarrier : Solid
    {
		
        public static Color BarrierPixelColor = Calc.HexToColor("5b6ee1");

        public float Flash;

        public float Solidify;

        public bool Flashing;

        private float solidifyDelay;

        private List<Vector2> particles = new List<Vector2>();

        private List<HoldableBarrier> adjacent = new List<HoldableBarrier>();

        private float[] speeds = new float[3] { 12f, 20f, 40f };

		//Added for Direction, Color modification
		public HoldableBarrierColorController colorController;

		//Added for priority modding
		private int attempts = 100;

		

		#region Hooks

		public static void Load()
        {
			On.Celeste.Actor.MoveH += SolidForHoldablesH;
			On.Celeste.Actor.MoveV += SolidForHoldablesV;
			On.Celeste.Actor.OnGround_int += SolidGround;
			//We don't need to hook the Vector2 variant because it calls the method with only one parameter.
			On.Celeste.Holdable.Release += OnRelease;
		}

		public static void Unload()
        {
			
			On.Celeste.Actor.MoveH -= SolidForHoldablesH;
			On.Celeste.Actor.MoveV -= SolidForHoldablesV;
			On.Celeste.Actor.OnGround_int -= SolidGround;
			On.Celeste.Holdable.Release -= OnRelease;
		}

		

		private static bool SolidForHoldablesH(On.Celeste.Actor.orig_MoveH orig, Actor self, float moveH, Collision c = null, Solid pusher = null)
        {
			//This hook is identical to the set up used in DJMapHelper's Theo Crystal Barrier, MaxHelpingHand's Kevin Barrier, etc.
			//We want to make the barrier solid for any actor that has a Holdable Component and is not being held currently.
			//Somehow this never breaks. I have no clue how this never breaks lmao.
			Holdable h = self.Get<Holdable>();
			List<Entity> barriers = self.Scene.Tracker.GetEntities<HoldableBarrier>();
			if (h?.IsHeld ?? false)
            {
				//We now know that there is a Holdable Component to this entity, and that it is not being currently held.
				
				barriers.ForEach(entity => entity.Collidable = true); //Just learned that this is doable on an enumerable. Very cool.
				bool t = orig(self, moveH, c, pusher);
				barriers.ForEach(entity => entity.Collidable = false); //Thanks to DJMapHelper for that insight.
				return t;
			}
			return orig(self, moveH, c, pusher);

		}

		private static bool SolidForHoldablesV(On.Celeste.Actor.orig_MoveV orig, Actor self, float moveV, Collision c = null, Solid pusher = null)
		{
			//This hook is identical to the set up used in DJMapHelper's Theo Crystal Barrier, MaxHelpingHand's Kevin Barrier, etc.
			//We want to make the barrier solid for any actor that has a Holdable Component and is not being held currently.
			//Somehow this never breaks. I have no clue how this never breaks lmao.
			Holdable h = self.Get<Holdable>();
			List<Entity> barriers = self.Scene.Tracker.GetEntities<HoldableBarrier>();
			if (h?.IsHeld ?? false)
            {
                //We now know that there is a Holdable Component to this entity, and that it is not being currently held.
                barriers.ForEach(entity => entity.Collidable = true); //Just learned that this is doable on an enumerable. Very cool.
                bool t = orig(self, moveV, c, pusher);
                barriers.ForEach(entity => entity.Collidable = false); //Thanks to DJMapHelper for that insight.
                return t;
            }
			return orig(self, moveV, c, pusher);

		}

		private static bool SolidGround(On.Celeste.Actor.orig_OnGround_int orig, Actor self, int d = 1)
        {
			//This hook is identical to the set up used in DJMapHelper's Theo Crystal Barrier, MaxHelpingHand's Kevin Barrier, etc.
			//We want to make the barrier solid for any actor that has a Holdable Component and is not being held currently.
			//Somehow this never breaks. I have no clue how this never breaks lmao.
			Holdable h = self.Get<Holdable>();
			if (h == null || h.IsHeld)
			{
				return orig(self, d);
			}
			//We now know that there is a Holdable Component to this entity, and that it is not being currently held.
			List<Entity> barriers = self.Scene.Tracker.GetEntities<HoldableBarrier>();
			barriers.ForEach(entity => entity.Collidable = true); //Just learned that this is doable on an enumerable. Very cool.
			bool t = orig(self, d);
			barriers.ForEach(entity => entity.Collidable = false); //Thanks to DJMapHelper for that insight.
			return t;
		}

		private static void OnRelease(On.Celeste.Holdable.orig_Release orig, Holdable self, Vector2 force)
        {
			//This hook is similar to the set up used in DJMapHelper's Theo Crystal Barrier, MaxHelpingHand's Kevin Barrier, etc.
			//In this case, we already know that the Holdable exists, since we are hooking into the Holdable method,
			//and we know it is not being held because we are hooking Release(Vector2 force). So this is just easy.
			
			//Added honey attribute to the HoldableBarrierController
			List<Entity> barriers = self.Scene.Tracker.GetEntities<HoldableBarrier>();
			if (barriers.Count > 0)
			{
				//1: obtain barriers, 2: obtain the barriers[0]'s current Controller, 3: check if that controller has solid on Release true.
				if (((HoldableBarrier)barriers[0]).colorController?.solidOnRelease ?? VivHelperModule.Session.savedHBController?.solidOnRelease ?? VivHelperModule.defaultHBController.solidOnRelease)
					barriers.ForEach(entity => entity.Collidable = true); //Just learned that this is doable on an enumerable. Very cool.
																		  //if the if statement is false, it skips the collidable instruction which just shortens down the code
				orig(self, force);
				barriers.ForEach(entity => entity.Collidable = false); //Thanks to DJMapHelper for that insight.
			}
            else
            {
				orig(self, force);
            }
		}

		#endregion

		public HoldableBarrier(Vector2 position, float width, float height)
		: base(position, width, height, safe: false)
		{
			Add(new HoldableCollider(OnHoldable));
			Collidable = true;
			for (int i = 0; (float)i < base.Width * base.Height / 16f; i++)
			{
				particles.Add(new Vector2(Calc.Random.NextFloat(base.Width - 1f), Calc.Random.NextFloat(base.Height - 1f)));
			}
			
		}

		public HoldableBarrier(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Tracker.GetEntity<HoldableBarrierRenderer>().Track(this);
		}

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
			colorController = scene.Tracker.GetEntity<HoldableBarrierColorController>();
        }

        public override void Removed(Scene scene)
		{
			base.Removed(scene);
			scene.Tracker.GetEntity<HoldableBarrierRenderer>().Untrack(this);
		}

		public void OnHoldable(Holdable h)
        {
			if (h.IsHeld)
			{
				return;
			}
        }
		private void OnReflect()
		{
			Flash = 1f;
			Flashing = true;
			Scene.CollideInto(new Rectangle((int)X, (int)Y - 2, (int)Width, (int)Height + 4), adjacent);
			Scene.CollideInto(new Rectangle((int)X - 2, (int)Y, (int)Width + 4, (int)Height), adjacent);
			foreach (HoldableBarrier barrier in adjacent)
			{
				if (!barrier.Flashing)
				{
					barrier.OnReflect();
				}
			}

			adjacent.Clear();
		}

		public override void Update()
		{
			if(colorController == null)
            {
				colorController = Scene.Tracker.GetEntity<HoldableBarrierColorController>();
			}

			if (Flashing)
			{
				Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
				if (Flash <= 0f)
				{
					Flashing = false;
				}
			}
			else if (solidifyDelay > 0f)
			{
				solidifyDelay -= Engine.DeltaTime;
			}
			else if (Solidify > 0f)
			{
				Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
			}
			int num = speeds.Length;
			int i = 0;
			for (int count = particles.Count; i < count; i++)
			{
				Vector2 value = particles[i] + (colorController?.particleDir ?? VivHelperModule.Session.savedHBController?.particleDir ?? VivHelperModule.defaultHBController.particleDir) * speeds[i % num] * Engine.DeltaTime;
				value.Y = mod(value.Y, Height - 1f); value.X = mod(value.X, Width - 1f); //Don't use this formatting for code. It's bad!
				particles[i] = value;
			}
			base.Update();
		}

		public override void Render()
		{
			Color color = (colorController?.particleColor ?? VivHelperModule.ColorFix(VivHelperModule.Session.savedHBController?.particleColorHex ?? VivHelperModule.defaultHBController.particleColorHex)) * 0.5f;
			foreach (Vector2 particle in particles)
			{
				Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
			}
			if (Flashing)
			{
				Draw.Rect(base.Collider, color * Flash);
			}
		}

		protected float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
