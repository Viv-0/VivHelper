module VivHelperActivateCPPTrigger
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/ActivateCPP" ACPPTrigger(x::Integer, y::Integer,
width::Integer=8, height::Integer=8, CPPID::String="", state::Bool=true, onlyOnce::Bool=false,
mode::String="WhilePlayerInside")

const placements = Ahorn.PlacementDict(
    "Activate Custom Player Playback Trigger (Viv's Helper)" => Ahorn.EntityPlacement(
        ACPPTrigger,
        "rectangle"
    )
)

function Ahorn.editingOptions(trigger::ACPPTrigger)
    return Dict{String, Any}(
        "mode" => Dict{String, String}(
			"On Player Entering Area" => "OnPlayerEnter",
			"On Player Leaving Area" =>	"OnPlayerLeave",
			"On Level Start" => "OnLevelStart",
			"While Player is Inside" => "WhilePlayerInside"
    		)
		)
end

end
