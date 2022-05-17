module VivHelperCurvedZipMover
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CurvedZipMover" LegacyZipMover(x::Integer, y::Integer, width::Integer=24, height::Integer=24,
    CurveIdentifier::String="curvedPath1", CustomSpritePath::String="objects/zipmover/", RopeColor::String="663931",
    RopeNotchColor::String="9b6157", EaseType::String="SineIn", MoveType::Bool=true, LightOcclusion::Number=1.0,
    Uniform::Bool=false, Reverse::Bool=false, Slingshot::Bool=false, SpeedModF::Number=1.0, SpeedModB::Number=1.0,
    PostLaunchPause::Number=0.5, BaseColor::String="ffffff", AudioOnLaunch::String="event:/game/01_forsaken_city/zip_mover",
    DrawBlackBorder::Bool=true
)

@mapdef Entity "VivHelper/CurvedZipMover2" CurvedZipMover(x::Integer, y::Integer, width::Integer=24, height::Integer=24,
    CurveIdentifier::String="curvedPath1", CustomSpritePath::String="objects/zipmover/", RopeColor::String="663931",
    RopeNotchColor::String="9b6157", EaseType::String="SineIn", LightOcclusion::Number=1.0,
    Uniform::Bool=false, Reverse::Bool=false, Slingshot::Bool=false, SpeedModF::Number=1.0, SpeedModB::Number=1.0,
    PostLaunchPause::Number=0.5, BaseColor::String="ffffff", AudioOnLaunch::String="event:/game/01_forsaken_city/zip_mover",
    DrawBlackBorder::Bool=true
)

@mapdef Entity "VivHelper/CustomZipMover" CustomZipMover(x::Integer, y::Integer, width::Integer=24, height::Integer=24,
    CurveIdentifier::String="curvedPath1", CustomSpritePath::String="objects/zipmover/", RopeColor::String="663931",
    RopeNotchColor::String="9b6157", EaseType::String="SineIn", LightOcclusion::Number=1.0,
    Uniform::Bool=false, Reverse::Bool=false, Slingshot::Bool=false, SpeedModF::Number=1.0, SpeedModB::Number=1.0,
    PostLaunchPause::Number=0.5, BaseColor::String="ffffff", AudioOnLaunch::String="event:/game/01_forsaken_city/zip_mover",
    DrawBlackBorder::Bool=true
)

@mapdef Entity "VivHelper/CrumblingZipMover" CrumbleZipMover(x::Integer, y::Integer, width::Integer=24, height::Integer=24,
    CurveIdentifier::String="curvedPath1", CustomSpritePath::String="objects/zipmover/", RopeColor::String="663931",
    RopeNotchColor::String="9b6157", EaseType::String="SineIn", MoveType::Bool=true, LightOcclusion::Number=1.0,
    Uniform::Bool=false, Reverse::Bool=false, Slingshot::Bool=false, SpeedModF::Number=1.0, SpeedModB::Number=1.0,
    PostLaunchPause::Number=0.5, BaseColor::String="ffffff",
    CrumbleTimer::Number=0.5, RespawnCount::Integer=-1, RespawnDelay::Number=3.0, DebrisAmount::String="Normal", AudioOnLaunch::String="event:/game/01_forsaken_city/zip_mover",
    DrawBlackBorder::Bool=true
)

@mapdef Entity "VivHelper/CurvedCrumblingZipMover" CrumbleCurvedZipMover(x::Integer, y::Integer, width::Integer=24, height::Integer=24,
    CurveIdentifier::String="curvedPath1", CustomSpritePath::String="objects/zipmover/", RopeColor::String="663931",
    RopeNotchColor::String="9b6157", EaseType::String="SineIn", LightOcclusion::Number=1.0,
    Uniform::Bool=false, Reverse::Bool=false, Slingshot::Bool=false, SpeedModF::Number=1.0, SpeedModB::Number=1.0,
    PostLaunchPause::Number=0.5, BaseColor::String="ffffff",
    CrumbleTimer::Number=0.5, RespawnCount::Integer=-1, RespawnDelay::Number=3.0, DebrisAmount::String="Normal", AudioOnLaunch::String="event:/game/01_forsaken_city/zip_mover",
    DrawBlackBorder::Bool=true
)

