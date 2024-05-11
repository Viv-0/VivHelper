local entities = require("entities")
local utils = require("utils")
local state = require('loaded_state')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local enum_colors = require('consts.xna_colors')

local script = {
    name = "vivh_ResolveColor",
    displayName = "Resolve Color Bug in Map",
    minimumVersion = "1.0.3",
    tooltip = "Resolves all instances of the Color value parity issue for VivHelper in Loenn/Celeste."
}

local SimpleColors = {
    "VivHelper/RainbowSpikesUp","VivHelper/RainbowSpikesLeft","VivHelper/RainbowSpikesDown","VivHelper/RainbowSpikesRight",
    "VivHelper/RainbowTriggerSpikesUp","VivHelper/RainbowTriggerSpikesLeft","VivHelper/RainbowTriggerSpikesDown","VivHelper/RainbowTriggerSpikesRight",
    "VivHelper/GhostBarrier", "VivHelper/DashCodeHeartController", "VivHelper/CustomLightbeam", "VivHelper/CPP", "VivHelper/TeleporterDash",
    "VivHelper/CornerSpike", "VivHelper/InLevelTeleporter", 
}

-- we want this to apply to All Rooms, regardless of mode, so we use prerun instead
function script.run(room, args, ctx) return end

function script.prerun(args, mode, ctx) 
    for _, styleground in ipairs(state.map.stylesFg) do
        if styleground._name == "VivHelper/CustomRain" or styleground._name == "VivHelper/WindRainFG" then
            styleground.Colors = styleground.colors
            styleground.colors = nil
        end
    end
    for _, styleground in ipairs(state.map.stylesBg) do
        if styleground._name == "VivHelper/CustomRain" or styleground._name == "VivHelper/WindRainFG" then
            styleground.Colors = styleground.colors
            styleground.colors = nil
        end
    end
    for _, room in ipairs(state.map.rooms) do 
        for _, entity in ipairs(room.entities) do
            if vivUtil.contains(SimpleColors, entity._name) then
                entity.color = vivUtil.swapRGBA(entity.Color)
                entity.Color = nil
            elseif entity._name == "VivHelper/WrapperRefillWall" then
                entity.innerColor = vivUtil.swapRGBA(entity.InnerColor)
                entity.InnerColor = nil
                entity.outerColor = vivUtil.swapRGBA(entity.OuterColor)
                entity.OuterColor = nil
            elseif entity._name == "VivHelper/TravelingFlame" or entity._name == "VivHelper/TravelingFlameCurve" then
                entity.color = vivUtil.swapRGBA(entity.ColorTint)
                entity.ColorTint = nil
            elseif entity._name == "VivHelper/Spawnpoint" or entity._name == "VivHelper/InterRoomSpawner" or
                   entity._name == "VivHelper/InterRoomSpawnTarget" or entity._name == "VivHelper/InterRoomSpawnTarget2" then
                entity.color = vivUtil.swapRGBA(entity.Color)
                entity.Color = nil
                entity.outlineColor = vivUtil.swapRGBA(entity.OutlineColor)
                entity.OutlineColor = nil
            elseif entity._name == "VivHelper/HoldableBarrierColorController" or "VivHelper/HoldableBarrierController2" then
                entity.particleColor = vivUtil.swapRGBA(entity.ParticleColor)
                entity.ParticleColor = nil
                entity.edgeColor = vivUtil.swapRGBA(entity.EdgeColor)
                entity.EdgeColor = nil
            elseif entity._name == "VivHelper/nWayDashBlock" then
                entity.detailColor = vivUtil.swapRGBA(entity.DetailColor)
                entity.DetailColor = nil
            elseif entity._name == "VivHelper/CrystalBombDetonatorController" then
                entity.particleColor = vivUtil.swapRGBA(entity.ParticleColor)
                entity.ParticleColor = nil
                entity.baseColor = vivUtil.swapRGBA(entity.BaseColor)
                entity.BaseColor = nil
            elseif entity._name == "VivHelper/Collectible" then
                entity.ParticleColor = vivUtil.swapRGBA(entity.particleColor)
                entity.particleColor = nil
            elseif entity._name == "VivHelper/CustomBirdPath" then
                entity.trailColor = vivUtil.swapRGBA(entity.TrailColor)
                entity.TrailColor = nil
            elseif entity._name == "VivHelper/CustomHangingLamp" then
                local colTable = vivUtil.alphMult(vivUtil.oldGetColorTable(entity.LightColor, true, {1,1,1,1}), entity.LightAlpha)
                entity.lightColor = utils.rgbaToHex(colTable[1],colTable[2],colTable[3],colTable[4])
                entity.LightColor = nil
                entity.LightAlpha = nil
            elseif entity._name == "VivHelper/CurvedZipMover" or entity._name == "VivHelper/CurvedZipMover2" or entity._name == "VivHelper/CustomZipMover" or
                   entity._name == "VivHelper/CrumblingZipMover" or entity._name == "VivHelper/CustomCrumblingZipMover" or entity._name == "VivHelper/CurvedCrumblingZipMover" then
                entity.ropeColor = vivUtil.swapRGBA(entity.RopeColor)
                entity.RopeColor = nil
                entity.ropeNotchColor = vivUtil.swapRGBA(entity.RopeNotchColor)
                entity.RopeNotchColor = nil
                entity.baseColor = vivUtil.swapRGBA(entity.BaseColor)
                entity.BaseColor = nil
            elseif entity._name == "VivHelper/CustomTorch" then
                entity.color = vivUtil.swapRGBA(entity.Color)
                entity.Color = nil
                entity.spriteColor = vivUtil.swapRGBA(entity.spriteColor) -- this is a weird case but i suspect noone used the alpha in this given its age
            elseif entity._name == "VivHelper/DashTempleGate" then
                entity.beamColor = vivUtil.swapRGBA(entity.BeamColor)
                entity.BeamColor = nil
            elseif entity._name == "VivHelper/CustomSpinner" or entity._name == "VivHelper/AnimatedSpinner" then
                entity.color = vivUtil.swapRGBA(entity.Color)
                if entity.Type ~= "White" then entity.color = "ffffff00" end
                entity.Color = nil
                entity.shatterColor = vivUtil.swapRGBA(entity.ShatterColor) 
                entity.ShatterColor = nil
                entity.borderColor = vivUtil.swapRGBA(entity.BorderColor)
                entity.BorderColor = nil
                entity.Type = nil
            elseif entity._name == "VivHelper/CassetteTileEntity" then
                entity.enabledColor = entity.enabledTint
                entity.enabledTint = nil
                entity.disabledColor = entity.disabledTint
                entity.disabledTint = nil
            end 
        end
        for _, trigger in ipairs(room.triggers) do
            if trigger._name == "VivHelper/BasicInstantTeleportTrigger" or trigger._name == "VivHelper/MainInstantTeleportTrigger" or trigger._name == "VivHelper/CustomInstantTeleportTrigger" then
                trigger.color = trigger.Color
                trigger.Color = nil
            end
        end
    end
end

return script