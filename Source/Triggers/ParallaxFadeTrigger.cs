using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.aonHelper.Triggers;

[CustomEntity("aonHelper/ParallaxFadeTrigger", "aonHelper/ParallaxColorFadeTrigger", "aonHelper/ParallaxAlphaFadeTrigger")]
public class ParallaxFadeTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    private readonly Color colorFrom = data.HexColor("colorFrom", Color.Black);
    private readonly Color colorTo = data.HexColor("colorTo", Color.White);
    
    private readonly float alphaFrom = data.Float("alphaFrom", 0f);
    private readonly float alphaTo = data.Float("alphaTo", 1f);
	
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
	    Color color = Color.Lerp(colorFrom, colorTo, GetPositionLerp(player, positionMode));
	    float alpha = Calc.ClampedMap(GetPositionLerp(player, positionMode), 0f, 1f, alphaFrom, alphaTo);
	    
	    foreach (Parallax parallax in allParallaxes.Where(parallax => parallax is not null))
	    {
		    parallax.Color = color;
		    parallax.Alpha = alpha;
	    }
    }
}