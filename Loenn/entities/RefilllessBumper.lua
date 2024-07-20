

local bumper = {}

bumper.name = "VivHelper/RefilllessBumper"
bumper.depth = 0
bumper.nodeLineRenderType = "line"
bumper.texture = "ahorn/VivHelper/norefillBumper"
bumper.nodeLimits = {0, 1}
bumper.placements = {
    name = "rlbumper"
}
bumper.selection = function(room,entity) return utils.rectangle(entity.x-8, entity.y-8, 16, 16) end
return bumper

