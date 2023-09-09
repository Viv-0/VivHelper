module VivHelperOrangeBooster
using ..Ahorn, Maple

@mapdef Entity "VivHelper/OrangeBooster" OrangeBooster(x::Integer, y::Integer, speed::Number=220.0)

const placements = Ahorn.PlacementDict(
    "Booster (Orange) (Viv's Helper) (BETA)" => Ahorn.EntityPlacement(
        OrangeBooster
    )
)

sprite = "VivHelper/boosters/boosterOrange00"

function Ahorn.selection(entity::OrangeBooster)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x-8,y-8,16,16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::OrangeBooster, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0; tint=(0.4307, 0.3547, 0.2145, 1.0))
end

end
