using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using VivHelper.Entities;

namespace VivHelper.Triggers {
    [CustomEntity("VivHelper/SpriteEntityActor = Load", "xoliEGC/SpriteEntityActor = Load")]
    public class SpriteEntityActor : Trigger {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new SpriteEntityActor(entityData, offset);

        public string tag, anim, overrideAudioEvent;
        public bool animBefore, randomizeFrame, disableAudio;
        public bool flipX, flipY;
        public Vector2? node;
        public float moveTime;
        public Ease.Easer easer;
        public SpriteEntityActor(EntityData data, Vector2 offset) : base(data, offset) {
            tag = data.Attr("tag");
            anim = data.Attr("PlayAnimation");
            randomizeFrame = data.Bool("RandomizeFrame", false);
            disableAudio = data.Bool("DisableAudioPlay", false);
            overrideAudioEvent = data.NoEmptyString("OverrideAudioEvent", null);
            if (overrideAudioEvent != null && overrideAudioEvent.Substring(0, 7) != "event:/")
                overrideAudioEvent = "event:/" + overrideAudioEvent;
            animBefore = data.Bool("AnimateBefore");
            flipX = data.Bool("FlipX");
            flipY = data.Bool("FlipY");
            node = data.FirstNodeNullable(offset);
            if (node != null) {
                moveTime = data.Float("MoveTime", 0.3f);
                easer = data.Easer("Easer", Ease.Linear);
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            SpriteEntity entity = Scene.Tracker.GetFirstEntity<SpriteEntity>((s) => s.tag == tag);
            if (animBefore) {
                entity.PlayAnimation(anim, randomizeFrame, flipX, flipY, disableAudio, overrideAudioEvent);

            }
            if (node != null) {
                if (moveTime > 0f) {
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, moveTime, false);
                    var origEntityPos = entity.Position;
                    tween.OnUpdate = delegate (Tween t) {
                        entity.Position = Vector2.Lerp(origEntityPos, node.Value, t.Eased);
                    };
                    if (!animBefore) {
                        tween.OnComplete = delegate (Tween t) { entity.PlayAnimation(anim, randomizeFrame, flipX, flipY, disableAudio, overrideAudioEvent); };
                    }
                    entity.Add(tween);
                    tween.Start();
                } else {
                    entity.Position = node.Value;
                    if (!animBefore) {
                        entity.PlayAnimation(anim, randomizeFrame, flipX, flipY, disableAudio, overrideAudioEvent);
                    }
                }
            }
        }
    }
}
