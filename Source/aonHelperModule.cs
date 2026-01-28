using System;
using Celeste.Mod.aonHelper.Entities;
using Celeste.Mod.aonHelper.Entities.Legacy;

namespace Celeste.Mod.aonHelper;

public class aonHelperModule : EverestModule
{
    public static aonHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(aonHelperModuleSettings);
    public static aonHelperModuleSettings Settings => (aonHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(aonHelperModuleSession);
    public static aonHelperModuleSession Session => (aonHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(aonHelperModuleSaveData);
    public static aonHelperModuleSaveData SaveData => (aonHelperModuleSaveData) Instance._SaveData;

    public aonHelperModule()
    {
        Instance = this;
       
        Logger.SetLogLevel(nameof(aonHelper), LogLevel.Debug);
    }

    public override void Load()
    {
        aonHelperDependencies.Load();
        
        aonHelperImports.Initialize();
        aonHelperExports.Initialize();
        
        #region Entities
        
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
        LightOcclusionFixController.Load();
        
        #region Legacy
        
        LegacyFeatherDashSwitch.Load();
        
        #endregion
        
        #endregion
    }
    
    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);

        aonHelperGFX.LoadContent();
    }

    public override void Unload()
    {
        aonHelperGFX.UnloadContent();
        
        #region Entities
        
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
        LightOcclusionFixController.Unload();
        
        #region Legacy
        
        LegacyFeatherDashSwitch.Unload();
        
        #endregion
        
        #endregion
        
        aonHelperDependencies.Unload();
    }
}
