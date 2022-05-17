module VivHelperCassetteTileEntity
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CassetteTileEntity" CTE(x::Integer, y::Integer, width::Integer=16, height::Integer=16, tiletype::String="3", index::Int=0, tempo::Number=1.0, blendin::Bool=false, enabledTint::String="ffffff", disabledTint::String="808080", ConnectTilesets::Bool=false)

const colorNames = Dict{String, Int}(
    "Blue" => 0,
    "Rose" => 1,
    "Bright Sun" => 2,
    "Malachite" => 3
)
const colors = [
    (76, 180, 255, 192) ./ 255,
    (255, 16, 202, 192) ./ 255,
	(252, 230, 70, 192) ./ 255,
	(64, 255, 89, 192) ./ 255
]


const placements = Ahorn.PlacementDict(
    "Cassette Tile Entity" => Ahorn.EntityPlacement(
        CTE,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::CTE) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
    "index" => colorNames
)

Ahorn.minimumSize(entity::CTE) = 16, 16
Ahorn.resizable(entity::CTE) = true, true

Ahorn.selection(entity::CTE) = Ahorn.getEntityRectangle(entity)

function getCassetteBlockRectangles(tileTypeCompare::String, room::Maple.Room)
    entities = filter(e -> e.name == "VivHelper/CassetteTileEntity", room.entities)
    rects = Dict{Int, Array{Ahorn.Rectangle, 1}}()

    for e in entities
        index = get(e.data, "index", 0)
        rectList = get!(rects, index) do
            Ahorn.Rectangle[]
        end
        
        push!(rectList, Ahorn.Rectangle(
            Int(get(e.data, "x", 0)),
            Int(get(e.data, "y", 0)),
            Int(get(e.data, "width", 8)),
            Int(get(e.data, "height", 8))
        ))
    end
        
    return rects
end

# Is there a casette block we should connect to at the offset?
function notAdjacent(entity::CTE, ox, oy, rects)
    x, y = Ahorn.position(entity)
    rect = Ahorn.Rectangle(x + ox + 4, y + oy + 4, 1, 1)

    for r in rects
        if Ahorn.checkCollision(r, rect)
            return false
        end
    end

    return true
end

function drawCassetteBlock(ctx::Ahorn.Cairo.CairoContext, entity::CTE, room::Maple.Room)
    tileType = get(entity.data, "tiletype", "3")
    if tileType == "0" || tileType == "\0" || tileType == ""
        tileType = "3"
    end
    cassetteBlockRectangles = getCassetteBlockRectangles(tileType, room)

    x,y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    tileWidth = ceil(Int, width / 8)
    tileHeight = ceil(Int, height / 8)

    index = Int(get(entity.data, "index", 0))
    color = get(colors, index+1, colors[1])

    rect = Ahorn.Rectangle(x, y, width, height)
    rects = get(cassetteBlockRectangles, index, Ahorn.Rectangle[])

    frame = "VivHelper/cassetteTileEntity/_pressed0";
    frame = string(frame, get(entity.data, "index", 0))
    if !(rect in rects)
        push!(rects, rect)
    end

    for x2 in 1:tileWidth, y2 in 1:tileHeight
        drawX, drawY = (x2 - 1) * 8, (y2 - 1) * 8

        closedLeft = !notAdjacent(entity, drawX - 8, drawY, rects)
        closedRight = !notAdjacent(entity, drawX + 8, drawY, rects)
        closedUp = !notAdjacent(entity, drawX, drawY - 8, rects)
        closedDown = !notAdjacent(entity, drawX, drawY + 8, rects)
        completelyClosed = closedLeft && closedRight && closedUp && closedDown

        if completelyClosed
            if notAdjacent(entity, drawX + 8, drawY - 8, rects)
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 24, 0, 8, 8, tint=color)
            elseif notAdjacent(entity, drawX - 8, drawY - 8, rects)
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 24, 8, 8, 8, tint=color)

            elseif notAdjacent(entity, drawX + 8, drawY + 8, rects)
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 24, 16, 8, 8, tint=color)

            elseif notAdjacent(entity, drawX - 8, drawY + 8, rects)
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 24, 24, 8, 8, tint=color)

            else
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 8, 8, 8, 8, tint=color)
            end

        else
            if closedLeft && closedRight && !closedUp && closedDown
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 8, 0, 8, 8, tint=color)
            elseif closedLeft && closedRight && closedUp && !closedDown
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 8, 16, 8, 8, tint=color)

            elseif closedLeft && !closedRight && closedUp && closedDown
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 16, 8, 8, 8, tint=color)

            elseif !closedLeft && closedRight && closedUp && closedDown
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 0, 8, 8, 8, tint=color)

            elseif closedLeft && !closedRight && !closedUp && closedDown
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 16, 0, 8, 8, tint=color)

            elseif !closedLeft && closedRight && !closedUp && closedDown
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 0, 0, 8, 8, tint=color)

            elseif !closedLeft && closedRight && closedUp && !closedDown
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 0, 16, 8, 8, tint=color)

            elseif closedLeft && !closedRight && closedUp && !closedDown
                Ahorn.drawImage(ctx, frame, x + drawX, y + drawY, 16, 16, 8, 8, tint=color)
            end
        end
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CTE, room::Maple.Room)
    Ahorn.drawTileEntity(ctx, room, entity; alpha=0.5, blendIn=get(entity.data, "blendin", false))
    drawCassetteBlock(ctx, entity, room)
end
end