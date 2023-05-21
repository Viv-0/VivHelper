local polygon = require("mods").requireFromPlugin("libraries.polygon")

local killbox = {
    name = "VivHelper/PolygonKillbox",
    nodeLimits = { 2, 999 },
    depth = -math.huge + 6,
    placements = {name = "killbox" },
    _vivh_finalizePlacement = function(room, layer, item)
        item.nodes[2].x = item.x + 8
        item.nodes[2].y = item.y + 16
    end
}

killbox.sprite = polygon.getSpriteFunc(true)
killbox.nodeSprite = polygon.nodeSprite
killbox.selection = polygon.selection

return killbox