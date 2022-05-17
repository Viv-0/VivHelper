module VivHelperTouchSwitchWall
using ..Ahorn, Maple

@mapdef Entity "VivHelper/TouchSwitchWall" TouchSwitchWall(x::Integer, y::Integer, width::Integer=16, height::Integer=16, AllowHoldables::Bool=true, AllowSeekers::Bool=true, DisableParticles::Bool=false)

const placements = Ahorn.PlacementDict(
    "Touch Switch Wall (Viv's Helper)" => Ahorn.EntityPlacement(
        TouchSwitchWall,
        "rectangle"
    )
)


Ahorn.minimumSize(entity::TouchSwitchWall) = 8, 8
Ahorn.resizable(entity::TouchSwitchWall) = true, true
Ahorn.selection(entity::TouchSwitchWall) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TouchSwitchWall, room::Maple.Room)
    sprite = "objects/touchswitch/icon00"
    width = get(entity.data, "width", 8); height = get(entity.data, "height", 8);
    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.0, 0.0, 0.0, 0.3), (1.0,1.0,1.0,0.5))
    Ahorn.drawSprite(ctx, sprite, width/2, height/2)
end

end
