module VivHelperDebrisLimiter
using ..Ahorn, Maple

@mapdef Entity "VivHelper/DebrisLimiter" DebrisLimiter(x::Integer, y::Integer, limiter::Integer=1)

const placements = Ahorn.PlacementDict(
    "Debris Limiter (Viv's Helper)" => Ahorn.EntityPlacement(
        DebrisLimiter,
        "point"
    )
)

Ahorn.editingOptions(entity::DebrisLimiter) = Dict{String, Any}(
    "limiter"=>Dict{String, Integer}("IgnoreSolids" => -1, "DisableAllParticles" => 1, "ReturnToDefault" => 0)
)

sprite = "ahorn/VivHelper/DebrisLimiter"

Ahorn.selection(entity::DebrisLimiter) = Ahorn.Rectangle(get(entity.data, "x", 12)-12, get(entity.data, "y", 12)-12, 24, 24)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DebrisLimiter, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end