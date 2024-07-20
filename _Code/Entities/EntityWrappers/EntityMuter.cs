using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Mono.Cecil;
using FMOD.Studio;
using Celeste.Mod;

namespace VivHelper.Entities {
    public class EntityMuterComponent : Component {
        #region Hooks

        private static Tuple<OpCode, object> baseParametrization = new Tuple<OpCode, object>(OpCodes.Ldarg_0, null);
        public static bool overrideMute = false;
        public static object objPlayingAudio = null;
        internal static List<ILHook> hooks;


        public static FieldInfo SoundSource_instance = typeof(SoundSource).GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance);

        // I genuinely think that this is the dumbest thing in my helper. This takes the cake.
        public static void Load() {
            On.Celeste.SoundSource.Update += SoundSource_Update;
            On.Celeste.Audio.CreateInstance += Audio_CreateInstance;
            hooks = new List<ILHook>();
            HookMethodInfoWithAudioMute(typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic|BindingFlags.Instance), false, null);
        }

        private static EventInstance Audio_CreateInstance(On.Celeste.Audio.orig_CreateInstance orig, string path, Vector2? position) {
            if(path == null) return orig(path, position); // bruh
            string _path = path;
            if (overrideMute) {
                _path = "event:/none";
            } else if (objPlayingAudio is Entity e) {
                var t = e?.Get<EntityMuterComponent>();
                if (t != null && (string.IsNullOrWhiteSpace(t.flag) || ((e.Scene as Level)?.Session?.GetFlag(t.flag) ?? false))) {
                    _path = "event:/none";
                }
            }
            if((VivHelperModule.Session?.AudioChanges?.TryGetValue(_path, out SoundChange sc) ?? false) && sc?.GrabEvent() is Event f) {
                EventInstance i = orig(f.Name == "default" ? _path : f.Name, position);
                f.SetParamsToEvent(i);
                return i;
            } else return orig(_path, position);
            
        } 

        public static void Unload() {
            On.Celeste.SoundSource.Update -= SoundSource_Update;
            On.Celeste.Audio.CreateInstance -= Audio_CreateInstance;
            foreach (ILHook hook in hooks) hook?.Dispose();
            hooks = null;
        }

        public static void HookMethodInfoWithAudioMute(MethodInfo info, bool isCoroutine, Tuple<OpCode, object>[] instrs) {
            MethodInfo i = info;
            if (instrs == null || instrs.Length == 0) {
                if (isCoroutine) {
                    i = info.GetStateMachineTarget();
                    instrs = new Tuple<OpCode, object>[2] {
                        baseParametrization,
                        new Tuple<OpCode, object>(OpCodes.Ldfld, i.DeclaringType.GetField("<>4__this"))
                    };
                } else {
                    instrs = new Tuple<OpCode, object>[1] {
                        baseParametrization
                    };
                }
            } 
            hooks.Add(new ILHook(info, (ctx) => AudioMuteHook(ctx, instrs)));
        }


        private static void AudioMuteHook(ILContext il, Tuple<OpCode, object>[] instrs) {
            ILCursor cursor = new ILCursor(il);
            TypeReference audio = il.Import(typeof(Audio));
            TypeReference eventInstance = il.Import(typeof(EventInstance));
            while (cursor.TryGotoNext(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference mr && mr.DeclaringType.FullName == "Celeste.Audio" && mr.ReturnType.FullName == "FMOD.Studio.EventInstance" && !mr.Name.StartsWith("get_"))) {
                foreach (var t in instrs) {
                    if (t.Item2 is not null)
                        cursor.Emit(t.Item1, t.Item2);
                    else
                        cursor.Emit(t.Item1);
                }
                cursor.Emit(OpCodes.Stsfld, typeof(EntityMuterComponent).GetField("objPlayingAudio", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
                cursor.GotoNext(MoveType.After, i => i.OpCode == OpCodes.Call && i.Operand is MethodReference mr && mr.DeclaringType.FullName == "Celeste.Audio" && mr.ReturnType.FullName == "FMOD.Studio.EventInstance" && !mr.Name.StartsWith("get_"));
                cursor.Emit(OpCodes.Ldnull);
                cursor.Emit(OpCodes.Stsfld, typeof(EntityMuterComponent).GetField("objPlayingAudio", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
            }
        }


        private static void SoundSource_Update(On.Celeste.SoundSource.orig_Update orig, SoundSource self) {
            if (self.Entity == objPlayingAudio) {
                if (self.Playing) {
                    EventInstance i = SoundSource_instance.GetValue(self) as EventInstance;
                    if (i?.getVolume(out _, out float finalVol) == FMOD.RESULT.OK && finalVol != 0f)
                        i.setVolume(0f);
                }
            } else if (!self.Playing) {
                EventInstance i = SoundSource_instance.GetValue(self) as EventInstance;
                if (i?.getVolume(out _, out float finalVol) == FMOD.RESULT.OK && finalVol != 1f)
                    i.setVolume(1f);
            }
            orig(self);
        }
        #endregion


        public string flag;
        public EntityMuterComponent(string flag = null) : base(true, false) {
            this.flag = flag;
        }

        public override void Added(Entity entity) {
            base.Added(entity);
            entity.PreUpdate += Entity_PreUpdate;
            entity.PostUpdate += Entity_PostUpdate;
            foreach(Component c in entity.Components) {
                if(c is PlayerCollider pc) {
                    Action<Player> oc = pc.OnCollide;
                    pc.OnCollide = delegate (Player p) {
                        objPlayingAudio = entity;
                        oc(p);
                        objPlayingAudio = null;
                    };
                }
            }
        }

        private void Entity_PreUpdate(Entity obj) {
            objPlayingAudio = obj;
        }
        private void Entity_PostUpdate(Entity obj) {
            objPlayingAudio = null;
        }
    }

    [CustomEntity("VivHelper/EntityMuter")]
    [Tracked]
    public class EntityMuter : Entity, IPostAwake {
        public List<Type> Types, assignableTypes;
        public bool all;
        public string flag;

        public EntityMuter(EntityData e, Vector2 v) : base(e.Position + v) {
            Collider = new Hitbox(e.Width, e.Height);
            all = e.Bool("all");
            string q = e.Attr("Types", "");
            assignableTypes = new List<Type>();
            Types = new List<Type>();
            Depth = int.MinValue;
            if (!string.IsNullOrEmpty(q)) {
                VivHelper.AppendTypesToList(q, ref Types, ref assignableTypes);
            }
        }

        public void PostAwake(Scene scene) {
            Collidable = true;
            foreach (Entity e in scene.Entities.Where<Entity>((f) => Collide.Check(this, f))) {
                var prev = e.Collidable;
                e.Collidable = true;
                if (Collide.Check(this, e) && VivHelper.MatchTypeFromTypeSet(e.GetType(), Types, assignableTypes)) {
                    e.Add(new EntityMuterComponent());
                    if (!all) {
                        e.Collidable = prev;
                        break;
                    }
                }
                e.Collidable = prev;
            }
            RemoveSelf();
        }
    }
}
