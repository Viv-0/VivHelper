module VivHelperInstantCatchupTrigger

using ..Ahorn, Maple

@mapdef Trigger "VivHelper/InstantCatchupTrigger" InstCatchup(x::Integer, y::Integer, width::Integer=8, height::Integer=8, state::Bool=true, persistence::String="None")

const placements = Ahorn.PlacementDict(
    "Instant Catchup Trigger (Viv's Helper)" => Ahorn.EntityPlacement(
        InstCatchup,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::InstCatchup) = Dict{String, Any}(
    "persistence" => String["None","OncePerRetry","OncePerMapPlay"]
)

end