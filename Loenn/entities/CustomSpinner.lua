local utils = require("utils")
local vivUtil = require("mods").requireFromPlugin("libraries.vivUtil")
local rainbowHelper = require('mods').requireFromPlugin('libraries.rainbowHelper')
local drawableSprite = require('structs.drawable_sprite')

local defaultTextureFG = "VivHelper/customSpinner/white/fg_white00"
local defaultTextureBG = "VivHelper/customSpinner/white/bg_white00"

local customSpinner = {}

customSpinner.name = "VivHelper/CustomSpinner"
customSpinner.fieldInformation = {
    Type = {fieldType = "string", options = {{"White", "White"}, {"Rainbow", "RainbowClassic"}}, editable = true },
    -- Depths = {fieldType = "integer", options = depths ???}
    Color = {fieldType = "VivHelper.color", allowXNAColors = true},
    BorderColor = {fieldType = "VivHelper.color", allowXNAColors = true},
    ShatterColor = {fieldType = "VivHelper.color", allowXNAColors = true},
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
    },
    ref = {fieldType = "path", allowFiles = true, allowFolders = false, filenameProcessor = function(filename, rawFilename, prefix) return vivUtil.trim(filename):gsub("%d+","") end}
}

customSpinner.placements = {
    name = "VivHelper/CustomSpinner",
    data = {
        AttachToSolid = true,
        Type = "White",
        ref = "VivHelper/customSpinner/white/fg_white",
        Color = "FFFFFFFF",
        BorderColor = "FF000000",
        ShatterColor = "FFFFFFFF",
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

function customSpinner.depth(room, entity, viewport) 
    return entity.Depth
end

local function getSqDistWithScaling(e1, e2)
    local distSq = ((e2.x - e1.x) * (e2.x - e1.x)) + ((e2.y - e1.y) * (e2.y - e1.y))
    local compSq = 144 * (e2.Scale + e1.Scale) * (e2.Scale + e1.Scale);
    return distSq < compSq
end

-- Taken from FrostHelper
local function createConnectorsForSpinner(room, entity, bgSprite)
    local sprites = {}

    for i = 1, #room.entities, 1 do
        local e2 = room.entities[i]
        if e2 == entity then break end
        if e2._name == entity._name and e2.attachToSolid == entity.attachToSolid and getSqDistWithScaling(entity, e2) then
            local connector = vivUtil.copyTexture(bgSprite, (entity.x + e2.x) / 2, (entity.y + e2.y) / 2, false)
            connector.depth = entity.Depth + 1
            table.insert(sprites, connector)
        end
    end
    return sprites
end

local function getSpinnerSprite(entity, foreground)
    -- Prevent color from spinner to tint the drawable sprite
    local color = string.lower(entity.color or defaultSpinnerColor)
    local position = {
        x = entity.x,
        y = entity.y
    }

    if customSpinnerColors[color] then
        color = customSpinnerColors[color]
    end

    local texture = getSpinnerTexture(entity, color, foreground)
    local sprite = drawableSprite.fromTexture(texture, position)

    -- Check if texture color exists, otherwise use default color
    -- Needed because Rainbow and Core colors doesn't have textures
    if sprite then
        return sprite

    else
        texture = getSpinnerTexture(entity, unknownSpinnerColor, foreground)

        return drawableSpriteStruct.fromTexture(texture, position)
    end
end



local function getSprite(entity, bg)
    local s = ""
    if entity.ref then 
        s = entity.ref .. "00"
        if bg then s = s:reverse():gsub("gf/","gb/",1):reverse() end -- get last occurrence of /fg and replace with /bg if background sprite
    else
        local s2 = not not entity.Subdirectory and "_" .. entity.Subdirectory or ""
        s = entity.Directory .. (bg and "/bg" or "/fg") .. entity.Subdirectory .. "00"
    end 
    return drawableSprite.fromTexture(s, entity)
end
--[[ Code copy from C#
    foreach (CustomSpinner target in base.Scene.Tracker.GetEntities<CustomSpinner>()) {
        if (target.createConnectors && target.ID > ID && !(target is MovingSpinner) && target.AttachToSolid == AttachToSolid && (target.Position - Position).LengthSquared() < (float) Math.Pow((double) (12 * (scale + target.scale)), 2)) {
            float e = Calc.Angle(target.Position, Position);
            float t = Calc.Angle(Position, target.Position);
            AddSprite(Vector2.Lerp(Position + Vector2.UnitX.RotateTowards(t, 6.3f), target.Position + Vector2.UnitX.RotateTowards(e, 6.3f), 0.5f) - Position, (target.scale + scale) / 2f, Color.Lerp(target.color, color, 0.5f), isSeeded);
        }
    }
]]
local function getConnectionSprites(room, entity)

    local sprites = {}

    for _, target in ipairs(room.entities) do -- Gets entities from rooms entities
        if target ~= nil and target ~= entity and entity._name == target._name then
            local scale = (12 * entity.Scale + 12 * target.Scale)
            if entity.attachToSolid == target.attachToSolid and entity._id > target._id and utils.distanceSquared(entity.x, entity.y, target.x, target.y) < scale * scale then

                local sprite = getSprite(entity, true)

                sprite.depth = ((target.Depth + entity.Depth) / 2) + 2
                sprite:setPosition(vivUtil.lerp(entity.x, target.x, 0.5), vivUtil.lerp(entity.y, target.y, 0.5))
                local color = vivUtil.colorLerp(entity.Color, target.Color, 0.5)
                if entity.Type ~= "White" or target.Type ~= "White" then color = rainbowHelper.getRainbowHue(room,x,y,8*(entity.Scale + target.Scale),8*(entity.Scale + target.Scale)) end
                sprite.color = color
                table.insert(sprites, sprite)
                vivUtil.addAll(sprites, vivUtil.getBorder(sprite, vivUtil.colorLerp(entity.BorderColor, target.BorderColor, 0.5)))
            end
        end
    end

    return sprites
end

function customSpinner.sprite(room, entity)

    local mainSprite = getSprite(entity, false)
    local color = vivUtil.getColor(entity.Color)
    if entity.Type ~= "White" then color = rainbowHelper.getRainbowHue(room,entity.x,entity.y,16*entity.Scale,16*entity.Scale) end
    mainSprite.color = color
    mainSprite.depth = entity.Depth
    local sprites = getConnectionSprites(room, entity)
    table.insert(sprites, mainSprite)
    vivUtil.addAll(sprites, vivUtil.getBorder(mainSprite, entity.BorderColor))

    return sprites
end

function customSpinner.selection(room, entity)
    return utils.rectangle(entity.x - 8 * entity.Scale, entity.y - 8 * entity.Scale, 16 * entity.Scale, 16 * entity.Scale)
end

return customSpinner