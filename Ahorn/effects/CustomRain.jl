module VivHelperCustomRain

using ..Ahorn, Maple

@mapdef Effect "VivHelper/CustomRain" CustomRain(only::String="*", exclude::String="", Scrollx::Number=0.0, Scrolly::Number=0.0, angle::Number=270.0, angleDiff::Number=2.86, speedMult::Number=1.0, Amount::Integer=240, colors::String="161933", alpha::Number=1.0)

placements = CustomRain

function Ahorn.canFgBg(CustomRain)
	return true, true
end

end