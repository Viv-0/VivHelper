local mods = require('mods')
local vivUtil = require('mods').requireFromPlugin('libraries.vivUtil')
local animation = require('mods').requireFromPlugin('libraries.drawable_animation')
local Sprite = require('structs.drawable_sprite')
local utils = require('utils')
local spikeHelper = require('helpers.spikes') -- used for specifically Ahorn-form plugins

local textureEndings = {"_right", "_up","_left","_down"}
local function spikeOffsets(idx, animation, entity)
    local j
    if idx % 2 == 1 then 
        animation.xSet = {}
        j = animation.meta[1].width
        for i = j/2,entity.width,j do table.insert(animation.xSet, entity.x + i) end
        animation.y = entity.y + 2 - idx -- 3 : -1, 1 : 1
    else
        animation.ySet = {}
        j = animation.meta[1].height
        for i = j/2,entity.height,j do table.insert(animation.ySet, entity.y + i) end
        animation.x = entity.x + idx - 1 -- 2 : 1, 0 : -1
    end
end
local baseSpikeJustifications = {{0.0, 0.5},{0.5, 1.0},{1.0, 0.5},{0.5, 0.0}}
local rotatedSpikeJustification = {{0.0, 0.5},{0.0, 0.5},{1.0, 0.5},{1.0, 0.5}}

local function Draw(idx, room, entity, viewport)
    local _entity = utils.deepcopy(entity)
    local _out,_valid = animation.fromTextures(entity.directory .. textureEndings[idx + 1], entity.timeDelay or 0.1, _entity)
    if _valid then
        spikeOffsets(idx, _out, entity)
        _out:setJustification(baseSpikeJustifications[idx+1])
        return _out 
    end
    _out,_valid = animation.fromTextures(entity.directory, entity.timeDelay or 0.1, _entity)
    if _valid then
        _out.r = (idx == 3 and math.pi or idx == 2 and math.pi * 1.5 or idx == 1 and 0 or math.pi * 0.5)
        spikeOffsets(idx, _out, entity)
--        _out:setJustification(rotatedSpikeJustification[idx+1])
        return _out
    else return Sprite.fromTexture(mods.internalModContent .. "/missing_image") end
end

local ret = {
    {
        name = "VivHelper/AnimatedSpikesUp",
        placements = {
            name = "main", data = {
                version = 2,
                width = 8, Color = "ffffff",
                directory = "danger/tentacles",
                centerVert = true
                }
            },
        ignoredFields = {"version"},
        fieldInformation = {
            directory = {fieldType = "path", allowFolders = false, allowFiles = true, filenameProcessor = function(filename, rawFilename, prefix) return vivUtil.trim(filename):gsub("%d+",""):gsub("_up","") end
            },
            Color = {fieldType = "VivHelper.color", allowXNAColors = true, allowRainbow = true }
        },
        sprite =function(room,entity,viewport) if entity.version == 2 then return Draw(1,room,entity,viewport) else
            local _entity = utils.deepcopy(entity) 
            _entity.type = "tentacles"
            return spikeHelper.getSpikeSprites(_entity, "up")
        end end,
        selection = function(room,entity) return utils.rectangle(entity.x,entity.y-4,entity.width,4) end
    },{
        name = "VivHelper/AnimatedSpikesLeft",
        placements = {
            name = "main", data = {
                version = 2,
                height = 8, Color = "ffffff",
                directory = "danger/tentacles",
                centerVert = true
                }
            },
        ignoredFields = {"version"},
        fieldInformation = {
            directory = {fieldType = "path", allowFolders = true, allowFiles = false, filenameProcessor = function(filename, rawFilename, prefix) return vivUtil.trim(filename):gsub("%d+",""):gsub("_left","") end
            },
            Color = {fieldType = "VivHelper.color", allowXNAColors = true, allowRainbow = true }
        },
        sprite = function(room,entity,viewport) if entity.version == 2 then return Draw(2,room,entity,viewport) else
            local _entity = utils.deepcopy(entity) 
            _entity.type = "tentacles"
            return spikeHelper.getSpikeSprites(_entity, "left")
        end end,
        selection = function(room,entity) return utils.rectangle(entity.x-4,entity.y,4,entity.height) end
    },{
        name = "VivHelper/AnimatedSpikesDown",
        placements = {
            name = "main", data = {
                version = 2,
                width = 8, Color = "ffffff",
                directory = "danger/tentacles",
                centerVert = true
                }
            },
        ignoredFields = {"version"},
        fieldInformation = {
            directory = {fieldType = "path", allowFolders = true, allowFiles = false, filenameProcessor = function(filename, rawFilename, prefix) return vivUtil.trim(filename):gsub("%d+",""):gsub("_down","") end
            },
            Color = {fieldType = "VivHelper.color", allowXNAColors = true, allowRainbow = true }
        },
        sprite = function(room,entity,viewport) if entity.version == 2 then return Draw(3,room,entity,viewport) else
            local _entity = utils.deepcopy(entity) 
            _entity.type = "tentacles"
            return spikeHelper.getSpikeSprites(_entity, "down")
        end end,
        selection = function(room,entity) return utils.rectangle(entity.x,entity.y,entity.width,4) end
    },{
        name = "VivHelper/AnimatedSpikesRight",
        placements = {
            name = "main", data = {
                version = 2,
                height = 8, Color = "ffffff",
                directory = "danger/tentacles",
                centerVert = true
                }
            },
        ignoredFields = {"version"},
        fieldInformation = {
            directory = {fieldType = "path", allowFolders = true, allowFiles = false, filenameProcessor = function(filename, rawFilename, prefix) return vivUtil.trim(filename):gsub("%d+",""):gsub("_right","") end
            },
            Color = {fieldType = "VivHelper.color", allowXNAColors = true, allowRainbow = true }
        },
        sprite = function(room,entity,viewport) if entity.version == 2 then return Draw(0,room,entity,viewport) else
            local _entity = utils.deepcopy(entity) 
            _entity.type = "tentacles"
            return spikeHelper.getSpikeSprites(_entity, "right")
        end end,
        selection = function(room,entity) return utils.rectangle(entity.x,entity.y,4,entity.height) end
    }
}

for _,h in ipairs(ret) do
    ret.rotate = vivUtil.getNameRotationHandler({"VivHelper/AnimatedSpikesUp","VivHelper/AnimatedSpikesRight","VivHelper/AnimatedSpikesDown","VivHelper/AnimatedSpikesLeft"})
    ret.flip = vivUtil.getNameFlipHandler({"VivHelper/AnimatedSpikesUp","VivHelper/AnimatedSpikesRight","VivHelper/AnimatedSpikesDown","VivHelper/AnimatedSpikesLeft"})
end
return ret