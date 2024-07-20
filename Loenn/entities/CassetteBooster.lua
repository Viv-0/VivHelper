

local cassetteBooster = {
    name = "VivHelper/CassetteBooster",
    texture = "ahorn/VivHelper/cassettebubble",
    placements = {
        name = "cb",
        data = {
            log2idx = 10,
            distance = 3,
            spriteXML = ""
        }
    },
    fieldInformation = {
        log2idx = { fieldType = "integer", options = {
            {"GGGG", 0}, {"RGGG", 1}, {"GRGG", 2}, {"RRGG", 3}, {"GGRG", 4},
            {"RGRG", 5}, {"GRRG", 6}, {"RRRG", 7}, {"GGGR", 8}, {"RGGR", 9},
            {"GRGR", 10}, {"RRGR", 11}, {"GGRR", 12}, {"RGRR", 13}, {"GRRR", 14}, {"RRRR", 15}
        }, minimumValue = 0, maximumValue = 255},
        distance = {fieldType = "number", minimumValue = 0 }
    }
}

return cassetteBooster