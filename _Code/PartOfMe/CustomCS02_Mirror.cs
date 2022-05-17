using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;

namespace VivTestMod.PartOfMe
{
	public class Custom_CS02_Mirror : CutsceneEntity
	{
		private Player player;

		private WarpDreamMirror mirror;

		private float playerEndX;

		private int direction = 1;
		private bool lookUp;

		private SoundSource sfx;

		public Custom_CS02_Mirror(Player player, WarpDreamMirror mirror, bool lookUp)
		{
			this.player = player;
			this.mirror = mirror;
			this.lookUp = lookUp;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			Add(sfx = new SoundSource());
			sfx.Position = mirror.Center;
			sfx.Play("event:/music/lvl2/dreamblock_sting_pt1");
			direction = Math.Sign(player.X - mirror.X);
			player.StateMachine.State = 11;
			playerEndX = 8 * direction;
			yield return 1f;
			player.Facing = (Facings)(-direction);
			yield return 0.4f;
			yield return player.DummyRunTo(mirror.X + playerEndX);
			yield return 0.5f;
			yield return level.ZoomTo(mirror.Position - level.Camera.Position - Vector2.UnitY * 24f, 2f, 1f);
			yield return 0.5f;
			yield return mirror.BreakRoutine(direction);
			player.DummyAutoAnimate = false;
			player.Sprite.Play("lookUp");
			if (lookUp)
			{
				Vector2 from = level.Camera.Position;
				Vector2 to = level.Camera.Position + new Vector2(0f, -80f);
				for (float ease = 0f; ease < 1f; ease += Engine.DeltaTime * 1.2f)
				{
					level.Camera.Position = from + (to - from) * Ease.CubeInOut(ease);
					yield return null;
				}
				Add(new Coroutine(ZoomBack()));
				using (List<Entity>.Enumerator enumerator = base.Scene.Tracker.GetEntities<DreamBlock>().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						DreamBlock dreamBlock = (DreamBlock)enumerator.Current;
						yield return dreamBlock.Activate();
					}
				}
			}
			yield return 0.5f;
			EndCutscene(level);
		}

		private IEnumerator ZoomBack()
		{
			yield return 1.2f;
			yield return Level.ZoomBack(3f);
		}

		public override void OnEnd(Level level)
		{
			mirror.Broken(WasSkipped);
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			if (entity != null)
			{
				entity.StateMachine.State = 0;
				entity.DummyAutoAnimate = true;
				entity.Speed = Vector2.Zero;
				entity.X = mirror.X + playerEndX;
				if (direction != 0)
				{
					entity.Facing = (Facings)(-direction);
				}
				else
				{
					entity.Facing = Facings.Right;
				}
			}
			foreach (DreamBlock entity2 in base.Scene.Tracker.GetEntities<DreamBlock>())
			{
				entity2.ActivateNoRoutine();
			}
			level.ResetZoom();
			level.Session.Inventory.DreamDash = true;
			level.Session.Audio.Music.Event = "event:/music/lvl2/mirror";
			level.Session.Audio.Apply();
		}
	}
}
