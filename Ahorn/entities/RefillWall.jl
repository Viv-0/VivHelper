module VivHelperRefillWall
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/RefillWall" RefillWall(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    twoDashes::Bool=false,
    oneUse::Bool=false, Alpha::Number=1.0, RespawnTime::Number=-1.0
)

@mapdef Entity "VivHelper/RefillWallWrapper" RefillWallWrapper(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    TypeName::String="Refill", ImageVariableName::String="sprite", RespawnMethodName::String="Respawn", InnerColor::String="888888",
    OuterColor::String="d0d0d0", RespawnTime::Number=-1.0, Depth::Integer=100, oneUse::Bool=false
)

function luminance(r::Number, g::Number, b::Number)
    r = r <= 0.03928 ? r / 12.92 : ((r + 0.055) / 1.055) ^ 2.4;
    g = g <= 0.03928 ? g / 12.92 : ((g + 0.055) / 1.055) ^ 2.4;
    b = b <= 0.03928 ? b / 12.92 : ((b + 0.055) / 1.055) ^ 2.4;
    return r * 0.2126 + g * 0.7152 + b * 0.0722
end

const placements = Ahorn.PlacementDict(
    "Refill Wall (Viv's Helper)" => Ahorn.EntityPlacement(
        RefillWall,
        "rectangle"
    ),
    "Refill Wall Wrapper (Viv's Helper)" => Ahorn.EntityPlacement(RefillWallWrapper, "rectangle")
)

Ahorn.editingOptions(entity::RefillWallWrapper) = Dict{String, Any}(
    "InnerColor" => VivHelper.XNAColors,
    "OuterColor" => VivHelper.XNAColors,
    "Depth" => merge(VivHelper.Depths, Dict{String, Any}("Default" => 100))
)

const RefillWalls = Union{RefillWall, RefillWallWrapper}

Ahorn.minimumSize(entity::RefillWalls) = 8, 8
Ahorn.resizable(entity::RefillWalls) = true, true
Ahorn.selection(entity::RefillWalls) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RefillWall, room::Maple.Room)
    t = get(entity.data, "twoDashes", false)
    u = get(entity.data, "oneUse", false) ? 0.25 : 0.7

    incolor = t ? (.738, .25, .578, u) : (.125, .5, .125, u)
    outcolor = t ? (.886, .408, .82, 0.7) : (.576, .741, .251, 0.7)
    sprite = t ? "objects/refillTwo/idle00.png" : "objects/refill/idle00.png"
    width = get(entity.data, "width", 16); height = get(entity.data, "height", 16);
    Ahorn.drawRectangle(ctx, 0, 0, width, height, incolor, outcolor)
    Ahorn.drawSprite(ctx, sprite, width/2, height/2)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RefillWallWrapper, room::Maple.Room)
    incolor = VivHelper.ColorFix(get(entity.data, "InnerColor", "888888"), 1.0)
    outcolor = VivHelper.ColorFix(get(entity.data, "OuterColor", "d0d0d0"), 1.0)
    width = get(entity.data, "width", 16); height = get(entity.data, "height", 16);
    Ahorn.drawRectangle(ctx, 0, 0, width, height, incolor, outcolor)
    l = luminance(incolor[1], incolor[2], incolor[3]);
    m = l < 0.5 ? 1 - l ^ 3 : (1-l)^3
    Ahorn.drawCenteredText(ctx, "Custom Refill Wall", 0, 0, width, height; tint=(m,m,m, 1.0))
end

end
