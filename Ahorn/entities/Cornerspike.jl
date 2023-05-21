module VivHelperCornerspike
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CornerSpike" CornerSpike(x::Integer, y::Integer, type::String="default", Color::String="White", DoNotAttach::Bool=false)

const placements = Ahorn.PlacementDict(
    "Cornerspike (Top Left Outer) (Viv's Helper)" => Ahorn.EntityPlacement(
        CornerSpike,
        "rectangle",
        Dict{String, Any}(
            "EdgeDirection" => "UpLeft",
            "KillFormat" => false
        )
    ), 
    "Cornerspike (Top Right Outer) (Viv's Helper)" => Ahorn.EntityPlacement(
        CornerSpike,
        "rectangle",
        Dict{String, Any}(
            "EdgeDirection" => "UpRight",
            "KillFormat" => false
        )
    ),
    "Cornerspike (Bottom Left Outer) (Viv's Helper)" => Ahorn.EntityPlacement(
        CornerSpike,
        "rectangle",
        Dict{String, Any}(
            "EdgeDirection" => "DownLeft",
            "KillFormat" => false
        )
    ),
    "Cornerspike (Bottom Right Outer) (Viv's Helper)" => Ahorn.EntityPlacement(
        CornerSpike,
        "rectangle",
        Dict{String, Any}(
            "EdgeDirection" => "DownRight",
            "KillFormat" => false
        )
    ),
    "Cornerspike (Bottom Right Inner) (Viv's Helper)" => Ahorn.EntityPlacement(
        CornerSpike,
        "rectangle",
        Dict{String, Any}(
            "EdgeDirection" => "InnerUpLeft",
            "KillFormat" => false
        )
    ), 
    "Cornerspike (Bottom Left Inner) (Viv's Helper)" => Ahorn.EntityPlacement(
        CornerSpike,
        "rectangle",
        Dict{String, Any}(
            "EdgeDirection" => "InnerUpRight",
            "KillFormat" => false
        )
    ),
    "Cornerspike (Top Right Inner) (Viv's Helper)" => Ahorn.EntityPlacement(
        CornerSpike,
        "rectangle",
        Dict{String, Any}(
            "EdgeDirection" => "InnerDownLeft",
            "KillFormat" => false
        )
    ),
    "Cornerspike (Top Left Inner) (Viv's Helper)" => Ahorn.EntityPlacement(
        CornerSpike,
        "rectangle",
        Dict{String, Any}(
            "EdgeDirection" => "InnerDownRight",
            "KillFormat" => false
        )
    )
)

Ahorn.editingOptions(entity::CornerSpike) = Dict{String, Any}(
    "EdgeDirection" => Dict{String, String}(
        "Top Left" => "UpLeft",
        "Top Right" => "UpRight",
        "Bottom Left" => "DownLeft",
        "Bottom Right" => "DownRight",
        "Inner Bottom Right" => "InnerUpLeft",
        "Inner Bottom Left" => "InnerUpRight",
        "Inner Top Right" => "InnerDownLeft",
        "Inner Top Left" => "InnerDownRight"
    ),
    "Color" => VivHelper.XNAColors,
    "type" => String["default", "outline"]
)

const DirOffsets = Dict{String, Tuple{Integer, Integer}}(
    "UpLeft" => (-8, -8),
    "UpRight" => (0, -8),
    "DownLeft" => (-8, 0),
    "DownRight" => (0, 0),
    "InnerUpLeft" => (-8, -8),
    "InnerUpRight" => (0, -8),
    "InnerDownLeft" => (-8, 0),
    "InnerDownRight" => (0, 0)
)

function DirectionOffsets(entity::CornerSpike)
    a =get(entity.data, "EdgeDirection", "")
    return haskey(DirOffsets, a) ? DirOffsets[a] : (0, 0)
end

function Ahorn.selection(entity::CornerSpike)
    x, y = Ahorn.position(entity)
    a, b = DirectionOffsets(entity)
    Ahorn.Rectangle(x+a,y+b,8,8)

end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CornerSpike)
    a = get(entity.data, "EdgeDirection", "")
    b0 = get(entity.data, "type", "default")
    x, y = Ahorn.position(entity)
    if a == "" || b0 == ""
        Ahorn.drawSprite(ctx, "", x, y, jx = 0.5, jy = 0.5)
    else
        b = string(b0 == "default" ? "danger/spikes/corners/default" : b0 == "outline" ? "danger/spikes/corners/outline" : b0, "_")
        c, d = DirectionOffsets(entity)
        oneColor = get(entity.data, "Color", "")
        oneColor = VivHelper.ColorFix((oneColor == "" ? "White" : oneColor), 1.0)
        Ahorn.drawSprite(ctx, string(b,a,"00"), x+(c * 9 / 8), y+(d * 9 / 8), jx=0, jy=0, tint=oneColor)
    end
end

end