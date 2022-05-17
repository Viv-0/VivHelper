module VivHelperRCSParticleCountTrigger
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/RCSParticleCountTrigger" RCSPCT(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    SetValue::String="Normal", RevertOnLeave::Bool=false
)

const placements = Ahorn.PlacementDict(
    "Refill Cancel Space Setting Trigger (Viv's Helper)" => Ahorn.EntityPlacement(
        RCSPCT,
        "rectangle"
        )
)

Ahorn.editingOptions(trigger::RCSPCT) = Dict{String, Any}(
    "SetValue" => String["Normal", "Decreased", "Minimal", "None"]
)

end
