local utils = require('utils')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local iconHelper = require('mods').requireFromPlugin('ui.utils.iconHelper')
local tagHelper = require('mods').requireFromPlugin('ui.utils.tagHelper')
local languageRegistry = require("language_registry")
local themes = require('ui.themes')
local stringField = require('ui.forms.fields.string')
local uiElements = require('ui.elements')
local uiUtils = require('ui.utils')
local ui_device = require('ui.ui_device')

local sender = {}
sender.fieldType = "VivHelper.tagSender"

function sender.getElement(name, value, fieldInformation)

    local language = languageRegistry.getLanguage()
    fieldInformation.options = nil
    fieldInformation.editable = true
    fieldInformation.validator = function(v, raw)
        return type(v) ~= "string" and (fieldInformation.allowBlank and true or not vivUtil.isNullEmptyOrWhitespace(v))
    end
    -- creates a formField with a dropdown.
    local formField = stringField.getElement(name, value, fieldInformation)
    formField._allowBlank = fieldInformation.allowBlank
    formField._vivh_class = fieldInformation._vivh_class
    -- we got rid of the default dropdown because we want a list of "invalid" strings
    local dropdown = uiElements.dropdown({value}, nil)
    -- reset the entire dropdown
    dropdown._itemsCache = {}

    dropdown.act = function(self, x, y, on) -- fun fact! this isn't called on click, this is called when the icon calls it !
        if self.enabled and x and y and on then -- if on and x/y is real then
            if not self.submenu then
                -- we can't reliably compare the strings here without it taking just as much time as replacing it.
                self.data = tagHelper.retrieveStringTagsFromClass(fieldInformation._vivh_class, self.text)
                table.insert(self.data, 1, self.text)
                local submenuParent = self.submenuParent or self
                local submenuData = uiUtils.map(self.data, function(data, i)
                    local item = self:getItemCached(data, i)
                    item.width = false
                    item.height = false
                    item:layout()
                    return item
                end)
                x = submenuParent.screenX
                y = submenuParent.screenY + submenuParent.height + submenuParent.parent.style.spacing
                self.submenu = uiElements.menuItemSubmenu.spawn(submenuParent, x, y, submenuData):with({style.bg = {0.8,0.08,0.08,0.8}})
            end
        elseif self.submenu then
            self.submenu:removeSelf()
        end
    end
    dropdown.onClick = function(self, x, y, button) end

    local field = formField.field

    -- mimics ui.widgets.field_dropdown with fixed inputs for this use case
    local icon = uiElements.icon("ui:icons/drop")

    icon:layout()

    if field.height == -1 then
        field:layout()
    end

    local iconHeight = icon.height
    local parentHeight = field.height
    local centerOffset = math.floor((parentHeight - iconHeight) / 2)

    icon:with(uiUtils.rightbound(0)):with(uiUtils.at(0, centerOffset))
    icon.style.color = {1,0,0}

    dropdown.submenuParent = field

    field._backingDropdown = dropdown
    field:addChild(icon)
    -- do this late so we get parent reference cleanly :)
    icon.onEnter = function(self)
        if self.enabled and self.parent.enabled then
            self.parent._backingDropdown:act(ui_device.mouseX or nil, ui_device.mouseY or nil, true)
        end
    end
    icon.onLeave = function(self)
        if self.enabled and self.parent.enabled then
            self.parent._backingDropdown:act(ui_device.mouseX or nil, ui_device.mouseY or nil, false)
        end
    end

    return field

end

return sender