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
        public SoundReplace(EntityData data) {
            DefaultEvent = null;
            flagEvents = new();
            Construct(data);
        }
        //Parameters are formatted as:
        // NAME: #### ; NAME2: ####
        // or
        // NAME: FLAG ? ### : ### ; NAME2 ...
        // $1{space}?{space}$2{space}:{space}$3
        // $1 = flag
        // $2 = number value
        // $3 = null *or* number value
        private static Regex ternary = new Regex(@"(\w+)?\s?\?\s*(\d+)\s*:\s*(?:(null)|(\d*))");

        private void Construct(EntityData data) {
            string flag = data.Attr("flag");

            string replace = data.Attr("replacementEvent");

            if (string.IsNullOrWhiteSpace(replace))
                return;
            Event _event = new Event(replace);

            string a = data.Attr("customParams");
            if (!string.IsNullOrWhiteSpace(a)) {
                List<AudioParam> @params = new List<AudioParam>();
                foreach (string b in a.Split(';')) {
                    int c = b.IndexOf(':');
                    if (c == -1) continue;
                    string name = b.Substring(0, c);
                    string detail = b.Substring(c+1);
                    if(float.TryParse(detail, out float normal)) {
                        @params.Add(new AudioParam { Name = name, Normal = normal });
                        continue;
                    } else {
                        Match m = ternary.Match(detail);
                        if (m.Success && float.TryParse(m.Captures[1].Value, out var ifflag)) {
                            AudioParam p = new AudioParam { Name = name, IfFlag = ifflag };
                            string _flag = m.Captures[0].Value;
                            if (_flag[0] == '!') {
                                p.FlagInvert = true;
                                p.Flag = _flag.Substring(1);
                            }
                            if (m.Captures[2].Value != "null") continue;
                            else if(float.TryParse(m.Captures[2].Value, out float norm)) p.Normal = norm;
                            @params.Add(p);
                        }
                        continue;
                    }
                }
                _event.Params = @params.ToArray();
            }
            if (string.IsNullOrWhiteSpace(flag)) {
                DefaultEvent = _event;
            } else {
                flagEvents.Add(flag, _event);
            }
        }


        private Event DefaultEvent;
        private Dictionary<string, Event> flagEvents;

        public override void AddOrChangeFromEntityData(EntityData data) {
            Construct(data);
        }
        public override Event GrabEvent() {
            Level l = Engine.Scene as Level;
            if (l?.Session is Session s && flagEvents != null) {
                foreach (var t in flagEvents) {
                    var f = t.Key;
                    var b = false;
                    if (f[0] == '!') {
                        b = true;
                        f = f.Substring(1);
                    }
                    if (s.GetFlag(f) != b) {
                        return t.Value;
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
