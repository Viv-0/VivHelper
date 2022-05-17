module VivHelperFloatyBreakBlock
using ..Ahorn, Maple

@mapdef Entity "VivHelper/FloatyBreakBlock" FloatyBreakBlock(
	x::Integer,
	y::Integer,
	width::Integer=Maple.defaultBlockWidth,
	height::Integer=Maple.defaultBlockHeight,
	delay::Number=1.0,
	delayType::String="timer",
	sidekill::Bool=false,
	disableSpawnOffset::Bool=false
)

const delayTypes = String[
	"timer",
	"sinking",
	"pressure"
]

const placements = Ahorn.PlacementDict(
    "FloatyBreakBlock (Viv's Helper)" => Ahorn.EntityPlacement(
        FloatyBreakBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::FloatyBreakBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
	"delayType" => delayTypes
)

Ahorn.minimumSize(entity::FloatyBreakBlock) = 8, 8
Ahorn.resizable(entity::FloatyBreakBlock) = true, true
Ahorn.selection(entity::FloatyBreakBlock) = Ahorn.getEntityRectangle(entity)
Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FloatyBreakBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end
