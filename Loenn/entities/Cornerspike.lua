local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local edges = {"UpLeft","UpRight","DownLeft","DownRight"}

local cs = {
    name = "VivHelper/Cornerspike",
    fieldOrder = {"type", "Color",
        "EdgeDirection","KillFormat", "DoNotAttach"
    },
    fieldInformation = {
        EdgeDirection = {fieldType = "string", options = {
        {"Top Left", "UpLeft"},
        {"Top Right", "UpRight"},
        {"Bottom Left", "DownLeft"},
        {"Bottom Right", "DownRight"},
        {"Inner Bottom Right", "InnerUpLeft"},
        {"Inner Bottom Left", "InnerUpRight"},
        {"Inner Top Right", "InnerDownLeft"},
        {"Inner Top Left", "InnerDownRight"}
        }},
        Color = {fieldType = "VivHelper.color", allowXNAColors = true }
    },
    texture = function(room,entity)
        local type = entity.type
        type = ((type == "default" or type == "outline") and "danger/spikes/corners/" .. type or type)
        return entity.type  .. "_" .. entity.EdgeDirection .. "00"
    end
}
cs.rotate = function(room,entity,direction)
    local inner = (entity.EdgeDirection:sub(1, 5) == "Inner") and "Inner" or ""
    local sub = entity.EdgeDirection:sub(inner and 0 or 6)
    local idx = vivUtil.indexof(edges,string.lower())
    entity.EdgeDirection = inner .. edges[vivUtil.mod(idx+direction, 4)]
end
cs.flip = function(room,entity,horizontal,vertical)
    if vertical then
        entity.EdgeDirection = entity.EdgeDirection:gsub("Up","XXX"):gsub("Down","Up"):gsub("XXX","Down")
    end
    if horizontal then
        entity.EdgeDirection = entity.EdgeDirection:gsub("Left","XXX"):gsub("Right","Left"):gsub("XXX","Right")
    end
end
cs.placements = {name = "main", data = {
    type = "default",
    Color = "ffffffff",
    EdgeDirection = "UpRight",
    DoNotAttach = true, KillFormat = false
}}

return cs