module VivHelperCBDNTCB
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CBDNCrackedBlock" CBDNTCB(x::Integer, y::Integer,
    width::Integer=16, height::Integer=16, persistent::Bool=false
)

const placements = Ahorn.PlacementDict(
    "Neutralizer Cracked Block (Viv's Helper)" => Ahorn.EntityPlacement(
        CBDNTCB,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::CBDNTCB) = 16, 16
Ahorn.resizable(entity::CBDNTCB) = true, true

Ahorn.selection(entity::CBDNTCB) = Ahorn.getEntityRectangle(entity)

frame = "VivHelper/CBDNTempleCrackedBlock/breakBlock00"

# Not the prettiest code, but it works
function rendertempleCrackedBlock(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number)
    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    Ahorn.Cairo.save(ctx)

    Ahorn.rectangle(ctx, 0, 0, width, height)
    Ahorn.clip(ctx)

    for i in 0:ceil(Int, tilesWidth / 4)
        Ahorn.drawImage(ctx, frame, i * 32 + 8, 0, 8, 0, 32, 8)

        for j in 0:ceil(Int, tilesHeight / 4)
            Ahorn.drawImage(ctx, frame, i * 32 + 8, j * 32 + 8, 8, 8, 32, 32)

            Ahorn.drawImage(ctx, frame, 0, j * 32 + 8, 0, 8, 8, 32)
            Ahorn.drawImage(ctx, frame, width - 8, j * 32 + 8, 40, 8, 8, 32)
        end

        Ahorn.drawImage(ctx, frame, i * 32 + 8, height - 8, 8, 40, 32, 8)
    end

    Ahorn.drawImage(ctx, frame, 0, 0, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, 0, 40, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, 0, height - 8, 0, 40, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, height - 8, 40, 40, 8, 8)

    Ahorn.restore(ctx)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CBDNTCB, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    rendertempleCrackedBlock(ctx, x, y, width, height)
end

end
