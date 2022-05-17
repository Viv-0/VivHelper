module VivHelperITTP2
using ..Ahorn, Maple
using Ahorn.VivHelper

#storage optimization because funky code
@mapdef Trigger "VivHelper/ITPT1Way" ITPT1Way(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
TargetID::String="Target",
onExit::Bool=false, ExitDirection::Integer=15,
RequiredFlags::String="", FlagsOnTeleport::String="",
RoomName::String="",
ResetDashes::Bool=true,
BringHoldableThrough::Bool=false,
IgnoreNoSpawnpoints::Bool=false,
Persistence::String="None",
EndCutsceneOnWarp::Bool=false,
customDelay::Number=0.0
)

@mapdef Trigger "VivHelper/TeleportTarget" TPT(x::Integer, y::Integer, width::Integer=8, height::Integer=8, TargetID::String="Target", AddTriggerOffset::Bool=false, SetState::String="-1")

function TPFinalizer(trigger::ITPT1Way, room::Maple.Room)
    trigger.data["RoomName"] = room.name
end



const placements = Ahorn.PlacementDict(
    "New Teleport Trigger (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        ITPT1Way,
        "rectangle",
        Dict{String, Any}(
            "TransitionType" => "None"
        ),
        function(trigger, room) 
            TPFinalizer(trigger, room)
        end
    ),
    "New Teleport Trigger (Flash) (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        ITPT1Way,
        "rectangle",
        Dict{String, Any}(
            "TransitionType" => "ColorFlash",
            "FlashColor" => "White",
            "FlashAlpha" => 1.0
        ),
        function(trigger, room) 
            TPFinalizer(trigger, room)
        end
    ),
    "New Teleport Trigger (Lightning) (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        ITPT1Way,
        "rectangle",
        Dict{String, Any}(
            "TransitionType" => "Lightning",
            "LightningCount" => 2,
            "LightningOffsetRange" => "-130,130",
            "LightningDelay" => 0.0,
            "LightningMaxDelay" => 0.25,
            "Flash" => true,
            "Shake" => true
        ),
        function(trigger, room) 
            TPFinalizer(trigger, room)
        end
    ), 
    "New Teleport Trigger (Glitch) (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        ITPT1Way,
        "rectangle",
        Dict{String, Any}(
            "TransitionType" => "Glitch",
            "GlitchStrength" => 0.5,
            "StartingGlitchEase" => "Linear",
            "StartingGlitchDuration" => 0.3,
            "EndingGlitchEase" => "Linear",
            "EndingGlitchDuration" => 0.3,
            "freezeOnTeleport" => true
        ),
        function(trigger, room) 
            TPFinalizer(trigger, room)
        end
    ),
    "New Teleport Trigger (Wipe) (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        ITPT1Way,
        "rectangle",
        Dict{String, Any}(
            "TransitionType" => "Wipe",
            "freezeOnTeleport" => true,
            "wipeOnLeave" => true,
            "wipeOnEnter" => true
        ),
        function(trigger, room) 
            TPFinalizer(trigger, room)
        end
    ),
    "New Teleport Target (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        TPT, "rectangle",
        Dict{String, Any}()
    ),
    "New Teleport Target (Speed Length Change) (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        TPT, "rectangle",
        Dict{String, Any}(
            "SpeedModifier" => "Add",
            "SpeedChangeStrength" => 0.0
        )
    ),
    "New Teleport Target (Speed Change) (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        TPT, "rectangle",
        Dict{String, Any}(
            "SpeedModifier" => "Add",
            "SpeedChangeX" => 0.0,
            "SpeedChangeY" => 0.0
        )
    ),
    "New Teleport Target (Rotate) (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        TPT, "rectangle",
        Dict{String, Any}(
            "RotateToAngle"=> false,
            "RotationValue" => 0.0
        )
    ),
    "New Teleport Target (Speed Length Change, Rotate) (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        TPT, "rectangle",
        Dict{String, Any}(
            "SpeedModifier" => "Add",
            "SpeedChangeStrength" => 0.0,
            "RotateBeforeSpeedChange" => false,
            "RotateToAngle"=> false,
            "RotationValue" => 0.0
        )
    ),
    "New Teleport Target (Speed Change, Rotate) (BETA, Viv's Helper)" => Ahorn.EntityPlacement(
        TPT, "rectangle",
        Dict{String, Any}(
            "SpeedModifier" => "Add",
            "SpeedChangeX" => 0.0,
            "SpeedChangeY" => 0.0,
            "RotateBeforeSpeedChange" => false,
            "RotateToAngle"=> false,
            "RotationValue" => 0.0
        )
    )
)

