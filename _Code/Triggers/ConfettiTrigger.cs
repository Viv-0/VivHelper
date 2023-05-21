using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/ConfettiTrigger")]
    public class ConfettiTrigger : Trigger {
        public Vector2 pos;
        public bool onlyOnce, permanent;
        private EntityID id;
        private float cooldownTimer;
        private float cooldown;

        public ConfettiTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
            this.id = id;
            pos = data.Nodes[0] + offset;
            onlyOnce = data.Bool("onlyOnce", true);
            permanent = data.Bool("permanent", false);
            cooldownTimer = data.Float("RepeatOnCycle");
            cooldown = 0;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            Audio.Play("event:/game/07_summit/checkpoint_confetti", pos);
            base.Scene.Add(new SummitCheckpoint.ConfettiRenderer(pos));
            if (cooldownTimer > 0f)
                cooldown = cooldownTimer;
            if (onlyOnce) { RemoveSelf(); }
            if (permanent) { SceneAs<Level>().Session.DoNotLoad.Add(id); RemoveSelf(); }
        }

        public override void OnStay(Player player) {
            base.OnStay(player);
            if (!onlyOnce && cooldownTimer > 0f) {
                cooldown -= Engine.DeltaTime;
                if (cooldown <= 0f) {
                    Audio.Play("event:/game/07_summit/checkpoint_confetti", pos);
                    base.Scene.Add(new SummitCheckpoint.ConfettiRenderer(pos));
                    cooldown = cooldownTimer;
                }
            }

        }
    }
}
