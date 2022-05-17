module VivHelperRefillCancelSpace
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/RefillCancelSpace" RefillCancelSpace(x::Integer, y::Integer,
width::Integer=24, height::Integer=24,
NoDash::Bool=false, NoDashRefill::Bool=false, NoStaminaRefill::Bool=false,
BrightnessTint::Number=1.0, Depth::Integer=1)

const placements = Ahorn.PlacementDict(
    "Dash Blocking Space (Viv's Helper)" => Ahorn.EntityPlacement(
        RefillCancelSpace,
        "rectangle",
        Dict{String, Any}(
            "NoDash" => true,
            "NoDashRefill" => false,
            "NoStaminaRefill" => false
        )
    ),
    "Dash Refill Blocking Space (Viv's Helper)" => Ahorn.EntityPlacement(
        RefillCancelSpace,
        "rectangle",
        Dict{String, Any}(
            "NoDash" => false,
            "NoDashRefill" => true,
            "NoStaminaRefill" => false
        )
    ),
    "Stamina Refill Blocking Space (Viv's Helper)" => Ahorn.EntityPlacement(
        RefillCancelSpace,
        "rectangle",
        Dict{String, Any}(
            "NoDash" => false,
            "NoDashRefill" => false,
            "NoStaminaRefill" => true
        )
    )
)

Ahorn.editingOptions(entity::RefillCancelSpace) = Dict{String, Any}(
    "Depth" => merge(VivHelper.Depths, Dict{String, Any}("Default" => 1))
)

Ahorn.resizable(entity::RefillCancelSpace) = true, true
Ahorn.minimumSize(entity::RefillCancelSpace) = 24, 24
Ahorn.selection(entity::RefillCancelSpace) = Ahorn.getEntityRectangle(entity)

const colorEval = Dict{Integer, Array{Float64, 1}}(
    1 => [1.0, 0.0, 0.0],
    2 => [0.0, 0.0, 1.0],
    3 => [1.0, 0.0, 1.0],
    4 => [1.0, 1.0, 0.0],
    5 => [1.0, 0.647, 0.0],
    6 => [0.0, 1.0, 0.0],
    7 => [1.0, 1.0, 1.0]
)

function ColorDeterminer(a::Bool, b::Bool, c::Bool, d::Number)
    t = 0;
    t = (a ? 1 : 0) + (b ? 2 : 0) + (c ? 4 : 0)
    color = colorEval[t];
    color[1] *= d; color[2] *= d; color[3] *= d;
    return (color[1], color[2], color[3], 0.25);
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::RefillCancelSpace, room::Maple.Room)
    x, y = Ahorn.position(entity)
    w, h = Int(entity.width), Int(entity.height)
    a = get(entity.data, "NoDash", false);
    b = get(entity.data, "NoDashRefill", false);
    c = get(entity.data, "NoStaminaRefill", false);
    d = 1.0
    Ahorn.drawRectangle(ctx, x, y, w, h, ColorDeterminer(a,b,c,d), ColorDeterminer(a,b,c,d))
    Ahorn.drawRectangle(ctx, (x + w/2) - 16, (y + h/2) - 6, 32, 12, (0.0, 0.0, 0.0, 0.67), (0.0, 0.0, 0.0, 0.67))
    colors = [(0.6, 0.6, 0.6, 0.25), (1.0, 1.0, 1.0, 1.0)]
    Ahorn.drawSprite(ctx, "VivHelper/PlayerIndicator/chevron", (x+w/2) - 12, (y + h/2); alpha=(a ? 1.0 : 0.5), tint=colors[(a ? 2 : 1)])
    Ahorn.drawSprite(ctx, "VivHelper/PlayerIndicator/triangle", (x+w/2), (y + h/2); alpha=(b ? 1.0 : 0.5), tint=colors[(b ? 2 : 1)])
    Ahorn.drawSprite(ctx, "VivHelper/PlayerIndicator/square", (x+w/2) + 12, (y + h/2); alpha=(c ? 1.0 : 0.5),tint=colors[(c ? 2 : 1)])
end

end