@mapdef Entity "VivHelper/CustomCrumblingZipMover" CrumbleCustomZipMover(x::Integer, y::Integer, width::Integer=24, height::Integer=24,
    CurveIdentifier::String="curvedPath1", CustomSpritePath::String="objects/zipmover/", RopeColor::String="663931",
    RopeNotchColor::String="9b6157", EaseType::String="SineIn", LightOcclusion::Number=1.0,
    Uniform::Bool=false, Reverse::Bool=false, Slingshot::Bool=false, SpeedModF::Number=1.0, SpeedModB::Number=1.0,
    PostLaunchPause::Number=0.5, BaseColor::String="ffffff",
    CrumbleTimer::Number=0.5, RespawnCount::Integer=-1, RespawnDelay::Number=3.0, DebrisAmount::String="Normal", AudioOnLaunch::String="event:/game/01_forsaken_city/zip_mover",
    DrawBlackBorder::Bool=true
)

const placements = Ahorn.PlacementDict(
    "~ Legacy Curved Zip Mover (Viv's Helper)" => Ahorn.EntityPlacement(
        LegacyZipMover,
        "rectangle"
    ),

    "Curved Zip Mover (Viv's Helper)" => Ahorn.EntityPlacement(
	CurvedZipMover,
	"rectangle"
    ),

    "Custom Zip Mover (Viv's Helper)" => Ahorn.EntityPlacement(
	CustomZipMover,
	"rectangle",
	Dict{String, Any}(),
	function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        end
    ),

    "~ Legacy Zip Mover (Crumbles) (Viv's Helper)" => Ahorn.EntityPlacement(
	CrumbleZipMover,
	"rectangle"
    ),

    "Curved Zip Mover (Crumbles) (Viv's Helper)" => Ahorn.EntityPlacement(
	CrumbleCurvedZipMover,
	"rectangle"
    ),

    "Custom Zip Mover (Crumbles) (Viv's Helper)" => Ahorn.EntityPlacement(
	CrumbleCustomZipMover,
	"rectangle",
	Dict{String, Any}(),
	function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        end
    )
)

ZipMovers = Union{LegacyZipMover, CurvedZipMover, CustomZipMover, CrumbleZipMover, CrumbleCustomZipMover, CrumbleCurvedZipMover}
OldZipMovers = Union{LegacyZipMover, CrumbleZipMover}
NewZipMovers = Union{CustomZipMover, CurvedZipMover, CrumbleCustomZipMover, CrumbleCurvedZipMover}
NewZipMovers1 = Union{CustomZipMover, CrumbleCustomZipMover} # linear
NewZipMovers0 = Union{CurvedZipMover, CrumbleCurvedZipMover} # curved

Ahorn.nodeLimits(entity::OldZipMovers) = get(entity.data, "MoveType", "Bezier") ? (1, 1) : (0, 0)
Ahorn.nodeLimits(entity::NewZipMovers0) = 0,0
Ahorn.nodeLimits(entity::NewZipMovers1) = 1,1
Ahorn.minimumSize(entity::ZipMovers) = 16, 16
Ahorn.resizable(entity::ZipMovers) = true, true

Ahorn.editingOptions(entity::LegacyZipMover) = Dict{String, Any}(
    "BaseColor" => VivHelper.XNAColors,
    "RopeColor" => VivHelper.XNAColors,
    "RopeNotchColor" => VivHelper.XNAColors,
    "CustomSpritePath" => Dict{String, String}( "default" => "", "moon" => "objects/zipmover/moon/"),
    "MoveType" => Dict{String, Bool}( "Bezier" => true, "Linear" => false ),
    "EaseType" => VivHelper.EaseTypes
)

