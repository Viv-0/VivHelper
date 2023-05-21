module VivHelperWindRainFG

using ..Ahorn, Maple

@mapdef Effect "VivHelper/WindRainFG" WindRainFG(only::String="*", exclude::String="", Scrollx::Number=0.0, Scrolly::Number=0.0, colors::String="161933", windStrength::Number=1.0)

placements = WindRainFG

function Ahorn.canFgBg(WindRainFG)
	return true, true
end

end
