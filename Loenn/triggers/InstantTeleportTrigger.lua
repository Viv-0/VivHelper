local vivUtil = require("mods").requireFromPlugin("libraries.vivUtil")
local state = require("loaded_state")
local vh_tag = require("mods").requireFromPlugin("ui.forms.fields.vh_tagString")

local oldFieldOrder = { 
    "x","y","width","height",
    "newPosX","newPosY",
    "WarpRoom","TransitionType",
    "AddTriggerOffset","ResetDashes", "bringHoldablesThrough",
    "TimeBeforeTeleport","ZFlagsData",
    "ExitVelocityX", "ExitVelocityY",
    "ExitVelocityS","Dreaming", "ForceNormalState",
    "VelocityModifier","RotationType", "RotationActor"
}

local oldFieldInfo = {
    newPosX = { fieldType = "integer", minimumValue = -1 },
    newPosY = { fieldType = "integer", minimumValue = -1 },
    WarpRoom = { fieldType = "VivHelper.room_names" },
    TransitionType = { fieldType = "string", options = {{"None", "None"},{"Lightning", "Lightning"},{"Glitch", "GlitchEffect"},{"Color Flash", "ColorFlash"}}, editable = false}
}

local function oldTextFunction(room, item) return "Instant Teleport [VivHelper]\n"..item.WarpRoom or "" .. "\n(" .. item.newPosX .. "," .. item.newPosY .. ")" end

local ittOB = { name = "VivHelper/BasicInstantTeleportTrigger",
    fieldInformation = oldFieldInfo,
    fieldOrder = oldFieldOrder,
    _vivh_textOverride = oldTextFunction,
    _vivh_finalizePlacement = function(room, layer, item) item.WarpRoom = room.name end
}
ittOB.placements = {
    name = "main",
    data = {
        newPosX = -1,
        newPosY = -1,
        WarpRoom = "", TransitionType="None",
        AddTriggerOffset=false, ResetDashes=false
    }
}
local ittOM = { name = "VivHelper/MainInstantTeleportTrigger", fieldInformation = oldFieldInfo, fieldOrder = oldFieldOrder, _vivh_replaceDrawTextFunc = oldTextFunction, _vivh_finalizePlacement = function(room, layer, item) item.WarpRoom = room.name end }
ittOM.placements = {
    name = "main",
    data = {
        newPosX = -1,
        newPosY = -1,
        WarpRoom = "", TransitionType="None",
        AddTriggerOffset=false, ResetDashes=false,
        ExitVelocityX=0.0,ExitVelocityY=0.0,VelocityModifier=false,
        TimeBeforeTeleport=0.0,ForceNormalState=false, bringHoldablesThrough = false
    }
}
local ittOC = { name = "VivHelper/CustomInstantTeleportTrigger",
fieldInformation = oldFieldInfo,
fieldOrder = oldFieldOrder,
_vivh_replaceDrawTextFunc = oldTextFunction,
_vivh_finalizePlacement = function(room, layer, item) item.WarpRoom = room.name end }
ittOC.placements = {
    name = "main",
    data = {
        newPosX = -1,
        newPosY = -1,
        WarpRoom = "", TransitionType="None",
        AddTriggerOffset=false, ResetDashes=false,
        ExitVelocityX=0.0,ExitVelocityY=0.0,ExitVelocityS=0.0,VelocityModifier=false,
        TimeBeforeTeleport=0.0,ForceNormalState=false,
        RotationType=false,RotationActor=0.0,
        TimeSlowDown=0.0, bringHoldablesThrough = false
    }
}

local Target = { name = "VivHelper/TeleportTarget", placements = {{
        name = "main",
        data = {
            TargetID="", AddTriggerOffset=false, SetState="-1"
        }},{
        name = "custom",
        data = {
            TargetID="", AddTriggerOffset=false, SetState="-1",
            RotationValue=0.0, RotateToAngle=false, RotateBeforeSpeedChange=false,
            SpeedModifier="NoChange", SpeedChangeX=0.0, SpeedChangeY=0.0
        }}
    },
    nodeLimits = {0,1},
    fieldInformation = {
        SpeedModifier = {fieldType = "string", options = {"NoChange","Add","Multiply","Set"}, editable = false}
    }
}
vh_tag.addTagControlToHandler(Target, "TargetID", "teleporttarget", true)

