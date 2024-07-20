local vivUtil = require("mods").requireFromPlugin("libraries.vivUtil")
local drawableLineStruct = require("structs.drawable_line")
local drawableFunction = require("structs.drawable_function")
local utils = require("utils")
local drawing = require("utils.drawing")
local drawableRectangleStruct = require('structs.drawable_rectangle')

local helper = {}

local function point(x, y, color)
    return drawableRectangleStruct.fromRectangle("fill", x - 1, y - 1, 2, 2, color or {1.0,1.0,1.0,1.0}):getDrawableSprite()
end

local function drawFilledPolygon(pt, fillColor)
    drawing.callKeepOriginalColor(function()
        love.graphics.setColor(fillColor)
        local ok, triangles = pcall(love.math.triangulate, pt)
        if not ok then return end
        for _, triangle in ipairs(triangles) do
            love.graphics.polygon("fill", triangle)
        end
    end)
end

function helper.getSpriteFunc(filled, nodeColor, lineColor, fillColor, positionDrawable)
    return function(room, entity)
        if entity.nodes then
            local nC = vivUtil.GetColorTable(entity, nil, "nodeColor", true, nodeColor or {1,0,0,1})
            local lC = vivUtil.GetColorTable(entity, nil, "lineColor", true, lineColor or {0,0,0,1})
            local fC = vivUtil.GetColorTable(entity, nil, "fillColor", true, fillColor or {0.8, 0.4, 0.4, 0.8})
            local points = {}
            local nodeSprites = {}
            if not positionDrawable then
                points = { entity.x, entity.y } 
                nodeSprites = { point(entity.x, entity.y, nC)}
            end
            for _, value in ipairs(entity.nodes) do
                table.insert(points, value.x)
                table.insert(points, value.y)

                table.insert(nodeSprites, point(value.x, value.y, nC))
            end
            if filled then
                table.insert(points, points[1]) -- always "wraps" back
                table.insert(points, points[2])
            end
            
            local ret = {}
            if filled then table.insert(ret, drawableFunction.fromFunction(drawFilledPolygon, points, fC)) end
            table.insert(ret, drawableLineStruct.fromPoints(points, lC, 1))
            table.insert(ret, nodeSprites)
            if positionDrawable then table.insert(ret, positionDrawable) end                
            return ret
        end
        return {}
    end
end

-- make sure nodes aren't drawn because it looks stupid
function helper.nodeSprite() end

function helper.selection(room, entity)
    local main = utils.rectangle(entity.x-2, entity.y-2, 4, 4)

    if entity.nodes then
        local nodeSelections = {}
        for _, node in ipairs(entity.nodes) do
            table.insert(nodeSelections, utils.rectangle(node.x-2, node.y-2, 4, 4))
        end
        return main, nodeSelections
    end

    return main, { }
end

function helper.nodeAdded(room, entity, node)
    -- place node at mouse position
    local mx, my = love.mouse.getPosition()
    local nodeX, nodeY = viewportHandler.getRoomCoordinates(room, mx, my)
    local nodes = entity.nodes
    if node == 0 then
        table.insert(nodes, 1, {x = nodeX, y = nodeY})
    else
        table.insert(nodes, node + 1, {x = nodeX, y = nodeY})
    end
    return true
end

return helper