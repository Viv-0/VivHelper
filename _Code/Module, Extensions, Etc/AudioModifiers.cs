using Celeste;
using FMOD.Studio;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VivHelper {
    public struct AudioParam {
        public string Name;
        public string Flag;
        public bool FlagInvert;
        public float IfFlag;
        public float? Normal;
    }

    public class Event {
        public string Name;
        public AudioParam[] Params;
        public Event(string n, params AudioParam[] Params) {
            Name = n;
            this.Params = Params;
        }

        public void SetParamsToEvent(EventInstance instance) {
            if (Params == null)
                return;
            Level l = Engine.Scene as Level;
            foreach (var param in Params) {
                if (l?.Session is Session s && !string.IsNullOrWhiteSpace(param.Flag) && (s.GetFlag(param.Flag) != param.FlagInvert)) {
                    instance.setParameterValue(param.Name, param.IfFlag);
                } else if (param.Normal.HasValue) {
                    instance.setParameterValue(param.Name, param.Normal.Value);
                }
            }
        }
    }

    public class SoundReplace : SoundChange {
        private static Regex ternary = new Regex("(\\w+)?\\s?\\?\\s*(\\d+)\\s*:\\s*(?:(null)|(\\d*))");

        private Event DefaultEvent;

        private Dictionary<string, Event> flagEvents;

        public SoundReplace(EntityData data) {
            DefaultEvent = null;
            flagEvents = new Dictionary<string, Event>();
            Construct(data);
        }

        private void Construct(EntityData data) {
            string text = data.Attr("flag");
            string text2 = data.Attr("replacementEvent");
            if (string.IsNullOrWhiteSpace(text2)) {
                return;
            }
            Event @event = new Event(text2);
            string text3 = data.Attr("customParams");
            if (!string.IsNullOrWhiteSpace(text3)) {
                List<AudioParam> list = new List<AudioParam>();
                string[] array = text3.Split(';');
                foreach (string text4 in array) {
                    int num = text4.IndexOf(':');
                    if (num == -1) {
                        continue;
                    }
                    string name = text4.Substring(0, num);
                    string text5 = text4.Substring(num + 1);
                    if (float.TryParse(text5, out var result)) {
                        list.Add(new AudioParam {
                            Name = name,
                            Normal = result
                        });
                        continue;
                    }
                    Match match = ternary.Match(text5);
                    if (!match.Success || !float.TryParse(match.Captures[1].Value, out var result2)) {
                        continue;
                    }
                    AudioParam audioParam = default(AudioParam);
                    audioParam.Name = name;
                    audioParam.IfFlag = result2;
                    AudioParam item = audioParam;
                    string value = match.Captures[0].Value;
                    if (value[0] == '!') {
                        item.FlagInvert = true;
                        item.Flag = value.Substring(1);
                    }
                    if (!(match.Captures[2].Value != "null")) {
                        if (float.TryParse(match.Captures[2].Value, out var result3)) {
                            item.Normal = result3;
                        }
                        list.Add(item);
                    }
                }
                @event.Params = list.ToArray();
            }
            if (string.IsNullOrWhiteSpace(text)) {
                DefaultEvent = @event;
            } else {
                flagEvents.Add(text, @event);
            }
        }

        public override void AddOrChangeFromEntityData(EntityData data) {
            Construct(data);
        }

        public override Event GrabEvent() {
            Session session = ((Engine.Scene is Level level) ? level.Session : null);
            if (session != null && flagEvents != null) {
                foreach (KeyValuePair<string, Event> flagEvent in flagEvents) {
                    string text = flagEvent.Key;
                    bool flag = false;
                    if (text[0] == '!') {
                        flag = true;
                        text = text.Substring(1);
                    }
                    if (session.GetFlag(text) != flag) {
                        return flagEvent.Value;
                    }
                }
            }
            return DefaultEvent;
        }
    }

    public class SoundMute : SoundChange {

        public SoundMute(EntityData data) {
            flag = data.Attr("flag");
        }

        private string flag;
        public override Event GrabEvent() {
            Level l = Engine.Scene as Level;
            if (l?.Session is Session s && !string.IsNullOrWhiteSpace(flag)) {
                bool b = false;
                string f = flag;
                if (flag[0] == '!') {
                    b = true;
                    f = flag.Substring(1);
                }
                return s.GetFlag(f) != b ? noneEvent : defaultEvent; // if the flag is true, it mutes
            }

            return noneEvent;
        }

        public override void AddOrChangeFromEntityData(EntityData data) {
            var f = data.Attr("flag");
            if (!string.IsNullOrWhiteSpace(f)) {
                flag = f;
            }
        }
    }

    public abstract class SoundChange {
        protected static Event noneEvent = new Event("event:/none", null);
        protected static Event defaultEvent = new Event("default", null);
        public abstract Event GrabEvent();
        public abstract void AddOrChangeFromEntityData(EntityData data);

    }
}
