local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")
local utils = require("utils")
local formHelper = require("ui.forms.form")
local languageRegistry = require("language_registry")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local utf8 = require("utf8")

local colorPicker = {}

local pickerAreaMinimumSize = 200
local sliderMinimumWidth = 40
local hsvFieldDecimals = 2

local valueRanges = {
    r = {0, 255},
    g = {0, 255},
    b = {0, 255},
    a = {0, 255},
    h = {0, 360},
    s = {0, 100},
    v = {0, 100}
}

local fieldTypes = {
    r = "integer",
    g = "integer",
    b = "integer",
    a = "integer",
}

local areaHSVShader = love.graphics.newShader[[
    uniform float hue;
    vec3 hsv_to_rgb(float h, float s, float v) {
        return mix(vec3(1.0), clamp((abs(fract(h + vec3(3.0, 2.0, 1.0) / 3.0) * 6.0 - 3.0) - 1.0), 0.0, 1.0), s) * v;
    }
    vec4 effect(vec4 color, Image tex, vec2 texture_coords, vec2 screen_coords)
    {
        vec3 rgb = hsv_to_rgb(hue, texture_coords[0], 1 - texture_coords[1]);
        return vec4(rgb[0], rgb[1], rgb[2], 1.0) * color;
    }
]]
local alphaShader = love.graphics.newShader[[
    vec4 lerp(vec4 a, vec4 b, float t) { return a*(1-t)+b*t;  }

    vec4 effect(vec4 color, Image tex, vec2 tc, vec2 sc)
    {
        return lerp(vec4(Texel(tex, tc).rgb, 1), color, tc[0]);
    }
]]
--[[local rainbowShader = love.graphics.newShader[[
    vec3 hsv_to_rgb(float h, float s, float v) {
        return mix(vec3(1.0), clamp((abs(fract(h + vec3(3.0, 2.0, 1.0) / 3.0) * 6.0 - 3.0) - 1.0), 0.0, 1.0), s) * v;
    }
    const float modu = 1;
    uniform float time;

    vec4 effect(vec4 color, Image tx, vec2 tc, vec2 sc) {
        vec3 rgb = hsv_to_rgb(mod(time + 2*tc[0] - tc[1], modu), 1,1);
        return vec4(rgb[0], rgb[1], rgb[2], 1.0) * color;
    }
]]

local function updateAreaColors(hue)
    areaHSVShader:send("hue", hue)
end

local function getColorPickerArea(h, s, v, width, height)
    local canvas = love.graphics.newCanvas(width, height)

    updateAreaColors(h)

    return canvas
end


local function getColorPickerSlider(h, s, v, width, height)
    local canvas = love.graphics.newCanvas(width, height)
    local imageData = canvas:newImageData()

    imageData:mapPixel(function(x, y, r, g, b, a)
        local hue = y / (height - 1)
        local cr, cg, cb = utils.hsvToRgb(hue, 1, 1)

        return cr, cg, cb, 1
    end, 0, 0, width, height)

    local image = love.graphics.newImage(imageData)

    return image, imageData
end

local function getAlphaSlider(_r,_g,_b, width, height)
    local canvas = love.graphics.newCanvas(width, height)
    local imageData = canvas:newImageData()

    imageData:mapPixel(function(x,y,r,g,b,a)
        local alpha = x / (width - 1)
        -- Draw checkerboard pattern, with 1,1,1,1 and 0.5,0.5,0.5,1 alternating every N pixels
        local n = 5
        local Z = (math.floor(x/8) + math.floor((y+4)/8)) % 2 
        
        local t = {(1 - 0.5 * Z), (1 - 0.5 * Z), (1 - 0.5 * Z), 1}
        return t[1],t[2],t[3],t[4]
    end,0,0,width,height)

    local image = love.graphics.newImage(imageData)

    return image, imageData
end

local function areaInteraction(interactionData)
    return function(widget, x, y)
        local areaSize = interactionData.areaSize
        local formFields = interactionData.formFields

        local innerX = utils.clamp(x - widget.screenX, 0, areaSize)
        local innerY = utils.clamp(y - widget.screenY, 0, areaSize)

        local saturation = innerX / areaSize
        local value = 1 - (innerY / areaSize)
        local data = formHelper.getFormData(formFields)

        data.s = utils.round(saturation * 100, hsvFieldDecimals)
        data.v = utils.round(value * 100, hsvFieldDecimals)
        interactionData.forceFieldUpdate = true

        formHelper.setFormData(formFields, data)
    end
end

local function sliderInteraction(interactionData)
    return function(widget, x, y)
        local areaSize = interactionData.areaSize
        local formFields = interactionData.formFields

        local innerY = utils.clamp(y - widget.screenY, 0, areaSize)
        local hue = innerY / areaSize
        local data = formHelper.getFormData(formFields)

        data.h = utils.round(hue * 360, hsvFieldDecimals)
        interactionData.forceFieldUpdate = true

        updateAreaColors(hue)
        formHelper.setFormData(formFields, data)
    end
end

local function alphaInteraction(interactionData)
    return function(widget,x,y)
        local areaSize = interactionData.areaSize
        local formFields = interactionData.formFields

        local innerX = utils.clamp(x - widget.screenX, 0, areaSize)
        local alpha = innerX / areaSize
        local data = formHelper.getFormData(formFields)

        data.a = math.floor(alpha * 255)
        formHelper.setFormData(formFields,data)
    end
end

local function getFormFieldOrder(options)
    local fieldOrder = {}

    table.insert(fieldOrder, "r")
    table.insert(fieldOrder, "g")
    table.insert(fieldOrder, "b")
    table.insert(fieldOrder, "a")

    if options.showHex ~= false then
        table.insert(fieldOrder, "hexColor")
    end

    return fieldOrder
end

local function findChangedColorGroup(current, previous)
    if current.r ~= previous.r or current.g ~= previous.g or current.b ~= previous.b or current.a ~= previous.a then
        return "rgb"

    elseif current.h ~= previous.h or current.s ~= previous.s or current.v ~= previous.v then
        return "hsv"

    elseif current.hexColor ~= previous.hexColor then
        return "hex"
    end
end

-- RGB normalized
local function updateHsvFields(data, r, g, b)
    local h, s, v = utils.rgbToHsv(r, g, b)

    data.h = utils.round(h * 360, hsvFieldDecimals)
    data.s = utils.round(s * 100, hsvFieldDecimals)
    data.v = utils.round(v * 100, hsvFieldDecimals)
end

-- HSV Normalized
local function updateRgbFields(data, h, s, v)
    local r, g, b = utils.hsvToRgb(h, s, v)

    data.r = utils.round(r * 255)
    data.g = utils.round(g * 255)
    data.b = utils.round(b * 255)
end

-- RGB normalized
local function updateHexField(data, r, g, b, a)
    data.hexColor = vivUtil.invertGetColor(r,g,b,a)
end

local function updateFields(data, changedGroup, interactionData)
    local callback = interactionData.callback

    -- Change group here to make logic simpler
    if changedGroup == "hex" then
        local parsed, r, g, b, a = vivUtil.oldGetColor(vivUtil.swapRGBA(data.hexColor))
        if parsed then 
            updateHsvFields(data, r, g, b)
            changedGroup = "hsv"
        end
    end

    if changedGroup == "rgb" then
        local r, g, b = (data.r or 0) / 255, (data.g or 0) / 255, (data.b or 0)/ 255

        updateHsvFields(data, r, g, b)
        updateHexField(data, r, g, b, data.a / 255)

    elseif changedGroup == "hsv" then
        local h, s, v = (data.h or 0) / 360, (data.s or 0) / 100, (data.v or 0) / 100
        updateRgbFields(data, h, s, v)

        local r, g, b = data.r / 255, data.g / 255, data.b / 255
        updateHexField(data, r, g, b, data.a / 255)
    end

    updateAreaColors((data.h or 0) / 360)

    if callback then
        callback(data)
    end

    return data
end

local function fieldUpdater(interactionData)
    return function()
        local formFields = interactionData.formFields

        if interactionData.forceFieldUpdate or formHelper.formValid(formFields) then
            local formData = formHelper.getFormData(formFields)
            local changedGroup = findChangedColorGroup(formData, interactionData.previousFormData or formData)

            if changedGroup then
                local data = updateFields(formData, changedGroup, interactionData)

                formHelper.setFormData(formFields, data)
            end

            interactionData.previousFormData = formData
            interactionData.forceFieldUpdate = false
        end
    end
end

local function areaDrawing(interactionData)
    return function(orig, widget)
        local previousShader = love.graphics.getShader()

        love.graphics.setShader(areaHSVShader)
        orig(widget)
        love.graphics.setShader(previousShader)

        local formData = formHelper.getFormData(interactionData.formFields)
        local areaSize = interactionData.areaSize
        local x = utils.round((formData.s or 0) / 100 * areaSize)
        local y = utils.round((1 - (formData.v or 0) / 100) * areaSize)
        local widgetX, widgetY = widget.screenX, widget.screenY
        local rightX = widgetX + x
        local bottomY = widgetY + y
        local width, height = widget.width, widget.height

        local pr, pg, pb, pa = love.graphics.getColor()
        local previousLineWidth = love.graphics.getLineWidth()

        love.graphics.setLineWidth(1)
        love.graphics.setColor(0, 0, 0, 1)
        love.graphics.rectangle("fill", widgetX, bottomY - 1, width, 3)
        love.graphics.rectangle("fill", rightX - 1, widgetY, 3, height)
        love.graphics.setColor(1, 1, 1, 1)
        love.graphics.rectangle("fill", widgetX + 1, bottomY, width - 2, 1)
        love.graphics.rectangle("fill", rightX, widgetY + 1, 1, height - 2)
        love.graphics.setLineWidth(previousLineWidth)
        love.graphics.setColor(pr, pg, pb, pa)
    end
end

local function sliderDrawIndication(interactionData)
    return function(orig, widget)
        orig(widget)

        local formData = formHelper.getFormData(interactionData.formFields)
        local areaSize = interactionData.areaSize
        local sliderWidth = interactionData.sliderWidth
        local y = utils.round((formData.h or 0) / 360 * areaSize)
        local widgetX, widgetY = widget.screenX, widget.screenY
        local sliderY = widgetY + y
        local width, height = widget.width, widget.height
        local pr, pg, pb, pa = love.graphics.getColor()
        local previousLineWidth = love.graphics.getLineWidth()

        love.graphics.setLineWidth(1)
        love.graphics.setColor(0, 0, 0, 1)
        love.graphics.rectangle("fill", widgetX, sliderY - 1, sliderWidth, 3)
        love.graphics.setColor(1, 1, 1, 1)
        love.graphics.rectangle("fill", widgetX + 1, sliderY, sliderWidth - 2, 1)
        love.graphics.setLineWidth(previousLineWidth)
        love.graphics.setColor(pr, pg, pb, pa)
    end
end

local function alphaSliderDraw(interactionData)
    return function(orig, widget)
        local previousShader = love.graphics.getShader()
        local pr,pg,pb,pa = love.graphics.getColor()
        love.graphics.setShader(alphaShader)
        local formData = formHelper.getFormData(interactionData.formFields)
        if not formData.r then formData.r = 0 end 
        if not formData.g then formData.g = 0 end 
        if not formData.b then formData.b = 0 end 
        love.graphics.setColor(formData.r/255,formData.g/255,formData.b/255,1)
        orig(widget)
        love.graphics.setColor(pr,pg,pb,pa)
        love.graphics.setShader(previousShader)

        local formData = formHelper.getFormData(interactionData.formFields)
        local areaSize = interactionData.areaSize
        local sliderHeight = interactionData.sliderWidth
        local x = utils.round((formData.a or 0) / 255 * areaSize)
        local widgetX, widgetY = widget.screenX, widget.screenY
        local sliderX = widgetX + x
        local width, height = widget.width, widget.height
        pr, pg, pb, pa = love.graphics.getColor()
        local previousLineWidth = love.graphics.getLineWidth()

        love.graphics.setLineWidth(1)
        love.graphics.setColor(0, 0, 0, 1)
        love.graphics.rectangle("fill", sliderX - 1,widgetY, 3, sliderHeight)
        love.graphics.setColor(1, 1, 1, 1)
        love.graphics.rectangle("fill", sliderX, widgetY + 1, 1, sliderHeight - 2)
        love.graphics.setLineWidth(previousLineWidth)
        love.graphics.setColor(pr, pg, pb, pa)
    end
end

function colorPicker.getColorPicker(hexColor, options)
    if hexColor == "Rainbow" then hexColor = "00000000" end

    options = options or {}

    local language = languageRegistry.getLanguage()
    local callback = options.callback or function() end

    local parsed, r, g, b, a = vivUtil.oldGetColor(hexColor)
    local h, s, v = utils.rgbToHsv(r or 0, g or 0, b or 0)

    local fieldOrder = getFormFieldOrder(options)
    local formData = {
        r = (r or 0) * 255,
        g = (g or 0) * 255,
        b = (b or 0) * 255,
        a = (a or 1) * 255,
        h = utils.round(h * 360, hsvFieldDecimals),
        s = utils.round(s * 100, hsvFieldDecimals),
        v = utils.round(v * 100, hsvFieldDecimals)
    }

    local formOptions = {
        columns = 3,
        fieldOrder = fieldOrder,
        hideUnordered = true,
        fields = {}
    }

    for name, _ in pairs(formData) do
        local field = formOptions.fields[name] or {}
        local ranges = valueRanges[name]

        field.fieldType = fieldTypes[name]
        if name == "a" then
            field.displayName = "Alpha"
            field.tooltipText = "Opacity of the color, where 0 is fully transparent and 255 is fully opaque."
        else
            field.displayName = tostring(language.ui.colorPicker.fieldTypes.name[name])
            field.tooltipText = tostring(language.ui.colorPicker.fieldTypes.description[name])
        end
        field.width = 60
        if name == "hexColor" then
            field.displayTransformer = vivUtil.hexToDisplayHex
            field.valueTransformer = vivUtil.hexToDisplayHex
        end
        if ranges then
            field.minimumValue = ranges[1]
            field.maximumValue = ranges[2]
        end

        formOptions.fields[name] = field
    end

    local formBody, formFields = formHelper.getFormBody(formData, formOptions)

    -- Form body height is not properly calculated at this point
    -- This approximation seems to be accurate enough
    local areaSize = options.areaSize or math.max(formBody.height * #fieldOrder * 6 / 7, pickerAreaMinimumSize)
    local sliderWidth = options.sliderWidth or sliderMinimumWidth

    local areaCanvas = getColorPickerArea(h, s, v, areaSize, areaSize)
    local sliderImage, sliderImageData = getColorPickerSlider(h, s, v, sliderWidth, areaSize)
    local alphaImage, alphaImageData = getAlphaSlider(r,g,b, areaSize, sliderWidth)

    local interactionData = {
        areaCanvas = areaCanvas,
        sliderImage = sliderImage,
        sliderImageData = sliderImageData,
        alphaImage = alphaImage,
        alphaImageData = alphaImageData,
        formFields = formFields,
        areaSize = areaSize,
        sliderWidth = sliderWidth,
        callback = callback,
        forceFieldUpdate = true,
        alphaPreMult = options.alphaPreMult,
        formData = formData
    }

    local areaElement = uiElements.image(areaCanvas):with({
        interactive = 1,
        onDrag = areaInteraction(interactionData),
        onClick = areaInteraction(interactionData)
    }):hook({
        draw = areaDrawing(interactionData)
    })
    local sliderElement = uiElements.image(sliderImage):with({
        interactive = 1,
        onDrag = sliderInteraction(interactionData),
        onClick = sliderInteraction(interactionData)
    }):hook({
        draw = sliderDrawIndication(interactionData)
    })
    local alphaElement = uiElements.image(alphaImage):with({
        interactive = 1,
        onDrag = alphaInteraction(interactionData),
        onClick = alphaInteraction(interactionData)}):hook({
            draw = alphaSliderDraw(interactionData, r,g,b)
        })
    local rainbowButton = uiElements.image()


    local column1 = uiElements.column() -- Creates a column "object" that contains children that are itemizable to the grid
    column1.children = {areaElement, alphaElement} 
    column1.cacheable = false -- required for live updates to areaElement and alphaElement

    local elements = {
        column1, -- puts the column object first in the elements of the window
        sliderElement,
    }

    if #fieldOrder > 0 then
        table.insert(elements, formBody)
    end

    local pickerRow = uiElements.row(elements):with({
        update = fieldUpdater(interactionData)
    })

    return pickerRow
end

return colorPicker