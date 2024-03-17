local refill = {}

refill.name = "VivHelper/BumperRefill"
refill.depth = -100
refill.placements = {
    {
        name = "one_dash",
        data = {
            oneUse = true,
        }
    },
    {
        name = "bumper_dash",
        data = {
            oneUse = false
        }
    }
}

refill.texture = "VivHelper/TSSbumperrefill/idle00"

return refill