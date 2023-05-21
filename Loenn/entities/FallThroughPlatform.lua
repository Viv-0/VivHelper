local drawableSpriteStruct = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local enums = require("consts.celeste_enums")

local textures = {"wood", "dream", "temple", "templeB", "cliffside", "reflection", "core", "moon"}

local function getTexture(entity)
    return entity.texture and entity.texture ~= "default" and entity.texture or "wood"
end

local fallThru = {}

fallThru.name = "VivHelper/FallThru"
fallThru.depth = -9000
fallThru.canResize = {true, false}
fallThru.fieldInformation = {
    texture = {
        options = textures
    },
    surfaceIndex = {
        options = enums.tileset_sound_ids,
        fieldType = "integer"
    }
}
fallThru.placements = {}

for i, texture in ipairs(textures) do
    fallThru.placements[i] = {
        name = texture,
        data = {
            width = 8,
            texture = texture,
            surfaceIndex = -1
        }
    }
end

function fallThru.sprite(room, entity)
    local textureRaw = getTexture(entity)
    local texture = "objects/jumpthru/" .. textureRaw

    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 8

    local startX, startY = math.floor(x / 8) + 1, math.floor(y / 8) + 1
    local stopX = startX + math.floor(width / 8) - 1
    local len = stopX - startX

    local sprites = {}

    for i = 0, len do
        local quadX = 8
        local quadY = 8

        if i == 0 then
            quadX = 0
            quadY = room.tilesFg.matrix:get(startX - 1, startY, "0") ~= "0" and 0 or 8

        elseif i == len then
            quadY = room.tilesFg.matrix:get(stopX + 1, startY, "0") ~= "0" and 0 or 8
            quadX = 16
        end

        local sprite = drawableSpriteStruct.fromTexture(texture, entity)

        sprite:setJustification(0, 0)
        sprite:addPosition(i * 8, 0)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)

        table.insert(sprites, sprite)
    end

    local arrow = drawableSpriteStruct.fromTexture("ahorn/VivHelper/arrow", entity)
    arrow.rotation = math.pi/2
    arrow:setJustification(1, 0.5)
    arrow:addPosition(math.floor(width/2), -1)
    table.insert(sprites, arrow)
    return sprites
end

function fallThru.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width, 8)
end

return fallThru