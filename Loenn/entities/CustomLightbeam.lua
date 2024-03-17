local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local clb = {
    name = "VivHelper/CustomLightbeam",
    fieldOrder = {"width","height","rotation","Texture","Color","Alpha","ChangeFlag","DisableFlag","DisableParticlesFlag","Depth","FadeWhenNear","NoParticles"},
    fieldInformation = {
        Color = {fieldType = "VivHelper.oldColor", allowXNAColors = true },
        Texture = {fieldType = "path", allowFiles = true, allowFolders = false},
        Depth = {fieldType = "integer"}
    }
}
clb.placements = {
    name = "main",
    data = {
        width = 32,
        height = 24,
        rotation = 0,
        ChangeFlag = "",
        DisableFlag = "",
        Color = "ffffff",
        Alpha = 1.0,
        Texture = "util/lightbeam",
        Depth = -9998,
        FadeWhenNear = true,
        NoParticles = false,
        DisableParticlesFlag = ""
    }
}

local drawing = require("utils.drawing")
local utils = require("utils")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local function getSprites(room, entity, onlyBase)
    -- Shallowcopy so we can change the alpha later
    local color = vivUtil.oldGetColorTable(entity.Color or {0.8, 1.0, 1.0, 0.4})

    if not color[4] then
        color[4] = 0.4
    end

    local sprites = {}
    local x, y = entity.x, entity.y

    local spr = entity.Texture
    if vivUtil.isNullEmptyOrWhitespace(spr) then spr = "util/lightbeam" end

    local sprite = drawableSprite.fromTexture(spr, entity)
    local theta = math.rad(entity.rotation or 0)
    local width = entity.width or 32
    local height = entity.height or 24
    local halfWidth = math.floor(width / 2)
    local widthOffsetX, widthOffsetY = halfWidth * math.cos(theta), halfWidth * math.sin(theta)
    local widthScale = (height - 4) / sprite.meta.width
    -- the part that works cleanly
    sprite:addPosition(widthOffsetX, widthOffsetY)
    sprite.color = color
    sprite:setJustification(0.0, 0.0)
    sprite:setScale(widthScale, width)
    sprite.rotation = theta + math.pi / 2

    table.insert(sprites, sprite)
    -- Selection doesn't need the extra visual beams
    if not onlyBase then
        utils.setSimpleCoordinateSeed(x, y)
        for i = 0, width - 1, 4 do
            local num = i * 0.6
            local lineWidth = 4 + math.sin(num * 0.5 + 1.2) * 4.0
            local alpha = 0.6 + math.sin(num + 0.8) * 0.3
            local offset = math.sin((num + i * 32) * 0.1 + math.sin(num * 0.05 + i * 0.1) * 0.25) * (width / 2.0 - lineWidth / 2.0)

            color[4] = alpha

            -- Makes rendering a bit less boring, not used by game
            local offsetMultiplier = (math.random() - 0.5) * 2

            for j = 1, 2 do
                local beamSprite = utils.deepcopy(sprite)
                local beamWidth = math.random(-4, 4)
                local extraOffset = offset * offsetMultiplier - width + beamWidth
                local offsetX = utils.round(extraOffset * math.cos(theta))
                local offsetY = utils.round(extraOffset * math.sin(theta))
                local beamLengthScale = (height - math.random(4, math.floor(height / 2)))/ beamSprite.meta.width

                beamSprite:addPosition(widthOffsetX, widthOffsetY)
                beamSprite:addPosition(offsetX, offsetY)
                beamSprite.color = color
                beamSprite:setJustification(0.0, 0.0)
                beamSprite:setScale(beamLengthScale, beamWidth)
                beamSprite.rotation = theta + math.pi / 2

                table.insert(sprites, beamSprite)
            end
        end
    end

    return sprites
end

clb.sprite = function(room,entity) return getSprites(room,entity,false) end

clb.selection = function(room,entity)
    local baseSprite = getSprites(room, entity,true)[1]

    return baseSprite:getRectangle()
end

clb.rotate = function(room, entity, direction)
    entity.rotation = ((entity.rotation or 0) + direction * 45) % 360

    return true
end

return clb