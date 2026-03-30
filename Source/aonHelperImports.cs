namespace Celeste.Mod.aonHelper;

public static class aonHelperImports
{
    [OnInitialize]
    internal static void Initialize()
    {
        FrostHelper.Load();
        ReverseHelper.Load();
    }
}
