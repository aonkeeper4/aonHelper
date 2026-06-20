namespace Celeste.Mod.aonHelper;

public static class aonHelperGFX
{
    private const string LogID = $"{nameof(aonHelper)}/{nameof(aonHelperGFX)}";
    
    public static SpriteBank SpriteBank { get; private set; }
    
    #region Effects

    private static Effect quantizedColorgradeEffect;
    public static Effect FxQuantizedColorgrade => quantizedColorgradeEffect;
    
    #endregion
    
    #region Buffers

    public delegate void DisposeBuffersHandler(ref int buffersDisposed);
    public static event DisposeBuffersHandler OnDisposeBuffers;
    
    #endregion

    internal static void LoadContent()
    {
        SpriteBank = new SpriteBank(GFX.Game, "Graphics/aonHelper/Sprites.xml");

        #region Effects
        
        quantizedColorgradeEffect = EffectHelper.LoadEffect("quantized_colorgrade");
        
        #endregion
        
        #region Buffers

        OnDisposeBuffers = null;
        
        #endregion
    }

    internal static void UnloadContent()
    {
        #region Effects
        
        EffectHelper.DisposeAndSetNull(ref quantizedColorgradeEffect);
        
        #endregion
        
        #region Buffers

        int buffersDisposed = 0;
        OnDisposeBuffers?.Invoke(ref buffersDisposed);
        Logger.Info(LogID, $"Disposed all buffers ({buffersDisposed} buffers disposed).");

        #endregion
    }
}
