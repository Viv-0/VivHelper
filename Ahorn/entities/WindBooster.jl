module VivHelperWindBooster
using ..Ahorn, Maple

@mapdef Entity "VivHelper/WindBooster" WindBooster(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Wind Booster (Viv's Helper) (BETA)" => Ahorn.EntityPlacement(
        WindBooster
    )
)

sprite = "VivHelper/VivHelperLightGrayBooster/boosterGray00"

function Ahorn.selection(entity::WindBooster)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x-8,y-8,16,16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WindBooster, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
