module VivHelperSolidModifier

using ..Ahorn, Maple

@mapdef Entity "VivHelper/SolidModifier" SolidModifier(x::Integer, y::Integer, width::Integer=8, height::Integer=8, Types::String="*Solid", EntitySelect::Bool=true)

const placements = Ahorn.PlacementDict(
    "~ Solid Modifier (Blank) (Viv's Helper)" => Ahorn.EntityPlacement(
        SolidModifier,
        "rectangle",
        Dict{String, Any}(
            "CornerBoostBlock" => 0,
            "TriggerOnBufferInput" => false,
            "TriggerOnTouch" => 0
        )
    ),
    "Corner Boost Block Modifier (Viv's Helper)" => Ahorn.EntityPlacement(
        SolidModifier,
        "rectangle",
        Dict{String,Any}(
            "CornerBoostBlock" => 0
        )
    ),
    "Touch Contact Modifier (Viv's Helper)" => Ahorn.EntityPlacement(
        SolidModifier,
        "rectangle",
        Dict{String,Any}(
            "TriggerOnBufferInput" => false,
            "TriggerOnTouch" => 0
        )
    )
)

Ahorn.editingOptions(entity::SolidModifier) = Dict{String, Any}(
    "Types" => String["*Celeste.Solid"],
    "CornerBoostBlock" => Dict{String, Any}(
        "Normal" => 0,
        "Corner Boost Block" => 1,
        "Retain Wall Speed" => 2
    ),
    "TriggerOnTouch" => Dict{String, Any}(
        "No Modifier" => 0,
        "On Touch" => 1,
        "On Touch + Bottom Contact" => 2
    ),
    "EntitySelect" => Dict{String, Any}(
        "All Valid Solids in Range" => true,
        "First Valid Solid in Range" => false
    )
)


Ahorn.resizable(entity::SolidModifier) = true, true
Ahorn.minimumSize(entity::SolidModifier) = 8,8

Ahorn.selection(entity::SolidModifier) = Ahorn.getEntityRectangle(entity)

sprite = "ahorn/VivHelper/solidModifierOutline.png"

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SolidModifier, room::Maple.Room)
    for x in 0:(entity["width"]/8)-1, y in 0:(entity["height"]/8)-1
        Ahorn.drawSprite(ctx, sprite, 8*x + 4, 8*y + 4; tint=(0.5, 0.1625, 0.143, 0.5))
    end
end

end