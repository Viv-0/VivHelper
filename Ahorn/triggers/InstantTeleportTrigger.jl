module VivHelperInstantTeleportTrigger
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Trigger "VivHelper/BasicInstantTeleportTrigger" BasicInstantTeleportTrigger(
	x::Integer, y::Integer, width::Integer=8, height::Integer=8,
	WarpRoom::String="", newPosX::Integer=-1, newPosY::Integer=-1,
	TransitionType::String="None", AddTriggerOffset::Bool=false, CameraType::Integer=2
)
@mapdef Trigger "VivHelper/MainInstantTeleportTrigger" MainInstantTeleportTrigger(
	x::Integer, y::Integer, width::Integer=8, height::Integer=8,
	WarpRoom::String="", newPosX::Integer=-1, newPosY::Integer=-1,
	AddTriggerOffset::Bool=false,
	VelocityModifier::Bool=false, ExitVelocityX::Number=0.0, ExitVelocityY::Number=0.0, Dreaming::Bool=false,
	TransitionType::String="None", TimeBeforeTeleport::Number=0.0, CameraType::Integer=3,
	ResetDashes::Bool=true, ForceNormalState::Bool=false,
	bringHoldablesThrough::Bool=false
)
@mapdef Trigger "VivHelper/CustomInstantTeleportTrigger" CustomInstantTeleportTrigger(
	x::Integer, y::Integer, width::Integer=8, height::Integer=8,
	WarpRoom::String="", newPosX::Integer=-1, newPosY::Integer=-1,
	VelocityModifier::Bool=false, ExitVelocityX::Number=0.0, ExitVelocityY::Number=0.0, ExitVelocityS::Number=0.0, Dreaming::Bool=false,
	ZFlagsData::String="", AddTriggerOffset::Bool=false,
	RotationType::Bool=false, RotationActor::Number=0.0,
	TimeSlowDown::Number=0.0, TimeBeforeTeleport::Number=0.0,
	TransitionType::String="None", CameraType::Integer=3,
	ResetDashes::Bool=true, ForceNormalState::Bool=false,
	bringHoldablesThrough::Bool=false
)

const placements = Ahorn.PlacementDict(
    "Instant Teleport Trigger (Simple) (Viv's Helper)" => Ahorn.EntityPlacement(
        BasicInstantTeleportTrigger,
        "rectangle"
    ),
	 "Instant Teleport Trigger (Advanced) (Viv's Helper)" => Ahorn.EntityPlacement(
        MainInstantTeleportTrigger,
        "rectangle"
    ),
	 "Instant Teleport Trigger (Fully Customizable) (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomInstantTeleportTrigger,
        "rectangle"
    )
)


const TransitionType = Dict{String, String}(
	"None" => "None",
	"Lightning" => "Lightning",
	"Glitch" => "GlitchEffect",
	"Color Flash" => "ColorFlash"
)

Ahorn.editingOptions(entity::BasicInstantTeleportTrigger) = Dict{String, Any}(
	"WarpRoom" => VivHelper.getRoomNames(),
    "TransitionType" => TransitionType,
	"CameraType" => Dict{String, Integer}(
		"Move to Player" => 0,
		"Center On Player" => 1,
		"Move to Player + Target/Offset" => 2,
		"Move to Player + Offset" => 3,
		"\"Perfect\"" => 4,
	)
)

Ahorn.editingOptions(entity::MainInstantTeleportTrigger) = Dict{String, Any}(
	"WarpRoom" => VivHelper.getRoomNames(),
    "TransitionType" => TransitionType,
	"VelocityModifier" =>  Dict{String, Bool}(
        "Multiply" => true,
        "Add" => false
	),
	"CameraType" => Dict{String, Integer}(
		"Move to Player" => 0,
		"Center On Player" => 1,
		"Move to Player + Target/Offset" => 2,
		"Move to Player + Offset" => 3
	)
)

Ahorn.editingOptions(entity::CustomInstantTeleportTrigger) = Dict{String, Any}(
	"WarpRoom" => VivHelper.getRoomNames(),
   "TransitionType" => TransitionType,
	"VelocityModifier" =>  Dict{String, Bool}(
        "Multiply" => true,
        "Add" => false
	),
	"RotationType" =>  Dict{String, Bool}(
        "Rotate To Direction" => true,
        "Rotate Degrees From Direction" => false
	),
	"RotationActor" => Number[0.0, 90.0, 180.0, 270.0],
	"TimeSlowDown" => Dict{String, Number}(
		"Off" => 0.0,
		"Default" => 0.25
	),
	"CameraType" => Dict{String, Integer}(
		"Move to Player" => 0,
		"Center On Player" => 1,
		"Move to Player + Target/Offset" => 2,
		"Move to Player + Offset" => 3
	)
)

end
