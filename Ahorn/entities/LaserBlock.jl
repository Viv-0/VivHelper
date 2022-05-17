module VivHelperLaserBlock
using ..Ahorn, Maple

@mapdef Entity "VivHelper/LaserBlock" LaserBlock(x::Integer, y::Integer, Direction::Integer=1,
    Directory::String="VivHelper/laserblock/techno", ChargeTime::Number=1.4, ActiveTime::Number=0.12,
    Delay::Number=1.4, StartDelay::Number=-1.0, AttachToSolid::Bool=false, Muted::Bool=false,
    StartShooting::Bool=true, Flag::String="", DarknessAlpha::Number=0.35
)

@mapdef Entity "VivHelper/LaserBlockMoving" LaserBlockM(x::Integer, y::Integer, Direction::Integer=1,
    Directory::String="VivHelper/laserblock/techno", ChargeTime::Number=1.4, ActiveTime::Number=0.12,
    Delay::Number=1.4, StartDelay::Number=-1.0, Muted::Bool=false,
    StartShooting::Bool=true, Flag::String="",
    nodes::Array{1, Tuple{Integer, Integer}}=[()],
    MoveTime::Number=1.0, MoveFlag::String="", StartMoving::Bool=true
    )

const Directions = Dict{String, Integer}(
    "Right" => 1,
    "Up" => 2,
    "Up, Right" => 3,
    "Left" => 4,
    "Left, Right" => 5,
    "Left, Up" => 6,
    "Left, Up, Right" => 7,
    "Down" => 8,
    "Down, Right" => 9,
    "Down, Up" => 10,
    "Down, Up, Right" => 11,
    "Down, Left" => 12,
    "Down, Left, Right" => 13,
    "Down, Left, Up" => 14,
    "All Directions" => 15
)

const placements = Ahorn.PlacementDict(
    "Laser Block (Viv's Helper)" => Ahorn.EntityPlacement(
        LaserBlock,
        "rectangle"
    )
)



Ahorn.editingOptions(entity::LaserBlock) = Dict{String, Any}(
    "Direction" => Dict{String, Integer}(
        "Right" => 1,
        "Up" => 2,
        "Up, Right" => 3,
        "Left" => 4,
        "Left, Right" => 5,
        "Left, Up" => 6,
        "Left, Up, Right" => 7,
        "Down" => 8,
        "Down, Right" => 9,
        "Down, Up" => 10,
        "Down, Up, Right" => 11,
        "Down, Left" => 12,
        "Down, Left, Right" => 13,
        "Down, Left, Up" => 14,
        "All Directions" => 15
    ),
    "StartDelay" => Dict{String, Number}("Normal Delay" => -1.0),
    "Directory" => String["VivHelper/laserblock/techno","VivHelper/laserblock/templeA","VivHelper/laserblock/templeB"]
)

Ahorn.resizable(entity::LaserBlock) = false, false
Ahorn.nodeLimits(entity::LaserBlock) = 0, 0

function Ahorn.selection(entity::LaserBlock)
    x, y = Ahorn.position(entity);
    x = x+8;
    y = y+8;
    z = get(entity.data, "Direction", 1);
    sprite = string(get(entity.data, "Directory", "VivHelper/laserblock/techno"),(z < 10 ? "0" : ""), z);
    return Ahorn.getSpriteRectangle(sprite, x, y);
end

function Ahorn.selection(entity::LaserBlock)
    x, y = Ahorn.position(entity);
    x = x+8;
    y = y+8;
    z = get(entity.data, "Direction", 1);
    sprite = string(get(entity.data, "Directory", "VivHelper/laserblock/techno"),(z < 10 ? "0" : ""), z);
    return Ahorn.getSpriteRectangle(sprite, x, y);
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LaserBlock, room::Maple.Room)
    z = get(entity.data, "Direction", 1);
    sprite = string(get(entity.data, "Directory", "VivHelper/laserblock/techno"),(z < 10 ? "0" : ""), z);
    if (z&1) > 0
        Ahorn.drawArrow(ctx, 8, 8, 24, 8, (1.0, 0.0, 0.0, 0.35), headLength=0)
    end
    if (z&2) > 0
        Ahorn.drawArrow(ctx, 8, 8, 8, -8, (1.0, 0.0, 0.0, 0.35), headLength=0)
    end
    if (z&4) > 0
        Ahorn.drawArrow(ctx, 8, 8, -8, 8, (1.0, 0.0, 0.0, 0.35), headLength=0)
    end
    if (z&8) > 0
        Ahorn.drawArrow(ctx, 8, 8, 8, 24, (1.0, 0.0, 0.0, 0.35), headLength=0)
    end


    Ahorn.drawSprite(ctx, sprite, 8, 8)
end

end
