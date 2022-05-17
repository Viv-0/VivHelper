module VivHelperCornerBoostBlock
using ..Ahorn, Maple

# Corner Boost Block

@mapdef Entity "VivHelper/CornerBoostBlock" CornerBoostBlock(x::Integer, y::Integer,
width::Integer=8, height::Integer=8, tiletype::String="3", blendin::Bool=true
)

Ahorn.editingOptions(entity::CornerBoostBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::CornerBoostBlock) = 8, 8
Ahorn.resizable(entity::CornerBoostBlock) = true, true

Ahorn.selection(entity::CornerBoostBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CornerBoostBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

# Corner Boost Falling Block

@mapdef Entity "VivHelper/CornerBoostFallingBlock" CBFallingBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tiletype::String="3", blendin::Bool=true, behind::Bool=false, climbFall::Bool=true, bufferClimbFall::Bool=false)

Ahorn.editingOptions(entity::CBFallingBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::CBFallingBlock) = 8, 8
Ahorn.resizable(entity::CBFallingBlock) = true, true

Ahorn.selection(entity::CBFallingBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CBFallingBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

# Corner Boost Switch Gate

@mapdef Entity "VivHelper/CornerBoostSwitchGate" CBSwitchGate(x::Integer, y::Integer, width::Integer=8, height::Integer=8, persistent::Bool=false, sprite::String="block")

function gateFinalizer(entity)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    entity.data["nodes"] = [(x + width, y)]
end

const switchgatetextures = ["block", "mirror", "temple", "stars"]



# Corner Boost Zip Mover

@mapdef Entity "VivHelper/CornerBoostZipMover" CBZipMover(x::Integer, y::Integer, width::Integer=8, height::Integer=8, theme::String="Normal")

Ahorn.editingOptions(entity::CBZipMover) = Dict{String, Any}(
    "theme" => Maple.Zip_mover_themes
)

Ahorn.nodeLimits(entity::CBZipMover) = 1, 1

Ahorn.minimumSize(entity::CBZipMover) = 16, 16
Ahorn.resizable(entity::CBZipMover) = true, true

function Ahorn.selection(entity::CBZipMover)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx + floor(Int, width / 2) - 5, ny + floor(Int, height / 2) - 5, 10, 10)]
end

function getTextures(entity::CBZipMover)
    theme = lowercase(get(entity, "theme", "normal"))

    if theme == "moon"
        return "objects/zipmover/moon/block", "objects/zipmover/moon/light01", "objects/zipmover/moon/cog"
    end

    return "objects/zipmover/block", "objects/zipmover/light01", "objects/zipmover/cog"
end

ropeColor = (102, 57, 49) ./ 255

function renderZipMover(ctx::Ahorn.Cairo.CairoContext, entity::CBZipMover)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    block, light, cog = getTextures(entity)
    lightSprite = Ahorn.getSprite(light, "Gameplay")

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    cx, cy = x + width / 2, y + height / 2
    cnx, cny = nx + width / 2, ny + height / 2

    length = sqrt((x - nx)^2 + (y - ny)^2)
    theta = atan(cny - cy, cnx - cx)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, cx, cy)
    Ahorn.rotate(ctx, theta)

    Ahorn.setSourceColor(ctx, ropeColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    # Offset for rounding errors
    Ahorn.move_to(ctx, 0, 4 + (theta <= 0))
    Ahorn.line_to(ctx, length, 4 + (theta <= 0))

    Ahorn.move_to(ctx, 0, -4 - (theta > 0))
    Ahorn.line_to(ctx, length, -4 - (theta > 0))

    Ahorn.stroke(ctx)

    Ahorn.Cairo.restore(ctx)

    Ahorn.drawRectangle(ctx, x + 2, y + 2, width - 4, height - 4, (0.0, 0.0, 0.0, 1.0))
    Ahorn.drawSprite(ctx, cog, cnx, cny)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, block, x + (i - 1) * 8, y, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, block, x + (i - 1) * 8, y + height - 8, 8, 16, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, block, x, y + (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, block, x + width - 8, y + (i - 1) * 8, 16, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, block, x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, block, x + width - 8, y, 16, 0, 8, 8)
    Ahorn.drawImage(ctx, block, x, y + height - 8, 0, 16, 8, 8)
    Ahorn.drawImage(ctx, block, x + width - 8, y + height - 8, 16, 16, 8, 8)

    Ahorn.drawImage(ctx, lightSprite, x + floor(Int, (width - lightSprite.width) / 2), y)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CBZipMover, room::Maple.Room)
    renderZipMover(ctx, entity)
