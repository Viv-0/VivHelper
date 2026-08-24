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
local drawableFunction = require("structs.drawable_function")
local drawableRectangle = require("structs.drawable_rectangle")
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local form = require('ui.forms.form')

--hotreload manager: if triggers._vivh_unloadSeq has had a value set already, we're reloading the plugin, since triggers._vivh_unloadSeq is only created in this file.
if triggers._vivh_unloadSeq then triggers._vivh_unloadSeq() end 
-- if triggers contains an object "_vivh_unloadSeq" then run the function. that function is defined at the end of this codebase. If any other mod creates this function, then it will break this, so use a different Lonn source file.

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
    placementUtils.finalizePlacement = _orig_placementUtils_finalizePlacement
end

return {}