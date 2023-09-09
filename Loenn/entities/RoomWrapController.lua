local drawableRectangle = require('structs.drawable_rectangle')
local drawableSprite = require('structs.drawable_sprite')
local utils = require('utils')
return {
    name = "VivHelper/RoomWrapController",
    fieldOrder = {
        "Top","Right","Bottom","Left",
        "TopOffset","TopExitSpeedAdd",
        "RightOffset","RightExitSpeedAdd",
        "BottomOffset","BottomExitSpeedAdd",
        "LeftOffset","LeftExitSpeedAdd",
        "setByCamera","allEntities","legacy",
        "ZFlagsData",
    },
    depth = -math.huge + 10,
    placements = {{
        name = "v",
        data = {
            Top = true, Right = false, Bottom = true, Left = false,
            TopOffset = 4.0, TopExitSpeedAdd = 15.0,
            RightOffset = 4.0, RightExitSpeedAdd = 15.0,
            BottomOffset = 4.0, BottomExitSpeedAdd = 15.0,
            LeftOffset = 4.0, LeftExitSpeedAdd = 15.0,
            setByCamera = false, allEntities = false, legacy = true,
            ZFlagsData = ""
        }},{
        name = "h",
        data = {
            Top = false, Right = true, Bottom = false, Left = true,
            TopOffset = 4.0, TopExitSpeedAdd = 15.0,
            RightOffset = 4.0, RightExitSpeedAdd = 15.0,
            BottomOffset = 4.0, BottomExitSpeedAdd = 15.0,
            LeftOffset = 4.0, LeftExitSpeedAdd = 15.0,
            setByCamera = false, allEntities = false, legacy = true,
            ZFlagsData = ""
        }},
        name = "b",
        data = {
            Top = true, Right = true, Bottom = true, Left = true,
            TopOffset = 4.0, TopExitSpeedAdd = 15.0,
            RightOffset = 4.0, RightExitSpeedAdd = 15.0,
            BottomOffset = 4.0, BottomExitSpeedAdd = 15.0,
            LeftOffset = 4.0, LeftExitSpeedAdd = 15.0,
            setByCamera = false, allEntities = false, legacy = true,
            ZFlagsData = ""
        }
    },
    selection = function(room,entity) return utils.rectangle(entity.x - 16, entity.y - 16, 32, 32) end,
    sprite = function(room, entity)
        local sprites = {drawableSprite.fromTexture("ahorn/VivHelper/wrap", entity)}
        local W = room.width
        local H = room.height
        local l = entity.LeftOffset
        local r = entity.RightOffset
        local t = entity.TopOffset
        local b = entity.BottomOffset
        local c = {0.67,0.7,0.25,0.75}
        if entity.Left then
            table.insert(sprites, drawableRectangle.fromRectangle("fill",0,t,l+1,H-t-b,c))
        end
        if entity.Top then
            table.insert(sprites, drawableRectangle.fromRectangle("fill",l,0,W-l-r,t+1,c))
        end
        if entity.Right then
            table.insert(sprites, drawableRectangle.fromRectangle("fill",W-r-1,t,r+1,H-t-b,c))
        end
        if entity.Bottom then
            table.insert(sprites, drawableRectangle.fromRectangle("fill",l,H-b-1,W-l-r,b+1,c))
        end
        return sprites
    end
}