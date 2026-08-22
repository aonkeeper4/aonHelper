namespace Celeste.Mod.aonHelper.Triggers;

[CustomEntity("aonHelper/ParallaxFadeTrigger", "aonHelper/ParallaxColorFadeTrigger", "aonHelper/ParallaxAlphaFadeTrigger")]
public class ParallaxFadeTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    private readonly Color? colorFrom = data.NullableHexColor("colorFrom");
    private readonly Color? colorTo = data.NullableHexColor("colorTo");
    
    private readonly float? alphaFrom = data.Nullable<float>("alphaFrom");
    private readonly float? alphaTo = data.Nullable<float>("alphaTo");
	
    private readonly PositionModes positionMode = data.Enum("positionMode", PositionModes.LeftToRight);

    private readonly string tagToAffect = data.String("tagToAffect");
    private Parallax[] affectedParallaxes;

    private readonly ConditionHelper.Condition condition = data.Condition("flag");

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        Level level = SceneAs<Level>();
        affectedParallaxes = level.Foreground.Backdrops
            .Concat(level.Background.Backdrops)
            .Where(b => b is Parallax parallax && (tagToAffect is null || parallax.Tags.Contains(tagToAffect)))
            .Cast<Parallax>()
            .ToArray();
    }

    public override void OnStay(Player player)
    {
	    if (!condition.Check(SceneAs<Level>()))
		    return;
	    
	    if (colorFrom is { } cFrom && colorTo is { } cTo)
		    foreach (Parallax parallax in affectedParallaxes)
				parallax.Color = Color.Lerp(cFrom, cTo, GetPositionLerp(player, positionMode));
	    
	    if (alphaFrom is { } aFrom && alphaTo is { } aTo)
		    foreach (Parallax parallax in affectedParallaxes)
			    parallax.Alpha = Calc.LerpClamp(aFrom, aTo, GetPositionLerp(player, positionMode));
    }
}