using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Monocle;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities.DarkerMatter;

[CustomEntity("aonHelper/DarkerMatterController")]
[Tracked]
public class DarkerMatterController : Entity
{
    public readonly float SpeedThreshold;
    public readonly float SpeedLimit;

    public readonly float StopGraceTimer;

    public readonly Color[] DarkerMatterColors;
    public readonly Color[] DarkerMatterWarpColors;

    public DarkerMatterController(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("darkerMatterColors"), data.Attr("darkerMatterWarpColors"), data.Float("speedThreshold", 0f), data.Float("speedLimit", 200f), data.Float("stopGraceTimer", 0.05f))
    {
    }

    public DarkerMatterController(Vector2 pos, string mainColors, string warpColors, float speedThreshold, float speedLimit, float stopGraceTimer) : base(pos)
    {
        SpeedThreshold = speedThreshold;
        SpeedLimit = speedLimit < 0 ? float.MaxValue : speedLimit;
        StopGraceTimer = stopGraceTimer;
        DarkerMatterColors = mainColors.Split(",").Select(Calc.HexToColor).ToArray();
        DarkerMatterWarpColors = warpColors.Split(",").Select(Calc.HexToColor).ToArray();
    }

    public static DarkerMatterController Default() => new(Vector2.Zero, "5e0824,47134c", "6a391c,775121", 0f, 200f, 0.05f);
}