module VivHelperCustomPlayerPlayback

using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CPP" CustomPlayerPlayback(x::Integer, y::Integer, tutorial::String="", nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[],
Delay::Number=1.0, StartActive::Bool=true, SpeedMultiplier::Number=1.0,
CustomStringID::String="", Color::String="")

const placements = Ahorn.PlacementDict(
    "Custom Player Playback (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomPlayerPlayback,
        "rectangle"
    )
)

const baseGameTutorials = String[
    "combo", "superwalljump", "too_close", "too_far",
    "wavedash", "wavedashppt"
]

Ahorn.editingOptions(entity::CustomPlayerPlayback) = Dict{String, Any}(
    "tutorial" => baseGameTutorials,
    "SpeedMultiplier" => Dict{String, Number}(
    "2x" => 2.0,
    "1x" => 1.0,
    "0.5x" => 0.5,
    "0.333x" => 0.3333333,
    "0.25x" => 0.25,
    "0.2x" => 0.2,
    "0.1x" => 0.1
    ),
    "Color" => VivHelper.XNAColors
)

Ahorn.nodeLimits(entity::CustomPlayerPlayback) = 0, 2

const sprite = "characters/player/sitDown00"

function Ahorn.selection(entity::CustomPlayerPlayback)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y, jx=0.5, jy=1.0)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomPlayerPlayback)
    c = get(entity.data, "Color", "")
    c = c == "" ? "cc3333" : c;
    color = VivHelper.ColorFix(c, 0.75);
    Ahorn.drawSprite(ctx, sprite, 0, 0, jx=0.5, jy=1.0, tint=color)
end

end
