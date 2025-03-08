using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.aonHelper.Triggers;

[CustomEntity("aonHelper/ParallaxAlphaFadeTrigger")]
public class ParallaxAlphaFadeTrigger : Trigger {
    private readonly float alphaFrom;
    private readonly float alphaTo;
    private readonly PositionModes positionMode;

    private readonly string tagToAffect;
    private List<Parallax> allParallaxes;

	public ParallaxAlphaFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
	{
        alphaFrom = data.Float("alphaFrom", 0f);
        alphaTo = data.Float("alphaTo", 1f);
        positionMode = data.Enum("positionMode", PositionModes.LeftToRight);
        tagToAffect = string.IsNullOrEmpty(data.Attr("tagToAffect")) ? null : data.Attr("tagToAffect");
	}

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        Level level = scene as Level;
        allParallaxes = level.Foreground.Backdrops.Concat(level.Background.Backdrops)
            .Where(b => b is Parallax parallax && (tagToAffect is null || parallax.Tags.Contains(tagToAffect))).Cast<Parallax>().ToList();
    }

    public override void OnStay(Player player)
	{
		float value = Calc.ClampedMap(GetPositionLerp(player, positionMode), 0f, 1f, alphaFrom, alphaTo);
		foreach (Parallax parallax in allParallaxes) {
            if (parallax is not null) parallax.Alpha = value;
        }
	}
}