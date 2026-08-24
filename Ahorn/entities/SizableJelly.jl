module VH_SizableJellyfish

using ..Ahorn, Maple

@mapdef Entity "VivHelper/SizableJelly" SizableJelly(x::Integer, y::Integer, width::Integer=16, height::Integer=16, bubble::Bool=false, tutorial::Bool=false, scaleGrabBoxWithHitbox::Bool=true)

const placements = Ahorn.PlacementDict(
    "Sizable Jellyfish" => Ahorn.EntityPlacement(
        SizableJelly
    ),
    "Sizable Jellyfish (Floating)" => Ahorn.EntityPlacement(
        SizableJelly,
        "point",
        Dict{String, Any}(
            "bubble" => true
        )
    )
)

Ahorn.resizable(entity::SizableJelly)

sprite = "objects/glider/idle0"

function Ahorn.selection(entity::SizableJelly)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x,y, get(entity, "width", 8), get(entity, "height", 8))
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SizableJelly, room::Maple.Room)
    x, y = Ahorn.position(entity)
    w, h = get(entity, "width", 8),get(entity, "height", 8)
    Ahorn.drawSprite(ctx, sprite, x + w/2, y+h/2; jx=0.5, jy=0.5, sx=w/8, sy=h/8)

    if get(entity, "bubble", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end

end