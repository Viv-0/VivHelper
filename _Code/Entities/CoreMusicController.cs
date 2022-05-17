using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities {
    [CustomEntity("VivHelper/CoreMusicController")]
    [Tracked]
    public class CoreMusicController : Entity {
        public string[] hotParams, coldParams;
        public CoreModeListener listener;

        public CoreMusicController(EntityData data, Vector2 offset) {
            string s = data.Attr("hotParams");
            if (!string.IsNullOrWhiteSpace(s))
                hotParams = s.Split(',');
            s = data.Attr("coldParams");
            if (!string.IsNullOrWhiteSpace(s))
                coldParams = s.Split(',');

            Tag = Tags.Global;
            Add(listener = new CoreModeListener(OnCoreModeChange));
        }

        public override void Awake(Scene scene) {
            if (scene.Tracker.TryGetEntity<CoreMusicController>(out CoreMusicController entity)) {
                Remove(listener); //Disables a 1f bug 
                RemoveSelf();
            }
            base.Awake(scene);

        }


        public void OnCoreModeChange(Session.CoreModes coreMode) {
            Level level = Scene as Level;
            foreach (string s in hotParams)
                level.Session.Audio.Music.Param(s, coreMode == Session.CoreModes.Hot);
            foreach (string s in coldParams)
                level.Session.Audio.Music.Param(s, coreMode == Session.CoreModes.Cold);
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
        }

    }
}
