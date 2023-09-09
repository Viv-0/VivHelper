local drawableRect = require('structs.drawable_rectangle')
local drawableSprite = require('structs.drawable_sprite')

return {
    name = "VivHelper/EntityMuter",
    placements = {
        name = "main",
        data = {
            width = 8, height = 8, 
            Types = "", all = true
        }
    },
    sprite = function(room,entity) return {
        drawableRect.fromRectangle("fill",entity.x,entity.y,entity.width,entity.height,{0.4,0.4,0.4,0.4}),
        drawableSprite.fromTexture('ahorn/VivHelper/mute', entity):setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)
    } end
    
}