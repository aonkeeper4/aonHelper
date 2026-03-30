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

    public override void Load()
    {
        aonHelperDependencies.Load();
        HookHelper.HookLazyLoadingManager.Load();
        
        LifecycleMethods.OnLoad();
    }
    
    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);

        LifecycleMethods.OnLoadContent(firstLoad);
    }

    public override void Initialize()
    {
        LifecycleMethods.OnInitialize();
    }

    public override void Unload()
    {
        LifecycleMethods.OnUnload();
        
        aonHelperDependencies.Unload();
        HookHelper.HookLazyLoadingManager.Unload();
    }
}
