namespace Celeste.Mod.aonHelper;

public static class aonHelperModOptions
{
    public const string ModOptionsPrefix = "aonHelper_modOptions";

    public static TextMenu.Option<int> CreateScaleOption<T>(
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
}