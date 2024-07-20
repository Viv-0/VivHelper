local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local rectangle = require('structs.drawable_rectangle')
local text = require('structs.drawable_text')


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
    "innerColor","outerColor"
}

rww.fieldInformation = {
    innerColor = {fieldType = "color", allowXNAColors = true, allowEmpty = false, useAlpha = true},
    outerColor = {fieldType = "color", allowXNAColors = true, allowEmpty = false, useAlpha = true},
    Depth = {fieldType = "integer" },
    ImageVariableName = {fieldType = "string", options = {{"Use Refill Render", "$render"}}, editable = true}
}

rww.placements = {
    name = "wrap",
    data = {
        width = 8, height = 8, 
        RespawnTime = -1.0, oneUse = false,
        TypeName = "Refill", ImageVariableName = "sprite",
        RespawnMethodName = "Respawn", Depth = 100,
        innerColor = "208020", outerColor = "93bd40"
    }
}

rww.sprite = function(room,entity)
    local incolor = vivUtil.GetColorTable(entity, "InnerColor", "innerColor", true, {0.125, 0.5, 0.125, 1})
    local outcolor = vivUtil.GetColorTable(entity, "OuterColor", "outerColor", true, {0.576, 0.741, 0.251, 1})
    vivUtil.alphMult(incolor, (entity.oneUse ? 0.25 : 0.7))
    vivUtil.alphMult(outcolor, (entity.oneUse ? 0.25 : 0.7))
    return { rectangle.fromRectangle("bordered",entity.x,entity.y,entity.width,entity.height,incolor,outcolor),
        text.fromText("Custom Refill Wall", entity.x,entity.y,entity.width,entity.height,nil,0.5)
    }
end

return rww