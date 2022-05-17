module VivHelper_CelsiusGrowBlock
using ..Ahorn, Maple 

@mapdef Entity "VivHelper/CelsiusGrowBlock" CelsiusGrowBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tiletype::String="3", moveX::Number=0.0, moveY::Number=1.0, flag::String="")

const placements = Ahorn.PlacementDict(
    "Grow Block (Viv's Helper)" => Ahorn.EntityPlacement(
        CelsiusGrowBlock,
        "rectangle",
        Dict{String, Any}("factor"=>56),
        Ahorn.tileEntityFinalizer
    )
)
Ahorn.minimumSize(entity::CelsiusGrowBlock) = 8, 8
Ahorn.resizable(entity::CelsiusGrowBlock) = true, true
Ahorn.nodeLimits(entity::CelsiusGrowBlock) = 0, -1





Ahorn.editingOptions(entity::CelsiusGrowBlock) = Dict{String, Any}(
	"tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.selection(entity::CelsiusGrowBlock) = Ahorn.getEntityRectangle(entity)

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CelsiusGrowBlock, room::Maple.Room)
	Ahorn.drawTileEntity(ctx, room, entity)
	x, y = Ahorn.position(entity)
	width = get(entity.data, "width", 8); height = get(entity.data, "height", 8);
	Ahorn.drawSprite(ctx, "ahorn/VivHelper/growEdge", x+2, y+2; sx=1, sy=1)
    Ahorn.drawSprite(ctx, "ahorn/VivHelper/growEdge", x+width-2, y+2; sx=-1, sy=1)
    Ahorn.drawSprite(ctx, "ahorn/VivHelper/growEdge", x+2, y+height-2; sx=1, sy=-1)
    Ahorn.drawSprite(ctx, "ahorn/VivHelper/growEdge", x+width-2, y+height-2; sx=-1, sy=-1)
end

end