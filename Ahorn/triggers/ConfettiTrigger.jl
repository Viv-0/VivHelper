module VivHelperConfettiTrigger
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/ConfettiTrigger" Confetti(x::Integer, y::Integer,
width::Integer=16, height::Integer=16, onlyOnce::Bool=true, permanent::Bool=false, RepeatOnCycle::Number=0.0)

const placements = Ahorn.PlacementDict(
    "Confetti Trigger (Viv's Helper)" => Ahorn.EntityPlacement(
        Confetti,
        "rectangle",
        Dict{String, Any}(),
        function(trigger)
            trigger.data["nodes"] = [(Int(trigger.data["x"]) + Int(trigger.data["width"]/2), Int(trigger.data["y"]) + Int(trigger.data["height"]/2))]
        end
    )
)

Ahorn.nodeLimits(trigger::Confetti) = 1, 1

end
