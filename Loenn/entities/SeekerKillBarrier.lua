local drawRect = require('structs.drawable_rectangle')
local drawableText = require('structs.drawable_text')
return {
    name = "VivHelper/SeekerKillBarrier",
    placements = {
        name = "main",
        data = {
            width = 8, height = 8,
        }
    },
    sprite = function(room,entity) return {
        drawRect.fromRectangle("bordered",entity.x,entity.y,entity.width,entity.height,{0.816,0.188,0.188,0.5}, {0.2,0.2,0.2,0.6}),
        drawableText.fromText("Seeker Kill Barrier", entity.x,entity.y,entity.width,entity.height,nil,0.5,{1,1,1,1})
    } end
}