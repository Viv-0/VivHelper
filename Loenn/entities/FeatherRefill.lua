local refill = {}

refill.name = "VivHelper/FeatherRefill"
refill.depth = -100
refill.placements = {
    {
        name = "one_use",
        data = {
            oneUse = true,
        }
    },
    {
        name = "feather",
        data = {
            oneUse = false
        }
    }
}

refill.texture = "VivHelper/TSSfeatherrefill/idle00"

return refill