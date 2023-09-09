using Celeste;
using Monocle;
using System;
using System.Collections;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using static VivHelper.VivHelper;
using MonoMod.Utils;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using VivHelper.Entities;
using FMOD.Studio;
using YamlDotNet.Core.Tokens;
using System.Linq.Expressions;
using YamlDotNet.Helpers;

namespace VivHelper {
    public static class Extensions {
        //Thanks to JaThePlayer for the class allowing for StateMachine States to be added.
        private static FieldInfo StateMachine_begins = typeof(StateMachine).GetField("begins", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo StateMachine_updates = typeof(StateMachine).GetField("updates", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo StateMachine_ends = typeof(StateMachine).GetField("ends", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo StateMachine_coroutines = typeof(StateMachine).GetField("coroutines", BindingFlags.Instance | BindingFlags.NonPublic);

        public static int AddState(this StateMachine machine, Func<int> onUpdate, Func<IEnumerator> coroutine = null, Action begin = null, Action end = null) {
            Action[] begins = (Action[]) StateMachine_begins.GetValue(machine);
            Func<int>[] updates = (Func<int>[]) StateMachine_updates.GetValue(machine);
            Action[] ends = (Action[]) StateMachine_ends.GetValue(machine);
            Func<IEnumerator>[] coroutines = (Func<IEnumerator>[]) StateMachine_coroutines.GetValue(machine);
            int nextIndex = begins.Length;
            Array.Resize(ref begins, begins.Length + 1);
            Array.Resize(ref updates, begins.Length + 1);
            Array.Resize(ref ends, begins.Length + 1);
            Array.Resize(ref coroutines, coroutines.Length + 1);
            StateMachine_begins.SetValue(machine, begins);
            StateMachine_updates.SetValue(machine, updates);
            StateMachine_ends.SetValue(machine, ends);
            StateMachine_coroutines.SetValue(machine, coroutines);
            machine.SetCallbacks(nextIndex, onUpdate, coroutine, begin, end);
            return nextIndex;
        }

        public static int AddState(this StateMachine machine, Func<Player, int> onUpdate, Func<Player, IEnumerator> coroutine = null, Action<Player> begin = null, Action<Player> end = null) {
            Action[] begins = (Action[]) StateMachine_begins.GetValue(machine);
            Func<int>[] updates = (Func<int>[]) StateMachine_updates.GetValue(machine);
            Action[] ends = (Action[]) StateMachine_ends.GetValue(machine);
            Func<IEnumerator>[] coroutines = (Func<IEnumerator>[]) StateMachine_coroutines.GetValue(machine);
            int nextIndex = begins.Length;
            Array.Resize(ref begins, begins.Length + 1);
            Array.Resize(ref updates, begins.Length + 1);
            Array.Resize(ref ends, begins.Length + 1);
            Array.Resize(ref coroutines, coroutines.Length + 1);
            StateMachine_begins.SetValue(machine, begins);
            StateMachine_updates.SetValue(machine, updates);
            StateMachine_ends.SetValue(machine, ends);
            StateMachine_coroutines.SetValue(machine, coroutines);
            Func<IEnumerator> _coroutine = null;
            if (coroutine != null) _coroutine = () => coroutine(machine.Entity as Player);
            machine.SetCallbacks(nextIndex, () => onUpdate(machine.Entity as Player), _coroutine, () => begin(machine.Entity as Player), () => end(machine.Entity as Player));
            return nextIndex;
        }

        public static List<Entity> FindAll(this EntityList self, params Type[] types) {
            List<Type> _types = types.ToList();
            List<Entity> list = new List<Entity>();
            foreach (Entity entity in VivHelper.getListOfEntities(self)) //getListOfEntities is faster. In the future I'm gonna make a FastFieldInfo but for now we have single use cases
            {
                if (_types.Contains(entity.GetType())) {
                    list.Add(entity);
                }
            }
            return list;
        }


        public static T GetFirstEntity<T>(this Tracker self, Predicate<T> predicate) where T : Entity {
            if (!self.Entities.ContainsKey(typeof(T)))
                return null;
            foreach (T obj in self.Entities[typeof(T)]) {
                if (predicate(obj))
                    return obj;
            }
            return null;
        }

        public static bool TryGetEntity<T>(this Tracker self, out T entity) where T : Entity {
            entity = null;
            if (!self.Entities.ContainsKey(typeof(T)) || self.Entities[typeof(T)].Count == 0)
                return false;
            entity = self.Entities[typeof(T)][0] as T;
            return true;
        }

        public static bool TryGetEntity(this Tracker self, Type t, out Entity entity) {
            entity = null;
            if (!self.Entities.ContainsKey(t) || self.Entities[t].Count == 0)
                return false;
            entity = self.Entities[t][0];
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetEntities<T>(this Tracker self, out List<Entity> entities) {
            return TryGetEntities(self, typeof(T), out entities);
        }

        public static bool TryGetEntities(this Tracker self, Type type, out List<Entity> entities) {
            entities = null;
            if (self.Entities.TryGetValue(type, out entities))
                return true;
            return false;
        }

        public static T GetNearestEntity<T>(this Tracker self, Vector2 nearestTo, out float distSq) where T : Entity {
            List<Entity> entities = self.GetEntities<T>();
            T val = null;
            distSq = 0f;
            foreach (T item in entities) {
                float num2 = Vector2.DistanceSquared(nearestTo, item.Position);
                if (val == null || num2 < distSq) {
                    val = item;
                    distSq = num2;
                }
            }
            return val;
        }

        public static bool StringIfNotEmpty(this EntityData data, string key, out string value) {
            value = data.Attr(key, null);
            return !string.IsNullOrEmpty(value);
        }
        public static bool StringIfNotEmpty(this EntityData data, string key, string defaultValue, out string value) {
            value = data.Attr(key, defaultValue);
            return !string.IsNullOrEmpty(value);
        }

        public static string NoEmptyString(this EntityData self, string key, string defaultValue = null) {
            string q = self.Attr(key, defaultValue);
            if (string.IsNullOrEmpty(q))
                return null;
            return q;
        }

        public static string NoEmptyStringReplace(this EntityData self, string key, string defaultValue = null) {
            string q = self.Attr(key, defaultValue);
            if (string.IsNullOrEmpty(q))
                return defaultValue;
            return q;
        }

        public static string ThrowOnEmptyAttr(this EntityData self, string key) {
            string q = self.Attr(key, null);
            if (string.IsNullOrEmpty(q))
                throw new InvalidPropertyException($"Property \"{key}\" was null or empty for entity of type {self.Name} at Position {self.Position}, when it cannot be for this to function.");
            return q;
        }

        public static Ease.Easer Easer(this EntityData self, string key, Ease.Easer defaultValue) {
            string q = self.Attr(key, null);
            if (string.IsNullOrEmpty(q))
                return defaultValue;
            if (!VivHelper.TryGetEaser(q, out Ease.Easer ease)) { return defaultValue; }
            return ease;
        }

        public static Color ColorCopy(Color color, int alpha) {
            return new Color(color.R, color.G, color.B, Calc.Clamp(alpha, 0, 255));
        }

        public static Color ColorCopy(Color color, float alpha) {
            return new Color(color.R, color.G, color.B, (byte) Calc.Clamp(255 * alpha, 0, 255));
        }

        public static void AddOrAddToSolidModifierComponent(this Solid entity, SolidModifierComponent smc, out SolidModifierComponent smc2) {
            if (entity.Get<SolidModifierComponent>() == null) {
                smc2 = smc;
                entity.Add(smc);
                return;
            }
            SolidModifierComponent main = entity.Get<SolidModifierComponent>();
            if (main.ContactMod + smc.ContactMod > 2)
                main.ContactMod = 3;
            else if (main.ContactMod != smc.ContactMod)
                main.ContactMod = Math.Max(main.ContactMod, smc.ContactMod);
            // If A has default and B doesn't, prioritize B (A|B)
            // If A has a specific integer value (positive) and B has a behavior integer value (negative), prioritize the negative
            // If A and B have specific integer values (positive), choose the greater of the two
            // If A and B have behavior integer values (negative), behavior is A|B
            if (main.CornerBoostBlock == 0) {
                main.CornerBoostBlock = smc.CornerBoostBlock; // functionally 0|B === B
            } else if (smc.CornerBoostBlock != 0) {
                if (main.CornerBoostBlock < 0) { // if A is behavioral
                    if (smc.CornerBoostBlock < 0) {
                        main.CornerBoostBlock = main.CornerBoostBlock | smc.CornerBoostBlock; // A | B
                    } // else do nothing, because A is already prioritized over B
                } else { // if A is specific integer value (positive)
                    if (smc.CornerBoostBlock > 0) { // if both A and B are specific integer values, choose the greater of the two
                        main.CornerBoostBlock = Math.Max(main.CornerBoostBlock, smc.CornerBoostBlock); // choose the greater leniency
                    } else { // if A is specific integer value and B is behavior integer value, prioritize B
                        main.CornerBoostBlock = smc.CornerBoostBlock;
                    }
                }
            }
            smc2 = main;
        }

        public static void AddOrAddToSolidModifierComponent(this Solid entity, SolidModifierComponent smc) {
            AddOrAddToSolidModifierComponent(entity, smc, out SolidModifierComponent _);
        }

        /// <summary>
        /// Returns a consistent "Random" value given 3 numeric inputs and an array of objects to choose between
        /// </summary>
        /// <param name="_a">An integer, ideally between 1 and 30000</param>
        /// <param name="_b">An integer, ideally between 1 and 30000</param>
        /// <param name="_c">An integer, ideally between 1 and 30000</param>
        public static T ConsistentChooser<T>(int _a, int _b, int _c, params T[] ts) {
            //Uses the Wichmann-Hill algorithm, we just kinda hope that the numbers don't go beyond 30000 on the x or y axis (note: this is basically 3750 tiles, and for level call it's fineeeee because of how I'm doing it)
            int a = mod(171 * _a, 30269);
            int b = mod(172 * _b, 30307);
            int c = mod(170 * _c, 30323);
            return ts[(int) (mod(a / 30269.0 + b / 30307.0 + c / 30323.0, 1.0)) * ts.Length];
        }

        public static T ConsistentChooser<T>(int _a, int _b, int _c, List<T> ts) {
            //Uses the Wichmann-Hill algorithm, we just kinda hope that the numbers don't go beyond 30000 on the x or y axis (note: this is basically 3750 tiles, and for level call it's fineeeee because of how I'm doing it)
            int a = mod(171 * _a, 30269);
            int b = mod(172 * _b, 30307);
            int c = mod(170 * _c, 30323);
            return ts[(int) (mod(a / 30269.0 + b / 30307.0 + c / 30323.0, 1.0)) * ts.Count];
        }

        private static FieldInfo chooser_choices = typeof(Chooser<string>).GetField("choices", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool CompareChooser<T>(Chooser<T> a, Chooser<T> b) {
            if (a.Choices.Count != b.Choices.Count)
                return false;
            foreach (Chooser<T>.Choice _c in a.Choices) {
                if (b.Choices.FirstOrDefault(e => e.Value.Equals(_c.Value) && e.Weight == _c.Weight) == null)
                    return false;
            }
            return true;
        }

        public static void ExtendAction(this Collider collider, float x) {
            if (collider is ColliderList) {

            } else if (collider is Circle || collider is Hitbox) {
                collider.Top -= x;
                collider.Bottom += x;
                collider.Left -= x;
                collider.Right += x;
            } else
                throw new Exception("Extensions of the collider type are not implemented!");
        }

        public static bool CollideEdge(this Entity self, Entity other) {
            if (Collide.Check(self, other))
                return false;
            if (self.Collider is Grid)
                throw new Exception("Collisions against the collider type are not implemented!");
            self.Collider.ExtendAction(1);
            bool b = Collide.Check(self, other);
            self.Collider.ExtendAction(-1);
            return b;
        }

        /// <summary>
        /// Gets an existing Entity from the Scene or Adds the Entity specified to the Scene. Entity must be Tracked.
        /// </summary>
        public static T GetOrAddEntity<T>(Scene self, T entity) where T : Entity {
            if (TryGetEntity<T>(self.Tracker, out T t))
                return t;
            self.Add(entity);
            return entity;

        }

        private static FieldInfo platform_movementCounter = typeof(Celeste.Platform).GetField("movementCounter", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool MoveHCollideEntities(this Celeste.Platform self, float moveH, List<Type>[] types, Action<Vector2, Vector2, Entity> onCollide = null) {
            if (Engine.DeltaTime == 0f) {
                self.LiftSpeed.X = 0f;
            } else {
                self.LiftSpeed.X = moveH / Engine.DeltaTime;
            }
            Vector2 movementCounter = (Vector2) platform_movementCounter.GetValue(self);
            movementCounter.X += moveH;
            int num = (int) Math.Round(movementCounter.X);
            if (num != 0) {
                movementCounter.X -= num;
                platform_movementCounter.SetValue(self, movementCounter);
                return self.MoveHExactCollideEntities(types, num, onCollide);
            }
            return false;
        }

        public static bool MoveVCollideEntities(this Celeste.Platform self, float moveV, List<Type>[] types, Action<Vector2, Vector2, Entity> onCollide = null) {
            if (Engine.DeltaTime == 0f) {
                self.LiftSpeed.X = 0f;
            } else {
                self.LiftSpeed.X = moveV / Engine.DeltaTime;
            }
            Vector2 movementCounter = (Vector2) platform_movementCounter.GetValue(self);
            movementCounter.Y += moveV;
            int num = (int) Math.Round(movementCounter.X);
            if (num != 0) {
                movementCounter.Y -= num;
                platform_movementCounter.SetValue(self, movementCounter);
                return self.MoveVExactCollideEntities(types, num, onCollide);
            }
            return false;
        }

        ///<summary>
        ///Runs through the same code as MoveHExactCollideSolids except on Collision with any Entity it determines if the Entity's type is within the typeSet parameters
        ///</summary>
        public static bool MoveHExactCollideEntities(this Celeste.Platform self, List<Type>[] types, int moveH, Action<Vector2, Vector2, Entity> onCollide = null) {
            float x = self.X;
            int num = Math.Sign(moveH);
            int num2 = 0;
            Entity entity = null;
            while (moveH != 0) {
                Dictionary<Type, List<Entity>> entities = self.Scene.Tracker.Entities;
                bool b = false;
                foreach (Type key in entities.Keys) {
                    if (VivHelper.MatchTypeFromTypeSet(key, types[0], types[1]))
                        continue;
                    entity = Collide.First(self, entities[key]);
                    if (entity != null) {
                        b = true;
                        break;
                    }
                }
                if (b)
                    break; //if this is true, break out of the while loop, or, break twice
                num2 += num;
                moveH -= num;
                self.X += num;
            }
            self.X = x;
            self.MoveHExact(num2);
            if (entity != null) {
                onCollide?.Invoke(Vector2.UnitY * num, Vector2.UnitY * num2, entity);
            }
            return entity != null;
        }

        public static bool MoveVExactCollideEntities(this Celeste.Platform self, List<Type>[] types, int moveV, Action<Vector2, Vector2, Entity> onCollide = null) {
            float y = self.Y;
            int num = Math.Sign(moveV);
            int num2 = 0;
            Entity entity = null;
            while (moveV != 0) {
                Dictionary<Type, List<Entity>> entities = self.Scene.Tracker.Entities;
                bool b = false;
                foreach (Type key in entities.Keys) {
                    if (!(types[0].Contains(key) || types[1].Any(t => key.IsAssignableFrom(t)))) { continue; }
                    entity = Collide.First(self, entities[key]);
                    if (entity != null) {
                        b = true;
                        break;
                    }
                }
                if (b)
                    break; //if this is true, break out of the while loop, or, break twice
                num2 += num;
                moveV -= num;
                self.Y += num;
            }
            self.Y = y;
            self.MoveVExact(num2);
            if (entity != null) {
                onCollide?.Invoke(Vector2.UnitY * num, Vector2.UnitY * num2, entity);
            }
            return entity != null;
        }

        public static MTexture MTexture(this EntityData self, string key, MTexture defaultValue = null, Atlas atlas = null) {
            if (atlas == null)
                atlas = GFX.Game;
            if (!self.StringIfNotEmpty(key, out string value)) {
                return defaultValue;
            }
            atlas.PushFallback(null);
            MTexture texture = atlas[key];
            atlas.PopFallback();
            return texture;
        }

        public static Color Color(this EntityData self, string key, Color? defaultValue = null, List<string> defaultColorParametrization = null) {
            Color _defaultValue = Microsoft.Xna.Framework.Color.White;
            if (defaultValue != null)
                _defaultValue = defaultValue.Value;
            if (defaultColorParametrization?.Count > 0 && defaultColorParametrization.Contains(key))
                return _defaultValue;
            else if (self.StringIfNotEmpty(key, out string val)) {
                return ColorFixWithNull(val) ?? _defaultValue;
            } else
                return _defaultValue;
        }
        public static Color ColorPlus(this EntityData self, string key, Color? defaultValue = null, params KeyValuePair<string, Color>[] overrides) {
            Color _defaultValue = Microsoft.Xna.Framework.Color.White;
            if (defaultValue != null)
                _defaultValue = defaultValue.Value;
            if (self.StringIfNotEmpty(key, out string val)) {
                if (overrides != null) {
                    foreach (KeyValuePair<string, Color> pair in overrides)
                        if (pair.Key == val)
                            return pair.Value;
                }
                return ColorFixWithNull(val) ?? _defaultValue;
            } else
                return _defaultValue;
        }
        public static Color? ColorOrNull(this EntityData self, string key, Color? defaultValue = null) {
            if (self.StringIfNotEmpty(key, out string val)) {
                return ColorFixWithNull(val);
            } else
                return defaultValue;
        }
        public static Color? ColorOrNullPlus(this EntityData self, string key, Color? defaultValue = null, params KeyValuePair<string, Color?>[] overrides) {
            if (self.StringIfNotEmpty(key, out string val)) {
                if (overrides != null) {
                    foreach (KeyValuePair<string, Color?> pair in overrides)
                        if (pair.Key == val)
                            return pair.Value;
                }
                return ColorFixWithNull(val);
            } else
                return defaultValue;
        }

        public static T BetterEnum<T>(this EntityData self, string key, T defaultValue) where T : struct {
            if (!self.Has(key))
                return defaultValue;
            if (self.Values[key] is int i) {
                var vals = Enum.GetValues(typeof(T));
                for (int h = 0; h < (vals?.Length ?? 0); h++) { int g = (int) vals.GetValue(h); if (g == i) return (T) Enum.Parse(typeof(T), (string) Enum.GetNames(typeof(T)).GetValue(h)); }
                return defaultValue;
            } else {
                if (Enum.TryParse<T>((string) self.Values[key], ignoreCase: true, out var result))
                    return result;
                return defaultValue;
            }
        }

        public static EntityData ConjoinEntityData(EntityData priority, EntityData appender) {
            //All values presented here are cloned already excluding values.
            EntityData priorCopy = new EntityData {
                ID = priority.ID,
                Height = priority.Height,
                Level = priority.Level,
                Name = priority.Name,
                Nodes = priority.Nodes,
                Origin = priority.Origin,
                Position = priority.Position,
                Values = new Dictionary<string, object>(priority.Values),
                Width = priority.Width
            };
            foreach (KeyValuePair<string, object> kvp in appender.Values) {
                if (!priorCopy.Values.TryGetValue(kvp.Key, out object v) || (v is string s && string.IsNullOrEmpty(s))) {
                    priorCopy.Values[kvp.Key] = kvp.Value;
                }
            }
            return priorCopy;
        }

        public static bool CollideCheck<T>(this Entity self, out T t) where T : Entity {
            t = null;
            if (!self.Scene.Tracker.Entities.TryGetValue(typeof(T), out List<Entity> q))
                return false;
            foreach (Entity b in q) {
                if (Collide.Check(self, b)) { t = (T) b; return true; }
            }
            return false;
        }

        public static bool CollideCheck<T>(this Entity self, Vector2 at, out T t) where T : Entity {
            t = null;
            if (!self.Scene.Tracker.Entities.TryGetValue(typeof(T), out List<Entity> q))
                return false;
            foreach (Entity b in q) {
                if (Collide.Check(self, b, at)) { t = (T) b; return true; }
            }
            return false;
        }

        public static List<Entity> CollideAll(this Entity self, Type type) {
            if (type == null || !self.Scene.Tracker.Entities.TryGetValue(type, out List<Entity> q))
                return new List<Entity>(0);
            return Collide.All(self, q);
        }
        public static List<Entity> CollideAll(this Entity self, Type type, Vector2 at) {
            if (type == null || !self.Scene.Tracker.Entities.TryGetValue(type, out List<Entity> q))
                return new List<Entity>(0);
            return Collide.All(self, q, at);
        }

        public static List<Entity> CollideAll(this Entity self, Func<Type, bool> predicate) {
            if (predicate == null || self.Scene.Tracker?.Entities == null)
                return new List<Entity>(0);
            List<Type> c = new List<Type>();
            List<Entity> into = new List<Entity>();
            foreach (KeyValuePair<Type, List<Entity>> kvp in self.Scene.Tracker.Entities) {
                if (predicate(kvp.Key)) {
                    Collide.All(self, kvp.Value, into);
                }
            }
            return into;
        }
        public static List<Entity> CollideAll(this Entity self, Func<Type, bool> predicate, Vector2 at) {
            if (predicate == null || self.Scene.Tracker?.Entities == null)
                return new List<Entity>(0);
            List<Type> c = new List<Type>();
            List<Entity> into = new List<Entity>();
            foreach (KeyValuePair<Type, List<Entity>> kvp in self.Scene.Tracker.Entities) {
                if (predicate(kvp.Key)) {
                    Collide.All(self, kvp.Value, into, at);
                }
            }
            return into;
        }


        public static bool CollideAnyWhere<T>(this Entity self, Predicate<T> predicate, out T firstMatch) where T : Entity {
            firstMatch = null;
            if (!self.Scene.Tracker.Entities.TryGetValue(typeof(T), out List<Entity> entities))
                return false;
            foreach (Entity e in entities) {
                T t = e as T;
                if (Collide.Check(self, t) && predicate(t)) {
                    firstMatch = t;
                    return true;
                }
            }
            return false;
        }
        public static bool CollideAnyWhere<T>(this Entity self, Predicate<T> predicate) where T : Entity {
            List<Entity> entities;
            if (!self.Scene.Tracker.Entities.TryGetValue(typeof(T), out entities))
                return false;
            foreach (Entity e in entities) {
                T t = e as T;
                if (Collide.Check(self, t) && predicate(t)) {

                    return true;
                }
            }
            return false;
        }
        public static bool CollideAnyWhere<T>(this Entity self, Predicate<T> predicate, Vector2 at, out T firstMatch) where T : Entity {
            firstMatch = null;
            if (!self.Scene.Tracker.Entities.TryGetValue(typeof(T), out List<Entity> entities))
                return false;
            foreach (Entity e in entities) {
                T t = e as T;
                if (Collide.Check(self, t, at) && predicate(t)) {
                    firstMatch = t;
                    return true;
                }
            }
            return false;
        }
        public static bool CollideAnyWhere<T>(this Entity self, Predicate<T> predicate, Vector2 at) where T : Entity {
            List<Entity> entities;
            if (!self.Scene.Tracker.Entities.TryGetValue(typeof(T), out entities))
                return false;
            foreach (Entity e in entities) {
                T t = e as T;
                if (Collide.Check(self, t, at) && predicate(t)) {

                    return true;
                }
            }
            return false;
        }
        //Goddamn I need to optimize this out
        public static bool CollideAnySetMatchWhere<T>(this Entity self, params Predicate<T>[] predicates) where T : Entity {
            if (!self.Scene.Tracker.Entities.TryGetValue(typeof(T), out List<Entity> entities))
                return false;
            if (predicates.Length > 31) //Slow version. This really isn't likely, but at least we accounted for it.
            {
                bool[] checks = new bool[predicates.Length];
                foreach (Entity e in entities) {
                    T t = e as T;
                    if (Collide.Check(self, t)) {
                        for (int i = 0; i < predicates.Length; i++) {
                            if (predicates[i].Invoke(t)) {
                                checks[i] = true;
                            }
                        }
                    }
                }
                return checks.All(a => a);
            } else {
                //Yo why can't bitshifting happen on longs wtf? 
                int checks = 0;
                foreach (Entity e in entities) {
                    T t = e as T;
                    if (Collide.Check(self, t)) {
                        for (int i = 0; i < predicates.Length; i++) {
                            if (((checks & (1 << i)) > 0) && predicates[i].Invoke(t)) {
                                checks |= 1 << i;
                            }
                        }
                    }
                }
                return checks == (1 << predicates.Length) - 1;
            }
        }
        public static bool CollideAnySetMatchWhere<T>(this Entity self, Vector2 at, params Predicate<T>[] predicates) where T : Entity {
            List<Entity> entities;
            if (!self.Scene.Tracker.Entities.TryGetValue(typeof(T), out entities))
                return false;
            if (predicates.Length > 31) //Slow version. This really isn't likely, but at least we accounted for it.
            {
                bool[] checks = new bool[predicates.Length];
                foreach (Entity e in entities) {
                    T t = e as T;
                    if (Collide.Check(self, t, at)) {
                        for (int i = 0; i < predicates.Length; i++) {
                            if (predicates[i].Invoke(t)) {
                                checks[i] = true;
                            }
                        }
                    }
                }
                return checks.All(a => a);
            } else {
                //Yo why can't bitshifting happen on longs wtf? 
                int checks = 0;
                foreach (Entity e in entities) {
                    T t = e as T;
                    if (Collide.Check(self, t, at)) {
                        for (int i = 0; i < predicates.Length; i++) {
                            if (((checks & (1 << i)) > 0) && predicates[i].Invoke(t)) {
                                checks |= 1 << i;
                            }
                        }
                    }
                }
                return checks == (1 << predicates.Length) - 1;
            }
        }

        private static MethodInfo m_TagLists_EntityAdded = typeof(TagLists).GetMethod("EntityAdded", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo m_Tracker_EntityAdded = typeof(Tracker).GetMethod("EntityAdded", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void ForceAdd(this EntityList list, params Entity[] entities) {
            DynData<EntityList> listData = new DynData<EntityList>(list);
            HashSet<Entity> current = listData.Get<HashSet<Entity>>("current");
            List<Entity> listEntities = listData.Get<List<Entity>>("entities");
            Scene scene = list.Scene;

            foreach (Entity entity in entities) {
                if (!current.Contains(entity)) {
                    current.Add(entity);
                    listEntities.Add(entity);
                    if (scene != null) {
                        m_TagLists_EntityAdded.Invoke(scene.TagLists, new object[] { entity });
                        m_Tracker_EntityAdded.Invoke(scene.Tracker, new object[] { entity });
                        entity.Added(scene);
                    }
                }
            }

            listEntities.Sort(EntityList.CompareDepth);

            foreach (Entity entity in entities) {
                if (entity.Scene == scene)
                    entity.Awake(scene);
            }
        }

        public static int TryCountEntities<T>(this Tracker tracker) where T : Entity {
            if (!tracker.Entities.ContainsKey(typeof(T)))
                return 0;
            else
                return tracker.Entities[typeof(T)].Count();
        }

        public static Entity FindFirst(this EntityList self, Type t) {
            foreach (Entity e in self.getListOfEntities()) {
                if (e.GetType() == t)
                    return e;
            }
            return null;
        }
        public static void SetVolume(this SoundSource self, float volume) {
            if (self == null)
                return;
            float vol = Calc.Clamp(volume, 0f, 1f);
            EventInstance i = (EventInstance) EntityMuterComponent.SoundSource_instance.GetValue(self);
            if (i != null && i.getVolume(out _, out float finalVol) == FMOD.RESULT.OK && finalVol != vol)
                i.setVolume(vol);
        }
    }
    public static class FastFieldInfoHelper {
        public static T CreateDynamicMethod<T>(string methodName, Action<ILProcessor> generator) where T : Delegate {
            Type TType = typeof(T);
            Type[] genTypes = TType.GenericTypeArguments;
            bool isFunc = TType.Name.Contains("Func");

            var method = new DynamicMethodDefinition(methodName,
                isFunc ? genTypes.Last() : null,
                isFunc ? genTypes.Take(genTypes.Length - 1).ToArray() : genTypes
                );

            generator(method.GetILProcessor());

            return method.Generate().CreateDelegate<T>();
        }
        public static Func<TDeclaring, TField> CreateFastGetter<TDeclaring, TField>(this FieldInfo field)
       => CreateAnyFastGetter<Func<TDeclaring, TField>>(field);

        public static Action<TField> CreateFastStaticGetter<TDeclaring, TField>(this FieldInfo field)
            => CreateAnyFastGetter<Action<TField>>(field);

        public static T CreateAnyFastGetter<T>(this FieldInfo field) where T : Delegate
            => CreateDynamicMethod<T>($"VH_{field.DeclaringType.FullName}.dyn_fastGet_{field.Name}", (il) => {
                if (field.IsStatic) {
                    il.Emit(OpCodes.Ldsfld, field);
                } else {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, field);
                }

                il.Emit(OpCodes.Ret);
            });
    }
}