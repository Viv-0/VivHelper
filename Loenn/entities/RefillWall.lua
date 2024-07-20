local drawRect = require('structs.drawable_rectangle')
local drawSprite = require('structs.drawable_sprite')

return {
    name = "VivHelper/RefillWall",
    placements = {
        name = "wall",
        data = {
            width = 8, height = 8, 
            twoDashes = false, oneUse = false,
            Alpha = 1.0, RespawnTime = -1,
        }
    },
    sprite = function(room,entity)
        local t = entity.twoDashes
        local u = entity.oneUse and 0.25 or 0.7

        local incolor = {.125, .5, .125, u}
        local outcolor = {.576, .741, .251, 0.7}
        local sprite = "objects/refill/idle00"
        if t then 
            incolor = {.738, .25, .578, u}
            outcolor = {.886, .408, .82, 0.7}
            sprite = "objects/refillTwo/idle00"
        end
        return {
        drawRect.fromRectangle("bordered",entity.x,entity.y, entity.width, entity.height, incolor, outcolor),
        drawSprite.fromTexture(sprite, entity):setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)}
    end
}