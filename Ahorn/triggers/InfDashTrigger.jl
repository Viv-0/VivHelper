module VivHelperINFDashTrigger
using ..Ahorn, Maple
@mapdef Trigger "VivHelper/InfDashTrigger" InfDashTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8)

const placements = Ahorn.PlacementDict(
    "Infinite Dash Trigger (Viv's Helper)"=>Ahorn.EntityPlacement(InfDashTrigger, "rectangle")
)

end