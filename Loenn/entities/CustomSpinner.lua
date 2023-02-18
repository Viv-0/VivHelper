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
    Depth = {fieldType = "integer"},
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
    if type(entity.Depth) == "number" then return entity.Depth else return -8500 end
end

local function getSprite(entity, bg)
    local s = ""
    if entity.ref then 
        s = entity.ref .. "00"
        if bg then
            s = s:reverse()
            s = s:gsub("gf/","gb/",1)
            s = s:reverse()
        end -- get last occurrence of /fg and replace with /bg if background sprite
        return drawableSprite.fromTexture(s, entity)
    else
        local s2 = entity.Subdirectory
        if entity.FrostHelper == true then 
            s2 = "" 
        elseif s2 == nil or #vivUtil.trim(s2) == 0 then
            if entity.CurrentVersion then s2 = "" else s2 = "_white" end
        else s2 = "_" .. s2 end
        local s3 = "/fg"
        if bg then s3 = "/bg" end
        s3 = s3 .. s2 .. "00"
        return drawableSprite.fromTexture(entity.Directory .. s3, entity)
    end 
    
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
                sprite:setPosition((target.x + entity.x / 2), (target.y + entity.y / 2))
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
    local c = entity.Color
    if #vivUtil.trim(c) == 0 then c = "FFFFFFFF" end
    local color = vivUtil.getColor(c)
    if entity.Type ~= "White" then color = rainbowHelper.getRainbowHue(room,entity.x,entity.y,16*entity.Scale,16*entity.Scale) end
    mainSprite.color = color
    local sprites = getConnectionSprites(room, entity)
    table.insert(sprites, mainSprite)
    vivUtil.addAll(sprites, vivUtil.getBorder(mainSprite, entity.BorderColor))
    return sprites
end

function customSpinner.selection(room, entity)
    return utils.rectangle(entity.x - 8 * entity.Scale, entity.y - 8 * entity.Scale, 16 * entity.Scale, 16 * entity.Scale)
end

return customSpinner