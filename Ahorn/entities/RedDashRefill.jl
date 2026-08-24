module VivHelperRedDashRefill
using ..Ahorn, Maple
@mapdef Entity "VivHelper/RedDashRefill" Refill(x::Integer, y::Integer, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "Red Dash Refill (Viv's Helper)" => Ahorn.EntityPlacement(
        Refill,
        "point"
    )
)
function Ahorn.selection(entity::Refill)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x-8,y-8,16,16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Refill, room::Maple.Room)
    Ahorn.drawSprite(ctx, "VivHelper/redDashRefill/redIdle00", 0, 0)
end

end