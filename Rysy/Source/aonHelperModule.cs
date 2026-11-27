using Rysy.Mods;

namespace Celeste.Mod.aonHelper.Rysy;

public class aonHelperModule : ModModule
{
    public static aonHelperModule Instance { get; private set; }

    public override void Load()
        => Instance = this;
}
