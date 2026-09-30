namespace Celeste.Mod.aonHelper;

[SettingName($"{ModOptionsPrefix}_title")]
public class aonHelperSettings : EverestModuleSettings
{
    private const string ModOptionsPrefix = "aonHelper_modOptions";

    #region Settings

    // lmao
    public float TaikoDrumEasterEggChance { get; set; } = 0f;

    #endregion

    #region Factory Methods

    public void CreateTaikoDrumEasterEggChanceEntry(TextMenu menu, bool inGame)
        => menu.Add(CreateScaleOption("taikoDrumEasterEggChance", "%",
            [0f, 0.01f, 0.05f, 0.1f, 0.5f, 1f, 2f, 3f, 4f, 5f, 10f, 20f, 30f, 40f, 50f, 60f, 70f, 80f, 90f, 100f],
            TaikoDrumEasterEggChance, value => TaikoDrumEasterEggChance = value / 100f));

    #endregion

    #region Utils

    private static TextMenu.Option<int> CreateScaleOption<T>(
        string label, string suffix,
        T[] scale, T value, Action<T> valueSetter,
        Func<T, string> formatter = null)
        where T : IComparable
    {
        List<T> choices = scale.ToList();
        ValueToIndex(value, choices);

        return new TextMenu.Slider(
                Dialog.Clean($"{ModOptionsPrefix}_{label}"),
                i =>
                {
                    T valueToFormat = choices[i];
                    if (formatter is not null)
                        return formatter(valueToFormat);

                    if (valueToFormat is float f)
                        return FormatFloat(f) + suffix;

                    return valueToFormat + suffix;
                },
                0, choices.Count - 1,
                ValueToIndex(value, choices))
            .Change(i => valueSetter(choices[i]));
    }

    private static int ValueToIndex<T>(T value, List<T> choices) where T : IComparable
    {
        if (choices.Contains(value))
            return choices.IndexOf(value);

        int position = 0;
        while (position < choices.Count && value.CompareTo(choices[position]) > 0)
            position++;

        if (position == choices.Count)
            choices.Add(value);
        else if (value.CompareTo(choices[position]) != 0)
            choices.Insert(position, value);

        return position;
    }

    private static string FormatFloat(float f)
        => f.ToString("N3").TrimEnd('0').TrimEnd('.');

    #endregion
}
