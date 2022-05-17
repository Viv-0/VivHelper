module VivHelperFlameTravelTrigger
using ..Ahorn, Maple

@mapdef Trigger "VivHelper/FlameTravelTrigger" FlameTravelTrigger(
	x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
	removeOnExit::Bool=false, TravelingFlameID::String="room-flame-1",
	Nodes::String="0"
)

@mapdef Trigger "VivHelper/FlameLightSwitch" FlameToggleTrigger(
	x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
	removeOnExit::Bool=false, TravelingFlameID::String="room-flame-1",
	Nodes::String="0", TurnOn::Bool=true
)

const placements = Ahorn.PlacementDict(
    "Traveling Flame Operator (Viv's Helper)" => Ahorn.EntityPlacement(
        FlameTravelTrigger,
        "rectangle"
    ),
	"Traveling Flame Light Toggle Trigger (Viv's Helper)" => Ahorn.EntityPlacement(
        FlameToggleTrigger,
        "rectangle"
    )
)


end
