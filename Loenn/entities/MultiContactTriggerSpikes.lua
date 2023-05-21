local spikeHelper = require("helpers.spikes")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local spikeVariants = {
    "default",
    "outline",
    "cliffside",
    "reflection"
}
local spikeUp = spikeHelper.createEntityHandler("VivHelper/MultiContactTriggerSpikesUp", "up", false, true, spikeVariants)
local spikeDown = spikeHelper.createEntityHandler("VivHelper/MultiContactTriggerSpikesDown", "down", false, true, spikeVariants)
local spikeLeft = spikeHelper.createEntityHandler("VivHelper/MultiContactTriggerSpikesLeft", "left", false, true, spikeVariants)
local spikeRight = spikeHelper.createEntityHandler("VivHelper/MultiContactTriggerSpikesRight", "right", false, true, spikeVariants)

local customFieldInfo = {
}

local function updateHandlers(handler)
    handler.fieldInformation["contactLimit"] = {fieldType = "integer", minimumValue=1}

    for _,i in ipairs(handler.placements) do
        i.data["contactLimit"] = 1
    end
end

updateHandlers(spikeUp)
updateHandlers(spikeDown)
updateHandlers(spikeLeft)
updateHandlers(spikeRight)

return {
    spikeUp,
    spikeDown,
    spikeLeft,
    spikeRight
}