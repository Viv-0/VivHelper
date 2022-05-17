module VivHelperSeekerGate
using ..Ahorn, Maple

@mapdef Entity "VivHelper/SeekerGate" SeekerGate(x::Integer, y::Integer,OpenRadius::Number=64.0, CloseRadius::Number=80.0, Directory::String="VivHelper/seekergate/seekerdoor")

const placements = Ahorn.PlacementDict(
    "Seeker Gate (Viv's Helper)" => Ahorn.EntityPlacement(
        SeekerGate,
        "rectangle"
    )
)
function getString(entity)
    s = get(entity.data, "Directory", "VivHelper/seekergate/seekerdoor") * "00"
    return s;
end

function Ahorn.selection(entity::SeekerGate)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x, y, 15, 48)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SeekerGate, room::Maple.Room) = Ahorn.drawImage(ctx, getString(entity), -5, 0)

end
