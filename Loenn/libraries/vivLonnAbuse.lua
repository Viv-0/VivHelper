-- Viv's Lonn Abuse

--[[
    Key detail: keywords will explicitly use _vivh_ as prefixes so as to not step on anyone's toes.
    If you use that prefix, you either enforce a dependency on VivHelper, or you're stepping on mine. If you want me to make this a plugin that's publicly available, don't copypaste here, just *tell me* and I'll do it.

    for any placement: (any templateable item, i.e. entities, triggers, decals)
        handler._vivh_unregisteredValues => table of strings or function(entity) returning table of strings
            Used like ignoredFields, but these values are not saved to the map. Useful for entity-specific values relating to temporary features, mainly Loenn visualization.
        handler._vivh_finalizePlacement => function
            Makes any potential changes to the initial state of the itemTemplate directly *after* placement on the map.
    for triggers only:
        handler._vivh_drawRect => function(trigger, room, handler) => 
            Completely replaces the rectangle of the drawable
        handler._vivh_replaceDrawTextFunc => any (function, nil, or anything else)
            If function => replace the text drawing function with this
            If nil => default to LoennExtended/Vanilla drawing function
            If anything else => treat it as a boolean false and simply don't draw the text. 
        handler._vivh_textOverride => string
            If not nil, replaces the displayname
        handler._vivh_drawAddendum => function(trigger, room, handler)
            Stuff drawn after the text drawing sequence and any additional "post-base trigger rendering points"
]]

local entities = require("entities")
local triggers = require("triggers")
local colors = require("consts.colors")
local placementUtils = require("placement_utils")
local utils = require("utils")
local drawing = require("utils.drawing")
local mods = require('mods')
local drawableFunction = require("structs.drawable_function")
local drawableRectangle = require("structs.drawable_rectangle")
local vivUtil = mods.requireFromPlugin('libraries.vivUtil')
local loadedState = require('loaded_state')

local form = require('ui.forms.form')

local loennExtended_triggerAPI = mods.requireFromPlugin("libraries.api.triggerRendering", "LoennExtended")
local loennExtended_textAPI = mods.requireFromPlugin("libraries.api.textRendering", "LoennExtended")
local loennExtended_layerAPI = mods.requireFromPlugin("libraries.api.layers", "LoennExtended")

--hotreload manager: if triggers._vivh_unloadSeq has had a value set already, we're reloading the plugin, since triggers._vivh_unloadSeq is only created in this file.
if triggers._vivh_unloadSeq then triggers._vivh_unloadSeq() end 
-- if triggers contains an object "_vivh_unloadSeq" then run the function. that function is defined at the end of this codebase. If any other mod creates this function, then it will break this, so use a different Lonn source file.

local function orig_triggers_getDrawable_backgroundOnly(trigger)
    local x = trigger.x or 0
    local y = trigger.y or 0
    local width = trigger.width or 16
    local height = trigger.height or 16
    local lineWidth = love.graphics.getLineWidth()

    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(colors.triggerBorderColor)
        love.graphics.rectangle("line", x + lineWidth / 2, y + lineWidth / 2, width - lineWidth, height - lineWidth)
        love.graphics.setColor(colors.triggerColor)
        love.graphics.rectangle("fill", x + lineWidth, y + lineWidth, width - 2 * lineWidth, height - 2 * lineWidth)
    end)
end

local function orig_triggers_getDrawable_textOnly(trigger, textOverride)
    local displayName = textOverride or triggers.getDrawableDisplayText(trigger)

    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(colors.triggerTextColor)
        drawing.printCenteredText(displayName, trigger.x or 0, trigger.y or 0, trigger.width or 16, trigger.height or 16, love.graphics.getFont(), 1)
    end)
    
end

local _orig_triggers_getDrawable = triggers.getDrawable
function triggers.getDrawable(name, handler, room, trigger, viewport)
    local _handler = triggers.registeredTriggers[trigger._name]
        
    return drawableFunction.fromFunction(function() 
            if _handler._vivh_drawRect then
                _handler._vivh_drawRect(trigger, room, _handler)
            elseif loennExtended_triggerAPI then 
                loennExtended_triggerAPI.getTriggerDrawableBg(trigger) 
            else 
                orig_triggers_getDrawable_backgroundOnly(trigger)
            end
            if loennExtended_textAPI and loennExtended_triggerAPI then
                loennExtended_textAPI.printCenteredText(_handler._vivh_textOverride and _handler._vivh_textOverride(room, trigger) or loennExtended_triggerAPI.getDisplayText(trigger), trigger.x or 0, trigger.y or 0, trigger.width or 16, trigger.height or 16, love.graphics.getFont(), loennExtended_triggerAPI.getFontSize())
            else
                orig_triggers_getDrawable_textOnly(trigger, _handler._vivh_textOverride and _handler._vivh_textOverride(room, trigger))
            end
            if _handler._vivh_drawAddendum then
                _handler._vivh_drawAddendum(trigger, room, handler)
            end
        end), 0
