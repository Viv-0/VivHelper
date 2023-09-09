local fakeTilesHelper = require("helpers.fake_tiles")

local fakeWall = {}

fakeWall.name = "VivHelper/CustomFakeWall"
fakeWall.depth = function(entity,room) return entity.depth end
fakeWall.placements = {
    name = "fake_wall",
    data = {
        tiletype = "3",
        width = 8,
        height = 8,
        depth = -13000,
        audioEvent = "",
        playReveal = "NotOnTransition",
        permanent = true
    }
}

fakeWall.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg", {1.0, 1.0, 1.0, 0.7})
fakeWall.fieldInformation = function(entity)
    local orig = fakeTilesHelper.getFieldInformation("tiletype")(entity)
    orig["Depth"] ={
        fieldType = "integer",
        options = require('consts.object_depths')
    }
    orig['playReveal'] = {
        fieldType = "string",
        options = {"Never","NotOnTransition","Always"}
    }
    return orig
end

return fakeWall