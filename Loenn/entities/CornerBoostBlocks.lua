local entities = require('entities')
local utils = require('utils')
local fakeTilesHelper = require('helpers.fake_tiles')

local cbbhandlers = {
    {
        name = "VivHelper/CornerBoostBlock",
        placements = {
            name = "main",
            data = {
                width=8, height=8, tiletype="3", blendin=true
            }
        },
        sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin"),
        fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")
    }
}

for i,t in ipairs({"zipMover","dreamBlock","cassetteBlock","fallingBlock","swapBlock"}) do
    local a = utils.deepcopy(entities.registeredEntities[t])
    a.name = "VivHelper/CornerBoost" .. t:gsub("^%l", string.upper)
    table.insert(cbbhandlers, a)
end
return cbbhandlers