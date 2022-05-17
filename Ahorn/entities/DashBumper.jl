module VivHelperDashBumper
using ..Ahorn, Maple

@mapdef Entity "VivHelper/DashBumper" DashBumper(x::Integer, y::Integer, Wobble::Bool=true, RespawnTime::Number=0.6, MoveTime::Number=1.81818, ReflectType::String="DashDir")

const placements = Ahorn.PlacementDict(
    "Dash Bumper (BETA) (Viv's Helper)" => Ahorn.EntityPlacement(
        DashBumper,
        "point"
    )
)

Ahorn.editingOptions(entity::DashBumper) = Dict{String, Any}(
    "ReflectType" => Dict{String, String}(
        "Reflect Dash Direction" => "DashDir",
        "4-Way Angle" => "Angle4",
        "8-Way Angle" => "Angle8",
        "Modified 4-Way Angle" => "AltAngle4"
    )
)

Ahorn.nodeLimits(entity::DashBumper) = 0, 1

function Ahorn.selection(entity::DashBumper)
    x, y = Ahorn.position(entity)
    b = 12
    res = [Ahorn.Rectangle(x-b, y-b, 2*b, 2*b)]
    nodes = get(entity.data, "nodes", ())
    if !isempty(nodes)
        nx, ny = Int.(nodes[1])

        push!(res, Ahorn.Rectangle(nx-b, ny-b, 2*b, 2*b))
    end
    return res;
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashBumper, room::Maple.Room) = Ahorn.drawSprite(ctx, "VivHelper/dashBumper/idle00", 0, 0)

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::DashBumper)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    if !isempty(nodes)
        nx, ny = Int.(nodes[1])

        theta = atan(y - ny, x - nx)
        Ahorn.drawArrow(ctx, x, y, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=4)
        Ahorn.drawSprite(ctx, "VivHelper/dashBumper/idle00", nx, ny)
    end
end

end