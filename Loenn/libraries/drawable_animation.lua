local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")
local modHandler = require('mods')
local drawableSprite = require('structs.drawable_sprite')


local animation = {}
local animationMt = {}
animationMt.__index = {}

--[[ reference sheet
current -- current index of animation
meta -- the list of spriteMeta objects that contain context for the image and how to draw it
x,y -- position
xSet, ySet -- number arrays denoting if the entity should be rendered more than once. Significantly cleaner than having 20 animated sprites since we need to cache the spriteMetas per anim
jx,jy -- justification
sx,sy -- scale
ox,oy -- offset
]]



function animationMt.__index:setJustification(justificationX, justificationY)
    if type(justificationX) == "table" then
        justificationX, justificationY = justificationX[1], justificationX[2]
    end

    self.jx = justificationX
    self.jy = justificationY

    return self
end
function animationMt.__index:setPosition(x, y)
    if type(x) == "table" then
        x, y = x[1] or x.x, x[2] or x.y
    end

    self.x = x
    self.y = y

    return self
end

function animationMt.__index:addPosition(x, y)
    if type(x) == "table" then
        x, y = x[1] or x.x, x[2] or x.y
    end

    self.x += x
    self.y += y

    return self
end

function animationMt.__index:setScale(scaleX, scaleY)
    if type(scaleX) == "table" then
        scaleX, scaleY = scaleX[1], scaleX[2]
    end

    self.sx = scaleX
    self.sy = scaleY

    return self
end

function animationMt.__index:setOffset(offsetX, offsetY)
    if type(offsetX) == "table" then
        offsetX, offsetY = offsetX[1], offsetX[2]
    end

    self.ox = offsetX
    self.oy = offsetY

    return self
end

local function setColor(target, color)
    local tableColor = utils.getColor(color)

    if tableColor then
        target.color = tableColor
    end

    return tableColor ~= nil
end

function animationMt.__index:setColor(color)
    return setColor(self, color)
end

function animationMt.__index:setAlpha(alpha)
    local r, g, b = unpack(self.color or {})
    local newColor = {r or 1, g or 1, b or 1, alpha}

    return setColor(self, newColor)
end

