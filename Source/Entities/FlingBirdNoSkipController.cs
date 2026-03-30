namespace Celeste.Mod.aonHelper.Entities;

[CustomEntity("aonHelper/FlingBirdNoSkipController")]
[Tracked]
public class FlingBirdNoSkipController(Vector2 position, string condition)
    : ConditionalController<FlingBirdNoSkipController>(position, condition)
{
    public FlingBirdNoSkipController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Attr("flag"))
    { }

    #region Hooks
    
    [OnLoad]
    internal static void Load()
    {
        IL.Celeste.FlingBird.Update += FlingBird_Update;
    }

    [OnUnload]
    internal static void Unload()
    {
        IL.Celeste.FlingBird.Update -= FlingBird_Update;
    }

    private static void FlingBird_Update(ILContext il)
    {
        ILCursor cursor = new(il);
        
        /*
         * IL_0080: ldarg.0
         * IL_0081: callvirt instance void Celeste.FlingBird::Skip()
         * IL_0086: ret
         */
        if (!cursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<FlingBird>("Skip"),
            instr => instr.MatchRet()))
            throw new HookHelper.HookException(il, "Unable to find call to `FlingBird.Skip`.");
        
        ILLabel ret = cursor.DefineLabel();
        
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(ShouldNotSkipNode);
        cursor.Emit(OpCodes.Brtrue, ret);
        
        cursor.GotoNext(MoveType.Before, instr => instr.MatchRet());
        cursor.MarkLabel(ret);

        return;

        static bool ShouldNotSkipNode(FlingBird bird)
            => ControllerActive(bird.SceneAs<Level>(), out _);
    }
    
    #endregion
}