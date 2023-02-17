local utils = require('utils')

local fields = {
     key="U,L,DR,UR,L,UL", CompleteFlag="DashCode", 
     Color="ffffff",
     GlitchLength="Medium",
     ClassName="", MethodName="", CustomParameters="",
}
local function getFields(spawnType)
    local t = utils.deepcopy(fields)
    t["spawnType"] = spawnType
    return {name = string.lower(spawnType), data = t}
end

local DCHC = { name = "VivHelper/DashCodeHeartController" }
DCHC.placements = {}
table.insert(DCHC.placements, getFields("Reflection"))
table.insert(DCHC.placements, getFields("ForsakenCity"))
table.insert(DCHC.placements, getFields("LevelUp"))
table.insert(DCHC.placements, getFields("FlashSpawn"))
table.insert(DCHC.placements, getFields("GlitchSpawn"))
table.insert(DCHC.placements, getFields("Custom"))


DCHC.ignoredFields = function(entity)
    local sT = entity.spawnType
    if sT == "LevelUp" then
        return {"multipleCheck", "GlitchLength", "CustomParameters", "ClassName", "MethodName"}
    elseif sT == "FlashSpawn" then
        return {"multipleCheck", "GlitchLength", "nodes", "CustomParameters", "ClassName", "MethodName"}
    elseif sT == "GlitchSpawn" then
        return {"multipleCheck", "Color", "nodes", "CustomParameters", "ClassName", "MethodName"}
    elseif sT == "Custom" then
        return {"multipleCheck", "Color", "GlitchLength"}
    else
        return {"multipleCheck", "GlitchLength", "Color", "CustomParameters", "ClassName", "MethodName"}
    end
end
DCHC.texture = "ahorn/VivHelper/heartCodeController"

DCHC.fieldInformation = {
    key = {fieldType = "string", validator = function(s)
        for _,ss in s:gmatch(',') do 
            local t = string.upper(ss)
            if t ~= 'R' and t ~= 'UR' and
               t ~= 'U' and t ~= 'UL' and
               t ~= 'L' and t ~= 'DL' and 
               t ~= 'D' and t ~= 'DR' then 
               return false
            end
        end
        return true end },
    Color = {fieldType = "color", allowXNAColors=true},
    GlitchLength = {fieldType = "string", options = {"Short", "Medium", "Long", "Glyph"}, editable = false}
}
return DCHC