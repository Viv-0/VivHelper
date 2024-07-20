local stringField = require("ui.forms.fields.string")
local intField = require("ui.forms.fields.integer")
local utils = require("utils")
local loadedState = require("loaded_state")
local nilField = require("ui.forms.fields.nil")
local entities = require("entities")
local triggers = require("triggers")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local fieldDropdown = require("ui.widgets.field_dropdown")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")

--[[
Outline of what the fuck I'm doing here:
VivHelper.tag is a multi-room "group" identifier
It can either be passed with a string or an integer as of right now (because i was too lazy to make two different fieldtypes)

The format goes, for any given tag 't': 
for string str: loadedState.map._vivh_tags[t][str]
for integer int: loadedState.map._vivh_tags[t][int]

There is multiple fieldInformation data required for a VivHelper.tag fieldType:
_vivh_class     - a string  - String that represents the tag name that all objects of that handler access
_vivh_format:   - a string - One of "group", "sender", or "receiver". Determines the format that the object is. See Format below
_vivh_isInt:    - a boolean - Whether or not the stored value should be in the form of an integer (true) or a string (false)

Format:
group    -  This is equivalent to FrostHelper's attachGroup. 
            Multiple objects can share the same group without bias and all group objects are added to the taglist
sender   -  The sender object specifies that only 1 sender object (or group object) can have a tag, in other words, it requires a "unique" tag
receiver -  Multiple objects can inherit a sender tag provided it already exists in the list. Since receivers always require a sender, receivers do not get added to the tags list, as this would conflict with the sender working. Groups cannot be helped unfortunately.

_vivh_tags is structured as:
_vivh_tags[class][tag][itemID] = true
class - The class is used as the reference to what objects ascribe to what "groups" exist, i.e. this generally gets the "list of groups" for the specific entities that try to group within it.
tag - the tag denoting a group. objects with similar tags are related to one another. This is also the value put into the field.
itemID - entity._id or 0 - trigger._id
]]

local vh_tag = {}
vh_tag.fieldType = "VivHelper.tag"
-- tag specific functions 
local function AddOrRemoveTag(class, itemValue, item, add)
    local itemTag = nil
    if not itemValue or not loadedState or not loadedState.map then return end
    if not loadedState.map._vivh_tags[class] then
        loadedState.map._vivh_tags[class] = {}
    end
    if not loadedState.map._vivh_tags[class][itemTag] then
        loadedState.map._vivh_tags[class][itemTag] = {}
    end
    if add then
        if not loadedState.map._vivh_tags[class][itemTag][item] then
            loadedState.map._vivh_tags[class][itemTag][item] = true -- Add to table
        end
    elseif loadedState.map._vivh_tags[class][itemTag][itemID] then
        loadedState.map._vivh_tags[class][itemTag][itemID] = nil -- Remove from table            
    end
end

local function setTagsFromMap(handler, item) 
    local fieldInformation = utils.callIfFunction(handler.fieldInformation, item)
    if not fieldInformation then return end
    for propName, propData in pairs(fieldInformation) do
        AddOrRemoveTag(propData._vivh_class, item[propName], item._id, true)
    end
end

local function populateTags() -- Initial repopulation based on entity values. Only happens on the first pass
    loadedState.map._vivh_tags = {}
    local handler
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

