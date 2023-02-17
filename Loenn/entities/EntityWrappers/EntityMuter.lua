return {
    name = "VivHelper/EntityMuter",
    placements = {
        name = "main",
        data = {
            width = 8, height = 8, 
            Types = "", all = true
        }
    },
    sprite = function(room,entity) return {
        require('structs.drawable_rectangle').fromRectangle("fill",entity.x,entity.y,entity.width,entity.height,{0.4,0.4,0.4,0.4}),
        require('structs.drawable_sprite').fromTexture('ahorn/VivHelper/mute', entity):setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)
    } end
    
}