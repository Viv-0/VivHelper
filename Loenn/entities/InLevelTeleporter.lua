local atlases = require('atlases')
local drawableRectangle = require('structs.drawable_rectangle')
local drawableSprite = require('structs.drawable_sprite')
local drawableLine = require('structs.drawable_line')
local utils = require('utils')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local iltp = {
    name = "VivHelper/InLevelTeleporter",
    nodeLimits = {1,1},
    nodeVisibility = "always",
    nodeJustification = {0.5,0.5}
}

iltp.fieldInformation = {
    dir1 = {fieldType="string",options={"Right","Up","Left","Down"},editable=false},
    dir2 = {fieldType="string",options={"Right","Up","Left","Down"},editable=false},
    NumberOfUses1 = {fieldType="integer",minimumValue=-1},
    NumberOfUses2 = {fieldType="integer",minimumValue=-1},
    l = {fieldType="integer",minimumValue=2},
    Path = vivUtil.GetFilePathWithNoTrailingNumbers(true),
    Color = {fieldType="color", allowEmpty = true, allowXNAColors = true}
}

iltp.placements = {{
    name ="iltp",
    data = {
        l=16,
        dir1="Up",dir2="Down",
        flags1="",flags2="",
        sO1=0.0,sO2=0.0,
        eO1=false,eO2=false,
        cW=true,legacy=false,center=false,
        OutbackHelperMod=false,
        allActors=false,
        NumberOfUses1=-1,
        NumberOfUses2=-1,
        Audio="",
        AlwaysRetainSpeed = true
    }},{
    name ="iltp2",
    data = {
        l=16,
        dir1="Up",dir2="Down",
        flags1="",flags2="",
        sO1=0.0,sO2=0.0,
        eO1=false,eO2=false,
        cW=true,legacy=false,center=false,
        OutbackHelperMod=false,
        allActors=false,
        NumberOfUses1=-1,
        NumberOfUses2=-1,
        AlwaysRetainSpeed = true,
        Audio="",
        Path="VivHelper/portal/portal",
        Color="White"
    }
    }
}

local directionalValues = {
    Right = { 
        rect = function(x,y,l) return utils.rectangle(x, y, 16, l) end,
        imagePosMod = function(img, entity, x, y, i) 
            img:setJustification(0,0)
            img:setPosition(x+16, y+entity.l-i)
            img.rotation = math.pi
        end,
        centerPoint = function(x,y,l) return {x + 16, y + l / 2} end
    },
    Down = {
        rect = function(x,y,l) return utils.rectangle(x, y, l, 16) end,
        imagePosMod = function(img, entity, x, y, i) 
            img:setJustification(0,0)
            img:setPosition(x+i, y+16)
            img.rotation = math.pi * 1.5
        end,
        centerPoint = function(x,y,l) return {x + l/2, y + 15} end
    },
    Left = {
        rect =function(x,y,l) return utils.rectangle(x-16, y, 16, l) end,
        imagePosMod = function(img, entity, x, y, i) 
            img:setJustification(0,0)
            img:setPosition(x-16, y+i)
        end,
        centerPoint = function(x,y,l) return {x - 15, y + l/2} end
    },
    Up = {
        rect = function(x,y,l) return utils.rectangle(x, y-16, l, 16) end,
        imagePosMod = function(img, entity, x, y, i) 
            img:setJustification(0,0)
            img:setPosition(x+entity.l-i, y-16)
            img.rotation = math.pi / 2
        end,
        centerPoint = function(x,y,l) return {x + l/2, y - 16} end
    }
}

local function getSprite(entity)
    local path = entity.Path
    local ret = nil
    if not vivUtil.isNullEmptyOrWhitespace(entity.Path) then
        ret = atlases.getResource(entity.Path, "Gameplay")
        if not ret then
            for i = 1,6,1 do 
                ret = atlases.getResource(entity.Path .. string.format("%0"..tostring(i).."d", 0), "Gameplay") -- path0, path00, path000, path0000, path00000, path000000
                if ret then break end
            end
        end
    end
    if not ret then
        ret = atlases.getResource("VivHelper/portal/portal", "Gameplay")
    end
    return ret
    
end
local function getDraw(entity, x, y, dir, nodeline)
    local sprites = {}
    local spriteMeta = getSprite(entity)
    for i = 0, entity.l-8, 8 do
        local quadY = 8

        if i == 0 then
            quadY = 0
        elseif i == entity.l-8 then
            quadY = 16
        end

        local s1 = drawableSprite.fromMeta(spriteMeta, entity)
        s1:useRelativeQuad(0, quadY, 8, 8)
        directionalValues[dir].imagePosMod(s1, entity, x, y, i)
        table.insert(sprites, s1)
    end
    if nodeline then 
        local a = directionalValues[entity.dir1].centerPoint(entity.x,entity.y,entity.l)
        local b = directionalValues[entity.dir2].centerPoint(entity.nodes[1].x,entity.nodes[1].y,entity.l)
        local line = drawableLine.fromPoints({a[1], a[2], b[1], b[2]}, vivUtil.oldGetColorTable(entity.Color, true, {1,1,1,1}), 1)
        table.insert(sprites, line)
    end
    return sprites
end

iltp.sprite = function(room,entity)
    return getDraw(entity, entity.x, entity.y, entity.dir1, true)
end
iltp.nodeSprite = function(room,entity,node,nodeIndex)
    return getDraw(entity, node.x, node.y, entity.dir2)
end
iltp.selection = function(room,entity) 
    return directionalValues[entity.dir1].rect(entity.x, entity.y, entity.l), {directionalValues[entity.dir2].rect(entity.nodes[1].x, entity.nodes[1].y, entity.l)}
end

return iltp