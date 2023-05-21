module VivHelperSoundReplacer
using ..Ahorn, Maple

@mapdef Entity "VivHelper/SoundReplacer" SoundMuter(x::Integer, y::Integer,
eventName::String="", replacementEvent::String="",
flag::String="", customParams::String="")

const placements = Ahorn.PlacementDict(
    "Sound Replacer (Viv's Helper)" => Ahorn.EntityPlacement(
        SoundMuter,
        "point"
    )
)

Ahorn.selection(entity::SoundMuter) = Ahorn.Rectangle(get(entity.data, "x", 12)-12, get(entity.data, "y", 12)-12, 24, 24)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SoundMuter, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/VivHelper/sound.png", 0, 0)
    Ahorn.drawString(ctx, Ahorn.pico8Font, entity.data["eventName"], -11,-12)
    Ahorn.drawString(ctx, Ahorn.pico8Font, entity.data["replacementEvent"], -11,6)
end

end