-- TODO - Verify that scales are correct
function animationMt.__index:getRectangleRaw()
    local idx = math.floor((love.timer.getTime()/self.delta) % (#self.meta))
    local x = self.x
    local y = self.y

    local width = self.meta[idx].width
    local height = self.meta[idx].height

    local realWidth = self.meta[idx].realWidth
    local realHeight = self.meta[idx].realHeight

    local offsetX = self.ox or self.meta[idx].offsetX
    local offsetY = self.oy or self.meta[idx].offsetY

    local justificationX = self.jx
    local justificationY = self.jy

    local rotation = self.r

    local scaleX = self.sx
    local scaleY = self.sy

    local drawX = math.floor(x - (realWidth * justificationX + offsetX) * scaleX)
    local drawY = math.floor(y - (realHeight * justificationY + offsetY) * scaleY)

    drawX += (scaleX < 0 and width * scaleX or 0)
    drawY += (scaleY < 0 and height * scaleY or 0)

    local drawWidth = width * math.abs(scaleX)
    local drawHeight = height * math.abs(scaleY)

    if rotation and rotation ~= 0 then
        -- Shorthand for each corner
        -- Remove x and y before rotation, otherwise we rotate around the wrong origin
        local tlx, tly = drawX - x, drawY - y
        local trx, try = drawX - x + drawWidth, drawY - y
        local blx, bly = drawX - x, drawY - y + drawHeight
        local brx, bry = drawX - x + drawWidth, drawY - y + drawHeight

        -- Apply rotation
        tlx, tly = utils.rotate(tlx, tly, rotation)
        trx, try = utils.rotate(trx, try, rotation)
        blx, bly = utils.rotate(blx, bly, rotation)
        brx, bry = utils.rotate(brx, bry, rotation)

        -- Find the best point for top left and bottom right
        local bestTlx, bestTly = math.min(tlx, trx, blx, brx), math.min(tly, try, bly, bry)
        local bestBrx, bestBry = math.max(tlx, trx, blx, brx), math.max(tly, try, bly, bry)

        drawX, drawY = utils.round(x + bestTlx), utils.round(y + bestTly)
        drawWidth, drawHeight = utils.round(bestBrx - bestTlx), utils.round(bestBry - bestTly)
    end

    return drawX, drawY, drawWidth, drawHeight
end

function animationMt.__index:getRectangle()
    return utils.rectangle(self:getRectangleRaw())
end

function animationMt.__index:drawRectangle(mode, color)
    mode = mode or "fill"

    if color then
        drawing.callKeepOriginalColor(function()
            love.graphics.setColor(color)
            love.graphics.rectangle(mode, self:getRectangleRaw())
        end)

    else
        love.graphics.rectangle(mode, self:getRectangleRaw())
    end
end

function animationMt.__index:draw()
    local idx = math.floor((love.timer.getTime()/self.delta) % (#self.meta))
    local image = self.meta[idx + 1]
    local offsetX = self.ox or math.floor((self.jx or 0.0) * image.realWidth + image.offsetX)
    local offsetY = self.oy or math.floor((self.jy or 0.0) * image.realHeight + image.offsetY)

    local layer = image.layer

    local function func1() 
        if layer then
            if self.xSet then
                for _,a in ipairs(self.xSet) do love.graphics.drawLayer(image.image, layer, image.quad, a, self.y, self.r, self.sx, self.sy, offsetX, offsetY) end
            elseif self.ySet then
                for _,b in ipairs(self.ySet) do love.graphics.drawLayer(image.image, layer, image.quad, self.x, b, self.r, self.sx, self.sy, offsetX, offsetY) end
            else
                love.graphics.drawLayer(image.image, layer, image.quad, self.x, self.y, self.r, self.sx, self.sy, offsetX, offsetY)
            end 
        elseif self.xSet then
            for _,c in ipairs(self.xSet) do love.graphics.draw(image.image, image.quad, c, self.y, self.r, self.sx, self.sy, offsetX, offsetY) end
        elseif self.ySet then
            for _,d in ipairs(self.ySet) do love.graphics.draw(image.image, image.quad, self.x, d, self.r, self.sx, self.sy, offsetX, offsetY) end
        else
            love.graphics.draw(image.image, image.quad, self.x, self.y, self.r, self.sx, self.sy, offsetX, offsetY) 
        end
    end

    if self.color and type(self.color) == "table" then
        drawing.callKeepOriginalColor(function()
            love.graphics.setColor(self.color)
            func1()
        end)

    else
        func1()
    end
end

function animationMt.__index:getRelativeQuad(x, y, width, height, hideOverflow, realSize)
    local imageMeta = self.meta

    if imageMeta then
        local quadTable

        if type(x) == "table" then
            quadTable = x
            x, y, width, height = x[1], x[2], x[3], x[4]
            hideOverflow = y
            realSize = width

        else
            quadTable = {x, y, width, height}
        end

        if not imageMeta.quadCache then
            imageMeta.quadCache = {}
        end

        -- Get value with false as default, then change it to the quad
        -- Otherwise we are just creating the quad every single request
        local quadCache = imageMeta.quadCache
        local value = utils.getPath(quadCache, quadTable, false, true)

        if value then
            return unpack(value)

        else
            local quad, offsetX, offsetY = drawing.getRelativeQuad(imageMeta, x, y, width, height, hideOverflow, realSize)

            quadCache[x][y][width][height] = {quad, offsetX, offsetY}

            return quad, offsetX, offsetY
        end
    end
end

function animationMt.__index:useRelativeQuad(x, y, width, height, hideOverflow, realSize)
    local quad, offsetX, offsetY = self:getRelativeQuad(x, y, width, height, hideOverflow, realSize)

    self.quad = quad
    if self.oxSet then for _,o in self.oxSet do o = o + offsetX end else self.ox = (self.ox or 0) + offsetX end
    if self.oxSet then for _,p in self.oxSet do p = p + offsetY end else self.oy = (self.oy or 0) + offsetY end
end

function animation.fromSpriteMetaList(meta, data, frameDelta)
    data = data or {}

    local animation = {
        _type = "drawableFunction" -- this is what makes this object cacheable. Bruh moment
    }

    animation.x = data.x or 0
    animation.y = data.y or 0

    animation.jx = data.jx or data.justificationX or 0.5
    animation.jy = data.jy or data.justificationY or 0.5

    animation.sx = data.sx or data.scaleX or data.scale or data.Scale or 1
    animation.sy = data.sy or data.scaleY or data.scale or data.Scale or 1

    animation.r = data.rotation or data.rotation or 0

    animation.depth = data.depth

    animation.meta = meta
    animation.delta = frameDelta or data.frameDelta or 0.1

    if data.color then
        setColor(animation, data.color)
    end

    return setmetatable(animation, animationMt)
end

function animation.fromTextures(texture, frameDelta, data, suffixList)
    local atlas = data and data.atlas or "Gameplay"
    local spriteMeta = {}
    local missingSprite = atlases.getResource(modHandler.internalModContent .. "/missing_image", atlas)
    if suffixList then
        for l in suffixList do table.insert(spriteMeta,atlases.getResource(texture..l, atlas) or missingSprite) end
    else
        local idx = 0
        while true do
            for i = 1,6,1 do 
                val = atlases.getResource(texture .. string.format("%0"..tostring(i).."d", idx), atlas)
                if val then break end
            end
            if val then table.insert(spriteMeta,val) else break end
            idx = idx+1
        end
    end
    if #spriteMeta > 0 then
        return animation.fromSpriteMetaList(spriteMeta, data, frameDelta), true
    else return drawableSprite.fromMeta(missingSprite, data), false end
end

function animation.fromInternalTextures(texture, frameDelta, data, suffixList)
    return animation.fromTextures(atlases.addInternalPrefix(texture), frameDelta, data, suffixList)
end

return animation