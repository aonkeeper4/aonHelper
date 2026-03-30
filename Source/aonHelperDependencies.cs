namespace Celeste.Mod.aonHelper;

public static class aonHelperDependencies
{
    // add mods to this class if we actually need to know they exist at load time, i.e. we need to do more than just modinterop
    
    #region DzhakeHelper
    
    private static readonly EverestModuleMetadata DzhakeHelper = new() { Name = "DzhakeHelper", Version = new Version(1, 4, 20) };
    internal static bool DzhakeHelperLoaded;
    
    #endregion
    
    #region ReverseHelper

    private static readonly EverestModuleMetadata ReverseHelper = new() { Name = "ReverseHelper", Version = new Version(1, 15, 20) };
    internal static bool ReverseHelperLoaded;
    
    #endregion
    
    private delegate bool DependencyLoadHandler(EverestModule module);
    private delegate void DependencyUnloadHandler();
    
    private class DependencyState(Action<bool> setLoaded, Func<bool> getLoaded, DependencyLoadHandler load, DependencyUnloadHandler unload)
    {
        public bool Loaded
        {
            get => getLoaded();
            set => setLoaded(value);
        }

        public readonly DependencyLoadHandler Load = load;
        public readonly DependencyUnloadHandler Unload = unload;
    }
    private static readonly Dictionary<EverestModuleMetadata, DependencyState> Dependencies = new()
    {
        { DzhakeHelper, new DependencyState(loaded => DzhakeHelperLoaded = loaded, () => DzhakeHelperLoaded, null, null) },
        { ReverseHelper, new DependencyState(loaded => ReverseHelperLoaded = loaded, () => ReverseHelperLoaded, null, null) }
    };
    
    // we need to make sure these are loaded *before* any hooks are loaded and *after* all hooks are unloaded, so we don't use the mod lifecycle attributes
    internal static void Load()
    {
        foreach ((EverestModuleMetadata metadata, DependencyState state) in Dependencies)
        {
            if (!state.Loaded && Everest.Loader.TryGetDependency(metadata, out EverestModule module))
                state.Loaded = state.Load?.Invoke(module) ?? true;
        }
    }
    
    internal static void Unload()
    {
        foreach (DependencyState state in Dependencies.Values.Where(state => state.Loaded))
        {
            state.Unload?.Invoke();
            state.Loaded = false;
        }
    }
}
