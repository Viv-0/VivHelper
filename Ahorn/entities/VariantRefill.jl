module VivHelperVariantRefill
using ..Ahorn, Maple

@mapdef Entity "VivHelper/VariantRefill" VariantRefill(
    x::Integer, y::Integer, VariantSwapType::String="swap",
    oneUse::Bool=false, RefillDashOnUse::Bool=true

)

const placements = Ahorn.PlacementDict(
   "Variant Swapping Refill (Viv's Helper)" => Ahorn.EntityPlacement(
      VariantRefill,
      "point")
)

const VariantType = Dict{String, String}(
	"Maddy Only" => "red",
	"Baddy Only" => "purp",
	"Swapping" => "swap"
)

Ahorn.editingOptions(entity::VariantRefill) = Dict{String, Any}(
    "VariantSwapType" => VariantType
)


const PurpSprite = "VivHelper/VariantRefill/purpIdle00"
const RedSprite = "VivHelper/VariantRefill/redIdle00"
const SwapSprite = "VivHelper/VariantRefill/swapIdle00"

function getSprite(entity::VariantRefill)
    swap = get(entity.data, "VariantSwapType", "red")
    if swap == "red"
        return RedSprite
    elseif swap == "purp"
        return PurpSprite
    else
        return SwapSprite
    end
end

function Ahorn.selection(entity::VariantRefill)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VariantRefill, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
