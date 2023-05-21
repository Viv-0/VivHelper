local fakeTilesHelper = require('helpers.fake_tiles')

local flagConBlock = {}

flagConBlock.name = "VivHelper/FlagConditionBlock2"
flagConBlock.depth = -13000
flagConBlock.placements = {
    name = "main",
    data = {
        tiletype = "3",
        blendin = false,
        width = 8,
        height = 8,
        Flag = "",
        InvertFlag=false,
        IgnoreStartVal=true,
        StartVal=false
    }
}


flagConBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
flagConBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return flagConBlock

