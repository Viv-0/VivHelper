module VivHelperDashCodeHeartController
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/DashCodeHeartController" DashCodeHeartController(x::Integer, y::Integer, key::String="U,L,DR,UR,L,UL", spawnType::String="Reflection", multipleCheck::Bool=false, CompleteFlag::String="DashCode", GlitchLength::String="Medium", ClassName::String="", MethodName::String="", CustomParameters::String="")

const placements = Ahorn.PlacementDict(
    "Dash Code Heart Controller (Reflection) (Viv's Helper)" => Ahorn.EntityPlacement(
        DashCodeHeartController,
        "point",
        Dict{String, Any}("spawnType"=>"Reflection"),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + 48, Int(entity.data["y"]) + 48)]
        end
    ),
    "Dash Code Heart Controller (Forsaken City) (Viv's Helper)" => Ahorn.EntityPlacement(
        DashCodeHeartController,
        "point",
        Dict{String, Any}("spawnType"=>"ForsakenCity"),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + 48, Int(entity.data["y"]) + 48)]
        end
    ),
    "Dash Code Heart Controller (Level Up) (Viv's Helper)" => Ahorn.EntityPlacement(
        DashCodeHeartController,
        "point",
        Dict{String, Any}("spawnType"=>"LevelUp", "Color"=>"White")
    ),
    "Dash Code Heart Controller (Flash) (Viv's Helper)" => Ahorn.EntityPlacement(
        DashCodeHeartController,
        "point",
        Dict{String, Any}("spawnType"=>"FlashSpawn", "Color"=>"White")
    ),
    "Dash Code Heart Controller (Glitch) (Viv's Helper)" => Ahorn.EntityPlacement(
        DashCodeHeartController,
        "point",
        Dict{String, Any}("spawnType"=>"GlitchSpawn")
    ),
    "Dash Code Heart Controller (Custom Code) (Viv's Helper)" => Ahorn.EntityPlacement(
        DashCodeHeartController,
        "point",
        Dict{String, Any}("spawnType"=>"Custom")
    )
)


const spawnTypes = Dict{String, String}(
    "Reflection" => "Reflection",
    "Forsaken City" => "ForsakenCity",
    "Level Up" => "LevelUp",
    "Flash" => "FlashSpawn",
    "Glitch" => "GlitchSpawn",
    "Custom Code (See Docs)" => "Custom"
)

Ahorn.editingOptions(entity::DashCodeHeartController) = Dict{String, Any}(
    "spawnType" => spawnTypes,
    "Color" => VivHelper.XNAColors,
    "GlitchLength" => String["Short", "Medium", "Long", "Glyph"]
)

function Ahorn.editingIgnored(entity::DashCodeHeartController, multiple::Bool=false)
    if multiple
        # We only want multipleCheck to appear because you currently cannot use multiple of these in a room.
        return String["x","y","nodes","key","spawnType","Color","GlitchLength", "CustomParameters", "ClassName", "MethodName"]
    end
    sT = get(entity.data, "spawnType", "Reflection")
    if sT == "LevelUp"
        return String["multipleCheck", "GlitchLength", "CustomParameters", "ClassName", "MethodName"]
    elseif sT == "FlashSpawn"
        return String["multipleCheck", "GlitchLength", "nodes", "CustomParameters", "ClassName", "MethodName"]
    elseif sT == "GlitchSpawn"
        return String["multipleCheck", "Color", "nodes", "CustomParameters", "ClassName", "MethodName"]
    elseif sT == "Custom"
        return String["multipleCheck", "Color", "GlitchLength"]
    else
        return String["multipleCheck", "GlitchLength", "Color", "CustomParameters", "ClassName", "MethodName"]
    end
end
function Ahorn.nodeLimits(entity::DashCodeHeartController)
    sT = get(entity.data, "spawnType", "Reflection")
    if sT == "Custom"
        return 0, -1
    else
        return 0, 1
    end
end

function Ahorn.selection(entity::DashCodeHeartController)
    res = [Ahorn.Rectangle(entity.data["x"]-16, entity.data["y"]-16, 32, 32)]
    nodes = get(entity.data, "nodes", ())
    if !isempty(nodes)
        
        for node in nodes
            nx, ny = Int.(node)
            push!(res, Ahorn.Rectangle(nx-2, ny-2, 4, 4))
        end
    end
    return res
end 

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::DashCodeHeartController)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    if !isempty(nodes)
        for node in nodes
            nx, ny = Int.(node)

            theta = atan(y - ny, x - nx)
            Ahorn.drawArrow(ctx, x, y, nx + cos(theta) * 2, ny + sin(theta) * 2, Ahorn.colors.selection_selected_fc, headLength=4)
            Ahorn.drawRectangle(ctx, nx-2, ny-2, 4, 4, (205, 205, 205, 255)./255)
        end
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashCodeHeartController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/VivHelper/heartCodeController", 0, 0)

end