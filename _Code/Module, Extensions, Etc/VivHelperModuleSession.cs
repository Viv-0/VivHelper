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

namespace VivHelper {
    public class VivHelperModuleSession : EverestModuleSession {


        public class HoldableBarrierCh {
            public string particleColorHex { get; set; }
            public string baseColorHex { get; set; }
            public Vector2 particleDir { get; set; }
            public bool solidOnRelease { get; set; }
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
        /// >0 : # of Frames
        /// <0 : permanent
        /// </summary>
        public int lockCamera { get; set; } = 0;

        public Dictionary<string, LevelInfo> LevelInfoCache { get; set; } = new Dictionary<string, LevelInfo>();

        /// <summary>
        /// Defined as the percentage of Debris not allowed in (1-# = percentage of Debris allowed)
        /// </summary>
        public float DebrisLimiter { get; set; } = 0f;

        public bool TeleportState { get; set; } = false;

        public bool PausedRetryCheck { get; set; } = false;

        public bool EnableRefillCancelSpaceIndicator { get; set; } = true;

        public bool DisableMapViewArbitrarySpawn { get; set; } = false;
    }
}
