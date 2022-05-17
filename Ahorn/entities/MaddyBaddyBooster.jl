module VivHelperVariantBooster
using ..Ahorn, Maple

@mapdef Entity "VivHelper/VariantSpecificBooster" VariantSpecificBooster(
    x::Integer,
    y::Integer,
    red::Bool=false,
    BadelineBooster::Bool=false,
    killIfWrong::Bool=false
)

const placements = Ahorn.PlacementDict(
   "Variant Specific Booster (Viv's Helper)" => Ahorn.EntityPlacement(
      VariantSpecificBooster
   )
)

function boosterSprite(entity::VariantSpecificBooster)
    Baddy = get(entity.data, "BadelineBooster", false)

    if Baddy
        return "VivHelper/VivHelperGrayBooster/boosterBaddy00"

    else
        return "VivHelper/VivHelperGrayBooster/boosterMaddy00"
    end
end

function Ahorn.selection(entity::VariantSpecificBooster)
    x, y = Ahorn.position(entity)
    sprite = boosterSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VariantSpecificBooster, room::Maple.Room)
    sprite = boosterSprite(entity)

    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
