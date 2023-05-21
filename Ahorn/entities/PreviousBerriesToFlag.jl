module VivHelperPrevBerriesToFlag
using ..Ahorn, Maple
 
@mapdef Entity "VivHelper/PreviousBerriesToFlag" PrevBerriesToFlag(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Previous Collected Berries to Flags Controller (Viv's Helper)" => Ahorn.EntityPlacement(
        PrevBerriesToFlag,
        "rectangle"
    )
)

sprite = "ahorn/VivHelper/PrevBerriesToFlag.png"

Ahorn.selection(entity::PrevBerriesToFlag) = Ahorn.Rectangle(get(entity.data, "x", 12)-12, get(entity.data, "y", 12)-12, 24, 24)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PrevBerriesToFlag, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end