local utils = require("utils");
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableLine = require("structs.drawable_line")
local drawableFunction = require('structs.drawable_function')
local state = require("loaded_state")

local vivUtil = {}

function vivUtil.addAll(addTo, toAddTable, insertLoc)
    if insertLoc then
        for _, value in ipairs(toAddTable) do
            table.insert(addTo, insertLoc, value)
        end
    else
        for _, value in ipairs(toAddTable) do
            table.insert(addTo, value)
        end
    end


    return addTo
end
-- Simple functions
function vivUtil.signedModulo(base,modulus) 
    return math.sign(base * modulus) * math.abs(base) % math.abs(modulus)
end

function vivUtil.mod(base,modulus) return (base % modulus + modulus) % modulus end

function vivUtil.copyTexture(baseTexture, x, y, relative)
    local texture = drawableSprite.fromMeta(baseTexture.meta, baseTexture)
    texture.x = relative and baseTexture.x + x or x
    texture.y = relative and baseTexture.y + y or y

    return texture
end

function vivUtil.contains(table, element)
    for _, value in pairs(table) do
        if value == element then
            return true
        end
    end
    return false
end

function vivUtil.trim(s) return s:match'^()%s*$' and '' or s:match'^%s*(.*%S)' end

function vivUtil.parseColor(color, _format)
    local format = _format or "argb"
    local s,r,g,b,a = nil
    if #color == 6 or format == "rgb" or format == "rgba" then
        return utils.parseHexColor(color)
    else
        return utils.parseHexColor(string.sub(color, 3)..string.sub(color,1,2))
    end
end

-- returns color in rgba format or nil
function vivUtil.getColor(color, format, xnaAllowed)
    xnaAllowed = xnaAllowed or true
    local colorType = type(color)

    if colorType == "string" then
        if xnaAllowed then
            -- Check XNA colors, otherwise parse as hex color
            local xnaColor = utils.getXNAColor(color)
            if xnaColor then
                return xnaColor
            end
        end
        local success, r, g, b, a = vivUtil.parseColor(color, format)

        if success then
            return {r,g,b,a}
        end

        return {"nil"}
    elseif colorType == "table" and (#color == 3 or #color == 4) then
        if format == "hsv" or format == "hsva" then 
            local r,g,b = utils.hsvToRgb(color[1], color[2],color[3])
            return {r*(color[4] or 1), g*(color[4] or 1), b*(color[4] or 1), (color[4] or 1)}
        else return color end
    end
end

function vivUtil.argbToHex(r,g,b,a)
    local r8 = math.floor(r * 255 + 0.5)
    local g8 = math.floor(g * 255 + 0.5)
    local b8 = math.floor(b * 255 + 0.5)
    local a8 = math.floor((a or 1) * 255 + 0.5)
    local number = a8 * 256^3 + r8 * 256^2 + g8 * 256 + b8

    return string.format("%08x", number)
end

function vivUtil.lerp(a,b,t) return a * (1-t) + b * t end

function vivUtil.colorLerp(a,b,lerp,aData,bData)
    local c1, c2 = vivUtil.getColor(a,(not not aData and aData.format or "argb"), (not not aData and aData.xnaAllowed or false)),vivUtil.getColor(b,(not not bData and bData.format or "argb"), (not not bData and bData.xnaAllowed or false))
    return {vivUtil.lerp(c1[1],c2[1],lerp), vivUtil.lerp(c1[2],c2[2],lerp), vivUtil.lerp(c1[3],c2[3],lerp), vivUtil.lerp(c1[4] or 1,c2[4] or 1,lerp)}
end
function vivUtil.alphMult(color, alpha)
    color[1] *= alpha
    color[2] *= alpha
    color[3] *= alpha
    if #color == 3 then table.insert(color, alpha) else color[4] *= alpha end
    return color
end

function vivUtil.indexof(keyvaluetable, toFind)
    for key, val in pairs(keyvaluetable) do
        if val == toFind then
            return key
        end
    end
end

---Returns a handler function to implement entity.rotate(room, entity, direction), where the entity's _name field will get changed according to the rotations table
function vivUtil.getNameRotationHandler(rotations)
    return function (room, entity, direction)
        local startIndex = indexof(rotations, entity._name)

        local realIndex = vivUtil.mod(startIndex + direction, 4)

        entity._name = rotations[realIndex]

        return true
    end
end

function vivUtil.getDataRotationHandler(dataName, rotations)
    return function (room, entity, direction)
        local startIndex = indexof(rotations, entity[dataName])

        local realIndex = vivUtil.mod(startIndex + direction, 4)

        entity[dataName] = rotations[realIndex]

        return true
    end
end

---Returns a handler function to implement entity.flip(room, entity, horizontal, vertical), where the entity's _name field will get changed according to the rotations table
function vivUtil.getNameFlipHandler(rotations)
    return function (room, entity, horizontal, vertical)
        local startIndex = indexof(rotations, entity._name)

        if vertical then
            if startIndex == 0 or startIndex == 2 then
                entity._name = rotations[(startIndex + 2) % 4]
                return true
            end
        else
            if startIndex == 1 or startIndex == 3 then
                entity._name = rotations[(startIndex + 2) % 4]
                return true
            end
        end

        return false
    end
end

function vivUtil.getBorder(sprite, color)
    local function get(xOffset,yOffset)
        local texture = drawableSprite.fromMeta(sprite.meta, sprite)
        texture.x += xOffset
        texture.y += yOffset
        if sprite.depth then texture.depth = sprite.depth + 1 else texture.depth = -8499 end -- fix preview depth
        texture.color = color and vivUtil.getColor(color) or {0, 0, 0, 1}
        return texture
    end

    return {
        get(0, -1),
        get(0, 1),
        get(-1, 0),
        get(1, 0)
    }
end

return vivUtil