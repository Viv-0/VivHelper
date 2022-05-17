module VivHelperCustomFallingBlock
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CustomFallingBlock" CustomFallingBlock(x::Integer, y::Integer,
    width::Integer=8, height::Integer=8, Accel::Number=500.0, MaxSpeed::Number=160.0,
    ShakeSFX::String="event:/game/general/fallblock_shake", ImpactSFX::String="event:/game/general/fallblock_impact",
    Direction::String="Down", FlagOnFall::String="", FlagTrigger::String="", FlagOnGround::String="",
    behind::Bool=false, climbFall::Bool=true, bufferClimbFall::Bool=false, Legacy::Bool=true
)

const placements = Ahorn.PlacementDict(
    "Custom Falling Block (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomFallingBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::CustomFallingBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
    "Direction" => String["Down", "Right", "Left", "Up"]
)

Ahorn.minimumSize(entity::CustomFallingBlock) = 8, 8
Ahorn.resizable(entity::CustomFallingBlock) = true, true

Ahorn.selection(entity::CustomFallingBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomFallingBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end
