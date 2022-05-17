module VivHelperWindRainFG

using ..Ahorn, Maple

@mapdef Effect "VivHelper/WindRainFG" WindRainFG(only::String="*", exclude::String="", scrollx::Number=1.0, scrolly::Number=1.0, colors::String="161933", windStrength::Number=1.0)

placements = WindRainFG

function Ahorn.canFgBg(WindRainFG)
	return true, true
end

end
