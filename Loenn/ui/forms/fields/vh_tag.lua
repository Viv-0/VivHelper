local stringField = require("ui.forms.fields.string")
local intField = require("ui.forms.fields.integer")
local utils = require("utils")
local loadedState = require("loaded_state")
local nilField = require("ui.forms.fields.nil")
local entities = require("entities")
local triggers = require("triggers")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local vh_tag = {}
vh_tag.fieldType = "VivHelper.tagString`"


local function tagFunction(table, key, value)
    if not table then print('error, no table found') return false end
    if value and value:match("%S") ~= nil then -- if value is not whitespace, empty, or nil
        if not table[key] then table[key] = {} end
        if not table[key][value] then
            table[key][value] = value
            return true
        end
    end
    return false
end

local function setTagsFromMap(handler, item) 
    local fieldInformation = (type(handler.fieldInformation) == "function" and handler.fieldInformation(item) or handler.fieldInformation)
    if not fieldInformation then return end
    for propName, propData in pairs(fieldInformation) do

        if propData.fieldType == "VivHelper.tagString" then

            tagFunction(loadedState.map._vivh_tags, propData._vivh_fieldSubdata, item[propName] or "")
        end
    end
end


local function populateTags() -- Initial repopulation based on entity values. Only happens on the first 
    loadedState.map._vivh_tags = {}
    for _,room in ipairs(loadedState.map.rooms) do
        for _,item in ipairs(room.entities) do
            handler = entities.registeredEntities[item._name]
            if handler then 
                setTagsFromMap(handler, item)
            end
        end
        for _,item in ipairs(room.triggers) do
            handler = triggers.registeredTriggers[item._name]
            if handler then 
                setTagsFromMap(handler, item)
            end
        end
    end
end

local function fieldCallback(options, self, value, previousValue)
    local tag = options._vivh_fieldSubdata
    if loadedState.side.map._vivh_tags[tag] then -- does the type exist in the table?
        if loadedState.side.map._vivh_tags[tag][previousValue] then -- Since we are certain there are no duplicate tags, we can remove the previous Tag value
            loadedState.side.map._vivh_tags[tag][previousValue] = nil
        end
    else loadedState.side.map._vivh_tags[tag] = {}  end
    loadedState.side.map._vivh_tags[tag][value] = value
end

function vh_tag.getElement(name, value, options)
    if not options._vivh_fieldSubdata then return nilField.getElement(name,value,options) end

    -- if the map has not had its tags set at all yet, say fuck it and just populate the tags list here. Doing this all at once reduces multiplicative growth
    if not loadedState.side.map._vivh_tags then
        populateTags(loadedState.side.map)
    end

    local field

    if options.editable then -- If the field is editable, that means a new value can be added to the list of tags for the key _vivh_fieldSubdata
        options.validator = function(v) 
            return vivUtil.isNullEmptyOrWhitespace(v) or loadedState.map._vivh_tags[options._vivh_fieldSubdata][v] ~= nil -- Is string not in the hashset?
        end
        field = stringField.getElement(name, value, options)
        -- Simple hook-in format
        local oldCallback = field.field.cb
        field.field.cb = function(...)
            fieldCallback(options, ...)
            oldCallback(...)
        end

        fieldCallback(options, field.field, field.field.text, "")
    else 
        options.options = loadedState.map._vivh_tags[options._vivh_fieldSubdata]
        
        field = stringField.getElement(name, value, options)
    end

    return field
end

function vh_tag.addTagControlToHandler(handler, propertyName, tagKey, _editable)
    local t = type(handler.fieldInformation)
    if t == "function" then
        local orig = handler.fieldInformation
        handler.fieldInformation = function(...)
            local ret = orig(...)
            if ret[propertyName] then 
                ret[propertyName].fieldType = "VivHelper.tagString"
                ret[propertyName]._vivh_fieldSubdata = tagKey
                ret[propertyName].editable = _editable
            else
                ret[propertyName] = {
                    fieldType = "VivHelper.tagString",
                    _vivh_fieldSubdata = tagKey,
                    editable = _editable
                }
            end
            return ret
        end
    elseif t == "table" then
        if handler.fieldInformation[propertyName] then 
            handler.fieldInformation[propertyName].fieldType = "VivHelper.tagString"
            handler.fieldInformation[propertyName]._vivh_fieldSubdata = tagKey
            handler.fieldInformation[propertyName].editable = _editable
        else
            handler.fieldInformation[propertyName] = {
                fieldType = "VivHelper.tagString",
                _vivh_fieldSubdata = tagKey,
                editable = _editable
            }
        end
    elseif t == "nil" then
        handler.fieldInformation = { }
        handler.fieldInformation[propertyName] = {fieldType = "VivHelper.tagString", _vivh_fieldSubdata = tagKey, editable = _editable}
    end
    local orig2 = handler.onDelete
    if orig2 then handler.onDelete = function(room, item, nodeIndex) if nodeIndex == 0 then loadedState.map._vivh_tags[tagKey][item[propertyName]] = nil end return orig2(room, entity, nodeIndex) end
    else handler.onDelete = function(room, item, nodeIndex) if nodeIndex == 0 then loadedState.map._vivh_tags[tagKey][item[propertyName]] = nil end return true end end
end

return vh_tag