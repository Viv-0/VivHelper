module VivHelperWrappableGlider
using ..Ahorn, Maple

@mapdef Entity "VivHelper/WrappableGlider" WrappableGlider(
	x::Integer,
	y::Integer,
	removeOnBottom::Bool=true,
	bubble::Bool=false

)

const placements = Ahorn.PlacementDict(
    "Glider (Wrappable) (Viv's Helper)" => Ahorn.EntityPlacement(
        WrappableGlider
	) )

function Ahorn.selection(entity::WrappableGlider)
	x, y = Ahorn.position(entity)

	return Ahorn.Rectangle(x - 14, y - 9, 28, 17)

end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WrappableGlider, room::Maple.Room)
	Ahorn.drawSprite(ctx, "ahorn/VivHelper/wrapglider", 0, 0)
	if get(entity, "bubble", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end

end
