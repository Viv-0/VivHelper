local refill = {}

refill.name = "VivHelper/WarpDashRefill"
refill.depth = -100
refill.placements = {
    {
        name = "one_dash",
        data = {
            oneUse = true,
        }
    },
    {
        name = "warp_dash",
        data = {
            oneUse = false
        }
    }
}

refill.texture = "VivHelper/TSStelerefill/idle00"

return refill