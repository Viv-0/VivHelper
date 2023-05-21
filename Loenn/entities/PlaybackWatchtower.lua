local watchtower = {}

watchtower.name = "VivHelper/CustomPlaybackWatchtower"
watchtower.depth = -8500
watchtower.justification = {0.5, 1.0}
watchtower.nodeLineRenderType = "line"
watchtower.texture = "objects/lookout/lookout05"
watchtower.nodeLimits = {0, -1}
watchtower.placements = {
    name = "watchtower",
    data = {
        summit = false,
        onlyY = false,
        XMLName="lookout",CustomPlaybackTag="",
        Accel=800, maxSpeed=240,
        InstantStart=false,
    }
}

return watchtower