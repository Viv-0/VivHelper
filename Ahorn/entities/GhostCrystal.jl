module VivHelperBooCrystal
using ..Ahorn, Maple

@mapdef Entity "VivHelper/BooCrystal" BooCrystal(
x::Integer, y::Integer, oneUse::Bool=false
)

const placements = Ahorn.PlacementDict(
    "Ghost Crystal (Viv's Helper)" => Ahorn.EntityPlacement(
        BooCrystal
    )
)

sprite = "VivHelper/entities/ghostIdle00.png"

function Ahorn.selection(entity::BooCrystal)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BooCrystal, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
