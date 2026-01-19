using Monocle;

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
    public static void ResizeIfNeeded(this VirtualRenderTarget target, int widthToCheck, int heightToCheck)
    {
        if (target == null || (target.Width == widthToCheck && target.Height == heightToCheck))
            return;

        target.Width = widthToCheck;
        target.Height = heightToCheck;
        target.Reload();
    }

    /// <summary>
    /// Creates a new Render Target if null, or resizes it to match the Gameplay size.
    /// </summary>
    public static void CreateOrResizeGameplayTarget(ref VirtualRenderTarget target, string name)
    {
        if (target == null)
            target = VirtualContent.CreateRenderTarget(name, GameplayWidth, GameplayHeight);
        else
            target.ResizeIfNeeded(GameplayWidth, GameplayHeight);
    }

    /// <summary>
    /// Creates a new Render Target if null, or resizes it to match the Gameplay size with additional width and height.
    /// </summary>
    public static void CreateOrResizeGameplayTarget(ref VirtualRenderTarget target, string name,
        int addWidth, int addHeight)
    {
        if (target == null)
            target = VirtualContent.CreateRenderTarget(name, GameplayWidth + addWidth, GameplayHeight + addHeight);
        else
            target.ResizeIfNeeded(GameplayWidth + addWidth, GameplayHeight + addHeight);
    }

    public static void DisposeAndSetNull(ref VirtualRenderTarget target)
    {
        target?.Dispose();
        target = null;
    }
}
