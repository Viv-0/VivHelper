using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.VivHelper;
using MonoMod.Utils;
using System.Reflection;
using VivHelper.Module__Extensions__Etc;
using System.Collections;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/InLevelTeleporter")]
    public class InLevelTeleporter : Entity {

        private static ParticleType P_Glow;

        public enum Directions { Up = 3, Down = 1, Left = 2, Right = 0 }

        public InLevelTeleporter pair;
        public Directions Direction { get; private set; }
        public string[] Flags;
        public float speedOut;
        public int length;
        public bool enabled;
        public bool cameraWarp;
        public int numOfUses;
        public int useCount;
        public string audio;
        private bool allActors; //Experimental. Why did I agree to do this?
        private bool AlwaysRetainSpeed;
        private bool legacy;
        private bool center, OHMod;
        private float cooldown;
        private float CooldownTime = 0.05f;
        private Vector2 particlePosition;
        private Vector2 particleScope;
        private float particleDir;
        private Color particleColor;


        public InLevelTeleporter(EntityData data, Vector2 offset) : this(data.Enum<Directions>("dir1", Directions.Up), data.Attr("flags1"), data.Float("sO1"), data.Int("l", 16), data.Bool("eO1"), data.Bool("allActors"), data.Bool("cW"), data.Int("NumberOfUses1", int.MaxValue), data.Attr("Audio", ""), data.Position + offset, data.Bool("legacy", true), data.Bool("center"), data.Bool("OutbackHelperMod"), data.Attr("Path"), data.Color("Color", Color.Transparent), data.Float("CooldownTime"),data.Bool("AlwaysRetainSpeed", true)) {
            Vector2 pos2 = data.Nodes[0] + offset;
            pair = new InLevelTeleporter(data.Enum<Directions>("dir2", Directions.Up), data.Attr("flags2"), data.Float("sO2"), data.Int("l", 8), data.Bool("eO2"), data.Bool("allActors"), data.Bool("cW"), data.Int("NumberOfUses2", int.MaxValue), data.Attr("Audio", ""), pos2, data.Bool("legacy", true), data.Bool("center"), data.Bool("OutbackHelperMod"), data.Attr("Path"), data.Color("Color", Color.Transparent),data.Float("CooldownTime"), data.Bool("AlwaysRetainSpeed", true));
        }
        //You shouldn't ever use this in your code if you choose to inherit this class. This constructor is to make sure that the base of the portal is made, and then the other constructor builds both portals.
        public InLevelTeleporter(Directions dir, string flags, float sOut, int length, bool eO, bool aA, bool cW, int use, string audio, Vector2 position, bool legacyTP, bool center, bool OOHMod, string spritePath, Color particleColor, float cooldowntime, bool alwaysRetainSpeed) : base(position) {
            Direction = dir;
            Flags = flags.Split(',');
            speedOut = sOut;
            this.length = length;
            this.audio = audio;
            AlwaysRetainSpeed = alwaysRetainSpeed;
            cameraWarp = cW;
            numOfUses = use;
            if (numOfUses == -1) { numOfUses = int.MaxValue; } else if (numOfUses < 1) { numOfUses = 1; }
            switch (Direction) {
                case Directions.Left:
                    base.Collider = new Hitbox(16f, length, -16f);
                    break;
                case Directions.Down:
                    base.Collider = new Hitbox(length, 16f);
                    break;
                case Directions.Right:
                    base.Collider = new Hitbox(16f, length);
                    break;
                default:
                    base.Collider = new Hitbox(length, 16f, y: -16f);
                    break;
            }
            if (!eO)
                Add(new PlayerCollider(OnPlayer, base.Collider));

            enabled = false;
            useCount = 0;
            allActors = aA;
            legacy = legacyTP;
            this.center = center;
            OHMod = OOHMod;
            CooldownTime = cooldowntime;
            if (!string.IsNullOrWhiteSpace(spritePath) && particleColor != Color.Transparent) { // If the path isn't null, then we want to draw things "standardly"
                float rot = Consts.PIover2 * (int) dir;
                particlePosition = Position + Vector2.UnitX.Rotate(rot) * 15f + Vector2.UnitY.Rotate(rot) * (length-1) / ((int) dir % 3 == 0 ? 2 : -2);
                particleScope = Vector2.UnitY.Rotate(rot) * (length-2) / 2;
                particleDir = -rot;
                this.particleColor = particleColor;
                GFX.Game.PushFallback(null);
                MTexture q = GFX.Game[spritePath];
                int lines = length / 8;
                if (q != null) { // Single-image variation
                    int num = q.Width / 8;
                    for (int i = 0; i < lines; i++) {
                        int yTilePosition;
                        if (i == 0) {
                            yTilePosition = 0;
                        } else if (i == lines - 1) {
                            yTilePosition = 2;
                        } else {
                            yTilePosition = 1;
                        }
                        Image image = new Image(q.GetSubtexture(0, yTilePosition * 8, 8, 8));
                        image.Position = Vector2.UnitX.Rotate(rot) * 16 + Vector2.UnitY.Rotate(rot) * (-8*i + ((int)dir%3==0 ? length : 0));
                        image.Rotation = rot + Consts.PI;
                        image.Color = particleColor;
                        Add(image);
                    }
                } else {
                    List<MTexture> set = GFX.Game.GetAtlasSubtextures(spritePath);
                    if (set.Count != 0) {
                        List<MTexture>[] r = new List<MTexture>[3] { new List<MTexture>(), new List<MTexture>(), new List<MTexture>() };

                        foreach (MTexture t in set) {
                            r[0].Add(t.GetSubtexture(0, 0, 8, 8));
                            r[1].Add(t.GetSubtexture(0, 8, 8, 8));
                            r[2].Add(t.GetSubtexture(0, 16, 8, 8));
                        }
                        MTexture[][] s = new MTexture[3][] { r[0].ToArray(), r[1].ToArray(), r[2].ToArray() };
                        for (int i = 0; i < lines; i++) {
                            int yTilePosition;
                            if (i == 0) {
                                yTilePosition = 0;
                            } else if (i == lines - 1) {
                                yTilePosition = 2;
                            } else {
                                yTilePosition = 1;
                            }
                            Sprite image = new Sprite(GFX.Game, "");
                            image.Justify = Vector2.Zero;
                            image.AddLoop("idle", 0.06f, s[yTilePosition]);
                            image.Position = Vector2.UnitX.Rotate(rot) * 16 + Vector2.UnitY.Rotate(rot) * (-8 * i + ((int) dir % 3 == 0 ? length : 0));
                            image.Rotation = rot + Consts.PI;
                            image.Color = particleColor;
                            Add(image);
                        }
                    }
                }
                GFX.Game.PopFallback();
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (P_Glow == null)
                P_Glow = new ParticleType(LightBeam.P_Glow) {
                    SpeedMin = 8f,
                    SpeedMax = 10f,
                    LifeMin = 0.4f,
                    LifeMax = 1.1f
                };
            if (pair != null) { pair.pair = this; scene.Add(pair); } 
            else RemoveSelf();
        }

        public override void Update() {
            base.Update();
            Collidable = enabled = VivHelperModule.OldGetFlags(base.Scene as Level, Flags, "and");
            if(cooldown > 0) {
                cooldown -= Engine.DeltaTime;
            }
            if(particleColor != Color.Transparent &&  Scene.OnInterval(0.1f)) {
                SceneAs<Level>().Particles.Emit(P_Glow, (int)Math.Ceiling(length/24f), particlePosition, particleScope, particleColor * 0.3f, particleDir);
            }
            if (allActors) {
                foreach (Actor actor in CollideAll<Actor>().Where((Entity a) => !(a is Player))) {

                    switch (Direction) {
                        case Directions.Right:
                            if (actor.Left > Left && actor.CenterY <= Bottom && actor.CenterY >= Top) {

                                pair.TeleportEntity(actor, actor.Top - Top, Direction);

                            }
                            break;
                        case Directions.Left:
                            if (actor.Right < Right && actor.CenterY <= Bottom && actor.CenterY >= Top) {
                                pair.TeleportEntity(actor, Bottom - actor.Bottom, Direction);
                            }
                            break;
                        case Directions.Up:
                            if (actor.Bottom < Bottom && actor.CenterX <= Right && actor.CenterX >= Left) {
                                pair.TeleportEntity(actor, Right - actor.Right, Direction);
                            }
                            break;
                        case Directions.Down:
                            if (actor.Top > Top && actor.CenterX <= Right && actor.CenterX >= Left) {

                                pair.TeleportEntity(actor, actor.Left - Left, Direction);
                            }
                            break;
                    }
                }
            }
        }

        protected virtual void OnPlayer(Player player) {
            if (useCount < numOfUses && cooldown <= 0) {
                useCount++;
                switch (Direction) {
                    case Directions.Right:
                        if (player.Left > Left && player.CenterY <= Bottom && player.CenterY >= Top) {
                            pair.TeleportPlayer(player, player.Top - Top, player.Speed, Direction);

                        }
                        break;
                    case Directions.Left:
                        if (player.Right < Right && player.CenterY <= Bottom && player.CenterY >= Top) {
                            pair.TeleportPlayer(player, Bottom - player.Bottom, player.Speed, Direction);
                        }
                        break;
                    case Directions.Up:
                        if (player.Bottom < Bottom && player.CenterX <= Right && player.CenterX >= Left) {
                            pair.TeleportPlayer(player, Right - player.Right, player.Speed, Direction);
                        }
                        break;
                    case Directions.Down:
                        if (player.Top > Top && player.CenterX <= Right && player.CenterX >= Left) {
                            pair.TeleportPlayer(player, player.Left - Left, player.Speed, Direction);
                        }
                        break;
                }
            }
        }
        protected void TeleportPlayer(Player player, float distance, Vector2 speed, Directions d) {

            Level l = SceneAs<Level>();
            if (l == null) { return; }
            Vector2 cameraDifferential = l.Camera.Position - player.CameraTarget;
            if (legacy) {
                switch (Direction) {
                    case Directions.Right:
                        if (center)
                            player.CenterRight = new Vector2(Left, CenterY);
                        else
                            player.BottomRight = new Vector2(Left, Bottom - distance);
                        break;
                    case Directions.Left:
                        if (center)
                            player.CenterLeft = new Vector2(Right, CenterY);
                        else
                            player.TopLeft = new Vector2(Right, Top + distance);
                        break;
                    case Directions.Up:
                        if (center)
                            player.BottomCenter = new Vector2(CenterX, Bottom);
                        else
                            player.BottomLeft = new Vector2(Left + distance, Bottom);
                        break;
                    case Directions.Down:
                        if (center)
                            player.TopCenter = new Vector2(CenterX, Top);
                        else
                            player.TopRight = new Vector2(Right - distance, Top);
                        break;
                }
                player.Position = player.Position.Ceiling();
                player.CleanUpTriggers();
                if (WiggleOutFail(player as Entity)) { player.Die(Vector2.Zero); return; }
                player.Speed = SpeedMod(player, d);
                if (cameraWarp) {
                    CameraShit(player.Scene as Level, player, cameraDifferential);
                }
                cooldown = CooldownTime;
                if (!string.IsNullOrEmpty(audio))
                    Audio.Play(audio, player.Position);
                if(cooldown < 0) {
                    cooldown = float.MaxValue;
                    Add(new Coroutine(DisablePlayerCollision(player)));
                }
            } else {

                l.OnEndOfFrame += delegate {
                    switch (Direction) {
                        case Directions.Right:
                            player.BottomRight = new Vector2(Left, Bottom - distance);
                            if(AlwaysRetainSpeed && player.Speed.X == 0)
                                player.Speed.X = (float) VivHelper.player_wallSpeedRetained.GetValue(player);
                            break;
                        case Directions.Left:
                            player.TopLeft = new Vector2(Right, Top + distance);
                            if (AlwaysRetainSpeed && player.Speed.X == 0)
                                player.Speed.X = (float) VivHelper.player_wallSpeedRetained.GetValue(player);
                            break;
                        case Directions.Up:
                            player.BottomLeft = new Vector2(Left + distance, Bottom);
                            break;
                        case Directions.Down:
                            player.TopRight = new Vector2(Right - distance, Top);
                            break;
                    }
                    if (WiggleOutFail(player)) { player.Die(Vector2.Zero); return; }
                    
                    player.Speed = SpeedMod(player, d);
                    if (cameraWarp) {
                        CameraShit(player.Scene as Level, player, cameraDifferential);
                    }
                    cooldown = CooldownTime;
                    if (!string.IsNullOrEmpty(audio))
                        Audio.Play(audio, player.Position);
                    if (cooldown < 0) {
                        cooldown = float.MaxValue;
                        Add(new Coroutine(DisablePlayerCollision(player)));
                    }
                };
            }
        }

        private IEnumerator DisablePlayerCollision(Player player) {
            while(Collide.Check(this, player)) {
                yield return null;
            }
            cooldown = 0.02f;
        }

        private Vector2 SpeedMod(Player player, Directions d) //d == direction, Direction == portal.direction (if reading in OutbackHelper)
        {
            Vector2 speed = player.Speed;
            if (OHMod) {
                //Direction is in the form of "right->down->left->up" because positive Y is downwards, and Pi/2 * Direction equals the angle in code.
                if (d == Directions.Up) // If the entry direction is Up, cap the speed @ 150 speed
                    speed.Y = Math.Max(speed.Y, 150f);
                //This code is an improvement on the Matrix transform found in OutbackHelper
                float anglediff = (Direction - d) * Consts.PIover2; //the difference in angle is what the speed gets rotated by, with
                speed = speed.Rotate(anglediff);
                speed *= -1f;
                //Dash Cancel action
                if (player.StateMachine.State == Player.StDash) {
                    player.StateMachine.State = Player.StNormal;
                }
                //Optimized code
                if (player.StateMachine.State != 5) {
                    if (d == Directions.Up) { //If the entry direction is Down, multiply speed by 1.5 on both axes (WHAT)
                        speed *= 1.5f;
                        if ((int) Direction % 2 == 0) { // if the exit direction is left or right, modify speed for some reason? 
                            speed.Y -= 150f; // I presume this is to make it feel "floatier" so you dont die without seeing it. That being said, why is it -=.
                        }
                    } else if (d == Directions.Up) { //If the entry direction is Up, multiply speed by 1.5 on both axes (WHAT)
                        speed *= 1.5f;
                    }
                }
                if (Direction == Directions.Down) { // If the exit Direction is Upwards, cap the Y speed at -150 so you dont yeet to space
                    speed.Y = Math.Min(player.Speed.Y, -150f);
                }
            } else {
                // NORMAL ASS PHYSICS
                float anglediff = (Direction - d) * Consts.PIover2;
                speed = speed.Rotate(anglediff);
                speed *= -1f;
            }
            return speed + new Vector2(speedOut * (float) Math.Cos(Calc.Angle(speed)), speedOut * (float) Math.Sin(Calc.Angle(speed)));
        }

        private void CameraShit(Level level, Player player, Vector2 cameraDifferential) {
            //Camera magic, this is such a dumb strategy but it works
            foreach (Trigger trigger in level.Tracker.GetEntities<Trigger>().Where(t => Collide.Check(player, t) && t.GetType().Name.IndexOf("camera", StringComparison.OrdinalIgnoreCase) > -1)) {
                VivHelper.PlayerTriggerCheck(player, trigger);
            }
            level.Camera.Position = player.CameraTarget + cameraDifferential;
        }

        private bool WiggleOutFail(Entity entity) {
            if (entity.CollideCheck<Solid>()) {
                for (int i = 1; i <= 5; i++) {
                    for (int j = -1; j <= 1; j += 2) {
                        for (int k = 1; k <= 5; k++) {
                            for (int l = -1; l <= 1; l += 2) {
                                Vector2 vector = new Vector2(i * j, k * l);
                                if (!entity.CollideCheck<Solid>(Position + vector)) {
                                    entity.Position += vector;
                                    return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public void TeleportEntity(Entity entity, float distance, Directions d) {

            switch (Direction) {
                case Directions.Right:
                    entity.BottomRight = new Vector2(Left, Bottom - distance);
                    break;
                case Directions.Left:
                    entity.TopLeft = new Vector2(Right, Top + distance);
                    break;
                case Directions.Up:
                    entity.BottomLeft = new Vector2(Left + distance, Bottom);
                    break;
                case Directions.Down:
                    entity.TopRight = new Vector2(Right - distance, Top);
                    break;
            }
            if (WiggleOutFail(entity)) { entity.RemoveSelf(); return; }

        }

        public Vector2 TeleportEntityWithSpeed(Entity entity, float distance, Vector2 speed, Directions d) {
            float anglediff = (Direction - d) * Consts.PIover2;
            speed = speed.Rotate(anglediff);
            speed *= -1f;
            switch (Direction) {
                case Directions.Right:
                    entity.BottomRight = new Vector2(Left, Bottom - distance);
                    break;
                case Directions.Left:
                    entity.TopLeft = new Vector2(Right, Top + distance);
                    break;
                case Directions.Up:
                    entity.BottomLeft = new Vector2(Left + distance, Bottom);
                    break;
                case Directions.Down:
                    entity.TopRight = new Vector2(Right - distance, Top);
                    break;
            }
            if (!string.IsNullOrEmpty(audio))
                Audio.Play(audio, entity.Position);
            if (WiggleOutFail(entity)) { entity.RemoveSelf(); return Vector2.Zero; }
            return speed + new Vector2(speedOut * (float) Math.Cos(Calc.Angle(speed)), speedOut * (float) Math.Sin(Calc.Angle(speed)));
        }

    }
}
