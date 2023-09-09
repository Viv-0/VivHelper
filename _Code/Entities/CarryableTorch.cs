using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.VivHelper;
using Microsoft.Xna.Framework;
using Monocle;
using VivHelper;

namespace Celeste.Mod.Torchlight {
    [Tracked]
    [CustomEntity("VivHelper/FollowTorch")]
    public class CarryableTorch : Entity {
        public static readonly Dictionary<string, Color[]> colortypes = new Dictionary<string, Color[]>
        {
            {"Default", new Color[] {Color.LightYellow, Color.Lerp(Color.Yellow, Color.LightYellow, .9f), Color.Lerp(Color.LightYellow, Color.White, .1f)} },
            {"Green", new Color[] {Color.LightGreen, Color.Lerp(Color.Green, Color.LightGreen, .9f), Color.Lerp(Color.LightGreen, Color.White, .1f) } },
            {"Red", new Color[] { Calc.HexToColor("e44040"), Calc.HexToColor("f76868"), Calc.HexToColor("fa1818") } },
            {"Blue", new Color[] {Color.SkyBlue, Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, .7f), Color.Lerp(Color.SkyBlue, Color.LightSkyBlue, .4f) } },
            {"Purple", new Color[] {Color.Lerp(Color.Purple, Color.White, .4f), Color.Lerp(Color.Violet, Color.Purple, .85f), Color.Lavender } },
            {"Orange", new Color[] {Color.Orange, Color.Lerp(Color.Red, Color.Orange, .9f), Color.Lerp(Color.Orange, Color.Yellow, .2f) } },
            {"Sunset", new Color[] {Color.Lerp(Calc.HexToColor("e07597"), Color.White, .25f), Calc.HexToColor("b62956"), Calc.HexToColor("eeb5c7") } },
            {"Gray", new Color[] { Calc.HexToColor("d8d8d8"), Calc.HexToColor("c1c1c1"), Calc.HexToColor("eaeaea") } } };
        public EntityID id;
        private readonly Follower follower;
        public Sprite sprite;
        private Color[] color;
        public VertexLight vLight;
        //CustomVars
        public int r1 = 48;
        public int r2 = 64;
        public float alpha = 1f;
        public string roomName;
        private Level level;
        private Vector2 start;
        private Player player;

        public CarryableTorch(EntityData data, Vector2 offset) : base(data.Position + offset) {

            r1 = data.Int("FadePoint", 48);
            r2 = data.Int("Radius", 64);
            alpha = data.Float("Alpha", 1f);
            color = colortypes[data.Attr("Color", "Default")];
            Position = (start = data.Position + offset);
            id = new EntityID(data.Level.Name, data.ID);
            roomName = data.Level.Name;
            Collider = new Hitbox(9, 12, -4.5f, -6f);
            Add(follower = new Follower(id));
            follower.FollowDelay = data.Float("followDelay", 0.2f);
            Add(new PlayerCollider(OnPlayer));
            Add(sprite = VivHelperModule.spriteBank.Create(data.Attr("Color", "Default") + "Torch"));
            sprite.CenterOrigin();

        }
        public override void Added(Scene scene) {
            if (alpha > 0) { 
                Add(vLight = new VertexLight(color[0], alpha, r1, r2));
            }
            base.Added(scene);
            Add(new TransitionListener {
                OnOut = delegate {
                    if (follower.HasLeader) { follower.Leader.LoseFollower(follower); RemoveSelf(); }
                }
            });
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            level = SceneAs<Level>();
            vLight.InSolidAlphaMultiplier = 0f;
            if (level != null)
                roomName = level.Session.Level;
        }

        public override void Update() {
            base.Update();
            vLight.Position = Position.Round() - Position;
        }

        private void OnPlayer(Player player) {
            this.player = player;
            Audio.Play("event:/env/local/campfire_start", Position);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            player.Leader.GainFollower(follower);
            Collidable = false;
            base.Depth = -1000000;
        }

    }
}
