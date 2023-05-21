local dR = require('structs.drawable_rectangle')
local dS = require('structs.drawable_sprite')

return {
    name = "VivHelper/LightningMuter",
    depth = -1000000,
    sprite = function(room,entity) 
        local s = dS.fromTexture("ahorn/VivHelper/mute", entity)
        s:addPosition(entity.width/2, entity.height/2)
        return {
            dR.fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height, {0.55, 0.97, 0.96, 0.4}, {0.99, 0.96, 0.47, 1.0}),
            s
        }
    end,
    placements = {
        name = "lm",
        data = {
            width = 8,
            height = 8,
            flag = "LightningMuter"
        }
    }
}