module VivHelperBumperWrapper
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/BumperWrapper" BumperWrapper(x::Integer, y::Integer,
TypeName::String="Celeste.Bumper",
RemoveBloom::Bool=false, RemoveLight::Bool=false, RemoveWobble::Bool=false,
ExplodeStrengthMultiplier::Number=1.0,
DashCooldown::Number=0.2,
NormalStateOnEnd::Bool=false,
BumperLaunchType::String="Ignore",
CoreMode::String="None",
Scale::Integer=12,
AnchorName::String="anchor",
AttachToSolid::Bool=false,
SetStamina::Integer=-1,
SetDashes::Integer=-1,
BumperBoost::Integer=0,
RespawnTime::Number=0.6
)

const placements = Ahorn.PlacementDict(

    "Bumper Wrapper (Viv's Helper) (BETA)" => Ahorn.EntityPlacement(
        BumperWrapper,
        "point"
    )
)

Ahorn.editingOptions(entity::BumperWrapper) = Dict{String, Any}(
    "BumperLaunchType" => Dict{String, String}(
        "Default" => "Ignore",
        "4-Way (Cardinal / No Diagonals)" => "Cardinal",
        "4-Way (Diagonals Only)" => "Diagonal",
        "8-Way" => "EightWay",
        "Modified 4-Way" => "Alt4way"
    ),
    "CoreMode" => Dict{String, String}(
        "Default" => "None",
        "Hot" => "Hot",
        "Cold" => "Cold"
    ),
    "DashCooldown" => Number[0.2],
    "SetStamina" => Dict{String, Integer}(
        "Ignore Bumper Refilling Stamina" => -2,
        "Default Behavior" => -1
    ),
    "SetDashes" => Dict{String, Integer}(
        "Ignore Bumper Refilling Dashes" => -2,
        "Default Behavior" => -1
    ),
    "BumperBoost" => Dict{String, Integer}(
        "Default Behavior" => 0,
        "Never Bumper Boost" => -1,
        "Better Bumper Boost" => 1,
        "Always Bumper Boost" => 2
    )
)
function Ahorn.nodeLimits(entity::BumperWrapper)
    a = get(entity.data, "useNodes", false)
    if(a)
        return 0, -1
    else
        return 0, 1
    end
end

function Ahorn.selection(entity::BumperWrapper)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())
    res = [Ahorn.Rectangle(x-6, y-6, 12, 12)]
    if !isempty(nodes)
        if get(entity.data, "useNodes", false)
            nx, ny = Int.(nodes[1])
            push!(res, Ahorn.Rectangle(nx, ny, 8, 8))
        else
            for node in nodes
                nx, ny = Int.(node)
                push!(res, Ahorn.Rectangle(nx-6, ny-6, 12, 12))
            end
        end
    end

    return res
end

sprite = "ahorn/VivHelper/bumperWrapper"

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::BumperWrapper)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    if !isempty(nodes)
        for node in nodes
            nx, ny = Int.(node)
            Ahorn.drawArrow(ctx, x, y, nx, ny, Ahorn.colors.selection_selected_fc, headLength=4)
            Ahorn.drawSprite(ctx, sprite, 2*nx, 2*ny; sx=0.5, sy=0.5)
        end
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BumperWrapper, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0; sx=0.5, sy=0.5)

end
