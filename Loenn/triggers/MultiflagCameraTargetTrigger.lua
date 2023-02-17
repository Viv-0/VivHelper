local enums = require("consts.celeste_enums")

return {
    name = "VivHelper/MultiflagCameraTargetTrigger",
    placements = {
        name = "main",
        data = {
            DeleteFlag="",
	        SingleFlag="",
	        ComplexFlagData="",
	        lerpStrength=0.0,
	        xOnly=false,
	        yOnly=false,
	        positionMode="NoEffect"
        }
    },
    fieldInformation = {positionMode = {fieldType = "string", options = enums.trigger_position_modes}},
    nodeLimits = {1,1}
}