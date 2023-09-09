local utils = require("utils")
local vivUtil = require("mods").requireFromPlugin("libraries.vivUtil")
local rainbowHelper = require('mods').requireFromPlugin('libraries.rainbowHelper')
local drawableSprite = require('structs.drawable_sprite')
local drawableRect = require('structs.drawable_rectangle')
local modHandler = require('mods')

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

local function getSprite(entity, bg)
    local s = ""
    if entity.ref then 
        s = entity.ref
        if bg then
            s = s:reverse()
            s = s:gsub("gf/","gb/",1)
            s = s:reverse()
        end -- get last occurrence of /fg and replace with /bg if background sprite
    else
        local s2 = entity.Subdirectory
        if entity.FrostHelper == true then 
            s2 = "" 
        elseif vivUtil.isNullEmptyOrWhitespace(s2) then
            if entity.CurrentVersion then s2 = "" else s2 = "_white" end
        else s2 = "_" .. s2 end
        local s3 = "/fg"
        if bg then s3 = "/bg" end
        s = entity.Directory ..  s3 .. s2
    end
    local ret = drawableSprite.fromTexture(s .. "00", entity)
    if ret then return ret end
    ret = drawableSprite.fromTexture(s, entity) -- apparently getSubtexture does this.
    if ret then return ret end
    return drawableSprite.fromTexture(modHandler.internalModContent .. "/missing_image", entity)
end
--[[ Hitbox drawing is disabled in Loenn because it puts too much load on render pipeline
local function drawHitboxes(entity)

    local function parse(a) return  end

    local function parseInt(s,f)
        if vivUtil.isNullEmptyOrWhitespace(s) then return 0 end
        local q = s:find('@')
        if q then
            local t = s:sub(q)
            return parseInt(s:sub(1,q), f) + parseInt(s:sub(q+1), f)
        elseif s[1] == '*' then
            return math.floor(tonumber(s:sub(s)))
        else
            return math.round(math.floor(tonumber(s)) * f)
        end    
    end
    local ret = {}
    local scale = entity.Scale or 1
    local color = (entity.AttachToSolid and {0.2647, 0.3539, 0.4608, 0.5} or {1.0,0.0,0.0,0.5})
    local ht = (vivUtil.isNullEmptyOrWhitespace(entity.HitboxType) and (not not entity.removeRectHitbox and "C:6" or "C:6;0,0|R:16,4;-8,*1@-4") or entity.HitboxType)
    for a in vivUtil.split(ht, "|") do
        local r = vivUtil.splitToTable(a, "[;:]")
        local k
        if r[1] == "C" then
            local d = 0
            local e = 0
            local f = 0
            d = parseInt(r[2], scale)
            if #r > 2 then
                k = vivUtil.splitToTable(r[3], ',')
                e = parseInt(k[1], scale)
                f = parseInt(k[2], scale)
            end
            table.insert(ret, vivUtil.drawableCircle("line", entity.x + e, entity.y + f, d, color))
        elseif r[1] == "R" then
            local w = 0
            local h = 0
            local ox = 0
            local oy = 0
            k = vivUtil.splitToTable(r[2], ',')
            w = parseInt(k[1], scale)
            h = parseInt(k[2], scale)
            if #r > 2 then
                k = vivUtil.splitToTable(r[3], ',')
                ox = parseInt(k[1], scale)
                oy = parseInt(k[2], scale)
            end
            table.insert(ret, drawableRect.fromRectangle("line",entity.x + ox,entity.y + oy,w,h,color))
        end
    end
    return ret
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
    local borders = {}
    for _, target in ipairs(room.entities) do -- Gets entities from rooms entities
        if target ~= nil and target ~= entity and entity._name == target._name then
            local scale = (12 * entity.Scale + 12 * target.Scale)
            if entity.attachToSolid == target.attachToSolid and entity._id > target._id and utils.distanceSquared(entity.x, entity.y, target.x, target.y) < scale * scale then

                local sprite = getSprite(entity, true)
                sprite:setPosition(((target.x + entity.x) / 2), ((target.y + entity.y) / 2))
                local color = vivUtil.colorLerp(entity.Color, target.Color, 0.5)
                if entity.Type ~= "White" or target.Type ~= "White" then color = rainbowHelper.getRainbowHue(room,entity.x,entity.y,8*(entity.Scale + target.Scale),8*(entity.Scale + target.Scale)) end
                sprite.color = color
                local scale = (entity.Scale + target.Scale) / 2
                sprite:setScale(scale,scale)
                vivUtil.addAll(borders, vivUtil.getBorder(sprite, vivUtil.colorLerp(entity.BorderColor, target.BorderColor, 0.5)))
                table.insert(sprites, sprite)
            end
        end
    end
    return sprites, borders
end

function customSpinner.sprite(room, entity)
    local mainSprite = getSprite(entity, false)
    local c = entity.Color
    if vivUtil.isNullEmptyOrWhitespace(c) then c = "FFFFFF" end
    local color = vivUtil.getColorTable(c)
    if entity.Type ~= "White" then color = rainbowHelper.getRainbowHue(room,entity.x,entity.y,16*entity.Scale,16*entity.Scale) end
    mainSprite.color = color
    mainSprite.depth = entity.Depth
    mainSprite:setScale(entity.Scale, entity.Scale)
    local cS, sprites = getConnectionSprites(room, entity)
    vivUtil.addAll(sprites, vivUtil.getBorder(mainSprite, entity.BorderColor))
    vivUtil.addAll(sprites, cS)
    table.insert(sprites, mainSprite)
    return sprites
end

function customSpinner.selection(room, entity)
    return utils.rectangle(entity.x - 8 * entity.Scale, entity.y - 8 * entity.Scale, 16 * entity.Scale, 16 * entity.Scale)
end

return customSpinner