module VivHelperHoldableBarrierStuff
using ..Ahorn, Maple
using Ahorn.VivHelper

# If anyone happens to be reading this trying to learn how to code Julia stuff,
# this is the second worst formatting I've done in VivHelper's Julia code. Don't
# use this one.

# Constants

const defaultColors = [(0.353, 0.431, 0.882, 0.5), (0.353, 0.431, 0.882, 0.75)]

function GetColorFromRoom(room::Maple.Room)
    color = []
    for entity in room.entities
        if occursin("VivHelper/HoldableBarrierController", entity.name)
            color = [VivHelper.ColorFix(get(entity.data, "ParticleColor", "6a5ee1"), 0.5), VivHelper.ColorFix(get(entity.data, "EdgeColor", "6a5ee1"), 0.75)]
            break
        end
    end
    if isempty(color)
        rooms = copy(Ahorn.loadedState.side.map.rooms) # gets a shallow copy of the rooms in the current map
        #sorts the rooms by distance away from the room we're in.
        sort!(rooms, lt=(x,y)->((room.position[1] - x.position[1])*(room.position[1] - x.position[1])+(room.position[2] - x.position[2])*(room.position[2] - x.position[2])) < ((room.position[1] - y.position[1])*(room.position[1] - y.position[1]) + (room.position[2] - y.position[2])*(room.position[2] - y.position[2])), alg=QuickSort)
        for r in rooms
            for entity in room.entities
                if occursin("VivHelper/HoldableBarrierController", entity.name) && entity.data["Persistent"]
                    color = [VivHelper.ColorFix(get(entity.data, "ParticleColor", "6a5ee1"), 0.5), VivHelper.ColorFix(get(entity.data, "EdgeColor", "6a5ee1"), 0.75)]
                    break
                end
            end
        end
    end
    if isempty(color)
        color = defaultColors
    end
    return color;
end
# Holdable Barrier Controller

@mapdef Entity "VivHelper/HoldableBarrierController" HoldableBarrierController(x::Integer, y::Integer, EdgeColor::String="5a6ee1", ParticleColor::String="5a6ee1", ParticleAngle::Number=4.7124, SolidOnRelease::Bool=true, Persistent::Bool=false)
@mapdef Entity "VivHelper/HoldableBarrierController2" HoldableBarrierController2(x::Integer, y::Integer, EdgeColor::String="5a6ee1", ParticleColor::String="5a6ee1", ParticleAngle::Number=270, SolidOnRelease::Bool=true, Persistent::Bool=false)
Controllers = Union{HoldableBarrierController, HoldableBarrierController2}
# placementDict is defined later, and you cannot create two const placements
Ahorn.editingOptions(entity::HoldableBarrierController) = Dict{String, Any}(
    "EdgeColor" => VivHelper.XNAColors,
    "ParticleColor" => VivHelper.XNAColors,
    "ParticleAngle" => Dict{String, Number}(
        "Down (Default)" => 4.7124,
        "Right" => 0.0,
        "Up" => 1.571,
        "Left" => 3.1416
    )
)

Ahorn.editingOptions(entity::HoldableBarrierController2) = Dict{String, Any}(
    "EdgeColor" => VivHelper.XNAColors,
    "ParticleColor" => VivHelper.XNAColors,
    "ParticleAngle" => Dict{String, Number}(
        "Down (Default)" => 270.0,
        "Right" => 0.0,
        "Up" => 90.0,
        "Left" => 180.0
    )
)

Ahorn.selection(entity::Controllers) = Ahorn.Rectangle(get(entity.data, "x", 12)-12, get(entity.data, "y", 12)-12, 24, 24)

ControllerSprite = "ahorn/VivHelper/HBC.png"

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Controllers, room::Maple.Room)
    Ahorn.drawSprite(ctx, ControllerSprite, 0, 0)
    color = [VivHelper.ColorFix(get(entity.data, "ParticleColor", "6a5ee1"), 0.5), VivHelper.ColorFix(get(entity.data, "EdgeColor", "6a5ee1"), 0.75)]
    Ahorn.drawRectangle(ctx, -7, 2, 14, 6, color[1], color[2])
end

# Holdable Barrier

@mapdef Entity "VivHelper/HoldableBarrier" HoldableBarrier(x::Integer, y::Integer, width::Integer=8, height::Integer=8)



Ahorn.resizable(entity::HoldableBarrier) = true, true
Ahorn.minimumSize(entity::HoldableBarrier) = 8, 8

Ahorn.selection(entity::HoldableBarrier) = Ahorn.getEntityRectangle(entity)


function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::HoldableBarrier, room::Maple.Room)
    x, y = Ahorn.position(entity)
    w, h = Int(entity.width), Int(entity.height)
    colors = GetColorFromRoom(room)
    Ahorn.drawRectangle(ctx, x, y, w, h, (colors[1][1], colors[1][2], colors[1][3], 0.5), (colors[2][1], colors[2][2], colors[2][3], 0.75))
    Ahorn.drawCenteredText(ctx, "Holdable Barrier", x, y, w, h)
end

# Holdable Barrier Jump Thru
@mapdef Entity "VivHelper/HoldableBarrierJumpThru" HoldableBarrierJumpThru(x::Integer, y::Integer, width::Integer=8)

const quads = Tuple{Integer, Integer, Integer, Integer}[
    (0, 0, 8, 8) (8, 0, 8, 8) (16, 0, 8, 8);
    (0, 8, 8, 6) (8, 8, 8, 6) (16, 8, 8, 6)
]

Ahorn.resizable(entity::HoldableBarrierJumpThru) = true, false
Ahorn.minimumSize(entity::HoldableBarrierJumpThru) = 8, 0
function Ahorn.selection(entity::HoldableBarrierJumpThru)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    return Ahorn.Rectangle(x, y, width, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HoldableBarrierJumpThru, room::Maple.Room)
    x, y = Ahorn.position(entity)
    colors = GetColorFromRoom(room)

    width = Int(get(entity.data, "width", 8))

    startX = div(x, 8) + 1
    stopX = startX + div(width, 8) - 1
    startY = div(y, 8) + 1

    len = stopX - startX
    for i in 0:len
        connected = false
        qx = 2
        if i == 0
            connected = get(room.fgTiles.data, (startY, startX - 1), false) != '0'
            qx = 1

        elseif i == len
            connected = get(room.fgTiles.data, (startY, stopX + 1), false) != '0'
            qx = 3
        end

        quad = quads[2 - connected, qx]
        Ahorn.drawImage(ctx, "VivHelper/holdableJumpThru/00", 8 * i, 0, quad[1], quad[2], quad[3], quad[4]; tint=colors[1])
        Ahorn.drawImage(ctx, "VivHelper/holdableJumpThru/01", 8 * i, 0, quad[1], quad[2], quad[3], quad[4]; tint=colors[2])
    end
    
    Ahorn.drawCenteredText(ctx, "Holdable JumpThru", 0, 8, width, 12)
end


# PlacementsDict
const placements = Ahorn.PlacementDict(
    "Holdable Barrier (Viv's Helper)" => Ahorn.EntityPlacement(
        HoldableBarrier,
        "rectangle"
    ),
    "Holdable Barrier Detail Controller (Viv's Helper)" => Ahorn.EntityPlacement(
        HoldableBarrierController2,
        "point"
    ),
    "Holdable Barrier Jump Thru (Viv's Helper)" => Ahorn.EntityPlacement(
        HoldableBarrierJumpThru,
        "rectangle"
    )
)

end
