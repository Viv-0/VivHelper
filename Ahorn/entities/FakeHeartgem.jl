module VivHelperFakeHeartGem

using ..Ahorn, Maple

@mapdef Entity "VivHelper/FakeHeartGem" AsleepHelperCustomHeartGemPlacement(
	x::Integer, y::Integer,
	fake::Bool=true, music::String="",
	removeCameraTriggers::Bool=false, ShatterSpinnersOnBreak::Bool=false,
	ChangeRespawnOnCollect::Bool=false
)

const placements = Ahorn.PlacementDict(
	"Fake Heart Gem (Blue) (Viv's Helper)" => Ahorn.EntityPlacement(
		AsleepHelperCustomHeartGemPlacement
	)
)

sprite = "collectables/heartGem/0/00.png"

function Ahorn.selection(entity::AsleepHelperCustomHeartGemPlacement)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AsleepHelperCustomHeartGemPlacement, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
