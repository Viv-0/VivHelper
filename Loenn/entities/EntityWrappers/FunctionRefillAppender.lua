return {
    name = "VivHelper/FunctionRefillAppender",
    placements = {
        name = "main",
        data = {
            width = 8, height = 8,
            Types="", all=true,
            DashesLogic="D", StaminaLogic="D"
        }
    },
    sprite = function(room,entity) return {
        require('structs.drawable_rectangle').fromRectangle("fill",entity.x,entity.y,entity.width,entity.height,{0.4,0.4,0.4,0.4}),
        require('structs.drawable_function').fromFunction(require('utils.drawing').printCenteredText, "Refill OnCollect Modifier [VivHelper]", entity.x,entity.y,entity.width,entity.height,nil,0.5)
    } end
}