local refill = {}

refill.name = "VivHelper/RedDashRefill"
refill.depth = -100
refill.placements = {
    {
        name = "red_dash",
        data = {
            oneUse = false
        }
    }, {
        name = "one_dash",
        data = {
            oneUse = true
        }
    }
}

refill.texture = "VivHelper/redDashRefill/redIdle00"

return refill