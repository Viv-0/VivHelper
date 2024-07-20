local Sprite = require('structs.drawable_sprite')
local drawRect =require('structs.drawable_rectangle')

return {
    name = "VivHelper/RippleSpace",
    placements = {
        name = "ripple",
        data = {
            width = 24, height = 24,
            RippleRate = 128
        }
    },
    fieldInformation = {
        RippleRate = {fieldType = "integer",minimumValue = 0, maximumValue = 255}
    },
    sprite = function(room,entity)
        local sprites = { drawRect.fromRectangle("bordered",entity.x,entity.y,entity.width,entity.height,{0.95,0.95,0.95,0.6},{0.9,0.9,0.9,0.5}) } 
        for i = 0,math.floor((width-1)/8) do
            for j = 0,math.floor((height-1)/8) do
                local s = Sprite.fromTexture("ahorn/VivHelper/ripple", entity)
                s:addPosition(8*i+4,8*j+4)
                table.insert(sprites, s)
            end
        end
        return sprites
    end
}