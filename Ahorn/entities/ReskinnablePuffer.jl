module VivHelperReskinnablePuffer
using ..Ahorn, Maple

@mapdef Entity "VivHelper/ReskinnablePuffer" ReskinPuffer(x::Integer, y::Integer, right::Bool=false, Directory::String="objects/puffer")

const placements = Ahorn.PlacementDict(
    "Reskinnable Puffer (Right) (Viv's Helper)" => Ahorn.EntityPlacement(
        ReskinPuffer,
        "point",
        Dict{String, Any}(
            "right" => true
        )
    ),
    "Reskinnable Puffer (Left) (Viv's Helper)" => Ahorn.EntityPlacement(
        ReskinPuffer,
        "point",
        Dict{String, Any}(
            "right" => false
        )
    )
)

function GetSprite(entity::ReskinPuffer)
    str = get(entity.data, "Directory", "objects/puffer")
    if endswith(str, "/")
        chop(str)
    end
    return string(str, "/idle00");
end

function Ahorn.selection(entity::ReskinPuffer)
    x, y = Ahorn.position(entity)
    scaleX = get(entity, "right", false) ? 1 : -1

    return Ahorn.getSpriteRectangle(GetSprite(entity), x, y, sx=scaleX)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReskinPuffer, room::Maple.Room)
    scaleX = get(entity, "right", false) ? 1 : -1

    Ahorn.drawSprite(ctx, GetSprite(entity), 0, 0, sx=scaleX)
end

function Ahorn.flipped(entity::ReskinPuffer, horizontal::Bool)
    if horizontal
        entity.right = !entity.right

        return entity
    end
end

end
