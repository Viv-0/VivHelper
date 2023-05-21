module VivHelperInLevelTeleporter
using ..Ahorn, Maple
using Ahorn.VivHelper

const directions = String[
    "Right",
    "Up",
    "Left",
    "Down"
]

@pardef ILTP(
    x1::Integer, y1::Integer,
    l::Integer=16,
    dir1::String="Up", dir2::String="Down",
    flags1::String="", flags2::String="",
    sO1::Number=0.0, sO2::Number=0.0,
    eO1::Bool=false, eO2::Bool=false,
    cW::Bool=false, legacy::Bool=true, center::Bool=false, OutbackHelperMod::Bool=false, allActors::Bool=false,
    NumberOfUses1::Integer=-1, NumberOfUses2::Integer=-1, Audio=""
) = Entity("VivHelper/InLevelTeleporter", x=x1, y=y1, nodes=Tuple{Int, Int}[],
       width=((dir1 == "Up" || dir1 == "Down" ) ? l : 8), height=((dir1 == "Up" || dir1 == "Down" ) ? 8 : l),
       l=l, dir1=dir1, dir2=dir2, flags1=flags1, flags2=flags2, sO1=sO1, sO2=sO2, eO1=eO1, eO2=eO2, cW=cW, NumberOfUses1=NumberOfUses1, NumberOfUses2=NumberOfUses2, Audio=Audio, legacy=legacy, center=center, OutbackHelperMod=OutbackHelperMod, allActors=allActors)

const placements = Ahorn.PlacementDict(
    "Same Room Teleporter + Entities (Viv's Helper)" => Ahorn.EntityPlacement(
        ILTP,
        "rectangle",
        Dict{String,Any}("cooldownTime" => 0.05),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"] + entity.data["l"] + 8), Int(entity.data["y"] + 24))]
        end
    ),
    "Same Room Teleporter (Skinnable) (Viv's Helper)" => Ahorn.EntityPlacement(
        ILTP,
        "rectangle",
        Dict{String,Any}(
            "Path" => "VivHelper/portal/portal",
            "Color" => "White"
        ),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"] + entity.data["l"] + 8), Int(entity.data["y"] + 24))]
        end
    )
)

Ahorn.editingOptions(entity::ILTP) = Dict{String, Any}(
    "dir1" => Dict{String, String}("Up" => "Down", "Down" => "Up", "Left" => "Right", "Right" => "Left"),
    "dir2" => Dict{String, String}("Up" => "Down", "Down" => "Up", "Left" => "Right", "Right" => "Left"),
    "Path" => String["VivHelper/portal/portal"],
    "Color" => VivHelper.XNAColors
)

Ahorn.resizable(entity::ILTP) = false, false
Ahorn.nodeLimits(entity::ILTP) = 1, 1

function getRect(dir::String, x::Int, y::Int, l::Int)
    if dir == "Right"
        return Ahorn.Rectangle(x, y, 16, l)
    elseif dir == "Down"
        return Ahorn.Rectangle(x, y, l, 16)
    elseif dir == "Left"
        return Ahorn.Rectangle(x-16, y, 16, l)
    else
        return Ahorn.Rectangle(x, y-16, l, 16)
    end

end

function Ahorn.selection(entity::ILTP)
    res = Ahorn.Rectangle[]
    node = get(entity.data, "nodes", ())[1]
    x, y = Ahorn.position(entity)
    l = Int(get(entity.data, "l", 16))
    dir = get(entity.data, "dir1", "Up")
    push!(res, getRect(dir, x, y, l))
    nx, ny = Int.(node)

    dir = get(entity.data, "dir2", "Up")
    push!(res, getRect(dir, nx, ny, l))
    return res
end