end

# Corner Boost Swap Block

@mapdef Entity "VivHelper/CornerBoostSwapBlock" CBSwapBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, theme::String="Normal")

function swapFinalizer(entity)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    entity.data["nodes"] = [(x + width, y)]
end

Ahorn.editingOptions(entity::CBSwapBlock) = Dict{String, Any}(
    "theme" => Maple.swap_block_themes
)

Ahorn.nodeLimits(entity::CBSwapBlock) = 1, 1

Ahorn.minimumSize(entity::CBSwapBlock) = 16, 16
Ahorn.resizable(entity::CBSwapBlock) = true, true

function Ahorn.selection(entity::CBSwapBlock)
    x, y = Ahorn.position(entity)
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(stopX, stopY, width, height)]
end

function getTextures(entity::CBSwapBlock)
    theme = lowercase(get(entity, "theme", "normal"))

    if theme == "moon"
        return "objects/swapblock/moon/blockRed", "objects/swapblock/moon/target", "objects/swapblock/moon/midBlockRed00"
    end

    return "objects/swapblock/blockRed", "objects/swapblock/target", "objects/swapblock/midBlockRed00"
end

function renderTrail(ctx, x::Number, y::Number, width::Number, height::Number, trail::String)
    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, trail, x + (i - 1) * 8, y + 2, 6, 0, 8, 6)
        Ahorn.drawImage(ctx, trail, x + (i - 1) * 8, y + height - 8, 6, 14, 8, 6)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, trail, x + 2, y + (i - 1) * 8, 0, 6, 6, 8)
        Ahorn.drawImage(ctx, trail, x + width - 8, y + (i - 1) * 8, 14, 6, 6, 8)
    end

    for i in 2:tilesWidth - 1, j in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, trail, x + (i - 1) * 8, y + (j - 1) * 8, 6, 6, 8, 8)
    end

    Ahorn.drawImage(ctx, trail, x + width - 8, y + 2, 14, 0, 6, 6)
    Ahorn.drawImage(ctx, trail, x + width - 8, y + height - 8, 14, 14, 6, 6)
    Ahorn.drawImage(ctx, trail, x + 2, y + 2, 0, 0, 6, 6)
    Ahorn.drawImage(ctx, trail, x + 2, y + height - 8, 0, 14, 6, 6)
end

function renderSwapBlock(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number, midResource::String, frame::String)
    midSprite = Ahorn.getSprite(midResource, "Gameplay")

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, frame, x + (i - 1) * 8, y, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, frame, x + (i - 1) * 8, y + height - 8, 8, 16, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, x, y + (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, frame, x + width - 8, y + (i - 1) * 8, 16, 8, 8, 8)
    end

    for i in 2:tilesWidth - 1, j in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, x + (i - 1) * 8, y + (j - 1) * 8, 8, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, frame, x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, x + width - 8, y, 16, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, x, y + height - 8, 0, 16, 8, 8)
    Ahorn.drawImage(ctx, frame, x + width - 8, y + height - 8, 16, 16, 8, 8)

    Ahorn.drawImage(ctx, midSprite, x + div(width - midSprite.width, 2), y + div(height - midSprite.height, 2))
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CBSwapBlock, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")
    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    frame, trail, mid = getTextures(entity)

    renderSwapBlock(ctx, stopX, stopY, width, height, mid, frame)
    Ahorn.drawArrow(ctx, startX + width / 2, startY + height / 2, stopX + width / 2, stopY + height / 2, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CBSwapBlock, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")

    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    frame, trail, mid = getTextures(entity)

    renderTrail(ctx, min(startX, stopX), min(startY, stopY), abs(startX - stopX) + width, abs(startY - stopY) + height, trail)
    renderSwapBlock(ctx, startX, startY, width, height, mid, frame)
end

# Corner Boost Dream Block

@mapdef Entity "VivHelper/CornerBoostDreamBlock" CBDreamBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, fastMoving::Bool=false, oneUse::Bool=false, below::Bool=false)

