namespace Celeste.Mod.aonHelper.Entities.Controllers;

[CustomEntity("aonHelper/FeatherBounceScamController")]
[Tracked]
public class FeatherBounceScamController(Vector2 position, float featherBounceScamThreshold, string condition)
    : ConditionalController<FeatherBounceScamController>(position, condition)
{
    private readonly float featherBounceScamThreshold = featherBounceScamThreshold;

    public FeatherBounceScamController(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Float("featherBounceScamThreshold"), data.Attr("flag"))
    { }

    #region Hooks
    
    [OnLoad]
    internal static void Load()
    {
        IL.Celeste.Player.OnCollideH += IL_Player_OnCollideHV;
        IL.Celeste.Player.OnCollideV += IL_Player_OnCollideHV;
    }

    [OnUnload]
    internal static void Unload()
    {
        IL.Celeste.Player.OnCollideH -= IL_Player_OnCollideHV;
        IL.Celeste.Player.OnCollideV -= IL_Player_OnCollideHV;
    }

    private static void IL_Player_OnCollideHV(ILContext il)
    {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Player>("starFlyTimer"),
            instr => instr.MatchLdcR4(Player.StarFlyEndNoBounceTime)))
            throw new HookHelper.HookException(il, "Unable to find check on `Player.starFlyTimer` to modify.");
        
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(FeatherBounceScamThreshold);

        return;
        
        static float FeatherBounceScamThreshold(float orig, Player player)
            => TryGetActiveController(player.SceneAs<Level>(), out FeatherBounceScamController controller)
                ? controller.featherBounceScamThreshold
                : orig;
    }
    
    #endregion
}