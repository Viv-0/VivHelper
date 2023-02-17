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

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/LightningMuter")]
    [Tracked]
    public class LightningMuter : Entity {
        public string flag;
        public string ParentClassRef;
        public Type type;

        public LightningMuter(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flag = data.NoEmptyString("flag");
            ParentClassRef = data.NoEmptyString("AudioPlayingClass");
            Depth = 1000000000;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (ParentClassRef == null){
                if(scene.Tracker.TryGetEntity<LightningRenderer>(out var lr)){
                    lr.PreUpdate += Lr_PreUpdate;
                }
            }
            else if (VivHelper.TryGetType(ParentClassRef, out type) && scene.Tracker.TryGetEntity(type, out var entity)) {
                entity.Add(new EntityMuterComponent(flag));
            }
        }

        private void Lr_PreUpdate(Entity obj) {
            if(obj is LightningRenderer lr) {
                if(string.IsNullOrWhiteSpace(flag) || (lr.SceneAs<Level>()?.Session?.GetFlag(flag) ?? false)) lr.StopAmbience(); else lr.StartAmbience();
            }
            
            
        }

        public override void Update() {
            RemoveSelf();
        }

    }
}
