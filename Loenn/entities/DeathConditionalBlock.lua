local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local fakeTilesHelper = require('helpers.fake_tiles')

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


deathCon.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendIn")
deathCon.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype")(entity)
    orig["DeathCount"] = {fieldType = "integer"}
    return orig
end
return deathCon

