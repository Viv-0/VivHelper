module VivHelperHideRoomInMap

using ..Ahorn, Maple

@mapdef Entity "VivHelper/HideRoomInMap" HRIM(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Hide Room Controller (Viv's Helper)" => Ahorn.EntityPlacement(
        HRIM, "point", Dict{String, Any}("Flag"=>"", "DummyOnly"=>false)
    )
)

sprite = "ahorn/VivHelper/HiddenRoom.png"

Ahorn.selection(entity::HRIM) = Ahorn.Rectangle(get(entity.data, "x", 12)-12, get(entity.data, "y", 12)-12, 24, 24)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HRIM, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end