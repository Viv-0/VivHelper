local stringField = require("ui.forms.fields.string")

local vanillaEasings = require("mods").requireFromPlugin("libraries.easing")

local easingField = {}

easingField.fieldType = "VivHelper.easing"

function easingField.getElement(name, value, options)
    -- Add extra options and pass it onto the string field
    options.displayTransformer = tostring
    options.validator = function(v)
        return not not vanillaEasings[v]
    end
    options.options = vanillaEasings
    options.editable = false

    local formField = stringField.getElement(name, value, options)

    return formField
end

return easingField