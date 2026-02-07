using Celeste.Mod.aonHelper.Helpers;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.aonHelper.Triggers;

[CustomEntity("aonHelper/ParallaxFadeTrigger", "aonHelper/ParallaxColorFadeTrigger", "aonHelper/ParallaxAlphaFadeTrigger")]
public class ParallaxFadeTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    private readonly Color? colorFrom = data.NullableHexColor("colorFrom");
    private readonly Color? colorTo = data.NullableHexColor("colorTo");
    
    private readonly float? alphaFrom = data.Nullable<float>("alphaFrom");
    private readonly float? alphaTo = data.Nullable<float>("alphaTo");
	
    private readonly PositionModes positionMode = data.Enum("positionMode", PositionModes.LeftToRight);

    private readonly string tagToAffect = string.IsNullOrEmpty(data.Attr("tagToAffect")) ? null : data.Attr("tagToAffect");
    private List<Parallax> allParallaxes;

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        Level level = SceneAs<Level>();
        allParallaxes = level.Foreground.Backdrops.Concat(level.Background.Backdrops)
                                                  .Where(b => b is Parallax parallax && (tagToAffect is null || parallax.Tags.Contains(tagToAffect)))
                                                  .Cast<Parallax>()
                                                  .ToList();
    }

    public override void OnStay(Player player)
    {
	    Parallax[] affected = allParallaxes.Where(parallax => parallax is not null).ToArray();

	    if (colorFrom is { } cFrom && colorTo is { } cTo)
		    foreach (Parallax parallax in affected)
				parallax.Color = Color.Lerp(cFrom, cTo, GetPositionLerp(player, positionMode));
	    
	    if (alphaFrom is { } aFrom && alphaTo is { } aTo)
		    foreach (Parallax parallax in affected)
			    parallax.Alpha = Calc.ClampedMap(GetPositionLerp(player, positionMode), 0f, 1f, aFrom, aTo);
    }
}