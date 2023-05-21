module VivHelperCrystalBombDetonator
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CrystalBombDetonator" CrystalBombDetonator2(x::Integer, y::Integer, width::Integer=8, height::Integer=8)

const placements = Ahorn.PlacementDict(
    "Crystal Bomb Detonator (Viv's Helper)" => Ahorn.EntityPlacement(
        CrystalBombDetonator2,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::CrystalBombDetonator2) = 8, 8
Ahorn.resizable(entity::CrystalBombDetonator2) = true, true

function Ahorn.selection(entity::CrystalBombDetonator2)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrystalBombDetonator2, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.45, 0.0, 0.45, 0.8), (0.7, 0.7, 0.0, 0.6))
end

end
