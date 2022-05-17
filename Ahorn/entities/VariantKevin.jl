module VivHelperVariantKevin
using ..Ahorn, Maple

@mapdef Entity "VivHelper/VariantKevin" VariantKevin(x::Integer, y::Integer, width::Integer=8, height::Integer=8, axes::String="both", chillout::Bool=false, Baddy::Bool=false)

const placements = Ahorn.PlacementDict(
    "Variant Kevin (Both)" => Ahorn.EntityPlacement(
        VariantKevin,
        "rectangle"
    ),
    "Variant Kevin (Vertical)" => Ahorn.EntityPlacement(
        VariantKevin,
        "rectangle",
        Dict{String, Any}(
            "axes" => "vertical"
        )
    ),
    "Variant Kevin (Horizontal)" => Ahorn.EntityPlacement(
        VariantKevin,
        "rectangle",
        Dict{String, Any}(
            "axes" => "horizontal"
        )
    ),
)

const smallFace = "/idle_face"
const giantFace = "/giant_block00"

const kevinColor = (98, 34, 43) ./ 255

const frameImage = Dict{String, String}(
    "none" => "/block00",
    "horizontal" => "/block01",
    "vertical" => "/block02",
    "both" => "/block03"
)

Ahorn.editingOptions(entity::VariantKevin) = Dict{String, Any}(
    "axes" => Maple.kevin_axes
)

Ahorn.minimumSize(entity::VariantKevin) = 24, 24
Ahorn.resizable(entity::VariantKevin) = true, true

Ahorn.selection(entity::VariantKevin) = Ahorn.getEntityRectangle(entity)

# Todo - Use randomness to decide on Kevin border
function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VariantKevin, room::Maple.Room)
    dir = string("VivHelper/VariantKevin/", (get(entity.data, "Baddy", false) ? "Baddy" : "Maddy"))
    axes = lowercase(get(entity.data, "axes", "both"))
    chillout = get(entity.data, "chillout", false)

    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    giant = height >= 48 && width >= 48 && chillout
    face = string(dir, (giant ? giantFace : smallFace))
    frame = string(dir, frameImage[lowercase(axes)])
    faceSprite = Ahorn.getSprite(face, "Gameplay")

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    Ahorn.drawRectangle(ctx, 2, 2, width - 4, height - 4, kevinColor)
    Ahorn.drawImage(ctx, faceSprite, div(width - faceSprite.width, 2), div(height - faceSprite.height, 2))

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, 0, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, height - 8, 8, 24, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, 0, (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, frame, width - 8, (i - 1) * 8, 24, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, frame, 0, 0, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, 0, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, 0, height - 8, 0, 24, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, height - 8, 24, 24, 8, 8)
end

function Ahorn.rotated(entity::Maple.CrushBlock, steps::Int)
    if abs(steps) % 2 == 1
        if entity.axes == "horizontal"
            entity.axes = "vertical"

            return entity

        elseif entity.axes == "vertical"
            entity.axes = "horizontal"

            return entity
        end
    end
end

end