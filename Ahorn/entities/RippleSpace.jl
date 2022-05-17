module VivHelperRippleSpace
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/RippleSpace" RippleSpace(x::Integer, y::Integer,
width::Integer=24, height::Integer=24, RippleRate::Integer=128)

const cols = [(0.95,0.95,0.95,0.6),(0.9,0.9,0.9,0.5)]

const placements = Ahorn.PlacementDict(
	"Ripple Space (Viv's Helper)" => Ahorn.EntityPlacement(
		RippleSpace,
		"rectangle"
	)
)

Ahorn.resizable(entity::RippleSpace) = true, true
Ahorn.minimumSize(entity::RippleSpace) = 8, 8
Ahorn.selection(entity::RippleSpace) = Ahorn.getEntityRectangle(entity)

const rippleSprite = "ahorn/VivHelper/ripple.png"

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RippleSpace, room::Maple.Room)
	width = get(entity.data, "width", 8);
	height = get(entity.data, "height", 8);
	Ahorn.drawRectangle(ctx, 0, 0, width, height, cols[1], cols[2]);
	for i in 0:floor((width-1)/8), j in 0:floor((height-1)/8)
		Ahorn.drawSprite(ctx, rippleSprite, 8*i+4, 8*j+4)
	end
end

end
