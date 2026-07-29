namespace Celeste.Mod.aonHelper.Entities.EchoingTheValleySings;

[GlobalHelper.GlobalEntity("aonHelper/BlossomBlockController", "global")]
[Tracked]
public class BlossomBlockController(
    string spritePath, int surfaceIndex,
    Color particleColor1, Color particleColor2, float ambientParticleDirection,
    float minSwirlRadius, float maxSwirlRadius, float minSwirlSpeed, float maxSwirlSpeed,
    int affectedDepth)
    : RendererController<BlossomBlockController>(affectedDepth)
{
    public const int DefaultDepth = Depths.Solids;
    public const string DefaultSpritePath = "objects/aonHelper/blossomBlock/block";
    public const int DefaultSurfaceIndex = global::Celeste.SurfaceIndex.Grass;
    public static readonly Color DefaultParticleColor1 = Calc.HexToColor("ff94af"), DefaultParticleColor2 = Calc.HexToColor("e1417f");
    public const float DefaultAmbientParticleDirection = 120f * Calc.DegToRad;
    public const float DefaultMinSwirlRadius = 0f, DefaultMaxSwirlRadius = 2f;
    public const float DefaultMinSwirlSpeed = MathF.PI * 2f / 6f, DefaultMaxSwirlSpeed = MathF.PI * 2f / 3f;

    public readonly string SpritePath = string.IsNullOrEmpty(spritePath) ? DefaultSpritePath : spritePath;
    public readonly int SurfaceIndex = surfaceIndex;
    public readonly Color BreakParticleColor1 = particleColor1, BreakParticleColor2 = particleColor2;
    public readonly float AmbientParticleDirection = ambientParticleDirection;
    public readonly float MinSwirlRadius = minSwirlRadius, MaxSwirlRadius = maxSwirlRadius, MinSwirlSpeed = minSwirlSpeed, MaxSwirlSpeed = maxSwirlSpeed;

    public BlossomBlockController(EntityData data, Vector2 offset)
        : this(data.Attr("spritePath", DefaultSpritePath), data.Int("surfaceIndex", DefaultSurfaceIndex),
            data.HexColor("particleColor1", DefaultParticleColor1), data.HexColor("particleColor2", DefaultParticleColor2), data.Float("ambientParticleDirection", DefaultAmbientParticleDirection) * Calc.DegToRad,
            data.Float("minSwirlRadius", 0f), data.Float("maxSwirlRadius", 2f), data.Float("minSwirlSpeed", 60f) * Calc.DegToRad, data.Float("maxSwirlSpeed", 120f) * Calc.DegToRad,
            data.Int("affectedDepth", DefaultDepth))
    { }
}
