namespace Celeste.Mod.aonHelper;

[SettingName($"{aonHelperModOptions.ModOptionsPrefix}_title")]
public class aonHelperSettings : EverestModuleSettings
{
    // lmao
    public float TaikoDrumEasterEggChance { get; set; } = 0f;

    #region Factory Methods

    public void CreateTaikoDrumEasterEggChanceEntry(TextMenu menu, bool inGame)
        => menu.Add(aonHelperModOptions.CreateScaleOption("taikoDrumEasterEggChance", "%",
            [0f, 0.01f, 0.05f, 0.1f, 0.5f, 1f, 2f, 3f, 4f, 5f, 10f, 20f, 30f, 40f, 50f, 60f, 70f, 80f, 90f, 100f],
            TaikoDrumEasterEggChance, value => TaikoDrumEasterEggChance = value / 100f));

    #endregion
}