module VivHelper
using ..Ahorn, Maple

function findTextureAnimation(texture::String, number::Integer, sprites::Dict{String, Ahorn.SpriteHolder}=Ahorn.getAtlas(), maxPadding=7)::String
    n = number < 0 ? 0 : number
    textures = Ahorn.findTextureAnimations(texture, sprites, maxPadding);
    return textures[n+1]
end

const XNAColors = sort(collect(keys(Ahorn.XNAColors.colors)))

function ColorFix(v::String, alpha::Float64=1.0)
    if v in XNAColors
        w = get(Ahorn.XNAColors.colors, v, (1.0, 1.0, 1.0, 1.0))
        return (w[1], w[2], w[3], alpha)
    else
        temp = Ahorn.argb32ToRGBATuple(parse(Int, v, base=16))[1:3] ./ 255
        color = (temp[1], temp[2], temp[3], alpha)
        return color
    end
    return (1.0, 1.0, 1.0, 1.0)
end

const Depths = Dict{String, Integer}(
    "BGTerrain (10000)" => 10000,
    "BGDecals (9000)" => 9000,
    "BGParticles (8000)" => 8000,
    "Below (2000)" => 2000,
    "NPCs (1000)" => 1000,
    "Player (0)" => 0,
    "Dust (-50)" => -50,
    "Pickups (-100)" => -100,
    "Particles (-8000)" => -8000,
    "Above Particles (-8500)" => -8500,
    "Solids (-9000)" => -9000,
    "FGTerrain (-10000)" => -10000,
    "FGDecals (-10500)" => -10500,
    "DreamBlocks (-11000)" => -11000,
    "CrystalSpinners (-11500)" => -11500,
    "Chaser (-12500)" => -12500,
    "Fake Walls (-13000)" => -13000,
    "FGParticles (-50000)" => -50000,
    "Above Blackout (-250002)" => -250002
)

const Particles = Dict{String, String}(
    "Blob" => "particles/blob",
    "Bubble" => "particles/bubble",
    "Circle" => "particles/circle",
    "Cloud" => "particles/cloud",
    "Confetti" => "particles/confetti",
    "Feather" => "particles/feather",
    "Fire" => "particles/fire",
    "None" => "",
    "Petal" => "particles/petal",
    "Rectangle" => "particles/rect",
    "Shard" => "particles/shard",
    "Shatter" => "particles/shatter",
    "Smoke" => "particles/smoke0,particles/smoke1,particles/smoke2,particles/smoke3",
    "Snow" => "particles/snow",
    "Triangle" => "particles/triangle",
    "Trigger Spikes" => "particles/triggerspike",
    "Zappy Smoke" => "particles/zappysmoke00,particles/zappysmoke01,particles/zappysmoke02,particles/zappysmoke03"
)

function TranslateParticles(particle::String)
    particles = split(particle, ",");
    return split(particles[1], ":")[1]
end

function getRoomNames()
    ret = Dict{String, String}()
    for room in Ahorn.loadedState.map.rooms
        s = room.name
        if startswith(s, "lvl_")
            s = SubString(s, 5) # gets the values after lvl_, julia is 1-indexed because hec u
        end
        ret[room.name] = s
    end
    return sort(collect(keys(ret)), by=x->x[1])
end

function getEntityValues(entityName, propName::String, ignoredValues)
    arr = []
    for room in Ahorn.loadedState.map.rooms
        for entity in room.entities
            if entity.name in entityName
                v = get(entity.data, propName, nothing)
                if v !== nothing && !(v in ignoredValues)
                    push!(arr, v)
                end
            end
        end
    end
    return arr
end

function getEntityValues(entityName, roomName::String, propName::String)
    arr = []
    _room = nothing;
    for room in Ahorn.loadedState.map.rooms
        if(room.name == roomName)
            _room = room
            break
        end
    end
    if _room === nothing 
        return []
    end
    for entity in _room.entities
        if entity.name in entityName
            v = get(entity.data, propName, nothing)
            if v !== nothing
                push!(arr, v)
            end
        end
    end
    return arr
end

function getTriggerValues(entityName, propName::String, ignoredValues)
    arr = []
    for room in Ahorn.loadedState.map.rooms
        for entity in room.triggers
            if entity.name in entityName
                v = get(entity.data, propName, nothing)
                if v !== nothing && !(v in ignoredValues)
                    push!(arr, v)
                end
            end
        end
    end
    return arr
end

function getTriggerValues(entityName, roomName::String, propName::String)
    arr = []
    _room = nothing;
    for room in Ahorn.loadedState.map.rooms
        if(room.name == roomName)
            _room = room
            break
        end
    end
    if _room === nothing 
        return []
    end
    for entity in _room.triggers
        if entity.name in entityName
            v = get(entity.data, propName, nothing)
            if v !== nothing
                push!(arr, v)
            end
        end
    end
    return arr
end

function getFirstMatchingEntity(predicate)
    for room in Ahorn.loadedState.map.rooms
        for entity in room.entities
            if predicate(entity)
                return entity
            end
        end
    end
end


const EaseTypes = String["Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut"]

function HSV2RGBATuple(hue::Number, saturation::Number, value::Number, alpha::Number=1.0)
    H = mod(hue, 1) * 360
    C = value * saturation
    X = C * (1 - abs(mod(H / 60, 2) -1 ))
    m = value - C
    c1 = [0.0,0.0,0.0]
    if H >= 0 && H < 60
        c1 = [C, X, 0.0]
    elseif H >= 60 && H < 120
        c1 = [X, C, 0.0]
    elseif H >= 120 && H < 180
        c1 = [0.0, C, X]
    elseif H >= 180 && H < 240
        c1 = [0.0, X, C]
    elseif H >= 240 && H < 300
        c1 = [X, 0.0, C]
    else
        c1 = [C, 0.0, X]
    end
    return (c1[1] + m, c1[2] + m, c1[3] + m, alpha)
end


function PolygonFinalizer(entity::Entity)
    entity.data["nodes"] = [
                (Int(entity.data["x"]) + 32, Int(entity.data["y"])),
                (Int(entity.data["x"]) + 8, Int(entity.data["y"]) - 16)
            ]
end

const polygonPointColor = (0.75, 0.75, 0.0, 0.5)

function getPolygonCentroid(nodes)::Tuple{Integer,Integer}
    list = deepcopy(nodes)
    x1,y1 = Integer.(nodes[1])
    xL,yL = last(nodes)
    if x1 != xL || y1 != yL
        list.append(nodes[1])
    end
    twicearea = 0
    x = 0
    y = 0
    nPts = size(list, 1)
    i = 1
    j = nPts
    while i < nPts
        p1x,p1y = list[i]
        p2x,p2y = list[j]
        f = (p1y - y1) * (p2x - x1) - (p2y - y1) * (p1x - x1)
        twicearea += f
        x += (p1x + p2x - 2 * x1) * f
        y += (p1y + p2y - 2 * y1) * f

        j = i
        i += 1
    end
    f = twicearea * 3

    return (Int(round(x / f + x1)), Int(round(y / f + y1)))
end

end
