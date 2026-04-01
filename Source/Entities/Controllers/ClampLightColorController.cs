namespace Celeste.Mod.aonHelper.Entities.Controllers;

[CustomEntity("aonHelper/ClampLightColorController")]
[Tracked]
public class ClampLightColorController(Vector2 position, Color clampColor, ClampLightColorController.ClampMethod clampMethod) : Controller(position)
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
        : this(data.Position + offset, data.HexColor("color", Color.White), data.Enum("clampMethod", ClampMethod.Clamp))
    { }

    #region Hooks
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.LightingRenderer.BeforeRender += LightingRenderer_BeforeRender;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.LightingRenderer.BeforeRender -= LightingRenderer_BeforeRender;
    }

    private static void LightingRenderer_BeforeRender(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene)
    {
        orig(self, scene);

        if (scene.Tracker.GetEntity<ClampLightColorController>() is not { } controller)
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