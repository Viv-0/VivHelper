local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local ccb = {name = "VivHelper/CustomCoreBlock", 
fieldOrder = {"x","y",
              "width","height",
              "CoreState","RespawnTime",
              "WindUpDist","IceWindUpDist",
              "BounceDist","LiftSpeedXMult",
              "WallPushTime","BounceEndTime",
              "Directory", "HotParticleColors","ColdParticleColors"},
fieldInformation = {
    CoreState = {options = {{"Swap","None"},{"Hot", "Hot"},{"Cold", "Cold"}}},
    Directory = {fieldType = "path", allowFolders = false, allowFiles = true, filenameProcessor = function(filename, rawFilename, prefix) return vivUtil.trim(filename):sub(1,filename:match('^.*()/')-1) end }
},
placements = {{name = "R", data = { width = 16,height = 16,
    Directory = "objects/BumpBlockNew",
    HotParticleColors = "", ColdParticleColors = ""
}},{name = "C", data = { width = 16,height = 16,
    WindUpDist = 10.0,IceWindUpDist = 16.0, BounceDist = 24.0,
    LiftSpeedXMult = 0.75, WallPushTime = 0.1, BounceEndTime = 0.05, RespawnTime = 1.6
}},{name = "RC", data = {
    width = 16,height = 16,
    Directory = "objects/BumpBlockNew",
    HotParticleColors = "", ColdParticleColors = "",
    WindUpDist = 10.0,IceWindUpDist = 16.0, BounceDist = 24.0,
    LiftSpeedXMult = 0.75, WallPushTime = 0.1, BounceEndTime = 0.05, RespawnTime = 1.6
}}}}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local function getTextures(dir, cold)
    if cold then
        return dir.."/ice00" , dir.."/ice_center00"
    else
        return dir.."/fire00" , dir.."/fire_center00"
    end
end

function ccb.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockTexture, crystalTexture = getTextures(entity.Directory, entity.Core == "Cold")

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local crystalSprite = drawableSprite.fromTexture(crystalTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    crystalSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, crystalSprite)

    return sprites
end

return ccb