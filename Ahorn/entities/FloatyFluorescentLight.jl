module VivHelperFloatyFluorescentLight
using ..Ahorn, Maple

@mapdef Entity "VivHelper/FloatyLight" FloatyLight(x::Integer, y::Integer, broken::Bool=false)

const placements = Ahorn.PlacementDict(
    "Floaty Light (Viv's Helper)" => Ahorn.EntityPlacement(
        FloatyLight,
        "point"
    )
)

function Ahorn.selection(entity::FloatyLight)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 6, 12, 12)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FloatyLight, room::Maple.Room) = Ahorn.drawSprite(ctx, string("VivHelper/fluorescentLight/", (get(entity.data, "broken", false) ? "half" : "full")), 0, 0)

end