function renderTeleport(ctx::Ahorn.Cairo.CairoContext, r::Ahorn.Rectangle, c1, c2)
    Ahorn.Cairo.save(ctx)

    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1)

    Ahorn.drawRectangle(ctx, r, c1, c2)

    Ahorn.restore(ctx)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ILTP, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(get(entity.data, "nodes", ())[1])
    l = Int(get(entity.data, "l", 16))
    dir1 = get(entity.data, "dir1", "Up")
    dir2 = get(entity.data, "dir2", "Up")
    U = (dir1 == "Up" || dir1 == "Down")
    V = (dir2 == "Up" || dir2 == "Down")
    Ahorn.drawArrow(ctx, x + (U ? l / 2 : 8) - (dir1 == "Left" ? 16 : 0), y + (U ? 8 : l / 2) - (dir1 == "Up" ? 16 : 0), nx + (V ? l / 2 : 4) - (dir2 == "Left" ? 8 : 0), ny + (V ? 4 : l / 2) - (dir2 == "Up" ? 8 : 0), Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ILTP, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(get(entity.data, "nodes", ())[1])
    l = Int(get(entity.data, "l", 16))
    dir1 = get(entity.data, "dir1", "Up")
    dir2 = get(entity.data, "dir2", "Up")
    renderTeleport(ctx, getRect(dir1, x, y, l), (0.5, 0.3, 0.3, 1.0), (0.6, 0.7, 0.3, 1.0))
    renderTeleport(ctx, getRect(dir2, nx, ny, l), (0.5, 0.3, 0.3, 1.0),(0.6, 0.7, 0.3, 1.0))
    U = (dir1 == "Up" || dir1 == "Down")
    V = (dir2 == "Up" || dir2 == "Down")
    if U
        Ahorn.drawArrow(ctx, x + (U ? l / 3 : 8) - (dir1 == "Left" ? 16 : 0), y + (U ? 8 : l / 3) - (dir1 == "Up" ? 16 : 0),
                             x + (U ? l / 3 : 8) - (dir1 == "Left" ? 16 : 0), y + (U ? 8 : l / 3) - (dir1 == "Up" ? -32 : 48), (0.5, 0.3, 0.3, 1.0), headLength=5)
        Ahorn.drawArrow(ctx, x + (U ? l / 1.5 : 8) - (dir1 == "Left" ? 16 : 0), y + (U ? 8 : l / 1.5) - (dir1 == "Up" ? 16 : 0),
                             x + (U ? l / 1.5 : 8) - (dir1 == "Left" ? 16 : 0), y + (U ? 8 : l / 1.5) - (dir1 == "Up" ? -32 : 48), (0.5, 0.3, 0.3, 1.0), headLength=5)
    else
        Ahorn.drawArrow(ctx, x + (U ? l / 3 : 8) - (dir1 == "Left" ? 16 : 0), y + (U ? 8 : l / 3) - (dir1 == "Up" ? 16 : 0),
                             x + (U ? l / 3 : 8) - (dir1 == "Left" ? -32 : 48), y + (U ? 8 : l / 3) - (dir1 == "Up" ? 16 : 0), (0.5, 0.3, 0.3, 1.0), headLength=5)
        Ahorn.drawArrow(ctx, x + (U ? l / 1.5 : 8) - (dir1 == "Left" ? 16 : 0), y + (U ? 8 : l / 1.5) - (dir1 == "Up" ? 16 : 0),
                             x + (U ? l / 1.5 : 8) - (dir1 == "Left" ? -32 : 48), y + (U ? 8 : l / 1.5) - (dir1 == "Up" ? 16 : 0), (0.5, 0.3, 0.3, 1.0), headLength=5)

    end
    if V
        Ahorn.drawArrow(ctx, nx + (V ? l / 3 : 8) - (dir2 == "Left" ? 16 : 0), ny + (V ? 8 : l / 3) - (dir2 == "Up" ? 16 : 0),
                             nx + (V ? l / 3 : 8) - (dir2 == "Left" ? 16 : 0), ny + (V ? 8 : l / 3) - (dir2 == "Up" ? -32 : 48), (0.5, 0.3, 0.3, 1.0), headLength=5)
        Ahorn.drawArrow(ctx, nx + (V ? l / 1.5 : 8) - (dir2 == "Left" ? 16 : 0), ny + (V ? 8 : l / 1.5) - (dir2 == "Up" ? 16 : 0),
                             nx + (V ? l / 1.5 : 8) - (dir2 == "Left" ? 16 : 0), ny + (V ? 8 : l / 1.5) - (dir2 == "Up" ? -32 : 48), (0.5, 0.3, 0.3, 1.0), headLength=5)
    else
        Ahorn.drawArrow(ctx, nx + (V ? l / 3 : 8) - (dir2 == "Left" ? 16 : 0), ny + (V ? 8 : l / 3) - (dir2 == "Up" ? 16 : 0),
                             nx + (V ? l / 3 : 8) - (dir2 == "Left" ? -32 : 48), ny + (V ? 8 : l / 3) - (dir2 == "Up" ? 16 : 0), (0.5, 0.3, 0.3, 1.0), headLength=5)
        Ahorn.drawArrow(ctx, nx + (V ? l / 1.5 : 8) - (dir2 == "Left" ? 16 : 0), ny + (V ? 8 : l / 1.5) - (dir2 == "Up" ? 16 : 0),
                             nx + (V ? l / 1.5 : 8) - (dir2 == "Left" ? -32 : 48), ny + (V ? 8 : l / 1.5) - (dir2 == "Up" ? 16 : 0), (0.5, 0.3, 0.3, 1.0), headLength=5)

    end
end

end
