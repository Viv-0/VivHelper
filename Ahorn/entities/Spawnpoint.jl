module VivHelperSpawnpoints
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/Spawnpoint" Spawnpoint(x::Integer, y::Integer, ShowTexture::Bool=true, Texture::String="VivHelper/player_outline", Depth::Int=5000, flipX::Bool=false)

@mapdef Entity "VivHelper/InterRoomSpawner" SpawnpointBetweenRooms(x::Integer, y::Integer, tag::Integer=1, ShowTexture::Bool=true, Texture::String="VivHelper/player_outline", Depth::Int=5000, flipX::Bool=false)

@mapdef Entity "VivHelper/InterRoomSpawnTarget" SpawnpointTarget1(x::Integer, y::Integer, tag::Integer=1, ShowTexture::Bool=true, Texture::String="VivHelper/player_outline", Depth::Int=5000, flipX::Bool=false)
@mapdef Entity "VivHelper/InterRoomSpawnTarget2" SpawnpointTarget2(x::Integer, y::Integer, tag::Integer=1, ShowTexture::Bool=true, Texture::String="VivHelper/player_outline", Depth::Int=5000, flipX::Bool=false)

const colorSet = sort(collect(keys(Ahorn.XNAColors.colors)))

const placements = Ahorn.PlacementDict(
    "Player Spawn Point (Customizable, Viv's Helper)" => Ahorn.EntityPlacement(
        Spawnpoint,
        "point",
        Dict{String, Any}(
            "Color" => "White",
            "OutlineColor" => "Black",
            "HideInMap" => false,
            "NoResetRespawn" => false,
            "NoResetOnRetry" => false,
            "forceFacing" => true
        )
    ),
    "Player Spawn Point (No Room Reset, Viv's Helper)" => Ahorn.EntityPlacement(
        Spawnpoint,
        "point",
        Dict{String, Any}(
            "Color"=>"White",
            "OutlineColor" => "Black",
            "NoResetRespawn" => true,
            "NoResetOnRetry" => false,
            "forceFacing" => true
        )
    ),
    "Player Spawn Point (Hide Spawn In Debug, Viv's Helper)" => Ahorn.EntityPlacement(
        Spawnpoint,
        "point",
        Dict{String, Any}(
            "Color"=>"White",
            "OutlineColor" => "Black",
            "HideInMap" => true,
            "forceFacing" => true
        )
    ),
    "Teleport Spawn Point (Viv's Helper)" => Ahorn.EntityPlacement(
        SpawnpointBetweenRooms,
        "point",
        Dict{String, Any}("Color"=>"White","OutlineColor"=>"Black","Flags"=>"")
    ),
    "Teleport Spawn Target (Viv's Helper)" => Ahorn.EntityPlacement(
        SpawnpointTarget1,
        "point",
        Dict{String, Any}("Color"=>"White","OutlineColor"=>"Black", "forceFacing" => true)
    ),
    "Teleport Spawn Target (Otherwise Not Spawnpoint, Viv's Helper)" => Ahorn.EntityPlacement(
        SpawnpointTarget2,
        "point"
    )
)

const ErrorSprites = ["ahorn/VivHelper/error1", "ahorn/VivHelper/error2"]

Spawnpoints = Union{Spawnpoint, SpawnpointBetweenRooms, SpawnpointTarget1, SpawnpointTarget2}

Ahorn.editingOptions(entity::Spawnpoints) = Dict{String, Any}(
    "Color" => VivHelper.XNAColors,
    "OutlineColor" => VivHelper.XNAColors,
    "Depth" => merge(VivHelper.Depths, Dict{String, Any}("Default" => 5000))
)

modulo(x::Number, m::Number) = (x % m + m) % m;

function Ahorn.selection(entity::Spawnpoints)
    x, y = Ahorn.position(entity)
    s = get(entity.data, "Texture", "VivHelper/player_outline")
    if isempty(s)
        s = "VivHelper/player_outline";
    end
    return Ahorn.getSpriteRectangle(s, x, y, jx=0.5, jy=1.0)
end


