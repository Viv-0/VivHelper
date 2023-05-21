local fakeTilesHelper = require('helpers.fake_tiles')
local drawableSpriteStruct = require('structs.drawable_sprite')
local cGB = {
    name = "VivHelper/CelsiusGrowBlock",
    placements = {
        name = "main",
        data = {
            width=8,
            height=8,
            tiletype="3",
            moveX=0.0,
            moveY=1.0,
            flag=""
        }
    },
    nodeLimits = {0,-1}
}

cGB.sprite = function(room,entity,node)
    local sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")(room,entity,node)
    for _,a in ipairs({{2,2,1,1},{entity.width-2,2,-1,1},{2,entity.height-2,1,-1},{entity.width-2,entity.height-2,-1,-1}}) do
        local s = drawableSpriteStruct.fromTexture("ahorn/VivHelper/growEdge", entity)
        s:addPosition(a[1],a[2]); s:setScale(a[3],a[4]);
        table.insert(sprites,s)
    end
    return sprites
end
cGB.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")
return cGB