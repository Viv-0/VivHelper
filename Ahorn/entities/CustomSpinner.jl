 module VivHelperCustomCrystalSpinner
using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CustomSpinner" CustomCrystalSpinner(x::Integer, y::Integer,
AttachToSolid::Bool=false, Directory::String="VivHelper/customSpinner/white",
Subdirectory::String="white", Type::String="White", shatterOnDash::Bool=false,
Color::String="White", ShatterColor::String="White", BorderColor::String="Black", Scale::Number=1.0,
ImageScale::Number=1.0, DebrisToScale::Bool=true, CustomDebris::Bool=false,
Depth::Integer=-8500, ShatterFlag::String="", HitboxType::String="C:6;0,0|R:16,4;-8,*1@-4", fromFrostHelper::Bool=false, isSeeded::Bool=false, seed::Integer=-1, ignoreConnection::Bool=false, 
flagToggle::String=""
)

@mapdef Entity "VivHelper/AnimatedSpinner" AnimatedSpinner(x::Integer, y::Integer,
AttachToSolid::Bool=false, Directory::String="VivHelper/customSpinner/animated/",
Subdirectories::String="spin", Type::String="White", shatterOnDash::Bool=false,
Color::String="White", ShatterColor::String="White", BorderColor::String="Black", Scale::Number=1.0,
ImageScale::Number=1.0, DebrisToScale::Bool=true, CustomDebris::Bool=false,
Depth::Integer=-8500, ShatterFlag::String="", HitboxType::String="C:6;0,0|R:16,4;-8,*1@-4", TimeBetweenFrames::Number=0.1, isSeeded::Bool=false, seed::Integer=-1, ignoreConnection::Bool=false, 
flagToggle::String=""
)

const placements = Ahorn.PlacementDict(
    "Custom Spinner (Viv's Helper)" => Ahorn.EntityPlacement(
        CustomCrystalSpinner,
        "point",
        Dict{String, Any}("CurrentVersion" => true)
    ),
    "Animated Spinner (Viv's Helper)" => Ahorn.EntityPlacement(
        AnimatedSpinner,
        "point",
        Dict{String, Any}("CurrentVersion" => true)
    )
)

Ahorn.editingOptions(entity::CustomCrystalSpinner) = Dict{String, Any}(
    "Type" => String["White", "RainbowClassic"],
    "Depth" => VivHelper.Depths,
    "Directory" => String["VivHelper/customSpinner/white", "VivHelper/customSpinner/hi-res", "danger/crystal"],
    "Color" => VivHelper.XNAColors,
    "ShatterColor" => VivHelper.XNAColors,
    "BorderColor" => VivHelper.XNAColors,
    "HitboxType" => Dict{String, String}(
        "Scaled Spinner (Default)" => "C:6;0,0|R:16,4;-8,*1@-4",
        "Thin Rectangle" => "C:6;0,0|R:16,*4;-8,*-3",
        "Normal Circle (No Rect)" => "C:6;0,0",
        "Larger Circle (No Rect)" => "C:8;0,0",
        "Upside Down Spinner" => "C:6;0,0|R:16,4;-8,*-1",
        "Upside Down Spinner, Thin Rect" => "C:6;0,0|R:16,*4;-8,*-1"
    )
)

Ahorn.editingOptions(entity::AnimatedSpinner) = Dict{String, Any}(
    "Type" => String["White", "RainbowClassic"],
    "Depth" => VivHelper.Depths,
    "Subdirectories" => String["spin"],
    "Color" => VivHelper.XNAColors,
    "ShatterColor" => VivHelper.XNAColors,
    "BorderColor" => VivHelper.XNAColors,
    "HitboxType" => Dict{String, String}(
        "Scaled Spinner" => "C:6;0,0|R:16,4;-8,*1@-4",
        "Thin Rectangle" => "C:6;0,0|R:16,*4;-8,*-3",
        "Normal Circle (No Rect)" => "C:6;0,0",
        "Larger Circle (No Rect)" => "C:8;0,0"
    )
)

Spinners = Union{CustomCrystalSpinner, AnimatedSpinner}

Ahorn.editingIgnored(entity::Spinners, multiple::Bool=false) = multiple ? String["x", "y", "AttachToSolid", "Type", "HitboxType", "Depth", "Scale", "ImageScale"] : String["CurrentVersion"]

Ahorn.nodeLimits(entity::Spinners) = 0, 0

