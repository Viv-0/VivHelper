local drawableSpriteStruct = require("structs.drawable_sprite")

local dreamMirror = {}

dreamMirror.name = "VivHelper/CutscenelessDreamMirror"
dreamMirror.placements = {
    name = "normal",
    data = {
        FrameType = "default",
        GlassType = "default",
        Broken = false,
        Reflection = "InversePlayer"
    }
}
dreamMirror.fieldInformation = {
    FrameType = {fieldType = "string", options = {"default", "copper", "gray", "purple", "shadow", "steel_gray", "warped1", "warped2"}, editable = false},
    Reflection = {fieldType = "string", options = {"InversePlayer", "SameAsPlayer", "MaddyOnly", "BaddyOnly", "None"}, editable = false},
    GlassType = {fieldType = "string", options = {{"default", "default"}, {"Madeline", "redMirror"}, {"Badeline", "purpMirror"}}, editable = false}
}

function dreamMirror.sprite(room, entity)
    local sprites = {}
    local ft = entity.FrameType or "default"
    local gt = entity.GlassType or "default"
    if gt == "default" then gt = (entity.Broken and "objects/mirror/glassbreak09" or "objects/mirror/glassbg")
    else gt = "VivHelper/MaddyBaddyMirror/" .. gt .. (entity.Broken and "01" or "00") end
    local frameSprite = drawableSpriteStruct.fromTexture("VivHelper/MaddyBaddyMirror/dream_frame_" .. ft, entity)
    frameSprite:setJustification(0.5, 1.0)
    frameSprite.depth = 9000

    local glassSprite = drawableSpriteStruct.fromTexture(gt, entity)
    glassSprite:setJustification(0.5, 1.0)
    glassSprite.depth = 9500

    local shineSprite = drawableSpriteStruct.fromTexture("VivHelper/MaddyBaddyMirror/shine00", entity)
    shineSprite:setJustification(0.5, 1.0)
    shineSprite:setAlpha(0.5)
    shineSprite.depth = 9499

    table.insert(sprites, glassSprite) -- Order important for pre-placement
    table.insert(sprites, shineSprite)
    table.insert(sprites, frameSprite)

    return sprites
end

return dreamMirror