Ahorn.editingOptions(entity::CrumbleZipMover) = Dict{String, Any}(
    "BaseColor" => VivHelper.XNAColors,
    "RopeColor" => VivHelper.XNAColors,
    "RopeNotchColor" => VivHelper.XNAColors,
    "MoveType" => Dict{String, Bool}( "Bezier" => true, "Linear" => false ),
    "CustomSpritePath" => Dict{String, String}( "default" => "", "moon" => "objects/zipmover/moon/"),
    "EaseType" => VivHelper.EaseTypes,
    "DebrisAmount" => String["Normal", "Half", "Quarter", "Eighth", "None"]
)

Ahorn.editingOptions(entity::CustomZipMover) = Dict{String, Any}(
    "BaseColor" => VivHelper.XNAColors,
    "RopeColor" => VivHelper.XNAColors,
    "RopeNotchColor" => VivHelper.XNAColors,
    "CustomSpritePath" => Dict{String, String}( "default" => "", "moon" => "objects/zipmover/moon/"),
    "EaseType" => VivHelper.EaseTypes
)
Ahorn.editingOptions(entity::CurvedZipMover) = Dict{String, Any}(
    "BaseColor" => VivHelper.XNAColors,
    "RopeColor" => VivHelper.XNAColors,
    "RopeNotchColor" => VivHelper.XNAColors,
    "CustomSpritePath" => Dict{String, String}( "default" => "", "moon" => "objects/zipmover/moon/"),
    "EaseType" => VivHelper.EaseTypes
)
Ahorn.editingOptions(entity::CrumbleCustomZipMover) = Dict{String, Any}(
    "BaseColor" => VivHelper.XNAColors,
    "RopeColor" => VivHelper.XNAColors,
    "RopeNotchColor" => VivHelper.XNAColors,
    "CustomSpritePath" => Dict{String, String}( "default" => "", "moon" => "objects/zipmover/moon/"),
    "EaseType" => VivHelper.EaseTypes,
    "DebrisAmount" => String["Normal", "Half", "Quarter", "Eighth", "None"]
)
Ahorn.editingOptions(entity::CrumbleCurvedZipMover) = Dict{String, Any}(
    "BaseColor" => VivHelper.XNAColors,
    "RopeColor" => VivHelper.XNAColors,
    "RopeNotchColor" => VivHelper.XNAColors,
    "CustomSpritePath" => Dict{String, String}( "default" => "", "moon" => "objects/zipmover/moon/"),
    "EaseType" => VivHelper.EaseTypes,
    "DebrisAmount" => String["Normal", "Half", "Quarter", "Eighth", "None"]
)

function Ahorn.selection(entity::OldZipMovers)
    x, y = Ahorn.position(entity)
    b = get(entity.data, "MoveType", true)
    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))
    if b
        return Ahorn.Rectangle(x, y, width, height)
    else
        nx, ny = Int.(get(entity.data, "nodes", ())[1])
        return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx + floor(Int, width / 2) - 5, ny + floor(Int, height / 2) - 5, 10, 10)]
    end
end

# Curved Zip Movers

function Ahorn.selection(entity::NewZipMovers0)
	x, y = Ahorn.position(entity)
	# b is always true
    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))
    return Ahorn.Rectangle(x, y, width, height)
end

# Custom Zip Movers (Linear)

function Ahorn.selection(entity::NewZipMovers1)
	x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))
    nx, ny = Int.(get(entity.data, "nodes", ())[1])
    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx + floor(Int, width / 2) - 5, ny + floor(Int, height / 2) - 5, 10, 10)]
end

getTextures(t::String) = (string(t, "block"), string(t, "light01"), string(t, "cog"))

# This works for all ZipMovers

