module VivHelperWatchtowers
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CustomPlaybackWatchtower" PlaybackWatchtower(x::Integer, y::Integer,
summit::Bool=false, onlyY::Bool=false, XMLName::String="lookout", CustomPlaybackTag::String="",
Accel::Number=800.0, maxSpeed::Number=240.0, FlagWhileInHud::String="", IgnoreBind::Bool=true, SetOnAwake::Bool=true)

@mapdef Entity "VivHelper/PlatinumWatchtower" HintWatchtower(x::Integer, y::Integer,
summit::Bool=false, onlyY::Bool=false, XMLName::String="lookout",
Accel::Number=800.0, maxSpeed::Number=240.0,
IconDirectory::String="VivHelper/PlatinumWatchtower/hud", FlagWhileInHud::String="",
InstantStart::Bool=false,
Hint1DialogID::String="", Hint1Tag::String="",
Hint2DialogID::String="", Hint2Tag::String="",
Hint3DialogID::String="", Hint3Tag::String="",
Hint4DialogID::String="", Hint4Tag::String="",
Hint5DialogID::String="", Hint5Tag::String="",
)

const placements = Ahorn.PlacementDict(
    "Watchtower (Custom Speed, Playback) (Viv's Helper)" => Ahorn.EntityPlacement(
        PlaybackWatchtower,
        "point",
        Dict{String, Any}()
    ),
    "Watchtower (Custom Speed, Hints) (Viv's Helper)" => Ahorn.EntityPlacement(
        HintWatchtower,
        "point",
        Dict{String, Any}()
    ),
)

Watchtowers = Union{PlaybackWatchtower, HintWatchtower}
Ahorn.nodeLimits(entity::Watchtowers) = 0, -1

sprite = "objects/lookout/lookout05.png"

function Ahorn.selection(entity::Watchtowers)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y, jx=0.5, jy=1.0)]

    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny, jx=0.5, jy=1.0))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::Watchtowers)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py - 8, nx, ny - 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny, jx=0.5, jy=1.0)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::PlaybackWatchtower, room::Maple.Room)
    x, y = Ahorn.position(entity)

    Ahorn.drawSprite(ctx, sprite, x, y, jx=0.5, jy=1.0)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::HintWatchtower, room::Maple.Room)
    x, y = Ahorn.position(entity)

    Ahorn.drawSprite(ctx, sprite, x, y, jx=0.5, jy=1.0)
end

end
