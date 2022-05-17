module VivHelperBaddyBoostCustomRefill
using ..Ahorn, Maple

@mapdef Entity "VivHelper/BadelineBoostNoRefill" BBNR(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[], lockCamera::Bool=false, canSkip::Bool=false, finalCh9Boost::Bool=false, finalCh9GoldenBoost::Bool=false, finalCh9Dialog::Bool=false, NoDashRefill::Bool=true, NoStaminaRefill::Bool=false)

@mapdef Entity "VivHelper/BadelineBoostCustomRefill" BBCR(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[], lockCamera::Bool=false, canSkip::Bool=false, finalCh9Boost::Bool=false, finalCh9GoldenBoost::Bool=false, finalCh9Dialog::Bool=false,
DashesLogic::String="+0", StaminaLogic::String="+0")

const placements = Ahorn.PlacementDict(
    "Badeline Boost (No Refill) (Viv's Helper)" => Ahorn.EntityPlacement(
        BBNR, "point"
    ),
    "Badeline Boost (Custom Refill Options) (Viv's Helper)" => Ahorn.EntityPlacement(
        BBCR, "point"
    )
)

GetSprite(entity::BBNR) = "ahorn/VivHelper/baddyboostnorefill"
GetSprite(entity::BBCR) = "ahorn/VivHelper/baddyboostcustomrefill"

BBR = Union{BBNR, BBCR}

Ahorn.nodeLimits(entity::BBR) = 0, -1

function Ahorn.selection(entity::BBR)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)
    
    res = Ahorn.Rectangle[Ahorn.Rectangle(x-8,y-8,16,16)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx-8,ny-8,16,16))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::BBR)
    px, py = Ahorn.position(entity)
    sprite = GetSprite(entity)
    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        theta = atan(py - ny, px - nx)
        Ahorn.drawArrow(ctx, px, py, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::BBR, room::Maple.Room)
    x, y = Ahorn.position(entity)
    sprite = GetSprite(entity)
    Ahorn.drawSprite(ctx, sprite, x, y)
end

end