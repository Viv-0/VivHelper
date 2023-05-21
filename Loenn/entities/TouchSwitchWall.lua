return {
    name = "VivHelper/TouchSwitchWall",
    placements = {
        name = "wall",
        data = {
            width = 8, height = 8, 
            AllowHoldables=true, AllowSeekers=true, DisableParticles=false
        }
    },
    sprite = function(room,entity)
        return {
        require('structs.drawable_rectangle').fromRectangle('bordered',entity.x,entity.y, entity.width, entity.height, {0.0, 0.0, 0.0, 0.3}, {1.0,1.0,1.0,0.5}),
        require('structs.drawable_sprite').fromTexture("objects/touchswitch/icon00", entity):setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)}
    end
}