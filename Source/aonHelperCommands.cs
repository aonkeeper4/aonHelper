namespace Celeste.Mod.aonHelper;

public static class aonHelperCommands
{
    private const string CommandPrefix = "aonHelper";

    [Command($"{CommandPrefix}.edit_curve_wipe", help: "Opens the curve wipe editor for this map or the map with the given SID, if specified.")]
    public static void TestCurveWipe(string sid = null)
    {
        Engine.Commands.Log("Work in progress!");
        return;

        // todo
        Scene next = new CurveWipeEditor(CurveWipe.DefaultSettings);

        Engine.Commands.Open = false;
        if (Engine.Scene is Level or Overworld)
            new FadeWipe(Engine.Scene, false, () => Engine.Scene = next);
        else
            Engine.Scene = next;

        Audio.Stop(Audio.CurrentMusicEventInstance);
        Audio.Stop(Audio.CurrentAmbienceEventInstance);
    }
}
