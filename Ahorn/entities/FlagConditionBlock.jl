module VivHelperFlagConditionBlock
using ..Ahorn, Maple
#Added Legacy Functionality
@mapdef Entity "VivHelper/FlagConditionBlock" FlagConditionBlock(x::Integer, y::Integer,
width::Integer=8, height::Integer=8, tileType::String="3", Flag::String="", blendIn::Bool=true)

@mapdef Entity "VivHelper/FlagConditionBlock2" FlagConditionBlock2(x::Integer, y::Integer,
width::Integer=8, height::Integer=8, tiletype::String="3", Flag::String="", blendIn::Bool=true, InvertFlag::Bool=false, IgnoreStartVal::Bool=true, StartVal::Bool=false)


const placements = Ahorn.PlacementDict(
    "Condition Block (Flag Activated) (Viv's Helper)" => Ahorn.EntityPlacement(
        FlagConditionBlock2,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

const FCB = Union{FlagConditionBlock, FlagConditionBlock2};

Ahorn.editingOptions(entity::FlagConditionBlock) = Dict{String, Any}(
    "tileType" => Ahorn.tiletypeEditingOptions()
)
Ahorn.editingOptions(entity::FlagConditionBlock2) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::FCB) = 8, 8
Ahorn.resizable(entity::FCB) = true, true
Ahorn.selection(entity::FCB) = Ahorn.getEntityRectangle(entity)
Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FCB, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, alpha=0.7)

end
