module VivHelperChangeRespawnIfFlag
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/ChangeRespawnIfFlag" ChangeRespawnIfFlag(x::Integer, y::Integer,
width::Integer=8, height::Integer=8, flag::String="")

const placements = Ahorn.PlacementDict(
    "Change Respawn If Flag Trigger (Viv's Helper)" => Ahorn.EntityPlacement(
        ChangeRespawnIfFlag,
        "rectangle",
        Dict{String, Any}(),
    )
)

Ahorn.nodeLimits(entity::ChangeRespawnIfFlag) = 0, 1

end