local Teleporter = { name = "VivHelper/ITPT1Way", 
    placements = {{
        name = "main",
        data = {
            TargetID="", ExitDirection=0, RoomName = "",
            RequiredFlags="", FlagsOnTeleport="",
            ResetDashes=false,
            BringHoldableThrough=false,
            IgnoreNoSpawnpoints=false,
            Persistence="None",
            EndCutsceneOnWarp=false,
            customDelay=0.0,
            TransitionType = "None"
        }
    }, {
        name = "flash",
        data = {
            TargetID="", ExitDirection=0, RoomName = "",
            RequiredFlags="", FlagsOnTeleport="",
            ResetDashes=false,
            BringHoldableThrough=false,
            IgnoreNoSpawnpoints=false,
            Persistence="None",
            EndCutsceneOnWarp=false,
            customDelay=0.0,
            TransitionType = "Flash",
            FlashColor = "ffffff",
            FlashAlpha = 1.0
        }
    }, {
        name = "lightning",
        data = {
            TargetID="", ExitDirection=0, RoomName = "",
            RequiredFlags="", FlagsOnTeleport="",
            ResetDashes=false,
            BringHoldableThrough=false,
            IgnoreNoSpawnpoints=false,
            Persistence="None",
            EndCutsceneOnWarp=false,
            TransitionType = "Lightning",
            LightningCount = 2,
            LightningOffsetRange = "-130,130",
            LightningDelay = 0.0,
            LightningMaxDelay = 0.25,
            Flash = true,
            Shake = true
        }
    }, {
        name = "glitch",
        data = {
            TargetID="", ExitDirection=0, RoomName = "",
            RequiredFlags="", FlagsOnTeleport="",
            ResetDashes=false,
            BringHoldableThrough=false,
            IgnoreNoSpawnpoints=false,
            Persistence="None",
            EndCutsceneOnWarp=false,
            TransitionType = "Glitch",
            GlitchStrength = 0.5,
            StartingGlitchEase = "Linear",
            StartingGlitchDuration = 0.3,
            EndingGlitchEase = "Linear",
            EndingGlitchDuration = 0.3,
            freezeOnTeleport = true
        }
    }, {
        name = "wipe",
        data = {
            TargetID="", ExitDirection=0, RoomName = "",
            RequiredFlags="", FlagsOnTeleport="",
            ResetDashes=false,
            BringHoldableThrough=false,
            IgnoreNoSpawnpoints=false,
            Persistence="None",
            EndCutsceneOnWarp=false,
            TransitionType = "Wipe",
            freezeOnTeleport = true,
            wipeOnLeave = true,
            wipeOnEnter = true
        }
    }},
    fieldOrder = { "x", "y", "width", "height", "TargetID", "RoomName", "Persistence", "ExitDirection",
        "RequiredFlags", "FlagsOnTeleport", "TransitionType", "ResetDashes", "BringHoldableThrough", "IgnoreNoSpawnpoints", "EndCutsceneOnWarp",
        "FlashColor", "FlashAlpha",
        "LightningCount", "LightningOffsetRange", "LightningDelay", "LightningMaxDelay", "Flash", "Shake",
        "GlitchStrength", "StartingGlitchDuration", "EndingGlitchDuration", "StartingGlitchEase", "EndingGlitchEase", 
        "freezeOnTeleport",
        "wipeOnEnter", "wipeOnLeave"
    },
    ignoredFields = function(entity) 
        local tt = entity.TransitionType
        if tt == "Flash" then return {"_id","_name","TransitionType","LightningCount","LightningOffsetRange", "LightningDelay", "LightningMaxDelay", "Flash", "Shake",
            "GlitchStrength", "StartingGlitchDuration", "EndingGlitchDuration", "StartingGlitchEase", "EndingGlitchEase", "freezeOnTeleport", "wipeOnEnter","wipeOnLeave"}
        elseif tt == "Lightning" then return {"_id","_name","TransitionType", "FlashColor","FlashAlpha",
            "GlitchStrength", "StartingGlitchDuration", "EndingGlitchDuration", "StartingGlitchEase", "EndingGlitchEase", "freezeOnTeleport", "wipeOnEnter","wipeOnLeave"}
        elseif tt == "Glitch" then return {"_id","_name","TransitionType", "FlashColor","FlashAlpha",
            "LightningCount","LightningOffsetRange", "LightningDelay", "LightningMaxDelay", "Flash", "Shake", "wipeOnEnter","wipeOnLeave"}
        elseif tt == "Wipe" then return {"_id","_name","TransitionType", "FlashColor","FlashAlpha",
            "LightningCount","LightningOffsetRange", "LightningDelay", "LightningMaxDelay", "Flash", "Shake",
            "GlitchStrength", "StartingGlitchDuration", "EndingGlitchDuration", "StartingGlitchEase", "EndingGlitchEase"}
        else return {"_id","_name","TransitionType", "FlashColor","FlashAlpha",
            "LightningCount","LightningOffsetRange", "LightningDelay", "LightningMaxDelay", "Flash", "Shake",
            "GlitchStrength", "StartingGlitchDuration", "EndingGlitchDuration", "StartingGlitchEase", "EndingGlitchEase", "freezeOnTeleport",
            "wipeOnEnter","wipeOnLeave"}
        end
    end
}

