local rainbowHelper = require('mods').requireFromPlugin('libraries.rainbowHelper')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local spikeHelper = require('helpers.spikes')

local spikeVariants = {
    "default",
    "outline",
    "whitereflection"
}

local spikeUp = spikeHelper.createEntityHandler("VivHelper/RainbowTriggerSpikesUp", "up", false, true, spikeVariants)
local spikeDown = spikeHelper.createEntityHandler("VivHelper/RainbowTriggerSpikesDown", "down", false, true, spikeVariants)
local spikeLeft = spikeHelper.createEntityHandler("VivHelper/RainbowTriggerSpikesLeft", "left", false, true, spikeVariants)
local spikeRight = spikeHelper.createEntityHandler("VivHelper/RainbowTriggerSpikesRight", "right", false, true, spikeVariants)
local spikes = {spikeUp,spikeDown,spikeLeft,spikeRight}
for i, spike in ipairs(spikes) do
    -- append Color and DoNotAttach options to placements by default
    for _,placement in ipairs(spike.placements) do
        placement.data["Color"] = ""
        placement.data["Grouped"] = false
    end

    -- append Color options to fieldInformation
    spike.fieldInformation["Color"] = {fieldType = "color", allowXNAColors = true, allowEmpty = true}

    -- append sprite function to have rainbow color
    local oldSpriteFunc = spike.sprite
    spike.sprite = function(room, entity) 
        local sprites = oldSpriteFunc(room,entity)
        for _,spike in ipairs(sprites) do 
            local color = nil
            local colorParsed, r,g,b,a = vivUtil.getColor(entity.Color)
            if vivUtil.isNullEmptyOrWhitespace(entity.Color) or not colorParsed then
                color = rainbowHelper.getRainbowHue(room, spike.x, spike.y)
            else color = {r,g,b,a or 1}
            end
            spike:setColor(color)
        end
        return sprites
    end
end
return spikes