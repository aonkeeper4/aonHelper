namespace Celeste.Mod.aonHelper.Helpers;

public static class RenderTargetHelper
{
    /// <summary>
    /// Dynamic width of the gameplay target. For Extended Camera Dynamics support.
    /// </summary>
    public static int GameplayWidth => GameplayBuffers.Gameplay?.Width ?? 320;

    /// <summary>
    /// Dynamic height of the gameplay target. For Extended Camera Dynamics support.
    /// </summary>
    public static int GameplayHeight => GameplayBuffers.Gameplay?.Height ?? 180;

    /// <summary>
    /// Checks a Render Target for any difference from the input width or height, then resizes it if needed.
    /// </summary>
    public static void ResizeIfNeeded(ref VirtualRenderTarget target, int widthToCheck, int heightToCheck)
    {
        if (target is null || target.IsDisposed || target.Width == widthToCheck && target.Height == heightToCheck)
            return;

        target.Width = widthToCheck;
        target.Height = heightToCheck;
        target.Reload();
    }

    /// <summary>
    /// Checks a Render Target for any difference from the input width or height, then resizes it if needed.
    /// </summary>
    public static void ResizeIfNeeded(ref RenderTarget2D target, int widthToCheck, int heightToCheck, bool withStencilBuffer)
    {
        if (target is null || target.IsDisposed || target.Width == widthToCheck && target.Height == heightToCheck)
            return;

        DepthFormat depthFormat = withStencilBuffer ? DepthFormat.Depth24Stencil8 : DepthFormat.None;

        target.Dispose();
        target = new RenderTarget2D(Engine.Graphics.GraphicsDevice, widthToCheck, heightToCheck, false, SurfaceFormat.Color, depthFormat);
    }

    /// <summary>
    /// Creates a new Render Target if null, or resizes it to match the Gameplay size with additional width and height.
    /// </summary>
    public static void CreateOrResizeGameplayTarget(ref VirtualRenderTarget target, string name, int addWidth = 0, int addHeight = 0)
    {
        if (target is null || target.IsDisposed)
        {
            DisposeAndSetNull(ref target);
            target = VirtualContent.CreateRenderTarget(name, GameplayWidth + addWidth, GameplayHeight + addHeight);
        }
        else
            ResizeIfNeeded(ref target, GameplayWidth + addWidth, GameplayHeight + addHeight);
    }

    /// <summary>
    /// Creates a new Render Target with a stencil buffer if null, or resizes it to match the Gameplay size with additional width and height.
    /// </summary>
    public static void CreateOrResizeGameplayTargetWithStencilBuffer(ref RenderTarget2D target, int addWidth = 0, int addHeight = 0)
    {
        if (target is null || target.IsDisposed)
        {
            DisposeAndSetNull(ref target);
            target = new RenderTarget2D(Engine.Graphics.GraphicsDevice, GameplayWidth + addWidth, GameplayHeight + addHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }
        else
            ResizeIfNeeded(ref target, GameplayWidth + addWidth, GameplayHeight + addHeight, true);
    }

    public static void DisposeAndSetNull(ref VirtualRenderTarget target)
    {
        target?.Dispose();
        target = null;
    }

    public static void DisposeAndSetNull(ref RenderTarget2D target)
    {
        target?.Dispose();
        target = null;
    }
}
