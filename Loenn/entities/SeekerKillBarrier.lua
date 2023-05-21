
return {
    name = "VivHelper/SeekerKillBarrier",
    placements = {
        name = "main",
        data = {
            width = 8, height = 8,
        }
    },
    sprite = function(room,entity) return {
        require('structs.drawable_rectangle').fromRectangle("bordered",entity.x,entity.y,entity.width,entity.height,{0.816,0.188,0.188,0.5}, {0.2,0.2,0.2,0.6}),
        require('structs.drawable_function').fromFunction(require('utils.drawing').printCenteredText, "Seeker Kill Barrier", entity.x,entity.y,entity.width,entity.height,nil,0.25)
    } end
}