module VivHelperFunctionRefillAppender
using ..Ahorn, Maple

@mapdef Entity "VivHelper/FunctionRefillAppender" FRA(x::Integer, y::Integer, width::Integer=8, height::Integer=8, Types::String="", all::Bool=true, DashesLogic::String="D", StaminaLogic::String="D")

const placements = Ahorn.PlacementDict(
    "Dash/Stamina Refill OnCollect Modifier (Viv's Helper)" => Ahorn.EntityPlacement(
        FRA,
        "rectangle"
    )
)

Ahorn.resizable(entity::FRA) = true, true
Ahorn.minimumSize(entity::FRA) = 8,8

Ahorn.selection(entity::FRA) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FRA, room::Maple.Room)
    Ahorn.drawRectangle(ctx, 0, 0, entity.data["width"], entity.data["height"], (0.4,0.4,0.4,0.4))
    Ahorn.drawCenteredText(ctx, "Refill OnCollect Modifier (Viv's Helper)", 0,0 ,entity.data["width"], entity.data["height"])
end

end