Teleporter.fieldInformation = function(entity)
    local tt = entity.TransitionType
    local ret = {ExitDirection = {fieldType = "integer", options = {
        ["Opposite Entry Only"] = -2,
        ["Any Side Not Entry"] = -1,
        ["Teleport On Entry"] = 0,
        ["Right Only"] = 1,
        ["Up Only"] = 2,
        ["Up or Right"] = 3,
        ["Left Only"] = 4,
        ["Horizontal (L/R)"] = 5,
        ["Left or Up"] = 6,
        ["Not Down (R/U/L)"] = 7,
        ["Down Only"] = 8,
        ["Down or Right"] = 9,
        ["Vertical (U/D)"] = 10,
        ["Not Left (R/U/D)"] = 11,
        ["Down or Left"] = 12,
        ["Not Up (R/L/D)"] = 13,
        ["Not Right (U/L/D)"] = 14,
        ["Any Exit (Default)"] = 15,
    }, editable = false },
    RoomName = {fieldType = "VivHelper.room_names", allowEmpty = true},
    Persistence = {options = {"None","OncePerRetry","OncePerMapPlay"}, editable = false} }

    if tt == "Flash" then 
        ret["FlashColor"] = {fieldType = "color", allowXNAColors = true, defaultValue = "fffffff"}
        ret["FlashAlpha"] = {fieldType = "number", minimumValue = 0.0, maximumValue = 1.0, defaultValue = 1.0 }
        entity.FlashColor = entity.FlashColor or "ffffffff"
        entity.FlashAlpha = entity.FlashAlpha or 1.0
    elseif tt == "Lightning" then 
        ret["LightningOffsetRange"] = {fieldType = "string", defaultValue = "-130,130"}
        ret["LightningCount"] = {fieldType = "integer", defaultValue = 2}
        ret["LightningDelay"] = {fieldType = "number", minimumValue = 0.0, defaultValue = 0.25 }
        ret["LightningMaxDelay"] = {fieldType = "number"}
        entity.LightningOffsetRange = entity.LightningOffsetRange or "-130,130"
        entity.LightningCount = entity.LightningCount or 2
        entity.LightningDelay = entity.LightningDelay or 0.25
        entity.LightningMaxDelay = entity.LightningMaxDelay or ""
    elseif tt == "Glitch" then 
        ret["GlitchStrength"] = {fieldType = "number", minimumValue = 0.0, maximumValue = 1.0, defaultValue = 0.5 }
        -- TODO : Easer control option on StartingGlitchEase and EndingGlitchEase
        ret["StartingGlitchDuration"] = {fieldType = "number", minimumValue = 0.0, defaultValue = 0.3}
        ret["EndingGlitchDuration"] = {fieldType = "number", minimumValue = 0.0, defaultValue = 0.3}
        entity.GlitchStrength = entity.GlitchStrength or 0.5
        entity.StartingGlitchDuration = entity.StartingGlitchDuration or 0.3
        entity.EndingGlitchDuration = entity.EndingGlitchDuration or 0.3
    end
    -- Flash, Shake
    
    -- freezeOnTeleport, wipeOnEnter, wipeOnLeave   
    return ret     
end

vh_tag.addTagControlToHandler(Teleporter, "TargetID", "teleporttarget", false)



return { ittOB, ittOM, ittOC, Target, Teleporter }