module VivHelperCustomCoreBlock
using ..Ahorn, Maple

@mapdef Entity "VivHelper/CustomCoreBlock" CustomBounceBlock(x::Integer, y::Integer,
width::Integer=16, height::Integer=16,
CoreState::String="None"
)

const placements = Ahorn.PlacementDict(
    "Reskinnable Bounce Block (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomBounceBlock,
        "rectangle",
        Dict{String, Any}(
            "Directory" => "objects/BumpBlockNew",
            "HotParticleColors" => "",
            "ColdParticleColors" => ""
        )
    ),
    "Custom Bounce Block (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomBounceBlock,
        "rectangle",
        Dict{String, Any}(
            "WindUpDist" => 10.0,
            "IceWindUpDist" => 16.0,
            "BounceDist" => 24.0,
            "LiftSpeedXMult" => 0.75,
            "WallPushTime" => 0.1,
            "BounceEndTime" => 0.05,
            "RespawnTime" => 1.6
        )
    ),
    "Custom Bounce Block (Reskinnable) (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomBounceBlock,
        "rectangle",
        Dict{String, Any}(
            "WindUpDist" => 10.0,
            "IceWindUpDist" => 16.0,
            "BounceDist" => 24.0,
            "LiftSpeedXMult" => 0.75,
            "WallPushTime" => 0.1,
            "BounceEndTime" => 0.05,
            "RespawnTime" => 1.6,
            "Directory" => "objects/BumpBlockNew",
            "HotParticleColors" => "",
            "ColdParticleColors" => ""
        )
    )
)

Ahorn.editingOptions(entity::CustomBounceBlock) = Dict{String, Any}(
    "CoreState" => Dict{String, String}(
        "Swap" => "None",
        "Hot" => "Hot",
        "Cold" => "Cold"
    )
)

Ahorn.minimumSize(entity::CustomBounceBlock) = 16, 16
Ahorn.resizable(entity::CustomBounceBlock) = true, true

Ahorn.selection(entity::CustomBounceBlock) = Ahorn.getEntityRectangle(entity)

getSprites(dir::String) = [Ahorn.getSprite(dir * "00", "Gameplay"), Ahorn.getSprite(dir * "_center00", "Gameplay")]

function renderBounceBlock(ctx::Ahorn.Cairo.CairoContext, entity::CustomBounceBlock, x::Number, y::Number, width::Number, height::Number)
    dir = get(entity.data, "Directory", "objects/BumpBlockNew"); if (endswith(dir, "/")) chop(dir); end
    dir = string(dir, (get(entity.data, "CoreState", "None") == "Cold" ? "/ice" : "/fire"));
    frameResource, crystalSprite = getSprites(dir)

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    Ahorn.Cairo.save(ctx)

    Ahorn.rectangle(ctx, 0, 0, width, height)
    Ahorn.clip(ctx)

    for i in 0:ceil(Int, tilesWidth / 6)
        Ahorn.drawImage(ctx, frameResource, i * 48 + 8, 0, 8, 0, 48, 8)

        for j in 0:ceil(Int, tilesHeight / 6)
            Ahorn.drawImage(ctx, frameResource, i * 48 + 8, j * 48 + 8, 8, 8, 48, 48)

            Ahorn.drawImage(ctx, frameResource, 0, j * 48 + 8, 0, 8, 8, 48)
            Ahorn.drawImage(ctx, frameResource, width - 8, j * 48 + 8, 56, 8, 8, 48)
        end

        Ahorn.drawImage(ctx, frameResource, i * 48 + 8, height - 8, 8, 56, 48, 8)
    end

    Ahorn.drawImage(ctx, frameResource, 0, 0, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frameResource, width - 8, 0, 56, 0, 8, 8)
    Ahorn.drawImage(ctx, frameResource, 0, height - 8, 0, 56, 8, 8)
    Ahorn.drawImage(ctx, frameResource, width - 8, height - 8, 56, 56, 8, 8)
    
    Ahorn.drawImage(ctx, crystalSprite, div(width - crystalSprite.width, 2), div(height - crystalSprite.height, 2))

    Ahorn.restore(ctx)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomBounceBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderBounceBlock(ctx, entity, x, y, width, height)
end

end
