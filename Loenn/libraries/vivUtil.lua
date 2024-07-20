local utils = require("utils");
local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableLine = require("structs.drawable_line")
local drawableFunction = require('structs.drawable_function')
local state = require("loaded_state")
local mods = require('mods')
local enum_colors = require('consts.xna_colors')
local atlases = require('atlases')

local vivUtil = {}
local ___p = 2^32

vivUtil.missingImageMeta = require('atlases').getResource(mods.internalModContent .. "/missing_image")


vivUtil.areaHSVShader = love.graphics.newShader[[
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
vivUtil.alphaShader = love.graphics.newShader[[
    vec4 effect(vec4 color, Image tex, vec2 tc, vec2 sc)
    {
        return mix(vec4(Texel(tex, tc).rgb, 1), color, tc[0]);
    }
]]

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

function vivUtil.GetColorKey(entity, keyV1, keyV2)
    if keyV2 and entity[keyV2] then
        return entity[keyV2]
    elseif keyV1 and entity[keyV1] then
        return entity[keyV1]
    end
    return ""
end

function vivUtil.GetColor(entity, keyV1, keyV2, allowXNAColors, defaultTable) 
    local parsed = false ; local r = 1 ; local g = 1 ; local b = 1 ; local a = 1
    if keyV2 and entity[keyV2] then
        parsed, r, g, b, a = vivUtil.newGetColor(entity[keyV2], allowXNAColors)
    elseif keyV1 and entity[keyV1] then
        parsed, r, g, b, a = vivUtil.oldGetColor(entity[keyV1], allowXNAColors)
    end
    if parsed then
        return true, r, g, b, a
    else 
        return false, defaultTable[1] or 1, defaultTable[2] or 1, defaultTable[3] or 1, defaultTable[4] or 1
    end
end
function vivUtil.GetColorTable(entity, keyV1, keyV2, allowXNAColors, defaultTable) 
    local parsed = false ; local r = 1; local g = 1; local b = 1; local a = 1
    if keyV2 and entity[keyV2] then
        parsed, r, g, b, a = vivUtil.newGetColor(entity[keyV2], allowXNAColors)
    elseif keyV1 and entity[keyV1] then
        parsed, r, g, b, a = vivUtil.oldGetColor(entity[keyV1], allowXNAColors)
    end
    if parsed then
        return {r or 1, g or 1, b or 1, a or 1}
    else 
        return {defaultTable[1] or 1, defaultTable[2] or 1, defaultTable[3] or 1, defaultTable[4] or 1}
    end
end

function vivUtil.newGetColorTable(color, allowXNAColors, defaultTable)
    defaultTable = defaultTable or {0,0,0,1}
    local parsed, r, g, b, a = vivUtil.newGetColor(color, allowXNAColors)
    if parsed then
        return {r,g,b,a or 1}
    else return defaultTable end
end

function vivUtil.newGetColor(color, allowXNAColors) -- we dont actually need "allowEmpty" here
    -- removed functionality for number
    if type(color) == "nil" or type(color) == "number" then
        return false
    elseif type(color) == "table" then
        return true, color[1],color[2],color[3],color[4] or 1
    elseif type(color) == "string" then -- either rgba or XNAColor
        local xnaColor = utils.getXNAColor(color)
        if allowXNAColors and xnaColor then
            return true, xnaColor[1], xnaColor[2], xnaColor[3], xnaColor[4] or 1
        end
        color2 = color:match("^#?%x+$") -- checks for hex color with optional # at the start
        if color2 ~= nil then
            color3 = color2:gsub("#", "")
            if #color == 3 then 
                local color4 = color3[1] .. color3[1] .. color3[2] .. color3[2] .. color3[3] .. color3[3]
                return utils.parseHexColor(color4)
            elseif #color == 6 or #color == 8 then
                return utils.parseHexColor(color3)
            else return false
            end
        end
    end
    return false
end
-- Format for old VivHelper is "abgr" (format follows Color.PackedValue)
function vivUtil.oldGetColor(color, allowXNAColors)
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

function vivUtil.oldGetColorTable(color, allowXNAColors, defaultTable)
    defaultTable = defaultTable or {0,0,0,1}
    local parsed, r, g, b, a = vivUtil.oldGetColor(color, allowXNAColors)
    if parsed then
        return {r,g,b,a or 1}
    else return defaultTable end
end

function vivUtil.oldInvertGetColor(r,g,b,a) -- returns a string in abgr format, such that getColor(oldInvertGetColor(r,g,b,a)) := r,g,b,a
    if type(r) == "nil" then return "" -- returns an empty string if r is blank/nil
    elseif  type(g) == "nil" and type(b) == "nil" and type(a) == "nil" then -- if oldInvertGetColor has 1 parameter
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
    if #str == 8 and not enum_colors[str] then return str:sub(7,8) .. str:sub(5,6) .. str:sub(3,4) .. str:sub(1,2) else return str end
end

function vivUtil.lerp(a,b,t) return a * (1-t) + b * t end

function vivUtil.colorLerp(a,b,lerp)
    aH, aS, aV = utils.rgbToHsv(a[1], a[2], a[3])
    bH, bS, bV = utils.rgbToHsv(b[1], b[2], b[3])

    rR, rG, rB = utils.hsvToRgb(vivUtil.lerp(aH, bH, lerp), vivUtil.lerp(aS, bS, lerp), vivUtil.lerp(aV, bV, lerp))
    return {rR, rG, rB, vivUtil.lerp(a[4] or 1, b[4] or 1, lerp)}
end
function vivUtil.alphMult(color, alpha)
    local newColor = {}
    newColor[1] = color[1] * alpha
    newColor[2] = color[2] * alpha
    newColor[3] = color[3] * alpha
    newColor[4] = (color[4] or 1) * alpha
    return newColor
end

function vivUtil.tableLerp(table1, table2, lerp) 
    local ret = {}
    for i=1,math.min(#table1, #table2) do
        table.insert(ret, table1[i] * (1-lerp) + table2[i] * lerp)
    end
    return ret
end

function vivUtil.indexof(keyvaluetable, toFind)
    for key, val in pairs(keyvaluetable) do
        if val == toFind then
            return key
        end
    end
end

local bloomMeta = atlases.getResource("util/bloomsprite", "Gameplay")
function vivUtil.bloomSprite(entity, radius, alpha)
    local sprite = drawableSprite.fromMeta(bloomMeta, entity)
    sprite:setColor({alpha,alpha,alpha,alpha})
    local scale = radius * 0.0078125 -- this is always the same value, radius * 2 / 256 = radius / 128 = radius * 0.0078125
    sprite:setScale(scale, scale)
    return sprite
end

function vivUtil.drawableCircle(mode, x, y, radius, color, strokeWidth, segments)
    local segs = segments or 16
    local sWidth = strokeWidth or 2
    return drawableFunction.fromFunction(function()
        local pr, pg, pb, pa = love.graphics.getColor()
        local previousLineWidth = love.graphics.getLineWidth()

        love.graphics.setLineWidth(sWidth)
        love.graphics.setColor(color[1], color[2], color[3], color[4] or 0.5)
        love.graphics.circle(mode, x, y, radius + 1 - math.ceil(sWidth / 2), segments)
        love.graphics.setLineWidth(previousLineWidth)
        love.graphics.setColor(pr, pg, pb, pa)
    end)
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
        texture.color = color or {0,0,0,1}
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
        val = atlases.getResource(string .. string.format("%0"..tostring(i).."d", idx), atlas)
        if val then break end
    end
    if val then return drawableSprite.fromMeta(val, data) else return drawableSprite.fromMeta(vivUtil.missingImageMeta, data) end
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

function vivUtil.sortByKeys(t) 
    local tkeys = {}
    -- populate the table that holds the keys
    for k in pairs(t) do table.insert(tkeys, k) end
    -- sort the keys
    table.sort(tkeys)
    return tkeys
end

function vivUtil.print_table(node, filename)
    local cache, stack, output = {},{},{}
    local depth = 1
    local output_str = "{\n"

    while true do
        local size = 0
        for k,v in pairs(node) do
            size = size + 1
        end

        local cur_index = 1
        for k,v in pairs(node) do
            if (cache[node] == nil) or (cur_index >= cache[node]) then

                if (string.find(output_str,"}",output_str:len())) then
                    output_str = output_str .. ",\n"
                elseif not (string.find(output_str,"\n",output_str:len())) then
                    output_str = output_str .. "\n"
                end

                -- This is necessary for working with HUGE tables otherwise we run out of memory using concat on huge strings
                table.insert(output,output_str)
                output_str = ""

                local key
                if (type(k) == "number" or type(k) == "boolean") then
                    key = "["..tostring(k).."]"
                else
                    key = "['"..tostring(k).."']"
                end

                if (type(v) == "number" or type(v) == "boolean") then
                    output_str = output_str .. string.rep('\t ',depth) .. key .. " = "..tostring(v)
                elseif (type(v) == "table") then
                    output_str = output_str .. string.rep('\t ',depth) .. key .. " = {\n"
                    table.insert(stack,node)
                    table.insert(stack,v)
                    cache[node] = cur_index+1
                    break
                else
                    output_str = output_str .. string.rep('\t',depth) .. key .. " = '"..tostring(v).."'"
                end

                if (cur_index == size) then
                    output_str = output_str .. "\n" .. string.rep('\t ',depth-1) .. "}"
                else
                    output_str = output_str .. ","
                end
            else
                -- close the table
                if (cur_index == size) then
                    output_str = output_str .. "\n" .. string.rep('\t ',depth-1) .. "}"
                end
            end

            cur_index = cur_index + 1
        end

        if (size == 0) then
            output_str = output_str .. "\n" .. string.rep('\t ',depth-1) .. "}"
        end

        if (#stack > 0) then
            node = stack[#stack]
            stack[#stack] = nil
            depth = cache[node] == nil and depth + 1 or depth - 1
        else
            break
        end
    end

    -- This is necessary for working with HUGE tables otherwise we run out of memory using concat on huge strings
    table.insert(output,output_str)
    output_str = table.concat(output)

    file = io.open(filename, "w")
    file:write(output_str)
    file:close()
    print("File written to " .. filename)
end

function vivUtil.isModLoaded(name)
    return mods.modMetadata[name] or mods.modMetadata[name .. "_zip"]
end

function vivUtil.countStringKeys(table)
    local ct,cn = utils.countKeys(table)
    return ct - cn
end

return vivUtil