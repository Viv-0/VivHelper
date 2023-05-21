local torch = {}

torch.name = "VivHelper/CustomTorch"
torch.depth = 2000
torch.placements = {
    name = "main",
    data = {
        startLit=false,
	    Color="ff80ffff",
	    spriteColor="ff80ffff",
	    Alpha=1.0,
	    startFade=48,
	    endFade=64,
	    RegisterRadius=4.0,
	    unlightOnDeath=false
    }
}
torch.fieldInformation = {
    Color = {fieldType = "VivHelper.color", allowXNAColors = true },
    spriteColor = {fieldType = "VivHelper.color", allowXNAColors = true },
    Alpha = {fieldType = "number", minimumValue = 0.0, maximumValue = 1.0},
    startFade = {fieldType = "integer", minimumValue = 0, maximumValue = 120 },
    endFade = {fieldType = "integer", minimumValue = 0, maximumValue = 120 }, -- Fun fact, this is the actual limit for lights in celeste
    RegisterRadius = {fieldType = "number", minimumValue = 2}
}

function torch.texture(room, entity)
    return entity.startLit and "ahorn/VivHelper/torch/grayTorchLit" or "ahorn/VivHelper/torch/grayTorchUnlit"
end

function torch.color(room, entity)
    return require('mods').requireFromPlugin('libraries.vivUtil').getColor(entity.spriteColor or "ffffffff")
end

return torch