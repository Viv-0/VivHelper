module VivHelperDreamMirrorNoCS
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CutscenelessDreamMirror" DreamMirrorNCS(
    x::Integer, y::Integer, FrameType::String="default", GlassType::String="default", Broken::Bool=false, Reflection::String="InversePlayer"
)

const placements = Ahorn.PlacementDict(
   "Dream Mirror (No Cutscene, Semi-Custom) (Viv's Helper)" => Ahorn.EntityPlacement(
      DreamMirrorNCS
   )
)

const glassType = Dict{String, String}(
    "default" => "default",
    "Madeline" => "redMirror",
    "Badeline" => "purpMirror"
)

Ahorn.editingOptions(entity::DreamMirrorNCS) = Dict{String, Any}(
    "FrameType" => String["default", "copper", "gray", "purple", "shadow", "steel_gray", "warped1", "warped2"],
    "Reflection" => String["InversePlayer", "SameAsPlayer", "MaddyOnly", "BaddyOnly", "None"],
    "GlassType" => glassType
)

function frameFind(entity::DreamMirrorNCS)
    temp = get(entity.data, "FrameType", "default")
    frame = "VivHelper/MaddyBaddyMirror/dream_frame_"
    if temp == "default"
        return frame
    else
        return frame * temp
    end
end

function glassFind(entity::DreamMirrorNCS)
    Baddy2 = get(entity.data, "GlassType", "default")
    broke = get(entity.data, "Broken", false)
    broken = broke ? "01" : "00"
    if Baddy2 == "default"
        if broke
            return "objects/mirror/glassbreak09"
        else
            return "objects/mirror/glassbg"
        end
    else
        return "VivHelper/MaddyBaddyMirror/" * Baddy2 * broken
    end
end

function Ahorn.selection(entity::DreamMirrorNCS)
    x, y = Ahorn.position(entity)
    f = frameFind(entity)
    g = glassFind(entity)
    return Ahorn.coverRectangles([
        Ahorn.getSpriteRectangle(f, x, y, jx=0.5, jy=1.0),
        Ahorn.getSpriteRectangle(g, x, y, jx=0.5, jy=1.0)
    ])
end

const shine = "VivHelper/MaddyBaddyMirror/shine00"

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamMirrorNCS, room::Maple.Room)
    frame = frameFind(entity)
    glass = glassFind(entity)

    Ahorn.drawSprite(ctx, glass, 0, 0, jx=0.5, jy=1.0)
    Ahorn.drawSprite(ctx, shine, 0, 0, jx=0.5, jy=1.0, alpha=0.5)
    Ahorn.drawSprite(ctx, frame, 0, 0, jx=0.5, jy=1.0)

end

end
