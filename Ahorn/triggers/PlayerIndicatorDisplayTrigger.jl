module VivHelperPlayerIndicatorTrigger

using ..Ahorn, Maple

@mapdef Trigger "VivHelper/PlayerIndicatorTrigger" PlayerIndicatorTrigger(x::Integer, y::Integer,
width::Integer=8, height::Integer=8, state::Bool=false, revertOnLeave::Bool=false)

const placements = Ahorn.PlacementDict(
    "Refill Cancel Space - Player Indicator Display Trigger (Viv's Helper)" => Ahorn.EntityPlacement(
        PlayerIndicatorTrigger,
        "rectangle"
        )
)

end
