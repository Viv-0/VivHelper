module VivHelperRefillPotion

using ..Ahorn, Maple

@mapdef Entity "VivHelper/RefillPotion" RefillPotion(x::Integer, y::Integer, twoDash::Bool=false, heavy::Bool=false, shatterOnGround::Bool=false, useAlways::Bool=false, floating::Bool=false)

const placements = Ahorn.PlacementDict(
    "Refill Potion (Viv's Helper)" => Ahorn.EntityPlacement(
        RefillPotion
    ),
    "Refill Potion (Floating) (Viv's Helper)" => Ahorn.EntityPlacement(
        RefillPotion,
        "point",
        Dict{String, Any}(
            "floating" => true
        )
    ),
)

function getSprite(entity::RefillPotion)
    twoDash = get(entity.data, "twoDash", false)

    return twoDash ? "VivHelper/Potions/PotRefillTwo00" : "VivHelper/Potions/PotRefill00"
end

function Ahorn.selection(entity::RefillPotion)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RefillPotion, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
    if get(entity, "floating", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end

end