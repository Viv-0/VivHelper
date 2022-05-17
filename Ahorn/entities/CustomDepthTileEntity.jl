module VivHelperDepthTileEntity
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CustomDepthTileEntity" DepthTileEntity(
x::Integer, y::Integer, width::Integer=8, height::Integer=8,
Depth::Integer=-10000)

const placements = Ahorn.PlacementDict(
    "Custom Depth Tile Entity (Viv's Helper)" => Ahorn.EntityPlacement(
        DepthTileEntity,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::DepthTileEntity) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
    "Depth" => VivHelper.Depths
)

Ahorn.minimumSize(entity::DepthTileEntity) = 8, 8
Ahorn.resizable(entity::DepthTileEntity) = true, true

Ahorn.selection(entity::DepthTileEntity) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::DepthTileEntity, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end
