return {
    name = "VivHelper/ActivateCPPTrigger",
    placements = {
        name = "main",
        data = {
            CPPID = "",
            state = true,
            onlyOnce = false,
            mode = "WhilePlayerInside"
        }
    },
    fieldInformation = {
        mode = {
            fieldType = "string", options = {
                {"On Player Entering Area", "OnPlayerEnter"},
                {"On Player Leaving Area" ,	"OnPlayerLeave"},
                {"On Level Start",          "OnLevelStart"},
                {"While Player is Inside", "WhilePlayerInside"}
            }
        }
    },
    nodeLimits = {0,0}
}