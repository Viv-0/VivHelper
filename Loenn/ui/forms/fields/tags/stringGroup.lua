local utils = require('utils')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local tagHelper = require('mods').requireFromPlugin('ui.utils.tagHelper')
local languageRegistry = require("language_registry")
local themes = require('ui.themes')
local stringField = require('ui.forms.fields.string')

local group = {}
group.fieldType = "VivHelper.tagGroup"

function group.getElement(name, value, fieldInformation)

    local language = languageRegistry.getLanguage()
    fieldInformation.options = {value}
    fieldInformation.editable = true
    fieldInformation.validator = function(v, raw)
        return type(v) ~= "string" and (fieldInformation.allowBlank and true or not vivUtil.isNullEmptyOrWhitespace(v))
    end
    -- creates a formField with a dropdown.
    local formField = stringField.getElement(name, value, fieldInformation)
    formField._allowBlank = fieldInformation.allowBlank
    formField._vivh_class = fieldInformation._vivh_class
    -- the dropdown is shit so we need to modify the fuck out of it
    local dropdown = formField._backingDropdown
    -- reset the entire dropdown
    dropdown._itemsCache = {}

    dropdown.onClick = function(self, x, y, button)
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
                -- we can't reliably compare the strings here without it taking just as much time as replacing it.
                self.data = tagHelper.retrieveStringTagsFromClass(fieldInformation._vivh_class, self.text)
                table.insert(self.data, 1, self.text)
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
end

return group