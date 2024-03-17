local stringField = require("ui.forms.fields.string")
local intField = require("ui.forms.fields.integer")
local utils = require("utils")
local loadedState = require("loaded_state")
local nilField = require("ui.forms.fields.nil")
local entities = require("entities")
local triggers = require("triggers")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

--[[
Outline of what the fuck I'm doing here:
VivHelper.tag is a multi-room "group" identifier
It can either be passed with a string or an integer as of right now (because i was too lazy to make two different fieldtypes)

The format goes, for any given tag 't': 
for string str: loadedState.map._vivh_tags[t][s]
for integer int: loadedState.map._vivh_tags[t]["$"..tostring(int)]
strings cannot start with $ for clear reasons

There is multiple fieldInformation data required for a VivHelper.tag fieldType:
_vivh_parentTag - a string  - String that represents the tag name that all objects of that handler access
_vivh_format:   - a string - One of "group", "sender", or "receiver". Determines the format that the object is. See Format below
_vivh_isInt:    - a boolean - Whether or not the stored value should be in the form of an integer (true) or a string (false)

Format:
group    -  This is equivalent to FrostHelper's attachGroup. 
            Multiple objects can share the same group without bias and all group objects are added to the taglist
sender   -  The sender object specifies that only 1 sender object (or group object) can have a tag, in other words, it requires a "unique" tag
receiver -  Multiple objects can inherit a sender tag provided it already exists in the list. 
]]

local vh_tag = {}
vh_tag.fieldType = "VivHelper.tag"

local function AddOrRemoveTag(parentTag, itemTag, value)
    if loadedState.map._vivh_tags[parentTag][itemTag] then 
        loadedState.map._vivh_tags[parentTag][itemTag] = loadedState.map._vivh_tags[parentTag][itemTag] + value
    elseif value > 0 then 
        loadedState.map._vivh_tags[parentTag][itemTag] = value
    elseif value < 0 then
        print("ERROR - Tried to remove a tag that does not exist")
    end
end

local function setTagsFromMap(handler, item, integer) 
    local fieldInformation = (type(handler.fieldInformation) == "function" and handler.fieldInformation(item) or handler.fieldInformation)
    if not fieldInformation then return end
    for propName, propData in pairs(fieldInformation) do
        if not vivUtil.isNullEmptyOrWhitespace(item[propName]) then 
            local value = propData._vivh_isInt and "$" .. tostring(item[propName]) or item[propName]
            tagFunction(loadedState.map._vivh_tags, propData._vivh_parentTag, value)
        end
    end
end


local function populateTags(integer) -- Initial repopulation based on entity values. Only happens on the first pass
    loadedState.map._vivh_tags = {}
    for _,room in ipairs(loadedState.map.rooms) do
        for _,item in ipairs(room.entities) do
            handler = entities.registeredEntities[item._name]
            if handler then 
                setTagsFromMap(handler, item, integer)
            end
        end
        for _,item in ipairs(room.triggers) do
            handler = triggers.registeredTriggers[item._name]
            if handler then 
                setTagsFromMap(handler, item, integer)
            end
        end
    end
end

local function fieldCallback(options, self, value, previousValue)
    if loadedState.side.map._vivh_tags[options._vivh_parentTag] then -- does the type exist in the table?
        if loadedState.side.map._vivh_tags[options._vivh_parentTag][previousValue] then -- Since we are certain there are no duplicate tags, we can remove the previous Tag value
            loadedState.side.map._vivh_tags[options._vivh_parentTag][previousValue] = nil
        end
    else loadedState.side.map._vivh_tags[options._vivh_parentTag] = {}  end
    loadedState.side.map._vivh_tags[options._vivh_parentTag][value] = value
end

function vh_tag.getElement(name, value, options)

    local field 

    if options._vivh_isInt then
        if options._vivh_format == "sender" then    

        elseif options._vivh_format == "receiver" then
    
        else
            options.validator = function(v, raw)
                if type(v) ~= "integer" then return false end 
        end
    else
        if options._vivh_format == "sender" then    

        elseif options._vivh_format == "receiver" then
    
        else
    
        end
    end
    

    local integer = type(options._vivh_fieldSubdata) == "integer" 
    -- if the map has not had its tags set at all yet, say fuck it and just populate the tags list here. Doing this all at once reduces multiplicative growth
    if not loadedState.side.map._vivhStringTags then
        populateTags(loadedState.side.map, t)
    end

    local field

    if options.editable then -- If the field is editable, that means a new value can be added to the list of tags for the key _vivh_fieldSubdata
        options.validator = function(v) 
            return vivUtil.isNullEmptyOrWhitespace(v) or loadedState.map._vivhStringTags[options._vivh_fieldSubdata][v] ~= nil -- Is string not in the hashset?
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
        options.options = loadedState.map._vivhStringTags[options._vivh_fieldSubdata]
        
        field = stringField.getElement(name, value, options)
    end

    return field
end

function vh_tag.addTagControlToHandler(handler, propertyName, tagKey, format, isInt)
    local t = type(handler.fieldInformation)
    if t == "function" then
        local orig = handler.fieldInformation
        handler.fieldInformation = function(...)
            local ret = orig(...)
            if ret[propertyName] then 
                ret[propertyName].fieldType = "VivHelper.tag"
                ret[propertyName]._vivh_parentTag = tagKey
                ret[propertyName]._vivh_format = format
                ret[propertyName]._vivh_isInt = isInt
            else
                ret[propertyName] = {
                    fieldType = "VivHelper.tag",
                    _vivh_parentTag = tagKey,
                    _vivh_format = format,
                    _vivh_isInt = isInt
                }
            end
            return ret
        end
    elseif t == "table" then
        if  handler.fieldInformation[propertyName] then 
            handler.fieldInformation[propertyName].fieldType = "VivHelper.tag"
            handler.fieldInformation[propertyName]._vivh_parentTag = tagKey
            handler.fieldInformation[propertyName]._vivh_format = format
            handler.fieldInformation[propertyName]._vivh_isInt = isInt
        else
            handler.fieldInformation[propertyName] = {
                fieldType = "VivHelper.tag",
                _vivh_parentTag = tagKey,
                _vivh_format = format,
                _vivh_isInt = isInt
            }
        end
    elseif t == "nil" then
        handler.fieldInformation = {
            [propertyName] = {
                fieldType = "VivHelper.tag",
                _vivh_parentTag = tagKey,
                _vivh_format = format,
                _vivh_isInt = isInt
            }
         }
    end
    if format == "sender" or format == "group" then
        if handler.onDelete then 
            handler.onDelete = function(room, item, nodeIndex) 
                if nodeIndex == 0 then 
                    AddOrRemoveTag(tagKey, item[propertyName], -1)
                end
                return orig2(room, entity, nodeIndex)
            end
        else 
            handler.onDelete = function(room, item, nodeIndex) 
                if nodeIndex == 0 then
                    AddOrRemoveTag(tagKey, item[propertyName], -1)
                end
                return true
            end
        end
    end
end

return vh_tag