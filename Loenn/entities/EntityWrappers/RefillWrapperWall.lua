local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local rectangle = require('structs.drawable_rectangle')
local func = require('structs.drawable_function')


local rww = {
    name = "VivHelper/RefillWallWrapper",
    depth = function(room, entity) return entity.Depth or 100 end
}

rww.fieldOrder = {
    "x","y",
    "width","height",
    "RespawnTime","oneUse",
    "TypeName","ImageVariableName",
    "RespawnMethodName","Depth",
    "InnerColor","OuterColor"
}

rww.fieldInformation = {
    InnerColor = {fieldType = "color", allowXNAColors = true, allowEmpty = false},
    OuterColor = {fieldType = "color", allowXNAColors = true, allowEmpty = false},
    Depth = {fieldType = "integer" }
}

rww.placements = {
    name = "wrap",
    data = {
        width = 8, height = 8, 
        RespawnTime = -1.0, oneUse = false,
        TypeName = "Refill", ImageVariableName = "sprite",
        RespawnMethodName = "Respawn", Depth = 100,
        InnerColor = "208020", OuterColor = "93bd40"
    }
}

rww.sprite = function(room,entity)
    local incolor = vivUtil.getColorTable(entity.InnerColor, true)
    local outcolor = vivUtil.getColorTable(entity.OuterColor, true)
    vivUtil.alphMult(incolor, (entity.oneUse ? 0.25 : 0.7))
    vivUtil.alphMult(outcolor, (entity.oneUse ? 0.25 : 0.7))
    return { rectangle.fromRectangle("bordered",entity.x,entity.y,entity.width,entity.height,incolor,outcolor),
        func.fromFunction(require('utils.drawing').printCenteredText,"Custom Refill Wall", entity.x,entity.y,entity.width,entity.height,nil,0.5)
    }
end

return rww