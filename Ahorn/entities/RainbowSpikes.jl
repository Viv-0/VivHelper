module VivHelperRainbowSpikes
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/RainbowSpikesUp" RainbowSpikesUp(x::Integer, y::Integer, width::Integer=Maple.defaultSpikeWidth, type::String="default", Color::String="", DoNotAttach::Bool=false)
@mapdef Entity "VivHelper/RainbowSpikesDown" RainbowSpikesDown(x::Integer, y::Integer, width::Integer=Maple.defaultSpikeWidth, type::String="default", Color::String="", DoNotAttach::Bool=false)
@mapdef Entity "VivHelper/RainbowSpikesLeft" RainbowSpikesLeft(x::Integer, y::Integer, height::Integer=Maple.defaultSpikeHeight, type::String="default", Color::String="", DoNotAttach::Bool=false)
@mapdef Entity "VivHelper/RainbowSpikesRight" RainbowSpikesRight(x::Integer, y::Integer, height::Integer=Maple.defaultSpikeHeight, type::String="default", Color::String="", DoNotAttach::Bool=false)



RainbowSpikes = Dict{String, Type}(
    "up" => RainbowSpikesUp,
    "down" => RainbowSpikesDown,
    "left" => RainbowSpikesLeft,
    "right" => RainbowSpikesRight
)

const spikeTypes = Dict{String, String}(
    "default" => "default",
    "outline" => "outline",
    "cliffside" => "whitereflection",
    "reflection" => "whitereflection"
)

RainbowSpikesUnion = Union{RainbowSpikesUp, RainbowSpikesDown, RainbowSpikesLeft, RainbowSpikesRight};

const placements = Ahorn.PlacementDict()

for variant in keys(spikeTypes)
    for (dir, entity) in RainbowSpikes
        key = "Rainbow Spikes ($(uppercasefirst(dir)), $(uppercasefirst(variant))) (Viv's Helper)"
        placements[key] = Ahorn.EntityPlacement(
            entity,
            "rectangle",
            Dict{String, Any}(
                "type" => variant
            )
        )
        key2 = "Colored Spikes ($(uppercasefirst(dir)), $(uppercasefirst(variant))) (Viv's Helper)"
        placements[key2] = Ahorn.EntityPlacement(
            entity,
            "rectangle",
            Dict{String, Any}(
                "type" => variant,
                "Color" => "White"
            )
        )
    end
end

Ahorn.editingOptions(entity::RainbowSpikesUnion) = Dict{String, Any}(
    "type" => spikeTypes,
    "Color" => VivHelper.XNAColors
)

const directions = Dict{String, String}(
    "VivHelper/RainbowSpikesUp" => "up",
    "VivHelper/RainbowSpikesDown" => "down",
    "VivHelper/RainbowSpikesLeft" => "left",
    "VivHelper/RainbowSpikesRight" => "right"
)

const offsets = Dict{String, Tuple{Integer, Integer}}(
    "up" => (4, -4),
    "down" => (4, 4),
    "left" => (-4, 4),
    "right" => (4, 4)
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
    "right" => (false, true)
)

