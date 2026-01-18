using System;
using Celeste.Mod.aonHelper.Entities;
using Celeste.Mod.aonHelper.Entities.Legacy;
using Celeste.Mod.aonHelper.Helpers;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Celeste.Mod.aonHelper;

public class aonHelperModule : EverestModule
{
    public static aonHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(aonHelperModuleSettings);
    public static aonHelperModuleSettings Settings => (aonHelperModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(aonHelperModuleSession);
    public static aonHelperModuleSession Session => (aonHelperModuleSession)Instance._Session;

    public override Type SaveDataType => typeof(aonHelperModuleSaveData);
    public static aonHelperModuleSaveData SaveData => (aonHelperModuleSaveData)Instance._SaveData;
    
    private static readonly FieldInfo Everest__ContentLoaded = typeof(Everest).GetField("_ContentLoaded", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static Hook hook_Everest_Register;

    private static readonly EverestModuleMetadata DzhakeHelper = new() { Name = "DzhakeHelper", Version = new Version(1, 4, 9) };
    private static readonly EverestModuleMetadata ReverseHelper = new() { Name = "ReverseHelper", Version = new Version(1, 15, 0) };
    
    internal bool DzhakeHelperLoaded;
    internal bool ReverseHelperLoaded;
    
    private void LoadDzhakeHelper() => DzhakeHelperLoaded = true;
    private void UnloadDzhakeHelper() => DzhakeHelperLoaded = false;

    private void LoadReverseHelper() => ReverseHelperLoaded = true;
    private void UnloadReverseHelper() => ReverseHelperLoaded = false;
    
    private static void LoadOptionalDependencies()
    {
        if (!Instance.DzhakeHelperLoaded && Everest.Loader.DependencyLoaded(DzhakeHelper))
            Instance.LoadDzhakeHelper();

        if (!Instance.ReverseHelperLoaded && Everest.Loader.DependencyLoaded(ReverseHelper))
            Instance.LoadReverseHelper();
    }
    
    private static void UnloadOptionalDependencies()
    {
        if (Instance.DzhakeHelperLoaded)
            Instance.UnloadDzhakeHelper();
        
        if (Instance.ReverseHelperLoaded)
            Instance.UnloadReverseHelper();
    }

    public aonHelperModule()
    {
        Instance = this;
        
#if DEBUG
        Logger.SetLogLevel(nameof(aonHelper), LogLevel.Verbose);
#else
        Logger.SetLogLevel(nameof(aonHelper), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        aonHelperImports.Initialize();
        aonHelperExports.Initialize();
        
        ResizableHeart.Load();
        FeatherDashSwitch.Load();
        ReboundModifyController.Load();
        FeatherBounceScamController.Load();
        FlingBirdNoSkipController.Load();
        FgStylegroundBloomController.Load();
        ClampLightColorController.Load();
        DarkerMatter.Load();
        LightningCornerboostController.Load();
        UnforgivingSpikes.Load();
        SeamlessLightningController.Load();
        IntroFacingController.Load();
        QuantizeColorgradeController.Load();
        DreamDashThroughTransitionController.Load();
        GlassLockBlockController.Load();
        DreamLockBlock.DreamBlockDummy.Load();
        
        #region Legacy
        
        LegacyFeatherDashSwitch.Load();
        
        #endregion
        
        hook_Everest_Register = new Hook(typeof(Everest).GetMethod("Register")!, Everest_Register);
    }
    
    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);

        aonHelperGFX.LoadContent();
    }
    
    public override void Initialize()
    {
        LoadOptionalDependencies();
    }

    public override void Unload()
    {
        aonHelperGFX.UnloadContent();
        
        ResizableHeart.Unload();
        FeatherDashSwitch.Unload();
        ReboundModifyController.Unload();
        FeatherBounceScamController.Unload();
        FlingBirdNoSkipController.Unload();
        FgStylegroundBloomController.Unload();
        ClampLightColorController.Unload();
        DarkerMatter.Unload();
        LightningCornerboostController.Unload();
        UnforgivingSpikes.Unload();
        SeamlessLightningController.Unload();
        IntroFacingController.Unload();
        QuantizeColorgradeController.Unload();
        DreamDashThroughTransitionController.Unload();
        GlassLockBlockController.Unload();
        DreamLockBlock.DreamBlockDummy.Unload();
        
        #region Legacy
        
        LegacyFeatherDashSwitch.Unload();
        
        #endregion
        
        HookHelper.DisposeAndSetNull(ref hook_Everest_Register);
        
        UnloadOptionalDependencies();
    }
    
    #region Hooks
    
    private static void Everest_Register(Action<EverestModule> orig, EverestModule module)
    {
        orig(module);

        if ((bool) Everest__ContentLoaded.GetValue(null)!)
            LoadOptionalDependencies();
    }
    
    #endregion
}
