local utils = require("utils")
local vivUtils = require("mods").requireFromPlugin("libraries.vivUtil")

local defaultTextureFG = "VivHelper/customSpinner/white/fg_white00"
local defaultTextureBG = "VivHelper/customSpinner/white/bg_white00"

local customSpinner = {}

customSpinner.name = "VivHelper/CustomSpinner"
customSpinner.fieldInformation = {
    Type = {fieldType = "string", options = {{"White", "White"}, {"Rainbow", "RainbowClassic"}}, editable = true }
    -- Depths = {fieldType = "integer", options = depths ???}
    Color = {fieldType = "color", allowXNAColors = true},
    BorderColor = {fieldType = "color", allowXNAColors = true},
    ShatterColor = {fieldType = "color", allowXNAColors = true},
    HitboxType = {
        fieldType = "string",
        options = {
            {"Scaled Spinner (Default)", "C:6;0,0|R:16,4;-8,*1@-4"},
            {"Thin Rectangle", "C:6;0,0|R:16,*4;-8,*-3"},
            {"Normal Circle (No Rect)", "C:6;0,0"},
            {"Larger Circle (No Rect)", "C:8;0,0"},
            {"Upside Down Spinner", "C:6;0,0|R:16,4;-8,*-1"},
            {"Upside Down Spinner, Thin Rect", "C:6;0,0|R:16,*4;-8,*-1"}
        },
        editable = true
    }
    Directory =
    {
        fieldType = "string",
        options = {
            {"VivHelper/customSpinner/white", "VivHelper/customSpinner/white"},
            {"VivHelper/customSpinner/hi-res", "VivHelper/customSpinner/hi-res"},
            {"danger/crystal", "danger/crystal"},
    
        }
    }

}

customSpinner.placements = {
    name = "VivHelper/CustomSpinner",
    data = {
        AttachToSolid = true,
        Type = "white",
        Directory = "danger/crystal",
        Subdirectory = "white",
        Color = "FFFFFF",
        BorderColor = "000000",
        ShatterColor = "FFFFFF",
        HitboxType =  "C:6;0,0|R:16,4;-8,*1@-4",
        shatterOnDash = false,
        Scale = 1.0,
        ImageScale = 1.0,
        CustomDebris = false,
        DebrisToScale = true,
        ShatterFlag = "",
        isSeeded = false,
        Seed = -1,
        ignoreConnection = false,
        flagToggle = "",
        Depth = -8500
    }
}

function customSpinner.depth(room, entity, viewport) 
    return entity.Depth;
end

local function getSqDistWithScaling(e1, e2)
    local distSq = ((e2.x - e1.x) * (e2.x - e1.x)) + ((e2.y - e1.y) * (e2.y - e1.y))
    local compSq = 144 * (e2.Scale + e1.Scale) * (e2.Scale + e1.Scale);
    return distSq < compSq
end

-- Taken from FrostHelper
local function createConnectorsForSpinner(room, entity, bgSprite)
    local sprites = {}

    for i = 1, #room.entities, 1 do
        local e2 = room.entities[i]
        if e2 == entity then break end
        if e2._name == entity._name and e2.attachToSolid == entity.attachToSolid and getSqDistWithScaling(entity, e2) then
            local connector = vivUtil.copyTexture(bgSprite, (entity.x + e2.x) / 2, (entity.y + e2.y) / 2, false)
            connector.depth = entity.Depth + 1
            table.insert(sprites, connector)
        end
    end
    return sprites
end
