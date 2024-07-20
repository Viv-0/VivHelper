local utils = require('utils')
local bumper = {}

bumper.name = "VivHelper/EvilBumper"
bumper.depth = 0
bumper.nodeLineRenderType = "line"
bumper.texture = "objects/Bumper/Evil22"
bumper.nodeLimits = {0, 1}
bumper.placements = {
    name = "bumper",
    data = {wobble = true}
}
bumper.selection = function(room,entity) return utils.rectangle(entity.x-8, entity.y-8, 16, 16) end

return bumper