function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Spawnpoint) 
    s = get(entity.data, "Texture", "VivHelper/player_outline")
    t = get(entity.data, "ShowTexture", false) # this should never default to false
    if t || isempty(s)
        s = "VivHelper/player_outline";
    end
    Ahorn.drawSprite(ctx, s, 0, 1; jx=0.5, jy=1.0, tint=VivHelper.ColorFix(get(entity.data, "Color", (1.0,1.0,1.0,1.0))))
end

function getFirstMatchingEntityAndRoom(nameMatch, tagMatch)
    for room in Ahorn.loadedState.map.rooms
        for entity in room.entities
            t = get(entity.data, "tag", 0) 
            if t != 0 && entity.name in nameMatch && t == tagMatch
                return (entity, room)
            end
        end
    end
    return nothing
end
function getEntityRoom(entity)
    for room in Ahorn.loadedState.map.rooms
        for entity2 in room.entities
            if entity.name == entity2.name && entity.data == entity2.data && entity.id == entity2.id
                return room
            end
        end
    end
    return nothing
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpawnpointBetweenRooms) 
    s = get(entity.data, "Texture", "VivHelper/player_outline")
    t = get(entity.data, "ShowTexture", false) # this should never default to false
    if t || isempty(s)
        s = "VivHelper/player_outline";
    end
    u = get(entity.data, "tag", 0)
    if u == 0
        z = trunc(Int, time()) % 2 == 0
        Ahorn.drawSprite(ctx, z ? ErrorSprites[2] : ErrorSprites[1], 0, 0, jx=0.5, jy=0.5)
        Ahorn.drawCenteredText(ctx, "Tag is 0!", 0, 32, 24, 24)
    else
        v = get(entity.data, "flipX", false) ? -1 : 1
        Ahorn.drawSprite(ctx, s, 0, 1, jx=0.5, jy=1.0, tint=Ahorn.XNAColors.colors[colorSet[mod1(237*(u+431), 140)]]; sx=v)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpawnpointTarget1) 
    s = get(entity.data, "Texture", "VivHelper/player_outline")
    t = get(entity.data, "ShowTexture", false) # this should never default to false
    if t || isempty(s)
        s = "VivHelper/player_outline";
    end
    s2 = Ahorn.getTextureSprite(s)
    u = get(entity.data, "tag", 0)
    if u == 0
        z = trunc(Int, time()) % 2 == 0
        Ahorn.drawSprite(ctx, z ? ErrorSprites[2] : ErrorSprites[1], 0, 0, jx=0.5, jy=0.5)
        Ahorn.drawCenteredText(ctx, "Tag is 0!", 0, 32, 24, 24)
    else
        v = get(entity.data, "flipX", false) ? -1 : 1
        Ahorn.drawSprite(ctx, s, 0, 1; jx=0.5, jy=1.0, tint=Ahorn.XNAColors.colors[colorSet[mod1(237*(u+431), 140)]], sx=v)
        Ahorn.drawSprite(ctx, "ahorn/VivHelper/portaltarget", 0, 0 - s2.realHeight, jx=0.5, jy=0.0)

    end
end
function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpawnpointTarget2) 
    s2 = Ahorn.getTextureSprite("VivHelper/player_outline")
    u = get(entity.data, "tag", 0)
    if u === 0
        z = trunc(Int, time()) % 2 == 0
        Ahorn.drawSprite(ctx, z ? ErrorSprites[2] : ErrorSprites[1], 0, 0, jx=0.5, jy=0.5)
        Ahorn.drawCenteredText(ctx, "Tag is 0!", 0, 32, 24, 24)
    else
        v = get(entity.data, "flipX", false) ? -1 : 1
        Ahorn.drawSprite(ctx, "VivHelper/player_outline", 0, 1; jx=0.5, jy=1.0, tint=Ahorn.XNAColors.colors[colorSet[mod1(237*(u+431), 140)]], sx=v)
        Ahorn.drawSprite(ctx, "ahorn/VivHelper/portaltarget", 0,  - s2.realHeight, jx=0.5, jy=0.0)
    end
end


end