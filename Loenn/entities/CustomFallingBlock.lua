local fakeTilesHelper = require("helpers.fake_tiles")
local drawableSprite = require("structs.drawable_sprite")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local customFallingBlock = {}

local directionMatrices = {
    Right = 0,
    Up = math.pi * 1.5,
    Left = math.pi,
    Down = math.pi * 0.5
}

customFallingBlock.name = "VivHelper/CustomFallingBlock"
customFallingBlock.placements = {
    name = "main",
    data = {
        Accel = 500.0,
        MaxSpeed = 160.0,
        ShakeSFX="event:/game/general/fallblock_shake", ImpactSFX="event:/game/general/fallblock_impact",
        FlagOnFall="", FlagTrigger="", FlagOnGround="",
        tiletype = "3",
        climbFall = true, bufferClimbFall = false, Legacy = true,
        Direction = "Down",
        behind = false,
        width = 8,
        height = 8
    }
}

customFallingBlock.rotate = require('mods').requireFromPlugin('libraries.vivUtil').getDataRotationHandler("Direction", {"Right","Down","Left","Up"})

customFallingBlock.sprite = function(room,entity,node) 
    local orig = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)(room,entity,node)
    local arrow = drawableSprite.fromTexture("ahorn/VivHelper/arrow")
    arrow:setJustification(0.5,0.5)
    arrow:addPosition(entity.x + entity.width/2,entity.y + entity.height/2)
    arrow.rotation = directionMatrices[entity.Direction]
    vivUtil.addAll(orig, vivUtil.getBorder(arrow))
    table.insert(orig, arrow)
    return orig
end
customFallingBlock.fieldInformation = function(room,entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype")(entity)
    orig["Direction"] = {fieldType = "string", options = {"Down","Up","Left","Right"},editable = false}
    return orig
end
function customFallingBlock.depth(room, entity)
    return entity.behind and 5000 or 0
end

return customFallingBlock