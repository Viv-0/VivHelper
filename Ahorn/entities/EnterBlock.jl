module VivHelperEnterBlock
using ..Ahorn, Maple

@mapdef Entity "VivHelper/EnterBlock" EnterBlock(
x::Integer, y::Integer, width::Integer=8, height::Integer=8,
tiletype::String="3", playTransitionReveal::Bool=false)

const placements = Ahorn.PlacementDict(
    "Enter Block (Viv's Helper)" => Ahorn.EntityPlacement(
        EnterBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::EnterBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::EnterBlock) = 8, 8
Ahorn.resizable(entity::EnterBlock) = true, true

Ahorn.selection(entity::EnterBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::EnterBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, alpha=0.7)

end
