module VivHelperDisableRefillCancelSpaceIndicator
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/DisableRefillCancelSpaceIndicator" DRCSI(x::Integer, y::Integer, width::Integer=16, height::Integer=16, state::Bool=false)

const placements = Ahorn.PlacementDict(
    "Disable Player Indicator for Refill Cancel Space (Viv's Helper)" => Ahorn.EntityPlacement(DRCSI, "rectangle")
)

end
