
local followTorch = {}

followTorch.name = "VivHelper/FollowTorch"
followTorch.fieldInformation = {
    FadePoint = {fieldType = "integer", minimumValue = 0, maximumValue = 120 },
    Radius = {fieldType = "integer", minimumValue = 0, maximumValue = 120 },
    Alpha = {fieldType = "number", minimumValue = 0.0, maximumValue = 1.0},
    Color = {fieldType = "string", options = {"Default", "Red", "Orange", "Green", "Blue", "Purple", "Sunset", "Gray"}, editable = false}
}
followTorch.placements = {
    name = "main",
    data = {
        FadePoint=48, Radius=64, Alpha=1.0,
        Color="Default", followDelay=0.2
    }
}

function followTorch.texture(room,entity)
    return "FollowTorchSprites/ThorcVar/" .. (entity.Color or "Default") .. "Torch00"
end
return followTorch