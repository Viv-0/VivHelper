module VivHelperTheoKillBarrier
using ..Ahorn, Maple

@mapdef Entity "VivHelper/TheoKillBarrier" TheoKillBarrier(x::Integer, y::Integer, width::Integer=8, height::Integer=8)

const placements = Ahorn.PlacementDict(
    "Theo Kill Barrier (Viv's Helper)" => Ahorn.EntityPlacement(
        TheoKillBarrier,
        "rectangle"
    )
)
Ahorn.minimumSize(entity::TheoKillBarrier) = 8, 8
Ahorn.resizable(entity::TheoKillBarrier) = true, true

function Ahorn.selection(entity::TheoKillBarrier)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TheoKillBarrier, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.251, 0.753, 0.816, 0.8), (0.0, 0.0, 0.0, 0.0))
end

end