function Ahorn.selection(entity::Spinners)
    x, y = Ahorn.position(entity)
    scale = get(entity.data, "Scale", 1.0);
    return Ahorn.Rectangle(x - (8 * scale), y - (8 * scale), (16 * scale), (16 * scale))
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomCrystalSpinner, room::Maple.Room)
    t = get(entity.data, "Directory", "VivHelper/customSpinner/white/");
    if endswith(t, "/")
        t = chop(t);
    end
    u = get(entity.data, "Subdirectory", "");
    if isempty(u)
        if "CurrentVersion" in keys(entity.data)
            texture = string(t, "/fg00.png")
        else
            texture = string(t, "/fg_white00.png")
        end
    else        
        texture = string(t, "/fg_", u, "00.png");
    end
    scale = get(entity.data, "Scale", 1.0);
    imageScale = get(entity.data, "ImageScale", 1.0);
    color = VivHelper.ColorFix(get(entity.data, "Color", "White"), 1.0)
    Ahorn.drawSprite(ctx, texture, 0, 0, sx=scale / imageScale, sy=scale / imageScale, tint=color)
    C = get(entity.data, "AttachToSolid", false) ? (0.0,0.56,0.0, 0.5) : (1.0,0.0,0.0,0.5)
    
    S = split(get(entity.data, "HitboxType", get(entity.data, "removeRectHitbox", false) ? "C:6" : "C:6;0,0|R:16,4;-8,*1@-4"),'|');
    for s in S
        r = split(s, (':', ';'));
        if r[1] == "C"
            a = 0
            b = 0
            c = 0
            a = ParseInt(r[2], scale)
            if length(r) > 2
                j = split(r[3], ',')
                b = ParseInt(j[1], scale)
                c = ParseInt(j[2], scale)
            end
            Ahorn.drawCircle(ctx, b, c, a, C)
        elseif r[1] == "R"
            w = 0
            h = 0
            ox = 0
            oy = 0
            k = split(r[2],',')
            w = ParseInt(k[1], scale)
            h = ParseInt(k[2], scale)
            if length(r) > 2
                l = split(r[3], ',')
                ox = ParseInt(l[1], scale)
                oy = ParseInt(l[2], scale)
            end
            Ahorn.drawRectangle(ctx, ox, oy, w, h, (0.0,0.0,0.0,0.0), C)
        end
    end
end

function ParseInt(s, f::Number)
    if occursin("@", s)
        r = split(s, '@')
        return ParseInt(r[1], f) + ParseInt(r[2], f)
    elseif s[1] == '*'
        return parse(Int, SubString(s, 2))
    else
        return floor(Int,round(parse(Int, s) * f))
    end
end

const hitboxCols = [(1.0,0.0,0.0,0.5), (0.0,1.0,1.0,0.5)]

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AnimatedSpinner, room::Maple.Room)
    t = get(entity.data, "Directory", "VivHelper/customSpinner/white/");
    if(endswith(t, "/"))
        t = chop(t);
    end
    u = get(entity.data, "Subdirectories", "spin")
    v = split(u, ","); #Vector of strings
    texture = string(t, "/", v[1], "/idle_fg00")

    scale = get(entity.data, "Scale", 1.0);
    imageScale = get(entity.data, "ImageScale", -1.0);
    imageScale = imageScale == -1.0 ? scale : imageScale;
    color = VivHelper.ColorFix(get(entity.data, "Color", "White"), 1.0)

    Ahorn.drawSprite(ctx, texture, 0, 0, sx=scale, sy=scale, tint=color)

    S = split(get(entity.data, "HitboxType", get(entity.data, "removeRectHitbox", false) ? "C:6" : "C:6;0,0|R:16,4;-8,*1@-4"),'|');
    
    # this is the only way I could get this to work it is wack wtf why this should never actually occur apparently the get is always true if it is not nothing or something wack aa
    C = get(entity.data, "AttachToSolid", false) ? (0.2647, 0.3539, 0.4608, 0.5) : (1.0,0.0,0.0,0.5)
    for s in S
        r = split(s, (':', ';'));
        if r[1] == "C"
            a = 0
            b = 0
            c = 0
            a = ParseInt(r[2], scale)
            if length(r) > 2
                j = split(r[3], ',')
                b = ParseInt(j[1], scale)
                c = ParseInt(j[2], scale)
            end
            Ahorn.drawCircle(ctx, b, c, a, C)
        elseif r[1] == "R"
            w = 0
            h = 0
            ox = 0
            oy = 0
            k = split(r[2],',')
            w = ParseInt(k[1], scale)
            h = ParseInt(k[2], scale)
            if length(r) > 2
                l = split(r[3], ',')
                ox = ParseInt(l[1], scale)
                oy = ParseInt(l[2], scale)
            end
            Ahorn.drawRectangle(ctx, ox, oy, w, h, (0.0,0.0,0.0,0.0), C)
        end
    end
end

end
