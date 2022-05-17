module VivHelperFollowerDistModTrigger
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/FollowerDistModTrigger" FDMTrigger(
x::Integer, y::Integer, width::Integer=8, height::Integer=8,
FFDistance::Integer=5, FPDistance::Integer=30, DisableLeadersInterval::Bool=false)

const placements = Ahorn.PlacementDict(
   "Follower Distance Modifier Trigger" => Ahorn.EntityPlacement(
      FDMTrigger,
      "rectangle"
   )
)

end