Ahorn.editingOrder(trigger::ITPT1Way) =
String[ "x", "y", "width", "height", "Persistence",
"TargetID", "ExitDirection", "RequiredFlags", "FlagsOnTeleport", "RoomName", "TransitionType",
"onExit", "ResetDashes", "AddTriggerOffset", "BringHoldableThrough", "IgnoreNoSpawnpoints", 
"FlashColor", "FlashAlpha",
"LightningCount", "LightningOffsetRange", "LightningDelay", "LightningMaxDelay", "Flash", "Shake",
"GlitchStrength", "StartingGlitchDuration", "EndingGlitchDuration", "StartingGlitchEase", "EndingGlitchEase", "freezeOnTeleport",
"WipeExtraDelay"
]
Ahorn.editingOrder(trigger::TPT) =
String[ "x", "y", "TargetID", "SetState", "SpeedModifier", "SpeedChangeStrength", "SpeedChangeX", "SpeedChangeY", "RotationValue", "AddTriggerOffset", "RotateBeforeSpeedChange", "RotateToAngle", "nodes", "width", "height"]

Ahorn.editingIgnored(trigger::ITPT1Way, multiple::Bool=false) = multiple ? String["x","y","width","height","TransitionType"] : String["TransitionType"]
    #=
    if !multiple
        return String["TransitionType"]
    end
    #= Fancy code to make editingIgnored ignore all different values, is currently borked
    arr = String["TargetID", "ExitDirection", "RequiredFlags", "FlagsOnTeleport", "RoomName", "TransitionType",
    "onExit", "ResetDashes", "AddTriggerOffset", 
    "FlashColor", "FlashAlpha",
    "LightningCount", "LightningOffsetRange", "LightningDelay", "LightningMaxDelay", "Flash", "Shake",
    "GlitchStrength", "StartingGlitchDuration", "EndingGlitchDuration", "StartingGlitchEase", "EndingGlitchEase",
    "WipeExtraDelay"]
    for selection in Ahorn.currentTool.selections
        tar = selection.target
        if isa(tar, Trigger)
            if tar.name != "VivHelper/ITPT1Way" && tar.name != "VivHelper/ITPT2Way" && tar.name != "VivHelper/TeleportTarget"
                return String["x","y","width","height","TransitionType", "TargetID"]
            else if tar.id != trigger.id
                for s in arr
                    if tar.data[s] != trigger.data[s]
                        filter!(arr, x->x≠s)
                    end
                end
            end
        end
    end
    return hcat(arr, String["x","y","width","height","TransitionType"])=#
    return String["x","y","width","height","TransitionType"]
end
=#
Ahorn.editingIgnored(trigger::TPT, multiple::Bool=false) = multiple ? String["x", "y", "width","height","SetState","AddTriggerOffset","SpeedModifier","SpeedChangeStrength","SpeedChangeX","SpeedChangeY","RotationValue","RotateToAngle","RotateBeforeSpeedChange"] : String["width", "height"]

