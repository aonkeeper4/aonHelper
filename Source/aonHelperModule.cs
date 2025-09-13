using System;
using Monocle;
using Celeste.Mod.aonHelper.Entities;

namespace Celeste.Mod.aonHelper
{
    public class aonHelperModule : EverestModule
    {
        public static aonHelperModule Instance { get; private set; }

        public override Type SettingsType => typeof(aonHelperModuleSettings);
        public static aonHelperModuleSettings Settings => (aonHelperModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(aonHelperModuleSession);
        public static aonHelperModuleSession Session => (aonHelperModuleSession)Instance._Session;

        public override Type SaveDataType => typeof(aonHelperModuleSaveData);
        public static aonHelperModuleSaveData SaveData => (aonHelperModuleSaveData)Instance._SaveData;

        public static SpriteBank SpriteBank { get; private set; }

        public aonHelperModule()
        {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(aonHelperModule), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(aonHelperModule), LogLevel.Info);
#endif
        }

        public override void Load()
        {
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
            
            aonHelperExports.Initialize();
        }

        public override void Unload()
        {
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
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);

            SpriteBank = new SpriteBank(GFX.Game, "Graphics/aonHelper/Sprites.xml");
        }
    }
}
