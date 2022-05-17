module VivHelperCustomLightbeam

using ..Ahorn, Maple
using Random
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CustomLightbeam" CustomLightbeam(x::Integer, y::Integer,
width::Integer=32, height::Integer=24,
rotation::Integer=0, ChangeFlag::String="",
DisableFlag::String="", Color::String="White",
Alpha::Number=1.0, Texture::String="util/lightbeam", Depth::Integer=-9998, FadeWhenNear::Bool=true, NoParticles::Bool=false, DisableParticlesFlag::String="")

function lightbeamFinalizer(entity::CustomLightbeam)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.nodes[1])

    theta = atan(y - ny, x - nx) + pi / 2
    magnitude = sqrt((y - ny)^2 + (x - nx)^2)

    entity.rotation = round(Int, rad2deg(theta))
    entity.height = round(Int, max(24, magnitude))
end


const placements = Ahorn.PlacementDict(
    "Custom Lightbeam (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomLightbeam,
        "line",
        Dict{String, Any}(),
        lightbeamFinalizer
    )
)

Ahorn.minimumSize(entity::CustomLightbeam) = 32, 24
Ahorn.resizable(entity::CustomLightbeam) = true, true

Ahorn.editingOptions(entity::CustomLightbeam) = Dict{String, Any}(
    "Color" => VivHelper.XNAColors,
    "Depth" => merge(VivHelper.Depths, Dict{String, Any}("Default" => -9998))
)

function rotate(point::Tuple{Number, Number}, theta::Number)
    res = [
        cos(theta)  -sin(theta);
        sin(theta) cos(theta)
    ] * [point[1]; point[2]]

    return (res[1], res[2])
end

function getSelectionRect(entity::CustomLightbeam)
    x, y = Ahorn.position(entity)

    theta = deg2rad(get(entity.data, "rotation", 0))
    width = round(Int, get(entity.data, "width", 32))
    height = round(Int, get(entity.data, "height", 24))

    points = Tuple{Number, Number}[
        (-width / 2, 0),
        (width / 2, 0),
        (-width / 2, height - 4),
        (width / 2, height - 4)
    ]

    rotated = rotate.(points, theta)

    tlx, tly = minimum(p -> p[1], rotated), minimum(p -> p[2], rotated)
    brx, bry = maximum(p -> p[1], rotated), maximum(p -> p[2], rotated)

    return Ahorn.Rectangle(x + tlx, y + tly, brx - tlx, bry - tly)
end

function Ahorn.selection(entity::CustomLightbeam)
    return getSelectionRect(entity)
end

function renderLightbeam(ctx::Ahorn.Cairo.CairoContext, entity::CustomLightbeam)
    x, y = Ahorn.position(entity)
    texture = get(entity.data, "Texture", "util/lightbeam")
    lightbeam = Ahorn.getSprite(texture, "Gameplay")

    theta = deg2rad(get(entity.data, "rotation", 0))
    width = round(Int, get(entity.data, "width", 32))
    height = round(Int, get(entity.data, "height", 24))

    rng = Ahorn.getSimpleEntityRng(entity)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, x, y)
    Ahorn.rotate(ctx, theta)
    Ahorn.translate(ctx, width / 2, 0)
    Ahorn.rotate(ctx, pi / 2)
    Ahorn.scale(ctx, (height - 4) / lightbeam.width, 1)
    Ahorn.set_antialias(ctx, 1)

    Ahorn.Cairo.save(ctx)

    for i in 0:width - 1
        Ahorn.drawImage(ctx, texture, 0, i, alpha=0.3)
    end

    for i in 0:4:width - 1
        num = i * 0.6
        lineWidth = 4.0 + sin(num * 0.5 + 1.2) * 4.0
        length = height + sin(num * 0.25) * 8
        alpha = 0.6 + sin(num + 0.8) * 0.3
        offset = sin((num + i * 32) * 0.1 + sin(num * 0.05 + i * 0.1) * 0.25) * (width / 2.0 - lineWidth / 2.0)

        # Not part of official calculations, but makes the render a bit less boring
        offsetMultiplier = (rand(rng) - 0.5) * 2

        for j in 0:3
            Ahorn.drawImage(ctx, texture, 0, offset * offsetMultiplier + width / 2 + j, alpha=alpha / 1.5)
        end
    end

    Ahorn.Cairo.restore(ctx)
end

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomLightbeam, room::Maple.Room) = renderLightbeam(ctx, entity)

end
