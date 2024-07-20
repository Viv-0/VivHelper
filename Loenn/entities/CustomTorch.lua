local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')

local torch = {}

torch.name = "VivHelper/CustomTorch"
torch.depth = 2000
torch.placements = {
    name = "main",
    data = {
        startLit=false,
	    color="80ffff",
	    spriteColor="80ffff",
	    Alpha=1.0,
	    startFade=48,
	    endFade=64,
	    RegisterRadius=4.0,
	    unlightOnDeath=false
    }
}
torch.fieldInformation = {
    color = {fieldType = "color", allowXNAColors = true, useAlpha=true },
    spriteColor = {fieldType = "color", allowXNAColors = true, useAlpha=true },
    Alpha = {fieldType = "number", minimumValue = 0.0, maximumValue = 1.0},
    startFade = {fieldType = "integer", minimumValue = 0, maximumValue = 120 },
    endFade = {fieldType = "integer", minimumValue = 0, maximumValue = 120 }, -- Fun fact, this is the actual limit for lights in celeste
    RegisterRadius = {fieldType = "number", minimumValue = 2}
}

function torch.texture(room, entity)
    return entity.startLit and "ahorn/VivHelper/torch/grayTorchLit" or "ahorn/VivHelper/torch/grayTorchUnlit"
end

function torch.color(room, entity)
    return vivUtil.GetColorTable(entity.spriteColor, true, {1,1,1,1})
end

return torch