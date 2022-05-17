module VivHelperNWayDashBlock
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/nWayDashBlock" NWayDashBlock(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tiletype::String="3",
blendin::Bool=true, canDash::Bool=true, permanent::Bool=true, Left::Bool=true, Right::Bool=true, Up::Bool=true, Down::Bool=true, DetailColor::String="Black"
)

const placements = Ahorn.PlacementDict(
    "N-Way Dash Block (Viv's Helper)" => Ahorn.EntityPlacement(
        NWayDashBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::NWayDashBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
    "DetailColor" => VivHelper.XNAColors
)
Ahorn.minimumSize(entity::NWayDashBlock) = 8, 8
Ahorn.resizable(entity::NWayDashBlock) = true, true

Ahorn.selection(entity::NWayDashBlock) = Ahorn.getEntityRectangle(entity)



function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::NWayDashBlock, room::Maple.Room)
    Ahorn.drawTileEntity(ctx, room, entity)
    x = get(entity.data, "x", 0)
    y = get(entity.data, "y", 0)
    w = get(entity.data, "width", 8)
    h = get(entity.data, "height", 8)
    c = get(entity.data, "DetailColor", "Black")
    l = get(entity.data, "Left", false)
    r = get(entity.data, "Right", false)
    u = get(entity.data, "Up", false)
    d = get(entity.data, "Down", false)
    for i = 1:6
        C = VivHelper.ColorFix(c, 0.6 - 0.1 * i)
        if u
            Ahorn.drawRectangle(ctx, x + i, y + i - 1, w - (2*i), 1, C);
        end
        if d
            Ahorn.drawRectangle(ctx, x + i, y + h - i, w - (2*i), 1, C);
        end
        if l
            Ahorn.drawRectangle(ctx, x + i - 1, y + i, 1, h - (2*i), C);
        end
        if r
            Ahorn.drawRectangle(ctx, x + w - i, y + i, 1, h - (2*i), C);
        end
    end
    dashColor = VivHelper.ColorFix("AC3232", 0.8);
    if u
        Ahorn.drawArrow(ctx, x + w / 2, y - 12, x + w / 2, y, dashColor, headLength=5)
    end
    if d
        Ahorn.drawArrow(ctx, x + w / 2, y + h + 12, x + w / 2, y + h, dashColor, headLength=5)
    end
    if l
        Ahorn.drawArrow(ctx, x - 12, y + h / 2, x, y + h / 2, dashColor, headLength=5)
    end
    if r
        Ahorn.drawArrow(ctx, x + w + 12, y + h / 2, x + w, y + h / 2, dashColor, headLength=5)
    end

end
end
