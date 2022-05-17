module VivHelperTimedFlagTrigger
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/TimedFlagTrigger" TFT(x::Integer, y::Integer,
 width::Integer=8, height::Integer=8, flag::String="", state::Bool=true,
 mode::String="OnPlayerEnter", onlyOnce::Bool=false, death_count::Integer=-1,
 Delay::Number=0.1
)

const placements = Ahorn.PlacementDict(
    "Timed Flag Trigger (Viv's Helper)" => Ahorn.EntityPlacement(
        TFT,
        "rectangle"
    )
)

function Ahorn.editingOptions(trigger::TFT)
    return Dict{String, Any}(
        "mode" => Maple.everest_flag_trigger_modes
    )
end

end
