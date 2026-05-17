namespace Celeste.Mod.aonHelper.Entities.Controllers;

[GlobalHelper.GlobalEntity("aonHelper/FlingBirdNoSkipController", "global")]
[Tracked]
public class FlingBirdNoSkipController(string condition)
    : ConditionalController<FlingBirdNoSkipController>(condition)
{
    public FlingBirdNoSkipController(EntityData data, Vector2 offset)
        : this(data.Attr("flag"))
    { }

    #region Hooks
    
    [OnLoad]
    internal static void Load()
    {
        IL.Celeste.FlingBird.Update += IL_FlingBird_Update;
    }

    [OnUnload]
    internal static void Unload()
    {
        IL.Celeste.FlingBird.Update -= IL_FlingBird_Update;
    }

    private static void IL_FlingBird_Update(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_0080: ldarg.0
         * IL_0081: callvirt instance void Celeste.FlingBird::Skip()
         * IL_0086: ret
         */
        if (!cursor.TryGotoNextBestFit(MoveType.AfterLabel,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<FlingBird>("Skip"),
            instr => instr.MatchRet()))
            throw new HookHelper.HookException(il, "Unable to find call to `FlingBird.Skip`.");
        
        ILLabel ret = cursor.DefineLabel();
        
        cursor.EmitLdarg0();
        cursor.EmitDelegate(ShouldNotSkipNode);
        cursor.EmitBrtrue(ret);
        
        cursor.GotoNext(MoveType.Before, instr => instr.MatchRet());
        cursor.MarkLabel(ret);

        return;

        static bool ShouldNotSkipNode(FlingBird bird)
            => TryGetController(bird.SceneAs<Level>(), out _);
    }
    
    #endregion
}