end

local _orig_triggers_addDrawables = triggers.addDrawables
function triggers.addDrawables(batch, room, targets, viewport, yieldRate)
    local font = love.graphics.getFont()

    -- Add rectangles first, then batch draw all text

    local postDrawEvent = function() end
    local function registerEvent(func)
        local old = postDrawEvent
        postDrawEvent = function()
            old()
            func()
        end
    end
    for i, trigger in ipairs(targets) do
        local handler = triggers.registeredTriggers[trigger._name]
        trigger._vivh_handler = handler -- saves us the work of looking up the file later
        local drawable = nil
        if handler._vivh_drawRect then 
            drawable = drawableFunction.fromFunction(handler._vivh_drawRect, trigger, room, handler) 
        elseif vivUtil.isModLoaded("LoennExtended") then 
            drawable = loennExtended_triggerAPI.getTriggerDrawableBg(trigger)
        else 
            drawable = drawableRectangle.fromRectangle("bordered", trigger.x or 0, trigger.y or 0, trigger.width or 16, trigger.height or 16, colors.triggerColor, colors.triggerBorderColor)
        end
        batch:addFromDrawable(drawable)

        if i % yieldRate == 0 then
            coroutine.yield(batch)
        end
        if handler._vivh_drawAddendum then
            registerEvent(function() handler._vivh_drawAddendum(trigger, room, handler) end, false)
        end
    end


    local textBatch = love.graphics.newText(font)

    for i, trigger in ipairs(targets) do
        local handler = trigger._vivh_handler
        trigger._vivh_handler = nil
        local displayName = handler._vivh_textOverride and handler._vivh_textOverride(room, trigger) or nil
        if vivUtil.isModLoaded("LoennExtended") then -- LoennExtended
            displayName = displayName or loennExtended_triggerAPI.getDisplayText(trigger)
    
            local color = colors.triggerTextColor
            -- add integration for layers
            if loennExtended_layerAPI and not loennExtended_layerAPI.isInCurrentLayer(trigger) then
                vivUtil.alphMult(color, loennExtended_layerAPI.hiddenLayerAlpha)
            end
            loennExtended_textAPI.addCenteredText(textBatch, displayName, trigger.x or 0, trigger.y or 0, trigger.width or 16, trigger.height or 16, font, loennExtended_triggerAPI.getFontSize(), nil, color)
        else --Loenn vanilla
            displayName = displayName or triggers.getDrawableDisplayText(trigger)
            drawing.addCenteredText(textBatch, displayName, trigger.x or 0, trigger.y or 0, trigger.width or 16, trigger.height or 16, font, 1)
        end
    end

    local function func()
        drawing.callKeepOriginalColor(function()
            love.graphics.setColor(colors.triggerTextColor)
            love.graphics.draw(textBatch)
        end)
        postDrawEvent() -- runs all the triggers' events in order.
    end
    batch:addFromDrawable(drawableFunction.fromFunction(func))
    return batch
end

local _orig_placementUtils_finalizePlacement = placementUtils.finalizePlacement
placementUtils.finalizePlacement = function(room, layer, item)
    _orig_placementUtils_finalizePlacement(room, layer, item)
    local handler = nil
    if layer == "entities" then
        handler = entities.registeredEntities[item._name]
    elseif layer == "triggers" then
        handler = triggers.registeredTriggers[item._name]
    end
    if handler and handler._vivh_finalizePlacement then handler._vivh_finalizePlacement(room, layer, item) end
end

-- ##########################################################################################

function triggers._vivh_unloadSeq() -- Handles hotreload.
    triggers.addDrawables = _orig_triggers_addDrawables
    triggers.getDrawable = _orig_triggers_getDrawable
    placementUtils.finalizePlacement = _orig_placementUtils_finalizePlacement
    loadedState.side.map._vivh_tags = nil
end


return {}