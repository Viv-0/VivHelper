module VivHelperSpriteEntity
using ..Ahorn, Maple

@mapdef Entity "VivHelper/SpriteEntity" SpriteEntity(x::Integer, y::Integer, tag::String="", spriteReference::String="", animationAudio::String="", AhornSpriteReference::String="")

const placements = Ahorn.PlacementDict(
    "Sprite Entity (Viv's Helper)" => Ahorn.EntityPlacement(
        SpriteEntity,
        "point"
    )
)

function Ahorn.selection(entity::SpriteEntity) 
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x-8,y-8,16,16)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpriteEntity, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity.data, "AhornSpriteReference", ""), 0,0)

end