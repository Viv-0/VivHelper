local fakeTilesHelper = require("helpers.fake_tiles")

local customFallingBlock = {}

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
    },
    fieldInformation = {Direction = {options = {"Down","Right","Left","Up"}}}
}

customFallingBlock.rotate = require('mods').requireFromPlugin('libraries.vivUtil').getDataRotationHandler("Direction", {"Up","Right","Down","Left"})

customFallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
customFallingBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

function customFallingBlock.depth(room, entity)
    return entity.behind and 5000 or 0
end

return customFallingBlock