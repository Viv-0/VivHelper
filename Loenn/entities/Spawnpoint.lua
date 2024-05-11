local colors = require('consts.xna_colors')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local drawable_sprite = require('structs.drawable_sprite')
local drawable_function = require('structs.drawable_function')
local tagHelper = require('libraries.tagHelper')

local colorSet = vivUtil.sortByKeys(colors)

local spawnPoint = {
    name = "VivHelper/Spawnpoint", 
    justification = {0.5,1.0},
    texture = function(room,entity) if vivUtil.isNullEmptyOrWhitespace(entity.Texture) then return "VivHelper/player_outline" else return entity.Texture end end,
    color = {0.675,0.196,0.196,1.0},
    depth = function(room, entity) return entity.Depth or 5000 end
}

spawnPoint.placements = { {
    name = "custom",
    data = {
        Color = "FFFFFF",
        OutlineColor = "000000",
        forceFacing = 0,
        NoResetOnRespawn = false,
        NoResetOnRetry = false,
        HideInMap = false,
        Texture = "",
        Depth = 5000,
    }},
    {name = "hide", data = {
        Color = "FFFFFF",
        OutlineColor = "000000",
        forceFacing = 0,
        HideInMap = false,
    }},
    {name = "reset", data = {
        Color = "FFFFFF",
        OutlineColor = "000000",
        forceFacing = 0,
        NoResetOnRespawn = true,
        NoResetOnRetry = false,
    }}
}
spawnPoint.fieldInformation = {
    Color = {fieldType = "color", allowXNAColors = true, allowEmpty = true},
    OutlineColor = {fieldType = "color", allowXNAColors = true, allowEmpty = true},
    Texture = {fieldType = "path", allowEmpty = true},
    forceFacing = {fieldType = "integer", options = {{"Normal", 0},{"Face Right", 1}, {"Face Left", -1}}, editable = false}
}


local tpSpawn = {
    name = "VivHelper/InterRoomSpawner", 
    depth = function(room, entity) return entity.Depth or 5000 end
}

tpSpawn.placements = {
    {name = "tpIn",
    data = {
        tag = 1,
        forceFacing = 0,
    }},
    {name = "tpInCustom",
    data = {
        tag = 1,
        forceFacing = 0,
        Texture = "", Depth = 5000,
        Color = "FFFFFF", OutlineColor = "000000",
        HideInMap = false,
        Flags=""
    }}
}

tpSpawn.fieldInformation = {
    forceFacing = {fieldType = "integer", options = {{"Normal", 0},{"Face Right", 1}, {"Face Left", -1}}, editable = false},
    tag = {fieldType = "VivHelper.tag", fieldSubtype = "integer"},
    Color = {fieldType = "color", allowXNAColors = true, allowEmpty = true},
    OutlineColor = {fieldType = "color", allowXNAColors = true, allowEmpty = true},
    Texture = {fieldType = "path", allowEmpty = true},
}

vh_tag.addTagControlToHandler(tpSpawn, "tag", "spawnpoint", false)

tpSpawn.sprite = function(room, entity)
    if not entity.tag or entity.tag == 0 then
        local sprites = {
            drawable_sprite.fromTexture("ahorn/VivHelper/", entity),
            drawable_function.fromFunction(vivUtil.printJustifyText, 
            "Add a tag to link this to a Spawn Target!",
            entity.x, entity.y - 22, 240, 20, love.graphics.getFont(), 0.5, true)
        }
        sprites[1]:setJustification(0.5,1)
        return sprites

    else
        local s = drawable_sprite.fromTexture(entity.Texture or "VivHelper/player_outline", entity)
        s:setJustification(0.5, 1)
        s:setColor(colors[colorSet[vivUtil.mod(237*(entity.tag+431), 140) + 1]])
        return s;
    end
end

local tpSpawnOut = {
    name = "VivHelper/InterRoomSpawnTarget", 
    depth = function(room, entity) return entity.Depth or 5000 end
}


tpSpawnOut.placements = {
    {name = "tpOut",
    data = {
        tag = 1,
        forceFacing = 0,
    }},
    {name = "tpOutCustom",
    data = {
        tag = 1,
        forceFacing = 0,
        Texture = "", Depth = 5000,
        Color = "FFFFFF", OutlineColor = "000000",
        HideInMap = false,
        Flags=""
    }}
}

tpSpawnOut.fieldInformation = {
    forceFacing = {fieldType = "integer", options = {{"Normal", 0},{"Face Right", 1}, {"Face Left", -1}}, editable = false},
    
    Color = {fieldType = "color", allowXNAColors = true, allowEmpty = true},
    OutlineColor = {fieldType = "color", allowXNAColors = true, allowEmpty = true},
    Texture = {fieldType = "path", allowEmpty = true},
}
vh_tag.addTagControlToHandler(tpSpawn, "tag", "spawnpoint", true)


return spawnPoint 