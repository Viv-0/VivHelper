local utils = require('utils')
local drawableSprite = require('structs.drawable_sprite')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local laserBlock = {
    name = "VivHelper/LaserBlock",
    depth = -9999
}

laserBlock.fieldInformation = {
    Directory = vivUtil.getDirectoryPathFromFile("Gameplay"),
    Direction = {fieldType = "integer", editable = false, options = {
        {"Right", 1},
        {"Up", 2},
        {"Up, Right", 3},
        {"Left", 4},
        {"Left, Right", 5},
        {"Left, Up", 6},
        {"Left, Up, Right", 7},
        {"Down", 8},
        {"Down, Right", 9},
        {"Down, Up", 10},
        {"Down, Up, Right", 11},
        {"Down, Left", 12},
        {"Down, Left, Right", 13},
        {"Down, Left, Up", 14},
        {"All Directions", 15}
    }}
}
laserBlock.fieldOrder = {
    "Direction", "Directory",
    "ChargeTime", "ActiveTime",
    "Delay", "StartDelay",
    "AttachToSolid", "Muted", "StartShooting",
    "Flag","DarknessAlpha"
}

laserBlock.placements = {
    name = "lb",
    data = {
        Direction=1,Directory="VivHelper/laserblock/techno",
        ChargeTime=1.4, ActiveTime=0.12,
        Delay=1.4, StartDelay=-1.0, 
        AttachToSolid=false, Muted=false, StartShooting=true,
        Flag="", DarknessAlpha=0.35
    }
}

laserBlock.selection = function(room,entity) return utils.rectangle(entity.x,entity.y,16,16) end
laserBlock.sprite = function(room,entity)
    local tex = (vivUtil.isNullEmptyOrWhitespace(entity.Directory) and "VivHelper/laserblock/techno" or entity.Directory)
    local z = entity.Direction
    local sprites = {drawableSprite.fromTexture(tex .. (z < 10 and "0" or "") .. tostring(z), entity):setJustification(0,0)}
    if z >= 8 then
        local a4 = drawableSprite.fromTexture("ahorn/VivHelper/arrow",entity)
        a4:setJustification(0, 0.5)
        a4:setColor({1.0,0.0,0.0,1.0})
        a4.rotation = math.pi / 2
        a4:addPosition(8,14)
        table.insert(sprites,a4)
        z = z - 8
    end
    if z >= 4 then
        local a3 = drawableSprite.fromTexture("ahorn/VivHelper/arrow",entity)
        a3:setJustification(0, 0.5)
        a3:setColor({1.0,0.0,0.0,1.0})
        a3.rotation = math.pi
        a3:addPosition(2,8)
        table.insert(sprites,a3)
        z = z - 4
    end
    if z >= 2 then
        local a2 = drawableSprite.fromTexture("ahorn/VivHelper/arrow",entity)
        a2:setJustification(0, 0.5)
        a2:setColor({1.0,0.0,0.0,1.0})
        a2.rotation = math.pi * 1.5
        a2:addPosition(8,2)
        table.insert(sprites,a2)
        z = z - 2
    end
    if z >= 1 then
        local a1 = drawableSprite.fromTexture("ahorn/VivHelper/arrow",entity)
        a1:setJustification(0, 0.5)
        a1:setColor({1.0,0.0,0.0,1.0})
        a1:addPosition(14,8)
        table.insert(sprites,a1)
    end
    return sprites    
end
 
return laserBlock