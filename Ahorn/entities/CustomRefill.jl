module VivHelperCustomRefill
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CustomRefill" CustomRefill(x::Integer, y::Integer, Scale::Number=1.0, ImageSize::Integer=16,
iStamina::Number=20.0, iLogic::Bool=false, iDashes::Integer=-1,
oStamina::Number=110.0, oDashes::Integer=-1, UseNumber::Integer=-1,
ShowColoredUseOutline::Bool=true,
ParticleSources::String="", ParticleColors::String="", WobbleType::String="YOnly",
Directory::String="objects/refill", ScaleWiggleEffect::Number=0.2,
BloomPoint::String="0.8|16", VertexLight::String="White,1,16,48",
CustomAudio::String="",
Depths::String="-100,8999", RespawnTime::Number=2.5,
FlagToggle::String=""
)

@mapdef Entity "VivHelper/CustomRefillString" CustomRefill2(x::Integer, y::Integer, Scale::Number=1.0, ImageSize::Integer=16,
iStam::String="20", iLog::String="|", iDash::String="D",
oStam::String="110", oDash::String="D", UseNum::String="-1",
ShowColoredUseOutline::Bool=true,
ParticleSources::String="", ParticleColors::String="", WobbleType::String="YOnly",
Directory::String="objects/refill", ScaleWiggleEffect::Number=0.2,
BloomPoint::String="0.8|16", VertexLight::String="White,1,16,48",
CustomAudio::String="",
Depths::String="-100,8999", RespawnTime::Number=2.5
)

const Refills = Union{CustomRefill, CustomRefill2}

const placements = Ahorn.PlacementDict(
    "Custom Refill (Simplest) (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomRefill,
        "rectangle",
        Dict{String, Any}( "simple" => 0 )
    ),
    "Custom Refill (Simple) (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomRefill,
        "rectangle",
        Dict{String, Any}( "simple" => 1 )
    ),
    "Custom Refill (Advanced) (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomRefill,
        "rectangle",
        Dict{String, Any}( "simple" => 2 )
    ),
    "Custom Refill (Complex String Version) (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomRefill2,
        "rectangle"
    )
)

function Ahorn.editingOptions(entity::CustomRefill)
    q = get(entity.data, "simple", 2)
    if q == 0
        return Dict{String, Any}(
            "iDashes" => Dict{String, Integer}("Default [< Given Dashes]" => -1),
            "iStamina" => Dict{String, Number}("Default (20)" => 20.0, "No Stamina (0)" => 0, "Full Stamina (110)"=>110),
            "oStamina" => Dict{String, Number}("Default (Full Stamina, 110)" => 110.0),
            "RespawnTime" => Dict{String, Number}("Default (2.5s)" => 2.5)
        )
    elseif q == 1
        return Dict{String, Any}(
            "iDashes" => Dict{String, Integer}("Default [< Given Dashes]" => -1),
            "iStamina" => Dict{String, Number}("Default (20)" => 20.0, "No Stamina (0)" => 0, "Full Stamina (110)"=>110),
            "oStamina" => Dict{String, Number}("Default (Full Stamina, 110)" => 110.0),
            "RespawnTime" => Dict{String, Number}("Default (2.5s)" => 2.5),
            "iLogic" => Dict{String, Bool}("Stamina Or Dashes (Default)" => false, "Stamina *and* Dashes" => true),
            "BloomPoint" => Dict{String, String}("Default" => "0.8|16"),
            "UseNumber" => Dict{String, Integer}("Infinite (Default)" => -1)
        )
    else 
        return Dict{String, Any}(
            "iDashes" => Dict{String, Integer}("Default [< Given Dashes]" => -1),
            "iStamina" => Dict{String, Number}("Default (20)" => 20.0, "No Stamina (0)" => 0, "Full Stamina (110)"=>110),
            "oStamina" => Dict{String, Number}("Default (Full Stamina, 110)" => 110.0),
            "RespawnTime" => Dict{String, Number}("Default (2.5s)" => 2.5),
            "iLogic" => Dict{String, Bool}("Stamina Or Dashes (Default)" => false, "Stamina *and* Dashes" => true),
            "BloomPoint" => Dict{String, String}("Default" => "0.8|16"),
            "WobbleType" => String["None", "YOnly (Default)", "XOnly", "Circle"],
            "UseNumber" => Dict{String, Integer}("Infinite (Default)" => -1)
        )
    end
end

function Ahorn.editingIgnored(entity::CustomRefill, multiple::Bool=false)
    simple = get(entity.data, "simple", 2);
    if simple == 0
        return multiple ? String["simple", "ShowColoredUseOutline", "ParticleSources", "ParticleColors", "Depths", "WobbleType", "ScaleWiggleEffect", "CustomAudio", "BloomPoint", "VertexLight", "UseNumber", "iLogic", "FlagToggle", "x", "y", "width", "height", "nodes"] :
                          String["simple", "ShowColoredUseOutline", "ParticleSources", "ParticleColors", "Depths", "WobbleType", "ScaleWiggleEffect", "CustomAudio", "BloomPoint", "VertexLight", "UseNumber", "iLogic", "FlagToggle"]
    elseif simple == 1
        return multiple ? String["simple", "ShowColoredUseOutline", "ParticleSources", "ParticleColors", "Depths", "WobbleType", "ScaleWiggleEffect", "x", "y", "width", "height", "nodes"] :
                          String["simple", "ShowColoredUseOutline", "ParticleSources", "ParticleColors", "Depths", "WobbleType", "ScaleWiggleEffect"];
    else
        return multiple ? String["x", "y", "simple"] : String["simple"]
    end
end


function getSprite(entity)
    s = get(entity.data, "Directory", "objects/refill")
    if endswith(s, "/")
        s = chop(s);
    end
    return string(s, "/idle00.png")
end

function Ahorn.selection(entity::Refills)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Refills, room::Maple.Room)
    sprite = getSprite(entity)
    scale = get(entity.data, "Scale", 1.0);
    imageScale = get(entity.data, "ImageScale", 1.0);
    Ahorn.drawSprite(ctx, sprite, 0, 0, sx=scale / imageScale, sy=scale / imageScale)
end

end
