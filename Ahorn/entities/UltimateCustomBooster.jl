module VivHelperUltimateCustomBooster
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/UltimateCustomBooster" UltraCustomBooster(x::Integer, y::Integer)


const placements = Ahorn.PlacementDict(
    "Custom Booster (Preset) (Viv's Helper)" => Ahorn.EntityPlacement(
        UltraCustomBooster,
        "rectangle",
        Dict{String,Any}(
            "UsePreset" => "Default"
        )
    ),
    "Custom Booster (Max Custom) (Viv's Helper)" => Ahorn.EntityPlacement(
        UltraCustomBooster, "rectangle",
        Dict{String, Any}(
            "SuperDashSteerSpeed" => 0.0,
            "AngleOffset" => 0.0,
            "DashAngleState" => "OffsetByAngle",
            "DashDuration" => 0.15,
            "HeldDash" => false,
            "CanDashExit" => true,
            "DashRefillAmount" => -1,
            "DashRefillType" => false,
            "DashIntoSolidEffect" => "None",
            "DashSpeed" => 240.0,
            "DropHoldableOnEntry" => true,
            "ExtraParameters" => "", # not working yet
            "FastBubbleState" => "Normal",
            "FastBubbleTimer" => 0.25,
            "RefillTiming" => "BoostBegin",
            "StaminaRefillAmount" => 110.0,
            "RespawnTime" => 1.0,
            "StaminaRefillType" => false,
            "SpeedRetention" => true,
            "UseSpritesFromXML" => false,
            "SpriteReference" => "VivHelper/boosters/hiCustomBooster",
            "OutlineColor" => "000000",
            "ShineColor" => "8cf7cf",
            "BubbleColor" => "4acfc6",
            "LightBase" => "1c7856",
            "DarkBase" => "0e4a36",
            "LightCore"=> "172b21",
            "DarkCore" => "0e1c15",
            "LightPoof" => "ffffff",
            "DarkPoof" => "291c33",
            "audioOnEnter" => "event:/game/05_mirror_temple/redbooster_enter",
            "audioOnExit" =>  "event:/game/05_mirror_temple/redbooster_end",
            "audioOnBoost" => "event:/game/05_mirror_temple/redbooster_dash",
            "audioWhileDashing" => "event:/game/05_mirror_temple/redbooster_move",
            "audioOnRespawn" => "event:/game/05_mirror_temple/redbooster_reappear",
            "AddLight" => true,
            "AddBloom" => true
        )
    )
)

Ahorn.editingOptions(entity::UltraCustomBooster) = Dict{String, Any}(
    "UsePreset" => String["Default", "Superdash", "HeldDash", "FragileDash", "DoubleSpeed", "HeldSuperdash"],
    "OutlineColor" => VivHelper.XNAColors,
    "ShineColor" => VivHelper.XNAColors,
    "BubbleColor" => VivHelper.XNAColors,
    "LightBase" => VivHelper.XNAColors,
    "DarkBase" => VivHelper.XNAColors,
    "LightCore"=> VivHelper.XNAColors,
    "DarkCore" => VivHelper.XNAColors,
    "LightPoof" => VivHelper.XNAColors,
    "DarkPoof" => VivHelper.XNAColors,
    "DashIntoSolidEffect" => String["None", "Kill", "Bounce"]
)

Ahorn.editingOrder(entity::UltraCustomBooster) = String[
    "x","y", "UsePreset", "DashDuration", "HeldDash", "CanDashExit",
    "DashSpeed", "DashIntoSolidEffect", 
    "DashAngleState", "AngleOffset",
    "FastBubbleState", "FastBubbleTimer",
    "SuperDashSteerSpeed", "SpeedRetention",
    "DropHoldableOnEntry", "UseSpritesFromXML",
    "SpriteReference", "audioOnEnter",
    "audioOnExit", "audioOnBoost",
    "audioWhileDashing", "audioOnRespawn",
    "AddLight", "AddBloom",
    "OutlineColor", "ShineColor",
    "BubbleColor", "LightBase",
    "DarkBase", "LightCore", "DarkCore",
    "LightPoof", "DarkPoof", "ExtraParameters", "RefillTiming",
    "DashRefillType", "DashRefillAmount", 
    "StaminaRefillType", "StaminaRefillAmount", "RespawnTime"
]

