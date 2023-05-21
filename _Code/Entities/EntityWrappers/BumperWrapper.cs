using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod;
using MonoMod.Utils;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/BumperWrapper")]
    public class BumperWrapper : Entity {


        private float Scale;
        private bool RemoveBloom;
        private bool RemoveLight;
        private bool RemoveWobble;
        private bool AttachToSolid;

        private Session.CoreModes CoreModeChanger;
        private ExplodeLaunchModifier.BumperModifierTypes bumperLaunchModifier;
        public float ExplodeMultiplier;
        private bool SetToNormalState;
        private float newDashCooldown;
        private int newDepth;
        private int setDashes = -2;
        private float setStamina = -2;
        public ExplodeLaunchModifier.RestrictBoost bumperBoost;
        //node stuff
        private Vector2[] nodes;
        private bool useNodes;
        private bool LoopType; // false = Loop, true = BackAndForth
        private bool multiple;
        private int num = 1;
        private bool pause;
        private float MoveTime, WaitTime, RespawnTime;
        private Ease.Easer EaseType;

        private string typeName;

        private EntityData entityData;
        private Vector2 Offset;

        DynamicData dynBumper;
        private Entity tiedEntity;
        private Action<Player> oldOnCollide;
        private Vector2 bumperAnchor;
        private string AnchorName;

        private bool firstUpdate;

        public BumperWrapper(EntityData data, Vector2 offset) : base(data.Position + offset) {
            entityData = data;
            Offset = offset;
            base.Collider = new Hitbox(4, 4, 2, 2);
            typeName = data.Attr("TypeName", "Celeste.Bumper");
            RemoveBloom = data.Bool("RemoveBloom", false);
            RemoveLight = data.Bool("RemoveLight", false);
            RemoveWobble = data.Bool("RemoveWobble", false);
            bumperLaunchModifier = data.Enum<ExplodeLaunchModifier.BumperModifierTypes>("BumperLaunchType", ExplodeLaunchModifier.BumperModifierTypes.IgnoreAll);
            ExplodeMultiplier = data.Float("ExplodeStrengthMultiplier", 1f);
            ExplodeMultiplier = ExplodeMultiplier == 0f ? 1f : ExplodeMultiplier;
            newDashCooldown = Math.Max(0f, data.Float("DashCooldown", 0.2f));
            SetToNormalState = data.Bool("NormalStateOnEnd");
            setDashes = Math.Max(-2, data.Int("SetDashes", -1));
            setStamina = Math.Max(-2, data.Int("SetStamina", -1));
            bumperBoost = data.BetterEnum<ExplodeLaunchModifier.RestrictBoost>("BumperBoost", ExplodeLaunchModifier.RestrictBoost.Default);
            AnchorName = data.Attr("AnchorName", "anchor");
            if (string.IsNullOrWhiteSpace(AnchorName))
                AnchorName = "anchor";
            CoreModeChanger = data.Enum<Session.CoreModes>("CoreMode", Session.CoreModes.None);
            RespawnTime = data.Float("RespawnTime", -1f);
            Scale = Math.Max(2, data.Int("Scale", 12)) / 12f;
            nodes = data.NodesWithPosition(offset);
            useNodes = data.Bool("useNodes", false);
            AttachToSolid = data.Bool("attachToSolid", false);
            if (nodes.Length > 1) {
                if (useNodes) {
                    LoopType = data.Bool("LoopType", true);
                    multiple = data.Bool("Multiple");

                    if (multiple) {
                        num = Math.Max(1, data.Int("Number", -1));
                    }
                    EaseType = multiple ? Ease.Linear : VivHelper.TryGetEaser(data.Attr("EaseType", "CubeInOut"), out EaseType) ? EaseType : Ease.CubeInOut;
                    pause = data.Bool("Pause", false);
                    MoveTime = data.Float("MoveTime", 1.81818f);
                    WaitTime = data.Float("DelayTime", 0.0f);
                }
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            bool b = false;

            foreach (PlayerCollider tie in CollideAllByComponent<PlayerCollider>()) {
                if (CheckEntity(tie.Entity.GetType(), tie.Entity, tie)) {
                    b = true;
                    break;
                }
            }
            if (!b) {
                RemoveSelf();
            }
        }

        private bool CheckEntity(Type type, Entity entity, PlayerCollider pc) {
            //This is where the magic happens.
            // Bumper check consists of PlayerCollider with Hitbox of 12 radius, and a Vector2 anchor. This isn't perfect, but realistically its what is the bare minimum
            // for Bumpers, since I'm not confident in MethodBody reflection to get me more information, since the methods can vary quite a lot in terms of IL.
            // (Plus how the fuck do you read Action delegates dynamically through IL) The fact that you have to match the TypeName also helps a ton.
            if (entity == null) {
                Logger.Log("VivHelperVerbose", "Entity is null!");
                return false;
            }
            if (type == null) {
                Logger.Log("VivHelperVerbose", "Type is null???");
                return false;
            }
            if (type.ToString() != typeName) {
                return false;
            }
            if (pc.OnCollide == null || (pc.Collider?.GetType() ?? pc.Entity.Collider.GetType()) != typeof(Circle)) {
                return false;
            }
            if (((pc.Collider ?? pc.Entity.Collider) as Circle).Radius != 12) {
                return false;
            }
            //stupid fix is stupid, apparently something broke in DynamicData, and since I'm only using it for Anchor this is fine.
            if (type.ToString() == "Celeste.Mod.MaxHelpingHand.Entities.RotatingBumper" || type.ToString() == "Celeste.Mod.MaxHelpingHand.Entities.MultiNodeBumper")
                type = VivHelper.GetType("Celeste.Mod.MaxHelpingHand.Entities.BumperNotCoreMode", true);

            dynBumper = new DynamicData(type, entity);
            Vector2 bA = new Vector2();
            if (!dynBumper.TryGet(AnchorName, out bA)) //We first want to check if the new Bumper is replacing the old private values with this check
            {
                if (type.IsSubclassOf(typeof(Bumper))) // Then, if not, we see if it extends from the class Bumper (if it is Bumper then this shouldn't be a problem, as it would pull to true on the first check)
                {
                    dynBumper = new DynamicData(typeof(Bumper), entity);
                    if (!dynBumper.TryGet<Vector2>(AnchorName, out bA)) {
                        throw new MissingFieldException($"The class you tried to create a bumper didn't have a valid Vector2 {AnchorName}, please report what entity you tried to use this Bumper Wrapper with to Viv on Discord @Viv#1113");
                    }
                } else {
                    throw new MissingFieldException($"The class you tried to create a bumper didn't have a valid Vector2 {AnchorName}, please report what entity you tried to use this Bumper Wrapper with to Viv on Discord @Viv#1113");
                }
            }
            if (!dynBumper.TryGet<float>("respawnTimer", out float _)) {
                throw new MissingFieldException("The class you tried to create a bumper from, didn't have a \"respawnTimer\" field.");
            }

            Circle c = entity.Collider as Circle;
            entity.Collider = pc.Collider = new Circle(2 * (int) (6f * Scale), c.Position.X * Scale, c.Position.Y * Scale); //There's a solid reason for why I did the entity Collide radius this way.
            oldOnCollide = pc.OnCollide;
            entity.Remove(entity.Get<PlayerCollider>());
            entity.Add(new PlayerCollider(ReplacementOnPlayer));
            entity.Depth = newDepth;
            if (entity.Get<CoreModeListener>() != null && CoreModeChanger != Session.CoreModes.None) {
                entity.Get<CoreModeListener>().OnChange(CoreModeChanger);
                entity.Remove(entity.Get<CoreModeListener>());
            }

            foreach (Sprite s in entity.Components.GetAll<Sprite>()) {
                s.Scale = Vector2.One * Scale;
            }
            if (RemoveBloom)
                entity.Remove(entity.Components.GetAll<BloomPoint>().ToArray());
            if (RemoveLight)
                entity.Remove(entity.Components.GetAll<VertexLight>().ToArray());
            if (RemoveWobble && entity.Get<SineWave>() != null) {
                entity.Get<SineWave>().Frequency = 0;
                entity.Get<SineWave>().Reset();
            }
            if (!useNodes && AttachToSolid && nodes.Length > 1) {
                StaticMover sm = new StaticMover();
                sm.SolidChecker = (s) => sm.Entity.CollideCheck(s, nodes[1]);
                sm.OnMove = (v) => {
                    if (entity == sm.Entity) {
                        Vector2 s = dynBumper.Get<Vector2>(AnchorName);
                        dynBumper.Set(AnchorName, (Vector2) s + v);
                    }
                };
                entity.Add(sm);
            }
            entity.Add(new BumperWrapperDebugRenderModifier(bumperLaunchModifier));
            return true;
        }



        private void ReplacementOnPlayer(Player player) {
            if (dynBumper.Get<float>("respawnTimer") > 0f)
                return;
            ExplodeLaunchModifier.bumperWrapperType = bumperLaunchModifier;
            int oldDashes = player.Dashes;
            float oldStamina = player.Stamina;
            ExplodeLaunchModifier.DisableFreeze = true;
            ExplodeLaunchModifier.DetectFreeze = false;
            oldOnCollide(player);
            ExplodeLaunchModifier.DisableFreeze = false;
            if (ExplodeLaunchModifier.DetectFreeze) {
                ExplodeLaunchModifier.ExplodeLaunchMaster(player, Position, false, false, this);
            }
            var oldCooldown = DynamicData.For(player).Get<float>("dashCooldownTimer");

            ExplodeLaunchModifier.bumperWrapperType = 0;
            switch (setDashes) {
                case -2:
                    player.Dashes = oldDashes;
                    break;
                case -1:
                    break;
                default:
                    player.Dashes = setDashes;
                    break;
            }
            switch (setStamina) {
                case -2:
                    player.Stamina = oldStamina;
                    break;
                case -1:
                    break;
                default:
                    player.Stamina = setStamina;
                    break;
            }
            player.Speed *= ExplodeMultiplier;
            DynamicData.For(player).Set("dashCooldownTimer", newDashCooldown < 0 ? oldCooldown : newDashCooldown);
            if (SetToNormalState)
                player.StateMachine.State = 0;
            if (RespawnTime >= 0f)
                dynBumper.Set("respawnTimer", RespawnTime);

        }

        private static int[] Loop(int count, bool LoopType) {
            List<int> ret = new List<int>();
            if (LoopType) {
                ret.Add(count - 1);
                for (int a = count - 2; a > 0; a--) { ret.Insert(ret.Count, a); ret.Insert(0, a); }
                ret.Insert(0, 0);
            } else {
                ret.AddRange(Enumerable.Range(0, count));
            }
            return ret.ToArray();
        }
    }

    public class BumperWrapperDebugRenderModifier : Component {
        public ExplodeLaunchModifier.BumperModifierTypes bumperModifier;
        private Circle circle => Entity?.Collider as Circle;

        public BumperWrapperDebugRenderModifier(ExplodeLaunchModifier.BumperModifierTypes b) : base(true, false) {
            bumperModifier = b;
        }

        public bool RenderNewDebug(Camera camera) {

            Vector2 AbsPos = circle.AbsolutePosition;
            float rad = circle.Radius;
            Color c = Entity.Collidable ? VivHelperModule.EntityDebugColor ?? Color.Red : VivHelperModule.EntityDebugColor * 0.5f ?? Color.DarkRed;
            Draw.Circle(AbsPos, rad, c, 4);
            switch (bumperModifier) {
                case ExplodeLaunchModifier.BumperModifierTypes.Cardinal:
                    Draw.Line(AbsPos - new Vector2(0.70711f, 0.70711f) * rad, AbsPos + new Vector2(0.70711f, 0.70711f) * rad, c);
                    Draw.Line(AbsPos - new Vector2(0.70711f, -0.70711f) * rad, AbsPos + new Vector2(0.70711f, -0.70711f) * rad, c);
                    break;
                case ExplodeLaunchModifier.BumperModifierTypes.Diagonal:
                    Draw.Line(AbsPos - Vector2.UnitX * rad, AbsPos + Vector2.UnitX * rad, c);
                    Draw.Line(AbsPos - Vector2.UnitY * rad, AbsPos + Vector2.UnitY * rad, c);
                    break;
                case ExplodeLaunchModifier.BumperModifierTypes.EightWay:
                    Draw.Line(AbsPos - new Vector2(0.70711f, 0.70711f) * rad, AbsPos + new Vector2(0.70711f, 0.70711f) * rad, c);
                    Draw.Line(AbsPos - new Vector2(0.70711f, -0.70711f) * rad, AbsPos + new Vector2(0.70711f, -0.70711f) * rad, c);
                    Draw.Line(AbsPos - Vector2.UnitX * rad, AbsPos + Vector2.UnitX * rad, c);
                    Draw.Line(AbsPos - Vector2.UnitY * rad, AbsPos + Vector2.UnitY * rad, c);
                    break;
                case ExplodeLaunchModifier.BumperModifierTypes.Alt4way:
                    Draw.Line(AbsPos + Vector2.UnitX.Rotate(1.0472f) * rad, AbsPos, c);
                    Draw.Line(AbsPos + Vector2.UnitX.Rotate(-0.6545f) * rad, AbsPos, c);
                    Draw.Line(AbsPos + Vector2.UnitX.Rotate(2.0944f) * rad, AbsPos, c);
                    Draw.Line(AbsPos + Vector2.UnitX.Rotate(-2.4871f) * rad, AbsPos, c);
                    Draw.Point(AbsPos, Extensions.ColorCopy(c, 1f));
                    break;
            }
            return true;
        }
    }
}
