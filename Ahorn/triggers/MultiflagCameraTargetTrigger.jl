module VivHelperMultiflagCameraTargetTrigger
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/MultiflagCameraTargetTrigger" MultiflagCameraTargetTrigger(
	x::Integer, y::Integer, width::Integer=8, height::Integer=8,
	DeleteFlag::String="",
	SingleFlag::String="",
	ComplexFlagData::String="",
	lerpStrength::Number=0.0,
	xOnly::Bool=false,
	yOnly::Bool=false,
	positionMode=String="NoEffect"
)

const placements = Ahorn.PlacementDict(
    "Multiflag Camera Target (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiflagCameraTargetTrigger,
        "rectangle",
        Dict{String, Any}(),
        function(trigger)
            trigger.data["nodes"] = [(Int(trigger.data["x"]) + Int(trigger.data["width"]) + 8, Int(trigger.data["y"]))]
        end
    )
)

function Ahorn.editingOptions(trigger::MultiflagCameraTargetTrigger)
    return Dict{String, Any}(
        "positionMode" => Maple.trigger_position_modes
    )
end

function Ahorn.nodeLimits(trigger::MultiflagCameraTargetTrigger)
    return 1, 1
end

end
