using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;

namespace VivHelper.Triggers {

    [CustomEntity("VivHelper/MusicParamTimeTrigger")]
    public class MusicParamTimeTrigger : Trigger {

        private bool onlyOnce;
        private EntityID id;
        protected string parameter;
        private float fadeTime;
        private float endValue;
        private TriggerActivationCondition triggerActivationCondition;
        private Ease.Easer easer;

        public MusicParamTimeTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
            parameter = data.Attr("Parameter");
            this.id = id;
            onlyOnce = data.Bool("onlyOnce", true);
            triggerActivationCondition = data.Enum("activateType", TriggerActivationCondition.OnEnter);
            endValue = data.Float("endValue");
            fadeTime = data.Float("time", 1);
            easer = data.Easer("easer", Ease.Linear);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (string.IsNullOrWhiteSpace(parameter)) { RemoveSelf(); return; }
        }

        public override void OnEnter(Player player) {
            PlayerIsInside = true;
            if (triggerActivationCondition != TriggerActivationCondition.OnLeave) {
                Audio.CurrentMusicEventInstance.getParameterValue("fade", out float startValue, out _);
                Tween t = Tween.Create(Tween.TweenMode.Oneshot, easer, fadeTime, false);
                t.OnUpdate = (t) => {
                    Audio.CurrentMusicEventInstance.setParameterValue("fade", Calc.LerpClamp(startValue, endValue, t.Eased));
                };
                t.OnComplete = (t) => {
                    Audio.CurrentMusicEventInstance.setParameterValue("fade", 0);
                };
                player.Add(t);
                t.Start();

                if (onlyOnce) {
                    (Scene as Level).Session.DoNotLoad.Add(id);
                }
            }
        }

        public override void OnLeave(Player player) {
            PlayerIsInside = false;
            if (triggerActivationCondition == TriggerActivationCondition.OnLeave) {
                Audio.CurrentMusicEventInstance.getParameterValue(parameter, out float startValue, out _);
                Tween t = Tween.Create(Tween.TweenMode.Oneshot, easer, fadeTime, false);
                t.OnUpdate = (t) => {
                    Audio.CurrentMusicEventInstance.setParameterValue(parameter, Calc.LerpClamp(startValue, endValue, t.Eased));
                };
                t.OnComplete = (t) => {
                    Audio.CurrentMusicEventInstance.setParameterValue(parameter, endValue);
                };
                player.Add(t);
                t.Start();

                if (onlyOnce) {
                    (Scene as Level).Session.DoNotLoad.Add(id);
                }
            }
        }
    }
    [CustomEntity("VivHelper/MusicFadeTimeTrigger")]
    public class MusicFadeOnTimeTrigger : MusicParamTimeTrigger {

        public MusicFadeOnTimeTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id) {
            parameter = "fade";
        }
    }
}
