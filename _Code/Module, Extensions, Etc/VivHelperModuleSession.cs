using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using VivHelper.Entities;
using VivHelper.Entities.Boosters;
using VivHelper.Entities.SeekerStuff;
using VivHelper.Triggers;
using Microsoft.Xna.Framework.Graphics;
using FMOD.Studio;
using System.Text.RegularExpressions;

namespace VivHelper {
    public class VivHelperModuleSession : EverestModuleSession {
        public enum DemoDashDisabler {
            Off = 0,
            DisableBind = 1,
            DisableAll = 2,
        }

        public class HoldableBarrierCh {
            public string particleColorHex { get; set; }
            public string baseColorHex { get; set; }
            public Vector2 particleDir { get; set; }
            public bool solidOnRelease { get; set; }
            public bool solidOnPlayer { get; set; }
            public bool toggleBloom { get; set; } = true;
        }
        public class CrystalBombDetonatorCh {
            public string particleColorHex { get; set; }
            public string baseColorHex { get; set; }
            public Vector2 particleDir { get; set; }
            public float DetonationDelay { get; set; }
            public bool CanBeNeutralized { get; set; }
        }
        public CustomDashStateCh customDashState { get; set; } = null;
        public CustomBooster CurrentBooster { get; set; } = null;
        public CrystalBombDetonatorCh savedCBDController { get; set; } = null;
        public HoldableBarrierCh savedHBController { get; set; } = null;
        //public Crystal_BombDetonatorController savedCBDController { get; set; } = new Crystal_BombDetonatorController();

        public DashPowerupManager dashPowerupManager = null;
        public bool HasSpeedPower { get; set; } = false;
        public bool CanAddSpeed { get; set; } = false;
        public Vector2 StoredSpeed { get; set; } = Vector2.Zero;
        public Facings Facing { get; set; } = Facings.Right;

        public int dashCount { get; set; }
        public float staminaCount { get; set; }

        public bool ShowIndicator { get; set; } = true;

        public void ResetSpeedPowerup() {
            HasSpeedPower = false;
            CanAddSpeed = false;
            StoredSpeed = Vector2.Zero;
            Facing = Facings.Right;
        }

        public int AlwaysBreakDashBlockDash = 0;
        //0 = not enabled
        //1 = can dash
        //2 = dashing

        public bool Blackout = false;

        public string PreviousLevel { get; set; } = null;

        public Dictionary<string, HashSet<EntityID>> CollectedCoins = new Dictionary<string, HashSet<EntityID>>();

        /// <summary>
        /// 0 : None,
        /// >0 : # of Seconds (to the nearest frame, rounded up)
        /// <0 : permanent
        /// </summary>
        public float lockCamera { get; set; } = 0;

        public Dictionary<string, LevelInfo> LevelInfoCache { get; set; } = new Dictionary<string, LevelInfo>();

        /// <summary>
        /// Defined as the percentage of Debris not allowed in (1-# = percentage of Debris allowed)
        /// </summary>
        public float DebrisLimiter { get; set; } = 0f;

        public int? OverrideTempleFallX = null;
        public bool TeleportState { get; set; } = false;

        [YamlDotNet.Serialization.YamlIgnore]
        public Action<Player, Level> TeleportAction = null;

        public bool PausedRetryCheck { get; set; } = false;

        public bool EnableRefillCancelSpaceIndicator { get; set; } = true;

        public bool DisableMapViewArbitrarySpawn { get; set; } = false;
        public bool DisableNeutralsOnHoldable { get; set; } = false;

        public bool StallScreenWipe = false;

        public int FFDistance = 5;
        public int FPDistance = 30;
        public bool MakeClose = false;
        public DemoDashDisabler demodashDisabler = DemoDashDisabler.Off;

        public float OrangeSpeed = 220f;

        [YamlDotNet.Serialization.YamlIgnore]
        public SolidModifierComponent currentActiveSolidModifier = null;


        [YamlDotNet.Serialization.YamlIgnore] //This will be set on all load-ins
        public Dictionary<string, SoundChange> AudioChanges = new Dictionary<string, SoundChange>();

