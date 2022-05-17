module VivHelperSpriteEntityActor
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/SpriteEntityActor" SpriteEntityActor(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tag::String="", PlayAnimation::String="", RandomizeFrame::Bool=false, DisableAudioPlay::Bool=false, OverrideAudioEvent::String="", AnimateBefore::Bool=true, FlipX::Bool=false, FlipY::Bool=false)

const placements = Ahorn.PlacementDict(
    "Sprite Entity Actor (Viv's Helper)" => Ahorn.EntityPlacement(
        SpriteEntityActor,
        "rectangle"
    ),
    "Sprite Entity Actor (Moves) (Viv's Helper)" => Ahorn.EntityPlacement(
        SpriteEntityActor,
        "rectangle",
        Dict{String, Any}(
           "MoveTime" => 0.0,
            "Easer" => "Linear"
        ),
        function(entity)
            x,y = Ahorn.position(entity)
            width = get(entity.data, "width", 8)
            entity.data["nodes"] = Vector{Tuple{Int, Int}}()
            push!(entity.data["nodes"], (x+width+8, y))
        end
    )
)

Ahorn.nodeLimits(entity::SpriteEntityActor) = get(entity.data, "Easer", nothing) === nothing ? (0,0) : (1,1)

end