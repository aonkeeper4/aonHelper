namespace Celeste.Mod.aonHelper;

public static class aonHelperImports
{
    // modinterop goes in `Imports` and gets loaded here
    // add mods to `aonHelperDependencies` if we need to do more than that, i.e. we actually need to know mods exist at load time
    
    internal static void Initialize()
    {
        FrostHelper.Load();
        ReverseHelper.Load();
    }
}
