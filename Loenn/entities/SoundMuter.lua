local drawing = require('mods').requireFromPlugin('libraries.vivUtil')
local dsprite = require('structs.drawable_sprite')
local drawableFunction = require("structs.drawable_function")

return {
    name = "VivHelper/SoundMuter",
    depth = 9-math.huge,
    placements = {
        name = "soundmuter",
        data = {
            eventName="",flag="",
        }
    },
    sprite = function(room,entity) return drawableFunction.fromFunction(function()
        local font = love.graphics.getFont()
        local s = dsprite.fromTexture('ahorn/VivHelper/mute', entity):setPosition(entity.x + 4, entity.y+4)
        s:draw()
        drawing.printJustifyText(entity.eventName, entity.x-3, entity.y-15, 240, 20, font, 0.5, true, "left")
        drawing.printJustifyText(entity.flag, entity.x-3, entity.y+1, 240, 20, font, 0.5, true, "left")
    end) end,
    selection = function(room,entity) return require('utils').rectangle(entity.x-2, entity.y-2, 12, 12) end,
}