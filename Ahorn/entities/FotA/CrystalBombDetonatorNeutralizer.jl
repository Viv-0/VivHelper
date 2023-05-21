module VivHelperCrystalBombDetonatorNeutralizer
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CrystalBombDetonatorNeutralizer" CBDN(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
"Crystal Bomb Detonator Neutralizer (Viv's Helper)" => Ahorn.EntityPlacement(
    CBDN,
    "point",
    Dict{String, Any}(
        "respawnTime" => 2.0,
        "explodeTime" => 1.0,
        "explodeOnSpawn" => false,
        "respawnOnExplode" => true,
        "breakDashBlocks" => false,
        "breakTempleCrackedBlocks" => true
    )
)
)

function Ahorn.selection(entity::CBDN)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle("VivHelper/crystalBombDetonatorNeutralizer/idle00.png", x, y, jx=0.5, jy=0.5)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CBDN) = Ahorn.drawSprite(ctx, "VivHelper/crystalBombDetonatorNeutralizer/idle00.png", 0, -3, jx=0.5, jy=0.5)

end
