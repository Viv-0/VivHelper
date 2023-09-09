local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local utils = require('utils')

local glider = {}

glider.name = "VivHelper/SizableJelly"
glider.depth = -5
glider.placements = {
    {
        name = "normal",
        data = {
            tutorial = false,
            bubble = false,
            width = 8, height = 8,
            scaleGrabBoxWithHitbox = true
        }
    },
    {
        name = "floating",
        data = {
            tutorial = false,
            bubble = true,
            width = 8, height = 8,
            scaleGrabBoxWithHitbox = true
        }
    }
}

local texture = "objects/glider/idle0"

function glider.sprite(room, entity)
    local bubble = entity.bubble

    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()
        local jellySprite = drawableSprite.fromTexture(texture, entity)
        jellySprite:setScale(entity.width/8,entity.height/8)

        table.insert(lineSprites, 1, jellySprite)

        return lineSprites

    else
        local jellySprite = drawableSprite.fromTexture(texture, entity)
        jellySprite:setScale(entity.width/8,entity.height/8)
        return jellySprite
    end
end

function glider.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local sx, sy = entity.width or 8, entity.height or 8
    return utils.rectangle(x - sx/2, y - sy, sx, sy)
end

return glider