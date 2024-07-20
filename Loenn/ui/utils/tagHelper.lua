


local tagHelper = {}

function tagHelper.populateTags(map)
    local map = map or loadedState.map
    if not map then return end
    map._vivh_tags = {}
    local handler
    for _,room in ipairs(map.rooms) do
        for _,item in ipairs(room.entities) do
            handler = entities.registeredEntities[item._name]
            if handler then 
                local fieldInformation = utils.callIfFunction(handler.fieldInformation, item)
                if not fieldInformation then return end
                for propName, propData in pairs(fieldInformation) do
                    if propData._vivh_class then
                        AddTag(propData._vivh_class, item[propName], item)
                    end
                end
            end
        end
        for _,item in ipairs(room.triggers) do
            handler = triggers.registeredTriggers[item._name]
            if handler then 
                local fieldInformation = utils.callIfFunction(handler.fieldInformation, item)
                if not fieldInformation then return end
                for propName, propData in pairs(fieldInformation) do
                    if propData._vivh_class then
                        AddTag(propData._vivh_class, item[propName], item)
                    end
                end
            end
        end
    end
end
function tagHelper.AddTag(class, tag, item)
    local map = loadedState.map
    if not map then return end
    if not map._vivh_tags then tagHelper.populateTags() end 
    if not map._vivh_tags[class] then
        map._vivh_tags[class] = {}
    end
    if not map._vivh_tags[class][tag] then
        map._vivh_tags[class][tag] = {}
    end 
    if not map._vivh_tags[class][itemTag][item] then
        map._vivh_tags[class][itemTag][item] = true -- Add to table
    end
end

function tagHelper.RemoveTag(class, tag, item)
    if not loadedState.map._vivh_tags[class] then
        loadedState.map._vivh_tags[class] = {}
    end
    if not loadedState.map._vivh_tags[class][tag] then
        loadedState.map._vivh_tags[class][tag] = {}
    end 
    if loadedState.map._vivh_tags[class][itemTag][item] then
        loadedState.map._vivh_tags[class][itemTag][item] = nil -- Remove from table            
    end
end

local function getFormatTable(class, format, integer)
    local table = {_vivh_class = class, lonnExt_onSaveChanges = function(item, k, old, new)
        if old then
            tagHelper.RemoveTag(class, old, item)
        end
        if new then
            tagHelper.AddTag(class, new, item)
        end
    end}
    if format == "sender" then
        table.fieldType = integer and "VivHelper.tagIntSender" or "VivHelper.tagSender"
    elseif format == "receiver" then 
        table.fieldType = integer and "VivHelper.tagIntReceiver" or "VivHelper.tagReceiver"
    elseif format == "group" then 
        table.fieldType = integer and "VivHelper.tagIntGroup" or "VivHelper.tagGroup"
    else
        error("Format was not one of `sender`, `receiver`, or `group`")
    end
    return table
end

local formatToFieldType = {
    sender = "VivHelper.tagSender",
    receiver = "VivHelper.tagReceiver",
    group = "VivHelper.tagGroup"
}
local intFormatToFieldType = {
    sender = "VivHelper.tagIntSender",
    receiver = "VivHelper.tagIntReceiver",
    group = "VivHelper.tagIntGroup"
}

function tagHelper.addTagControlToHandler(handler, _format, propertyName, class, isInt)
    local t = type(handler.fieldInformation)
    local format
    if isInt then
        format = intFormatToFieldType[_format] or error("VivHelper - handler for " .. handler.name .. "has error with format " .. _format .. ", must match \"sender\", \"receiver\", or \"group\".")
    else
        format = formatToFieldType[_format] or error("VivHelper - handler for " .. handler.name .. "has error with format " .. _format .. ", must match \"sender\", \"receiver\", or \"group\".")
    end

    local table = getFormatTable(_format)
    if t == "function" then
        local orig = handler.fieldInformation
        handler.fieldInformation = function(...)
            local ret = orig(...)
            if ret[propertyName] then 
            else
                ret[propertyName] = table
            end
            return ret
        end
    elseif t == "table" then
        if  handler.fieldInformation[propertyName] then 
            for k,v in pairs(table) do
                handler.fieldInformation[propertyName][k] = v
            end
        else
            handler.fieldInformation[propertyName] = table
        end
    elseif t == "nil" then
        handler.fieldInformation = {
            [propertyName] = table
         }
    end
    if handler.onDelete then 
        local orig2 = handler.onDelete
        handler.onDelete = function(room, item, nodeIndex) 
            if nodeIndex == 0 then 
                tagHelper.RemoveTag(propertyName, class, item)
            end
            return orig2(room, entity, nodeIndex)
        end
    else 
        handler.onDelete = function(room, item, nodeIndex) 
            if nodeIndex == 0 then
                tagHelper.RemoveTag(propertyName, class, item)
            end
            return true
        end
    end
    if handler.lonnExt_finalizePlacement then 
        local orig3 = handler.lonnExt_finalizePlacement
        handler.lonnExt_finalizePlacement = function(room, layer, item)
            
        end
end

function tagHelper.retrieveStringTagsFromClass(class, ignore, sort)
    sort = sort or true
    local tags = {}
    if loadedState and loadedState.map and loadedState.map._vivh_tags and 
    type(loadedState.map._vivh_tags[class]) == "table" then
        local c = loadedState.map._vivh_tags[class]
        if (isInteger and #c or vivUtil.countStringKeys(c)) > 0 then
            for key,_ in pairs(c) do
                if type(key) == "string" and key ~= ignore then
                    table.insert(tags, key)
                end
            end 
            if sort then table.sort(tags) end
        end
    end
    return tags
end

function tagHelper.stringFieldCB(fieldInformation, field, newValue, oldValue) 
    if not (fieldInformation and fieldInformation._vivh_class and fieldInformation.validator(newValue) and field.metadata and field.metadata.formData) then return end

    local item = 

    if vivUtil.isNullEmptyOrWhitespace(oldValue) then
        tagHelper.RemoveTag(fieldInformation._vivh_class, oldValue, fieldInformation.fieldMetadata.formData._id)
    end
    tagHelper.AddTag(fieldInformation._vivh_class, newValue, fieldInformation.fieldMetadata.formData._id)
end

function tagHelper.getNextInteger(class)
    if not (loadedState and loadedState.map and loadedState.map._vivh_tags and type(loadedState.map._vivh_tags[class]) == "table") then return 0 end
    return #loadedState.map._vivh_tags[class]
end
function tagHelper.addNextInteger(class, itemID)
    local c = tagHelper.getNextInteger(class)
    if c == 0 then return 0 end
    loadedState.map._vivh_tags[class][c][itemID] = true
    return c
end

return tagHelper