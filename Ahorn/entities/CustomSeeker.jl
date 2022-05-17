module VivHelperCustomSeeker

using ..Ahorn, Maple
using Ahorn.VivHelper

@mapdef Entity "VivHelper/CustomSeeker" CustomSeekerX(x::Integer, y::Integer,
    Accel::Number=600.0, WallCollideStunThreshold::Number=100.0, StunXSpeed::Number=100.0,
    BounceSpeed::Number=200.0, SightDistance::Number=160.0, ExplodeRadius::Number=40.0,
    StrongSkiddingTime::Number=0.08, FarDist::Number=112.0, IdleAccel::Number=200.0,
    IdleSpeed::Number=50.0, SkiddingAccel::Number=200.0, StrongSkiddingAccel::Number=400.0,
    PatrolSpeed::Number=25.0, PatrolWaitTime::Number=0.4, SpottedTargetSpeed::Number=60.0,
    FarDistSpeedMult::Number=2.0, DirectionDotThreshold::Number=0.4, AttackMinXDist::Number=16.0,
    SpottedMaxYDist::Number=24.0, SpottedLosePlayerTime::Number=0.6, SpottedMinAttackTime::Number=0.2,
    AttackWindUpSpeed::Number=60.0, AttackWindUpTime::Number=0.3, AttackStartSpeed::Number=180.0, XDistanceFromWindUp::Number=36.0,
    AttackTargetSpeed::Number=260.0, AttackAccel::Number=300.0, StunnedAccel::Number=150.0,
    StunTime::Number=0.8, DisableAllParticles::Bool=false, ParticleEmitInterval::Number=0.04,
    TrailCreateInterval::Number=0.06, RegenerationTimerLength::Number=1.85, AttackMaxRotateDegrees::Number=35,
    FinalDash::Bool=true, TrailColor::String="99e550",
    MaxNumberOfDashes::Integer=-1, MaxNumberOfWallCollides::Integer=-1, MaxNumberOfBounces::Integer=-1,
    DeathEffectColor::String="ffffff", RemoveBounceHitbox::Bool=false,
    boopedSFXPath::String="", aggroSFXPath::String="", reviveSFXPath::String="",
    CustomSpritePath::String="", CustomShockwavePath::String="", SeekerColorTint::String="ffffff"
)

@mapdef Entity "VivHelper/CustomSeekerS" CustomSeekerS(x::Integer, y::Integer,
    Accel::Number=600.0, FlagOnDeath::String="",
 	SightDistance::Number=160.0, ExplodeRadius::Number=40.0,
    IdleAccel::Number=200.0,
    IdleSpeed::Number=50.0, SkiddingAccel::Number=200.0,
    PatrolSpeed::Number=25.0, SpottedTargetSpeed::Number=60.0,
	AttackStartSpeed::Number=180.0,
    AttackTargetSpeed::Number=260.0, AttackAccel::Number=300.0,
 	DisableAllParticles::Bool=false, ParticleEmitInterval::Number=0.04,
    TrailCreateInterval::Number=0.06, RegenerationTimerLength::Number=1.85,
    DeathEffectColor::String="ffffff", RemoveBounceHitbox::Bool=false,
    boopedSFXPath::String="", aggroSFXPath::String="", reviveSFXPath::String="",
    CustomSpritePath::String="", CustomShockwavePath::String="", SeekerColorTint::String="ffffff", TrailColor::String="99e550"
)

@mapdef Entity "VivHelper/CustomSeekerYaml" CustomSeekerYaml(x::Integer, y::Integer,
YamlPath::String="")

const sprite = "characters/monsters/predator43.png"
CustomSeeker = Union{CustomSeekerS, CustomSeekerX, CustomSeekerYaml}


const placements = Ahorn.PlacementDict(
	"Custom Seeker (Viv's Helper)" => Ahorn.EntityPlacement(
		CustomSeekerS,
		"rectangle",
		Dict{String, Any}(),
	),
	"Custom Seeker (Fully Custom) (Viv's Helper)" => Ahorn.EntityPlacement(
		CustomSeekerX,
		"rectangle",
		Dict{String, Any}()
	),
	"Custom Seeker (From YAML file) (Viv's Helper)" => Ahorn.EntityPlacement(
		CustomSeekerYaml,
		"rectangle",
		Dict{String, Any}()
	)
)
Ahorn.editingOptions(entity::CustomSeeker) = Dict{String, Any}(
    "SeekerColorTint" => VivHelper.XNAColors,
	"DeathEffectColor" => VivHelper.XNAColors,
	"TrailColor" => VivHelper.XNAColors
)

Ahorn.nodeLimits(entity::CustomSeeker) = 0, -1

function Ahorn.selection(entity::CustomSeeker)
	nodes = get(entity.data, "nodes", ())
	x, y = Ahorn.position(entity)
	res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]

    for node in nodes
        nx, ny = node

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CustomSeeker)
    px, py = Ahorn.position(entity)
	col = VivHelper.ColorFix(get(entity.data, "SeekerColorTint", "ffffff"), 1.0)
    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny, tint=col)

        px, py = nx, ny
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomSeeker, room::Maple.Room)
	col = VivHelper.ColorFix(get(entity.data, "SeekerColorTint", "ffffff"), 1.0)
	Ahorn.drawSprite(ctx, sprite, 0, 0, tint=col)
end

end
