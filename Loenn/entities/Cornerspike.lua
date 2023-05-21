local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local edges = {"UpLeft","UpRight","DownLeft","DownRight"}

local renderSupport = {
    UpLeft = {1,1},
    UpRight = {0,1},
    DownLeft = {1,0},
    DownRight = {0,0},
}

local cs = {
    name = "VivHelper/CornerSpike",
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
    depth = -2,
    texture = function(room,entity)
        local t = entity.type
        t = ((t == "default" or t == "outline") and "danger/spikes/corners/" .. t or t)
        return t .. "_" .. entity.EdgeDirection .. "00"
    end,
    justification = function(room,entity)
        local sub = entity.EdgeDirection:sub(entity.EdgeDirection:sub(1, 5) == "Inner" and 6 or 0)
        return renderSupport[sub]
    end

}
cs.rotate = function(room,entity,direction)
    local inner = (entity.EdgeDirection:sub(1, 5) == "Inner") and "Inner" or ""
    local sub = entity.EdgeDirection:sub(vivUtil.isNullEmptyOrWhitespace(inner) and 6 or 0)
    local idx = vivUtil.indexof(edges,sub)
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