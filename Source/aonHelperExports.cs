using Celeste.Mod.aonHelper.Entities;
using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.aonHelper;

public static class aonHelperExports
{
    internal static void Initialize()
    {
        typeof(FgStylegroundRenderCompat).ModInterop();
    }

    [ModExportName("aonHelper.FgStylegroundRenderCompat")]
    public static class FgStylegroundRenderCompat
    {
        public static void AddBeforeForegroundRenderAction(Action<bool> action)
            => FgStylegroundBloomController.AddBeforeForegroundRenderAction(action);
        public static void RemoveBeforeForegroundRenderAction(Action<bool> action)
            => FgStylegroundBloomController.RemoveBeforeForegroundRenderAction(action);
    
        public static void AddAfterForegroundRenderAction(Action<bool> action)
            => FgStylegroundBloomController.AddAfterForegroundRenderAction(action);
        public static void RemoveAfterForegroundRenderAction(Action<bool> action)
            => FgStylegroundBloomController.RemoveAfterForegroundRenderAction(action);

        public static string GetCurrentBloomTag(Level level)
            => FgStylegroundBloomController.GetCurrentBloomTag(level);
    }
}
