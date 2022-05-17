module VivHelperRefilllessBumper
using ..Ahorn, Maple

@mapdef Entity "VivHelper/RefilllessBumper" RefilllessBumper(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "No Refill Bumper (Viv's Helper)" => Ahorn.EntityPlacement(
        RefilllessBumper
    )
)

Ahorn.nodeLimits(entity::RefilllessBumper) = 0, 1

sprite = "ahorn/VivHelper/norefillBumper"

function Ahorn.selection(entity::RefilllessBumper)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    if !isempty(nodes)
        nx, ny = Int.(nodes[1])

        return [Ahorn.Rectangle(x-12, y-12, 24, 24), Ahorn.Rectangle(nx-12, ny-12, 24, 24)]
    end

    return Ahorn.Rectangle(x-12, y-12, 24, 24)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::RefilllessBumper)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    if !isempty(nodes)
        nx, ny = Int.(nodes[1])

        theta = atan(y - ny, x - nx)
        Ahorn.drawArrow(ctx, x, y, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RefilllessBumper, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end