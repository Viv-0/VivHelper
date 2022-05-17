module VivHelperEnergyCrystal
using ..Ahorn, Maple

@mapdef Entity "VivHelper/EnergyCrystal" EnergyCrystal(
x::Integer, y::Integer, oneUse::Bool=true
)

const placements = Ahorn.PlacementDict(
    "Energy Crystal (Viv's Helper)" => Ahorn.EntityPlacement(
        EnergyCrystal,
        "point"
    )
)

function Ahorn.selection(entity::EnergyCrystal)
    x,y = Ahorn.position(entity)
    Ahorn.getSpriteRectangle("VivHelper/entities/gem.png", x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EnergyCrystal, room::Maple.Room) = Ahorn.drawSprite(ctx, "VivHelper/entities/gem.png", 0, 0)

end
