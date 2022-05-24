using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using MonoMod;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace VivHelper.Entities {

    /// Great example class for DynData replacement on all systems, simple and good.
    [CustomEntity("VivHelper/SeekerKillBarrier")]
    [TrackedAs(typeof(SeekerBarrier))]
    public class SeekerKillBarrier : SeekerBarrier {
        private static FieldInfo seeker_dead = typeof(Seeker).GetField("dead", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Load() { IL.Celeste.Seeker.SlammedIntoWall += Seeker_SlammedIntoWall; }

        private static void Seeker_SlammedIntoWall(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Wiggler>("Start"))) {
                ILLabel ret = cursor.Clone().GotoNext(instr => instr.MatchRet()).MarkLabel(); //ret call must exist
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Seeker, CollisionData, bool>>(SeekerKillBarrierCheck);
                cursor.Emit(OpCodes.Brtrue, ret);
            }
        }

        public static void Unload() { IL.Celeste.Seeker.SlammedIntoWall -= Seeker_SlammedIntoWall; }

        private static bool SeekerKillBarrierCheck(Seeker s, CollisionData c) {
            if (c.Hit is SeekerKillBarrier k) {
                k.OnReflectSeeker2(s);
                return true;
            }
            return false;
        }


        public DynData<SeekerBarrier> dyn;
        private static Color baseColor = Calc.HexToColor("d03030");


        public SeekerKillBarrier(EntityData data, Vector2 offset) : base(data, offset) {
            dyn = new DynData<SeekerBarrier>(this);
        }

        public void OnReflectSeeker2(Seeker seeker) {
            if (!(bool) seeker_dead.GetValue(seeker)) {
                Entity entity = new Entity(seeker.Position);
                DeathEffect component = new DeathEffect(Color.HotPink, seeker.Center - seeker.Position) {
                    OnEnd = delegate {
                        entity.RemoveSelf();
                    }
                };
                entity.Add(component);
                entity.Depth = -1000000;
                base.Scene.Add(entity);
                Audio.Play("event:/game/05_mirror_temple/seeker_death", seeker.Position);
                seeker.RemoveSelf();
                seeker_dead.SetValue(seeker, true);
                Flashing = true;
                Flash = 1f;
            }
        }

        public override void Render() {
            VivHelper.Entity_Render(this);
            foreach (Vector2 particle in dyn.Get<List<Vector2>>("particles")) {
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, baseColor * 0.5f);
            }
            if (Flashing) {
                Draw.Rect(base.Collider, Color.Lerp(Color.White, baseColor, Flash) * 0.5f);
            }
        }

    }
}
