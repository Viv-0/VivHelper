local drawableRect = require('structs.drawable_rectangle')
local drawableText = require('structs.drawable_text')

return {{
    name = "VivHelper/SolidModifier",
    fieldInformation = {
        CornerBoostBlock = {fieldType = "integer", options = {
            ["Normal"] = 0,
            ["Climb at Any Speed"] = 1,
            ["Climb at Any Speed + Retain Wall Speed"] = 2
        }, editable = false},
        TriggerOnTouch = {fieldType = "integer", options = {
            ["No Modifier"] = 0,
            ["On Touch"] = 1,
            ["On Touch + Bottom Contact"] = 2
        }, editable = false},
    },
    fieldOrder = {"x","y","width","height","Types","EntitySelect","CornerBoostBlock","TriggerOnTouch","TriggerOnBufferInput"},--[[
        Remove legacy placements, retain editor functionality
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
    },]]--
    sprite = function(room,entity) return {
        drawableRect.fromRectangle("fill",entity.x,entity.y,entity.width,entity.height,{0.4,0.2,0.2,0.4}),
        drawableFunc.fromFunction(require('utils.drawing').printCenteredText, "~Legacy - SolidModifier [VivHelper]", entity.x,entity.y,entity.width,entity.height,nil,0.5)
    } end,
    depth = -100000
}, {
    name = "VivHelper/SolidModifier2",
    fieldInformation = {
        cornerBoostBlock = {fieldType = "integer", options = {
            {"Normal", 0},
            {"Based On Speed", -1},
            {"1px", 1}, {"2px", 2}, {"3px", 3}, {"4px", 4}, {"5px", 5}, {"6px", 6}, {"7px", 7}, {"8px", 8}, {"9px", 9}, {"10px", 10}, {"11px", 11}, {"12px", 12}, {"13px", 13}, {"14px", 14}, {"15px", 15}, {"16px", 16}, {"17px", 17}, {"18px", 18}, {"19px", 19}, {"20px", 20}, {"21px", 21}, {"22px", 22}, {"23px", 23}, {"24px", 24}, {"25px", 25}
        }, editable = true},
        TriggerOnTouch = {fieldType = "integer", options = {
            ["No Changes"] = 0,
            ["On Side Contact"] = 1,
            ["On Side + Bottom Contact"] = 2
        }, editable = false},
    },
    fieldOrder = {"x","y","width","height","Types","EntitySelect","CornerBoostBlock","RetainWallSpeed","TriggerOnTouch","TriggerOnBufferInput"},
    placements = {
        {name = "main", data = { width = 8, height = 8, 
            Types = "*Solid", EntitySelect = true,
            cornerBoostBlock = 0,  TriggerOnBufferInput = false, TriggerOnTouch = 0, RetainWallSpeed = false
        }},
        {name = "cb", data = { width = 8, height = 8, Types = "*Solid", EntitySelect = true, 
            cornerBoostBlock = -1, RetainWallSpeed = true
        }},
        {name = "touch", data = { width = 8, height = 8, Types = "*Solid", EntitySelect = true, 
            TriggerOnTouch = 0, TriggerOnBufferInput = false
        }}
    },
    sprite = function(room,entity) return {
        drawableRect.fromRectangle("fill",entity.x,entity.y,entity.width,entity.height,{0.4,0.2,0.2,0.4}),
        drawableText.fromText("SolidModifier [VivHelper]", entity.x,entity.y,entity.width,entity.height,nil,0.5)
    } end,
    depth = -100000
}}