local fakeTilesHelper = require('helpers.fake_tiles')
local drawableSpriteStruct = require('structs.drawable_sprite')
local drawableRectangle = require('structs.drawable_rectangle')
local nway = {
    name = "VivHelper/nWayDashBlock",
    placements = {
        name = "nway",
        data = {
            width=8,
            height=8,
            tiletype="3",
            blendin=true, canDash=true, permanent=true,
            Left=true, Right=true, Up=true, Down=true,
            detailColor="000000"
        }
    }
}
nway.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype")(entity)
    orig["detailColor"] =  {fieldType = "color",allowXNAColors=true}
    return orig
end

nway.sprite = function(room,entity,node)
    local sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")(room,entity,node)
    if(entity.Up)   then table.insert(sprites, drawableRectangle.fromRectangle("fill", entity.x + 5, entity.y + 1, entity.width - 10, 4, {0,0,0,0.5})) end
    if(entity.Left) then table.insert(sprites, drawableRectangle.fromRectangle("fill", entity.x + 1, entity.y + 5, 4, entity.height - 10, {0,0,0,0.5})) end
    if(entity.Right)then table.insert(sprites, drawableRectangle.fromRectangle("fill", entity.x + entity.width - 5, entity.y + 5, 4, entity.height - 10, {0,0,0,0.5})) end
    if(entity.Down) then table.insert(sprites, drawableRectangle.fromRectangle("fill", entity.x + 5, entity.y + entity.height - 5, entity.width - 10, 4, {0,0,0,0.5})) end
    return sprites
end

return nway