local fakeTilesHelper = require('helpers.fake_tiles')

local FloatyBreakBlock = {}

FloatyBreakBlock.name = "VivHelper/FloatyBreakBlock"
FloatyBreakBlock.depth = -13000
FloatyBreakBlock.placements = {
    name = "main",
    data = {
        tiletype = "3",
        blendin = false,
        width = 8,
        height = 8,
        delay=1.0,
        delayType="timer",
        sidekill=false,
        disableSpawnOffset=false
    }
}

FloatyBreakBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")

FloatyBreakBlock.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype")(entity)
    orig["delayType"] = {fieldType = "string", options = {"timer","sinking","pressure"}, editable=false}
    return orig
end
return FloatyBreakBlock