Ahorn.editingOptions(trigger::ITPT1Way) = Dict{String, Any}(
    "RoomName" => VivHelper.getRoomNames(),
    "TargetID" => VivHelper.getTriggerValues(["VivHelper/TeleportTarget"], "TargetID", String[""]),
    "ExitDirection" => Dict{String, Integer}(
        "Opposite Entry Only" => -2,
        "Any Side Not Entry" => -1,
        "Right Only" => 1,
        "Up Only" => 2,
        "Up or Right" => 3,
        "Left Only" => 4,
        "Horizontal (L/R)" => 5,
        "Left or Up" => 6,
        "Not Down (R/U/L)" => 7,
        "Down Only" => 8,
        "Down or Right" => 9,
        "Vertical (U/D)" => 10,
        "Not Left (R/U/D)" => 11,
        "Down or Left" => 12,
        "Not Up (R/L/D)" => 13,
        "Not Right (U/L/D)" => 14,
        "Any Side (Default)" => 15
    ),
    "Persistence" => String["None","OncePerRetry","OncePerMapPlay"],

    "FlashColor" => VivHelper.XNAColors,

    "LightningOffsetRange" => String["-130,130"],

    "GlitchStrength" => Number[0.25, 0.5, 0.8],
    "StartingGlitchEase" => VivHelper.EaseTypes,
    "EndingGlitchEase" => VivHelper.EaseTypes,

    #CustomWipe coming soon:tm:
)
Ahorn.editingOptions(trigger::TPT) = Dict{String, Any}(
    "TargetID" => VivHelper.getTriggerValues(["VivHelper/ITPT1Way"], "TargetID", String[""]),
    "SetState" => Dict{String, String}(
        "NoChange (Default)" => "-1",
        "Normal State" => "0",
        "Climb" => "1",
        "Dash" => "2",
        "Swim" => "3",
        "Boost" => "4",
        "Red Bubble" => "5",
        "StHitSquash" => "6",
        "Badeline Launch" => "7",
        "Pickup Holdable" => "8",
        "Dream Dash" => "9",
        "Summit Launch" => "10",
        "Dummy State" => "11",
        "Intro Walk" => "12",
        "Intro Jump" => "13",
        "Intro Respawn" => "14",
        "Intro Wake Up" => "15",
        "Prologue Dash Sequence" => "16",
        "Frozen State" => "17",
        "Reflection Fall" => "18",
        "Feather Flight" => "19",
        "Temple Fall Sequence" => "20",
        "Cassette Bubble Fly" => "21",
        "Attract (Badeline Boss Hit Sequence)" => "22",
        "Moon Jump (Farewell 3rd Cutscene)" => "23",
        "Bird Launch" => "24",
        "Intro Think for a Bit" => "25"
    ),
    "SpeedModifier"=>String["NoChange", "Add", "Multiply", "Set"],


)

Ahorn.resizable(trigger::TPT) = false, false
Ahorn.nodeLimits(trigger::TPT) = 0, 1

const ErrorSprites = "ahorn/VivHelper/error1", "ahorn/VivHelper/error2"

function getFirstMatchingTrigger(compare::String="")
    for room in Ahorn.loadedState.map.rooms
        for trigger in room.triggers
            t = get(trigger.data, "TargetID", nothing) 
            if t !== nothing && trigger.name == "VivHelper/ITPT1Way" && t == compare
                return trigger
            end
        end
    end
    return nothing
end

#Custom implementation meant to clean up the text from Viv Helper ITPT1 Way to something more legible
function Ahorn.renderTrigger(ctx::Ahorn.Cairo.CairoContext, layer::Ahorn.Layer, trigger::ITPT1Way, room::Maple.Room; alpha=nothing)
    if ctx.ptr != C_NULL # this line just prevents a crash
        Ahorn.Cairo.save(ctx)

        x, y = Ahorn.position(trigger)
        w, h = Int(trigger.width), Int(trigger.height)

        Ahorn.rectangle(ctx, x, y, w, h)
        Ahorn.clip(ctx)

        text = "Viv Helper Teleporter (1-Way)"
        Ahorn.drawRectangle(ctx, x, y, w, h, (0.6, 0.2, 0.6, 0.6), (0.6, 0.2, 0.6, 0.4))
        if get(trigger.data, "TargetID", "") == ""
            z = trunc(Int, time()) % 2 == 0
            Ahorn.drawSprite(ctx, z ? ErrorSprites[2] : ErrorSprites[1], x + w/2, y + h/2)
            Ahorn.drawCenteredText(ctx, "TargetID is empty!", x, y+h/2+16, w, h)
        else
            Ahorn.drawCenteredText(ctx, text, x, y, w, h)
        end
        Ahorn.restore(ctx)
    end
