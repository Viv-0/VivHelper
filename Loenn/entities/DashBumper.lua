local bumper = {}

bumper.name = "VivHelper/DashBumper"
bumper.depth = 0
bumper.nodeLineRenderType = "line"
bumper.texture = "VivHelper/dashBumper/idle00"
bumper.nodeLimits = {0, 1}
bumper.placements = {
    name = "bumper",
    data = {
        Wobble=true,
        RespawnTime=0.6,
        MoveTime=1.81818,
        ReflectType="DashDir"
    }
}
bumper.fieldInformation = {
    RespawnTime = {fieldType = "number", minimumValue = 0.01},
    MoveTime = {fieldType = "number", minimumValue = 0.01},
    ReflectType={fieldType = "string", options = {
        {"Reflect Dash Direction", "DashDir"},
        {"4-Way Angle", "Angle4"},
        {"8-Way Angle", "Angle8"},
        {"Modified 4-Way Angle", "AltAngle4"}
    }}
}

return bumper