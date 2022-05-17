module VivHelperDeathConditionalBlock
using ..Ahorn, Maple

@mapdef Entity "VivHelper/DeathConditionalBlock" DeathConditionalBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
tiletype::Char='3', blendIn::Bool=false, DeathCount::Integer=25, DisappearOnDeaths::Bool=true)

const placements = Ahorn.PlacementDict(
    "Condition Block (Death Counter) (Viv's Helper)" => Ahorn.EntityPlacement(
        DeathConditionalBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::DeathConditionalBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::DeathConditionalBlock) = 8, 8
Ahorn.resizable(entity::DeathConditionalBlock) = true, true

Ahorn.selection(entity::DeathConditionalBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::DeathConditionalBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, alpha=0.7)

end
