

local ccm = {name = "VivHelper/CustomCoreMessage"}

ccm.texture = "@Internal@/core_message"
ccm.depth = -10000000

ccm.placements = {
    name = "main",
    data = {
        line = 0,
        dialog = "app_ending",
        OutlineColor="",
        Scale=1.25,
        RenderDistance=128.0,
        AlwaysRender=false,
        LockPosition=false,
        DefaultFadedValue=0.0,
        PauseType="Hidden",
        TextColor1="ffffff",
        EaseType="CubeInOut"
    }, nodeLimits = {0,2}
}

ccm.fieldInformation = {
    OutlineColor = {fieldType = "color", allowXNAColors=true, allowEmpty = true},
    TextColor1 = {fieldType = "color", allowXNAColors=true},
    Scale = {fieldType = "number", minimumValue = 0.125},
    EaseType = { fieldType = "VivHelper.easing"},
    line = { fieldType = "integer", minimumValue = 0 },
    PauseType = {fieldType = "string", options = {"Hidden","Shown","Fade"}, editable = false}
}

return ccm