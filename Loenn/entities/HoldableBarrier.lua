local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local utils = require('utils')
local drawableSpriteStruct = require('structs.drawable_sprite')
local drawableRectangle = require('structs.drawable_rectangle')

local HoldableBarrier = {}
local C1 = {0.353,0.431,0.882,0.5}
local C2 = {0.353,0.431,0.882,0.75}

HoldableBarrier.name = "VivHelper/HoldableBarrier"
HoldableBarrier.depth = 0
HoldableBarrier.placements = {
    name = "barrier",
    data = {
        width = 8,
        height = 8
    }
}
function HoldableBarrier.sprite(room,entity)
    return drawableRectangle.fromRectangle("bordered",entity.x,entity.y,entity.width,entity.height,C1,C2)
end

local HBC2 = {
    name = "VivHelper/HoldableBarrierController2",
    depth = -1000000
}
HBC2.placements = {
    name = "main",
    data = {
        EdgeColor="5a6ee1", ParticleColor="5a6ee1",
        ParticleAngle=270, SolidOnRelease=true,
        Persistent=false
    }
}
HBC2.fieldInformation = {
    EdgeColor = {fieldType = "VivHelper.color", allowXNAColors = true},
    ParticleColor = {fieldType = "VivHelper.color", allowXNAColors = true},
    ParticleAngle = {minimumValue = 0.0, maximumValue = 360.0}
}
HBC2.sprite = function(room,entity) return {
        drawableSpriteStruct.fromTexture("ahorn/VivHelper/HBC", entity),
        drawableRectangle.fromRectangle("bordered", entity.x-7,entity.y+2,14,6,vivUtil.getColorTable(entity.ParticleColor, true, C1),vivUtil.getColorTable(entity.EdgeColor, true, C2))
} end

local HoldableJumpthru = {}

HoldableJumpthru.name = "VivHelper/HoldableBarrierJumpThru"
HoldableJumpthru.depth = -9000
HoldableJumpthru.canResize = {true, false}
HoldableJumpthru.placements = {name = "main", data = {
    width = 8
}}

function HoldableJumpthru.sprite(room, entity)
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

        local s1 = drawableSpriteStruct.fromTexture("VivHelper/holdableJumpThru/00", entity)
        s1:setJustification(0, 0)
        s1:addPosition(i * 8, 0)
        s1:useRelativeQuad(quadX, quadY, 8, 8)
        s1:setColor(C1)
        table.insert(sprites, s1)
        local s2 = drawableSpriteStruct.fromTexture("VivHelper/holdableJumpThru/01", entity)
        s2:setJustification(0, 0)
        s2:addPosition(i * 8, 0)
        s2:useRelativeQuad(quadX, quadY, 8, 8)
        s2:setColor(C2)
        table.insert(sprites, s2)
    end
    return sprites
end

function HoldableJumpthru.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width, 8)
end

return {HoldableBarrier, HBC2, HoldableJumpthru}