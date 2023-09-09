local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local edges = {"UpLeft","UpRight","DownRight","DownLeft"}

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
    color = function(room,entity) 
        vivUtil.getColorTable(entity.Color, true, {1,1,1,1})
    end,
    justification = function(room,entity)
        local sub = entity.EdgeDirection:sub(entity.EdgeDirection:sub(1, 5) == "Inner" and 6 or 0)
        return renderSupport[sub]
    end

}
local function starts(String,Start)
    return string.sub(String,1,string.len(Start))==Start
 end

cs.rotate = function(room,entity,direction)
    local a = starts(entity.EdgeDirection,"Inner") and 5 or 0
    local pre = string.sub(entity.EdgeDirection, 1, a)
    local post = string.sub(entity.EdgeDirection, a+1)
    local startIndex = vivUtil.indexof(edges, post)
    if not startIndex then return false end
    local realIndex = vivUtil.mod(startIndex - 1 + direction, 4) + 1
    entity.EdgeDirection = pre .. edges[realIndex]

    return true
end
cs.flip = function(room,entity,horizontal,vertical)
    if vertical then
        entity.EdgeDirection = entity.EdgeDirection:gsub("Up","XXX"):gsub("Down","Up"):gsub("XXX","Down")
    end
    if horizontal then
        entity.EdgeDirection = entity.EdgeDirection:gsub("Left","XXX"):gsub("Right","Left"):gsub("XXX","Right")
    end
end
cs.placements = {}
for _,edge in ipairs(edges) do
    table.insert(cs.placements,{
    name = edge, data = {
        type = "default",
        Color = "ffffff",
        EdgeDirection = edge,
        DoNotAttach = true, KillFormat = false
    }})
    
end
return cs