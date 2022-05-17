module VivHelperVariantMirror
using ..Ahorn, Maple

@mapdef Entity "VivHelper/VariantChangingMirror" VariantChangingMirror(
    x::Integer, y::Integer, BadelineMirror::Bool=false
)

const placements = Ahorn.PlacementDict(
   "Variant Changing Mirror (Viv's Helper)" => Ahorn.EntityPlacement(
      VariantChangingMirror
   )
)
function Ahorn.selection(entity::VariantChangingMirror)
    x, y = Ahorn.position(entity)
    frameSprite = Ahorn.getSprite("objects/mirror/resortframe", "Gameplay")

    return Ahorn.Rectangle(x - frameSprite.width / 2, y - frameSprite.height, frameSprite.width, frameSprite.height)
end

function glassFind(entity::VariantChangingMirror)
    Baddy2 = get(entity.data, "BadelineMirror", false)

    if Baddy2
        return "VivHelper/MaddyBaddyMirror/purpMirror00"

    else
        return "VivHelper/MaddyBaddyMirror/redMirror00"
    end
end


function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VariantChangingMirror, room::Maple.Room)
    frameSprite = Ahorn.getSprite("objects/mirror/resortframe", "Gameplay")
    glass = glassFind(entity)
    glassSprite = Ahorn.getSprite(glass, "Gameplay")

    glassWidth = frameSprite.width - 4
    glassHeight = frameSprite.height - 8

    Ahorn.drawImage(ctx, glassSprite, -glassHeight / 2, -glassHeight, (glassSprite.width - glassWidth) / 2, glassSprite.height - glassHeight, glassWidth, glassHeight)
    Ahorn.drawImage(ctx, frameSprite, -frameSprite.width / 2, -frameSprite.height)
end

end
