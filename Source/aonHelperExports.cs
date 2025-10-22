using Celeste.Mod.aonHelper.Entities;
using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.aonHelper;

public static class aonHelperExports
{
    internal static void Initialize()
    {
        typeof(FgStylegroundBloomControllerCompat).ModInterop();
    }

    /// <summary>
    /// Provides ModInterop exports for interfacing correctly with the rendering changes imposed by Fg Styleground Bloom Controllers <b>with a set bloom tag</b>.
    /// </summary>
    [ModExportName("aonHelper.FgStylegroundBloomControllerCompat")]
    public static class FgStylegroundBloomControllerCompat
    {
        /// <summary>
        /// Adds a callback to be invoked before the `Foregound.Render` call in `Level.Render`.
        /// </summary>
        /// <param name="action">The callback to add.</param>
        public static void AddBeforeForegroundRenderAction(Action<Level, bool> action)
            => FgStylegroundBloomController.AddBeforeForegroundRenderAction(action);
        /// <summary>
        /// Removes a callback to be invoked before the `Foregound.Render` call in `Level.Render`.
        /// </summary>
        /// <param name="action">The callback to remove.</param>
        public static void RemoveBeforeForegroundRenderAction(Action<Level, bool> action)
            => FgStylegroundBloomController.RemoveBeforeForegroundRenderAction(action);
    
        /// <summary>
        /// Adds a callback to be invoked after the `Foregound.Render` call in `Level.Render`.
        /// </summary>
        /// <param name="action">The callback to add.</param>
        public static void AddAfterForegroundRenderAction(Action<Level, bool> action)
            => FgStylegroundBloomController.AddAfterForegroundRenderAction(action);
        /// <summary>
        /// Removes a callback to be invoked after the `Foregound.Render` call in `Level.Render`.
        /// </summary>
        /// <param name="action">The callback to remove.</param>
        public static void RemoveAfterForegroundRenderAction(Action<Level, bool> action)
            => FgStylegroundBloomController.RemoveAfterForegroundRenderAction(action);

        /// <summary>
        /// Retrieves the bloom tag of the current Fg Stryleground Bloom Controller.
        /// </summary>
        /// <param name="level">The current <see cref="Level"/> instance to use.</param>
        /// <returns>The current controller's bloom tag.</returns>
        public static string GetCurrentBloomTag(Level level)
            => FgStylegroundBloomController.GetCurrentBloomTag(level);
    }
}
