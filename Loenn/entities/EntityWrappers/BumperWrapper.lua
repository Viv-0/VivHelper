local enums = require('consts.celeste_enums')
local drawableSprite = require('structs.drawable_sprite')

local BumperWrapper = {name = "VivHelper/BumperWrapper",
fieldOrder = { -- The newlines in this file should denote the accurate visual alignment of the fieldOrders.
    "x","y",
    "TypeName","AnchorName",
    "RemoveBloom","RemoveLight","RemoveWobble", "AttachToSolid",
    "BumperLaunchType", "BumperBoost",
    "ExplodeStrengthModifier", "RespawnTime",
    "SetDashes","SetStamina",
    "CoreMode","Scale",
    "NormalStateOnEnd"
}, nodeLimits = {0,1} }
BumperWrapper.placements = {
    {
        name = "main",
        data = {
            TypeName = "Celeste.Bumper", AnchorName = "anchor",
            RemoveBloom = true, RemoveLight = true, RemoveWobble = true, AttachToSolid = false,
            BumperLaunchType = "Ignore", BumperBoost = 0,
            ExplodeStrengthMultiplier=1.0, RespawnTime = 0.5,
            SetDashes = -1, SetStamina = -1,
            CoreMode = "None", Scale = 12,
            NormalStateOnEnd = false
        }
    }
}
BumperWrapper.fieldInformation = {
    BumperLaunchType = {fieldType = "string", options = {
        ["Default"] = "Ignore",
        ["4-Way (Cardinal / No Diagonals)"] = "Cardinal",
        ["4-Way (Diagonals Only)"] = "Diagonal",
        ["8-Way"] = "EightWay",
        ["Modified 4-Way"] = "Alt4way"
    }, editable = false},
    CoreMode = {fieldType = "string", options = enums.core_modes, editable = false},
    DashCooldown = {fieldType = "number", options = {Default = 0.2}, editable = true},
    SetStamina = {fieldType = "integer", options = {
        ["Ignore Bumper Refilling Stamina"] = -2,
        ["Default Behavior"] = -1
    }, editable = true},
    SetDashes = {fieldType = "integer", options = {
        ["Ignore Bumper Refilling Dashes"] = -2,
        ["Default Behavior"] = -1
    }, editable = true},
    BumperBoost = {fieldType = "integer", options = {
        ["Default Behavior"] = 0,
        ["Never Bumper Boost"] = -1,
        ["Better Bumper Boost"] = 1,
        ["Always Bumper Boost"] = 2
    }, editable = false},
    Scale = {fieldType = "integer"}
}
BumperWrapper.texture = "ahorn/VivHelper/bumperWrapper"
BumperWrapper.color = {0.5,0.5,0.5,0.5}
BumperWrapper.depth = -100

return BumperWrapper