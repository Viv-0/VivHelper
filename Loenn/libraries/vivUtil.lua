local utils = require("utils");
local drawableSpriteStruct = require("structs.drawable_sprite")
local drawableRectangleStruct = require("structs.drawable_rectangle")
local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableLineStruct = require("structs.drawable_line")

function vivUtil.copyTexture(baseTexture, x, y, relative)
    local texture = drawableSpriteStruct.fromMeta(baseTexture.meta, baseTexture)
    texture.x = relative and baseTexture.x + x or x
    texture.y = relative and baseTexture.y + y or y

    return texture
end