module VivHelperFlagIntroCrusher
using ..Ahorn, Maple

@mapdef Entity "VivHelper/FlagIntroCrusher" FIntroCrusher(x::Integer, y::Integer,
width::Integer=8, height::Integer=8, tileType::String="3",
delay::Number=1.2, speed::Number=2.0, flags::String=""
)

const placements = Ahorn.PlacementDict(
    "Flag Intro Crusher (Viv's Helper)" => Ahorn.EntityPlacement(
        FIntroCrusher,
        "rectangle",
        Dict{String, Any}(),
        function(entity)
            Ahorn.tileEntityFinalizer(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 16, Int(entity.data["y"]))]
        end
    )
)

Ahorn.editingOptions(entity::FIntroCrusher) = Dict{String, Any}(
    "tileType" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::FIntroCrusher) = 8,8
Ahorn.resizable(entity::FIntroCrusher) = true, true
Ahorn.nodeLimits(entity::FIntroCrusher) = 1, 1

function Ahorn.selection(entity::FIntroCrusher)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx, ny, width, height)]
end

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FIntroCrusher, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, blendIn=false)

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::FIntroCrusher, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))
    
    if !isempty(nodes)
        nx, ny = Int.(nodes[1])
        cox, coy = floor(Int, width / 2), floor(Int, height / 2)

        material = get(entity.data, "tileType", "3")[1] 

        fakeTiles = Ahorn.createFakeTiles(room, nx, ny, width, height, material, blendIn=false)
        Ahorn.drawFakeTiles(ctx, room, fakeTiles, room.objTiles, true, nx, ny, clipEdges=true)
        Ahorn.drawArrow(ctx, x + cox, y + coy, nx + cox, ny + coy, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

end
