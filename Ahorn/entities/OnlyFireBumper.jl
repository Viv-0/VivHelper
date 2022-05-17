module VivHelperOnlyFireBumper
using ..Ahorn, Maple

@mapdef Entity "VivHelper/EvilBumper" EvilBumper(x::Integer, y::Integer, wobble::Bool=true)

const placements = Ahorn.PlacementDict(
    "Fire Bumper (Viv's Helper)" => Ahorn.EntityPlacement(
        EvilBumper,
        "point"
    )
)

sprite = "objects/Bumper/Evil22.png"


Ahorn.nodeLimits(entity::EvilBumper) = 0, 1

function Ahorn.selection(entity::EvilBumper)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    if !isempty(nodes)
        nx, ny = Int.(nodes[1])

        return [Ahorn.Rectangle(x-12, y-12, 24, 24), Ahorn.Rectangle(nx-12, ny-12, 24, 24)]
    end

    return Ahorn.Rectangle(x-12, y-12, 24, 24)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::EvilBumper)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    if !isempty(nodes)
        nx, ny = Int.(nodes[1])

        theta = atan(y - ny, x - nx)
        Ahorn.drawArrow(ctx, x, y, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EvilBumper, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end