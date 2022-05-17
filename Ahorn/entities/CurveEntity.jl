module VivHelperCurveEntity
using ..Ahorn, Maple
using Ahorn.VivHelper

@pardef CurveEntity(
    x1::Integer, y1::Integer,
    width::Integer=8, height::Integer=8,
    CurvesNumberOfPoints::String="All Simple",
    Spline::Bool=false,
    Identifier::String="curvedPath1",
    AhornCurveRenderColor::String="000000", AhornLineRenderColor::String="add8e6",
    AhornPointRenderColor::String="0000ff"
) = Entity("VivHelper/CurveEntity", x=x1, y=y1, nodes=Tuple{Int, Int}[], width=width, height=height, CurvesNumberOfPoints=CurvesNumberOfPoints, Spline=Spline, Identifier=Identifier, AhornCurveRenderColor=AhornCurveRenderColor, AhornLineRenderColor=AhornLineRenderColor, AhornPointRenderColor=AhornPointRenderColor)

const placements = Ahorn.PlacementDict(
    "Curve Entity (Viv's Helper)" => Ahorn.EntityPlacement(
        CurveEntity,
        "rectangle",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + 16, Int(entity.data["y"])), (Int(entity.data["x"]) + 32, Int(entity.data["y"]) + 16)]
        end
    )
)

Ahorn.editingOptions(entity::CurveEntity) = Dict{String, Any}(
    "CurvesNumberOfPoints" => String["All Simple", "All Cubic", "Automatic"],
    "AhornCurveRenderColor" => VivHelper.XNAColors,
    "AhornLineRenderColor" => VivHelper.XNAColors,
    "AhornPointRenderColor" => VivHelper.XNAColors
)

Ahorn.minimumSize(entity::CurveEntity) = 8, 8
Ahorn.resizable(entity::CurveEntity) = false, false
Ahorn.nodeLimits(entity::CurveEntity) = 2, -1

function Ahorn.selection(entity::CurveEntity)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.Rectangle(x-4,y-4,8,8)]

    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx-4, ny-4, 8, 8))
    end

    return res
end

const colorDefaults = [
    "000000",
    "add8e6",
    "0000ff"
]

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CurveEntity, room::Maple.Room)
    t = get(entity.data, "CurvesNumberOfPoints", "Automatic")
    x, y = Ahorn.position(entity)
    nS = Tuple{Int, Int}[(x,y)]
    nodes = get(entity.data, "nodes", ())
    for node in nodes
        nx, ny = Int.(node)

        push!(nS, (nx, ny))
    end
    b = get(entity.data, "Spline", false)
    b &= length(nS) != 3

    if t == "All Simple"
        CurveRender(ctx, entity, nS, "2", b)
    elseif t == "All Cubic"
        CurveRender(ctx, entity, nS, "3", b)
    elseif t == "Automatic"
        q = length(nS) - (b ? 0 : 1)
        r = Char[]
        while(q>2)
            if q == 3
                push!(r, '3')
            else
                push!(r, '2')
            end
            q = q - 2
        end
        CurveRender(ctx, entity, nS, String(r), b)
    else
        CurveRender(ctx, entity, nS, t, b)
    end

end

function DrawBezier3(ctx::Ahorn.Cairo.CairoContext, pts::Array{Tuple{Int64,Int64},1}, c, thickness=2)
    Ahorn.Cairo.save(ctx);
    x, y = pts[1];
    Ahorn.Cairo.move_to(ctx, x, y);
    ax, ay = pts[2]
    bx, by = pts[3]
    cx, cy = pts[4]
    Ahorn.Cairo.curve_to(ctx, ax, ay, bx, by, cx, cy)
    Ahorn.Cairo.set_antialias(ctx, thickness)
    Ahorn.Cairo.set_line_width(ctx, thickness)
    Ahorn.Cairo.set_source_rgba(ctx, c[1], c[2], c[3], c[4]);
    Ahorn.Cairo.stroke(ctx)
end
function CurveRender(ctx::Ahorn.Cairo.CairoContext, entity::CurveEntity, nS::Array{Tuple{Int, Int}, 1}, t::String="2", b::Bool=false)
    Chars = Vector{Char}(t);
    nSL = length(nS) + (b ? 1 : 0)
    curvecol = VivHelper.ColorFix(get(entity.data, "AhornCurveRenderColor", colorDefaults[1]), 1.0)
    linecol  = VivHelper.ColorFix(get(entity.data, "AhornLineRenderColor", colorDefaults[2]), 0.25)
    pointcol = VivHelper.ColorFix(get(entity.data, "AhornPointRenderColor", colorDefaults[3]), 0.6)
    i = 1
    j = 1
    if(length(Chars) != 0)
        while i < nSL
            if Chars[((j-1)%length(Chars))+1] == '2' && i + 2 <= nSL
                curve = Ahorn.SimpleCurve(nS[i], nS[((i+1)%length(nS))+1], nS[i+1])
                Ahorn.drawSimpleCurve(ctx, curve, curvecol)
            elseif Chars[((j-1)%length(Chars))+1] == '3' && i + 3 <= nSL
                DrawBezier3(ctx, [nS[i], nS[i+1], nS[i+2], nS[((i+2)%length(nS))+1]], curvecol)
                i = i + 1
            end
            i = i + 2
            j = j + 1
        end
    end
    for k in 1:nSL - 1
        Ahorn.drawLines(ctx, [nS[k], nS[(k%length(nS))+1]], linecol, thickness=1)
        Ahorn.drawCircle(ctx, nS[k][1], nS[k][2], 1.5, pointcol)
    end
    if !b
        Ahorn.drawCircle(ctx, nS[length(nS)][1], nS[length(nS)][2], 1.5, pointcol)
    end
end



end
