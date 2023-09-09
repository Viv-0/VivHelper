local utils = require("utils")
local loadedState = require("loaded_state")

local rainbowHelper = {}

local function yoyo(value)
    if value <= 0.5 then return value * 2 end

    return 1 - (value - 0.5) * 2
end


local function vanillaRainbow(room, x, y)
    local rx,ry = x + room.x, y + room.y
    local posLength = math.sqrt((rx * rx) + (ry * ry)) % 280 / 280

    return { utils.hsvToRgb(0.4 + yoyo(posLength) * 0.4, 0.4, 0.9) }
end

-- Maddie Helping Hand integration

local function distance(v1, v2)
    local x = v1.x - v2.x
    local y = v1.y - v2.y

    return math.sqrt(x*x + y*y)
end

local function clamp(value, min, max)
    value = ((value > max) and max or value);
    value = ((value < min) and min or value);
    return value;

end

local function lerp(value1, value2, amount)
	return value1 + (value2 - value1) * amount;
end

-- Performs linear interpolation of two colors
local function lerpColor(a, b, progress)
    local amount = clamp(progress, 0, 1);

    return {
        lerp(a[1], b[1], amount),
        lerp(a[2], b[2], amount),
        lerp(a[3], b[3], amount),
        lerp(a[4] or 1, b[4] or 1, amount)
    }
end

local function getModHue(colors, gradientSize, room, position, loopColors, center, gradientSpeed)
    local len = #colors

    if len == 1 then
        -- edge case: there is 1 color, just return it!
        return colors[1]
    end

    local progress = distance(position, center)

    while progress < 0 do
        progress = progress + gradientSize
    end

    progress = progress % gradientSize / gradientSize

    if not loopColors then
        progress = yoyo(progress);
    end

    if progress == 1 then
        return colors[len];
    end

    local globalProgress = (len - 1) * progress
    local colorIndex = math.floor(globalProgress)
    local progressInIndex = globalProgress - colorIndex
    return lerpColor(colors[colorIndex + 1], colors[colorIndex + 2], progressInIndex)
end

local function colorsFromList(colorString)
    local split = colorString:split(',')()
    local t = {}

    for _, value in ipairs(split) do
        table.insert(t, utils.getColor(utils.trim(value)))
    end

    return t
end

local function getColorFromController(room, x, y, controller)
    return getModHue(
        colorsFromList(controller.colors),
        controller.gradientSize,
        room, {x = x, y = y}, controller.loopColors, {x=controller.centerX, y=controller.centerY}, controller.gradientSpeed
    )
end

function rainbowHelper.getRainbowHue(room, x, y, width, height)
    width, height = width or 16, height or 16
    local selfRect = utils.rectangle(x - width / 2, y - height / 2, width, height)

    for _, entity in ipairs(room.entities) do
        local name = entity._name
        if name == "MaxHelpingHand/RainbowSpinnerColorController"
        or name == "MaxHelpingHand/FlagRainbowSpinnerColorController" then
            return getColorFromController(room, x, y, entity)
        elseif name == "MaxHelpingHand/RainbowSpinnerColorAreaController"
            or name == "MaxHelpingHand/FlagRainbowSpinnerColorAreaController" then

            if utils.aabbCheck(entity, selfRect) then
                return getColorFromController(room, x, y, entity)
            end
        end
    end


    return vanillaRainbow(room, x, y)
end

return rainbowHelper