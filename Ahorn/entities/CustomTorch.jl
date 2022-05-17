module VivHelperCustomTorch

using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CustomTorch" CustomTorch(
	x::Integer,
	y::Integer,
	startLit::Bool=false,
	Color::String="80ffff",
	spriteColor::String="80ffff",
	Alpha::Number=1.0,
	startFade::Integer=48,
	endFade::Integer=64,
	RegisterRadius::Number=4.0,
	unlightOnDeath::Bool=false
)

const placements = Ahorn.PlacementDict(
	"Colored Torch (Viv's Helper)" => Ahorn.EntityPlacement(
		CustomTorch
	)
)
Ahorn.editingOptions(entity::CustomTorch) = Dict{String, Any}(
    "Color" => VivHelper.XNAColors
)

function torchSprite(entity::CustomTorch)
	lit = get(entity.data, "startLit", false)

	return lit ? "ahorn/VivHelper/torch/grayTorchLit.png" : "ahorn/VivHelper/torch/grayTorchUnlit.png"
end

function Ahorn.selection(entity::CustomTorch)
	x, y = Ahorn.position(entity)
	sprite = torchSprite(entity)

	return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomTorch, room::Maple.Room)
	sprite = torchSprite(entity)
	tint1 = VivHelper.ColorFix(get(entity.data, "spriteColor", "ffffff"), 1.0)
	Ahorn.drawSprite(ctx, sprite, 0, 0, tint=tint1)
end

end
