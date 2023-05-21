module VivHelperMultiContactTriggerSpikes
using ..Ahorn, Maple
 
@mapdef Entity "VivHelper/MultiContactTriggerSpikesUp" MultiContactTriggerSpikesUp(x::Integer, y::Integer, width::Integer=8, type::String="default", contactLimit::Integer=1)
@mapdef Entity "VivHelper/MultiContactTriggerSpikesDown" MultiContactTriggerSpikesDown(x::Integer, y::Integer, width::Integer=8, type::String="default", contactLimit::Integer=1)
@mapdef Entity "VivHelper/MultiContactTriggerSpikesLeft" MultiContactTriggerSpikesLeft(x::Integer, y::Integer, height::Integer=8, type::String="default", contactLimit::Integer=1)
@mapdef Entity "VivHelper/MultiContactTriggerSpikesRight" MultiContactTriggerSpikesRight(x::Integer, y::Integer, height::Integer=8, type::String="default", contactLimit::Integer=1)



const MCTriggerSpikesTypes = Dict{String, Type}(
    "up" => MultiContactTriggerSpikesUp,
    "down" => MultiContactTriggerSpikesDown,
    "left" => MultiContactTriggerSpikesLeft,
    "right" => MultiContactTriggerSpikesRight
)

const SpikeTypes = String[
    "default",
    "outline",
    "cliffside",
    "dust",
    "reflection"
]

spikesUnion = Union{MultiContactTriggerSpikesUp, MultiContactTriggerSpikesDown, MultiContactTriggerSpikesLeft, MultiContactTriggerSpikesRight}

const placements = Ahorn.PlacementDict(
    "Multi Contact Trigger Spikes (Up, Default) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesUp,
        "rectangle",
        Dict{String, Any}(
            "type" => "default"
        )
    ),
    "Multi Contact Trigger Spikes (Down, Default) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesDown,
        "rectangle",
        Dict{String, Any}(
            "type" => "default"
        )
    ),
    "Multi Contact Trigger Spikes (Left, Default) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesLeft,
        "rectangle",
        Dict{String, Any}(
            "type" => "default"
        )
    ),
    "Multi Contact Trigger Spikes (Right, Default) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesRight,
        "rectangle",
        Dict{String, Any}(
            "type" => "default"
        )
    ),
    "Multi Contact Trigger Spikes (Up, Dust) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesUp,
        "rectangle",
        Dict{String, Any}(
            "type" => "dust"
        )
    ),
    "Multi Contact Trigger Spikes (Down, Dust) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesDown,
        "rectangle",
        Dict{String, Any}(
            "type" => "dust"
        )
    ),
    "Multi Contact Trigger Spikes (Left, Dust) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesLeft,
        "rectangle",
        Dict{String, Any}(
            "type" => "dust"
        )
    ),
    "Multi Contact Trigger Spikes (Right, Dust) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesRight,
        "rectangle",
        Dict{String, Any}(
            "type" => "dust"
        )
    ),
    "Multi Contact Trigger Spikes (Up, Outline) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesUp,
        "rectangle",
        Dict{String, Any}(
            "type" => "outline"
        )
    ),
    "Multi Contact Trigger Spikes (Down, Outline) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesDown,
        "rectangle",
        Dict{String, Any}(
            "type" => "outline"
        )
    ),
    "Multi Contact Trigger Spikes (Left, Outline) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesLeft,
        "rectangle",
        Dict{String, Any}(
            "type" => "outline"
        )
    ),
    "Multi Contact Trigger Spikes (Right, Outline) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesRight,
        "rectangle",
        Dict{String, Any}(
            "type" => "outline"
        )
    ),
    "Multi Contact Trigger Spikes (Up, Cliffside) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesUp,
        "rectangle",
        Dict{String, Any}(
            "type" => "cliffside"
        )
    ),
    "Multi Contact Trigger Spikes (Down, Cliffside) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesDown,
        "rectangle",
        Dict{String, Any}(
            "type" => "cliffside"
        )
    ),
    "Multi Contact Trigger Spikes (Left, Cliffside) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesLeft,
        "rectangle",
        Dict{String, Any}(
            "type" => "cliffside"
        )
    ),
    "Multi Contact Trigger Spikes (Right, Cliffside) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesRight,
        "rectangle",
        Dict{String, Any}(
            "type" => "cliffside"
        )
    ),
    "Multi Contact Trigger Spikes (Up, Reflection) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesUp,
        "rectangle",
        Dict{String, Any}(
            "type" => "reflection"
        )
    ),
    "Multi Contact Trigger Spikes (Down, Reflection) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesDown,
        "rectangle",
        Dict{String, Any}(
            "type" => "reflection"
        )
    ),
    "Multi Contact Trigger Spikes (Left, Reflection) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesLeft,
        "rectangle",
        Dict{String, Any}(
            "type" => "reflection"
        )
    ),
    "Multi Contact Trigger Spikes (Right, Reflection) (Viv's Helper)" => Ahorn.EntityPlacement(
        MultiContactTriggerSpikesRight,
        "rectangle",
        Dict{String, Any}(
            "type" => "reflection"
        )
    )
);


