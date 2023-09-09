local utils = require("utils");
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableLine = require("structs.drawable_line")
local drawableFunction = require('structs.drawable_function')
local state = require("loaded_state")

local vivUtil = {}
local ___p = 2^32

function vivUtil.rshift(x, s_amount)
	if math.abs(s_amount) >= 32 then return 0 end
	x = x % ___p
	if s_amount > 0 then
		return math.floor(x * (2 ^ - s_amount))
	else
		return (x * (2 ^ -s_amount)) % ___p
	end
end

function vivUtil.addAll(receiver, sender, insertLoc)
    if insertLoc then
        for _, value in ipairs(sender) do
            table.insert(receiver, insertLoc, value)
        end
    else
        for _, value in ipairs(sender) do
            table.insert(receiver, value)
        end
    end


    return receiver
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

function vivUtil.trim(s) return ((string.match(s,'^()%s*$')) and '' or string.match(s,'^%s*(.*%S)')) end

function vivUtil.isNullEmptyOrWhitespace(s)
    if s then
        return #vivUtil.trim(s) == 0
    end return true
end

-- Format for VivHelper is "abgr" (format follows Color.PackedValue)
function vivUtil.getColor(color, allowXNAColors)
    if type(color) == "nil" then 
        return false
    elseif type(color) == "number" then
        return true, (color % 0x100), (vivUtil.rshift(color, 8) % 0x100), (vivUtil.rshift(color,16) % 0x100),(vivUtil.rshift(color,24) % 0x100)
    elseif type(color) == "table"  and (#color == 3 or #color == 4) then -- [r,g,b,a]
        return true, color[1],color[2],color[3],color[4] or 1
    elseif type(color) == "string" then -- either rgba or XNAColor
        local xnaColor = utils.getXNAColor(color)
        if allowXNAColors and xnaColor then
            return true, xnaColor[1], xnaColor[2], xnaColor[3], xnaColor[4] or 1
        end
        if #color < 5 then return false 
        elseif #color == 6 then return utils.parseHexColor(color) -- rrggbb
        elseif #color == 7 then return utils.parseHexColor(color:sub(1,6)) -- rrggbb, ignore 7th digit
        elseif #color == 8 then
            if rgba then return utils.parseHexColor(color)
            else return utils.parseHexColor(color:sub(7,8) .. color:sub(5,6) .. color:sub(3,4) .. color:sub(1,2)) -- aabbggrr
            end
        else return false
        end
    end
    return false
end

function vivUtil.getColorTable(color, allowXNAColors, defaultTable)
    defaultTable = defaultTable or {0,0,0,1}
    local parsed, r, g, b, a = vivUtil.getColor(color, allowXNAColors)
    if parsed then
        return {r,g,b,a or 1}
    else return defaultTable end
end

function vivUtil.invertGetColor(r,g,b,a) -- returns a string in abgr format, such that getColor(invertGetColor(r,g,b,a)) == r,g,b,a is true
    if type(r) == "nil" then return "" -- returns an empty string if r is blank/nil
    elseif  type(g) == "nil" and type(b) == "nil" and type(a) == "nil" then -- if invertGetColor has 1 parameter
        if type(r) == "number" then return string.format("%08x", math.floor(r)) -- yields a number in aabbggrr format
        elseif type(r) == "string" then return r:sub(7,8) .. r:sub(5,6) .. r:sub(3,4) .. r:sub(1,2) -- yields aabbggrr
        end
    else
        local _r = (type(r) == "table" and r[1] or r)
        local _g = g or r[2]; local _b = b or r[3]; local _a = (a or r[3] or 1);
        local r8 = math.floor(_r * 255 + 0.5)
        local g8 = math.floor(_g * 255 + 0.5)
        local b8 = math.floor(_b * 255 + 0.5)
        local a8 = math.floor(_a * 255 + 0.5)
        local number = a8 * 256^3 + b8 * 256^2 + g8 * 256 + r8
        return string.format("%08x", number) -- yields a strings in abgr format
    end
end

function vivUtil.swapRGBA(str) -- swaps rgba to abgr and vice versa
    if #str >= 8 then return str:sub(7,8) .. str:sub(5,6) .. str:sub(3,4) .. str:sub(1,2) else return str end
end

function vivUtil.lerp(a,b,t) return a * (1-t) + b * t end

function vivUtil.colorLerp(a,b,lerp)
    local A, ar, ag, ab, aa = vivUtil.getColor(a, true)
    local B, br, bg, bb, ba = vivUtil.getColor(b, true)
    return {vivUtil.lerp(ar, br,lerp), vivUtil.lerp(ag,bg,lerp), vivUtil.lerp(ab,bb,lerp), vivUtil.lerp(aa or 1,ba or 1,lerp)}
end
function vivUtil.alphMult(color, alpha)
    local newColor = {}
    newColor[1] = color[1] * alpha
    newColor[2] = color[2] * alpha
    newColor[3] = color[3] * alpha
    newColor[4] = (color[4] or 1) * alpha
    return newColor
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
        local startIndex = vivUtil.indexof(rotations, entity._name)

        local realIndex = vivUtil.mod(startIndex + direction, 4) + 1

        entity._name = rotations[realIndex]

        return true
    end
end

function vivUtil.getDataRotationHandler(dataName, rotations)
    return function (room, entity, direction)
        local startIndex = vivUtil.indexof(rotations, entity[dataName])
        local realIndex = vivUtil.mod(startIndex - 1 + direction, 4) + 1
        entity[dataName] = rotations[realIndex]

        return true
    end
end

---Returns a handler function to implement entity.flip(room, entity, horizontal, vertical), where the entity's _name field will get changed according to the rotations table
function vivUtil.getNameFlipHandler(rotations)
    return function (room, entity, horizontal, vertical)
        local startIndex = vivUtil.indexof(rotations, entity._name)

        if vertical then
            if startIndex == 0 or startIndex == 2 then
                entity._name = rotations[vivUtil.mod((startIndex + 2), 4) + 1]
                return true
            end
        else
            if startIndex == 1 or startIndex == 3 then
                entity._name = rotations[vivUtil.mod((startIndex + 2), 4) + 1]
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
        if sprite.depth then texture.depth = sprite.depth + 1 end
        texture.color = color and vivUtil.getColorTable(color) or {0, 0, 0, 1}
        return texture
    end

    return {
        get(0, -1),
        get(0, 1),
        get(-1, 0),
        get(1, 0)
    }
end

function vivUtil.angle2vec(angle, length, _x, _y)
    return {x=(_x or 0) + math.cos(angle)*length, y=(_y or 0) + math.sin(angle)*length}
end

--- Returns a functional splitter 
function vivUtil.split(str, pat)
    local st, g = 1, str:gmatch("()("..pat..")")
    local function getter(str, segs, seps, sep, cap1, ...)
        st = sep and seps + #sep
        return str:sub(segs, (seps or 0) - 1), cap1 or sep, ...
    end
    local function splitter(self)
        if st then return getter(str, st, g()) end
    end
    return splitter, self
end

function vivUtil.splitToTable(str, pat) 
    local ret = {}
    for d in vivUtil.split(str, pat) do table.insert(ret, d) end
    return ret
end

function vivUtil.containsPredicate(predicate, data)
    for _, dataValue in ipairs(data) do
        if predicate(dataValue) then
            return true
        end
    end

    return false
end
function vivUtil.firstPredicate(predicate, data)
    for _, dataValue in ipairs(data) do
        if predicate(dataValue) then
            return dataValue
        end
    end

    return nil
end
--- returns a given sprite with a number at the end of the string
--- getImageWithNumbers("objects/refill/idle", 00, data) => sprite from the image objects/refill/idle00
function vivUtil.getImageWithNumbers(string, idx, data, _atlas) 
    local atlas = _atlas or data.atlas or "Gameplay"
    local val = nil
    for i = 1,6,1 do 
        val = require('atlases').getResource(string .. string.format("%0"..tostring(i).."d", idx), atlas)
        if val then break end
    end
    if val then return drawableSprite.fromMeta(val, data) else return drawableSprite.fromMeta(require('atlases').getResource(require('mods').internalModContent .. "/missing_image"), data) end
end

function vivUtil.GetFilePathWithNoTrailingNumbers(AllowEmpty, atlasName)
    local atlas = altasName or "Gameplay"
    return {
        fieldType = "path",
        allowEmpty = not not AllowEmpty,
        allowFiles = true,
        allowFolders = false,
        filenameProcessor = function(filename, rawFilename, prefix)
            local str = vivUtil.trim((not filename and "" or filename))
            local a = false
            local offset = 19 + #atlas
            for i = #str, 1, -1 do
                local b = str:byte(i,i)
                if a then -- check for trailing numbers
                    if b > 57 or b < 48 then
                        return str:sub(offset, i)
                    end
                else
                    a = b == 46 -- if the character is a period, check for trailing numbers
                end 
            end
            return str:sub(offset)
        end
    }
end

function vivUtil.getDirectoryPathFromFile(AllowEmpty, atlasName)
    local atlas = altasName or "Gameplay"
    return {
        fieldType = "path",
        allowEmpty = not not AllowEmpty,
        allowFiles = true,
        allowFolders = false,
        filenameProcessor = function(filename, rawFilename, prefix)
            local str = vivUtil.trim((not filename and "" or filename))
            local offset = 19 + #atlas -- | refers to substring start in "Graphics/Atlases/<atlasName>/|restofstring"
            for i = #str, 1, -1 do
                if str:byte(i,i) == 47 then -- return the substring up to but not including the last `/` so you grab the folder reference.
                    return str:sub(offset, i-1)
                end
            end
            return str:sub(offset)
        end
    }
end

function vivUtil.printJustifyText(text, x, y, width, height, font, fontSize, trim, align)
    font = font or love.graphics.getFont()
    fontSize = fontSize or 1

    if trim ~= false then
        text = utils.trim(text)
    end

    local fontHeight = font:getHeight()
    local fontLineHeight = font:getLineHeight()
    local longest, lines = font:getWrap(text, width / fontSize)
    local textHeight = (#lines - 1) * (fontHeight * fontLineHeight) + fontHeight

    local offsetX = 1
    local offsetY = math.floor((height - textHeight * fontSize) / 2) + 1

    love.graphics.push()

    love.graphics.translate(x + offsetX, y + offsetY)
    love.graphics.scale(fontSize, fontSize)

    love.graphics.printf(text, 0, 0, width / fontSize, align or "center")

    love.graphics.pop()
end

return vivUtil