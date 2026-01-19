using System;
using System.Collections.Generic;

namespace Celeste.Mod.aonHelper;

public static class aonHelperDependencies
{
    // add mods to this class if we actually need to know they exist at runtime, i.e. we need to do more than just modinterop
    // modinterop goes in `aonHelperImports`
    
    #region DzhakeHelper
    
    private static readonly EverestModuleMetadata DzhakeHelper = new() { Name = "DzhakeHelper", Version = new Version(1, 4, 20) };
    internal static bool DzhakeHelperLoaded;
    
    // do special stuff here
    private static void LoadDzhakeHelper() => DzhakeHelperLoaded = true;
    private static void UnloadDzhakeHelper() => DzhakeHelperLoaded = false;
    
    #endregion
    
    #region ReverseHelper

    private static readonly EverestModuleMetadata ReverseHelper = new() { Name = "ReverseHelper", Version = new Version(1, 15, 20) };
    internal static bool ReverseHelperLoaded;

    // do special stuff here
    private static void LoadReverseHelper() => ReverseHelperLoaded = true;
    private static void UnloadReverseHelper() => ReverseHelperLoaded = false;
    
    #endregion

    private class DependencyHandler(Func<bool> isLoaded, Action load, Action unload)
    {
        public readonly Func<bool> IsLoaded = isLoaded;

        public readonly Action Load = load;
        public readonly Action Unload = unload;
    }
    private static readonly Dictionary<EverestModuleMetadata, DependencyHandler> DependencyHandlers = new()
    {
        { DzhakeHelper, new DependencyHandler(() => DzhakeHelperLoaded, LoadDzhakeHelper, UnloadDzhakeHelper) },
        { ReverseHelper, new DependencyHandler(() => ReverseHelperLoaded, LoadReverseHelper, UnloadReverseHelper) }
    };
    
    internal static void Load()
    {
        foreach ((EverestModuleMetadata metadata, DependencyHandler handler) in DependencyHandlers)
            if (!handler.IsLoaded() && Everest.Loader.DependencyLoaded(metadata))
                handler.Load();
    }
    
    internal static void Unload()
    {
        foreach (DependencyHandler handler in DependencyHandlers.Values)
            if (handler.IsLoaded())
                handler.Unload();
    }
}
