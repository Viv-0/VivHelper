local utils = require('utils')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local tagHelper = require('mods').requireFromPlugin('ui.utils.tagHelper')
local languageRegistry = require("language_registry")
local uiElements = require('ui.elements')

local intSender = {}
intSender.fieldType = "VivHelper.tagIntSender"


local function integerValueTransformer(v) return tonumber((v or "-1")) end
local function integerDisplayTransformer(v) return tostring((v or -1)) end


local function buttonPressed(formField)
    return function (element)
        if not (formField and formField.metadata and formField.metadata.formData) then return end
        formField.field.text = tagHelper.addNextInteger(class, formField.metadata.formData._id)
        formField.field.index = #formField.field.text

        formField:notifyFieldChanged()
    end
end

local function fieldCallback(fieldInformation, field, newValue, oldValue)
    -- Code shamelessly stolen from FrostHelper.attachGroup
    local text = field.text or ""
    local font = field.label.style.font
    local button = field.button

    -- should just be button.width, but that isn't correct initially :(
    local offset = -font:getWidth(button.text) - (2 * button.style.padding)

    self.button.x = -font:getWidth(text) + self.minWidth + offset - 40
end

function intSender.getElement(name, value, fieldInformation)
-- if the map has not had its tags set at all yet, say fuck it and just populate the tags list here. Doing this all at once reduces multiplicative growth  
-- We only need the tags to have values from map *if* we're editing one of the tagged elements, so this is fine to do here.
    if not loadedState.map._vivh_tags then
        tagHelper.populateTags()
    end
    local language = languageRegistry.getLanguage()

    fieldInformation.options = nil -- because we are dynamically containing tags in the dropdown, there's no need for a default dropdown.
    fieldInformation.valueTransformer = integerValueTransformer
    fieldInformation.displayTransformer = integerDisplayTransformer
    fieldInformation.editable = false

    function fieldInformation.validator(v)
        local num = tonumber((v or "-1"))
        local tag = loadedState.map._vivh_tags[fieldInformation._vivh_class][v]
        -- sender can only have 1 element in iTag, itself
        if utils.isInteger(num) then
            if num > 0 then
                return not (type(tag) == "table" and #tag > 0) 
            elseif num == 0 then
                return fieldInformation.allowBlank and not (type(tag) == "table" and #tag > 0)
            end
        end
        return false
    end
    local field = stringField.getElement(name, value, fieldInformation)
    local button = uiElements.button(tostring(language.ui.VivHelper.intSender.label), buttonPressed(formField))

    button.style.padding *= 0.36
    button.style.spacing = 0
    button.tooltipText = string.format(tostring(language.ui.VivHelper.intSender.tooltip), tostring(language.tags.VivHelper[fieldInformation._vivh_class].smalldetail))
    formField.field:addChild(button)
    formField.field.button = button

    -- Start adding manipulations
    -- Hook field callback 
    local orig = field.field.cb
    field.field.cb = function(...)
        orig(...)
        fieldCallback(fieldInformation, ...)
    end
    fieldCallback(fieldInformation, field.field, field.field.text, nil)

    return field
end

return intSender