local puffer = {}
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

puffer.name = "VivHelper/ReskinnablePuffer"
puffer.depth = 0
puffer.placements = {
    {
        name = "left",
        data = {
            right = false,
            Directory = "objects/puffer"
        }
    },
    {
        name = "right",
        data = {
            right = true,
            Directory = "objects/puffer"
        }
    }
}
puffer.fieldInformation = {
    Directory = vivUtil.getDirectoryPathFromFile(false)
}
puffer.sprite = function(room,entity) 
    local sprite = vivUtil.getImageWithNumbers(entity.Directory .. "/idle",0,entity)
    sprite:setScale(entity.right and 1 or -1, 1)
    return sprite
end


function puffer.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.right = not entity.right
    end

    return horizontal
end

return puffer