return nil
-- i'll come back to this later lol
local pc = {
    name = "VivHelper/PowerupController"
}
pc.placements = {
    name = "main",
    data = {
        format = "Override",
        convolve = false,
        defaultPowerup = ""
    }
}
pc.fieldInformation = {
    format = {options = {"Override", "Prevent", "Queue"}, editable = false}
}