using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Module__Extensions__Etc {
    [MonoMod.ModInterop.ModImportName("GravityHelper")]
    internal static class GravityHelperAPI {
        public static Action<string> RegisterModSupportBlacklist;

        public static Func<int, string> GravityTypeFromInt;

        public static Func<string, int> GravityTypeToInt;

        public static Func<int> GetPlayerGravity;

        public static Func<Actor, int> GetActorGravity;

        public static Action<int, float> SetPlayerGravity;

        public static Action<Actor, int, float> SetActorGravity;

        public static Func<bool> IsPlayerInverted;

        public static Func<Actor, bool> IsActorInverted;

        public static Func<Actor, Action<Entity, int, float>, Component> CreateGravityListener;

        public static Func<Action<Entity, int, float>, Component> CreatePlayerGravityListener;

        public static Action BeginOverride;
        public static Action EndOverride;
    }
}