function Ahorn.selection(entity::UltraCustomBooster)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x-8,y-8,16,16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::UltraCustomBooster, room::Maple.Room)
    colors = []
    a = get(entity.data, "UsePreset", "")
    if a != ""
        colors = getPresetColors(a)
    else
        a = get(entity.data, "ColorSet", "")
        if a != ""
            strs = split(a, ',')
            for str in strs
                push!(colors, VivHelper.ColorFix(str, 1.0))
            end
        else
            colors = [  VivHelper.ColorFix(get(entity.data, "OutlineColor", "000000")),
                        VivHelper.ColorFix(get(entity.data, "ShineColor", "8cf7cf")),
                        VivHelper.ColorFix(get(entity.data, "BubbleColor", "4acfc6")),
                        VivHelper.ColorFix(get(entity.data, "LightBase", "1c7856")),
                        VivHelper.ColorFix(get(entity.data, "DarkBase", "0e4a36")),
                        VivHelper.ColorFix(get(entity.data, "LightCore", "172b21")),
                        VivHelper.ColorFix(get(entity.data, "DarkCore", "0e1c15")),
                        VivHelper.ColorFix(get(entity.data, "LightPoof", "ffffff")),
                        VivHelper.ColorFix(get(entity.data, "DarkPoof", "291c33"))
                    ]
        end
    end
    q = size(colors, 1)
    sprite = Ahorn.getSprite(get(entity.data,"SpriteReference","VivHelper/boosters/hiCustomBooster"))
    if q > 0
        
        for i in 0:32:sprite.height
            Ahorn.drawImage(ctx, sprite.surface, -16, -16, 0, i, 32, 32; tint=colors[Int(mod(i / 32, q) + 1)])
        end
    else
        for i in 0:32:sprite.height
            j = i * 1.0
            Ahorn.drawImage(ctx, sprite.surface, -16, -16, 0, i, 32, 32; tint=VivHelper.HSV2RGBATuple(j / sprite.height, 1.0,1.0,0.9))
        end
    end
    
end

function getPresetColors(str::String)
    if str == "Default"
        return [
            VivHelper.ColorFix("000000", 1.0),
            VivHelper.ColorFix("8cf7cf", 1.0),
            VivHelper.ColorFix("4acfc6", 1.0),
            VivHelper.ColorFix("1c7856", 1.0),
            VivHelper.ColorFix("0e4a36", 1.0),
            VivHelper.ColorFix("172b21", 1.0),
            VivHelper.ColorFix("0e1c15", 1.0),
            VivHelper.ColorFix("ffffff", 1.0),
            VivHelper.ColorFix("291c33", 1.0)
        ]
    elseif str == "Superdash"
        return [
            VivHelper.ColorFix("000000", 1.0),
            VivHelper.ColorFix("5cd4ff", 1.0),
            VivHelper.ColorFix("5ca6e5", 1.0),
            VivHelper.ColorFix("005c7b", 1.0),
            VivHelper.ColorFix("003146", 1.0),
            VivHelper.ColorFix("192d33", 1.0),
            VivHelper.ColorFix("191922", 1.0),
            VivHelper.ColorFix("ffffff", 1.0),
            VivHelper.ColorFix("291c33", 1.0)
        ]
    elseif str == "FragileDash"
        return [VivHelper.ColorFix("000000", 1.0),
            VivHelper.ColorFix("ffffa7", 1.0),
            VivHelper.ColorFix("b2ef5f", 1.0),
            VivHelper.ColorFix("84912e", 1.0),
            VivHelper.ColorFix("555f1f", 1.0),
            VivHelper.ColorFix("3e3e28", 1.0),
            VivHelper.ColorFix("2e2e1f", 1.0),
            VivHelper.ColorFix("ffffff", 1.0),
            VivHelper.ColorFix("291c33", 1.0)
        ]
    elseif str == "HeldDash"
        return [VivHelper.ColorFix("000000", 1.0),
            VivHelper.ColorFix("f1ceab", 1.0),
            VivHelper.ColorFix("e5a565", 1.0),
            VivHelper.ColorFix("9c5b1a", 1.0),
            VivHelper.ColorFix("5f3f10", 1.0),
            VivHelper.ColorFix("3d230a", 1.0),
            VivHelper.ColorFix("271707", 1.0),
            VivHelper.ColorFix("ffffff", 1.0),
            VivHelper.ColorFix("291c33", 1.0)
        ]
    elseif str == "DoubleSpeed"
        return [
            VivHelper.ColorFix("000000", 1.0),
            VivHelper.ColorFix("ceffff", 1.0),
            VivHelper.ColorFix("62ffff", 1.0),
            VivHelper.ColorFix("16ad75", 1.0),
            VivHelper.ColorFix("006241", 1.0),
            VivHelper.ColorFix("0e2f1e", 1.0),
            VivHelper.ColorFix("00160b", 1.0),
            VivHelper.ColorFix("ffffff", 1.0),
            VivHelper.ColorFix("291c33", 1.0)
        ]
    elseif str == "HeldSuperdash"
        return [
            VivHelper.ColorFix("000000", 1.0),
            VivHelper.ColorFix("5cd4ff", 1.0),
            VivHelper.ColorFix("e5a565", 1.0),
            VivHelper.ColorFix("005c7b", 1.0),
            VivHelper.ColorFix("5f3f10", 1.0),
            VivHelper.ColorFix("192d33", 1.0),
            VivHelper.ColorFix("171707", 1.0),
            VivHelper.ColorFix("ffffff", 1.0),
            VivHelper.ColorFix("291c33", 1.0)
        ]
    end
    return [VivHelper.ColorFix("000000", 1.0),
    VivHelper.ColorFix("5cd4ff", 1.0),
    VivHelper.ColorFix("5ca6e5", 1.0),
    VivHelper.ColorFix("005c7b", 1.0),
    VivHelper.ColorFix("003146", 1.0),
    VivHelper.ColorFix("192d33", 1.0),
    VivHelper.ColorFix("191922", 1.0),
    VivHelper.ColorFix("ffffff", 1.0),
    VivHelper.ColorFix("291c33", 1.0)]
end

end