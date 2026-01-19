using Celeste.Mod.aonHelper.Entities;
using Celeste.Mod.aonHelper.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

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
    
    private static VirtualRenderTarget glassLockBlockBeamsBuffer, glassLockBlockStarsBuffer, glassLockBlockStencilBuffer;
    
    #endregion

    internal static void LoadContent()
    {
        SpriteBank = new SpriteBank(GFX.Game, "Graphics/aonHelper/Sprites.xml");

        #region Effects
        
        quantizedColorgradeEffect = EffectHelper.LoadEffect("quantized_colorgrade");
        
        #endregion
    }

    internal static void UnloadContent()
    {
        #region Effects
        
        EffectHelper.DisposeAndSetNull(ref quantizedColorgradeEffect);
        
        #endregion
        
        #region Buffers
        
        RenderTargetHelper.DisposeAndSetNull(ref glassLockBlockBeamsBuffer);
        RenderTargetHelper.DisposeAndSetNull(ref glassLockBlockStarsBuffer);
        RenderTargetHelper.DisposeAndSetNull(ref glassLockBlockStencilBuffer);
        
        #endregion
    }
    
    public static void QueryGlassLockBlockBuffers(out VirtualRenderTarget beamsBuffer, out VirtualRenderTarget starsBuffer, out VirtualRenderTarget stencilBuffer)
    {
        if (glassLockBlockBeamsBuffer is not { IsDisposed: false }
            || glassLockBlockStarsBuffer is not { IsDisposed: false }
            || glassLockBlockStencilBuffer is not { IsDisposed: false })
        {
            RenderTargetHelper.DisposeAndSetNull(ref glassLockBlockBeamsBuffer);
            RenderTargetHelper.DisposeAndSetNull(ref glassLockBlockStarsBuffer);
            RenderTargetHelper.DisposeAndSetNull(ref glassLockBlockStencilBuffer);

            glassLockBlockBeamsBuffer = VirtualContent.CreateRenderTarget($"{nameof(aonHelper)}/{nameof(GlassLockBlock)}_beams", RenderTargetHelper.GameplayWidth, RenderTargetHelper.GameplayHeight);
            glassLockBlockStarsBuffer = VirtualContent.CreateRenderTarget($"{nameof(aonHelper)}/{nameof(GlassLockBlock)}_stars", RenderTargetHelper.GameplayWidth, RenderTargetHelper.GameplayHeight);
            glassLockBlockStencilBuffer = VirtualContent.CreateRenderTarget($"{nameof(aonHelper)}/{nameof(GlassLockBlock)}_stencil", RenderTargetHelper.GameplayWidth, RenderTargetHelper.GameplayHeight);

            Logger.Info(LogID, "Created new Glass Lock Block buffer triplet.");
        }

        beamsBuffer = glassLockBlockBeamsBuffer;
        starsBuffer = glassLockBlockStarsBuffer;
        stencilBuffer = glassLockBlockStencilBuffer;
    }
}
