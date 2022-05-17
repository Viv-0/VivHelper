module VivHelperEntityMuter
using ..Ahorn, Maple

@mapdef Entity "VivHelper/EntityMuter" EntityMuter(x::Integer, y::Integer, width::Integer=8, height::Integer=8, Types::String="", all::Bool=true)

const placements = Ahorn.PlacementDict(
    "Entity Muter (Viv's Helper)" => Ahorn.EntityPlacement(
        EntityMuter,
        "rectangle"
    )
)

Ahorn.resizable(entity::EntityMuter) = true, true
Ahorn.minimumSize(entity::EntityMuter) = 8,8

Ahorn.selection(entity::EntityMuter) = Ahorn.getEntityRectangle(entity)

sprite = "ahorn/VivHelper/mute.png"

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EntityMuter, room::Maple.Room)
    Ahorn.drawRectangle(ctx, 0, 0, entity.data["width"], entity.data["height"], (0.4,0.4,0.4,0.4))
    Ahorn.drawSprite(ctx, sprite, entity.data["width"]/2, entity.data["height"]/2)
end

end