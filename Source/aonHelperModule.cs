namespace Celeste.Mod.aonHelper;

public class aonHelperModule : EverestModule
{
    public static aonHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(aonHelperSettings);
    public static aonHelperSettings Settings => (aonHelperSettings) Instance._Settings;

    public override Type SessionType => typeof(aonHelperSession);
    public static aonHelperSession Session => (aonHelperSession) Instance._Session;

    public override Type SaveDataType => typeof(aonHelperSaveData);
    public static aonHelperSaveData SaveData => (aonHelperSaveData) Instance._SaveData;

    public aonHelperModule()
    {
        Instance = this;
        
        // if i accidentally leave a debug log in a released build i'll explode into a million pieces and die
        Logger.SetLogLevel(nameof(aonHelper), LogLevel.Debug);
    }

    // we don't use any lifecycle attributes for the stuff in this namespace (and the lazy loading manager) so we can ensure everything happens in the right order
    // todo: add some sort of preload method w/ attribute so we can stop hardcoding dependencies/type processors?
    
    public override void Load()
    {
        aonHelperTypeProcessor.Load();
        aonHelperExports.Load();
        HookHelper.HookLazyLoadingManager.Load();
        
        LifecycleMethods.OnLoad();
    }
    
    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        
        aonHelperGFX.LoadContent();
    }

    public override void Initialize()
    {
        aonHelperDependencies.Initialize();
        aonHelperImports.Initialize();
        
        LifecycleMethods.OnInitialize();
    }

    public override void Unload()
    {
        LifecycleMethods.OnUnload();
        
        aonHelperGFX.UnloadContent();
        aonHelperDependencies.Uninitialize();
        HookHelper.HookLazyLoadingManager.Unload();
    }
}
