local utils = require('utils')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local reskinJelly = {
    name = "VivHelper/ReskinnableJellyfish",
    fieldOrder = {
        "x","y",
        'bubble','tutorial',
        'Directory','Depth',
        'GlidePath','GlowPath',
        'GlideColor1','GlideColor2',
        'GlowColor1','GlowColor2'        
    },
    fieldInformation = {
        Directory = vivUtil.getDirectoryPathFromFile(false),
        Depth = {fieldType = 'integer'},
        GlidePath = vivUtil.GetFilePathWithNoTrailingNumbers(true),
        GlowPath = vivUtil.GetFilePathWithNoTrailingNumbers(true),
        GlideColor1 = {fieldType = 'color', allowXNAColors = true, allowEmpty = true },
        GlideColor2 = {fieldType = 'color', allowXNAColors = true, allowEmpty = true },
        GlowColor1 = {fieldType = 'color', allowXNAColors = true, allowEmpty = true },
        GlowColor2 = {fieldType = 'color', allowXNAColors = true, allowEmpty = true }
    }
}

reskinJelly.placements = {
    name = "jelly",
    data = {
        bubble = false, tutorial = false,
        Directory = "objects/glider", Depth = -5,
        GlidePath = "particles/rect", GlowPath = "",
        GlideColor1="4FFFF3", GlideColor2="FFF899",
        GlowColor1="B7F3FF", GlowColor2="F4FDFF"
    }
}

reskinJelly.sprite = function(room,entity)
    local sprite = vivUtil.getImageWithNumbers(entity.Directory .. "/idle", 0, entity)
    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()

        table.insert(lineSprites, sprite)

        return lineSprites
    else
        return sprite 
    end
end

return reskinJelly