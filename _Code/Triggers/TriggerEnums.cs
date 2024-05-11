using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper.Triggers {
    public enum TriggerPersistence {
        Default,
        OncePerRetry,
        OncePerMapPlay
    }

    public enum TriggerActivationCondition {
        OnEnter,
        OnStay,
        OnLeave,
    }
}
