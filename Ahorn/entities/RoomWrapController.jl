module VivHelperRoomWrapController
using ..Ahorn, Maple

@mapdef Entity "VivHelper/RoomWrapController" RoomWrapController(
	x::Integer,
	y::Integer,
	Top::Bool=false,
	TopOffset::Number=4.0,
	TopExitSpeedAdd::Number=15.0,
	Right::Bool=false,
	RightOffset::Number=8.0,
	RightExitSpeedAdd::Number=0.0,
	Bottom::Bool=false,
	BottomOffset::Number=12.0,
	BottomExitSpeedAdd::Number=15.0,
	Left::Bool=false,
	LeftOffset::Number=8.0,
	LeftExitSpeedAdd::Number=0.0,
	setByCamera::Bool=false,
	allEntities::Bool=false,
	ZFlagsData::String="",
	AutomateCameraTriggers::Bool=false,
	legacy::Bool=true
)

const placements = Ahorn.PlacementDict(
    "Room Wrap Controller (Viv's Helper)" => Ahorn.EntityPlacement(
        RoomWrapController
	) )

function Ahorn.selection(entity::RoomWrapController)
	x, y = Ahorn.position(entity)

	return Ahorn.Rectangle(x - 16, y - 16, 32, 32)

end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::RoomWrapController, room::Maple.Room)
	x, y = Ahorn.position(entity)
	Ahorn.drawSprite(ctx, "ahorn/VivHelper/wrap", x, y)
	# Creates rectangle showing the wrapping bounds of the level
	xW, yH = room.size
	xL = Int(get(entity.data, "LeftOffset", 8.0));
	xR = xW - Int(get(entity.data, "RightOffset", 8.0));
	yT = Int(get(entity.data, "TopOffset", 4.0));
	yB = yH - Int(get(entity.data, "BottomOffset", 12.0));
	n = []
	if(get(entity.data, "Left", false))
		push!(n, [(xL+8,yT+8), (xL+8,yB-8)])
	end
	if(get(entity.data, "Top", false))
		push!(n, [(xL+8,yT+8), (xR-8,yT+8)])
	end
	if(get(entity.data, "Right", false))
		push!(n, [(xR-8,yT+8), (xR-8,yB-8)])
	end
	if(get(entity.data, "Bottom", false))
		push!(n, [(xL+8,yB-8), (xR-8,yB-8)])
	end
	for N in n
		Ahorn.drawLines(ctx, N, ((32,178,170,127) ./ 255); thickness=2)
	end
end

end