function renderZipMover(ctx::Ahorn.Cairo.CairoContext, entity::ZipMovers)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    block, light, cog = getTextures(get(entity.data, "CustomSpritePath", ""))
    lightSprite = Ahorn.getSprite(light, "Gameplay")

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    cx, cy = x + width / 2, y + height / 2
    cnx, cny = nx + width / 2, ny + height / 2

    length = sqrt((x - nx)^2 + (y - ny)^2)
    theta = atan(cny - cy, cnx - cx)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, cx, cy)
    Ahorn.rotate(ctx, theta)

    Ahorn.setSourceColor(ctx, VivHelper.ColorFix(get(entity.data, "RopeColor", "663931"), 1.0))
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    # Offset for rounding errors
    Ahorn.move_to(ctx, 0, 4 + (theta <= 0))
    Ahorn.line_to(ctx, length, 4 + (theta <= 0))

    Ahorn.move_to(ctx, 0, -4 - (theta > 0))
    Ahorn.line_to(ctx, length, -4 - (theta > 0))

    Ahorn.stroke(ctx)

    Ahorn.Cairo.restore(ctx)

    Ahorn.drawRectangle(ctx, x + 2, y + 2, width - 4, height - 4, (0.0, 0.0, 0.0, 1.0))
    Ahorn.drawSprite(ctx, cog, cnx, cny)
    renderZipMoverBlock(ctx, entity)
end

# This works for all ZipMovers

function renderZipMoverBlock(ctx::Ahorn.Cairo.CairoContext, entity::ZipMovers)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))
    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)
    block, light, cog = getTextures(get(entity.data, "CustomSpritePath", ""))
    lightSprite = Ahorn.getSprite(light, "Gameplay")


    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, block, x + (i - 1) * 8, y, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, block, x + (i - 1) * 8, y + height - 8, 8, 16, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, block, x, y + (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, block, x + width - 8, y + (i - 1) * 8, 16, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, block, x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, block, x + width - 8, y, 16, 0, 8, 8)
    Ahorn.drawImage(ctx, block, x, y + height - 8, 0, 16, 8, 8)
    Ahorn.drawImage(ctx, block, x + width - 8, y + height - 8, 16, 16, 8, 8)

    Ahorn.drawImage(ctx, lightSprite, x + floor(Int, (width - lightSprite.width) / 2), y)
end

# Legacy Renderer

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::OldZipMovers, room::Maple.Room)
    x,y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 32))
    b = get(entity.data, "MoveType", true)
	# Goddamn, do not do this in the future, it's terrible
    if b && !isempty(get(entity.data, "nodes", ()))
        pop!(entity.data["nodes"])
    elseif !b && isempty(get(entity.data, "nodes",()))
        push!(entity.data["nodes"], (x + width + 8, y))
    end

    if b

        height = Int(get(entity.data, "height", 32))
        curves = filter(f -> f.name == "VivHelper/CurveEntity", room.entities)
        d = ""
        cx, cy = -1000, -1000
        for c in curves
            d = get(c.data, "Identifier", "")
            if d == get(entity.data, "CurveIdentifier", "") && d != ""
                cx, cy = Ahorn.position(c)
                break;
            end
        end
        if(!(cx == -1000))
            color = (0.0,0.0,0.0,0.3)
            Ahorn.drawArrow(ctx, x + width/2, y + height/2, cx, cy, color, headLength=4)
        end
        renderZipMoverBlock(ctx, entity)
    else
        renderZipMover(ctx, entity)

    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::NewZipMovers0, room::Maple.Room)
	x,y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 32))

	# MoveType is always Curved
	height = Int(get(entity.data, "height", 32))
	curves = filter(f -> f.name == "VivHelper/CurveEntity", room.entities)
	d = ""
	cx, cy = -1000, -1000
	for c in curves
		d = get(c.data, "Identifier", "")
		if d == get(entity.data, "CurveIdentifier", "") && d != ""
			cx, cy = Ahorn.position(c)
			break;
		end
	end
	if(!(cx == -1000))
		color = (0.0,0.0,0.0,0.3)
		Ahorn.drawArrow(ctx, x + width/2, y + height/2, cx, cy, color, headLength=4)
	end
	renderZipMoverBlock(ctx, entity)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::NewZipMovers1, room::Maple.Room)
    x,y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 32))
	renderZipMover(ctx, entity)
end

end
