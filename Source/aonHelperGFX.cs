namespace Celeste.Mod.aonHelper;

public static class aonHelperGFX
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(aonHelperGFX)}";
    
    public static SpriteBank SpriteBank { get; private set; }
    
    #region Effects

    private static Effect quantizedColorgradeEffect;
    public static Effect FxQuantizedColorgrade => quantizedColorgradeEffect;

    private static Effect swirlDisplacementEffect;
    public static Effect FxSwirlDisplacement => swirlDisplacementEffect;
    
    #endregion
    
    #region Buffers

    public delegate void DisposeBuffersHandler();
    public static event DisposeBuffersHandler OnDisposeBuffers;
    
    #endregion

    internal static void LoadContent()
    {
        SpriteBank = new SpriteBank(GFX.Game, "Graphics/aonHelper/Sprites.xml");
        
        quantizedColorgradeEffect = EffectHelper.LoadEffect("quantized_colorgrade");
        swirlDisplacementEffect = EffectHelper.LoadEffect("swirl_displacement");

        OnDisposeBuffers = null;
    }

    internal static void UnloadContent()
    {
        EffectHelper.DisposeAndSetNull(ref quantizedColorgradeEffect);
        EffectHelper.DisposeAndSetNull(ref swirlDisplacementEffect);

        OnDisposeBuffers?.Invoke();
    }
}
