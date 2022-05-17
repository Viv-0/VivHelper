module VivHelperCustomPlatform
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CustomPlatform" CustomPlatform(
    x::Integer,
    y::Integer,
    width::Integer=Maple.defaultBlockWidth,
    InnerLineColor::String="2a1923",
    OuterLineColor::String="160b12",
    TexturePath::String="default",
    SpeedMod::Number=1.0,
    PathType::Bool=true,
    Identifier::String="curvedPath1",
    Reverse::Bool=false,
    UniformMovement::Bool=false,
    EaseType::String="Linear"
)

const placements = Ahorn.PlacementDict(
    "~ Curved Custom Platform (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomPlatform,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::CustomPlatform) = Dict{String, Any}(
    "TexturePath" => Maple.wood_platform_textures,
    "InnerLineColor" => VivHelper.XNAColors,
    "OuterLineColor" => VivHelper.XNAColors,
    "PathType" => Dict{String, Bool}(
        "Linear" => false,
        "Bezier" => true
    ),
    "EaseType" => String["Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut"]
)

Ahorn.resizable(entity::CustomPlatform) = true, false
Ahorn.minimumSize(entity::CustomPlatform) = 8, 0
Ahorn.nodeLimits(entity::CustomPlatform) = 0, 1

function Ahorn.selection(entity::CustomPlatform, room::Maple.Room)
    x, y =  Int(entity.data["x"]), Int(entity.data["y"])
    width = Int(get(entity.data, "width", 8))

    b = get(entity.data, "PathType", true)
    if b || isempty(get(entity.data, "nodes", ()))
        return Ahorn.Rectangle(x, y, width, 8)
    else
        nx, ny = get(entity.data, "nodes", ())[1]
        return [Ahorn.Rectangle(x, y, width, 8), Ahorn.Rectangle(nx, ny, width, 8)]
    end
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomPlatform, room::Maple.Room)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))
    b = get(entity.data, "PathType", true)
    renderPlatform(ctx, x, y, width)
    if !b
        nx, ny = get(entity.data, "nodes", (0,0))[1]
        renderPlatform(ctx, nx, ny, width)
    end

    curves = filter(f -> f.name == "VivHelper/CurveEntity", room.entities)
    d = ""
    e = nothing

    for c in curves
        d = c.data["Identifier"]
        if d == get(entity.data, "Identifier", "") && d != ""
            e = c;
            break;
        end
    end
    if(!(e == nothing))
        Ahorn.drawArrow(ctx, x + width/2, y + 4, e.data["x"], e.data["y"], (0.0, 0.0, 0.0, 0.3), headLength=4)
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomPlatform, room::Maple.Room)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))
    b = get(entity.data, "PathType", false)
    if b && !isempty(get(entity.data, "nodes", ()))
        pop!(entity.data["nodes"])
    elseif !b
        if isempty(get(entity.data, "nodes", ()))
            push!(entity.data["nodes"], (x + width + 8, y))
        end
        nx, ny = get(entity.data, "nodes", (0,0))[1]
        color1 = VivHelper.ColorFix(get(entity.data, "InnerLineColor", "2a1923"), 1.0)
        color2 = VivHelper.ColorFix(get(entity.data, "OuterLineColor", "160b12"), 1.0)
        renderConnection(ctx, x, y, nx, ny, width, color1, color2)
    end
    renderPlatform(ctx, x, y, width)
end

function renderPlatform(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number)
    tilesWidth = div(width, 8)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, "objects/woodPlatform/default", x + 8 * (i - 1), y, 8, 0, 8, 8)
    end

    Ahorn.drawImage(ctx, "objects/woodPlatform/default", x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/default", x + tilesWidth * 8 - 8, y, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/default", x + floor(Int, width / 2) - 4, y, 16, 0, 8, 8)
end

function renderConnection(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, nx::Number, ny::Number, width::Number, innerColor::Ahorn.colorTupleType, outerColor::Ahorn.colorTupleType)
    cx, cy = x + floor(Int, width / 2), y + 4
    cnx, cny = nx + floor(Int, width / 2), ny + 4

    length = sqrt((x - nx)^2 + (y - ny)^2)
    theta = atan(cny - cy, cnx - cx)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, cx, cy)
    Ahorn.rotate(ctx, theta)

    Ahorn.setSourceColor(ctx, outerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 3);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.setSourceColor(ctx, innerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.Cairo.restore(ctx)
end

end
