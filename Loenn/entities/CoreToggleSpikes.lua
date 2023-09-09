local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local spikeHelper = require('helpers.spikes')

local spikeVariants = {
    "default",
    "outline",
    "whitereflection"
}

local spikeUp = spikeHelper.createEntityHandler("VivHelper/CoreToggleSpikesUp", "up", false, true, spikeVariants)
local spikeDown = spikeHelper.createEntityHandler("VivHelper/CoreToggleSpikesDown", "down", false, true, spikeVariants)
local spikeLeft = spikeHelper.createEntityHandler("VivHelper/CoreToggleSpikesLeft", "left", false, true, spikeVariants)
local spikeRight = spikeHelper.createEntityHandler("VivHelper/CoreToggleSpikesRight", "right", false, true, spikeVariants)
local spikes = {spikeUp,spikeDown,spikeLeft,spikeRight}
for i, spike in ipairs(spikes) do
    -- append Color and DoNotAttach options to placements by default
    for _,placement in ipairs(spike.placements) do
        placement.data["coreMode"] = "Hot"
        placement.data["hotColor"] = "eb2a3a"
        placement.data["coldColor"] = "a6fff4"
        placement.data["attachToSolid"] = true
    end

    -- append Color options to fieldInformation
    spike.fieldInformation["coreMode"] = {fieldType = "string", editable=false, options = {"Hot", "Cold"}}
    spike.fieldInformation["hotColor"] = {fieldType = "color", allowXNAColors = true}
    spike.fieldInformation["coldColor"] = {fieldType = "color", allowXNAColors = true}
    

    -- append sprite function to have rainbow color
    local oldSpriteFunc = spike.sprite
    spike.sprite = function(room, entity) 
        local sprites = oldSpriteFunc(room,entity)
        for _,spike in ipairs(sprites) do 
            local color = ((entity.coreMode == "Hot") and entity.hotColor or entity.coldColor)
            spike:setColor(color)
        end
        return sprites
    end
end
return spikes