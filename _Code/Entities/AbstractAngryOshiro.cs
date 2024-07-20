using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    public abstract class AbstractAngryOshiro : Entity {


        protected StateMachine state;

        protected VertexLight light;
        protected Sprite oshiroSprite;
        protected Sprite lightningSprite;

        protected Shaker shaker;

        protected bool leaving;

        /// anxietyAngle refers to the angle that the oshiro should be expected to dash in
        public AbstractAngryOshiro(Vector2 position,
                                   Sprite oshiroSprite,
                                   Sprite lightningSprite,
                                   bool shake,
                                   bool light,
                                   float anxietyAngle) : base(position) {
            this.oshiroSprite = oshiroSprite;
            this.lightningSprite = lightningSprite;
            if(shake) Add(shaker = new Shaker(on: false));
            if(light) Add(this.light = new VertexLight(Color.White, 1f, 32, 64));

            float angle = Calc.WrapAngle(anxietyAngle);
            if (angle == -Consts.PI || angle == Consts.PI)
                Distort.AnxietyOrigin = new Vector2(0f, 0.5f);
            else {
                // I know there's a better way to do this. It is 4am and I do not care.
                float a = Engine.Viewport.Width;
                float b = Engine.Viewport.Height;
                float c = (float)Math.Cos(angle);
                float s = (float) Math.Sin(angle);
                float d = a * s / (2 * c);
                float val = (float) Math.Atan(Engine.Viewport.Height/Engine.Viewport.Width); //arc
                Vector2 res;
                if (Math.Abs(angle) <= val)
                    res = new Vector2(a, d + b/2);
                else if (Math.Abs(angle) >= Consts.PI - val)
                    res = new Vector2(0, b/2 - d);
                else
                    res = new Vector2((b * c / (2*Math.Abs(s))) + a / 2, (b * s / (2 * Math.Abs(s))) + b/2);
                Distort.AnxietyOrigin = res / new Vector2(a, b);

            }
        }







    }
}
