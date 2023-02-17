local fakeTilesHelper = require('helpers.fake_tiles')

local coverupWall = {}

coverupWall.name = "VivHelper/CustomCoverupWall"
coverupWall.depth = function(room,entity) return entity.Depth or -13000 end
coverupWall.placements = {
    name = "main",
    data = {
        tiletype = "3",
        width = 8,
        height = 8,
        alpha = 1.0,
        Depth = -13000,
        instant = true,
        flag = "",
        inverted = false
    },
    fieldInformation = {instant = {options = {{"Instant", true}, {"Gradual", false}}}}
}

coverupWall.sprite = function(room,entity) return fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendIn", "tilesFg", {entity.alpha, entity.alpha, entity.alpha, entity.alpha * 0.7})(room,entity) end
coverupWall.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return coverupWall