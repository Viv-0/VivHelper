module VivHelperBlackoutController
using ..Ahorn, Maple;

@mapdef Entity "VivHelper/BlackoutController" BlackoutController(x::Integer, y::Integer,
StartingState::String="Off", Delay::Number=3.0)

const placements = Ahorn.PlacementDict(
    "Blackout Controller (Viv's Helper)" => Ahorn.EntityPlacement(
        BlackoutController,
        "point"
    )
)

Ahorn.editingOptions(entity::BlackoutController) = Dict{String, Any}(
    "StartingState" => String["Off","On", "Flashing", "Flag"]
)

function Ahorn.selection(entity::BlackoutController)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x-16, y-16, 32, 32)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BlackoutController, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawCircle(ctx, 0, 0, 16, (1.0, 1.0, 1.0, 1.0))
    Ahorn.drawCircle(ctx, 0, 0, 14, (0.0,0.0,0.0,1.0))
    Ahorn.drawCircle(ctx, 0, 0, 12, (0.0,0.0,0.0,1.0))
    Ahorn.drawCircle(ctx, 0, 0, 10, (0.0,0.0,0.0,1.0))
    Ahorn.drawCircle(ctx, 0, 0, 8, (0.0,0.0,0.0,1.0))
    Ahorn.drawCircle(ctx, 0, 0, 6, (0.0,0.0,0.0,1.0))
    Ahorn.drawCircle(ctx, 0, 0, 4, (0.0,0.0,0.0,1.0))
    Ahorn.drawCircle(ctx, -8, -8, 2, (1.0,1.0,1.0,1.0))
    Ahorn.drawCircle(ctx, 8, -8, 2, (1.0,1.0,1.0,1.0))
    Ahorn.drawCircle(ctx, 8, 8, 2, (1.0,1.0,1.0,1.0))
    Ahorn.drawCircle(ctx, -8, 8, 2, (1.0,1.0,1.0,1.0))

end

end
