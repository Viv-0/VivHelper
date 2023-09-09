local drawableRect = require('structs.drawable_rectangle')
local drawableFunc = require('structs.drawable_function')

return {
    name = "VivHelper/SolidModifier",
    fieldInformation = {
        CornerBoostBlock = {fieldType = "integer", options = {
            ["Normal"] = 0,
            ["Corner Boost Block"] = 1,
            ["Retain Wall Speed"] = 2
        }, editable = false},
        TriggerOnTouch = {fieldType = "integer", options = {
            ["No Modifier"] = 0,
            ["On Touch"] = 1,
            ["On Touch + Bottom Contact"] = 2
        }, editable = false},
    },
    fieldOrder = {"x","y","width","height","Types","EntitySelect","CornerBoostBlock","TriggerOnTouch","TriggerOnBufferInput"},
    placements = {
        {name = "main", data = { width = 8, height = 8, 
            Types = "*Solid", EntitySelect = true,
            CornerBoostBlock = 0,  TriggerOnBufferInput = false, TriggerOnTouch = 0
        }},
        {name = "cb", data = { width = 8, height = 8, Types = "*Solid", EntitySelect = true, 
            CornerBoostBlock = 0
        }},
        {name = "touch", data = { width = 8, height = 8, Types = "*Solid", EntitySelect = true, 
            TriggerOnTouch = 0, TriggerOnBufferInput = false
        }}
    },
    sprite = function(room,entity) return {
        drawableRect.fromRectangle("fill",entity.x,entity.y,entity.width,entity.height,{0.4,0.2,0.2,0.4}),
        drawableFunc.fromFunction(require('utils.drawing').printCenteredText, "SolidModifier [VivHelper]", entity.x,entity.y,entity.width,entity.height,nil,0.5)
    } end,
    depth = -100000
}