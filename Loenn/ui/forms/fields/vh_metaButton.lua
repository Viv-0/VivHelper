-- Fallback for missing values

local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")
local languageRegistry = require("language_registry")

local buttonField = {}

buttonField.fieldType = "VivHelper.button"

buttonField._MT = {}
buttonField._MT.__index = {}

local function packFormButtonCallback(formField, func)
    func = func or function() end

    return function(self, x, y, button)
        return func(formField, self, x, y, button)
    end
end

function buttonField._MT.__index:setValue(value)
    self.currentValue = value
end

function buttonField._MT.__index:getValue()
    return self.currentValue
end

function buttonField._MT.__index:fieldValid()
    return true
end

function buttonField.getElement(name, value, options)
    local formField = {}

    formField.name = name

    local button = uiElements.button(options.displayName or name, packFormButtonCallback(formField, options.callback))
    formField.width = 1
    formField.elements = {
        uiElements.row({button})
    }

    return setmetatable(formField, buttonField._MT)
end

return buttonField