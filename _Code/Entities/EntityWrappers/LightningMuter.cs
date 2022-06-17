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
            flag = data.Attr("flag");
            ParentClassRef = data.NoEmptyString("AudioPlayingClass");

        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (string.IsNullOrWhiteSpace(flag))
                return;
            if(ParentClassRef == null && scene.Tracker.TryGetEntity<LightningRenderer>(out var lr)) {
                lr.Add(new EntityMuterComponent());
            }
            if (ParentClassRef != null && VivHelper.TryGetType(ParentClassRef, out type) && scene.Tracker.TryGetEntity(type, out var entity)) {
                entity.Add(new EntityMuterComponent());
            }
        }

        public override void Update() {
            RemoveSelf();
        }

    }
}