Ahorn.nodeLimits(entity::CBDreamBlock) = 0, 1

Ahorn.minimumSize(entity::CBDreamBlock) = 8, 8
Ahorn.resizable(entity::CBDreamBlock) = true, true

function Ahorn.selection(entity::CBDreamBlock)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    nodes = get(entity.data, "nodes", ())
    if isempty(nodes)
        return Ahorn.Rectangle(x, y, width, height)

    else
        nx, ny = Int.(nodes[1])
        return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx, ny, width, height)]
    end
end

function renderSpaceJam(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number)
    Ahorn.Cairo.save(ctx)

    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1)

    Ahorn.drawRectangle(ctx, x, y, width, height, (0.0, 0.0, 0.0, 0.4), (1.0, 1.0, 1.0, 1.0))

    Ahorn.restore(ctx)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CBDreamBlock)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    if !isempty(nodes)
        nx, ny = Int.(nodes[1])

        cox, coy = floor(Int, width / 2), floor(Int, height / 2)

        renderSpaceJam(ctx, nx, ny, width, height)
        Ahorn.drawArrow(ctx, x + cox, y + coy, nx + cox, ny + coy, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CBDreamBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderSpaceJam(ctx, 0, 0, width, height)
end

# Corner Boost Cassette Blocks

@mapdef Entity "VivHelper/CornerBoostCassetteBlock" CBCassetteBlock(x::Integer, y::Integer, width::Integer=16, height::Integer=16, index::Integer=0, tempo::Number=1.0)

const colorNames = Dict{String, Int}(
    "Blue" => 0,
    "Rose" => 1,
    "Bright Sun" => 2,
    "Malachite" => 3
)

Ahorn.editingOptions(entity::CBCassetteBlock) = Dict{String, Any}(
    "index" => colorNames
)

Ahorn.minimumSize(entity::CBCassetteBlock) = 16, 16
Ahorn.resizable(entity::CBCassetteBlock) = true, true

Ahorn.selection(entity::CBCassetteBlock) = Ahorn.getEntityRectangle(entity)

const colors = Dict{Int, Ahorn.colorTupleType}(
    1 => (240, 73, 190, 255) ./ 255,
	2 => (252, 220, 58, 255) ./ 255,
	3 => (56, 224, 78, 255) ./ 255,
)

const defaultColor = (73, 170, 240, 255) ./ 255
const borderMultiplier = (0.9, 0.9, 0.9, 1)

const frame = "objects/cassetteblock/solid"

function getCassetteBlockRectangles(room::Maple.Room)
    entities = filter(e -> e.name == "VivHelper/CornerBoostCassetteBlock", room.entities)
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
function notAdjacent(entity::CBCassetteBlock, ox, oy, rects)
    x, y = Ahorn.position(entity)
    rect = Ahorn.Rectangle(x + ox + 4, y + oy + 4, 1, 1)

    for r in rects
        if Ahorn.checkCollision(r, rect)
            return false
        end
    end

    return true
end

function drawCassetteBlock(ctx::Ahorn.Cairo.CairoContext, entity::CBCassetteBlock, room::Maple.Room)
    cassetteBlockRectangles = getCassetteBlockRectangles(room)

    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    tileWidth = ceil(Int, width / 8)
    tileHeight = ceil(Int, height / 8)

    index = Int(get(entity.data, "index", 0))
    color = get(colors, index, defaultColor)

    rect = Ahorn.Rectangle(x, y, width, height)
    rects = get(cassetteBlockRectangles, index, Ahorn.Rectangle[])

    if !(rect in rects)
        push!(rects, rect)
    end

    for x in 1:tileWidth, y in 1:tileHeight
        drawX, drawY = (x - 1) * 8, (y - 1) * 8

        closedLeft = !notAdjacent(entity, drawX - 8, drawY, rects)
        closedRight = !notAdjacent(entity, drawX + 8, drawY, rects)
        closedUp = !notAdjacent(entity, drawX, drawY - 8, rects)
        closedDown = !notAdjacent(entity, drawX, drawY + 8, rects)
        completelyClosed = closedLeft && closedRight && closedUp && closedDown

        if completelyClosed
            if notAdjacent(entity, drawX + 8, drawY - 8, rects)
                Ahorn.drawImage(ctx, frame, drawX, drawY, 24, 0, 8, 8, tint=color)

            elseif notAdjacent(entity, drawX - 8, drawY - 8, rects)
                Ahorn.drawImage(ctx, frame, drawX, drawY, 24, 8, 8, 8, tint=color)

            elseif notAdjacent(entity, drawX + 8, drawY + 8, rects)
                Ahorn.drawImage(ctx, frame, drawX, drawY, 24, 16, 8, 8, tint=color)

            elseif notAdjacent(entity, drawX - 8, drawY + 8, rects)
                Ahorn.drawImage(ctx, frame, drawX, drawY, 24, 24, 8, 8, tint=color)

            else
                Ahorn.drawImage(ctx, frame, drawX, drawY, 8, 8, 8, 8, tint=color)
            end

        else
            if closedLeft && closedRight && !closedUp && closedDown
                Ahorn.drawImage(ctx, frame, drawX, drawY, 8, 0, 8, 8, tint=color)

            elseif closedLeft && closedRight && closedUp && !closedDown
                Ahorn.drawImage(ctx, frame, drawX, drawY, 8, 16, 8, 8, tint=color)

            elseif closedLeft && !closedRight && closedUp && closedDown
                Ahorn.drawImage(ctx, frame, drawX, drawY, 16, 8, 8, 8, tint=color)

            elseif !closedLeft && closedRight && closedUp && closedDown
                Ahorn.drawImage(ctx, frame, drawX, drawY, 0, 8, 8, 8, tint=color)

            elseif closedLeft && !closedRight && !closedUp && closedDown
                Ahorn.drawImage(ctx, frame, drawX, drawY, 16, 0, 8, 8, tint=color)

            elseif !closedLeft && closedRight && !closedUp && closedDown
                Ahorn.drawImage(ctx, frame, drawX, drawY, 0, 0, 8, 8, tint=color)

            elseif !closedLeft && closedRight && closedUp && !closedDown
                Ahorn.drawImage(ctx, frame, drawX, drawY, 0, 16, 8, 8, tint=color)

            elseif closedLeft && !closedRight && closedUp && !closedDown
                Ahorn.drawImage(ctx, frame, drawX, drawY, 16, 16, 8, 8, tint=color)
            end
        end
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CBCassetteBlock, room::Maple.Room) = drawCassetteBlock(ctx, entity, room)

# placements

const placements = Ahorn.PlacementDict(
    "Corner Boost Block (Viv's Helper)" => Ahorn.EntityPlacement(
        CornerBoostBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    ),
    "Corner Boost Falling Block (Viv's Helper)" => Ahorn.EntityPlacement(
        CBFallingBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

for theme in Maple.swap_block_themes
    key = "Corner Boost Zip Mover ($(uppercasefirst(theme))) (Viv's Helper)"
    placements[key] = Ahorn.EntityPlacement(
        CBZipMover,
        "rectangle",
        Dict{String, Any}(
            "theme" => theme
        ),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        end
    )
    key2 = "Corner Boost Swap Block ($(uppercasefirst(theme))) (Viv's Helper)"
    placements[key2] = Ahorn.EntityPlacement(
        CBSwapBlock,
        "rectangle",
        Dict{String, Any}(
            "theme" => theme
        ),
        swapFinalizer
    )
end
for (color, index) in colorNames
    key = "Corner Boost Cassette Block ($index - $color) (Viv's Helper)"
    placements[key] = Ahorn.EntityPlacement(
        CBCassetteBlock,
        "rectangle",
        Dict{String, Any}(
            "index" => index
        )
    )
end

end
