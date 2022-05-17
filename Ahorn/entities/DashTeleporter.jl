module VivHelperDashTP
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/TeleporterDash" DashTP(x::Integer, y::Integer,
    width::Integer=16, height::Integer=16,
    WarpRoom::String="", newPosX::Integer=-1, newPosY::Integer=-1,
	TransitionType::String="None", ZFlagsData::String="",
	CooldownLength::Number=0.0, tiletype::String="3"
)

const placements = Ahorn.PlacementDict(
    "Teleporter Block (Viv's Helper)" => Ahorn.EntityPlacement(
        DashTP,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.minimumSize(entity::DashTP) = 16, 16
Ahorn.resizable(entity::DashTP) = true, true
Ahorn.nodeLimits(entity::DashTP) = 0, -1


const TransitionType = Dict{String, String}(
	"None" => "None",
	"Lightning" => "Lightning",
	"Glitch" => "GlitchEffect",
	"Color Flash" => "ColorFlash"
)


Ahorn.editingOptions(entity::DashTP) = Dict{String, Any}(
	"WarpRoom" => VivHelper.getRoomNames(),
    "TransitionType" => TransitionType,
	"tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.selection(entity::DashTP) = Ahorn.getEntityRectangle(entity)

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::DashTP, room::Maple.Room)
	Ahorn.drawTileEntity(ctx, room, entity)
	x, y = Ahorn.position(entity)
	width = get(entity.data, "width", 24); height = get(entity.data, "height", 16);
	Ahorn.drawRectangle(ctx, x+4, y+4, width-8, height-8, (0.789, 0.287, 0.652, 0.5), (0.789, 0.287, 0.652, 0.5));
	px, py = x+(width/2), y+(height/2)
	curves = [
	Ahorn.SimpleCurve((px, py), (px+4, py), (px+1, py+2)),
	Ahorn.SimpleCurve((px, py), (px, py+4), (px-2, py+1)),
	Ahorn.SimpleCurve((px, py), (px-4, py), (px-1, py-2)),
	Ahorn.SimpleCurve((px, py), (px, py-4), (px+2, py-1))
	]
	for a in curves
		Ahorn.drawSimpleCurve(ctx, a, (0.0, 0.0, 0.0, 1.0))
	end
end

end
