local fakeTilesHelper = require("helpers.fake_tiles")

local dashBlock = {}

dashBlock.name = "VivHelper/CustomDashBlock"
dashBlock.depth = 0
dashBlock.placements = {
    name = "main",
    data = {
        tiletype = "3",
        blendin = true,
        canDash = true,
        permanent = true,
        width = 8,
        height = 8
    }, fieldInformation = {
        AudioEvent = {options = {"event:/game/general/wall_break_dirt", "event:/game/general/wall_break_wood", "event:/game/general/wall_break_ice", "event:/game/general/wall_break_stone"}}
    }
}

dashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
dashBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return dashBlock