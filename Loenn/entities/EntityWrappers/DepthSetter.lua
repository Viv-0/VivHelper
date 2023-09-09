return {
    name = "VivHelper/DepthSetter",
    depth = -math.huge,
    fieldInformation = {
        depth = {
            fieldType = "integer",
            options = require('consts.object_depths')
        }
    },
    fillColor = {0.2, 0.8, 0.4, 0.8},
    borderColor = {0.4, 0.4, 0.4, 0.4},
    placements = {
        name = "depth",
        data = {
            width = 8, height = 8,
            depth = 0,
            Types = "", EarlyAwake = true
        }
    }
}