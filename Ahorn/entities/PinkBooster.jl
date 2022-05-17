module VivHelperPinkBooster
using ..Ahorn, Maple

@mapdef Entity "VivHelper/PinkBooster" PinkBooster(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Booster (Pink) (Viv's Helper) (BETA)" => Ahorn.EntityPlacement(
        PinkBooster
    )
)

sprite = "VivHelper/boosters/boosterPink00"

function Ahorn.selection(entity::PinkBooster)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x-8,y-8,16,16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PinkBooster, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0; tint=(0.4586, 0.2613, 0.2801, 1.0))
end

end
