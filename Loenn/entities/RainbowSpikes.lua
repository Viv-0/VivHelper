local rainbowHelper = require('mods').requireFromPlugin('libraries.rainbowHelper')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local spikeHelper = require('helpers.spikes')

local spikeVariants = {
    "default",
    "outline",
    "whitereflection"
}



local spikeUp = spikeHelper.createEntityHandler("VivHelper/RainbowSpikesUp", "up", false, false, spikeVariants)
local spikeDown = spikeHelper.createEntityHandler("VivHelper/RainbowSpikesDown", "down", false, false, spikeVariants)
local spikeLeft = spikeHelper.createEntityHandler("VivHelper/RainbowSpikesLeft", "left", false, false, spikeVariants)
local spikeRight = spikeHelper.createEntityHandler("VivHelper/RainbowSpikesRight", "right", false, false, spikeVariants)
local spikes = {spikeUp,spikeDown,spikeLeft,spikeRight}
for i, spike in ipairs(spikes) do
    -- append Color and DoNotAttach options to placements by default
    for _,placement in ipairs(spike.placements) do
        placement.data["Color"] = ""
        placement.data["DoNotAttach"] = false
    end

    -- append Color options to fieldInformation
    spike.fieldInformation["Color"] = {fieldType = "color", allowXNAColors = true, allowEmpty = true}

    -- append sprite function to have rainbow color
    local oldSpriteFunc = spike.sprite
    spike.sprite = function(room, entity) 
        local sprites = oldSpriteFunc(room,entity)
        for _,spike in ipairs(sprites) do 
            local color = entity.Color
            if vivUtil.isNullEmptyOrWhitespace(entity.Color) then
                color = rainbowHelper.getRainbowHue(room, spike.x, spike.y)
            end
            spike:setColor(color)
        end
        return sprites
    end
end
return spikes