Ahorn.editingOptions(entity::spikesUnion) = Dict{String, Any}(
    "type" => SpikeTypes
)

const directions = Dict{String, String}(
"VivHelper/MultiContactTriggerSpikesUp" => "up",
"VivHelper/MultiContactTriggerSpikesDown" => "down",
"VivHelper/MultiContactTriggerSpikesLeft" => "left",
"VivHelper/MultiContactTriggerSpikesRight" => "right"
)

const offsets = Dict{String, Tuple{Integer, Integer}}(
    "up" => (4, -4),
    "down" => (4, 4),
    "left" => (-4, 4),
    "right" => (4, 4)
)


const triggerSpikeOffsets = Dict{String, Tuple{Integer, Integer}}(
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

const triggerRotationOffsets = Dict{String, Tuple{Number, Number}}(
    "up" => (3, -1),
    "right" => (4, 3),
    "down" => (5, 5),
    "left" => (-1, 4),
)

const names = String["MultiContactTriggerSpikesDown", "MultiContactTriggerSpikesLeft", "MultiContactTriggerSpikesRight", "MultiContactTriggerSpikesUp"]

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

Ahorn.minimumSize(entity::spikesUnion) = (8, 8)

Ahorn.resizable(entity::MultiContactTriggerSpikesUp) = (true, false)
Ahorn.resizable(entity::MultiContactTriggerSpikesDown) = (true, false)
Ahorn.resizable(entity::MultiContactTriggerSpikesLeft) = (false, true)
Ahorn.resizable(entity::MultiContactTriggerSpikesRight) = (false, true)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::spikesUnion, room::Maple.Room)
        variant = get(entity.data, "type", "default")
        direction = get(directions, entity.name, "up")
        triggerOriginalOffset = triggerSpikeOffsets[direction]
        width = get(entity.data, "width", 8)
        height = get(entity.data, "height", 8)

        for ox in 0:8:width - 8, oy in 0:8:height - 8
            drawX = ox + offsets[direction][1] + triggerOriginalOffset[1]
            drawY = oy + offsets[direction][2] + triggerOriginalOffset[2]

            Ahorn.drawSprite(ctx, "danger/spikes/$(variant)_$(direction)00", drawX, drawY)
        end
end

for (to, from) in [(MultiContactTriggerSpikesLeft, MultiContactTriggerSpikesRight)]
    @eval function Ahorn.flipped(entity::$to, horizontal::Bool)
        if horizontal
            return $from(entity.x, entity.y, entity.height, entity.type)
        end
    end

    @eval function Ahorn.flipped(entity::$from, horizontal::Bool)
        if horizontal
            return $to(entity.x, entity.y, entity.height, entity.type)
        end
    end
end

# TODO - Rotations might need offsets

const spikesUp = [MultiContactTriggerSpikesUp]
const spikesRight = [MultiContactTriggerSpikesRight]
const spikesDown = [MultiContactTriggerSpikesDown]
const spikesLeft = [MultiContactTriggerSpikesLeft]

for (left, normal, right) in zip(spikesLeft, spikesUp, spikesRight)
    @eval function Ahorn.rotated(entity::$normal, steps::Int)
        if steps > 0
            return Ahorn.rotated($right(entity.x, entity.y, entity.width, entity.type), steps - 1)

        elseif steps < 0
            return Ahorn.rotated($left(entity.x, entity.y, entity.width, entity.type), steps + 1)
        end

        return entity
    end
end

for (left, normal, right) in zip(spikesUp, spikesRight, spikesDown)
    @eval function Ahorn.rotated(entity::$normal, steps::Int)
        if steps > 0
            return Ahorn.rotated($right(entity.x, entity.y, entity.height, entity.type), steps - 1)

        elseif steps < 0
            return Ahorn.rotated($left(entity.x, entity.y, entity.height, entity.type), steps + 1)
        end

        return entity
    end
end

for (left, normal, right) in zip(spikesRight, spikesDown, spikesLeft)
    @eval function Ahorn.rotated(entity::$normal, steps::Int)
        if steps > 0
            return Ahorn.rotated($right(entity.x, entity.y, entity.width, entity.type), steps - 1)

        elseif steps < 0
            return Ahorn.rotated($left(entity.x, entity.y, entity.width, entity.type), steps + 1)
        end

        return entity
    end
end

for (left, normal, right) in zip(spikesDown, spikesLeft, spikesUp)
    @eval function Ahorn.rotated(entity::$normal, steps::Int)
        if steps > 0
            return Ahorn.rotated($right(entity.x, entity.y, entity.height, entity.type), steps - 1)

        elseif steps < 0
            return Ahorn.rotated($left(entity.x, entity.y, entity.height, entity.type), steps + 1)
        end

        return entity
    end
end


end
