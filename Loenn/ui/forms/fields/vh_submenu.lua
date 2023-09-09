-- Fallback for missing values

local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")
local contextMenu = require("ui.context_menu")
local utils = require('utils')
local submenu = require('mods').requireFromPlugin('ui.widgets.submenu')

local buttonField = {}

buttonField.fieldType = "VivHelper.submenu"

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

local function updatedDraw(orig, element)
    orig(element)
    local x, y = element.screenX, element.screenY
    local drawX, drawY = x + button.label.calcWidth(), y

end

function buttonField.getElement(name, value, options)
    local formField = {}

    formField.name = name

    local button = uiElements.button(options.displayName or name, packFormButtonCallback(formField, options.callback))
    button._parent = formField
    button.maxWidth = button.label:calcWidth() + 8
    local cross = uiElements.icon("ui:icons/checkboxCross"):with(uiUtils.at(0.999, 0.5 + 2))
    cross.style = {color = {1,0,0,1}}
    button:addChild(cross)
    button.icon = cross
    local buttonWithRow = contextMenu.addContextMenu(uiElements.row({button}),
        function()
            return submenu.getSubmenu(button._parent._vivh_data, button._parent._vivh_fieldInformation, options.saveChangesCallback, options.submenuOptions or {})
        end,
        {
            mode = "focused"
        })
    formField.width = 1
    formField.elements = {
        buttonWithRow
    }
    formField._vivh_dataToSteal = options.submenuContents or nil
    formField._vivh_data = options.presetData or {}
    formField._vivh_fieldInformation = options.presetFieldInfo or {}

    return setmetatable(formField, buttonField._MT)
end

return buttonField