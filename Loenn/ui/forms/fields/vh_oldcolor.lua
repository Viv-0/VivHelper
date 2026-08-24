--[[
    vh_color:
    if Text is 6 characters long, Display Text is RrGgBb and Actual Text is also RrGgBb,
    if Text is 8 characters long,
    Display Text must be in the form "RrGgBbAa"
    Actual Text must be in the form "AaBbGgRr" (*not* aAbBgGrR)
]]



local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")
local contextMenu = require("ui.context_menu")
local utils = require("utils")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local colorPicker = require('mods').requireFromPlugin("ui.widgets.improved_color_picker")
local configs = require("configs")
local xnaColors = require("consts.xna_colors")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local colorField = {}

colorField.fieldType = "VivHelper.oldColor"

colorField._MT = {}
colorField._MT.__index = {}

local fallbackHexColor = "ffffff"

local invalidStyle = {
    normalBorder = {0.65, 0.2, 0.2, 0.9, 2.0},
    focusedBorder = {0.9, 0.2, 0.2, 1.0, 2.0}
}

function colorField._MT.__index:setValue(value)
    self.currentText = vivUtil.swapRGBA(value)
    self.field:setText(self.currentText)
    self.currentValue = value
end

function colorField._MT.__index:getValue()
    return self.currentValue or fallbackHexColor
end

function colorField._MT.__index:fieldValid(...)
    local current = self:getValue()
    local fieldEmpty = vivUtil.isNullEmptyOrWhitespace(current)

    if fieldEmpty then
        return self._allowEmpty
    elseif self._allowRainbow and current == "Rainbow" then
        return true
    else
        local parsed, r, g, b, a = vivUtil.getColor(current, self._allowXNAColors)

        return parsed
    end
end
-- Return the hex color of the XNA name if allowed
-- Otherwise return the value as it is
local function getXNAColorHex(element, value)
    local fieldEmpty = value == nil or #value == 0

    if fieldEmpty and element._allowEmpty then
        return fallbackHexColor
    end

    if element._allowXNAColors then
        local xnaColor = utils.getXNAColor(value or "")

        if xnaColor then
            return utils.rgbToHex(unpack(xnaColor))
        end
    end

    return value
end


local function cacheFieldPreviewColor(element, new, old)
    local parsed, r, g, b, a = vivUtil.getColor(new)
    if not parsed then return false, element._r, element._g, element._b, element._a end
    element._r, element._g, element._b, element._a = r, g, b, a
    return parsed, r, g, b, a
end

local function fieldChanged(formField)
    return function(element, new, old)
        local wasValid = formField:fieldValid()     
        local abgr = vivUtil.swapRGBA(new)   
        local valid, r, g, b, a = cacheFieldPreviewColor(element, new, old)
        formField.currentValue = abgr
        formField.currentText = new
        if wasValid ~= valid then
            if valid then
                -- Reset to default
                formField.field.style = nil

            else
                formField.field.style = invalidStyle
            end

            formField.field:repaint()
        end

        formField:notifyFieldChanged()
    end
end

local function getColorPreviewArea(element)
    local x, y = element.screenX, element.screenY
    local width, height = element.width, element.height
    local padding = element.style:get("padding") or 0
    local previewSize = height - padding * 2
    local drawX, drawY = x + width - previewSize - padding, y + padding

    return drawX, drawY, previewSize, previewSize
end

local function fieldDrawColorPreview(orig, element)
    orig(element)

    local parsed = element and element._parsed
    local r, g, b, a = element._r or 0, element._g or 0, element._b or 0, element._a or 1
    local pr, pg, pb, pa = love.graphics.getColor()

    local drawX, drawY, width, height = getColorPreviewArea(element)

    love.graphics.setColor(0, 0, 0)
    love.graphics.rectangle("fill",  drawX, drawY, width, height)
    love.graphics.setColor(1, 1, 1)
    love.graphics.rectangle("fill",  drawX + 1, drawY + 1, width - 2, height - 2)
    love.graphics.setColor(r, g, b,1)
    love.graphics.rectangle("fill",  drawX + 2, drawY + 2, width - 4, height - 4)
    love.graphics.setColor(pr, pg, pb, pa)
end

local function shouldShowMenu(element, x, y, button)
    local menuButton = configs.editor.contextMenuButton
    local actionButton = configs.editor.toolActionButton

    if button == menuButton then
        return true

    elseif button == actionButton then
        local drawX, drawY, width, height = getColorPreviewArea(element)

        return utils.aabbCheckInline(x, y, 1, 1, drawX, drawY, width, height)
    end

    return false
end


function colorField.getElement(name, value, options)
    local formField = {}

    local minWidth = options.minWidth or options.width or 160
    local maxWidth = options.maxWidth or options.width or 160
    local allowXNAColors = options.allowXNAColors
    local allowEmpty = options.allowEmpty
    local label = uiElements.label(options.displayName or name)
    local field = uiElements.field(value or fallbackHexColor, fieldChanged(formField)):with({
        ["minWidth"] = minWidth,
        ["maxWidth"] = maxWidth,
        ["_allowXNAColors"] = allowXNAColors,
        ["_allowEmpty"] = allowEmpty,
        ["_allowRainbow"] = options.allowRainbow,
    }):hook({
        draw = fieldDrawColorPreview
    })
    local fieldWithContext = contextMenu.addContextMenu(
        field,
        function()
            local pickerOptions = {
                callback = function(data)
                    field:setText(data.hexColor)
                    field.index = #data.hexColor
                end,
                alphaPreMult = options.alphaPreMult or true
            }

            local fieldText = getXNAColorHex(field, field:getText() or "")
            return colorPicker.getColorPicker(fieldText, pickerOptions)
        end,
        {
            shouldShowMenu = shouldShowMenu
        }
    )

    cacheFieldPreviewColor(field, value or "")
    field:setPlaceholder(vivUtil.swapRGBA(value))

    if options.tooltipText then
        label.interactive = 1
        label.tooltipText = options.tooltipText
    end

    label.centerVertically = true

    formField.label = label
    formField.field = field
    formField.name = name
    formField.initialValue = value
    formField.currentValue = value
    formField.currentText = vivUtil.swapRGBA(value)
    formField._allowXNAColors = allowXNAColors
    formField._allowEmpty = allowEmpty
    formField._allowRainbow = allowRainbow
    formField.width = 2
    formField.elements = {
        label, fieldWithContext
    }
    return setmetatable(formField, colorField._MT)
end

return colorField