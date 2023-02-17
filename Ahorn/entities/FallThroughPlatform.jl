module VivHelperFallThru
using ..Ahorn, Maple

@mapdef Entity "VivHelper/FallThru" FallThru(x::Integer, y::Integer, width::Integer=8, surfaceIndex::Int=-1, FallTime::Number=0.35, IgnoreOnlyThis::Bool=false)

const textures = ["wood", "dream", "temple", "templeB", "cliffside", "reflection", "core", "moon"]
const placements = Ahorn.PlacementDict(
    "Fall Through ($(uppercasefirst(texture))) (Viv's Helper)" => Ahorn.EntityPlacement(
        FallThru,
        "rectangle",
        Dict{String, Any}(
            "texture" => texture
        )
    ) for texture in textures
)

const quads = Tuple{Integer, Integer, Integer, Integer}[
    (0, 0, 8, 7) (8, 0, 8, 7) (16, 0, 8, 7);
    (0, 8, 8, 5) (8, 8, 8, 5) (16, 8, 8, 5)
]

Ahorn.editingOptions(entity::FallThru) = Dict{String, Any}(
    "texture" => textures,
    "surfaceIndex" => Maple.tileset_sound_ids,
    "FallTime" => Number[0.35]
)

Ahorn.minimumSize(entity::FallThru) = 8, 0
Ahorn.resizable(entity::FallThru) = true, false

function Ahorn.selection(entity::FallThru)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    return Ahorn.Rectangle(x, y, width, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FallThru, room::Maple.Room)
    texture = get(entity.data, "texture", "wood")
    texture = texture == "default" ? "wood" : texture

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 8))

    startX = div(x, 8) + 1
    stopX = startX + div(width, 8) - 1
    startY = div(y, 8) + 1

    len = stopX - startX
    for i in 0:len
        connected = false
        qx = 2
        if i == 0
            connected = get(room.fgTiles.data, (startY, startX - 1), false) != '0'
            qx = 1

        elseif i == len
            connected = get(room.fgTiles.data, (startY, stopX + 1), false) != '0'
            qx = 3
        end

        quad = quads[2 - connected, qx]
        Ahorn.drawImage(ctx, "objects/jumpthru/$(texture)", 8 * i, 0, quad[1], quad[2], quad[3], quad[4])
    end
end

end