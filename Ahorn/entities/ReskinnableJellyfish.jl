module VivHelperReskinnableJelly
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/ReskinnableJelly" ReskinJelly(x::Integer, y::Integer, bubble::Bool=false, tutorial::Bool=false,
Directory::String="objects/glider", Depth::Integer=-5,
GlidePath::String="particles/rect", GlideColor1::String="4FFFF3", GlideColor2::String="FFF899",
GlowPath::String="", GlowColor1::String="B7F3FF", GlowColor2::String="F4FDFF")

const placements = Ahorn.PlacementDict(
    "Reskinnable Jellyfish (Custom Particles) (Viv's Helper)" => Ahorn.EntityPlacement(
        ReskinJelly,
        "point"
    )
)

Ahorn.editingOptions(entity::ReskinJelly) = Dict{String, Any}(
    "Directory" => String["objects/glider"],
    "Depth" => merge(VivHelper.Depths, Dict{String, Integer}("Default" => -5)),
    "GlidePath" => merge(VivHelper.Particles, Dict{String, Any}("Default" => "particles/rect")),
    "GlowPath" => merge(VivHelper.Particles, Dict{String, Any}("Default" => "")),
    "GlideColor1" => VivHelper.XNAColors,
    "GlideColor2" => VivHelper.XNAColors,
    "GlowColor1" => VivHelper.XNAColors,
    "GlowColor2" => VivHelper.XNAColors,
)

function getSprite(entity::ReskinJelly)
    s = get(entity.data, "Directory", "objects/glider")
    if (endswith(s, "/")) s = chop(s); end
    sprites = Ahorn.findTextureAnimations(string(s, "/idle"), Ahorn.getAtlas());
    return sprites[1];
end

function Ahorn.selection(entity::ReskinJelly)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(getSprite(entity), x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReskinJelly, room::Maple.Room)

    Ahorn.drawSprite(ctx, getSprite(entity), 0, 0)

    if get(entity.data, "bubble", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end

end
