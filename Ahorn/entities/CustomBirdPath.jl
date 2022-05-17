module VivHelperCustomBirdPath
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CustomBirdPath" CustomBirdPath(x::Integer, y::Integer, only_once::Bool=false, onlyIfLeft::Bool=false, speedMult::Number=1.0, SpritePath::String="characters/bird/flyup", TrailColor::String="639bff")

const placements = Ahorn.PlacementDict(
    "Custom Bird Path (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomBirdPath
    ),
)

Ahorn.nodeLimits(entity::CustomBirdPath) = 0, -1

function Ahorn.selection(entity::CustomBirdPath)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(string(get(entity.data, "SpritePath", "characters/bird/flyup"), "00"), x, y)]

    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(string(get(entity.data, "SpritePath", "characters/bird/flyup"), "00"), nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomBirdPath)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        theta = atan(py - ny, px - nx)
        Ahorn.drawArrow(ctx, px, py, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, string(get(entity.data, "SpritePath", "characters/bird/flyup"), "00"), nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomBirdPath, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, get(entity.data, "SpritePath", "characters/bird/flyup") * "00", x, y)
end

end
