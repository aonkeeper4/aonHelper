namespace Celeste.Mod.aonHelper.Entities.LockBlocks;

[GlobalHelper.GlobalEntity("aonHelper/GlassLockBlockController, MoreLockBlocks/GlassLockBlockController", "global")]
[Tracked]
public class GlassLockBlockController : RendererController<GlassLockBlockController>
{
    public static readonly Color[] DefaultStarColors = [Calc.HexToColor("7f9fba"), Calc.HexToColor("9bd1cd"), Calc.HexToColor("bacae3")];
    public readonly Color[] StarColors;
    public static readonly Color DefaultBgColor = Calc.HexToColor("0d2e89"), DefaultLineColor = Color.White, DefaultRayColor = Color.White;
    public readonly Color BgColor, LineColor, RayColor;

    public const bool DefaultWavy = true;
    public readonly bool Wavy;
    public const bool DefaultVanillaEdgeBehavior = true;
    public readonly bool VanillaEdgeBehavior;

    public GlassLockBlockController(
        Color bgColor, Color lineColor, Color rayColor, Color[] starColors,
        bool wavy, bool vanillaEdgeBehavior,
        int? affectedDepth)
        : base(affectedDepth)
    {
        BgColor = bgColor;
        LineColor = lineColor;
        RayColor = rayColor;
        StarColors = starColors;
        
        Wavy = wavy;
        VanillaEdgeBehavior = vanillaEdgeBehavior;
    }

    public GlassLockBlockController(EntityData data, Vector2 offset)
        : this(
            data.HexColor("bgColor", DefaultBgColor), data.HexColor("lineColor", DefaultLineColor), data.HexColor("rayColor", DefaultRayColor), data.HexColorArray("starColors", DefaultStarColors),
            data.Bool("wavy", DefaultWavy), data.Bool("vanillaEdgeBehavior", DefaultVanillaEdgeBehavior),
            data.Nullable<int>("affectedDepth"))
    { }
}
