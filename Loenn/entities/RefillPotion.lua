return {
    name = "VivHelper/RefillPotion",
    placements = {
        name = "potion",
        data = {
            twoDash = false,
            heavy = false,
            shatterOnGround = false,
            useAlways = false
        }
    }
    texture = function(room, entity) return "VivHelper/Potions/PotRefill" .. (entity.twoDash and "Two" or "") .. "00" end 
}