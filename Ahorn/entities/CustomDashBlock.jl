module VivHelperCustomDashBlock
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CustomDashBlock" CustomDashBlock(x::Integer, y::Integer,
width::Integer=8, height::Integer=8, tiletype::String="3",
blendin::Bool=true, canDash::Bool=true, permanent::Bool=true,
FlagOnBreak::String="", AudioEvent::String="event:/game/general/wall_break_stone"
)

const placements = Ahorn.PlacementDict(
    "Custom Dash Block (Flags, Audio) (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomDashBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::CustomDashBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
    "AudioEvent" => String["event:/game/general/wall_break_dirt", "event:/game/general/wall_break_wood", "event:/game/general/wall_break_ice", "event:/game/general/wall_break_stone"]
)

Ahorn.minimumSize(entity::CustomDashBlock) = 8, 8
Ahorn.resizable(entity::CustomDashBlock) = true, true

Ahorn.selection(entity::CustomDashBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomDashBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end
