local drawableSprite = require('structs.drawable_sprite')
local utils = require('utils')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local chl = {
    name = "VivHelper/CustomHangingLamp",
    fieldOrder = {
        "height", "directory",
        "AnimationSpeed","WeightMultiplier",
        "BloomRadius","BloomAlpha",
        "lightColor",
        "LightFadeIn","LightFadeOut",
        "AudioPath", "DrawOutline"
    },
    fieldInformation = {
        directory = {fieldType = "path", allowFiles = true, allowFolders = false, filenameProcessor = function(filename, rawFilename, prefix) return vivUtil.trim(filename):sub(1,filename:match('^.*()/')-1) end},
        LightColor = {fieldType = "VivHelper.oldColor", allowXNAColors = true},
        lightColor = {fieldType = "color", allowXNAColors = true, useAlpha},
    },
    minimumSize = {8, 16}
}
chl.placements = {
    name = "main",
    data = {
        height = 16,
        directory="VivHelper/customHangingLamp", AnimationSpeed=0.2,
    BloomAlpha=1.0, BloomRadius=48,
    lightColor="ffffffff",
    LightFadeIn=24, LightFadeOut=48,
    AudioPath="event:/game/02_old_site/lantern_hit",
    WeightMultiplier=1.0,
    DrawOutline=true
    }
}
-- Manual offsets and justifications of the sprites
function chl.sprite(room, entity)
    local sprites = {}
    local h = math.max(entity.height or 0, 16)
    local s0 = entity.directory
    if vivUtil.isNullEmptyOrWhitespace(s0) then s0 = "VivHelper/customHangingLamp" end
    local s1 = entity.Suffix or ""

    local topSprite = drawableSprite.fromTexture(s0.."/base" .. s1 .. "00", entity)
    local middleSprite = drawableSprite.fromTexture(s0.."/chain" .. s1 .. "00", entity)
    local bottomSprite = drawableSprite.fromTexture(s0.."/lamp" .. s1 .. "00", entity)
    topSprite:setJustification(0.5, 0)
    topSprite:setOffset(0, 0)

    table.insert(sprites, topSprite)
    
    for i = 0, h - topSprite.meta.height - bottomSprite.meta.height, middleSprite.meta.height do
        ms = utils.deepcopy(middleSprite)
        ms:setJustification(0.5, 0)
        ms:setOffset(0, 0)
        ms:addPosition(0, i)

        table.insert(sprites, ms)
    end

    
    bottomSprite:setJustification(0, 0)
    bottomSprite:setOffset(0, 0)
    bottomSprite:addPosition(0, h - bottomSprite.meta.height)

    table.insert(sprites, bottomSprite)
    local colTable 
    if entity.LightColor and entity.LightAlpha then
        colTable = vivUtil.alphMult(vivUtil.oldGetColorTable(entity.LightColor, true, {1,1,1,1}), entity.LightAlpha * 0.6)
    else
        colTable = vivUtil.newGetColorTable(entity.lightColor, true, {1,1,1,1})
        if colTable[4] and (colTable[1] > colTable[4] or colTable[2] > colTable[4] or colTable[3] > colTable[4]) then
            colTable = vivUtil.alphMult({colTable[1], colTable[2], colTable[3]}, colTable[4] * 0.6)
        end
    end
    table.insert(sprites, vivUtil.drawableCircle("line", entity.x + 4, entity.y + entity.height - 4, entity.LightFadeOut, colTable))

    return sprites
end

function chl.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, math.max(entity.height, 16))
end

return chl