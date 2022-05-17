module VivHelperVariantZipMover
using ..Ahorn, Maple

@mapdef Entity "VivHelper/VariantZipMover" VariantZipMover(x::Integer, y::Integer, width::Integer=8, height::Integer=8, BadelineZipMover::Bool=false)

const placements = Ahorn.PlacementDict(
	"Variant Zip Mover (Viv's Helper)" => Ahorn.EntityPlacement(
		VariantZipMover,
		"rectangle",
		Dict{String, Any}(),
		function(entity)
           	    entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        	end
	)
)

const colors = sort(collect(keys(Ahorn.XNAColors.colors)))

function ColorFix(v, alpha::Float64=1.0)
    if v in colors
        w = get(Ahorn.XNAColors.colors, v, (1.0, 1.0, 1.0, 1.0))
        return (w[1], w[2], w[3], alpha)
    else
        temp = Ahorn.argb32ToRGBATuple(parse(Int, v, base=16))[1:3] ./ 255
        color = (temp[1], temp[2], temp[3], alpha)
        return color
    end
    return (1.0, 1.0, 1.0, 1.0)
end


Ahorn.nodeLimits(entity::VariantZipMover) = 1, 1

Ahorn.minimumSize(entity::VariantZipMover) = 16, 16
Ahorn.resizable(entity::VariantZipMover) = true, true

function Ahorn.selection(entity::VariantZipMover)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx + floor(Int, width / 2) - 5, ny + floor(Int, height / 2) - 5, 10, 10)]
end

function getTexturesAndColor(entity::VariantZipMover)
    theme = get(entity.data, "BadelineZipMover", false)
    
    if theme
        return "VivHelper/VariantZip/baddyblock", "VivHelper/VariantZip/baddylight01", "objects/zipmover/cog", ColorFix("9B3FB5")
    end

    return "VivHelper/VariantZip/maddyblock", "VivHelper/VariantZip/maddylight01", "objects/zipmover/cog", ColorFix("AC3232")
end

function renderZipMover(ctx::Ahorn.Cairo.CairoContext, entity::VariantZipMover)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    block, light, cog, ropeColor = getTexturesAndColor(entity)
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

    Ahorn.setSourceColor(ctx, ropeColor)
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

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::VariantZipMover, room::Maple.Room)
    renderZipMover(ctx, entity)
end

end
