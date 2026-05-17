namespace Celeste.Mod.aonHelper.Entities.Controllers;

[GlobalHelper.GlobalEntity("aonHelper/ClampLightColorController", "global", true)]
[Tracked]
public class ClampLightColorController(Color clampColor, ClampLightColorController.ClampMethod clampMethod)
    : Controller<ClampLightColorController>
{
    private readonly Color clampColor = clampColor;

    public enum ClampMethod {
        Clamp,
        Tint
    }
    private readonly ClampMethod clampMethod = clampMethod;

    // this might be wrongg but it looks fine to me
    private static readonly BlendState ClampColorState = new()
    {
        ColorBlendFunction = BlendFunction.Min,
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One
    };
    private static readonly BlendState TintColorState = new()
    {
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One
    };

    public ClampLightColorController(EntityData data, Vector2 offset)
        : this(data.HexColor("color", Color.White), data.Enum("clampMethod", ClampMethod.Clamp))
    { }

    #region Hooks
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.LightingRenderer.BeforeRender += On_LightingRenderer_BeforeRender;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.LightingRenderer.BeforeRender -= On_LightingRenderer_BeforeRender;
    }

    private static void On_LightingRenderer_BeforeRender(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene)
    {
        orig(self, scene);

        if (!TryGetController(scene as Level, out ClampLightColorController controller))
            return;
        
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, controller.clampMethod switch {
            ClampMethod.Clamp => ClampColorState,
            ClampMethod.Tint => TintColorState,
            _ => throw new ArgumentOutOfRangeException()
        });
        Draw.Rect(new Rectangle(0, 0, GameplayBuffers.Light.Width, GameplayBuffers.Light.Height), controller.clampColor);
        Draw.SpriteBatch.End();
    }
    
    #endregion
}