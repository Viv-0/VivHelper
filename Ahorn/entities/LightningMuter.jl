module VivHelperLightningMuter
using ..Ahorn, Maple

@mapdef Entity "VivHelper/LightningMuter" LM(x::Integer, y::Integer, flag::String="LightningMuter")

const placements = Ahorn.PlacementDict(
    "Lightning Muter (on Flag) (Viv's Helper)" => Ahorn.EntityPlacement(LM)
)

function Ahorn.selection(entity::LM)
    x,y = Ahorn.position(entity);
    return Ahorn.Rectangle(x-8,y-8,16,16)
end

sprite = "ahorn/VivHelper/mute.png"
const lightningFillColor = (0.55, 0.97, 0.96, 0.4)
const lightningBorderColor = (0.99, 0.96, 0.47, 1.0)

function renderLightningBlock(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number)
    Ahorn.Cairo.save(ctx)

    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1)

    Ahorn.drawRectangle(ctx, x, y, width, height, lightningFillColor, lightningBorderColor)

    Ahorn.restore(ctx)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LM, room::Maple.Room)
    
    renderLightningBlock(ctx,-8,-8,16,16)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end 