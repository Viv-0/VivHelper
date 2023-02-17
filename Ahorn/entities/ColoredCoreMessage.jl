module VivHelperColoredCoreMessage
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CustomCoreMessage" CoCoM(x::Integer, y::Integer,
    TextColor1::String="White", TextColor2::String="White", dialog::String="", line::Integer=0,
    Scale::Number=1.25, RenderDistance::Number=128.0,
    EaseType::String="CubeInOut", OutlineColor::String="Black",
    AlwaysRender::Bool=false, LockPosition::Bool=false,
    DefaultFadedValue::Number=0.0, AlphaMultiplier::Number=1.0,
    RenderSpacesNumber::Integer=0, outline::Bool=false
)

const placements = Ahorn.PlacementDict(
    "Colored Core Message (Viv's Helper)" => Ahorn.EntityPlacement(
        CoCoM,
        "rectangle",
        Dict{String,Any}(),
        function(entity)
            entity.data["nodes"] = Tuple{Int,Int}[]
            x, y = entity.data["x"] + 12, entity.data["y"] + 12
            push!(entity.data["nodes"], (x - 24, y + 8))
            push!(entity.data["nodes"], (x + 24, y + 8))
        end
    )
)

Ahorn.nodeLimits(entity::CoCoM) = 0, -1

Ahorn.editingOptions(entity::CoCoM) = Dict{String, Any}(
    "TextColor1" => VivHelper.XNAColors,
    "TextColor2" => VivHelper.XNAColors,
    "OutlineColor" => VivHelper.XNAColors,
    "EaseType" => String["Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut"]
)

function Ahorn.selection(entity::CoCoM)
    x, y = Ahorn.position(entity)
    s = [Ahorn.Rectangle(x - 12, y - 12, 24, 24)]
    for n in get(entity.data, "nodes", ())
        nx, ny = Int.(n)
        push!(s, Ahorn.Rectangle(nx, ny, 8, 8))
    end
    return s
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CoCoM, room::Maple.Room)
    Ahorn.drawImage(ctx, Ahorn.Assets.speechBubble, -12, -12; tint=VivHelper.ColorFix(get(entity.data, "TextColor1", "White"), 1.0))
    nodes = get(entity.data, "nodes", ())
    px, py = Ahorn.position(entity)
    for node in nodes
        nx, ny = Int.(node)
        Ahorn.drawRectangle(ctx, nx - px, ny - py, 8, 8, VivHelper.ColorFix(get(entity.data, "TextColor1", "White"), 1.0))
        Ahorn.drawArrow(ctx, nx - px + 4, ny - py + 4, 0, 0, VivHelper.ColorFix(get(entity.data, "TextColor1", "White"), 0.3); headLength=4)
    end
end

end
