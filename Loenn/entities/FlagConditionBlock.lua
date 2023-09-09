local fakeTilesHelper = require('helpers.fake_tiles')
local drawableSprite = require("structs.drawable_sprite")

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


flagConBlock.sprite = function(room,entity,node)
    local orig = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendIn")(room,entity,node)
    local sprite = drawableSprite.fromTexture(((entity.width > 15 and entity.height > 15) and "ahorn/VivHelper/flag" or "ahorn/VivHelper/smolFlag"), entity)
    sprite:setJustification(0.5,0.5)
    sprite:addPosition(entity.width / 2, entity.height / 2)
    sprite:setColor({0.7,0.7,0.7,0.7})
    table.insert(orig, sprite)
    return orig
end
flagConBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return flagConBlock

