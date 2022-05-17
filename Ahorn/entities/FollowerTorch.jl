module VivHelperFollowTorch
using ..Ahorn, Maple, Random

@mapdef Entity "VivHelper/FollowTorch" Torchlight(x::Integer, y::Integer,
FadePoint::Integer=48, Radius::Integer=64, Alpha::Number=1.0,
Color::String="Default", followDelay::Number=0.2)

const placements = Ahorn.PlacementDict(
   "Torch Follower (Viv's Helper)" => Ahorn.EntityPlacement(
      Torchlight
   )
)

function getSprite(entity::Torchlight)
   s = "FollowTorchSprites/ThorcVar/"
   t = get(entity.data, "Color", "Default")
   u = string(s, t, "Torch/", t, "Torch00.png")
   return u
end

Ahorn.editingOptions(entity::Torchlight) = Dict{String, Any}(
  "Color" => String["Default", "Red", "Orange", "Green", "Blue", "Purple", "Sunset", "Gray"]
)

function Ahorn.selection(entity::Torchlight)
   x, y = Ahorn.position(entity)
   z = getSprite(entity)
   return Ahorn.getSpriteRectangle(z, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Torchlight, room::Maple.Room) = Ahorn.drawSprite(ctx, getSprite(entity), 0, 0)

end
