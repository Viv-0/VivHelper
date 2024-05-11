local uiUtils = require("ui.utils")
local utils = require("utils")

local iconHelper = {}

iconHelper.getIconArea(field, icon)
    -- Calculate custom rectangle, as the image one is too small

    local iconX, iconY, iconWidth, iconHeight = icon.screenX, icon.screenY, icon.width, icon.height
    local fromRightEdge = field.screenX + field.width - iconX - iconWidth
    local fromTop = iconY - field.screenY

    local rectangleX = iconX - fromRightEdge
    local rectangleY = iconY - fromTop
    local rectangleWidth = iconWidth + fromRightEdge * 2
    local rectangleHeight = iconHeight + fromTop * 2

    return rectangleX, rectangleY, rectangleWidth, rectangleHeight
end

iconHelper.hoveringDropdownArea(field, icon, x, y)
    local rectangleX, rectangleY, rectangleWidth, rectangleHeight = iconHelper.getIconArea(field, icon)

    return utils.aabbCheckInline(rectangleX, rectangleY, rectangleWidth, rectangleHeight, x, y, 1, 1)
end

return iconHelper