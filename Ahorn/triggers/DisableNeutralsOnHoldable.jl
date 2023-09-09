module VivHelperDisableNeutralsOnHoldableTrigger
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/DisableNeutralsOnHoldableTrigger" DRCSI(x::Integer, y::Integer, width::Integer=16, height::Integer=16, state::Bool=true, RevertOnLeave::Bool=false)

const placements = Ahorn.PlacementDict(
    "Disable Neutrals While Carrying (Viv's Helper)" => Ahorn.EntityPlacement(DRCSI, "rectangle")
)

end
