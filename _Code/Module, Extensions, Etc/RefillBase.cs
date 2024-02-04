using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;

namespace VivHelper {
    public abstract class RefillBase : Entity {
        protected Sprite sprite;

	    protected Sprite flash;

	    protected Image outline;

	    protected Wiggler wiggler;

	    protected BloomPoint bloom;

	    protected VertexLight light;

	    protected Level level;

	    protected SineWave sine;

	    protected bool oneUse, spriteDrawOutline;

	    protected ParticleType p_shatter = Refill.P_Shatter;

        protected ParticleType p_regen = Refill.P_Regen;

        protected ParticleType p_glow = Refill.P_Glow;

	    protected float respawnTimer;

	    public RefillBase(Vector2 position, bool oneUse, bool spriteDrawOutline)
		    : base(position)
	    {
		    base.Collider = new Hitbox(16f, 16f, -8f, -8f);
		    this.oneUse = oneUse;
            this.spriteDrawOutline = spriteDrawOutline;
            Add(new PlayerCollider(OnPlayer));
		    Add(wiggler = Wiggler.Create(1f, 4f, delegate(float v)
		    {
                Vector2 b = Vector2.One * (1f + v * 0.2f);
                foreach(Image i in Components.OfType<Image>()) {
                    i.Scale = b;
                }
		    }));
		    Add(new MirrorReflection());
		    Add(bloom = new BloomPoint(0.8f, 16f));
		    Add(light = new VertexLight(Color.White, 1f, 16, 48));
		    Add(sine = new SineWave(0.6f, 0f));
		    sine.Randomize();
		    UpdateY();
		    base.Depth = -100;
	    }

        public RefillBase(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse", false), data.Bool("spriteDrawOutline", true)) { }

	    public override void Added(Scene scene)
	    {
		    base.Added(scene);
		    level = SceneAs<Level>();
            if (sprite != null) {
                sprite.CenterOrigin();
                sprite.Visible = true;
            }
            if (outline != null) {
                outline.CenterOrigin();
                outline.Visible = false;
            }
        }

	    public override void Update()
	    {
		    base.Update();
		    if (respawnTimer > 0f)
		    {
			    respawnTimer -= Engine.DeltaTime;
			    if (respawnTimer <= 0f)
			    {
				    Respawn();
			    }
		    }
		    else if (base.Scene.OnInterval(0.1f))
		    {
			    level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
            }
            if (sprite != null)
                sprite.Position = Position;
            if (outline != null)
                outline.Position = Position;
            UpdateY();
            if(light != null) light.Alpha = Calc.Approach(light.Alpha, (sprite?.Visible ?? false) ? 1f : 0f, 4f * Engine.DeltaTime);
		    if(bloom != null) bloom.Alpha = light.Alpha * 0.8f;
		    if (base.Scene.OnInterval(2f) && (sprite?.Visible ?? false) && flash != null)
		    {
			    flash.Play("flash", restart: true);
			    flash.Visible = true;
		    }
	    }

	    protected virtual void Respawn()
	    {
		    if (!Collidable)
		    {
			    Collidable = true;
                if(sprite != null) sprite.Visible = true;
			    if(outline != null) outline.Visible = false;
			    base.Depth = -100;
			    wiggler.Start();
			    Audio.Play("event:/game/general/diamond_return", Position);
			    level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
		    }
	    }

	    protected virtual void UpdateY()
	    {
            float num = sine.Value * 2f;
            if (flash != null)
                flash.Y = num;
            if (sprite != null)
                sprite.Y = Position.Y + num;
            if (bloom != null)
                bloom.Y = num;
	    }

	    public override void Render()
	    {
            if (outline?.Visible ?? false)
                outline.Render();
            if (sprite?.Visible ?? false) {
                if (spriteDrawOutline)
                    sprite.DrawOutline();
                sprite.Render();
            }

            
		    base.Render();
	    }

        protected abstract void OnPlayer(Player player);
    }
}
