local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local matrixLib = require("utils.matrix")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local cte = {}

local colors = { -- 255 / 0.75 = 340
    {73 / 340, 170 / 340, 240 / 340, 0.75}, 
    {240 / 340, 73 / 340, 190 / 340, 0.75},
    {252 / 340, 220 / 340, 58 / 340, 0.75},
    {56 / 340, 224 / 340, 78 / 340, 0.75},
}

local colorNames = {
    ["1 - Blue"] = 0,
    ["2 - Rose"] = 1,
    ["3 - Bright Sun"] = 2,
    ["4 - Malachite"] = 3
}

cte.name = "VivHelper/CassetteTileEntity"
cte.minimumSize = {16, 16}
cte.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype")(entity)
    orig["index"] = {
        fieldType = "integer",
        options = colorNames,
        editable = false
    }
    orig["enabledTint"] = {fieldType = "VivHelper.oldColor", allowXNAColors = true, allowEmpty = true }
    orig["disabledTint"] = {fieldType = "VivHelper.oldColor", allowXNAColors = true, allowEmpty = true }
    return orig
end
cte.placements = {}

for i, _ in ipairs(colors) do
    cte.placements[i] = {
        name = tostring(i-1),
        data = {
            tiletype = "3",
            index = i - 1,
            tempo = 1.0,
            width = 16,
            height = 16,
            enabledTint="4488ccff",
            disabledTint="",
            ConnectTilesets = false
        }
    }
end

-- Filter by cassette blocks sharing the same index
local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name and entity.index == target.index
    end
end

local function getTileSprite(entity, x, y, frame, color, depth, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local drawX, drawY = (x - 1) * 8, (y - 1) * 8

    local closedLeft = hasAdjacent(entity, drawX - 8, drawY, rectangles)
    local closedRight = hasAdjacent(entity, drawX + 8, drawY, rectangles)
    local closedUp = hasAdjacent(entity, drawX, drawY - 8, rectangles)
    local closedDown = hasAdjacent(entity, drawX, drawY + 8, rectangles)
    local completelyClosed = closedLeft and closedRight and closedUp and closedDown

    local quadX, quadY = false, false

    if completelyClosed then
        if not hasAdjacent(entity, drawX + 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 0

        elseif not hasAdjacent(entity, drawX - 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 8

        elseif not hasAdjacent(entity, drawX + 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 16

        elseif not hasAdjacent(entity, drawX - 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 24

        else
            quadX, quadY = 8, 8
        end
    else
        if closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 8, 0

        elseif closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 8, 16

        elseif closedLeft and not closedRight and closedUp and closedDown then
            quadX, quadY = 16, 8

        elseif not closedLeft and closedRight and closedUp and closedDown then
            quadX, quadY = 0, 8

        elseif closedLeft and not closedRight and not closedUp and closedDown then
            quadX, quadY = 16, 0

        elseif not closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 0, 0

        elseif not closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 0, 16

        elseif closedLeft and not closedRight and closedUp and not closedDown then
            quadX, quadY = 16, 16
        end
    end

    if quadX and quadY then
        local sprite = drawableSprite.fromTexture(frame, entity)
        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)
        sprite:setColor(color)

        sprite.depth = depth

        return sprite
    end
end

function cte.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype","blendin", nil, {0.75,0.75,0.75,0.75})(room,entity)

    local width, height = entity.width or 32, entity.height or 32
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local index = (entity.index or 0) % 4
    local color = colors[index + 1] -- I don't want mappers to be confused by colors so I'm not drawing it with special tint here
    local frame = "VivHelper/cassetteTileEntity/_pressed0" .. tostring(index)
    local depth = -9999
    for x = 1, tileWidth do
        for y = 1, tileHeight do
            local sprite = getTileSprite(entity, x, y, frame, color, depth, rectangles)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end
    return sprites
end

return cte