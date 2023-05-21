local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local fakeTilesHelper = require('helpers.fake_tiles')

local enterBlock = {}

enterBlock.name = "VivHelper/EnterBlock"
enterBlock.depth = -13000
enterBlock.placements = {
    name = "main",
    data = {
        tiletype = "3",
        width = 8,
        height = 8,
        playTransitionReveal=false
    }
}


enterBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", true)
enterBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return enterBlock

