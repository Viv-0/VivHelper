module VivHelperWrappableFallingBlock

using ..Ahorn, Maple

@mapdef Entity "VivHelper/WrappableFallingBlock" WrappableFallingBlock(
	x::Integer,
	y::Integer,
	width::Integer=16,
	height::Integer=16,
	tiletype::String="3",
	climbFall::Bool=true,
	maxRevolutions::Integer=-1
)


const placements = Ahorn.PlacementDict(
    "Falling Block (Wrappable) (Viv's Helper)" => Ahorn.EntityPlacement(
        WrappableFallingBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::WrappableFallingBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::WrappableFallingBlock) = 8, 8
Ahorn.resizable(entity::WrappableFallingBlock) = true, true

Ahorn.selection(entity::WrappableFallingBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::WrappableFallingBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end
