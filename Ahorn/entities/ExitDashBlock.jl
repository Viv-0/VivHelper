module VivHelperExitDashBlock
using ..Ahorn, Maple

@mapdef Entity "VivHelper/ExitDashBlock" ExitDashBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tiletype::String="3", blendin::Bool=true, canDash::Bool=true, permanent::Bool=true)

const placements = Ahorn.PlacementDict(
    "Exit Dash Block (Viv's Helper)" => Ahorn.EntityPlacement(
        ExitDashBlock,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::ExitDashBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::ExitDashBlock) = 8, 8
Ahorn.resizable(entity::ExitDashBlock) = true, true

Ahorn.selection(entity::ExitDashBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ExitDashBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, alpha=0.7)

end
