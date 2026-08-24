local colors = require('consts.xna_colors')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local colorSet = vivUtil.sortByKeys(colors)

local spawnPoint = {
    name = "VivHelper/Spawnpoint"
}
spawnPoint.justification = {0.5, 1.0}
spawnPoint.texture = "VivHelper/player_outline"
spawnPoint.color = function(room, entity) 
    return colors[colorSet[mod1(237*(entity.tag+431), 140)]]
end

spawnPoint.placements = { {
    name = "custom",
    data = {
        Color = "FFFFFF",
        OutlineColor = "000000",
        forceFacing = true,
        NoResetOnRespawn = false,
        NoResetOnRetry = false,
        HideInMap = false,
        Texture = "",
        Depth = 0,
        tag = 0,
    }},
    {name = "hide", data = {
        Color = "FFFFFF",
        OutlineColor = "000000",
        forceFacing = true,
        HideInMap = false,
    }},
    {name = "reset", data = {
        Color = "FFFFFF",
        OutlineColor = "000000",
        forceFacing = true,
        NoResetOnRespawn = true,
        NoResetOnRetry = false,
    }}
}

spawnPoint.depth = function(room,entity) return entity.depth end
spawnPoint.fieldInformation = {
    Color = {fieldType = "color", allowXNAColors = true, allowEmpty = true},
    OutlineColor = {fieldType = "color", allowXNAColors = true, allowEmpty = true},
    Texture = {fieldType = "path", allowEmpty = true},
    tag = {fieldType = "VivHelper.tag", fieldSubtype = "integer"}
}

return spawnPoint 