local function retrieveTagsFromClass(class, integer)
    local tags = {}
    if loadedState and loadedState.map and loadedState.map._vivh_tags and 
       type(loadedState.map._vivh_tags[class]) == "table" then
        local c = loadedState.map._vivh_tags[class]
        if (integer and #c or vivUtil.countStringKeys(c)) > 0 then
            for key,_ in pairs(c) do
                table.insert(tags, key)
            end 
            table.sort(tags)
        end
    end
    return tags
end

-- Field-specific instructions
local function dropdownChanged(formField, optionsFlattened)
    return function(element, new)
        local value
        local old = formField.currentValue

        for _, option in ipairs(optionsFlattened) do
            if option[1] == new then
                value = option[2]
            end
        end

        if value ~= old then
            formField.currentValue = value

            local valid = formField:fieldValid()
            local warningValid = formField:fieldWarning()

            updateFieldStyle(formField, valid, warningValid)
            formField:notifyFieldChanged()
        end
    end
end


local function stringFieldCB(fieldInformation, self, newValue, oldValue) 
    if not fieldInformation or not fieldInformation._vivh_class or not fieldInformation.validator(newValue) then return end
    if vivUtil.isNullEmptyOrWhitespace(oldValue) then
        AddOrRemoveTag(self.name, fieldInformation._vivh_class, oldValue, false)
    end
    AddOrRemoveTag(self.name, fieldInformation._vivh_class, newValue, true)
end
local function integerFieldCB(fieldInformation, self, newValue, oldValue) 

    if not fieldInformation or not fieldInformation._vivh_class or not fieldInformation.validator(newValue) then return end
    if oldValue then
        AddOrRemoveTag(self.name, fieldInformation._vivh_class, oldValue, false)
    end
    AddOrRemoveTag(self.name, fieldInformation._vivh_class, newValue, true)
end

local function integerValueTransformer(v) return tonumber((v or "-1")) end
local function integerDisplayTransformer(v) return tostring((v or -1)) end

function vh_tag.getElement(name, value, fieldInformation)
   
-- if the map has not had its tags set at all yet, say fuck it and just populate the tags list here. Doing this all at once reduces multiplicative growth  
-- We only need the tags to have values from map *if* we're editing one of the tagged elements, so this is fine to do here.
if not loadedState.side.map._vivh_tags then
    populateTags(loadedState.side.map)
end
    
    fieldInformation.options = nil -- because we are dynamically containing tags in the dropdown, there's no need for a default dropdown.
    local field
    local format = fieldInformation._vivh_format
    local integer = fieldInformation._vivh_isInt
    if integer then
        fieldInformation.valueTransformer = integerValueTransformer
        fieldInformation.displayTransformer = integerDisplayTransformer
        fieldInformation.editable = false
    end
    if format == "sender" then
        -- Validator is "return whether the value tag associated with this element does not exist in the known tags for that class"
        fieldInformation.validator = integer and function(v)
            local num = tonumber((v or "-1"))
            local tag = loadedState.map._vivh_tags[fieldInformation._vivh_class][v]
            -- sender can only have 1 element in iTag, itself

            return utils.isInteger(num) and num >= 0 and not (type(tag) == "table" and #tag > 0)
        end or fieldInformation.validator = function(v)
            if type(v) ~= "string" then return false end
            local tag = loadedState.map._vivh_tags[fieldInformation._vivh_class][v]
            -- sender can only have 1 element in iTag, itself

            return type(tag) ~= "table" or vivUtil.countStringKeys(tag) < 1
        end
        -- Create the field!
        field = stringField.getElement(name, value, options)
        -- Start adding manipulations
        -- Hook field callback 
        local orig1 = field.field.cb
        field.field.cb = integer and function(...)
            orig1(...)
            integerFieldCB(fieldInformation, ...)
        end or function (...)
            orig1(...)
            stringFieldCB(fieldInformation, ...)
        end


    if fieldInformation._vivh_isInt then -- ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||| INTEGER SEGMENT ||||||||||||||||||||||||||||
        if fieldInformation._vivh_format == "sender" then
            -- Stuff we have to initialize before creating the field
            
            fieldInformation.options = nil

            

            
            dropdown = uiElements.dropdown(retrieveTagsFromClass(fieldInformation._vivh_class, true), nil):with({
                minHeight = 160, maxHeight = 160
            })
            -- Modify dropdown to not actually impact the value of text
            dropdown.cbOnItemClick = false
            -- clear cache before it gets opened so we don't get problems with sender
            dropdown._itemsCache = {}
            -- rewrite getItemCached to no longer have that hook
            dropdown.getItemCached = function(self, text, i)
                local cache = self._itemsCache
                local item = cache[i]
                if item then
                    local data
                    if text and text.text and text.data ~= nil then
                        data = text.data
                        text = text.text
                    end
                    item.text = text
                    item.data = data
                else
                    item = uie.listItem(text):with({
                        owner = self
                    })
                    item.label.style.color = {1,0,0,1}
                    cache[i] = item
                end
                return item
            end
            -- reassign cache before it gets opened so we don't get problems with sender
            for i = 1, #dropdown.data do
                self:getItemCached(list[i], i)
            end

            
        elseif fieldInformation._vivh_format == "group" then
            -- Stuff we have to initialize before creating the field
            fieldInformation.validator = function(v)
                local num = tonumber((v or "-1"))
                -- Group is generic, any number of items of type group can be boxed into a given tag
                return utils.isInteger(num) and num >= 0
            end
            -- Create the field!
            field = stringField.getElement(name, value, options)

            -- Start adding manipulations
            -- Hook field callback 
            local orig2 = field.field.cb
            field.field.cb = function (...)
                orig2(...)
                integerFieldCB(fieldInformation, ...)
            end
            dropdown = uiElements.dropdown(retrieveTagsFromClass(fieldInformation._vivh_class, true), dropdownChanged(field, optionsFlattened)):with({
                minHeight = 160, maxHeight = 160
            })
            -- completely rewrite dropdown.onClick to use tags from class and add "added" button
            dropdown.onClick = function(orig, self, x, y, button)
                if self.enabled and button == 1 then
                    local submenu = self.submenu
                    local spawnNewMenu = true
                    if submenu then
                        -- Submenu might still exist if it was closed by clicking one of the options
                        -- In which case we should spawn a new menu
                        spawnNewMenu = not submenu.alive
                        submenu:removeSelf()
                    end
                    if spawnNewMenu then
                        local submenuParent = self.submenuParent or self
                        local data = retrieveTagsFromClass(fieldInformation._vivh_class, true)
                        if #data < 1 then data = self.data
                        local submenuData = uiu.map(data, function(data, i)
                            local item = self:getItemCached(data, i)
                            item.width = false
                            item.height = false
                            item:layout()
                            return item
                        end)
                        x = submenuParent.screenX
                        y = submenuParent.screenY + submenuParent.height + submenuParent.parent.style.spacing
                        self.submenu = uie.menuItemSubmenu.spawn(submenuParent, x, y, submenuData)
                    end
                end
            end                 
        elseif fieldInformation._vivh_format == "receiver" then
            fieldInformation.validator = function(v)
                local num = tonumber((v or "-1"))
                return utils.isInteger(num) and num >= 0 and loadedState.map._vivh_tags[fieldInformation._vivh_class][v] end
            end
            -- Create the field!
            field = stringField.getElement(name, value, options)

            -- Start adding manipulations
            -- Hook field callback 
            local orig2 = field.field.cb
            field.field.cb = function (...)
                orig2(...)
                integerFieldCB(fieldInformation, ...)
            end
            dropdown = uiElements.dropdown(retrieveTagsFromClass(fieldInformation._vivh_class, true), dropdownChanged(field, optionsFlattened)):with({
                minHeight = 160, maxHeight = 160
            })
        else 
            error("VivHelper.tag error - field information for " .. name .. " was found to be " .. fieldInformation._vivh_format)
        end    
    else -- ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||| STRING SEGMENT ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        -- transformers are correct by default
        if fieldInformation._vivh_format == "sender" then
            -- Stuff we have to initialize before creating the field
            fieldInformation.editable = true
            fieldInformation.validator = function(v)
                if type(v) ~= "string" then return false end
                local tag = loadedState.map._vivh_tags[fieldInformation._vivh_class][v]
                -- sender can only have 1 element in iTag, itself

                return type(tag) ~= "table" or vivUtil.countStringKeys(tag) < 1
            end

            -- Create the field!
            field = stringField.getElement(name, value, options)

            -- Start adding manipulations
            -- Hook field callback 
            local orig3 = field.field.cb
            field.field.cb = 
            dropdown = uiElements.dropdown(retrieveTagsFromClass(fieldInformation._vivh_class, false), nil):with({
                minHeight = 160, maxHeight = 160
            })
            -- Modify dropdown to not actually impact the value of text
            dropdown.cbOnItemClick = false
            -- clear cache before it gets opened so we don't get problems with sender
            dropdown._itemsCache = {}
            -- rewrite getItemCached to no longer have that hook
            dropdown.getItemCached = function(self, text, i)
                local cache = self._itemsCache
                local item = cache[i]
                if item then
                    local data
                    if text and text.text and text.data ~= nil then
                        data = text.data
                        text = text.text
                    end
                    item.text = text
                    item.data = data
                else
                    item = uie.listItem(text):with({
                        owner = self
                    })
                    item.label.style.color = {1,0,0,1}
                    cache[i] = item
                end
                return item
            end
            -- reassign cache before it gets opened so we don't get problems with sender
            for i = 1, #dropdown.data do
                self:getItemCached(list[i], i)
            end
        else
            if fieldInformation._vivh_format == "group" then
                -- Stuff we have to initialize before creating the field
                fieldInformation.editable = true
            elseif fieldInformation._vivh_format == "receiver" then
                fieldInformation.editable = false
                fieldInformation.validator = function(v)
                    if type(v) ~= "string" then return false end
                    local tag = loadedState.map._vivh_tags[fieldInformation._vivh_class][v]
                    -- sender can only have 1 element in iTag, itself
                    return type(tag) ~= "table" or vivUtil.countStringKeys(tag) < 1
                end
            else 
                error("VivHelper.tag error - field information for " .. name .. " was found to be " .. fieldInformation._vivh_format)
            end
                
            -- Create the field!
            field = stringField.getElement(name, value, options)

            -- Start adding manipulations
            -- Hook field callback 
            local orig4 = field.field.cb
            field.field.cb = function (...)
                orig4(...)
                stringFieldCB(fieldInformation, ...)
            end
            dropdown = uiElements.dropdown(retrieveTagsFromClass(fieldInformation._vivh_class, false), dropdownChanged(field, optionsFlattened)):with({
                minHeight = 160, maxHeight = 160
            })
        end
    end

    field.
end



return vh_tag