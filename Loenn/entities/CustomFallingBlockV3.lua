local fakeTilesHelper = require("helpers.fake_tiles")
local drawableSprite = require("structs.drawable_sprite")
local objectDepths = require('consts.object_depths')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local customFallingBlockv3 = {
    name = "VivHelper/CustomFallingBlock3",
}

customFallingBlockv3.placements = {
    name = "main",
    data = {
        width = 16,
        height = 16,
        tiletype = '3',
        firstTrigger = "none",
        finalBehavior = "RepeatLast",
        thruDashBlocks = true,
        Depth = -9000,
        operations0 = "270,180,0.4,false"
    }
}
customFallingBlockv3.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)

customFallingBlockv3.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype")(entity)
--[[
    orig["firstTrigger"] = {fieldType = "string", options = { {"Ignore" => "none"}, {"Trigger Automatically" => "false"}, {"Trigger On First Touch" => "true"}}, editable = false}
    orig["finalBehavior"] = {fieldType = "string", options = {"RepeatLast", "Stop", "Loop", "Shatter"}, editable = false}
    orig["Depth"] = {fieldType = "integer", options = objectDepths, editable = true }]]
    return orig
end
function customFallingBlockv3.depth(room, entity)
    return entity.Depth or -9000
end

return customFallingBlockv3