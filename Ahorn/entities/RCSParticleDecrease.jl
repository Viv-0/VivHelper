module VivHelperRefillCancelSpaceParticleDecrease
using ..Ahorn, Maple

@mapdef Entity "VivHelper/RefillSpaceParticleMod" RCSPM(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Refill Cancel Space - Particle Modifier (Viv's Helper)" => Ahorn.EntityPlacement(
        RCSPM
    )
)

sprite = "VivHelper/entities/particleBoxIdle00"

function Ahorn.selection(entity::RCSPM)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y, sx=1, jx=0, jy=0)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RCSPM, room::Maple.Room)

    Ahorn.drawSprite(ctx, sprite, 0, 0, sx=1, jx=0, jy=0)
end

end
