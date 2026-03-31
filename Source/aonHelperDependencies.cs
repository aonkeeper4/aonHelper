namespace Celeste.Mod.aonHelper;

public static class aonHelperDependencies
{
    // add mods to this class if we actually need to know they exist at runtime, i.e. we need to do more than just modinterop

    #region Handling
    
    private const string LogID = $"{nameof(aonHelper)}/{nameof(aonHelperDependencies)}";

    public enum DependencyState
    {
        Unknown,
        Loaded,
        Unloaded
    }

    private delegate bool ShouldLoadDependencyHandler(EverestModule module);
    private delegate void LoadDependencyHandler();
    private delegate void UnloadDependencyHandler();
    
    private class DependencyHandler(
        Action<DependencyState> setLoaded, Func<DependencyState> getLoaded,
        ShouldLoadDependencyHandler shouldLoadDependency, LoadDependencyHandler loadDependency, UnloadDependencyHandler unloadDependency)
    {
        public DependencyState Loaded
        {
            get => getLoaded();
            set => setLoaded(value);
        }

        public readonly ShouldLoadDependencyHandler ShouldLoadDependency = shouldLoadDependency;
        public readonly LoadDependencyHandler LoadDependency = loadDependency;
        public readonly UnloadDependencyHandler UnloadDependency = unloadDependency;
    }
    
    private static void UpdateDependencies(bool load)
    {
        foreach ((EverestModuleMetadata metadata, DependencyHandler handler) in Dependencies)
        {
            string moduleName = metadata.Name;
            
            EverestModule module = null;
            bool shouldLoad = load
                && Everest.Loader.TryGetDependency(metadata, out module)
                && (handler.ShouldLoadDependency?.Invoke(module!) ?? true);
            if (handler.Loaded switch 
                {
                    DependencyState.Loaded when shouldLoad => true,
                    DependencyState.Unloaded when !shouldLoad => true,
                    _ => false
                })
                continue;

            if (shouldLoad)
            {
                handler.LoadDependency?.Invoke();
                handler.Loaded = DependencyState.Loaded;
                Logger.Info(LogID, $"Registered support for {moduleName} (version {module!.Metadata.Version}).");
            }
            else
            {
                handler.UnloadDependency?.Invoke();
                handler.Loaded = DependencyState.Unloaded;
                Logger.Info(LogID, $"Unregistered support for {moduleName}.");
            }
        }
    }
    
    internal static void Initialize()
        => UpdateDependencies(true);
    
    internal static void Uninitialize()
        => UpdateDependencies(false);
    
    #endregion
    
    #region Dependencies
    
    private static readonly EverestModuleMetadata DzhakeHelper = new() { Name = "DzhakeHelper", Version = new Version(1, 4, 20) };
    internal static DependencyState DzhakeHelperLoaded { get; private set; } = DependencyState.Unknown;

    private static readonly EverestModuleMetadata ReverseHelper = new() { Name = "ReverseHelper", Version = new Version(1, 15, 20) };
    internal static DependencyState ReverseHelperLoaded { get; private set; } = DependencyState.Unknown;
    
    private static readonly Dictionary<EverestModuleMetadata, DependencyHandler> Dependencies = new()
    {
        { DzhakeHelper, new DependencyHandler(state => DzhakeHelperLoaded = state, () => DzhakeHelperLoaded, null, null, null) },
        { ReverseHelper, new DependencyHandler(state => ReverseHelperLoaded = state, () => ReverseHelperLoaded, null, null, null) }
    };
    
    #endregion
}
