module VivHelperModulePolygonStuff
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Trigger "VivHelper/PolygonKillbox" PolygonKillbox(x::Integer, y::Integer)

@mapdef Trigger "VivHelper/PolygonTriggerTrigger" PolygonTrTr(x::Integer, y::Integer, width::Integer=8, height::Integer=8)

const placements = Ahorn.PlacementDict(
    "Polygonal Activation Trigger (Viv's Helper BETA)" => Ahorn.EntityPlacement(
        PolygonTrTr,
        "point",
        Dict{String, Any}(),
        function(trigger)
            trigger.data["nodes"] = [
                (Int(trigger.data["x"]) - 20, Int(trigger.data["y"] + 6)),
                (Int(trigger.data["x"]) - 8, Int(trigger.data["y"]) - 12),
                (Int(trigger.data["x"]) + 28, Int(trigger.data["y"]) + 6)
            ]
            trigger.data["width"] = 2
            trigger.data["height"] = 2
        end
    ),
)

Ahorn.nodeLimits(trigger::PolygonTrTr) = 3, -1

function Ahorn.triggerSelection(trigger::PolygonTrTr, room::Maple.Room, node::Int=0)
    x, y = Ahorn.position(trigger)
    res = [Ahorn.Rectangle(x-3,y-3,6,6)]
    l = typemax(Int32)
    t = typemax(Int32)
    r = typemin(Int32)
    b = typemin(Int32)
    for n in get(trigger.data, "nodes", ())
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
    trigger.data["width"] = r - l;
    trigger.data["height"] = b - t;
    return res
end

function Ahorn.renderTriggerSelection(ctx::Ahorn.Cairo.CairoContext, layer::Ahorn.Layer, trigger::PolygonTrTr, room::Maple.Room)
    x, y = Ahorn.position(trigger)
    nodes = get(trigger.data, "nodes", nothing)
    if nodes !== nothing
        w = deepcopy(nodes)
        push!(w,nodes[1])
        Ahorn.drawLines(ctx, w, Ahorn.colors.selection_selected_bc; thickness=2, filled=true, fc=Ahorn.colors.selection_selected_bc)
        cX, cY = VivHelper.getPolygonCentroid(w)
        if abs(x - cX) > 4 || abs(y - cY) > 4
            Ahorn.drawArrow(ctx, cX,cY, x,y, Ahorn.colors.selection_selected_bc, headLength=5)
        end
    end 
end

function Ahorn.renderTrigger(ctx::Ahorn.Cairo.CairoContext, layer::Ahorn.Layer, trigger::PolygonTrTr, room::Maple.Room)
    x,y = Ahorn.position(trigger)
    nodes = get(trigger.data, "nodes", nothing)
    if nodes !== nothing
        w = deepcopy(nodes)
        push!(w,nodes[1])
        cX, cY = VivHelper.getPolygonCentroid(w)
        if abs(x - cX) > 4 || abs(y - cY) > 4
            Ahorn.drawArrow(ctx, cX,cY, x,y, (0.2,0.5,0.2,0.3), headLength=5)
        end
        Ahorn.drawLines(ctx, w, Ahorn.colors.trigger_fc; thickness=2, filled=true, fc=Ahorn.colors.trigger_bc)
        l = typemax(Int32)
        t = typemax(Int32)
        r = typemin(Int32)
        b = typemin(Int32)
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
        Ahorn.drawCircle(ctx, cX, cY, 1.5, (1.0, 0.9411764705882353, 0.9607843137254902, 0.25))
        Ahorn.drawCircle(ctx, x, y, 1.5, (1.0, 0.9411764705882353, 0.9607843137254902, 0.5))
        trigger.data["width"] = r - l;
        trigger.data["height"] = b - t;
        Ahorn.drawCenteredText(ctx, "Polygon TriggerTrigger", cX-16, cY-16, 32, 32)
    end 
end



end