        public void MapChangesToAudioSet(EntityData data) {
            var eventName = data.Attr("eventName");
            if (string.IsNullOrWhiteSpace(eventName) || eventName == "event:/none")
                return;
            SoundChange change;
            if (AudioChanges.TryGetValue(eventName, out change)) {
                if (change is SoundMute && data.Name == "VivHelper/SoundMuter")
                    change.AddOrChangeFromEntityData(data);
                else if (change is SoundReplace && data.Name == "VivHelper/SoundReplacer")
                    change.AddOrChangeFromEntityData(data);
                return;
            } else if(data.Name == "VivHelper/SoundMuter") {
                AudioChanges.Add(eventName, new SoundMute(data));
                return;
            } else if(data.Name == "VivHelper/SoundReplacer") {
                AudioChanges.Add(eventName, new SoundReplace(data));
                return;
            }
        }
        /*
        [NonSerialized]
        [YamlDotNet.Serialization.YamlIgnore] // Alternate Tracker data on a per load basis.
        public Dictionary<Type, Grouper> groupers = new Dictionary<Type, Grouper>(1);

        private const int GRADMAPSIZE_SQRT = 64;

        // Hi! If you're thinking about trying to create gradients with this method, PLEASE DONT
        // I only did this as a temporary fix to a small problem with my one case scenario.
        // If you absolutely have to do this, DM me and mention this comment, but I assure you 95% of the time you don't need it, and I'll evenutally build a *proper* solution you can use.
        public struct ColorSet {

            public ColorSet(Vector3 vec) { color = vec; count = 1; }

            public Vector3 color = Vector3.Zero; //Black texture
            public int count = -1; //Default only
            public bool MatchValue(Vector3 vec) => vec.Equals(color);
        }

        public MHHRainbowInfo currentRainbowInfo { get; set; } = SpinnerGrouper.defaultRainbowInfo;
        [NonSerialized]
        [YamlDotNet.Serialization.YamlIgnore] //This is only here because it'll be caught by SaveLoad tool
        internal Texture2D gradientMap = new Texture2D(Engine.Graphics.GraphicsDevice, GRADMAPSIZE_SQRT, 1); //Please no touch
        [NonSerialized]
        [YamlDotNet.Serialization.YamlIgnore]  //This is only here because it'll be caught by SaveLoad tool
        private List<ColorSet> gradientData = new List<ColorSet>(GRADMAPSIZE_SQRT); //This is smart because this reduces our memory footprint for this object significantly. Please no touch
        [NonSerialized]
        [YamlDotNet.Serialization.YamlIgnore]
        private bool gradientMapHasChanged = false; // Triggers the Renderer to run the ResolveGradientDifferential function.

        internal int GetOrAddColorToGradientMap(Vector3 vec) { // Alpha is completely ignored for these values.
            if (gradientData.Count == 0) {
                gradientData.Add(new ColorSet(vec));
                gradientMapHasChanged = true;
                return 0;
            }
            int firstEmptyColor = -1;
            // Check the values currently stored by the Texture2D gradientMap, with gradientData. This is a faster operation with relatively low cost to the computer;
            for (int i = 0; i < gradientData.Count; i++) {
                ColorSet cs = gradientData[i];
                if (cs.MatchValue(vec)) {
                    cs.count++;
                    return i;
                } else if (cs.count == 0 && firstEmptyColor == -1) {
                    firstEmptyColor = i;
                }
            } // No colors currently match the known values, but we probably have some unused color in the current gradientMap, the index of which is found by `firstEmptyColor`.
            if (firstEmptyColor != -1) {
                gradientData[firstEmptyColor] = new ColorSet(vec);
                gradientMapHasChanged = true;
                return firstEmptyColor;
            } else if (gradientData.Count == GRADMAPSIZE_SQRT * GRADMAPSIZE_SQRT) {
                return GRADMAPSIZE_SQRT * GRADMAPSIZE_SQRT - 1; // This is the failsafe mechanism. This occurs when all 1024 colors are being used and sets that color as the ;ast value in the map since it cannot fit any more
            } else if (gradientData.Count < gradientData.Capacity) {
                gradientData.Add(new ColorSet(vec));
                gradientMapHasChanged = true;
                return gradientData.Count - 1;
            } else {
                // Registers a new Texture with a new size. Size modifiers are used explicitly for reducing memory costs
                int nextVal = gradientData.Capacity + 1;
                gradientData.Capacity += GRADMAPSIZE_SQRT;
                gradientMap.Dispose();
                gradientMap = new Texture2D(Engine.Graphics.GraphicsDevice, GRADMAPSIZE_SQRT, gradientData.Capacity / GRADMAPSIZE_SQRT);
                gradientData[nextVal] = new ColorSet(vec);
                gradientMapHasChanged = true;
                return nextVal;
            }

        }
        public void RemoveValueFromGradientMap(Vector3 color) {
            for (int i = 0; i < gradientData.Count; i++) {
                ColorSet cs = gradientData[i];
                if (cs.MatchValue(color)) {
                    cs.count--;
                    return;
                }
            }
        }

        internal void ResolveGradientMapDifferential() {
            if (!gradientMapHasChanged)
                return;
            Vector4[] vecs = new Vector4[gradientData.Capacity];
            for (int i = 0; i < gradientData.Capacity; i++) {
                if (i >= gradientData.Count) { vecs[i] = Vector4.Zero; } else { vecs[i] = new Vector4(gradientData[i].color, 1f); }
            }
            gradientMap.SetData(vecs);
            gradientMapHasChanged = false;
        }
        */
    }
}
