module VivHelperAnimatedSpikes
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/AnimatedSpikesUp" AnimatedSpikesUp(x::Integer, y::Integer, width::Integer=Maple.defaultSpikeWidth, directory::String="tentacles", Color::String="White")
@mapdef Entity "VivHelper/AnimatedSpikesDown" AnimatedSpikesDown(x::Integer, y::Integer, width::Integer=Maple.defaultSpikeWidth, directory::String="tentacles", Color::String="White")
@mapdef Entity "VivHelper/AnimatedSpikesLeft" AnimatedSpikesLeft(x::Integer, y::Integer, height::Integer=Maple.defaultSpikeHeight, directory::String="tentacles", Color::String="White")
@mapdef Entity "VivHelper/AnimatedSpikesRight" AnimatedSpikesRight(x::Integer, y::Integer, height::Integer=Maple.defaultSpikeHeight, directory::String="tentacles", Color::String="White")


const placements = Ahorn.PlacementDict();

const spikes = Dict{String, Type}(
    "up" => AnimatedSpikesUp,
    "down" => AnimatedSpikesDown,
    "left" => AnimatedSpikesLeft,
    "right" => AnimatedSpikesRight,
)

const animSpikesUnion = Union{AnimatedSpikesUp, AnimatedSpikesDown, AnimatedSpikesLeft, AnimatedSpikesRight}

for (dir, entity) in spikes
        key = "~ Animated Spikes ($(uppercasefirst(dir))) (LEGACY) (Viv's Helper)"
        placements[key] = Ahorn.EntityPlacement(
            entity,
            "rectangle",
        )
end
const directions = Dict{String, String}(
    "VivHelper/AnimatedSpikesUp" => "up",
    "VivHelper/AnimatedSpikesDown" => "down",
    "VivHelper/AnimatedSpikesLeft" => "left",
    "VivHelper/AnimatedSpikesRight" => "right",
)

Ahorn.editingOptions(entity::animSpikesUnion) = Dict{String, Any}(
    "type" => String["tentacles"],
    "Color" => VivHelper.XNAColors
)

const rotations = Dict{String, Number}(
    "up" => 0,
    "right" => pi / 2,
    "down" => pi,
    "left" => pi * 3 / 2
)

const rotationOffsets = Dict{String, Tuple{Number, Number}}(
    "up" => (0.5, 0.25),
    "right" => (1, 0.675),
    "down" => (1.5, 1.125),
    "left" => (0, 1.675)
)

const resizeDirections = Dict{String, Tuple{Bool, Bool}}(
    "up" => (true, false),
    "down" => (true, false),
    "left" => (false, true),
    "right" => (false, true),
)

const offsets = Dict{String, Tuple{Integer, Integer}}(
    "up" => (4, -4),
    "down" => (4, 4),
    "left" => (-4, 4),
    "right" => (4, 4),
)

const spikeNames = String["spikesDown", "spikesLeft", "spikesRight", "spikesUp"]

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::animSpikesUnion)
    direction = get(directions, entity.name, "up")
    theta = rotations[direction] - pi / 2

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    x, y = Ahorn.position(entity)
    cx, cy = x + floor(Int, width / 2) - 8 * (direction == "left"), y + floor(Int, height / 2) - 8 * (direction == "up")

    Ahorn.drawArrow(ctx, cx, cy, cx + cos(theta) * 24, cy + sin(theta) * 24, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.selection(entity::animSpikesUnion)
    if haskey(directions, entity.name)
        x, y = Ahorn.position(entity)

        width = Int(get(entity.data, "width", 8))
        height = Int(get(entity.data, "height", 8))

        direction = get(directions, entity.name, "up")

        ox, oy = offsets[direction]
        return Ahorn.Rectangle(x + ox - 4, y + oy - 4, width, height)
    end
end

Ahorn.minimumSize(entity::animSpikesUnion) = (8,8)

Ahorn.resizable(entity::AnimatedSpikesUp) = (true, false)
Ahorn.resizable(entity::AnimatedSpikesDown) = (true, false)
Ahorn.resizable(entity::AnimatedSpikesLeft) = (false, true)
Ahorn.resizable(entity::AnimatedSpikesRight) = (false, true)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::animSpikesUnion)
        direction = get(directions, entity.name, "up")
        oneColor = get(entity.data, "Color", "")
        oneColor = VivHelper.ColorFix((oneColor == "" ? "White" : oneColor), 1.0)
        width = get(entity.data, "width", 8)
        height = get(entity.data, "height", 8)
        str = "danger/spikes/default_$(direction)00";
        for ox in 0:8:width - 8, oy in 0:8:height - 8
            drawX = ox + offsets[direction][1]
            drawY = oy + offsets[direction][2]

            Ahorn.drawSprite(ctx, str, drawX, drawY; tint=oneColor)

        end
        if direction == "up"
            px, py = (width/2, -16)
            qx, qy = (width/2, 4)
        elseif direction == "down"
            px, py = (width/2, 16)
            qx, qy = (width/2, -4)
        elseif direction == "left"
            px, py = (-16, height/2)
            qx, qy = (4, height/2)
        else
            px, py = (16, height/2)
            qx, qy = (-4, height/2)
        end
        Ahorn.drawCenteredText(ctx, "Animated", px, py, 4, 4; tint=(1.0,0.0,0.0,0.5))
end

end