end


# Custom implementation for way better reasons
function Ahorn.triggerSelection(trigger::TPT, room::Maple.Room, node::Int=0)
    x, y = Ahorn.position(trigger)
    width = get(trigger.data, "width", 24)
    height = get(trigger.data, "height", 24)
    u = get(trigger.data, "TargetID", "")
    if u == ""
        return Ahorn.Rectangle(x,y,width,height)
    end
    ato = get(trigger.data, "AddTriggerOffset", false)
    if ato && u != ""
        trigger2 = getFirstMatchingTrigger(u)
        if trigger2 !== nothing
            width = get(trigger2.data, "width", width)
            height = get(trigger2.data, "height", height)
        end
    end
    nodes = get(trigger.data, "nodes", Tuple{Int, Int}[])

    if isempty(nodes)
        return Ahorn.Rectangle(x, y, width, height)

    else
        res = Ahorn.Rectangle[Ahorn.Rectangle(x, y, width, height)]

        for node in nodes
            nx, ny = Int.(node)

            push!(res, Ahorn.Rectangle(nx, ny, 8, 8))
        end

        return res
    end
end

function Ahorn.renderTriggerSelection(ctx::Ahorn.Cairo.CairoContext, layer::Ahorn.Layer, trigger::TPT, room::Maple.Room; alpha=nothing)
    x, y = Int(trigger.data["x"]), Int(trigger.data["y"])
    width, height = Int(trigger.data["width"]), Int(trigger.data["height"])
    u = get(trigger.data, "TargetID", "")
    if u == ""
       Ahorn.drawRectangle(ctx, x-12,y-12,24,24, Ahorn.colors.trigger_fc, Ahorn.colors.trigger_bc)
       return 
    end
    s = get(trigger.data, "AddTriggerOffset", false) 
    if s && u != ""
        trigger2 = getFirstMatchingTrigger(u)
        if trigger2 !== nothing
            width = get(trigger2.data, "width", width)
            height = get(trigger2.data, "height", height)
        end
    end
    nodes = get(trigger.data, "nodes", Tuple{Int, Int}[])
    offsetCenterX, offsetCenterY = floor(Int, width / 2), floor(Int, height / 2)

    for node in nodes
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, x + offsetCenterX, y + offsetCenterY, nx + 4, ny + 4, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawRectangle(ctx, nx, ny, 8, 8, Ahorn.colors.trigger_fc, Ahorn.colors.trigger_bc)
    end
end

function Ahorn.renderTrigger(ctx::Ahorn.Cairo.CairoContext, layer::Ahorn.Layer, trigger::TPT, room::Maple.Room; alpha=nothing)
    if ctx.ptr != C_NULL # this line just prevents a crash
        Ahorn.Cairo.save(ctx)

        x, y = Ahorn.position(trigger)
        width, height = Int(trigger.width), Int(trigger.height)
        u = get(trigger.data, "TargetID", "")
        if u == ""
            z = trunc(Int, time()) % 2 == 0
            Ahorn.drawSprite(ctx, z ? ErrorSprites[2] : ErrorSprites[1], x, y)
            Ahorn.drawCenteredText(ctx, "TargetID is empty!", x, y+24, 32, 16)
        else
            ato = get(trigger.data, "AddTriggerOffset", false)
            if ato
                trigger2 = getFirstMatchingTrigger(u)
                if trigger2 !== nothing
                    width = get(trigger2.data, "width", width)
                    height = get(trigger2.data, "height", height)
                end
            end
            Ahorn.rectangle(ctx, x, y, width, height)
            Ahorn.clip(ctx)

            text = "Viv Helper Teleport Target"
            Ahorn.drawRectangle(ctx, x, y, width, height, (0.6, 0.2, 0.6, 0.6), (0.6, 0.2, 0.6, 0.4))
            Ahorn.drawCenteredText(ctx, text, x, y, width, height)
            
            Ahorn.restore(ctx)
        end
    end
end



end