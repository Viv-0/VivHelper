module VivHelperCustomHangingLamp
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CustomHangingLamp" CustomHangingLamp(x::Integer, y::Integer, height::Integer=16,
directory::String="VivHelper/customHangingLamp", AnimationSpeed::Number=0.2,
BloomAlpha::Number=1.0, BloomRadius::Integer=48,
LightAlpha::Number=1.0, LightColor::String="White",
LightFadeIn::Integer=24, LightFadeOut::Integer=48,
AudioPath::String="event:/game/02_old_site/lantern_hit",
WeightMultiplier::Number=1.0,
DrawOutline::Bool=true

)

function hangingLampFinalizer(entity::CustomHangingLamp)
    nx, ny = Int.(entity.data["nodes"][1])
    y = Int(entity.data["y"])

    entity.data["height"] = max(abs(ny - y), 8)

    delete!(entity.data, "nodes")
end

const placements = Ahorn.PlacementDict(
    "Hanging Lamp (Custom) (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomHangingLamp,
        "line",
        Dict{String, Any}(),
        hangingLampFinalizer
    )
)


Ahorn.minimumSize(entity::CustomHangingLamp) = 0, 8
Ahorn.resizable(entity::CustomHangingLamp) = false, true
Ahorn.editingOptions(entity::CustomHangingLamp) = Dict{String, Any}(
    "LightColor" => VivHelper.XNAColors,
    "AudioPath" => String["event:/game/02_old_site/lantern_hit"], # This is just a signature of my entities, I try to give Default values for long strings for custom things
    "AnimationSpeed" => Number[0.2]
)

function Ahorn.selection(entity::CustomHangingLamp)
    x, y = Ahorn.position(entity)
    height = get(entity.data, "height", 16)

    return Ahorn.Rectangle(x + 1, y, 8, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomHangingLamp, room::Maple.Room)
    height = get(entity.data, "height", 16)
    spriteDir = get(entity.data, "directory", "VivHelper/customHangingLamp");
    if endswith(spriteDir, '/')
        chop(spriteDir)
    end
    spriteDir = string(spriteDir, "/");
    
    base = Ahorn.getSprite(string(spriteDir, "base00"))
    chain = Ahorn.getSprite(string(spriteDir, "chain00"))
    lamp = Ahorn.getSprite(string(spriteDir, "lamp00"))
    Ahorn.drawCircle(ctx, 1 + lamp.width /2, height - (lamp.height / 2), clamp(get(entity.data, "LightFadeIn", 24), 0, 120), VivHelper.ColorFix(get(entity.data, "LightColor", "White"), clamp(get(entity.data, "LightAlpha", 1.0), 0.0, 1.0)))
    Ahorn.drawCircle(ctx, 1 + lamp.width /2, height - (lamp.height / 2), clamp(get(entity.data, "LightFadeOut", 48), 0, 120), VivHelper.ColorFix(get(entity.data, "LightColor", "White"), clamp(get(entity.data, "LightAlpha", 1.0), 0.0, 1.0) * 0.25))
    Ahorn.drawImage(ctx, base, 1, 0)


    for i in 0:chain.height:height - (lamp.height + base.height)
        Ahorn.drawImage(ctx, chain, 1, i)
    end

    Ahorn.drawImage(ctx, lamp, 1, height - lamp.height)
end

end