const rainbowSpikeNames = String["RainbowSpikesDown", "RainbowSpikesLeft", "RainbowSpikesRight", "RainbowSpikesUp"]

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::RainbowSpikesUnion)
    direction = get(directions, entity.name, "up")
    theta = rotations[direction] - pi / 2

    width = Int(get(entity.data, "width", 0))
    height = Int(get(entity.data, "height", 0))

    x, y = Ahorn.position(entity)
    cx, cy = x + floor(Int, width / 2) - 8 * (direction == "left"), y + floor(Int, height / 2) - 8 * (direction == "up")

    Ahorn.drawArrow(ctx, cx, cy, cx + cos(theta) * 24, cy + sin(theta) * 24, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.selection(entity::RainbowSpikesUnion)
        x, y = Ahorn.position(entity)

        width = Int(get(entity.data, "width", 8))
        height = Int(get(entity.data, "height", 8))

        direction = get(directions, entity.name, "up")

        ox, oy = offsets[direction]

        return Ahorn.Rectangle(x + ox - 4, y + oy - 4, width, height)
end

Ahorn.minimumSize(entity::RainbowSpikesUnion) = (8,8)


Ahorn.resizable(entity::RainbowSpikesUp) = (true, false)
Ahorn.resizable(entity::RainbowSpikesDown) = (true, false)
Ahorn.resizable(entity::RainbowSpikesLeft) = (false, true)
Ahorn.resizable(entity::RainbowSpikesRight) = (false, true)


function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RainbowSpikesUnion)
        variant = get(entity.data, "type", "default")
        direction = get(directions, entity.name, "up")
        oneColor = get(entity.data, "Color", "")
        oneColor = VivHelper.ColorFix((oneColor == "" ? "White" : oneColor), 1.0)
        width = get(entity.data, "width", 8)
        height = get(entity.data, "height", 8)
        str = (variant == "cliffside" || variant == "reflection" ? "VivHelper/spikes/whitereflection_$(direction)00" : "danger/spikes/$(variant)_$(direction)00")

        for ox in 0:8:width - 8, oy in 0:8:height - 8
            drawX = ox + offsets[direction][1]
            drawY = oy + offsets[direction][2]

            Ahorn.drawSprite(ctx, str, drawX, drawY; tint=oneColor)
        end
end

for (to, from) in [(RainbowSpikesLeft, RainbowSpikesRight)]
    @eval function Ahorn.flipped(entity::$to, horizontal::Bool)
        if horizontal
            return $from(entity.x, entity.y, entity.height, entity.type, entity.Color, entity.DoNotAttach)
        end
    end

    @eval function Ahorn.flipped(entity::$from, horizontal::Bool)
        if horizontal
            return $to(entity.x, entity.y, entity.height, entity.type, entity.Color, entity.DoNotAttach)
        end
    end
end

# TODO - Rotations might need offsets

const spikesUp = [RainbowSpikesUp]
const spikesRight = [RainbowSpikesRight]
const spikesDown = [RainbowSpikesDown]
const spikesLeft = [RainbowSpikesLeft]

for (left, normal, right) in zip(spikesLeft, spikesUp, spikesRight)
    @eval function Ahorn.rotated(entity::$normal, steps::Int)
        if steps > 0
            return Ahorn.rotated($right(entity.x, entity.y, entity.width, entity.type, entity.Color, entity.DoNotAttach), steps - 1)

        elseif steps < 0
            return Ahorn.rotated($left(entity.x, entity.y, entity.width, entity.type, entity.Color, entity.DoNotAttach), steps + 1)
        end

        return entity
    end
end

for (left, normal, right) in zip(spikesUp, spikesRight, spikesDown)
    @eval function Ahorn.rotated(entity::$normal, steps::Int)
        if steps > 0
            return Ahorn.rotated($right(entity.x, entity.y, entity.height, entity.type, entity.Color, entity.DoNotAttach), steps - 1)

        elseif steps < 0
            return Ahorn.rotated($left(entity.x, entity.y, entity.height, entity.type, entity.Color, entity.DoNotAttach), steps + 1)
        end

        return entity
    end
end

for (left, normal, right) in zip(spikesRight, spikesDown, spikesLeft)
    @eval function Ahorn.rotated(entity::$normal, steps::Int)
        if steps > 0
            return Ahorn.rotated($right(entity.x, entity.y, entity.width, entity.type, entity.Color, entity.DoNotAttach), steps - 1)

        elseif steps < 0
            return Ahorn.rotated($left(entity.x, entity.y, entity.width, entity.type, entity.Color, entity.DoNotAttach), steps + 1)
        end

        return entity
    end
end

for (left, normal, right) in zip(spikesDown, spikesLeft, spikesUp)
    @eval function Ahorn.rotated(entity::$normal, steps::Int)
        if steps > 0
            return Ahorn.rotated($right(entity.x, entity.y, entity.height, entity.type, entity.Color, entity.DoNotAttach), steps - 1)

        elseif steps < 0
            return Ahorn.rotated($left(entity.x, entity.y, entity.height, entity.type, entity.Color, entity.DoNotAttach), steps + 1)
        end

        return entity
    end
end


end
