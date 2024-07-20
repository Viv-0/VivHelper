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
        placement.data["color"] = ""
        placement.data["Grouped"] = false
    end

    -- append Color options to fieldInformation
    spike.fieldInformation["color"] = {fieldType = "color", allowXNAColors = true, allowEmpty = true, useAlpha = true}

    -- append sprite function to have rainbow color
    local oldSpriteFunc = spike.sprite
    spike.sprite = function(room, entity) 
        local sprites = oldSpriteFunc(room,entity)
        for _,spike in ipairs(sprites) do 
            local color = nil
            local table = vivUtil.GetColorTable(entity, "Color", "color", true, {1,1,1,0})
            if table[4] == 0 then
                color = rainbowHelper.getRainbowHue(room, spike.x, spike.y)
            else 
                color = table
            end
            spike:setColor(color)
        end
        return sprites
    end
end
return spikes