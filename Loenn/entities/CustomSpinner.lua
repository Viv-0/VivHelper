local utils = require("utils")
local vivUtil = require("mods").requireFromPlugin("libraries.vivUtil")
local rainbowHelper = require('mods').requireFromPlugin('libraries.rainbowHelper')
local drawableSprite = require('structs.drawable_sprite')
local drawableRect = require('structs.drawable_rectangle')
local modHandler = require('mods')
local atlases = require('atlases')

local defaultTextureFG = "VivHelper/customSpinner/white/fg_white00"
local defaultTextureBG = "VivHelper/customSpinner/white/bg_white00"

local customSpinner = {}

customSpinner.name = "VivHelper/CustomSpinner"
customSpinner.fieldInformation = {
    Type = {fieldType = "string", options = {{"White", "White"}, {"Rainbow", "RainbowClassic"}}, editable = true },
    Depth = {fieldType = "integer"},
    Color = {fieldType = "VivHelper.oldColor", allowXNAColors = true},
    BorderColor = {fieldType = "VivHelper.oldColor", allowXNAColors = true},
    ShatterColor = {fieldType = "VivHelper.oldColor", allowXNAColors = true},
    color = {fieldType = "color", allowXNAColors = true, useAlpha = true, showAlpha = true},
    borderColor = {fieldType = "color", allowXNAColors = true, useAlpha = true, showAlpha = true},
    shatterColor = {fieldType = "color", allowXNAColors = true, useAlpha = true, showAlpha = true},
    HitboxType = {
        fieldType = "string",
        options = {
            {"Scaled Spinner (Default)", "C:6;0,0|R:16,4;-8,*1@-4"},
            {"Thin Rectangle", "C:6;0,0|R:16,*4;-8,*-3"},
            {"Normal Circle (No Rect)", "C:6;0,0"},
            {"Larger Circle (No Rect)", "C:8;0,0"},
            {"Upside Down Spinner", "C:6;0,0|R:16,4;-8,*-1"},
            {"Upside Down Spinner, Thin Rect", "C:6;0,0|R:16,*4;-8,*-1"}
        },
        editable = true
    }
}
customSpinner.fieldInformation["ref"] = vivUtil.GetFilePathWithNoTrailingNumbers(false)

customSpinner.placements = {
    name = "VivHelper/CustomSpinner",
    data = {
        AttachToSolid = true,
        Type = "White",
        ref = "VivHelper/customSpinner/white/fg_white",
        Color = "FFFFFF",
        BorderColor = "000000",
        ShatterColor = "FFFFFF",
        HitboxType =  "C:6;0,0|R:16,4;-8,*1@-4",
        shatterOnDash = false,
        Scale = 1.0,
        ImageScale = 1.0,
        CustomDebris = false,
        DebrisToScale = true,
        ShatterFlag = "",
        isSeeded = false,
        Seed = -1,
        ignoreConnection = false,
        flagToggle = "",
        Depth = -8500
    }
}



local textureCache = {}
local function getSprites(entity)
    local s = nil
    if entity.ref then 
        s = string.gsub(entity.ref, "/[bf]g_?(%a+)%d*$", "|%1")
    else
        local s2 = entity.Subdirectory
        if entity.FrostHelper == true then 
            s2 = "" 
        elseif vivUtil.isNullEmptyOrWhitespace(s2) then
            if entity.CurrentVersion then s2 = "" else s2 = "|white" end
        else s2 = "|" .. s2 end
        s = entity.Directory .. s2
    end
    if s then
        if not textureCache[s] then
            local fg, bg
            local t = string.gsub(s, "|(%a+)$", "/fg_%100")
            if t then
                fg = atlases.getResource(t, "Gameplay")
            end
            if not fg then 
                t = string.gsub(s, "|(%a+)$", "/fg_%1")
                if t then
                    fg = atlases.getResource(t, "Gameplay")
                end
            end
            t = string.gsub(s, "|(%a+)$", "/bg_%100")
            if t then 
                bg = atlases.getResource(t, "Gameplay")
            end
            if not bg then
                t = string.gsub(s, "|(%a+)$", "/bg_%1")
                if t then
                    bg = atlases.getResource(t, "Gameplay")
                end
            end
            if fg and bg then
                textureCache[s] = {fg, bg}
            else 
                return {vivUtil.missingImageMeta, vivUtil.missingImageMeta}
            end
        end
        return textureCache[s]
    end
end

local function getConnectorSprites(room, entity, borderMeta, entityColor, entityBorderColor)

    local sprites = {}
    for _, target in ipairs(room.entities) do -- Gets entities from rooms entities
        if target ~= nil and target ~= entity and entity._name == target._name and entity.attachToSolid == target.attachToSolid then
            local scale = (12 * entity.Scale + 12 * target.Scale)
            if utils.distanceSquared(entity.x, entity.y, target.x, target.y) < scale * scale and entity._id < target._id then

                local sprite = drawableSprite.fromMeta(borderMeta, entity)
                local mX = (target.x + entity.x) / 2
                local mY=  (target.y + entity.y) / 2
                sprite:setPosition(mX, mY)
                local targetColor = vivUtil.GetColorTable(target, "Color", "color", true, {1,1,1,1})
                local color = vivUtil.colorLerp(entityColor, targetColor, 0.5)
                if entityColor == {1,1,1,0} or targetColor == {1,1,1,0} or
                   (entity.Type and entity.Type ~= "White") or (target.Type and target.Type ~= "White") then color = rainbowHelper.getRainbowHue(room,mX,mY,0,0,true) end
                sprite:setColor(color)
                local scale = (entity.Scale + target.Scale) / 2
                sprite:setScale(scale,scale)
                table.insert(sprites, sprite)
            end
        end
    end
    return sprites
end

function customSpinner.sprite(room, entity)
    local color = vivUtil.GetColorTable(entity, "Color", "color", true, {1,1,1,1})
    local borderColor = vivUtil.GetColorTable(entity, "BorderColor", "borderColor", true, {0,0,0,1})
    local connectors, sprites = nil,nil;

    local cache = getSprites(entity)
    -- connectors don't get borders here, i do not care :)
    sprites = getConnectorSprites(room, entity, cache[2], color, borderColor)

    local fgSprite = drawableSprite.fromMeta(cache[1], entity)
    fgSprite:setColor(color)
    vivUtil.addAll(sprites, vivUtil.getBorder(fgSprite, borderColor))
    table.insert(sprites, fgSprite)
    return sprites
end



function customSpinner.selection(room, entity)
    return utils.rectangle(entity.x - 8 * entity.Scale, entity.y - 8 * entity.Scale, 16 * entity.Scale, 16 * entity.Scale)
end

return customSpinner