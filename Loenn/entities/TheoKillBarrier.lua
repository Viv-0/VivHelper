local drawRect = require('structs.drawable_rectangle')
local drawableText = require('structs.drawable_text')
return {
    name = "VivHelper/TheoKillBarrier",
    placements = {
        name = "teo",
        data = {
            width = 8, height = 8,
        }
    },
    sprite = function(room,entity) return {
        drawRect.fromRectangle("fill",entity.x,entity.y,entity.width,entity.height,{0.251, 0.753, 0.816, 0.8}),
        drawableText.fromText("Theo Kill Barrier", entity.x,entity.y,entity.width,entity.height,nil,0.5,{1,1,1,1})
    } end
}