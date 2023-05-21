local utils = require('utils')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

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
        return {"multipleCheck", "GlitchLength", "CustomParameters", "ClassName", "MethodName", "Name", "id"}
    elseif sT == "FlashSpawn" then
        return {"multipleCheck", "GlitchLength", "nodes", "CustomParameters", "ClassName", "MethodName", "Name", "id"}
    elseif sT == "GlitchSpawn" then
        return {"multipleCheck", "Color", "nodes", "CustomParameters", "ClassName", "MethodName", "Name", "id"}
    elseif sT == "Custom" then
        return {"multipleCheck", "Color", "GlitchLength", "Name", "id"}
    else
        return {"multipleCheck", "GlitchLength", "Color", "CustomParameters", "ClassName", "MethodName", "Name", "id"}
    end
end
DCHC.texture = "ahorn/VivHelper/heartCodeController"

DCHC.fieldInformation = {
    key = {fieldType = "string", validator = function(s)
        if vivUtil.isNullEmptyOrWhitespace(s) then return false end
        for ss in vivUtil.split(s, ',') do 
            if vivUtil.isNullEmptyOrWhitespace(ss) then return false end
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