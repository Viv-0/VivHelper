local dL = {name = "VivHelper/DebrisLimiter", 
depth = -1000000,
placements = {
    name = "main",
    data = {
        limiter=1
    }
}, fieldInformation = {
    limiter = {fieldType = "integer", options = {
        {"IgnoreSolids", -1}, {"DisableAllParticles", 1}, {"ReturnToDefault", 0}}
}}}

dL.texture = "ahorn/VivHelper/DebrisLimiter"

return dL