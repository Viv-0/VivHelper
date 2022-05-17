module VivHelperPolygonKillbox
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/PolygonKillbox" PolygonKillbox(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Polygon Killbox (Viv's Helper BETA)" => Ahorn.EntityPlacement(
        PolygonKillbox,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [
                (Int(entity.data["x"]) + 12, Int(entity.data["y"]) - 18),
                (Int(entity.data["x"]) + 48, Int(entity.data["y"]))
            ]
        end
    )
)
Ahorn.nodeLimits(entity::PolygonKillbox) = 2, -1

function Ahorn.selection(entity::PolygonKillbox)
    x, y = Ahorn.position(entity)
    res = [Ahorn.Rectangle(x-3,y-3,6,6)]
    l = x
    t = y
    r = x
    b = y
    for n in get(entity.data, "nodes", ())
        nx,ny = Int.(n)
        push!(res, Ahorn.Rectangle(nx-3,ny-3,6,6))
        if nx < l
            l = nx
        elseif nx > r
            r = nx
        end
        if ny < t
            t = ny
        elseif ny > b
            b = ny
        end
    end
    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::PolygonKillbox, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", nothing)
    if nodes !== nothing
        w = deepcopy(nodes)
        pushfirst!(w,(x,y))
        push!(w,(x,y))
        Ahorn.drawLines(ctx, w, Ahorn.colors.selection_selected_bc; thickness=2, filled=true, fc=Ahorn.colors.selection_selected_bc)
    end 
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::PolygonKillbox, room::Maple.Room)
    x,y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", nothing)
    if nodes !== nothing
        w = deepcopy(nodes)
        pushfirst!(w,(x,y))
        push!(w,(x,y))
        cX, cY = VivHelper.getPolygonCentroid(w)
        Ahorn.drawLines(ctx, w, (0.2,0.6,0.6,0.6); thickness=2, filled=true, fc=(0.2,0.6,0.6,0.4))
        l = x
        t = y
        r = x
        b = y
        for node in nodes
            nx,ny = Int.(node)
            if nx < l
                l = nx
            elseif nx > r
                r = nx
            end
            if ny < t
                t = ny
            elseif ny > b
                b = ny
            end
            Ahorn.drawCircle(ctx, nx, ny, 1.5, VivHelper.polygonPointColor)
        end
        Ahorn.drawCircle(ctx, x, y, 1.5, (1.0, 0.9411764705882353, 0.9607843137254902, 0.25))
        Ahorn.drawCenteredText(ctx, "Polygon Killbox", cX-16, cY-16, 32, 32)
    end
end

end