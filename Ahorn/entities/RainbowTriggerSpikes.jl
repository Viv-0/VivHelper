module VivHelperRainbowTriggerSpikes
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/RainbowTriggerSpikesUp" RainbowTriggerSpikesUp(x::Integer, y::Integer, width::Integer=Maple.defaultSpikeWidth, type::String="default", Color::String="", Grouped::Bool=false, DoNotAttach::Bool=false)
@mapdef Entity "VivHelper/RainbowTriggerSpikesDown" RainbowTriggerSpikesDown(x::Integer, y::Integer, width::Integer=Maple.defaultSpikeWidth, type::String="default", Color::String="", Grouped::Bool=false, DoNotAttach::Bool=false)
@mapdef Entity "VivHelper/RainbowTriggerSpikesLeft" RainbowTriggerSpikesLeft(x::Integer, y::Integer, height::Integer=Maple.defaultSpikeHeight, type::String="default", Color::String="", Grouped::Bool=false, DoNotAttach::Bool=false)
@mapdef Entity "VivHelper/RainbowTriggerSpikesRight" RainbowTriggerSpikesRight(x::Integer, y::Integer, height::Integer=Maple.defaultSpikeHeight, type::String="default", Color::String="", Grouped::Bool=false, DoNotAttach::Bool=false)

const placements = Ahorn.PlacementDict()

const triggerEntities = Dict{String, Type}(
    "up" => RainbowTriggerSpikesUp,
    "down" => RainbowTriggerSpikesDown,
    "left" => RainbowTriggerSpikesLeft,
    "right" => RainbowTriggerSpikesRight,
)

const spikeTypes = Dict{String, String}(
    "default" => "default",
    "outline" => "outline",
    "cliffside" => "whitereflection",
    "reflection" => "whitereflection"
)


const spikesUnion = Union{RainbowTriggerSpikesUp, RainbowTriggerSpikesDown, RainbowTriggerSpikesLeft, RainbowTriggerSpikesRight}

for variant in keys(spikeTypes)
    for (dir, entity) in triggerEntities
        key = "Rainbow Trigger Spikes ($(uppercasefirst(dir)), $(uppercasefirst(variant))) (Viv's Helper)"
        placements[key] = Ahorn.EntityPlacement(
            entity,
            "rectangle",
            Dict{String, Any}(
                "type" => variant
            )
        )
    end
end

Ahorn.editingOptions(entity::spikesUnion) = Dict{String, Any}(
    "type" => spikeTypes,
    "Grouped" => Dict{String, Bool}(
        "Individual" => false,
        "Grouped (Requires Max's Helping Hand)" => true
    ),
    "Color" => VivHelper.XNAColors
)

const directions = Dict{String, String}(
    "VivHelper/RainbowTriggerSpikesUp" => "up",
    "VivHelper/RainbowTriggerSpikesDown" => "down",
    "VivHelper/RainbowTriggerSpikesLeft" => "left",
    "VivHelper/RainbowTriggerSpikesRight" => "right"
)

const offsets = Dict{String, Tuple{Integer, Integer}}(
    "up" => (4, -4),
    "down" => (4, 4),
    "left" => (-4, 4),
    "right" => (4, 4),
)

const triggerOriginalOffsets = Dict{String, Tuple{Integer, Integer}}(
    "up" => (0, 5),
    "down" => (0, -4),
    "left" => (5, 0),
    "right" => (-4, 0),
)

const rotations = Dict{String, Number}(
    "up" => 0,
    "right" => pi / 2,
    "down" => pi,
    "left" => pi * 3 / 2
)

const triggerRotationOffsets = Dict{String, Tuple{Number, Number}}(
    "up" => (3, -1),
    "right" => (4, 3),
    "down" => (5, 5),
    "left" => (-1, 4),
)

const resizeDirections = Dict{String, Tuple{Bool, Bool}}(
    "up" => (true, false),
    "down" => (true, false),
    "left" => (false, true),
    "right" => (false, true),
)

const triggerOriginalNames = String["VivHelper/RainbowTriggerSpikesDown", "VivHelper/RainbowTriggerSpikesLeft", "VivHelper/RainbowTriggerSpikesRight", "VivHelper/RainbowTriggerSpikesUp"]

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::spikesUnion)
    direction = get(directions, entity.name, "up")
    theta = rotations[direction] - pi / 2

    width = Int(get(entity.data, "width", 0))
    height = Int(get(entity.data, "height", 0))

    x, y = Ahorn.position(entity)
    cx, cy = x + floor(Int, width / 2) - 8 * (direction == "left"), y + floor(Int, height / 2) - 8 * (direction == "up")

    Ahorn.drawArrow(ctx, cx, cy, cx + cos(theta) * 24, cy + sin(theta) * 24, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.selection(entity::spikesUnion)
    if haskey(directions, entity.name)
        x, y = Ahorn.position(entity)

        width = Int(get(entity.data, "width", 8))
        height = Int(get(entity.data, "height", 8))

        direction = get(directions, entity.name, "up")
        variant = get(entity.data, "type", "default")
        width = Int(get(entity.data, "width", 8))
        height = Int(get(entity.data, "height", 8))

        ox, oy = offsets[direction]

        return Ahorn.Rectangle(x + ox - 4, y + oy - 4, width, height)
    end
end

function Ahorn.minimumSize(entity::spikesUnion)
    if haskey(directions, entity.name)
        variant = get(entity.data, "type", "default")

        return variant == "tentacles" ? (16, 16) : (8, 8)
    end
end

function Ahorn.resizable(entity::spikesUnion)
    variant = get(entity.data, "type", "default")
    direction = get(directions, entity.name, "up")

    return resizeDirections[direction]
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::spikesUnion)
    variant = spikeTypes[get(entity.data, "type", "default")]
    direction = get(directions, entity.name, "up")
    triggerOriginalOffset = entity.name in triggerOriginalNames ? triggerOriginalOffsets[direction] : (0, 0)
    oneColor = get(entity.data, "Color", "")
    oneColor = VivHelper.ColorFix((oneColor == "" ? "White" : oneColor), 1.0)
    width = get(entity.data, "width", 8)
    height = get(entity.data, "height", 8)
    for ox in 0:8:width - 8, oy in 0:8:height - 8
        drawX = ox + offsets[direction][1] + triggerOriginalOffset[1]
        drawY = oy + offsets[direction][2] + triggerOriginalOffset[2]
        Ahorn.drawSprite(ctx, "danger/spikes/$(variant)_$(direction)00", drawX, drawY; tint=oneColor)
    end
end

end
