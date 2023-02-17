local bbcr = {
    name = "VivHelper/BadelineBoostCustomRefill",
}
bbcr.depth = -1000000
bbcr.nodeLineRenderType = "line"
bbcr.texture = "ahorn/VivHelper/baddyboostcustomrefill"
bbcr.nodeLimits = {0, -1}
bbcr.placements = {
    name = "main",
    data = {
        lockCamera = true,
        canSkip = false,
        finalCh9Boost = false,
        finalCh9GoldenBoost = false,
        finalCh9Dialog = false,
        DashesLogic="+0", StaminaLogic="+0"
    }
}

local bbnr = require('utils').deepcopy(bbcr)
bbnr.name = "VivHelper/BadelineBoostNoRefill"
bbnr.texture = "ahorn/VivHelper/baddyboostnorefill"
bbnr.placements.data = {
    lockCamera = true,
    canSkip = false,
    finalCh9Boost = false,
    finalCh9GoldenBoost = false,
    finalCh9Dialog = false,
    NoDashRefill=true, NoStaminaRefill=false

}

return {bbcr,bbnr}