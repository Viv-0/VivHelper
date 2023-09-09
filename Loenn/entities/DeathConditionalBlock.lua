local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local fakeTilesHelper = require('helpers.fake_tiles')
local drawableSprite = require("structs.drawable_sprite")

local deathCon = {}

deathCon.name = "VivHelper/DeathConditionalBlock"
deathCon.depth = function(room,entity) return entity.Depth or -10000 end
deathCon.placements = {
    name = "main",
    data = {
        tiletype = "3",
        blendIn = false,
        width = 8,
        height = 8,
        DeathCount = 25,
        DisappearOnDeath = true
    }
}


deathCon.sprite = function(room,entity,node)
    local orig = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendIn")(room,entity,node)
    local sprite = drawableSprite.fromTexture("ahorn/VivHelper/skull", entity)
    sprite:setJustification(0.5,0.5)
    if entity.width < 16 or entity.height < 16 then
        sprite:setScale(0.5,0.5)
    end
    sprite:setColor({0.8,0.8,0.8,0.8})
    sprite:addPosition(entity.width / 2, entity.height / 2)
    table.insert(orig, sprite)
    return orig
end
deathCon.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype")(entity)
    orig["DeathCount"] = {fieldType = "integer"}
    return orig
end
return deathCon

