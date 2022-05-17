module VivHelperSeekerKillBarrier
using ..Ahorn, Maple

@mapdef Entity "VivHelper/SeekerKillBarrier" SeekerKillBarrier(x::Integer, y::Integer, width::Integer=8, height::Integer=8)

const placements = Ahorn.PlacementDict(
    "Seeker Kill Barrier (Viv's Helper)" => Ahorn.EntityPlacement(
        SeekerKillBarrier,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::SeekerKillBarrier) = 8, 8
Ahorn.resizable(entity::SeekerKillBarrier) = true, true

function Ahorn.selection(entity::SeekerKillBarrier)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SeekerKillBarrier, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.816, 0.188, 0.188, 0.5), (0.2, 0.2, 0.2, 0.6))
    Ahorn.drawCenteredText(ctx, "Seeker Kill Barrier", 0, 0, width, height)
end

end