local fakeTilesHelper = require("helpers.fake_tiles")

local depthTileEntity = {}

depthTileEntity.name = "VivHelper/CustomDepthTileEntity"
depthTileEntity.depth = function(room,entity) return entity.Depth or -10000 end
depthTileEntity.placements = {
    name = "main",
    data = {
        tiletype = "3",
        Depth = -10000,
        width = 8,
        height = 8
    }, fieldInformation = {
        Depth = {fieldType = "integer"}
    }
}

depthTileEntity.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
depthTileEntity.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return depthTileEntity