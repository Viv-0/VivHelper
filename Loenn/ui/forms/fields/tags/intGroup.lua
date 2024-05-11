local utils = require('utils')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local tagHelper = require('mods').requireFromPlugin('ui.utils.tagHelper')
local languageRegistry = require("language_registry")
local themes = require('ui.themes')
local stringField = require('ui.forms.fields.string')
local uiElements = require('ui.elements')
local uiu = require('ui.utils')

local intGroup = {}
intGroup.fieldType = "VivHelper.tagIntGroup"

local function valueTransformer(v) return tonumber((v or "0")) end
local function displayTransformer(v) return tostring((v or 0)) end

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

function intGroup.getElement(name, value, fieldInformation)
    local formField = {}
    formField._vivh_class = fieldInformation._vivh_class
    formField._allowBlank = fieldInformation._allowBlank
-- if the map has not had its tags set at all yet, say fuck it and just populate the tags list here. Doing this all at once reduces multiplicative growth  
-- We only need the tags to have values from map *if* we're editing one of the tagged elements, so this is fine to do here.
    if not loadedState.side.map._vivh_tags then
        tagHelper.populateTags(loadedState.side.map)
    end
    local language = languageRegistry.getLanguage()

    fieldInformation.options = nil -- because we are dynamically containing tags in the dropdown, there's no need for a default dropdown.
    fieldInformation.editable = false

    local list = formField._allowBlank and {0} or {}
    for i=tagHelper.getNextInteger(formField._vivh_class),1,-1 do
        table.insert(list, tostring(i))
    end
    
    local dropdown = uiElements.dropdown(list, dropdownChanged(formField, optionsFlattened)):with({
        minHeight = 160, maxHeight = 160
    })

    -- modify initialized values

    -- add this reference for updating the dropdown when we open it !
    dropdown._peak = fieldInformation.allowBlank and 1 or 0
    dropdown._capReference = function() return tagHelper.getNextInteger(fieldInformation._vivh_class) end

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
            }):hook({
                onClick = function(orig, self, x, y, button)
                    orig(self, x, y, button)
                    self.owner.selected = self
                    self.owner.text = self.text
                    self.owner.submenu:removeSelf()
                end
            })
            cache[i] = item
        end
        if i == 0 and dropdown._peak == 1 then
            item.label.style.color = {1,1,0,1}
        elseif i == dropdown._peak then
            item.label.style.color = {0,1,0,1}
        elseif themes.currentTheme then
            item.label.style.color = themes.currentTheme.label.color or {1,1,1,1}
        else
            item.label.style.color = {1,1,1,1}
        end
        return item
    end

    -- completely rewrite dropdown.onClick to use tags from class and add custom stylization
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
                local peak = dropdown._capReference()
                if peak ~= dropdown.getItem(dropdown._peak) then
                    local data = dropdown._peak == 1 and {0} or {}
                    for i=peak,1,-1 do
                        table.insert(list, tostring(i))
                    end
                    self.data = data
                end
                local submenuParent = self.submenuParent or self
                local submenuData = uiu.map(self.data, function(data, i)
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

    dropdown.tooltipText = string.format(tostring(language.ui.VivHelper.intGroup.tooltip), tostring(language.tags.VivHelper[fieldInformation._vivh_class].largedetail))

    formField.label = label
    formField.field = dropdown
    formField.name = name
    formField.initialValue = value
    formField.currentValue = value
    formField.validator = function(v, raw) 
        local num = tonumber((v or "-1"))
        local tag = loadedState.map._vivh_tags[formField._vivh_class][v]
        -- sender can only have 1 element in iTag, itself
        if utils.isInteger(num) then
            if num > 0 then
                return not (type(tag) == "table" and #tag > 0) 
            elseif num == 0 then
                return formField._allowBlank or not (type(tag) == "table" and #tag > 0)
            end
        end
        return false
    end
    formField.warningValidator = function(v) return true end
    formField.valueTransformer = valueTransformer
    formField.displayTransformer = displayTransformer
    formField.validVisuals = true
    formField.width = 2
    formField.elements = {
        label, dropdown
    }

    formField = setmetatable(formField, stringField._MT)

    return formField
end

return intGroup