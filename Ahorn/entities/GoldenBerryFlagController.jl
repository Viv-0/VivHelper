module VivHelperGoldenBerryToFlag
using ..Ahorn, Maple

@mapdef Entity "VivHelper/GoldenBerryToFlag" GBTF(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Golden Berry to Flag Controller (Viv's Helper)" => Ahorn.EntityPlacement(
        GBTF,
        "point"
    )
)
sprite = "ahorn/VivHelper/GoldenBerryToFlag.png"

Ahorn.selection(entity::GBTF) = Ahorn.Rectangle(get(entity.data, "x", 12)-12, get(entity.data, "y", 12)-12, 24, 24)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GBTF, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)


end