local drawableRect = require('structs.drawable_rectangle')
local drawableText = require('structs.drawable_text')


return {
    name = "VivHelper/FunctionRefillAppender",
    placements = {
        name = "main",
        data = {
            width = 8, height = 8,
            Types="", all=true, includeSolidOnDash="None",
            DashesLogic="D", StaminaLogic="D"
        }
    },
    fieldInformation = {
        includeSolidOnDash = {fieldType = "string", options = {"None", "Rebound", "NormalCollision", "NormalOverride", "Bounce", "Ignore"}, editable = false}
    },
    sprite = function(room,entity) return {
        drawableRect.fromRectangle("fill",entity.x,entity.y,entity.width,entity.height,{0.4,0.4,0.4,0.4}),
        drawableText.fromText("Refill OnCollect Modifier [VivHelper]", entity.x,entity.y,entity.width,entity.height,nil,0.5)
    } end
}