local drawableSprite = require("structs.drawable_sprite")
local celesteEnums = require("consts.celeste_enums")
local utils = require("utils")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local templeGate = {}

local textureOptions = {}
local typeOptions = celesteEnums.temple_gate_modes

templeGate.name = "VivHelper/SeekerGate"
templeGate.depth = -9000
templeGate.canResize = {false, false}
templeGate.fieldInformation = 
{
    Directory = vivUtil.GetFilePathWithNoTrailingNumbers(false)
}
templeGate.placements = {
    name = "gate",
    data = {
        OpenRadius = 64.0,
        CloseRadius = 80.0,
        Directory = "VivHelper/seekergate/seekerdoor"
    }
}

function templeGate.sprite(room, entity)
    local sprite =  vivUtil.getImageWithNumbers(entity.Directory, 0, entity)
    local height = entity.height or 48

    -- Weird offset from the code, justifications are from sprites.xml
    sprite:setJustification(0.5, 0.0)
    sprite:addPosition(4, height - 48)

    return sprite
end

return templeGate