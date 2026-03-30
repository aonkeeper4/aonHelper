namespace Celeste.Mod.aonHelper;

public static class aonHelperDependencies
{
    // add mods to this class if we actually need to know they exist at load time, i.e. we need to do more than just modinterop

    private const string LogID = $"{nameof(aonHelper)}/{nameof(aonHelperDependencies)}";
    
    #region DzhakeHelper
    
    private static readonly EverestModuleMetadata DzhakeHelper = new() { Name = "DzhakeHelper", Version = new Version(1, 4, 20) };
    internal static bool DzhakeHelperLoaded;
    
    #endregion
    
    #region ReverseHelper

    private static readonly EverestModuleMetadata ReverseHelper = new() { Name = "ReverseHelper", Version = new Version(1, 15, 20) };
    internal static bool ReverseHelperLoaded;
    
    #endregion

    private delegate bool ShouldLoadDependencyHandler(EverestModule module);
    private delegate void LoadDependencyHandler();
    private delegate void UnloadDependencyHandler();
    
    private class DependencyState(
        Action<bool> setLoaded, Func<bool> getLoaded,
        ShouldLoadDependencyHandler shouldLoadDependency, LoadDependencyHandler loadDependency, UnloadDependencyHandler unloadDependency)
    {
        public bool Loaded
        {
            get => getLoaded();
            set => setLoaded(value);
        }

        public readonly ShouldLoadDependencyHandler ShouldLoadDependency = shouldLoadDependency;
        public readonly LoadDependencyHandler LoadDependency = loadDependency;
        public readonly UnloadDependencyHandler UnloadDependency = unloadDependency;
    }
    private static readonly Dictionary<EverestModuleMetadata, DependencyState> Dependencies = new()
    {
        { DzhakeHelper, new DependencyState(loaded => DzhakeHelperLoaded = loaded, () => DzhakeHelperLoaded, null, null, null) },
        { ReverseHelper, new DependencyState(loaded => ReverseHelperLoaded = loaded, () => ReverseHelperLoaded, null, null, null) }
    };
    
    private static void UpdateDependencies(bool load)
    {
        foreach ((EverestModuleMetadata metadata, DependencyState state) in Dependencies)
        {
            EverestModule module = null;
            bool shouldLoad = load
                && Everest.Loader.TryGetDependency(metadata, out module)
                && (state.ShouldLoadDependency?.Invoke(module!) ?? true);
            if (shouldLoad == state.Loaded)
                continue;

            if (shouldLoad)
            {
                state.LoadDependency?.Invoke();
                state.Loaded = true;
                Logger.Info(LogID, $"Registered support for {metadata.Name} (version {module!.Metadata.Version.ToString()}).");
            }
            else
            {
                state.UnloadDependency?.Invoke();
                state.Loaded = false;
                Logger.Info(LogID, $"Unregistered support for {metadata.Name}.");
            }
        }
    }
    
    // we need to make sure these are loaded *before* any hooks are loaded and *after* all hooks are unloaded, so we don't use the mod lifecycle attributes
    internal static void Load()
        => UpdateDependencies(true);
    
    internal static void Unload()
        => UpdateDependencies(false);
}
