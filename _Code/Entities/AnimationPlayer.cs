using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;


namespace VivHelper.Entities {
    [CustomEntity("VivHelper/SpriteEntity")]
    [Tracked]
    public class SpriteEntity : Actor {
        private Sprite sprite;
        public readonly string tag;
        public readonly Dictionary<string, string> animationAudio = null;

        public SpriteEntity(EntityData data, Vector2 offset) : base(data.Position + offset) {
            sprite = GFX.SpriteBank.Create(data.ThrowOnEmptyAttr("spriteReference"));
            tag = data.ThrowOnEmptyAttr("tag");
            string t = data.Attr("animationAudio");
            if (!string.IsNullOrEmpty(t)) {
                animationAudio = VivHelper.ParseDictFromString<string>(t, ';', ',', u => u.Substring(0, 7) == "event:/" ? u : "event:/" + u);
            }

            Add(sprite);
        }

        public void PlayAnimation(string animID, bool randomizeFrame = false, bool flipX = false, bool flipY = false, bool disableAudioPlay = false, string overrideAudioEvent = null) {
            if (flipX)
                sprite.FlipX = true;
            if (flipY)
                sprite.FlipY = true;
            if (sprite.Animations.ContainsKey(animID)) {
                sprite.Play(animID, true, randomizeFrame);
            }
            if (!disableAudioPlay) {
                if (overrideAudioEvent != null)
                    Audio.Play(overrideAudioEvent);
                else if (animationAudio != null && animationAudio.ContainsKey(animID))
                    Audio.Play(animationAudio[animID]);
            }
        }


    }
}
