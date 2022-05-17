module VivHelperSpawnpoints
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/Spawnpoint" Spawnpoint(x::Integer, y::Integer, ShowTexture::Bool=true, Texture::String="VivHelper/player_outline", Depth::Int=5000, flipX::Bool=false)

@mapdef Entity "VivHelper/InterRoomSpawner" SpawnpointBetweenRooms(x::Integer, y::Integer, tag::Integer=1, ShowTexture::Bool=true, Texture::String="VivHelper/player_outline", Depth::Int=5000, flipX::Bool=false)

@mapdef Entity "VivHelper/InterRoomSpawnTarget" SpawnpointTarget1(x::Integer, y::Integer, tag::Integer=1, ShowTexture::Bool=true, Texture::String="VivHelper/player_outline", Depth::Int=5000, flipX::Bool=false)
@mapdef Entity "VivHelper/InterRoomSpawnTarget2" SpawnpointTarget2(x::Integer, y::Integer, tag::Integer=1, ShowTexture::Bool=true, Texture::String="VivHelper/player_outline", Depth::Int=5000, flipX::Bool=false)

#This is a semi-randomized set of colors. please close the array info and don't worry about it, it means basically nothing,
#it's just giving a clean color set for Spawnpoints between Rooms and the corresponding targets by their integer tags (any value greater than 140 is set to 140 through min(a,b))
const colorSet = [(0.0, 0.0, 0.0, 1.0),                                                   #Black
    (1.0, 0.0, 0.0, 1.0),                                                   #Red
    (0.0, 1.0, 1.0, 1.0),                                                   #Cyan
    (0.0, 0.5019607843137255, 0.0, 1.0),                                    #Green
    (0.9803921568627451, 0.9215686274509803, 0.8431372549019608, 1.0),      #AntiqueWhite
    (0.0, 0.0, 1.0, 1.0),                                                   #Blue
    (0.1843137254901961, 0.30980392156862746, 0.30980392156862746, 1.0),    #DarkSlateGray
    (1.0, 0.7529411764705882, 0.796078431372549, 1.0),                      #Pink
    (1.0, 0.6470588235294118, 0.0, 1.0),                                    #Orange
    (0.8470588235294118, 0.7490196078431373, 0.8470588235294118, 1.0),      #Thistle
    (0.0, 1.0, 0.0, 1.0),                                                   #Lime
    (0.23529411764705882, 0.7019607843137254, 0.44313725490196076, 1.0),    #MediumSeaGreen
    (0.4823529411764706, 0.40784313725490196, 0.9333333333333333, 1.0),     #MediumSlateBlue
    (0.803921568627451, 0.5215686274509804, 0.24705882352941178, 1.0),      #Peru
    (0.27450980392156865, 0.5098039215686274, 0.7058823529411765, 1.0),     #SteelBlue
    (0.9568627450980393, 0.6431372549019608, 0.3764705882352941, 1.0),      #SandyBrown
    (0.9803921568627451, 0.9803921568627451, 0.8235294117647058, 1.0),      #LightGoldenrodYellow
    (0.8627450980392157, 0.0784313725490196, 0.23529411764705882, 1.0),     #Crimson
    (0.8705882352941177, 0.7215686274509804, 0.5294117647058824, 1.0),      #BurlyWood
    (0.2823529411764706, 0.23921568627450981, 0.5450980392156862, 1.0),     #DarkSlateBlue
    (0.6, 0.19607843137254902, 0.8, 1.0),                                   #DarkOrchid
    (0.9411764705882353, 0.5019607843137255, 0.5019607843137255, 1.0),      #LightCoral
    (0.6470588235294118, 0.16470588235294117, 0.16470588235294117, 1.0),    #Brown
    (1.0, 0.9803921568627451, 0.803921568627451, 1.0),                      #LemonChiffon
    (1.0, 0.0784313725490196, 0.5764705882352941, 1.0),                     #DeepPink
    (0.9607843137254902, 0.9607843137254902, 0.9607843137254902, 1.0),      #WhiteSmoke
    (0.6627450980392157, 0.6627450980392157, 0.6627450980392157, 1.0),      #DarkGray
    (0.5019607843137255, 0.5019607843137255, 0.0, 1.0),                     #Olive
    (0.37254901960784315, 0.6196078431372549, 0.6274509803921569, 1.0),     #CadetBlue
    (0.12549019607843137, 0.6980392156862745, 0.6666666666666666, 1.0),     #LightSeaGreen
    (0.9607843137254902, 1.0, 0.9803921568627451, 1.0),                     #MintCream
    (0.9411764705882353, 0.9019607843137255, 0.5490196078431373, 1.0),      #Khaki
    (0.0, 0.0, 0.5019607843137255, 1.0),                                    #Navy
    (0.13333333333333333, 0.5450980392156862, 0.13333333333333333, 1.0),    #ForestGreen
    (0.0, 1.0, 0.4980392156862745, 1.0),                                    #SpringGreen
    (0.9137254901960784, 0.5882352941176471, 0.47843137254901963, 1.0),     #DarkSalmon
    (0.7215686274509804, 0.5254901960784314, 0.043137254901960784, 1.0),    #DarkGoldenrod
    (1.0, 0.8705882352941177, 0.6784313725490196, 1.0),                     #NavajoWhite
    (0.7529411764705882, 0.7529411764705882, 0.7529411764705882, 1.0),      #Silver
    (0.8784313725490196, 1.0, 1.0, 1.0),                                    #LightCyan
    (0.4392156862745098, 0.5019607843137255, 0.5647058823529412, 1.0),      #SlateGray
    (0.9411764705882353, 0.9725490196078431, 1.0, 1.0),                     #AliceBlue
    (1.0, 0.4980392156862745, 0.3137254901960784, 1.0),                     #Coral
    (0.5803921568627451, 0.0, 0.8274509803921568, 1.0),                     #DarkViolet
    (0.8627450980392157, 0.8627450980392157, 0.8627450980392157, 1.0),      #Gainsboro
    (0.6784313725490196, 1.0, 0.1843137254901961, 1.0),                     #GreenYellow
    (1.0, 0.27058823529411763, 0.0, 1.0),                                   #OrangeRed
    (0.4980392156862745, 1.0, 0.0, 1.0),                                    #Chartreuse
    (0.41568627450980394, 0.35294117647058826, 0.803921568627451, 1.0),     #SlateBlue
    (0.25098039215686274, 0.8784313725490196, 0.8156862745098039, 1.0),     #Turquoise
    (1.0, 0.8941176470588236, 0.8823529411764706, 1.0),                     #MistyRose
    (0.0, 0.9803921568627451, 0.6039215686274509, 1.0),                     #MediumSpringGreen
    (1.0, 0.0, 1.0, 1.0),                                                   #Fuchsia
    (0.596078431372549, 0.984313725490196, 0.596078431372549, 1.0),         #PaleGreen
    (0.3333333333333333, 0.4196078431372549, 0.1843137254901961, 1.0),      #DarkOliveGreen
    (0.0, 0.7490196078431373, 1.0, 1.0),                                    #DeepSkyBlue
    (0.0, 0.5019607843137255, 0.5019607843137255, 1.0),                     #Teal
    (1.0, 0.7137254901960784, 0.7568627450980392, 1.0),                     #LightPink
    (0.9019607843137255, 0.9019607843137255, 0.9803921568627451, 1.0),      #Lavender
    (0.2823529411764706, 0.8196078431372549, 0.8, 1.0),                     #MediumTurquoise
    (1.0, 0.9607843137254902, 0.9333333333333333, 1.0),                     #SeaShell
    (0.6039215686274509, 0.803921568627451, 0.19607843137254902, 1.0),      #YellowGreen
    (1.0, 0.9215686274509803, 0.803921568627451, 1.0),                      #BlanchedAlmond
    (0.9411764705882353, 1.0, 1.0, 1.0),                                    #Azure
    (0.5294117647058824, 0.807843137254902, 0.9803921568627451, 1.0),       #LightSkyBlue
    (1.0, 0.9803921568627451, 0.9803921568627451, 1.0),                     #Snow
    (0.19607843137254902, 0.803921568627451, 0.19607843137254902, 1.0),     #LimeGreen
    (0.5019607843137255, 0.0, 0.0, 1.0),                                    #Maroon
    (1.0, 0.8941176470588236, 0.7098039215686275, 1.0),                     #Moccasin
    (0.4666666666666667, 0.5333333333333333, 0.6, 1.0),                     #LightSlateGray
    (0.4117647058823529, 0.4117647058823529, 0.4117647058823529, 1.0),      #DimGray
    (0.6274509803921569, 0.3215686274509804, 0.17647058823529413, 1.0),     #Sienna
    (0.6901960784313725, 0.7686274509803922, 0.8705882352941177, 1.0),      #LightSteelBlue
    (0.5764705882352941, 0.4392156862745098, 0.8588235294117647, 1.0),      #MediumPurple
    (1.0, 0.0, 1.0, 1.0),                                                   #Magenta
    (0.9803921568627451, 0.5019607843137255, 0.4470588235294118, 1.0),      #Salmon
    (0.9333333333333333, 0.9098039215686274, 0.6666666666666666, 1.0),      #PaleGoldenrod
    (0.2549019607843137, 0.4117647058823529, 0.8823529411764706, 1.0),      #RoyalBlue
    (0.9725490196078431, 0.9725490196078431, 1.0, 1.0),                     #GhostWhite
    (1.0, 0.8549019607843137, 0.7254901960784313, 1.0),                     #PeachPuff
    (1.0, 0.6274509803921569, 0.47843137254901963, 1.0),                    #LightSalmon
    (0.39215686274509803, 0.5843137254901961, 0.9294117647058824, 1.0),     #CornflowerBlue
    (0.0, 0.0, 0.803921568627451, 1.0),                                     #MediumBlue
    (1.0, 0.9372549019607843, 0.8352941176470589, 1.0),                     #PapayaWhip
    (0.8235294117647058, 0.7058823529411765, 0.5490196078431373, 1.0),      #Tan
    (0.5411764705882353, 0.16862745098039217, 0.8862745098039215, 1.0),     #BlueViolet
    (1.0, 0.8941176470588236, 0.7686274509803922, 1.0),                     #Bisque
    (0.0, 1.0, 1.0, 1.0),                                                   #Aqua
    (0.5450980392156862, 0.0, 0.0, 1.0),                                    #DarkRed
    (0.7372549019607844, 0.5607843137254902, 0.5607843137254902, 1.0),      #RosyBrown
    (0.6980392156862745, 0.13333333333333333, 0.13333333333333333, 1.0),    #Firebrick
    (0.9607843137254902, 0.8705882352941177, 0.7019607843137254, 1.0),      #Wheat
    (1.0, 1.0, 0.9411764705882353, 1.0),                                    #Ivory
    (0.0, 0.39215686274509803, 0.0, 1.0),                                   #DarkGreen
    (0.8666666666666667, 0.6274509803921569, 0.8666666666666667, 1.0),      #Plum
    (0.8549019607843137, 0.6470588235294118, 0.12549019607843137, 1.0),     #Goldenrod
    (0.0, 0.5450980392156862, 0.5450980392156862, 1.0),                     #DarkCyan
    (0.29411764705882354, 0.0, 0.5098039215686274, 1.0),                    #Indigo
    (0.4, 0.803921568627451, 0.6666666666666666, 1.0),                      #MediumAquamarine
    (1.0, 0.9411764705882353, 0.9607843137254902, 1.0),                     #LavenderBlush
    (0.8549019607843137, 0.4392156862745098, 0.8392156862745098, 1.0),      #Orchid
    (0.5607843137254902, 0.7372549019607844, 0.5450980392156862, 1.0),      #DarkSeaGreen
    (0.9607843137254902, 0.9607843137254902, 0.8627450980392157, 1.0),      #Beige
    (0.09803921568627451, 0.09803921568627451, 0.4392156862745098, 1.0),    #MidnightBlue
    (0.8588235294117647, 0.4392156862745098, 0.5764705882352941, 1.0),      #PaleVioletRed
    (0.4196078431372549, 0.5568627450980392, 0.13725490196078433, 1.0),     #OliveDrab
    (1.0, 1.0, 0.0, 1.0),                                                   #Yellow
    (1.0, 0.5490196078431373, 0.0, 1.0),                                    #DarkOrange
    (0.0, 0.0, 0.5450980392156862, 1.0),                                    #DarkBlue
    (0.0, 0.807843137254902, 0.8196078431372549, 1.0),                      #DarkTurquoise
    (0.1803921568627451, 0.5450980392156862, 0.3411764705882353, 1.0),      #SeaGreen
    (1.0, 0.8431372549019608, 0.0, 1.0),                                    #Gold
    (0.6862745098039216, 0.9333333333333333, 0.9333333333333333, 1.0),      #PaleTurquoise
    (0.9333333333333333, 0.5098039215686274, 0.9333333333333333, 1.0),      #Violet
    (1.0, 0.38823529411764707, 0.2784313725490196, 1.0),                    #Tomato
    (1.0, 0.4117647058823529, 0.7058823529411765, 1.0),                     #HotPink
    (0.9921568627450981, 0.9607843137254902, 0.9019607843137255, 1.0),      #OldLace
    (0.5294117647058824, 0.807843137254902, 0.9215686274509803, 1.0),       #SkyBlue
    (0.8274509803921568, 0.8274509803921568, 0.8274509803921568, 1.0),      #LightGray
    (0.6901960784313725, 0.8784313725490196, 0.9019607843137255, 1.0),      #PowderBlue
    (1.0, 0.9803921568627451, 0.9411764705882353, 1.0),                     #FloralWhite
    (0.4980392156862745, 1.0, 0.8313725490196079, 1.0),                     #Aquamarine
    (0.11764705882352941, 0.5647058823529412, 1.0, 1.0),                    #DodgerBlue
    (0.9411764705882353, 1.0, 0.9411764705882353, 1.0),                     #Honeydew
    (0.8235294117647058, 0.4117647058823529, 0.11764705882352941, 1.0),     #Chocolate
    (1.0, 1.0, 0.8784313725490196, 1.0),                                    #LightYellow
    (0.9803921568627451, 0.9411764705882353, 0.9019607843137255, 1.0),      #Linen
    (0.5019607843137255, 0.0, 0.5019607843137255, 1.0),                     #Purple
    (0.5019607843137255, 0.5019607843137255, 0.5019607843137255, 1.0),      #Gray
    (0.7803921568627451, 0.08235294117647059, 0.5215686274509804, 1.0),     #MediumVioletRed
    (0.5450980392156862, 0.27058823529411763, 0.07450980392156863, 1.0),    #SaddleBrown
    (0.6784313725490196, 0.8470588235294118, 0.9019607843137255, 1.0),      #LightBlue
    (0.803921568627451, 0.3607843137254902, 0.3607843137254902, 1.0),       #IndianRed
    (0.5450980392156862, 0.0, 0.5450980392156862, 1.0),                     #DarkMagenta
    (0.48627450980392156, 0.9882352941176471, 0.0, 1.0),                    #LawnGreen
    (0.7411764705882353, 0.7176470588235294, 0.4196078431372549, 1.0),      #DarkKhaki
    (0.5647058823529412, 0.9333333333333333, 0.5647058823529412, 1.0),      #LightGreen
    (0.7294117647058823, 0.3333333333333333, 0.8274509803921568, 1.0),      #MediumOrchid
    (1.0, 0.9725490196078431, 0.8627450980392157, 1.0),                     #Cornsilk
    (1.0, 1.0, 1.0, 1.0),                                                   #White
] 

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
        Ahorn.drawSprite(ctx, s, 0, 1, jx=0.5, jy=1.0, tint=colorSet[modulo(u, 140) + 1]; sx=v)
        Ahorn.drawCenteredText(ctx, string(u), 0, 1, 16, 16)
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
        Ahorn.drawSprite(ctx, s, 0, 1; jx=0.5, jy=1.0, tint=colorSet[modulo(u, 140) + 1], sx=v)
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
        Ahorn.drawSprite(ctx, "VivHelper/player_outline", 0, 1; jx=0.5, jy=1.0, tint=colorSet[modulo(u, 140) + 1], sx=v)
        Ahorn.drawSprite(ctx, "ahorn/VivHelper/portaltarget", 0,  - s2.realHeight, jx=0.5, jy=0.0)
        Ahorn.drawCenteredText(ctx, string(u), 0, 32, 24, 24)
    end
end


end