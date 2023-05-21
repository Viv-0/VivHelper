module VivHelperCrystalBombDetonatorColorController
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CrystalBombDetonatorController" CBDC(x::Integer, y::Integer,
BaseColor::String="6a0dad", ParticleColor::String="ffff00",
ParticleAngle::Number=270, SolidOnRelease::Bool=true,
Persistent::Bool=false)

# Constants
const colors = sort(collect(keys(Ahorn.XNAColors.colors)))

function ColorFix(v, alpha::Float64)
    if v in colors
        w = get(Ahorn.XNAColors.colors, v, (1.0, 1.0, 1.0, 1.0))
        return (w[1], w[2], w[3], alpha)
    else
        temp = Ahorn.argb32ToRGBATuple(parse(Int, v, base=16))[1:3] ./ 255
        color = (temp[1], temp[2], temp[3], alpha)
        return color
    end
    return (1.0, 1.0, 1.0, 1.0)
end


const placements = Ahorn.PlacementDict(
    "Crystal Bomb Detonator Color Controller (Viv's Helper)" => Ahorn.EntityPlacement(
        CBDC,
        "point"
    )
)

Ahorn.editingOptions(entity::CBDC) = Dict{String, Any}(
    "BaseColor" => colors,
    "ParticleColor" => colors,
    "ParticleAngle" => Dict{String, Number}(
        "Down (Default)" => 270.0,
        "Right" => 0.0,
        "Up" => 90.0,
        "Left" => 180.0
    )
)
Ahorn.selection(entity::CBDC) = Ahorn.Rectangle(get(entity.data, "x", 12)-12, get(entity.data, "y", 12)-12, 24, 24)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CBDC, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/VivHelper/CBDC.png", 0, 0)
    color = [ColorFix(get(entity.data, "BaseColor", "6a0dad"), 0.5), ColorFix(get(entity.data, "ParticleColor", "ffff00"), 0.25)]
    Ahorn.drawRectangle(ctx, -7, 2, 14, 6, color[1], color[2])
end

end
