module VivHelperTravelingFlame
using ..Ahorn, Maple, Random, Ahorn.VivHelper

@mapdef Entity "VivHelper/TravelingFlame" TravelingFlame(x::Integer, y::Integer,
StringID::String="room-flame-1", CurveData::String="Normal", CurveGen::Int=1, RotationType::Int=1,
ColorTint::String="ffffff", LightFadePoint::Number=32.0, LightRadius::Number=48.0, LightAlpha::Number=1.0, CycleDelay::Number=0.0,
Delay::Number=0.0, SpeedMultiplier::Number=1.0, onCycle::Bool=false
)

# @mapdef Entity "VivHelper/TravelingFlameCurve" TravelingFlameCurve(x::Integer, y::Integer, StringID::String="room-flame-1",
# CurveID::String="curvedpath1", ColorTint::String="ffffff", LightFadePoint::Number=32.0, LightRadius::Number=48.0, LightAlpha::Number=1.0, CycleDelay::Number=0.0,
# Delay::Number=0.0, SpeedMultiplier::Number=1.0, onCycle::Bool=false)

function TravelingFlameFinalizer(entity::TravelingFlame)
    x, y = Ahorn.position(entity)


    entity.data["nodes"] = [(x + 16, y)]
end

# function CurveFinalizer(entity::TravelingFlameCurve)


const placements = Ahorn.PlacementDict(
    "Traveling Flame (Auto-Generated Curves) (Viv's Helper)" => Ahorn.EntityPlacement(
      TravelingFlame,
      "point",
      Dict{String, Any}(),
      TravelingFlameFinalizer
      )
#    "Traveling Flame (Using Curve Entity) (Viv's Helper)" => Ahorn.EntityPlacement(
#      TravelingFlameCurve,
#      "point",
#      CurveFinalizer
#    )
)

const sprite = "VivHelper/entities/ahornflame.png"

Ahorn.editingOptions(entity::TravelingFlame) = Dict{String, Any}(
    "ColorTint" => XNAColors,
    "CurveData" => String["Very Shallow", "Shallow", "Normal", "Deep", "Very Deep", "Radial", "Wide", "Very Wide"],
    "CurveGen" => Dict{String, Int}("Default" => 0, "Automated" => 1),
    "RotationType" => Dict{String, Int}("Alternating" => 0, "Smart" => 1, "Stupid" => -1)
  )

 # Ahorn.editingOptions(entity::TravelingFlameCurve) = Dict{String, Any}(
#      "ColorTint" => colors,
#      "CurveID" => getCurveIDs()
#)

function getCurveIDs()
    k = String[];
    entities = filter(e -> e.name == "VivHelper/CurveEntity", room.entities)
    for e in entities
        a = get(e.data, "Identifier")
        if a != nothing
            push!(k, a)
        end
    end
    return k
end


function Ahorn.selection(entity::TravelingFlame)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]

    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

Ahorn.nodeLimits(entity::TravelingFlame) = 1, -1
# Ahorn.nodeLimits(entity::TravelingFlameCurve) = 0


function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::TravelingFlame)
    px, py = Ahorn.position(entity)
    color = VivHelper.ColorFix(get(entity.data, "ColorTint", "ffffff"), 1.0)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny, tint=color)

        px, py = nx, ny
    end
    if get(entity.data, "onCycle", false)
        tx, ty = Ahorn.position(entity)
        Ahorn.drawArrow(ctx, px, py, tx, ty, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::TravelingFlame, room::Maple.Room)
    x, y = Ahorn.position(entity)
    color = VivHelper.ColorFix(get(entity.data, "ColorTint", "ffffff"), 1.0)
    Ahorn.drawSprite(ctx, sprite, x, y, tint=color)
end
end
