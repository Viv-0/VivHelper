local fakeTilesHelper = require('helpers.fake_tiles')

local exitDashBlock = {}

exitDashBlock.name = "VivHelper/ExitDashBlock"
exitDashBlock.depth = -13000
exitDashBlock.placements = {
    name = "main",
    data = {
        tiletype = "3",
        blendin = false,
        width = 8,
        height = 8,
        canDash = true,
        permanent = false,
    }
}


exitDashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
exitDashBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return exitDashBlock

