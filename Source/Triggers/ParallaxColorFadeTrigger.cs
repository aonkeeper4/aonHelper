using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.aonHelper.Triggers;

[CustomEntity("aonHelper/ParallaxColorFadeTrigger")]
public class ParallaxColorFadeTrigger : Trigger {
    private readonly Color colorFrom;
    private readonly Color colorTo;
    private readonly PositionModes positionMode;

    private readonly string tagToAffect;
    private List<Parallax> allParallaxes;

	public ParallaxColorFadeTrigger(EntityData data, Vector2 offset) : base(data, offset)
	{
        colorFrom = data.HexColor("colorFrom", Color.Black);
        colorTo = data.HexColor("colorTo", Color.White);
        positionMode = data.Enum("positionMode", PositionModes.LeftToRight);
        tagToAffect = string.IsNullOrWhiteSpace(data.Attr("tagToAffect")) ? null : data.Attr("tagToAffect");
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
		Color value = Color.Lerp(colorFrom, colorTo, GetPositionLerp(player, positionMode));
		foreach (Parallax parallax in allParallaxes) {
            if (parallax is not null) parallax.Color = value;
        }
	}
}