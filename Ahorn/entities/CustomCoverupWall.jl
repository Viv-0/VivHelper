module VivHelperCustomCoverupWall
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CustomCoverupWall" CustomCoverupWall(x::Integer, y::Integer,
width::Integer=8, height::Integer=8, tiletype::String="3", alpha::Number=1.0,
Depth::Integer=-13000, flag::String="", inverted::Bool=false, instant::Bool=true, RenderPlayerOver::Bool=false)

const placements = Ahorn.PlacementDict(
    "Coverup Wall (Customizable) (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomCoverupWall,
        "rectangle",
        Dict{String, Any}("blendIn" => true),
        Ahorn.tileEntityFinalizer
    ),
)
Ahorn.editingOptions(entity::CustomCoverupWall) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
    "Depth" => VivHelper.Depths,
    "instant" => Dict{String, Bool}("Instant" => true, "Gradual" => false)
)
Ahorn.minimumSize(entity::CustomCoverupWall) = 8, 8
Ahorn.resizable(entity::CustomCoverupWall) = true, true
Ahorn.selection(entity::CustomCoverupWall) = Ahorn.getEntityRectangle(entity)
Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomCoverupWall, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, alpha=0.7 * clamp(get(entity.data, "alpha", 1.0), 0.4, 1.0))
end
