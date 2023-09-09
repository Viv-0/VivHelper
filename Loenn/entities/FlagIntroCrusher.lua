local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")

local introCrusher = {}

introCrusher.name = "VivHelper/FlagIntroCrusher"
introCrusher.depth = 0
introCrusher.nodeLineRenderType = "line"
introCrusher.nodeLimits = {1, 1}
introCrusher.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")
introCrusher.placements = {
    name = "intro_crusher",
    data = {
        tiletype = "3",
        flags = "",
        width = 8,
        height = 8,
        delay=1.2,
        speed=2.0,
    }
}

introCrusher.sprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false)
introCrusher.nodeSprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false)

function introCrusher.nodeRectangle(room, entity, node)
    return utils.rectangle(node.x or 0, node.y or 0, entity.width or 8, entity.height or 8)
end

return introCrusher